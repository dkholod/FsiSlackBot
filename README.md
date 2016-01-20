# FsiSlackBot
F# interactive Bot for Slack

##### Description
A simple Slack Bot to interpret F# expressions.
![FsiSlackBot](https://raw.githubusercontent.com/wiki/dkholod/FsiSlackBot/FsiBotChat.png)

##### Set-up and run
- Compile solution
- In BotHost project's app.config file modify the folowing settings 

`fsiPath` - specify path to F# interactive Fsi.exe e.g. C:\Program Files (x86)\Microsoft SDKs\F#\4.0\Framework\v4.0\Fsi.exe  
`slackKey` - specify Slack key assosiated with your Bot    
`AzureTable` - connection string to Azure Table for logging  

- Run `BotHost.exe`. The Bot will respond while it keeps running.

##### Credits
F# interpretation and security is powered by https://github.com/mathias-brandewinder/fsibot aka Tweeter [@fsibot](https://twitter.com/fsibot)  
Slack communication works as magic by means of https://github.com/noobot/SlackConnector
