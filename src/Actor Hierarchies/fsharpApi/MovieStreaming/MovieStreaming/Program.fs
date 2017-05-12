﻿open Akka.Actor
open Akka.FSharp 
open System
open System.Threading
open ComposeIt.Akka.FSharp.Extensions.Actor

open Actors
open ConsoleHelpers
open Messages

[<EntryPoint>]
let main argv = 

    cprintfn ConsoleColor.Gray "Creating MovieStreamingActorSystem"
    let system = System.create "MovieStreamingActorSystem" <| Configuration.load()

    cprintfn ConsoleColor.Gray "Creating actor supervisory hierarchy"
    
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
                                            <| fun userMailbox ->
                                                let rec userLoop userId = actor {
                                                    return! userLoop 1
                                                }
                                                userLoop 0
                                    let newUsers = users.Add (userId, user)
                                    cprintfn ConsoleColor.Cyan "UserCoordinatorActor created new child UserActor for %i (Total Users: %i)" userId users.Count
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
                                        let childActorRef = users.[pmm.UserId]
                                        childActorRef <! pmm
                                        newUsers
                                    | StopMovie smm ->
                                        let newUsers = createChildUserIfNotExists smm.UserId
                                        let childActorRef = users.[smm.UserId]
                                        childActorRef <! smm
                                        newUsers
                                | _ -> userCoordinatorMailbox.Unhandled msg
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
                | Message m ->
                    userCoordinator.Forward(m)  // forward all messages through
                    playbackStatistics.Forward(m)
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

            let message = {MovieTitle = movieTitle; UserId = userId}
            let aref = select "/user/Playback/UserCoordinator" system
            aref <! message

            readConsole()

        elif command.StartsWith("stop") then
            let userId = command.Split(',').[1] |> int
            let message = {UserId = userId}
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