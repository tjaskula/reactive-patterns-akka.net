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

    let actor4 = 
        spawn system "UserActor"
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor 4..."
            let rec loop lastState = actor {                
                let! msg = mailbox.Receive()

                let handlePlayMovieMessage (message : PlayMovieMessage) : string =
                    match lastState with
                    | null | "" -> cprintfn ConsoleColor.Yellow "User is currently watching %s" message.MovieTitle
                                   message.MovieTitle
                    | t -> cprintfn ConsoleColor.Red "Error: cannot start playing another movie before stopping existing one"
                           lastState

                let handleStopMovieMessage () : string =
                    match lastState with
                    | null | "" -> cprintfn ConsoleColor.Red "Error: cannot stop if nothing is playing"
                                   lastState
                    | _ -> cprintfn ConsoleColor.Yellow "User has stopped watching %s" lastState
                           String.Empty

                let newState = match msg with
                               | Lifecycle e ->
                                    match e with
                                    | PreStart -> cprintfn ConsoleColor.Green "UserActor PreStart"
                                    | PostStop -> cprintfn ConsoleColor.Green "UserActor PostStop"
                                    | PreRestart (exn, _) -> cprintfn ConsoleColor.Green "UserActor PreRestart because: %A" exn
                                    | PostRestart exn -> cprintfn ConsoleColor.Green "UserActor PostRestart because: %A" exn
                                    ""
                               | Message m ->
                                    match m with
                                    | PlayMovie pmm -> handlePlayMovieMessage pmm
                                    | StopMovie -> handleStopMovieMessage ()
                               | _ -> cprintfn ConsoleColor.Red "Unhadled message..."
                                      mailbox.Unhandled msg
                                      ""
                return! loop newState
            }
            loop String.Empty

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Codenan the Destroyer)" 
    actor4 <! PlayMovie({MovieTitle = "Codenan the Destroyer"; UserId = 42})
    
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Boolean Lies)" 
    actor4 <! PlayMovie({MovieTitle = "Boolean Lies"; UserId = 42})

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a StopMovieMessage" 
    actor4 <! StopMovie

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a another StopMovieMessage" 
    actor4 <! StopMovie

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