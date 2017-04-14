module Actors
    
    open Akka.Actor
    open Akka.FSharp
    open Akka.Configuration

    open System

    open ConsoleHelpers
    open Messages

    type UserCoordinatorActor() as this =
        inherit ReceiveActor()

    type PlaybackStatisticsActor() as this =
        inherit ReceiveActor()
    
    type PlaybackActor() as this  =
        inherit ReceiveActor()

        do
            PlaybackActor.Context.ActorOf(Props.Create<UserCoordinatorActor>(), "UserCoordinator") |> ignore
            PlaybackActor.Context.ActorOf(Props.Create<PlaybackStatisticsActor>(), "PlaybackStatistics") |> ignore
        
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