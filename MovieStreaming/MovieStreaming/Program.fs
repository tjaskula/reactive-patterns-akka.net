open Akka.Actor
open Akka.FSharp
open ComposeIt.Akka.FSharp.Extensions.Lifecycle
open System

open Actors
open ActorConfiguration
open ConsoleHelpers
open Messages

[<EntryPoint>]
let main argv = 

    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()
    cprintfn ConsoleColor.Gray "Actor system created."

    let rec readConsole() =

        Console.WriteLine(Environment.NewLine)
        Console.WriteLine("Which actor do you want to run? Or type 'Q' to exit...")
        Console.WriteLine(Environment.NewLine)
        // actor 1
        Console.WriteLine("   1. Untyped basic actor F# Api")
        // actor 1bis
        Console.WriteLine("   2. Typed basic actor")
        // actor 2
        Console.WriteLine("   3. F# Api actor with strongly typed messages")
        // actor 2bis
        Console.WriteLine("   4. Actor with strongly typed messages")
        // actor 3
        Console.WriteLine("   5. F# Api Actor overriding lifecycles")
        // actor 4
        Console.WriteLine("   6. F# Api Stateful Actor with overriding lifecycles")

        let keyInfo = Console.ReadKey()
        Console.WriteLine(Environment.NewLine)

        if keyInfo.Key = ConsoleKey.Q then ()
        else
            match keyInfo.KeyChar with
            | '1' -> startActor system 1
            | '2' -> startActor system 2
            | '3' -> startActor system 3
            | '4' -> startActor system 4
            | '5' -> startActor system 5
            | '6' -> startActor system 6
            | _ -> Console.WriteLine "Choice not known"
            readConsole()

    readConsole()
    
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