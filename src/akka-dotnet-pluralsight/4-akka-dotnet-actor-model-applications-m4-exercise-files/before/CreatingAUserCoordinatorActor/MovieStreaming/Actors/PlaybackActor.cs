using System;
using Akka.Actor;
using MovieStreaming.Messages;

namespace MovieStreaming.Actors
{
    public class PlaybackActor : ReceiveActor
    {
        private readonly IActorRef _userCoordinator;
        private readonly IActorRef _statistics;

        public PlaybackActor()
        {           
            _userCoordinator = Context.ActorOf(Props.Create<UserCoordinatorActor>(), "UserCoordinator");
            _statistics = Context.ActorOf(Props.Create<PlaybackStatisticsActor>(), "PlaybackStatistics");


            Receive<PlayMovieMessage>(message =>
                                      {
                                          ColorConsole.WriteLineGreen(
                                              "PlaybackActor received PlayMovieMessage '{0}' for user {1}",
                                              message.MovieTitle, message.UserId);

                                          _userCoordinator.Tell(message);                                         
                                      });
        }




        #region Lifecycle hooks
        protected override void PreStart()
        {
            ColorConsole.WriteLineGreen("PlaybackActor PreStart");
        }

        protected override void PostStop()
        {
            ColorConsole.WriteLineGreen("PlaybackActor PostStop");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteLineGreen("PlaybackActor PreRestart because: " + reason);

            base.PreRestart(reason, message);
        }

        protected override void PostRestart(Exception reason)
        {
            ColorConsole.WriteLineGreen("PlaybackActor PostRestart because: " + reason);

            base.PostRestart(reason);
        } 
        #endregion
    }
}
