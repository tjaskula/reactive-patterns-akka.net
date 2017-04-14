module Actors
    
    open Akka.Actor
    open Akka.FSharp
    open Akka.Configuration

    open System
    open System.Collections.Generic

    open ConsoleHelpers
    open Messages

    type UserCoordinatorActor() as this =
        inherit ReceiveActor()

        let users = new Dictionary<int, IActorRef>()

        do
            this.Receive<PlayMovieMessage>(fun (message : PlayMovieMessage) -> 
                                                //createChildUserIfNotExists(message.UserId)
                                                let childActorRef = users.[message.UserId]
                                                childActorRef <! message)


            this.Receive<StopMovieMessage>(fun (message : StopMovieMessage) ->
                                                //CreateChildUserIfNotExists(message.UserId)
                                                let childActorRef = users.[message.UserId]
                                                childActorRef <! message)
               
        override __.PreStart() =
            cprintfn ConsoleColor.Cyan "UserCoordinatorActor PreStart"

        override __.PostStop() =
            cprintfn ConsoleColor.Cyan "UserCoordinatorActor PostStop"

        override __.PreRestart (e, message) =
            cprintfn ConsoleColor.Cyan "UserCoordinatorActor PreRestart because: %A" e
            base.PreRestart(e, message)

        override __.PostRestart e =
            cprintfn ConsoleColor.Cyan "UserCoordinatorActor PostRestart because: %A" e
            base.PostRestart(e)

    type PlaybackStatisticsActor() =
        inherit ReceiveActor()

        override __.PreStart() =
            cprintfn ConsoleColor.White "PlaybackStatisticsActor PreStart"

        override __.PostStop() =
            cprintfn ConsoleColor.White "PlaybackStatisticsActor PostStop"

        override __.PreRestart (e, message) =
            cprintfn ConsoleColor.White "PlaybackStatisticsActor PreRestart because: %A" e
            base.PreRestart(e, message)

        override __.PostRestart e =
            cprintfn ConsoleColor.White "PlaybackStatisticsActor PostRestart because: %A" e
            base.PostRestart(e)
    
    type PlaybackActor()  =
        inherit ReceiveActor()

        do
            PlaybackActor.Context.ActorOf(Props.Create<UserCoordinatorActor>(), "UserCoordinator") |> ignore
            PlaybackActor.Context.ActorOf(Props.Create<PlaybackStatisticsActor>(), "PlaybackStatistics") |> ignore

        override __.PreStart() =
            cprintfn ConsoleColor.Green "PlaybackActor PreStart"

        override __.PostStop() =
            cprintfn ConsoleColor.Green "PlaybackActor PostStop"

        override __.PreRestart (e, message) =
            cprintfn ConsoleColor.Green "PlaybackActor PreRestart because: %A" e
            base.PreRestart(e, message)

        override __.PostRestart e =
            cprintfn ConsoleColor.Green "PlaybackActor PostRestart because: %A" e
            base.PostRestart(e)