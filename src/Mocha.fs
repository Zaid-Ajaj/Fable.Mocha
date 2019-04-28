namespace Fable.Mocha

open Fable.Core.Testing
open Fable.Core 

type TestCase = 
    | SyncTest of string * (unit -> unit) 
    | AsyncTest of string * (unit -> Async<unit>)

type TestModule = TestModule of string * TestCase list

[<AutoOpen>]
module Test = 
    let testCase name body = SyncTest(name, body)
    let testCaseAsync name body = AsyncTest(name, body)
    let testList name tests = TestModule(name, tests)

[<RequireQualifiedAccess>]
module Expect = 
    let areEqual expected actual : unit =
        if expected = actual 
        then Assert.AreEqual(expected, actual)
        else failwithf "Expected %A but got %A" expected actual

    let isTrue cond = areEqual true cond 
    let isFalse cond = areEqual false cond 
    let isZero number = areEqual 0 number 
    let isEmpty (x: 'a seq) = areEqual true (Seq.isEmpty x)
    let pass() = areEqual true true

module Mocha = 
    let [<Global>] private describe (name: string) (f: unit->unit) = jsNative
    let [<Global>] private it (msg: string) (f: unit->unit) = jsNative

    let [<Emit("it($0, $1)")>] private itAsync msg (f: (unit -> unit) -> unit) = jsNative 
     
    let runTests testModules = 
        for TestModule(name, testCases) in testModules do 
            describe name <| fun () ->
                testCases
                |> List.iter (function 
                    | SyncTest(msg, test) -> 
                        it msg test
                    | AsyncTest(msg, test) -> 
                        itAsync msg (fun finished -> 
                            async {
                                match! Async.Catch(test()) with 
                                | Choice1Of2 () -> do finished()
                                | Choice2Of2 err -> do finished(unbox err)
                            } |> Async.StartImmediate)) 