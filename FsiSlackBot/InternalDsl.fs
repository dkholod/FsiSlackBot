namespace FsiSlackBot

module InternalDsl =

    let (|StartsWith|_|) prefix (candidate : string) =
        if candidate.StartsWith prefix then 
            Some ((candidate.Substring prefix.Length).Trim())
        else None

    let toString obj = 
        obj.ToString()

    let contains subString (str: string) = 
        str.ToLowerInvariant().Contains(subString)

    type Result<'TSuccess, 'TMessage> =
        | Success of 'TSuccess * 'TMessage list
        | Failure of 'TMessage list

    let succeed e =
        Success (e,[])
    
    let bind f res =         
        let mergeMsgs msgs = function 
            | Success (x, m) -> Success (x, msgs @ m)
            | Failure errors -> Failure (msgs @ errors)
        match res with
            | Success (e,m) -> f e |> mergeMsgs m
            | Failure errors -> Failure errors   

    let (>>=) inp f = bind f inp

    let onSuccess f input = 
        match input with
        | Success (e, m) -> f e
                            input
        | Failure errors -> Failure errors

    let (<+>) inp f = onSuccess f inp

    let onAny f input = 
        match input with
        | Success (e, m) -> f m                            
        | Failure errors -> f errors
        
        input

    let (<*>) inp f = onAny f inp