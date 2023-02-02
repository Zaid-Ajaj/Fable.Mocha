module Program

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Xml
open System.Xml.Linq
open Fake.IO
open Fake.Core
open Newtonsoft.Json
open Newtonsoft.Json.Linq

let path xs = Path.Combine(Array.ofList xs)

let solutionRoot = Files.findParent __SOURCE_DIRECTORY__ "README.md";

let project name = path [ solutionRoot; $"Feliz.{name}" ]

let mocha = path [ solutionRoot; "src" ]
let tests = path [ solutionRoot; "tests" ]
let headlessRunner = path [ solutionRoot; "headless" ]

let publish projectDir =
    path [ projectDir; "bin" ] |> Shell.deleteDir
    path [ projectDir; "obj" ] |> Shell.deleteDir

    if Shell.Exec(Tools.dotnet, "pack --configuration Release", projectDir) <> 0 then
        failwithf "Packing '%s' failed" projectDir
    else
        let nugetKey =
            match Environment.environVarOrNone "NUGET_KEY" with
            | Some nugetKey -> nugetKey
            | None -> 
                printfn "The Nuget API key must be set in a NUGET_KEY environmental variable"
                System.Console.Write "Enter your NUGET_KEY here: "
                System.Console.ReadLine()

        let nugetPath =
            Directory.GetFiles(path [ projectDir; "bin"; "Release" ])
            |> Seq.head
            |> Path.GetFullPath

        if Shell.Exec(Tools.dotnet, sprintf "nuget push %s -s nuget.org -k %s" nugetPath nugetKey, projectDir) <> 0
        then failwith "Publish failed"

let dotnetExpectoTest() = 
    if Shell.Exec(Tools.dotnet, "run -c EXPECTO", tests) <> 0
    then failwith "Failed running tests using dotnet and Expecto"

let headlessTests() = 
    if Shell.Exec(Tools.npm, "run nagareyama-headless-tests", solutionRoot) <> 0
    then failwith "Headless tests failed :/"

let npmInstall() = 
    if Shell.Exec(Tools.npm, "install", solutionRoot) <> 0
    then failwith "Npm install failed"

let dotnetToolRestore() = 
    if Shell.Exec(Tools.dotnet, "tool restore", solutionRoot) <> 0
    then failwith "dotnet tool restore failed"

let nodejsTest() = 
    if Shell.Exec(Tools.npm, "test", solutionRoot) <> 0
    then failwith "Nodejs tests failed"

let addTypeModuleToPackageJson() = 
    let packageJsonPath = Path.Combine(solutionRoot, "package.json")
    let packageJson = JObject.Parse(File.ReadAllText(packageJsonPath))
    if packageJson.["type"] = null then
        packageJson.Add("type", "module")
        File.WriteAllText(packageJsonPath, packageJson.ToString())
    else
        printfn "package.json already has type module"

let removeTypeModuleFromPackageJson() = 
    let packageJsonPath = Path.Combine(solutionRoot, "package.json")
    let packageJson = JObject.Parse(File.ReadAllText(packageJsonPath))
    if packageJson.["type"] <> null then
        packageJson.Remove("type") |> ignore
        File.WriteAllText(packageJsonPath, packageJson.ToString())
    else
        printfn "package.json does not have type: module"

[<EntryPoint>]
let main (args: string[]) = 
    Console.OutputEncoding <- Encoding.UTF8
    try
        // run tasks
        match args with 
        | [| "publish-mocha" |] -> publish mocha
        | [| "publish-headless-runner" |] -> publish headlessRunner
        | [| "dotnet-test" |] -> dotnetExpectoTest()
        | [| "nodejs-test" |] -> 
            addTypeModuleToPackageJson()
            dotnetToolRestore()
            npmInstall()
            nodejsTest()
        | [| "headless-test" |] -> 
            removeTypeModuleFromPackageJson()
            dotnetToolRestore()
            npmInstall()
            headlessTests()
        | _ -> 
            printfn "Unknown args: %A" args
        
        // exit succesfully
        0
    with 
    | ex -> 
        // something bad happened
        printfn "Error occured"
        printfn "%A" ex
        1