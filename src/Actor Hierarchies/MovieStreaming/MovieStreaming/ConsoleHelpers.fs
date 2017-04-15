module ConsoleHelpers

    open System

    let consolePrint c cwriter format =
        Printf.kprintf
            (fun s -> 
                let old = Console.ForegroundColor 
                try 
                  Console.ForegroundColor <- c;
                  cwriter(s)
                finally
                  Console.ForegroundColor <- old) format

    let cprintf c format = 
        consolePrint c (fun s -> Console.Write(s)) format
    
        
    // Colored printfn
    let cprintfn c format = 
        consolePrint c (fun s -> Console.WriteLine(s)) format