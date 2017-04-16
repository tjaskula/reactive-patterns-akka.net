open Akka.Actor
open Akka.FSharp 
open System
open System.Threading
open ComposeIt.Akka.FSharp.Extensions.Actor

open Actors
open ConsoleHelpers
open Messages

[<EntryPoint>]
let main argv = 

    cprintfn ConsoleColor.Gray "Creating MovieStreamingActorSystel"
    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()

    cprintfn ConsoleColor.Gray "Creating actor supervisory hierarchy"
    
    let preStart = Some(fun (baseFn : unit -> unit) -> cprintfn ConsoleColor.Green "PlaybackActor PreStart")
    let postStop = Some(fun (baseFn : unit -> unit) -> cprintfn ConsoleColor.Green "PlaybackActor PostStop")
    let preRestart = Some(fun (baseFn : exn * obj -> unit) -> cprintfn ConsoleColor.Green "PlaybackActor PreRestart because: %A" exn)
    let postRestart = Some(fun (baseFn : exn -> unit) -> cprintfn ConsoleColor.Green "PlaybackActor PostRestart because: %A" exn)             
    
    let playback = 
        spawnOvrd system "Playback"
        <| fun parentMailbox ->
            let child = 
                spawn parentMailbox "child" 
                    <| fun childMailbox ->
                        childMailbox.Defer (fun () -> printfn "Child stopping")
                        printfn "Child started"
                        let rec childLoop() = actor {
                            let! msg = childMailbox.Receive()
                            return! childLoop()
                        }
                        childLoop()
            cprintfn ConsoleColor.Gray "Creating parent actor..."
            // define parent behavior
            let rec loop() = actor {
                let! msg = parentMailbox.Receive()
                child.Forward(msg)  // forward all messages through
                return! loop ()
            }
            loop ()
        <| {defOvrd with PreStart = preStart; PostStop = postStop; PreRestart = preRestart; PostRestart = postRestart}

    let rec readConsole() =

        Thread.Sleep(450)

        Console.WriteLine(Environment.NewLine)
        Console.WriteLine("Enter a command and hit enter")
        let command = Console.ReadLine()
        
        if command.StartsWith("play") then
            let userId = command.Split(',').[1] |> int
            let movieTitle = command.Split(',').[2]

            let message = {MovieTitle = movieTitle; UserId = userId}
            let aref = select "/user/Playback/UserCoordinator" system
            aref <! message

            readConsole()

        elif command.StartsWith("stop") then
            let userId = command.Split(',').[1] |> int
            let message = {UserId = userId}
            let aref = select "/user/Playback/UserCoordinator" system
            aref <! message

            readConsole()

        elif command.StartsWith("exit") then
            ()

    readConsole()

    // tells the actor system (and all child actors) to shutdown
    system.Terminate() |> ignore

    // wait for actor system to finish shutting down
    system.WhenTerminated.Wait(TimeSpan.FromSeconds(2.)) |> ignore
    cprintfn ConsoleColor.Gray "Actor system shutdown."
    Console.ReadKey() |> ignore

    0 // return an integer exit code