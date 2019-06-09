
// included package Fable.MochaPuppeteerRunner

[<EntryPoint>]
let main argv =
    "../public"
    |> System.IO.Path.GetFullPath
    |> Puppeteer.runTests
    |> Async.RunSynchronously


