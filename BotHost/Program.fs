open System.Threading

open BotHost

[<EntryPoint>]
let main argv = 
   printfn "Starting Bot. Ctrl+C to exit."
   
   BotBootstrap.initBot() |> ignore
   Thread.Sleep Timeout.Infinite

   0