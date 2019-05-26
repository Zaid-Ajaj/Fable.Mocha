module Tests

open Fable.Mocha

let mochaTests =
    testList "Mocha framework tests" [

        testCase "testCase works" <| fun () ->
            Expect.areEqual (1 + 1) 2

        testCase "isFalse works" <| fun () ->
            Expect.isFalse (1 = 2)

        testCase "areEqual with msg" <| fun _ ->
            Expect.areEqualWithMsg  1 1 "They are the same"

        testCaseAsync "testCaseAsync works" <| fun () ->
            async {
                do! Async.Sleep 1000
                let! x = async { return 21 }
                let answer = x * 2
                Expect.areEqual 42 answer
            }

        ptestCase "skipping this one" <| fun _ ->
            failwith "Shouldn't be running this test"

        ptestCaseAsync "skipping this one async" <| fun _ ->
            async {
                failwith "Shouldn't be running this test"
            }
    ]

let secondModuleTests =
    testList "second Module tests" [
        testCase "module works properly" <| fun _ ->
            let answer = 31415.0
            let pi = answer / 10000.0
            Expect.areEqual 3.1415 pi
    ]

let structuralEqualityTests =
    testList "testing records" [
        testCase "they are equal" <| fun _ ->
            let expected = {| one = "one"; two = 2 |}
            let actual = {| one = "one"; two = 2 |}
            Expect.areEqual expected actual

        testCase "they are not equal" <| fun _ ->
            let expected = {| one = "one"; two = 1 |}
            let actual = {| one = "one"; two = 2 |}
            Expect.notEqual expected actual
    ]

let nestedTestCase =
    testList "Nested" [
        testList "Nested even more" [
            testCase "Nested test case" <| fun _ -> Expect.isTrue true
        ]
    ]

let focusedTestsCases =
    testList "Focused" [
        ftestCase "Focused sync test" <| fun _ ->
            Expect.areEqual (1 + 1) 2
        ftestCaseAsync "Focused async test" <| fun _ ->
            async {
                Expect.areEqual (1 + 1) 2
            }
    ]

Mocha.runTests [
    mochaTests
    secondModuleTests
    structuralEqualityTests
    nestedTestCase
    // focusedTestsCases
]