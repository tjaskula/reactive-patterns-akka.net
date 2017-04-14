open Akka.Actor
open Akka.FSharp
open ComposeIt.Akka.FSharp.Extensions.Actor
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
        // actor 4bis
        Console.WriteLine("   7. Stateful Actor with overriding lifecycles")
        // actor 5
        Console.WriteLine("   8. F# Api Stateful Actor with Become")
        // actor 5bis
        Console.WriteLine("   9. Stateful Actor with Become")
        // actor 5ter
        Console.WriteLine("   10. Stateful Actor with Become F# style")

        let line = Console.ReadLine()
        Console.WriteLine(Environment.NewLine)

        if line = "Q" then ()
        else
            match line with
            | "1" -> startActor system 1
            | "2" -> startActor system 2
            | "3" -> startActor system 3
            | "4" -> startActor system 4
            | "5" -> startActor system 5
            | "6" -> startActor system 6
            | "7" -> startActor system 7
            | "8" -> startActor system 8
            | "9" -> startActor system 9
            | "10" -> startActor system 10
            | _ -> Console.WriteLine "Choice not known"
            readConsole()

    readConsole()

    // tells the actor system (and all child actors) to shutdown
    system.Terminate() |> ignore

    // wait for actor system to finish shutting down
    system.WhenTerminated.Wait(TimeSpan.FromSeconds(2.)) |> ignore
    cprintfn ConsoleColor.Gray "Actor system shutdown."
    Console.ReadKey() |> ignore

    0 // return an integer exit code