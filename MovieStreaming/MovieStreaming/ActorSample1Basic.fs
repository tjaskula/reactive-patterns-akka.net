module Actor1

open Akka.Actor
open Akka.FSharp
open ComposeIt.Akka.FSharp.Extensions.Lifecycle
open System

open Actors
open ConsoleHelpers
open Messages

(* 
=======
Untyped
=======
*)

let start1 system =

    // 1# actor
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let actor1 = 
        spawn system "PlaybackActor1" 
        <| fun mailbox ->
            cprintfn ConsoleColor.Gray "Creating the actor..."
            let rec loop() = actor {
                let! (msg : obj) = mailbox.Receive()
                match msg with
                    | :? string as e -> cprintfn ConsoleColor.Yellow "Received movie title %s" e
                    | :? int as i -> cprintfn ConsoleColor.Yellow "Received user ID %i" i
                    | _ -> mailbox.Unhandled msg
                return! loop()
            }
            loop()

    actor1 <! "Akka.NET : The Movie"
    actor1 <! 42

    Console.ReadKey() |> ignore

let start2 (system : ActorSystem) =
    (* 
    =======
    Typed
    =======
    *)

    // 1'# actor
    Console.WriteLine(Environment.NewLine)
    cprintfn ConsoleColor.Magenta "Starting new actor..."

    let props = Props.Create<PlaybackActor>()
    let actor1' = system.ActorOf(props, "PlaybackActor1bis")

    actor1' <! "Akka.NET : The Movie"
    actor1' <! 42
    actor1' <! 'c'