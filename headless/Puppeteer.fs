module Puppeteer

open System
open System.IO
open Suave
open Suave.Operators
open Suave.Filters
open PuppeteerSharp
open System.Threading
open YoLo
open System.Diagnostics

let private isAbsolute path =
    Path.IsPathRooted(path)
    && not (Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))

let private validatePath path =
    if not (isAbsolute path) && Environment.OSVersion.Platform = PlatformID.Win32NT
    then
      printfn "Given path '%s' is relative. Please provide an absolute path instead" path
      Error <| async { return 1 }
    elif not (Directory.Exists path) then
      printfn "Given path '%s' does not exist" path
      Error <| async { return 1 }
    else
      Ok ()

let private run (launchOptions: LaunchOptions) (path: string) =
    let rnd = Random()
    let port = rnd.Next(5000, 9000)
    printfn "Chosen random port %d for the static files server" port
    let cts = new CancellationTokenSource()
    printfn "Serving files from '%s'" path

    let suaveConfig =
        { defaultConfig with
            homeFolder = Some path
            bindings   = [ HttpBinding.createSimple HTTP "127.0.0.1" port ]
            bufferSize = 2048
            cancellationToken = cts.Token }

    // simple static web server
    let webApp = GET >=> Files.browseHome

    let listening, server = startWebServerAsync suaveConfig webApp

    Async.Start server

    listening
    |> Async.RunSynchronously
    |> ignore

    printfn "Server started"
    printfn ""

    async {
        use! browser = Async.AwaitTask(Puppeteer.LaunchAsync(launchOptions))
        use! page = Async.AwaitTask(browser.NewPageAsync())
        printfn ""
        printfn "Navigating to http://localhost:%d/index.html" port
        let! _ = Async.AwaitTask (page.GoToAsync (sprintf "http://localhost:%d/index.html" port))
        let stopwatch = Stopwatch.StartNew()
        let toArrayFunction = """
        window.domArr = function(elements) {
            var arr = [ ];
            for(var i = 0; i < elements.length;i++) arr.push(elements.item(i));
            return arr;
        };
        """

        let getResultsFunctions = """
        window.getTests = function() {
            var tests = document.querySelectorAll("div.passed, div.executing, div.failed, div.pending");
            return domArr(tests).map(function(test) {
                var name = test.getAttribute('data-test')
                var type = test.classList[0]
                var module =
                    type === 'failed'
                    ? test.parentNode.parentNode.parentNode.getAttribute('data-module')
                    : test.parentNode.parentNode.getAttribute('data-module')

                return [name, type, module];
            });
        }
        """
        let! _ = Async.AwaitTask (page.EvaluateExpressionAsync(toArrayFunction))
        let! _ = Async.AwaitTask (page.EvaluateExpressionAsync(getResultsFunctions))
        printfn "Waiting for tests to finish executing..."
        // disable timeout when waiting for tests
        let waitOptions = WaitForFunctionOptions()
        waitOptions.Timeout <- Nullable(0)
        let! _ = Async.AwaitTask (page.WaitForExpressionAsync("document.getElementsByClassName('executing').length === 0", waitOptions))
        stopwatch.Stop()
        printfn "Finished running tests, took %d ms" stopwatch.ElapsedMilliseconds
        let passingTests = "document.getElementsByClassName('passed').length"
        let! passedTestsCount = Async.AwaitTask (page.EvaluateExpressionAsync<int>(passingTests))
        let failingTests = "document.getElementsByClassName('failed').length"
        let! failedTestsCount = Async.AwaitTask (page.EvaluateExpressionAsync<int>(failingTests))
        let pendingTests = "document.getElementsByClassName('pending').length"
        let! pendingTestsCount = Async.AwaitTask(page.EvaluateExpressionAsync<int>(pendingTests))
        let! testResults = Async.AwaitTask (page.EvaluateExpressionAsync<string [] []>("window.getTests()"))
        printfn ""
        printfn "========== SUMMARY =========="
        printfn ""
        printfn "Total test count %d" (passedTestsCount + failedTestsCount + pendingTestsCount)
        printfn "Passed tests %d" passedTestsCount
        printfn "Failed tests %d" failedTestsCount
        printfn "Skipped tests %d" pendingTestsCount
        printfn ""
        printfn "========== TESTS =========="
        printfn ""
        let moduleGroups = testResults |> Array.groupBy (fun arr -> arr.[2])

        for (moduleName, tests) in moduleGroups do
            for test in tests do
                let name = test.[0]
                let testType = test.[1]

                match testType with
                | "passed" ->
                    Console.ForegroundColor <- ConsoleColor.Green
                    printfn "√ %s / %s" moduleName name
                | "failed" ->
                    Console.ForegroundColor <- ConsoleColor.Red
                    printfn "X %s / %s" moduleName name
                | "pending" ->
                    Console.ForegroundColor <- ConsoleColor.Blue
                    printfn "~ %s / %s" moduleName name
                | _ ->
                    ignore()

        Console.ResetColor()
        printfn ""
        printfn "Stopping web server..."
        cts.Cancel()
        printfn "Exit code: %d" failedTestsCount
        return failedTestsCount
    }

let runTests (path: string) : Async<int> =
    match validatePath path with
    | Error result -> result
    | Ok () ->
        printfn ""
        printfn "========== SETUP =========="
        printfn ""
        printfn "Downloading chromium browser..."
        let browserFetcher = BrowserFetcher()
        browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore

        printfn "Chromium browser downloaded"
        let launchOptions = LaunchOptions(Headless = true, ExecutablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision))
        run launchOptions path

let runTestsWithConfig (config: {| ExecutablePath: string; Arguments: string[] |}) (path: string) : Async<int> =
    match validatePath path with
    | Error result -> result
    | Ok () ->
        printfn "Using browser located at: %s with additional arguments: %A" config.ExecutablePath config.Arguments
        let launchOptions = LaunchOptions(Headless = true, ExecutablePath = config.ExecutablePath, Args = config.Arguments)
        run launchOptions path