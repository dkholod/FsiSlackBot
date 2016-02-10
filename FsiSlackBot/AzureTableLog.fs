namespace FsiSlackBot

open System

open FSharp.Azure.StorageTypeProvider
open FSharp.Azure.StorageTypeProvider.Table

module AzureTableLog =
    
    type FromConfig = AzureTypeProvider<connectionStringName = "AzureTable", configFileName="App.config">
    
    type LogRecord = { User: string; Question: string; Answer: string }

    let logsTable = FromConfig.Tables.Logs

    let stringGuid () = 
        Guid.NewGuid().ToString()

    let write team user question answer = 
       try
            logsTable.Insert(team |> Partition, stringGuid() |> Row, { User = user; Question = question; Answer = answer }) |> ignore
       with | _ -> ()