namespace BotHost

module BotBootstrap =
    
    open System.Threading
    
    open FSharp.Configuration
    
    open FsiSlackBot
    open FsiSlackBot.SlackBot

    open SlackConnector.Models
    open System.Threading.Tasks
    open SlackConnector
    
    open SlackConnector.EventHandlers

    type Agent<'T> = Microsoft.FSharp.Control.MailboxProcessor<'T>
    
    type Settings = AppSettings<"app.config">

    let post (agent:Agent<BotMessage>) (hub:SlackChatHub) text =
        let message = BotMessage()
        message.Text <- text
        message.ChatHub <- hub
        agent.Post message

    let receiveMessage (agent: Agent<BotMessage>) (domainMsg: RecievedMessage) (slackMsg: SlackMessage) =
        processMessage { domainMsg with Question = slackMsg.Text
                                        UserName = slackMsg.User.Name
                                        IsBotMentioned = slackMsg.MentionsBot
                                        PostAction = post agent slackMsg.ChatHub
                       } |> ignore
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
    
    let toDomainMessage (connection: ISlackConnection) =         
        let bm = new BotMessage()
        {
            TeamId = connection.Team.Id
            TeamName = connection.Team.Name
            BotId = connection.Self.Id
            BotName = connection.Self.Name            
            // defaults
            Answer = null
            IsBotMentioned = false
            Question = null
            UserName = null
            PostAction = fun _ -> ()
        }

    let rec initBot() =
        let connection = SlackConnector().Connect(Settings.SlackKey).Result
        let domainMsg = toDomainMessage connection
        let agent = createAgent connection
        connection.add_OnMessageReceived (MessageReceivedEventHandler (fun msg -> receiveMessage agent domainMsg msg))
        connection.add_OnDisconnect (DisconnectEventHandler initBot)
        ()