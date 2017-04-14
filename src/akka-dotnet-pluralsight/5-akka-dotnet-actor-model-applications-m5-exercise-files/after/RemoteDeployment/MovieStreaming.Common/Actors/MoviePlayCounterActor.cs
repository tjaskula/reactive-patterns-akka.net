using System;
using System.Collections.Generic;
using Akka.Actor;
using MovieStreaming.Common.Exceptions;
using MovieStreaming.Common.Messages;

namespace MovieStreaming.Common.Actors
{
    public class MoviePlayCounterActor : ReceiveActor
    {
        private readonly Dictionary<string, int> _moviePlayCounts;

        public MoviePlayCounterActor()
        {
            ColorConsole.WriteMagenta("MoviePlayCounterActor constructor executing");
            _moviePlayCounts = new Dictionary<string, int>();

            Receive<IncrementPlayCountMessage>(message => HandleIncrementMessage(message));
        }

        private void HandleIncrementMessage(IncrementPlayCountMessage message)
        {
            if (_moviePlayCounts.ContainsKey(message.MovieTitle))
            {
                _moviePlayCounts[message.MovieTitle]++;
            }
            else
            {
                _moviePlayCounts.Add(message.MovieTitle, 1);
            }

            //  Simulated bugs
            if (_moviePlayCounts[message.MovieTitle] > 3)
            {
                throw new SimulatedCorruptStateException();
            }

            if (message.MovieTitle == "Partial Recoil")
            {
                throw new SimulatedTerribleMovieException();
            }

            ColorConsole.WriteMagenta(
                "MoviePlayCounterActor '{0}' has been watched {1} times", 
                message.MovieTitle, _moviePlayCounts[message.MovieTitle]);
        }



        #region Lifecycle hooks

        protected override void PreStart()
        {
            ColorConsole.WriteMagenta("MoviePlayCounterActor PreStart");
        }

        protected override void PostStop()
        {
            ColorConsole.WriteMagenta("MoviePlayCounterActor PostStop");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteMagenta("MoviePlayCounterActor PreRestart because: {0}", reason.Message);

            base.PreRestart(reason, message);
        }

        protected override void PostRestart(Exception reason)
        {
            ColorConsole.WriteMagenta("MoviePlayCounterActor PostRestart because: {0} ", reason.Message);

            base.PostRestart(reason);
        }
        #endregion
    }
}
