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
            cprintfn ConsoleColor.Gray "Creating Playback Actor..."

        override __.OnReceive message =
            match message with
            | :? string as e -> cprintfn ConsoleColor.Yellow "Received movie title %s" e
            | :? int as i -> cprintfn ConsoleColor.Yellow "Received user ID %i" i
            | _ -> __.Unhandled(message)

        override __.PreStart() =
            cprintfn ConsoleColor.Green "Playback Actor PreStart"

        override __.PostStop() =
            cprintfn ConsoleColor.Green "Playback Actor PostStop"

    type PlaybackActorTyped() as this =
        inherit ReceiveActor()

        do
            cprintfn ConsoleColor.Gray "Creating Playback Actor typed..."
            this.Receive<PlayMovieMessage>((fun message -> this.HandlePlayMovieMessage message), (fun message -> message.UserId > 40))
        
        member this.HandlePlayMovieMessage (message : PlayMovieMessage) : unit =
            cprintfn ConsoleColor.Yellow "Received typed message - movie title %s and User ID %i" message.MovieTitle message.UserId

        override __.PreStart() =
            cprintfn ConsoleColor.Green "Playback typed Actor PreStart"

        override __.PostStop() =
            cprintfn ConsoleColor.Green "Playback typed Actor PostStop"