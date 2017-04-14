using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace MovieStreaming.Actors
{
    public class PlaybackActor : UntypedActor
    {

        public PlaybackActor()
        {
            Console.WriteLine("Creating a PlaybackActor");
        }
        protected override void OnReceive(object message)
        {
            throw new NotImplementedException();
        }
    }
}
