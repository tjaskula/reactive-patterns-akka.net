module Actor3

open Akka.Actor
open Akka.FSharp
open System

open Actors
open ComposeIt.Akka.FSharp.Extensions.Actor
open ConsoleHelpers
open Messages

let start5 system =
    // 3# actor, overriding lifecycle
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let actor3 = 
        spawn system "PlaybackActor3"
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor 3..."
            let rec loop() = actor {                
                let! msg = mailbox.Receive()
                match msg with
                    | Lifecycle e ->
                        match e with
                        | PreStart -> cprintfn ConsoleColor.Green "Playback Actor 3 PreStart"
                        | PostStop -> cprintfn ConsoleColor.Green "Playback Actor 3 PostStop"
                        | PreRestart (exn, _) -> cprintfn ConsoleColor.Green "Playback Actor PreRestart because: %A" exn
                        | PostRestart exn -> cprintfn ConsoleColor.Green "Playback Actor PostRestart because: %A" exn
                    | Message m when m.UserId > 40 -> cprintfn ConsoleColor.Yellow "Received movie title %s and User ID %i" m.MovieTitle m.UserId
                    | _ -> cprintfn ConsoleColor.Red "Unhandled message..."
                           mailbox.Unhandled msg
                return! loop()
            }
            loop()

    actor3 <! {MovieTitle = "Akka.NET : The Movie"; UserId = 42}
    actor3 <! {MovieTitle = "Partial Recall"; UserId = 99}
    actor3 <! {MovieTitle = "Boolean Lies"; UserId = 77}
    actor3 <! {MovieTitle = "Codenan the Destroyer"; UserId = 1}
    actor3 <! 87
    actor3 <! PoisonPill.Instance

    Console.ReadKey() |> ignore