module Actors
    
    open Akka.Actor
    open Akka.FSharp
    open Akka.Configuration

    open System

    open ConsoleHelpers
    open Messages

    type PlaybackActor() =    
        inherit UntypedActor()

        do
            printfn "Creating Playback Actor..."

        override __.OnReceive message =
            match message with
            | :? string as e -> cprintfn ConsoleColor.Yellow "Received movie title %s" e
            | :? int as i -> cprintfn ConsoleColor.Yellow "Received user ID %i" i
            | _ -> __.Unhandled(message)

    type PlaybackActorTyped() as this =
        inherit ReceiveActor()

        do
            printfn "Creating Playback Actor typed..."
            this.Receive<PlayMovieMessage>((fun message -> this.HandlePlayMovieMessage message), (fun message -> message.UserId > 40))
        
        member this.HandlePlayMovieMessage (message : PlayMovieMessage) : unit =
            cprintfn ConsoleColor.Yellow "Received typed message - movie title %s and User ID %i" message.MovieTitle message.UserId