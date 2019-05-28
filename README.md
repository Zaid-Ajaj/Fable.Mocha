# Fable.Mocha [![Nuget](https://img.shields.io/nuget/v/Fable.Mocha.svg?colorB=green)](https://www.nuget.org/packages/Fable.Mocha) [![Build status](https://ci.appveyor.com/api/projects/status/3s3xorc0oevkuk4w?svg=true)](https://ci.appveyor.com/project/Zaid-Ajaj/fable-mocha)


[Fable](https://github.com/fable-compiler/Fable) library for testing. Inspired by the popular [Expecto](https://github.com/haf/expecto) library for F# and adopts the `testList`, `testCase` and `testCaseAsync` primitives for defining tests.

The tests themselves are written once and can run:
 - [Inside node.js using Mocha](#running-the-tests-on-node.js-with-mocha)
 - [Inside the broswer](#running-the-tests-using-the-browser) (standalone)
 - [Inside dotnet with Expecto](#running-the-tests-on-dotnet-with-Expecto)
 - To Do: Inside dotnet standalone (PR's are welcome)

![gif](live.gif)

## Installation
Install the Fable binding from Nuget
```bash
# using nuget
dotnet add package Fable.Mocha

# or with paket
paket add Fable.Mocha --project /path/to/project.fsproj
```

## Writing Tests
Assuming you have you the following project structure:
```
repo
 |
 |-- package.json
 |-- src
      | -- YourLibrary.fsproj
      | -- Library.fs
 |-- tests
      | -- Tests.fsproj
      | -- Tests.fs
```
Where `tests` contains the test project, you will install `Fable.Mocha` into the `Tests.fsproj` and it will look like this:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Tests.fs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\src\YourLibrary.fsproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Fable.Mocha" Version="1.0.0" />
  </ItemGroup>
</Project>
```
then you can start writing your tests into the `Test.fs` file:
```fs
module Tests

open Fable.Mocha

let arithmeticTests =
    testList "Arithmetic tests" [

        testCase "plus works" <| fun () ->
            Expect.areEqual (1 + 1) 2

        testCase "Test for falsehood" <| fun () ->
            Expect.isFalse (1 = 2)

        testCaseAsync "Test async code" <| fun () ->
            async {
                let! x = async { return 21 }
                let answer = x * 2
                Expect.areEqual 42 answer
            }
    ]

Mocha.runTests arithmeticTests
```

## Running the tests on node.js with Mocha

Install the actual `mocha` test runner from Npm along side `fable-splitter` that will compile your test project into a node.js application
```bash
npm install --save-dev mocha fable-splitter
```
Add the following `pretest` and `test` npm scripts to your `package.json` file:
```json
"pretest": "fable-splitter tests -o dist/tests --commonjs",
"test": "mocha dist/tests"
```
Now you can simply run `npm test` in your terminal and it will run the `pretest` script to compile the test project and afterwards the `test` script to actually run the (compiled) tests using mocha.

## Running the tests on dotnet with Expecto

Since the API exactly follows that of Expecto's you can simply run the tests on dotnet as well using Expecto. This way you can check whether your code runs correctly on different platforms whether it is dotnet or node.js. This is achieved using *compiler directives* as follows. First of all you need to install the `Expecto` library from nuget:
```
dotnet add package Expecto
```
then Add a special compiler directory used from the configuration flag to your `Tests.fsproj`:
```xml
<PropertyGroup Condition="'$(Configuration)'=='EXPECTO'">
  <DefineConstants>$(DefineConstants);EXPECTO</DefineConstants>
</PropertyGroup>
```
This means that the compiler directive called `EXPECTO` will be active when you run your project like this:
```bash
dotnet run -c EXPECTO
```
which is short for
```bash
dotnet run --configuration EXPECTO
```
Now from your tests you need to hide the Fable stuff when `EXPECTO` is active, this means opening `Expecto` namespace and ignoring `Fable.Mocha`
```fs
#if EXPECTO
open Expecto
#else
open Fable.Mocha
#endif
```
The same goes for the entry point:
```fs
[<EntryPoint>]
let main args =
#if EXPECTO
    runTestsWithArgs defaultConfig args allTests
#else
    Mocha.runTests allTests
#endif
```
And you are done, the code of the tests themselves doesn't need to change! Of course assuming you don't have platform specific code in there. This feature is made to test pure F# code that should give the same results with dotnet and Fable.

## Running the tests using the browser
Trying to use mocha to run tests in the browser will give you headaches as you have to include the compiled individual test files by yourself along with mocha specific dependencies. That's why Fable.Mocha includes a *built-in* test runner for the browser. You don't need to change anything in the existing code, it just works!

Compile your test project using default fable/webpack as follows.

First, install `fable-loader` along with with `webpack` if you haven't already:
```
npm install fable-loader webpack webpack-cli webpack-dev-server
```
Add an `index.html` page inside directory called `public` that contains:
```html
<!DOCTYPE html>
<html>
    <head>
        <title>Mocha tests</title>
    </head>
    <body>
        <script src="bundle.js"></script>
    </body>
</html>
```
Create a webpack config file that compiles your `Tests.fsproj`
```js
var path = require("path");

module.exports = {
    entry: "./tests/Tests.fsproj",
    output: {
        path: path.join(__dirname, "./public"),
        filename: "bundle.js",
    },
    devServer: {
        contentBase: "./public",
        port: 8080,
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
}
```

Now you can run your tests live using webpack-dev-server or compile the tests and run them by yourself. Add these scripts to your `package.json`
```
"start": "webpack-dev-server",
"build-for-browser": "webpack"
```
Now if you run `npm start` you can navigate to `http://localhost:8080` to see the results of your tests.

### Testing multiple modules
The function `Mocha.runTests` takes in a list of test modules. Each test module is created using the `testList` function so you can seperate your tests into these modules:
```fs
module Tests

open Fable.Mocha

let firstModuleTests =
    testList "firstModule" [
        testCase "module works properly" <| fun _ ->
            let answer = 21
            Expect.areEqual 42 (answer * 2)
    ]

let secondModuleTests =
    testList "secondModuleModule" [
        testCase "module works properly" <| fun _ ->
            let answer = 31415.0
            let pi = answer / 10000.0
            Expect.areEqual 3.1415 pi
    ]

// Tests can be nested too!
let nestedTests =
    testList "first level" [
        testList "second level" [
            testCase "my test code" <| fun _ -> ()
        ]
    ]

let allTests = testList "All" [
    firstModuleTests
    secondModuleTests
    nestedTests
]

Mocha.runTests allTests
```