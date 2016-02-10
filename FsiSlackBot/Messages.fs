namespace FsiSlackBot

module Messages =
    
    type Error = string

    type DomainMessage =
        | BotWasNotMentioned
        | BotMentionCleaned of string
        | ExpressionEvaluated of string
        | ExpressionEvaluationFaild of Error
        | MessagePosted of string
        | MessagePosteFailed of Error

    let printMessage = function 
        | BotWasNotMentioned -> printfn "Bot was not mentioned, skipping message"
        | BotMentionCleaned s -> printfn "Bot mentioned stripped: %s" s
        | ExpressionEvaluated s -> printfn "Expression was evaluated to: %s" s
        | ExpressionEvaluationFaild e -> printfn "Expression evaluation failed: %s" e
        | MessagePosted s -> printfn "Message was posted: %s" s
        | MessagePosteFailed e -> printfn "Message posted failed: %s" e