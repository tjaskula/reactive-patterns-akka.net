module Actor2

open Akka.Actor
open Akka.FSharp
open System

open Actors
open ConsoleHelpers
open Messages

let start3 system =
    // 2# actor F# API strongly typed message
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
    
let start4 (system : ActorSystem) =
    // 2'# actor strongly typed message
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