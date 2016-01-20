open System.Threading
open FsiSlackBot

[<EntryPoint>]
let main argv = 
   printfn "Starting Bot. Ctrl+C to exit."
   
   SlackBot.initBot() |> ignore
   Thread.Sleep Timeout.Infinite

   0
