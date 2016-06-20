module ActorConfiguration

open Actor1
open Actor2
open Actor3
open Actor4
open Actor5
open Akka.Actor

let (config : System.Collections.Generic.IDictionary<int, ActorSystem -> unit>) = 
    dict[(1, start1); (2, start2); (3, start3); (4, start4); (5, start5); (6, start6); (7, start7); (8, start8); (9, start9)] 

let startActor system number =
    let (success, startfn) = config.TryGetValue(number)
    if success then
        startfn system
    else failwithf "There is no start function for actor %i" number
        