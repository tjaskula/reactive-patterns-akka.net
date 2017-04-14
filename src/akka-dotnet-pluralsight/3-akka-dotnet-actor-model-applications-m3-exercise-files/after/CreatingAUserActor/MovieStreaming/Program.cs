using System;
using Akka.Actor;
using MovieStreaming.Actors;
using MovieStreaming.Messages;

namespace MovieStreaming
{
    internal class Program
    {
        private static ActorSystem MovieStreamingActorSystem;

        private static void Main(string[] args)
        {
            MovieStreamingActorSystem = ActorSystem.Create("MovieStreamingActorSystem");
            Console.WriteLine("Actor system created");

            Props userActorProps = Props.Create<UserActor>();
            IActorRef userActorRef = MovieStreamingActorSystem.ActorOf(userActorProps, "UserActor");


            Console.ReadKey();
            Console.WriteLine("Sending a PlayMovieMessage (Codenan the Destroyer)");
            userActorRef.Tell(new PlayMovieMessage("Codenan the Destroyer", 42));

            Console.ReadKey();
            Console.WriteLine("Sending another PlayMovieMessage (Boolean Lies)");
            userActorRef.Tell(new PlayMovieMessage("Boolean Lies", 42));

            Console.ReadKey();
            Console.WriteLine("Sending a StopMovieMessage");
            userActorRef.Tell(new StopMovieMessage());

            Console.ReadKey();
            Console.WriteLine("Sending another StopMovieMessage");
            userActorRef.Tell(new StopMovieMessage());



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
