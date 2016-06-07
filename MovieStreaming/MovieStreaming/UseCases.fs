module ActorConfiguration

open Actor1
open Actor2
open Akka.Actor

let (config : System.Collections.Generic.IDictionary<int, ActorSystem -> unit>) = 
    dict[(1, start1); (2, start2); (3, start3); (4, start4)] 

let startActor system number =
    let (success, startfn) = config.TryGetValue(number)
    if success then
        startfn system
    else failwithf "There is no start function for actor %i" number
        