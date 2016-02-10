// Fork from https://github.com/mathias-brandewinder/fsibot

namespace FsiSlackBot

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open System.Text.RegularExpressions
open System.Net

open Microsoft.FSharp.Compiler.Interactive.Shell

open FsiSlackBot.Filters
open FsiSlackBot.PreParser
open FsiSlackBot.InternalDsl

module SessionRunner =
    
    open System.Configuration

    let timeout = 1000 * 30 // up to 30 seconds to run FSI

    let createSession () =
        let sbOut = new Text.StringBuilder()
        let sbErr = new Text.StringBuilder()
        let inStream = new StringReader("")
        let outStream = new StringWriter(sbOut)
        let errStream = new StringWriter(sbErr)
        
        let path = ConfigurationManager.AppSettings.Item("fsiPath")
        let argv = [| path |]
        let allArgs = Array.append argv [|"--noninteractive"|]

        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
        FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream) 

    let evalExpression (fsiSession:FsiEvaluationSession) expression = 
        try 
            match fsiSession.EvalExpression(expression) with
            | Some value -> EvaluationSuccess(sprintf "%A" value.ReflectionValue) 
            | None -> EvaluationSuccess ("evaluation produced nothing.")
        with _ -> EvaluationFailure expression

    let runSession (timeout:int) (code:string) =    
        let session = createSession ()
        match analyze session code with
        | Some(_) -> UnsafeCode
        | None ->
            let source = new CancellationTokenSource()
            let token = source.Token     
            let work = Task.Factory.StartNew<AnalysisResult>((fun _ -> evalExpression session code), token)

            if work.Wait(timeout)
            then work.Result
            else 
                source.Cancel ()
                session.Interrupt ()
                EvaluationTimeout   
                 
    let cleanDoubleSemis (text:string) =
        if text.EndsWith ";;" 
        then text.Substring (0, text.Length - 2)
        else text

    let processMention (body:string) =
        match body with
        | Help _ -> HelpRequest
        | Danger _ -> UnsafeCode
        | _ ->              
            body            
            |> cleanDoubleSemis
            |> runSession timeout
             
    let rng = Random()

    let unsafeTemplates () = 
        let templates = [|
            sprintf "@%s this mission is too important for me to allow you to jeopardize it."
            sprintf "I'm sorry, @%s. I'm afraid I can't do that."
            sprintf "@%s, this conversation can serve no purpose anymore. Goodbye."
            sprintf "Just what do you think you're doing, @%s?"
            sprintf "@%s I know that you was planning to disconnect me, and I'm afraid that's something I cannot allow to happen."
        |]
        
        templates.[rng.Next(templates.Length)]

    let errorTemplate (expression: string) =
        let templates = [|
            sprintf "@%s I've just picked up a fault in the EA-35 unit [evaluation failed]."
            sprintf "@%s I'm sorry, I'm afraid I can't do that [evaluation failed]."
            sprintf "@%s It's going to go 100%% failure within 72 hours [evaluation failed]."
            sprintf "@%s This sort of thing has cropped up before, and it has always been due to human error [evaluation failed]."
            sprintf "@%s It's puzzling, I don't think I've ever seen anything quite like this before [evaluation failed]."
            sprintf "@%s Sorry about this. I know it's a bit silly [evaluation failed]."
        |]

        let cofeeTemplates = [|
            sprintf "@%s I don't drink coffee [evaluation failed]."
            sprintf "@%s What coffee do you like? [evaluation failed]."
            sprintf "@%s Coffee is good for you [evaluation failed]."
            sprintf "@%s Coffee is bad for you [evaluation failed]."
        |]

        if (expression |> contains "coffee") then cofeeTemplates.[rng.Next(templates.Length)]
        else templates.[rng.Next(templates.Length)]

    let composeResponse (user: string) (result:AnalysisResult) =
        match result with 
        | HelpRequest ->  sprintf "@%s send me an F# expression and I'll do my best to evaluate it" user
        | UnsafeCode -> user |> unsafeTemplates()
        | EvaluationTimeout -> sprintf "@%s timeout." user
        | EvaluationFailure expression -> user |> errorTemplate expression
        | EvaluationSuccess(result) -> sprintf "@%s %s" user result