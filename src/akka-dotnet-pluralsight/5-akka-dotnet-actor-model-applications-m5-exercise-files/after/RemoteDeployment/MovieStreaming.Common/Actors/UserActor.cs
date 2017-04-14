using System;
using Akka.Actor;
using MovieStreaming.Common.Messages;

namespace MovieStreaming.Common.Actors
{
    public class UserActor : ReceiveActor
    {
        private readonly int _userId;
        private string _currentlyWatching;

        public UserActor(int userId)
        {
            _userId = userId;

            Stopped();
        }

        private void Playing()
        {
            Receive<PlayMovieMessage>(
                message => ColorConsole.WriteLineRed(
                    "UserActor {0} Error: cannot start playing another movie before stopping existing one", _userId));
           
            Receive<StopMovieMessage>(message => StopPlayingCurrentMovie());

            ColorConsole.WriteLineYellow("UserActor {0} has now become Playing", _userId);
        }

        private void Stopped()
        {
            Receive<PlayMovieMessage>(message => StartPlayingMovie(message.MovieTitle));
          
            Receive<StopMovieMessage>(
                message => ColorConsole.WriteLineRed("UserActor {0} Error: cannot stop if nothing is playing", _userId));

            ColorConsole.WriteLineYellow("UserActor {0} has now become Stopped", _userId);
        }
        
        private void StartPlayingMovie(string title)
        {
            _currentlyWatching = title;

            ColorConsole.WriteLineYellow("UserActor {0} is currently watching '{1}'", _userId, _currentlyWatching);

            Context.ActorSelection("/user/Playback/PlaybackStatistics/MoviePlayCounter")
                .Tell(new IncrementPlayCountMessage(title));

            Become(Playing);
        }

        private void StopPlayingCurrentMovie()
        {
            ColorConsole.WriteLineYellow("UserActor {0} has stopped watching '{1}'", _userId, _currentlyWatching);

            _currentlyWatching = null;

            Become(Stopped);
        }



        #region Lifecycle hooks
        protected override void PreStart()
        {
            ColorConsole.WriteLineYellow("UserActor {0} PreStart", _userId);
        }

        protected override void PostStop()
        {
            ColorConsole.WriteLineYellow("UserActor {0} PostStop", _userId);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteLineYellow("UserActor {0} PreRestart because: {1}", _userId, reason);

            base.PreRestart(reason, message);
        }

        protected override void PostRestart(Exception reason)
        {
            ColorConsole.WriteLineYellow("UserActor {0} PostRestart because: {1}", _userId, reason);

            base.PostRestart(reason);
        } 
        #endregion
    }
}
