module Actors 
    
    open Akka.Actor
    open Akka.FSharp

    open System
    open System.Collections.Generic

    open ComposeIt.Akka.FSharp.Extensions.Actor

    open ConsoleHelpers
    open Messages

    type UserCoordinatorActor() as this =
        inherit ReceiveActor()

        let users = new Dictionary<int, IActorRef>()

        do
            this.Receive<PlayMovieMessage>(fun (message : PlayMovieMessage) -> 
                                                this.CreateChildUserIfNotExists(message.UserId)
                                                let childActorRef = users.[message.UserId]
                                                childActorRef <! message)


            this.Receive<StopMovieMessage>(fun (message : StopMovieMessage) ->
                                                this.CreateChildUserIfNotExists(message.UserId)
                                                let childActorRef = users.[message.UserId]
                                                childActorRef <! message)
        
        member private x.CreateChildUserIfNotExists userId =
            if not (users.ContainsKey(userId)) then
                let newChildActorRef = UserCoordinatorActor.Context.ActorOf(Props.Create(typeof<UserActor>, userId), "User" + userId.ToString())
                users.Add(userId, newChildActorRef)
            cprintfn ConsoleColor.Cyan "UserCoordinatorActor created new child UserActor for %i (Total Users: %i)" userId users.Count

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

    and UserActor(userId : int) as this =
        inherit ReceiveActor()

        let mutable currentlyWatching = String.Empty
        let userId = userId

        do this.Stopped()
     
        member private this.StartPlayingMovie title =
            currentlyWatching <- title
            cprintfn ConsoleColor.Yellow "UserActor %i is currently watching '%s'" userId currentlyWatching
            this.Become(this.Playing)

        member private this.StopPlayingCurrentMovie() =
            cprintfn ConsoleColor.Yellow "UserActor %i has stopped watching '%s'" userId currentlyWatching
            currentlyWatching <- null
            this.Become(this.Stopped)

        member private this.Playing() =
            this.Receive<PlayMovieMessage>((fun _ -> cprintfn ConsoleColor.Red "UserActor %i Error: cannot start playing another movie before stopping existing one" userId))
            this.Receive<StopMovieMessage>((fun _ -> this.StopPlayingCurrentMovie()))
            cprintfn ConsoleColor.Yellow "UserActor %i has now become Playing" userId

        member private this.Stopped() =
            this.Receive<PlayMovieMessage>((fun message -> this.StartPlayingMovie message.MovieTitle))
            this.Receive<StopMovieMessage>((fun _ -> cprintfn ConsoleColor.Red "UserActor %i Error: cannot stop if nothing is playing" userId))
            cprintfn ConsoleColor.Yellow "UserActor %i has now become Stopped" userId

        override __.PreStart() =
            cprintfn ConsoleColor.Yellow "UserActor %i PreStart" userId

        override __.PostStop() =
            cprintfn ConsoleColor.Yellow "UserActor %i PostStop" userId

        override __.PreRestart (e, message) =
            cprintfn ConsoleColor.Yellow "UserActor %i PreRestart because: %A" userId e
            base.PreRestart(e, message)

        override __.PostRestart e =
            cprintfn ConsoleColor.Yellow "UserActor %i PostRestart because: %A" userId  e
            base.PostRestart(e)


    let rec moviePlayer lastState = function
        | PlayMovie m -> 
            match lastState with
            | Playing _ -> cprintfn ConsoleColor.Red "Error: cannot start playing another movie before stopping existing one"
            | Stopped t -> cprintfn ConsoleColor.Yellow "User is currently watching %s" t
                           cprintfn ConsoleColor.Cyan "User Actor has now become Playing"
            become (moviePlayer (Playing m.MovieTitle))        
        | StopMovie s -> 
            match lastState with
            | Playing t -> cprintfn ConsoleColor.Yellow "User has stopped watching %s" t
                           cprintfn ConsoleColor.Cyan "User Actor has now become Stopped"
            | Stopped _ -> cprintfn ConsoleColor.Red "Error: cannot stop if nothing is playing"
            become (moviePlayer (Stopped ""))                    
    
  


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