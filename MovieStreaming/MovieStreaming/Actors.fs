module Actors
    
    open Akka.Actor
    open Akka.FSharp
    open Akka.Configuration

    open System

    open Messages

    type PlaybackActor() =    
        inherit UntypedActor()

        do
            printfn "Creating Playback Actor..."

        override __.OnReceive message =
            match message with
            | :? string as e -> printfn "Received movie title %s" e
            | :? int as i -> printfn "Received user ID %i" i
            | _ -> __.Unhandled(message)

    type PlaybackActorTyped() as this =
        inherit ReceiveActor()

        do
            printfn "Creating Playback Actor typed..."
            this.Receive<PlayMovieMessage>(fun message -> this.HandlePlayMovieMessage message)
        
        member this.HandlePlayMovieMessage (message : PlayMovieMessage) : unit =
            printfn "Received typed message - movie title %s and User ID %i" message.MovieTitle message.UserId