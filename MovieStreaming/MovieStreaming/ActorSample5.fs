module Actor5

open Akka.Actor
open Akka.FSharp
open System

open Actors
open ComposeIt.Akka.FSharp.Extensions.Lifecycle
open ConsoleHelpers
open Messages

let start8 system =
    // 5# user actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let preStart = Some(fun (baseFn : unit -> unit) -> cprintfn ConsoleColor.Green "UserActor PreStart")
    let postStop = Some(fun (baseFn : unit -> unit) -> cprintfn ConsoleColor.Green "UserActor PostStop")
    let preRestart = Some(fun (baseFn : exn * obj -> unit) -> cprintfn ConsoleColor.Green "UserActor PreRestart because: %A" exn)
    let postRestart = Some(fun (baseFn : exn -> unit) -> cprintfn ConsoleColor.Green "UserActor PostRestart because: %A" exn)

    let actor5 = 
        spawnOvrd system "UserActorBecome"
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor 5..."
            let rec loop lastState = actor {                
                let! (msg : obj) = mailbox.Receive()

                let handlePlayMovieMessage (message : PlayMovieMessage) : string =
                    match lastState with
                    | null | "" -> cprintfn ConsoleColor.Yellow "User is currently watching %s" message.MovieTitle
                                   message.MovieTitle
                    | t -> cprintfn ConsoleColor.Red "Error: cannot start playing another movie before stopping existing one"
                           lastState

                let handleStopMovieMessage (message : StopMovieMessage) : string =
                    match lastState with
                    | null | "" -> cprintfn ConsoleColor.Red "Error: cannot stop if nothing is playing"
                                   lastState
                    | _ -> cprintfn ConsoleColor.Yellow "User has stopped watching %s" lastState
                           String.Empty

                let newState = match msg with
                               | :? PlayMovieMessage as pmm -> handlePlayMovieMessage pmm
                               | :? StopMovieMessage as smm -> handleStopMovieMessage smm
                               | _ -> cprintfn ConsoleColor.Red "Unhadled message..."
                                      mailbox.Unhandled msg
                                      ""
                return! loop newState
            }
            loop String.Empty
        <| {defOvrd with PreStart = preStart; PostStop = postStop; PreRestart = preRestart; PostRestart = postRestart}

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Codenan the Destroyer)" 
    actor5 <! {MovieTitle = "Codenan the Destroyer"; UserId = 42}
    
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Boolean Lies)" 
    actor5 <! {MovieTitle = "Boolean Lies"; UserId = 42}

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a StopMovieMessage" 
    actor5 <! StopMovieMessage()

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a another StopMovieMessage" 
    actor5 <! StopMovieMessage()

    Console.ReadKey() |> ignore