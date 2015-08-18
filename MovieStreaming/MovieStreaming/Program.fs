open Akka.Actor
open System

let system = ActorSystem.Create "MovieStreamingActorSystem"

[<EntryPoint>]
let main argv = 
    
    Console.ReadLine() |> ignore

    system.Shutdown()

    0 // return an integer exit code
