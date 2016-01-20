namespace FsiSlackBot

open System.Threading.Tasks

open InternalDsl

open SlackConnector
open SlackConnector.Models
open SlackConnector.EventHandlers

module SlackBot = 

    type ConnectionData = { 
        TeamId: string; 
        TeamName: string; 
        BotId: string; 
        BotName: string; 
        Post: BotMessage -> unit 
    }

    let cleanupBotMention message botId =                
        let botName = botId |> sprintf "<@%s>"
        let botNameRef = botName + ":"

        match message with
        | StartsWith botNameRef msg -> msg
        | StartsWith botName msg -> msg
        | _ -> message

    let parseFsiExpression user msg = 
        SessionRunner.processMention msg
        |> SessionRunner.composeResponse user

    let toBotMessage hub msg = 
        let message = BotMessage()
        message.Text <- msg
        message.ChatHub <- hub
        message
        
    let log team user question answer =
        printfn "Team: %s - Question: %s " team question
        printfn "Relaying with message: %s " answer
        do AzureTableLog.write team user question answer
        answer

    let receiveMessage (message: SlackMessage) connection =
        if message.MentionsBot then
            let question = cleanupBotMention message.Text connection.BotId
            let userName = message.User.Name

            question
            |> parseFsiExpression userName
            |> log connection.TeamName userName question
            |> toBotMessage message.ChatHub
            |> connection.Post
        
        new Task(fun () -> ())
        
    let createAgent (connection: ISlackConnection) =
        let agent = new Agent<BotMessage>(fun inbox ->
            let rec messageLoop () = async {
                let! msg = inbox.Receive()
                connection.Say(msg).Wait()
                return! messageLoop()
                }

            messageLoop())
        do agent.Start()
        agent

    let rec initBot() =
        let connection = SlackConnector().Connect(Settings.SlackKey).Result
        let agent = createAgent connection
       
        let data = {
                TeamId = connection.Team.Id
                TeamName = connection.Team.Name
                BotId = connection.Self.Id
                BotName = connection.Self.Id
                Post = agent.Post
            }

        connection.add_OnMessageReceived (MessageReceivedEventHandler (fun msg -> receiveMessage msg data))
        connection.add_OnDisconnect (DisconnectEventHandler initBot)
        ()