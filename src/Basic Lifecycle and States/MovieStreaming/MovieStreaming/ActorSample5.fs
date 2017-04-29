module Actor5

open Akka.Actor
open Akka.FSharp
open System

open Actors
open ComposeIt.Akka.FSharp.Extensions.Actor
open ConsoleHelpers
open Messages

let start8 system =
    // 5# user actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let actor5 = 
        spawn system "UserActorBecome"
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor 5..."
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
                                    | PlayMovie pm -> handlePlayMovieMessage pm
                                    | StopMovie -> handleStopMovieMessage ()
                               | _ -> cprintfn ConsoleColor.Red "Unhadled message..."
                                      mailbox.Unhandled msg
                                      ""
                return! loop newState
            }
            loop String.Empty

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Codenan the Destroyer)" 
    actor5 <! PlayMovie ({MovieTitle = "Codenan the Destroyer"; UserId = 42})
    
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Boolean Lies)" 
    actor5 <! PlayMovie ({MovieTitle = "Boolean Lies"; UserId = 42})

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a StopMovieMessage" 
    actor5 <! StopMovie

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a another StopMovieMessage" 
    actor5 <! StopMovie

    Console.ReadKey() |> ignore

let start9 (system : ActorSystem) =
    // 5'# user actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let props = Props.Create<UserActorBecome>()
    let actor5' = system.ActorOf(props, "UserActorBecomeBis")
  
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Codenan the Destroyer)" 
    actor5' <! {MovieTitle = "Codenan the Destroyer"; UserId = 42}
    
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Boolean Lies)" 
    actor5' <! {MovieTitle = "Boolean Lies"; UserId = 42}

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a StopMovieMessage" 
    actor5' <! StopMovieMessage()

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a another StopMovieMessage" 
    actor5' <! StopMovieMessage()

    Console.ReadKey() |> ignore

let start10 (system : ActorSystem) =
    // 5''# user actor become
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let rec moviePlayer lastState = function
        | PlayMovie m -> 
            match lastState with
            | Playing _ -> cprintfn ConsoleColor.Red "Error: cannot start playing another movie before stopping existing one"
            | Stopped t -> cprintfn ConsoleColor.Yellow "User is currently watching %s" t
                           cprintfn ConsoleColor.Cyan "User Actor has now become Playing"
            become (moviePlayer (Playing m.MovieTitle))        
        | StopMovie -> 
            match lastState with
            | Playing t -> cprintfn ConsoleColor.Yellow "User has stopped watching %s" t
                           cprintfn ConsoleColor.Cyan "User Actor has now become Stopped"
            | Stopped _ -> cprintfn ConsoleColor.Red "Error: cannot stop if nothing is playing"
            become (moviePlayer (Stopped ""))                    
    
    let actor5'' = 
        spawn system "UserActorBecome"
        <| actorOf( cprintfn ConsoleColor.Gray "Creating a UserActor"
                    cprintfn ConsoleColor.Cyan "Setting initial behavior to Stopped"
                    moviePlayer (Stopped ""))

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Codenan the Destroyer)" 
    actor5'' <! PlayMovie ({MovieTitle = "Codenan the Destroyer"; UserId = 42})
    
    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a PlayMovieMessage (Boolean Lies)" 
    actor5'' <! PlayMovie ({MovieTitle = "Boolean Lies"; UserId = 42})

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a StopMovieMessage" 
    actor5'' <! StopMovie

    Console.ReadKey() |> ignore
    cprintfn ConsoleColor.Blue "Sending a another StopMovieMessage" 
    actor5'' <! StopMovie

    Console.ReadKey() |> ignore