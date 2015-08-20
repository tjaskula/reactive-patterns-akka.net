open Akka.Actor
open Akka.FSharp
open System

open Actors
open Messages

[<EntryPoint>]
let main argv = 
    
    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()
    Console.WriteLine("Actor system created.")

    // 1# actor
    let actor1 = 
        spawn system "PlaybackActor" 
        <| fun mailbox ->
            printfn "Creating the actor..."
            let rec loop() = actor {
                let! (msg : obj) = mailbox.Receive()
                match msg with
                    | :? string as e -> printfn "Received movie title %s" e
                    | :? int as i -> printfn "Received user ID %i" i
                    | _ -> mailbox.Unhandled msg
                return! loop()
            }
            loop()

    actor1 <! "Akka.NET : The Movie"
    actor1 <! 42

    // 2# actor
    let props = Props.Create<PlaybackActor>()
    let actor2 = system.ActorOf(props, "PlaybackActor2")

    actor2 <! "Akka.NET : The Movie"
    actor2 <! 42
    actor2 <! 'c'

    // 3# actor
    let actor3 = 
        spawn system "PlaybackActor3"
        <| fun mailbox ->
            printfn "Creating the actor 3..."
            let rec loop() = actor {
                let! (msg : obj) = mailbox.Receive()
                match msg with
                    | :? PlayMovieMessage as e -> printfn "Received movie title %s and User ID %i" e.MovieTitle e.UserId
                    | _ -> printfn "Unhadled message..."
                           mailbox.Unhandled msg
                return! loop()
            }
            loop()

    actor3 <! {MovieTitle = "Akka.NET : The Movie"; UserId = 42}
    actor3 <! 87

    // 4# actor
    let props = Props.Create<PlaybackActorTyped>()
    let actor4 = system.ActorOf(props, "PlaybackActor4")

    actor4 <! {MovieTitle = "Akka.NET : The Movie"; UserId = 42}
    actor4 <! {MovieTitle = "Akka.NET : The Movie"; UserId = 38}
    actor4 <! 48 // TODO : check how unhalded messages are processed in this configuration

    Console.ReadLine() |> ignore

    system.Shutdown()

    0 // return an integer exit code