open Akka.Actor
open Akka.FSharp
open System

open Actors

[<EntryPoint>]
let main argv = 
    
    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()
    Console.WriteLine("Actor system created.")

    // 1# actor
    let actor = 
        spawn system "PlaybackActor" 
        <| fun mailbox ->
            let rec loop() = actor {
                let! msg = mailbox.Receive()
                // TODO : Doesn't work
//                match msg with
//                    | :? string as e -> printfn "Received movie title %s" e
//                    | :? int as i -> ()
//                    | _ -> ()
                return! loop()
            }
            loop()

    actor <! "Akka.NET : The Movie"
    actor <! 42

    // 2# actor
    let props = Props.Create<PlaybackActor>()
    let actor2 = system.ActorOf(props, "PlaybackActor2")

    actor2 <! "Akka.NET : The Movie"
    actor2 <! 42
    actor2 <! 'c'

    Console.ReadLine() |> ignore

    system.Shutdown()

    0 // return an integer exit code