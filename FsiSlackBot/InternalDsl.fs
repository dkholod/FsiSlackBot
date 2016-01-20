namespace FsiSlackBot

open FSharp.Configuration

module InternalDsl =

    type Agent<'T> = Microsoft.FSharp.Control.MailboxProcessor<'T>

    type Settings = AppSettings<"app.config">

    let (|StartsWith|_|) prefix (candidate : string) =
        if candidate.StartsWith prefix then 
            Some ((candidate.Substring prefix.Length).Trim())
        else None

    let toString obj = 
        obj.ToString()