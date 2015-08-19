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
            Console.WriteLine("Hello rien")