open Akka.Actor
open Akka.FSharp 
open System
open System.Threading
open ComposeIt.Akka.FSharp.Extensions.Actor

open ConsoleHelpers
open Messages

[<EntryPoint>]
let main argv = 

    cprintfn ConsoleColor.Gray "Creating MovieStreamingActorSystem"
    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()

    cprintfn ConsoleColor.Gray "Creating actor supervisory hierarchy"

    let rec moviePlayer userId lastState = function
        | Lifecycle e ->
            match e with
            | PreStart -> cprintfn ConsoleColor.Yellow "UserActor %i PreStart" userId
            | PostStop -> cprintfn ConsoleColor.Yellow "UserActor %i PostStop" userId
            | PreRestart (exn, _) -> cprintfn ConsoleColor.Yellow "UserActor %i PreRestart because: %A" userId exn
            | PostRestart exn -> cprintfn ConsoleColor.Yellow "UserActor %i PostRestart because: %A" userId  exn
            become (moviePlayer userId (Stopped ""))
        | Message m ->
            match m with
            | PlayMovie p -> 
                match lastState with
                | Playing _ -> cprintfn ConsoleColor.Red "UserActor %i Error: cannot start playing another movie before stopping existing one" p.UserId
                | Stopped _ -> cprintfn ConsoleColor.Yellow "UserActor %i is currently watching '%s'" p.UserId p.MovieTitle
                               cprintfn ConsoleColor.Cyan "UserActor %i has now become Playing" p.UserId
                become (moviePlayer userId (Playing p.MovieTitle))        
            | StopMovie s -> 
                match lastState with
                | Playing t -> cprintfn ConsoleColor.Yellow "UserActor %i has stopped watching '%s'" s.UserId t
                               cprintfn ConsoleColor.Cyan "UserActor %i has now become Stopped" s.UserId
                | Stopped _ -> cprintfn ConsoleColor.Red "UserActor %i Error: cannot stop if nothing is playing" s.UserId
                become (moviePlayer userId (Stopped ""))
        | _ -> become (moviePlayer userId (Stopped ""))
    
    let playback = 
        spawn system "Playback"
        <| fun playbackMailbox ->
            let userCoordinator = 
                spawn playbackMailbox "UserCoordinator" 
                    <| fun userCoordinatorMailbox ->
                        let rec userCoordinatorLoop (users: Map<int, IActorRef>) = actor {
                            let createChildUserIfNotExists userId =
                                if not (users.ContainsKey userId) then
                                    let user = 
                                        spawn userCoordinatorMailbox (sprintf "User%i" userId)
                                            <| actorOf(cprintfn ConsoleColor.Gray "Creating a UserActor %i" userId
                                                       cprintfn ConsoleColor.Cyan "UserActor %i: Setting initial behavior to Stopped" userId
                                                       moviePlayer userId (Stopped ""))
                                    let newUsers = users.Add (userId, user)
                                    cprintfn ConsoleColor.Cyan "UserCoordinatorActor created new child UserActor for %i (Total Users: %i)" userId newUsers.Count
                                    newUsers
                                else users

                            let! msg = userCoordinatorMailbox.Receive()
                            
                            let newUsers = 
                                match msg with
                                | Lifecycle evt ->
                                    match evt with
                                    | PreStart -> cprintfn ConsoleColor.Cyan "UserCoordinatorActor PreStart"
                                    | PostStop -> cprintfn ConsoleColor.Cyan "UserCoordinatorActor PostStop"
                                    | PreRestart(e, _) -> cprintfn ConsoleColor.Cyan "UserCoordinatorActor PreRestart because: %A" e
                                    | PostRestart e -> cprintfn ConsoleColor.Cyan "UserCoordinatorActor PostRestart because: %A" e
                                    users
                                | Message m ->
                                    match m with
                                    | PlayMovie pmm -> 
                                        let newUsers = createChildUserIfNotExists pmm.UserId
                                        let childActorRef = newUsers.[pmm.UserId]
                                        childActorRef <! m
                                        newUsers
                                    | StopMovie smm ->
                                        let newUsers = createChildUserIfNotExists smm.UserId
                                        let childActorRef = newUsers.[smm.UserId]
                                        childActorRef <! m
                                        newUsers
                                | _ -> userCoordinatorMailbox.Unhandled msg
                                       cprintfn ConsoleColor.Red "UserCoordinatorActor unhandled message '%A'" msg
                                       users
                            return! userCoordinatorLoop newUsers
                        }
                        userCoordinatorLoop Map.empty
            let playbackStatistics =
                spawn playbackMailbox "PlaybackStatistics" 
                    <| fun playbackStatisticsMailbox ->
                        let rec playbackStatisticsLoop() = actor {
                            let! msg = playbackStatisticsMailbox.Receive()
                            match msg with
                            | Lifecycle evt ->
                                match evt with
                                | PreStart -> cprintfn ConsoleColor.White "PlaybackStatisticsActor PreStart"
                                | PostStop -> cprintfn ConsoleColor.White "PlaybackStatisticsActor PostStop"
                                | PreRestart(e, _) -> cprintfn ConsoleColor.White "PlaybackStatisticsActor PreRestart because: %A" e
                                | PostRestart e -> cprintfn ConsoleColor.White "PlaybackStatisticsActor PostRestart because: %A" e
                            | _ -> ()
                            return! playbackStatisticsLoop()
                        }
                        playbackStatisticsLoop()
            
            // define parent behavior
            let rec loop() = actor {
                let! msg = playbackMailbox.Receive()
                match msg with
                | Lifecycle evt ->
                    match evt with
                    | PreStart -> cprintfn ConsoleColor.Green "PlaybackActor PreStart"
                    | PostStop -> cprintfn ConsoleColor.Green "PlaybackActor PostStop"
                    | PreRestart(e, _) -> cprintfn ConsoleColor.Green "PlaybackActor PreRestart because: %A" e
                    | PostRestart e -> cprintfn ConsoleColor.Green "PlaybackActor PostRestart because: %A" e
                | Message _ ->
                    userCoordinator.Forward(msg)  // forward all messages through
                    playbackStatistics.Forward(msg)
                | _ -> playbackMailbox.Unhandled msg
                return! loop ()
            }
            loop ()

    let rec readConsole() =

        Thread.Sleep(450)

        Console.WriteLine(Environment.NewLine)
        Console.WriteLine("Enter a command and hit enter")
        let command = Console.ReadLine()
        
        if command.StartsWith("play") then
            let userId = command.Split(',').[1] |> int
            let movieTitle = command.Split(',').[2]

            let message = PlayMovie {MovieTitle = movieTitle; UserId = userId}
            let aref = select "/user/Playback/UserCoordinator" system
            aref <! message

            readConsole()

        elif command.StartsWith("stop") then
            let userId = command.Split(',').[1] |> int
            let message = StopMovie {UserId = userId}
            let aref = select "/user/Playback/UserCoordinator" system
            aref <! message

            readConsole()

        elif command.StartsWith("exit") then
            ()

    readConsole()

    // tells the actor system (and all child actors) to shutdown
    system.Terminate() |> ignore

    // wait for actor system to finish shutting down
    system.WhenTerminated.Wait(TimeSpan.FromSeconds(2.)) |> ignore
    cprintfn ConsoleColor.Gray "Actor system shutdown."
    Console.ReadKey() |> ignore

    0 // return an integer exit code