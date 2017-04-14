using System;
using Akka.Actor;
using MovieStreaming.Actors;
using MovieStreaming.Messages;

namespace MovieStreaming
{
    class Program
    {
        private static ActorSystem MovieStreamingActorSystem;

        static void Main(string[] args)
        {            
            MovieStreamingActorSystem = ActorSystem.Create("MovieStreamingActorSystem");
            Console.WriteLine("Actor system created");

            Props playbackActorProps = Props.Create<PlaybackActor>();
            IActorRef playbackActorRef = MovieStreamingActorSystem.ActorOf(playbackActorProps, "PlaybackActor");
            

            playbackActorRef.Tell(new PlayMovieMessage("Akka.NET: The Movie", 42));
            playbackActorRef.Tell(new PlayMovieMessage("Partial Recall", 99));             
            playbackActorRef.Tell(new PlayMovieMessage("Boolean Lies", 77));
            playbackActorRef.Tell(new PlayMovieMessage("Codenan the Destroyer", 1));


            // press any key to start shutdown of system
            Console.ReadKey();
            

            // Tell actor system (and all child actors) to shutdown
            MovieStreamingActorSystem.Shutdown();
            // Wait for actor system to finish shutting down
            MovieStreamingActorSystem.AwaitTermination();
            Console.WriteLine("Actor system shutdown");


            // Press any key to stop console application
            Console.ReadKey();
        }
    }
}
