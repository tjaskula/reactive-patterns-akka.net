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

        override __.PreRestart (e, message) =
            cprintfn ConsoleColor.Green "Playback Actor PreRestart because: %A" e
            base.PreRestart(e, message)

        override __.PostRestart e =
            cprintfn ConsoleColor.Green "Playback Actor PostRestart because: %A" e
            base.PostRestart(e)

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

        override __.PreRestart (e, message) =
            cprintfn ConsoleColor.Green "Playback typed Actor PreRestart because: %A" e
            base.PreRestart(e, message)

        override __.PostRestart e =
            cprintfn ConsoleColor.Green "Playback typed Actor PostRestart because: %A" e
            base.PostRestart(e)

    type UserActor() as this =
        inherit ReceiveActor()

        do
            cprintfn ConsoleColor.Gray "Creating a UserActor"
            this.Receive<PlayMovieMessage>((fun message -> this.HandlePlayMovieMessage message))
            this.Receive<StopMovieMessage>((fun message -> this.HandleStopMovieMessage message))
          
        let mutable currentlyWatching = String.Empty

        let startPlayingMovie title =
            currentlyWatching <- title
            cprintfn ConsoleColor.Yellow "User is currently watching %s" currentlyWatching

        let stopPlayingCurrentMovie() =
            cprintfn ConsoleColor.Yellow "User has stopped watching %s" currentlyWatching
            currentlyWatching <- null
            ()
        
        member private __.HandlePlayMovieMessage (message : PlayMovieMessage) : unit =
            match currentlyWatching with
            | null | "" -> startPlayingMovie message.MovieTitle
            | t -> cprintfn ConsoleColor.Red "Error: cannot start playing another movie before stopping existing one"
            

        member private __.HandleStopMovieMessage (message : StopMovieMessage) : unit =
            match currentlyWatching with
            | null | "" -> cprintfn ConsoleColor.Red "Error: cannot stop if nothing is playing"
            | _ -> stopPlayingCurrentMovie()

        override __.PreStart() =
            cprintfn ConsoleColor.Green "UserActor PreStart"

        override __.PostStop() =
            cprintfn ConsoleColor.Green "UserActor PostStop"

        override __.PreRestart (e, message) =
            cprintfn ConsoleColor.Green "UserActor PreRestart because: %A" e
            base.PreRestart(e, message)

        override __.PostRestart e =
            cprintfn ConsoleColor.Green "UserActor PostRestart because: %A" e
            base.PostRestart(e)

    type UserActorBecome() as this =
        inherit ReceiveActor()

        do
            cprintfn ConsoleColor.Gray "Creating a UserActor"
            cprintfn ConsoleColor.Cyan "Setting initial behavior to stopped"
            this.Stopped()
          
        let mutable currentlyWatching = String.Empty

        let startPlayingMovie title =
            currentlyWatching <- title
            cprintfn ConsoleColor.Yellow "User is currently watching %s" currentlyWatching

        let stopPlayingCurrentMovie() =
            cprintfn ConsoleColor.Yellow "User has stopped watching %s" currentlyWatching
            currentlyWatching <- null
            ()

        member private this.Playing() =
            ()

        member private this.Stopped() =
            ()
        
        member this.HandlePlayMovieMessage (message : PlayMovieMessage) : unit =
            match currentlyWatching with
            | null | "" -> startPlayingMovie message.MovieTitle
            | t -> cprintfn ConsoleColor.Red "Error: cannot start playing another movie before stopping existing one"
            

        member this.HandleStopMovieMessage (message : StopMovieMessage) : unit =
            match currentlyWatching with
            | null | "" -> cprintfn ConsoleColor.Red "Error: cannot stop if nothing is playing"
            | _ -> stopPlayingCurrentMovie()

        override __.PreStart() =
            cprintfn ConsoleColor.Green "UserActor PreStart"

        override __.PostStop() =
            cprintfn ConsoleColor.Green "UserActor PostStop"

        override __.PreRestart (e, message) =
            cprintfn ConsoleColor.Green "UserActor PreRestart because: %A" e
            base.PreRestart(e, message)

        override __.PostRestart e =
            cprintfn ConsoleColor.Green "UserActor PostRestart because: %A" e
            base.PostRestart(e)