#I "../packages/Newtonsoft.Json.7.0.1/lib/net45/"
#r "../packages/Akka.1.0.4/lib/net45/Akka.dll"
#r "../packages/Akka.FSharp.1.0.4/lib/net45/Akka.FSharp.dll"
#r "../packages/FsPickler.1.2.21/lib/net45/FsPickler.dll"
#r "Newtonsoft.Json.dll"

open Akka.FSharp
open Akka.Configuration
open System

type SomeActorMessages =
    | Greet of string
    | Hi

type SomeActor() =    
    inherit Actor()

    override __.OnReceive message =
        match message with
        | :? SomeActorMessages as m ->  
            match m with
            | Greet(name) -> printfn "Hello in actor %s" name
            | Hi -> printfn "Hello from actor F#!"
        | _ -> failwith "unknown message"

let main() =
    printf "Greeter example:\n"
    let system = ConfigurationFactory.Default() |> System.create "FSharpActors"
    let actor = 
        spawn system "MyActor"
        <| fun mailbox ->
            let rec again name =
                actor {
                    let! message = mailbox.Receive()
                    match message with
                    | Greet(n) when n = name ->
                        printfn "Hello again, %s" name
                        return! again name
                    | Greet(n) -> 
                        printfn "Hello %s" n
                        return! again n
                    | Hi -> 
                        printfn "Hello from F#!"
                        return! again name }
            and loop() =
                actor {
                    let! message = mailbox.Receive()
                    match message with
                    | Greet(name) -> 
                        printfn "Hello %s" name
                        return! again name
                    | Hi ->
                        printfn "Hello from F#!"
                        return! loop() } 
            loop()

    actor <! Greet "roger"
    actor <! Hi
    actor <! Greet "roger"
    actor <! Hi
    actor <! Greet "jeremie"
    actor <! Hi
    actor <! Greet "jeremie"