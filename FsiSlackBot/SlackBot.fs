namespace FsiSlackBot

open System.Threading.Tasks

open InternalDsl
open Messages

module SlackBot = 
    
    type RecievedMessage = { 
        TeamId: string
        TeamName: string
        BotId: string
        BotName: string
        IsBotMentioned: bool        
        Question: string
        Answer: string
        UserName: string
        PostAction: string -> unit
    }

    let checkBotMentioned message =
        if message.IsBotMentioned then
            Success (message, [])
        else Failure [BotWasNotMentioned]

    let cleanupBotMention msg =
        let botId = msg.BotId |> sprintf "<@%s>"
        let botIdRef = botId + ":"
        let cleanMsg = 
            match msg.Question with
                | StartsWith botIdRef msg -> msg
                | StartsWith botId msg -> msg
                | _ -> msg.Question
        Success({ msg with Question = cleanMsg }, [BotMentionCleaned cleanMsg])

    let parseFsiExpression msg =
        try
            let parsed = SessionRunner.processMention msg.Question
                         |> SessionRunner.composeResponse msg.UserName
            Success({ msg with Answer = parsed }, [ExpressionEvaluated parsed])
        with
        | ex -> Failure [ExpressionEvaluated ex.Message]

    let postReply msg =
        try            
            msg.PostAction msg.Answer
            Success(msg, [MessagePosted (sprintf "| Team: %s | Question: %s | Reply: %s |" msg.TeamName msg.Question msg.Answer)])
        with
        | ex -> Failure [MessagePosteFailed ex.Message]

    let logToAzure msg =
        do AzureTableLog.write msg.TeamName msg.UserName msg.Question msg.Answer

    let printLog (msgList: DomainMessage list) =
        msgList |> List.iter printMessage

    let processMessage message =
        succeed message
        >>= checkBotMentioned
        >>= cleanupBotMention
        >>= parseFsiExpression
        >>= postReply
        <+> logToAzure
        <*> printLog