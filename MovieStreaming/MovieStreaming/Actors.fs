module Actors
    
    open Akka.Actor
    open Akka.FSharp
    open Akka.Configuration

    open System

    type PlaybackActor() =    
        inherit UntypedActor()

        do
            printfn "Creating Playback Actor"

        override __.OnReceive message =
            match message with
            | :? string as e -> printfn "Received movie title %s" e
            | :? int as i -> printfn "Received user ID %i" i
            | _ -> __.Unhandled(message)