module Tests


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let mochaTests =
    testList "Mocha framework tests" [

        testCase "testCase works" <| fun () ->
            Expect.equal 2 (1 + 1) "Should be equal"

        testCase "isFalse works" <| fun () ->
            Expect.isFalse (1 = 2) "Should be equal"

        testCase "areEqual with msg" <| fun _ ->
            Expect.equal  1 1 "They are the same"

        testCaseAsync "testCaseAsync works" <|
            async {
                do! Async.Sleep 1000
                let! x = async { return 21 }
                let answer = x * 2
                Expect.equal 42 answer "Should be equal"
            }

        ptestCase "skipping this one" <| fun _ ->
            failwith "Shouldn't be running this test"

        ptestCaseAsync "skipping this one async" <| 
            async {
                failwith "Shouldn't be running this test"
            }
    ]

let secondModuleTests =
    testList "second Module tests" [
        testCase "module works properly" <| fun _ ->
            let answer = 31415.0
            let pi = answer / 10000.0
            Expect.equal 3.1415 pi "Should be equal"
    ]

let structuralEqualityTests =
    testList "testing records" [
        testCase "they are equal" <| fun _ ->
            let expected = {| one = "one"; two = 2 |}
            let actual = {| one = "one"; two = 2 |}
            Expect.equal expected actual "Should be equal"

        testCase "they are not equal" <| fun _ ->
            let expected = {| one = "one"; two = 1 |}
            let actual = {| one = "one"; two = 2 |}
            Expect.notEqual expected actual "Should be equal"
    ]

let nestedTestCase =
    testList "Nested" [
        testList "Nested even more" [
            testCase "Nested test case" <| fun _ -> Expect.isTrue true "Should be true"
        ]
    ]

let focusedTestsCases =
    testList "Focused" [
        ftestCase "Focused sync test" <| fun _ ->
            Expect.equal (1 + 1) 2 "Should be equal"
        ftestCaseAsync "Focused async test" <|
            async {
                Expect.equal (1 + 1) 2 "Should be equal"
            }
    ]

let allTests = [
    mochaTests
    secondModuleTests
    structuralEqualityTests
    nestedTestCase
    // focusedTestsCases
]

#if FABLE_COMPILER
Mocha.runTests allTests
#else
[<EntryPoint>]
let main args =
    testList "All" allTests
    |> runTestsWithArgs defaultConfig args
#endif