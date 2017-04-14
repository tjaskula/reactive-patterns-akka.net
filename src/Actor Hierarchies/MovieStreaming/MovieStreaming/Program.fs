open Akka.Actor
open Akka.FSharp
open ComposeIt.Akka.FSharp.Extensions.Actor
open System
open System.Threading

open Actors
open ConsoleHelpers
open Messages

[<EntryPoint>]
let main argv = 

    cprintfn ConsoleColor.Gray "Creating MovieStreamingActorSystel"
    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()

    cprintfn ConsoleColor.Gray "Creating actor supervisory hierarchy"
    let props = Props.Create<PlaybackActor>()
    let actor = system.ActorOf(props, "Playback")
    
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