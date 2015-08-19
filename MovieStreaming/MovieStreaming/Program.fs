open Akka.Actor
open Akka.FSharp
open System

open Actors

[<EntryPoint>]
let main argv = 
    
    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()
    Console.WriteLine("Actor system created.")

    // 1# actor
    let actor = spawnOpt system "PlaybackActor" <| fun mailbox ->
        let rec loop() = actor {
            return! loop()
            }
        loop()

    // 2# actor
    let props = Props.Create<PlaybackActor>()
    let actor2 = system.ActorOf(props, "PlaybackActor2")

    Console.ReadLine() |> ignore

    system.Shutdown()

    0 // return an integer exit code