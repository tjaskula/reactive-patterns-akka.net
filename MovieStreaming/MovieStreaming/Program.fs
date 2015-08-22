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

    // 1'# actor
    let props = Props.Create<PlaybackActor>()
    let actor1' = system.ActorOf(props, "PlaybackActor1'")

    actor1' <! "Akka.NET : The Movie"
    actor1' <! 42
    actor1' <! 'c'

    // 2# actor
    let actor2 = 
        spawn system "PlaybackActor2"
        <| fun mailbox ->
            printfn "Creating the actor 2..."
            let rec loop() = actor {                
                let! (msg : PlayMovieMessage) = mailbox.Receive()
                match msg with // This filter is similar to this.Receive<PlayMovieMessage>((fun message -> this.HandlePlayMovieMessage message), (fun message -> message.UserId > 40)) which will be invoked also when message should be handled.
                    | m when m.UserId > 40 -> printfn "Received movie title %s and User ID %i" m.MovieTitle m.UserId
                    | _ -> printfn "Unhadled message..."
                           mailbox.Unhandled msg
                return! loop()
            }
            loop()

    actor2 <! {MovieTitle = "Akka.NET : The Movie"; UserId = 42}
    actor2 <! 87

    // 2'# actor
    let props = Props.Create<PlaybackActorTyped>()
    let actor2' = system.ActorOf(props, "PlaybackActor2'")

    actor2' <! {MovieTitle = "Akka.NET : The Movie"; UserId = 42}
    actor2' <! {MovieTitle = "Akka.NET : The Movie"; UserId = 38}
    actor2' <! 48 // Unhandled method is called itself on typed actor

    Console.ReadLine() |> ignore

    // tells the actor system (and all child actors) to shutdown
    system.Shutdown()

    // wait for actor system to finish shutting down
    system.AwaitTermination()

    0 // return an integer exit code