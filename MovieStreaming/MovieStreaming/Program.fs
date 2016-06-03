open Akka.Actor
open Akka.FSharp
open ComposeIt.Akka.FSharp.Extensions.Lifecycle
open System

open Actors
open ConsoleHelpers
open Messages

[<EntryPoint>]
let main argv = 
    
    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()
    cprintfn ConsoleColor.Gray "Actor system created."

    (* 
    =======
    Untyped
    =======
    *)

    // 1# actor
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let actor1 = 
        spawn system "PlaybackActor1" 
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor..."
            let rec loop() = actor {
                let! (msg : obj) = mailbox.Receive()
                match msg with
                    | :? string as e -> cprintfn ConsoleColor.Yellow "Received movie title %s" e
                    | :? int as i -> cprintfn ConsoleColor.Yellow "Received user ID %i" i
                    | _ -> mailbox.Unhandled msg
                return! loop()
            }
            loop()

    actor1 <! "Akka.NET : The Movie"
    actor1 <! 42

    Console.ReadKey() |> ignore

    (* 
    =======
    Typed
    =======
    *)

    // 1'# actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let props = Props.Create<PlaybackActor>()
    let actor1' = system.ActorOf(props, "PlaybackActor1bis")

    actor1' <! "Akka.NET : The Movie"
    actor1' <! 42
    actor1' <! 'c'

    // 2# actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let actor2 = 
        spawn system "PlaybackActor2"
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor 2..."
            let rec loop() = actor {                
                let! (msg : PlayMovieMessage) = mailbox.Receive()
                match msg with // This filter is similar to this.Receive<PlayMovieMessage>((fun message -> this.HandlePlayMovieMessage message), (fun message -> message.UserId > 40)) which will be invoked also when message should be handled.
                    | m when m.UserId > 40 -> cprintfn ConsoleColor.Yellow "Received movie title %s and User ID %i" m.MovieTitle m.UserId
                    | _ -> cprintfn ConsoleColor.Red "Unhadled message..."
                           mailbox.Unhandled msg
                return! loop()
            }
            loop()

    actor2 <! {MovieTitle = "Akka.NET : The Movie"; UserId = 42}
    actor2 <! {MovieTitle = "Partial Recall"; UserId = 99}
    actor2 <! {MovieTitle = "Boolean Lies"; UserId = 77}
    actor2 <! {MovieTitle = "Codenan the Destroyer"; UserId = 1}
    actor2 <! 87
    actor2 <! PoisonPill.Instance

    Console.ReadKey() |> ignore
    
    // 2'# actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let props = Props.Create<PlaybackActorTyped>()
    let actor2' = system.ActorOf(props, "PlaybackActor2bis")

    actor2' <! {MovieTitle = "Akka.NET : The Movie"; UserId = 42}
    actor2' <! {MovieTitle = "Partial Recall"; UserId = 99}
    actor2' <! {MovieTitle = "Boolean Lies"; UserId = 77}
    actor2' <! {MovieTitle = "Codenan the Destroyer"; UserId = 1}
    actor2' <! 48 // Unhandled method is called itself on typed actor
    actor2' <! PoisonPill.Instance

    Console.ReadKey() |> ignore

    // 3# actor, overriding lifecycle
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let preStart = Some(fun (baseFn : unit -> unit) -> cprintfn ConsoleColor.Green "Playback Actor 3 PreStart")
    let postStop = Some(fun (baseFn : unit -> unit) -> cprintfn ConsoleColor.Green "Playback Actor 3 PostStop")
    let preRestart = Some(fun (baseFn : exn * obj -> unit) -> cprintfn ConsoleColor.Green "Playback Actor PreRestart because: %A" exn)
    let postRestart = Some(fun (baseFn : exn -> unit) -> cprintfn ConsoleColor.Green "Playback Actor PostRestart because: %A" exn)

    let actor3 = 
        spawnOvrd system "PlaybackActor3"
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor 3..."
            let rec loop() = actor {                
                let! (msg : PlayMovieMessage) = mailbox.Receive()
                match msg with
                    | m when m.UserId > 40 -> cprintfn ConsoleColor.Yellow "Received movie title %s and User ID %i" m.MovieTitle m.UserId
                    | _ -> cprintfn ConsoleColor.Red "Unhadled message..."
                           mailbox.Unhandled msg
                return! loop()
            }
            loop()
        <| {defOvrd with PreStart = preStart; PostStop = postStop; PreRestart = preRestart; PostRestart = postRestart}

    actor3 <! {MovieTitle = "Akka.NET : The Movie"; UserId = 42}
    actor3 <! {MovieTitle = "Partial Recall"; UserId = 99}
    actor3 <! {MovieTitle = "Boolean Lies"; UserId = 77}
    actor3 <! {MovieTitle = "Codenan the Destroyer"; UserId = 1}
    actor3 <! 87
    actor3 <! PoisonPill.Instance

    Console.ReadKey() |> ignore

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

    // tells the actor system (and all child actors) to shutdown
    system.Terminate() |> ignore

    // wait for actor system to finish shutting down
    system.WhenTerminated.Wait(TimeSpan.FromSeconds(2.)) |> ignore
    cprintfn ConsoleColor.Gray "Actor system shutdown."
    Console.ReadKey() |> ignore

    0 // return an integer exit code