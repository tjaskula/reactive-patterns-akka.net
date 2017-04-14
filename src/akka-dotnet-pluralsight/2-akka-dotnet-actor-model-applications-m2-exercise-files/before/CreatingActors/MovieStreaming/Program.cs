using System;
using Akka.Actor;

namespace MovieStreaming
{
    class Program
    {
        private static ActorSystem MovieStreamingActorSystem;

        static void Main(string[] args)
        {
            MovieStreamingActorSystem = ActorSystem.Create("MovieStreamingActorSystem");
            Console.WriteLine("Actor system created");



            Console.ReadLine();

            MovieStreamingActorSystem.Shutdown();
        }
    }
}
