using System;
using Akka.Actor;
using MovieStreaming.Common.Exceptions;

namespace MovieStreaming.Common.Actors
{
    public class PlaybackStatisticsActor : ReceiveActor
    {
        public PlaybackStatisticsActor()
        {
            Context.ActorOf(Props.Create<MoviePlayCounterActor>(), "MoviePlayCounter");
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                exception =>
                {
                    if (exception is SimulatedCorruptStateException)
                    {
                        return Directive.Restart;
                    }
                    if (exception is SimulatedTerribleMovieException)
                    {
                        return Directive.Resume;
                    }

                    return Directive.Restart;
                }
                );

        }

        #region Lifecycle hooks

        protected override void PreStart()
        {
            ColorConsole.WriteWhite("PlaybackStatisticsActor PreStart");
        }

        protected override void PostStop()
        {
            ColorConsole.WriteWhite("PlaybackStatisticsActor PostStop");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteWhite("PlaybackStatisticsActor PreRestart because: {0}", reason.Message);

            base.PreRestart(reason, message);
        }

        protected override void PostRestart(Exception reason)
        {
            ColorConsole.WriteWhite("PlaybackStatisticsActor PostRestart because: {0} ", reason.Message);

            base.PostRestart(reason);
        }
        #endregion
    }
}