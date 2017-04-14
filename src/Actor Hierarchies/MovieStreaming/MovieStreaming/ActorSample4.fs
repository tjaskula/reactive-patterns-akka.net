module Actor4

open Akka.Actor
open Akka.FSharp
open System

open Actors
open ComposeIt.Akka.FSharp.Extensions.Actor
open ConsoleHelpers
open Messages

let start6 system =
    // 4# user actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let preStart = Some(fun (baseFn : unit -> unit) -> cprintfn ConsoleColor.Green "UserActor PreStart")
    let postStop = Some(fun (baseFn : unit -> unit) -> cprintfn ConsoleColor.Green "UserActor PostStop")
    let preRestart = Some(fun (baseFn : exn * obj -> unit) -> cprintfn ConsoleColor.Green "UserActor PreRestart because: %A" exn)
    let postRestart = Some(fun (baseFn : exn -> unit) -> cprintfn ConsoleColor.Green "UserActor PostRestart because: %A" exn)

    let actor4 = 
        spawnOvrd system "UserActor"
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor 4..."
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
    actor4 <! {MovieTitle = "Codenan the Destroyer"; UserId = 42}
    
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Boolean Lies)" 
    actor4 <! {MovieTitle = "Boolean Lies"; UserId = 42}

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a StopMovieMessage" 
    actor4 <! StopMovieMessage()

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a another StopMovieMessage" 
    actor4 <! StopMovieMessage()

    Console.ReadKey() |> ignore

let start7 (system : ActorSystem) =
    // 4'# user actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let props = Props.Create<UserActor>()
    let actor4' = system.ActorOf(props, "UserActorBis")
  
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Codenan the Destroyer)" 
    actor4' <! {MovieTitle = "Codenan the Destroyer"; UserId = 42}
    
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Boolean Lies)" 
    actor4' <! {MovieTitle = "Boolean Lies"; UserId = 42}

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a StopMovieMessage" 
    actor4' <! StopMovieMessage()

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a another StopMovieMessage" 
    actor4' <! StopMovieMessage()

    Console.ReadKey() |> ignore    