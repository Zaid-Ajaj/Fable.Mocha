module Tests

#if EXPECTO
open Expecto
#else
open Fable.Mocha
#endif

let mochaTests =
    testList "Mocha framework tests" [

        testSequenced <| testList "Sequential" [
            testCaseAsync "one" <| async {
                do! Async.Sleep 1000
            }

            testCase "sync one" <| fun _ -> Expect.isTrue true ""

            testCaseAsync "two" <| async {
                do! Async.Sleep 1000
            }

            testCase "sync two" <| fun _ -> Expect.isTrue true ""

            testList "Many" [
                testCase "syncThree" <| fun _ -> Expect.isTrue true ""
            ]

            testCaseAsync "three" <| async {
                do! Async.Sleep 1000
            }
        ]

        testCase "testCase works with numbers" <| fun () ->
            Expect.equal (1 + 1) 2 "Should be equal"

        testCase "isFalse works" <| fun () ->
            Expect.isFalse (1 = 2) "Should be equal"

        testCase "areEqual with msg" <| fun _ ->
            Expect.equal 2 2 "They are the same"

        testCase "isOk works correctly" <| fun _ ->
            let actual = Ok true
            Expect.isOk actual "Should be Ok"

        testCase "isOk fails correctly" <| fun _ ->
            let case () =
                let actual = Error "fails"
                Expect.isOk actual "Should fail"
                Expect.equal true false "Should not be tested"
            let catch (exn: System.Exception) =
                Expect.equal exn.Message "Should fail. Expected Ok, was Error(\"fails\")." "Error messages should be the same"
            Expect.throwsC case catch
        
        testCase "isError works correctly" <| fun _ ->
            let actual = Error "Is Error"
            Expect.isError actual "Should be Error"
        
        testCase "isError fails correctly" <| fun _ ->
            let case () =
                let actual = Ok true
                Expect.isError actual "Should fail"
                Expect.equal true false "Should not be tested"
            let catch (exn: System.Exception) =
                Expect.equal exn.Message "Should fail. Expected Error _, was Ok(true)." "Error messages should be the same"
            Expect.throwsC case catch
        
        testCase "isSome works correctly" <| fun _ ->
            let actual = Some true
            Expect.isSome actual "Should be Some"
        
        testCase "isSome fails correctly" <| fun _ ->
            let case () = 
                let actual = None
                Expect.isSome actual "Should fail"
                Expect.equal true false "Should not be tested"
            let catch (exn: System.Exception) =
                Expect.equal exn.Message "Should fail. Expected Some _, was None." "Error messages should be the same"
            Expect.throwsC case catch
        
        testCase "isNone works correctly" <| fun _ ->
            let actual = None
            Expect.isNone actual "Should be Some"
        
        testCase "isNone fails correctly" <| fun _ ->
            let case () =
                let actual = Some true
                Expect.isNone actual "Should fail"
                Expect.equal true false "Should not be tested"
            let catch (exn: System.Exception) =
                Expect.equal exn.Message "Should fail. Expected None, was Some(true)." "Error messages should be the same"
            Expect.throwsC case catch
        
        testCase "isNotNull works correctly" <| fun _ ->
            let actual = "not null"
            Expect.isNotNull actual "Should not be null"
        
        testCase "isNull works correctly" <| fun _ ->
            let actual = null
            Expect.isNull actual "Should not be null"
        
        testCase "isNotNaN works correctly" <| fun _ ->
            let actual = 20.4
            Expect.isNotNaN actual "Should not be nan"
        
        testCase "isNotNaN fails correctly" <| fun _ ->
            let case () =
                let actual = nan
                Expect.isNotNaN actual "Should fail"
            Expect.throws case "Should have failed"
        
        testCase "isNotInfinity works correctly" <| fun _ ->
            let actual = 20.4
            Expect.isNotInfinity actual "Shouldn't be infinity"
        
        testCase "isNotInfinity fails correctly" <| fun _ ->
            let case () =
                let actual = infinity
                Expect.isNotInfinity actual "Should fail"
            Expect.throws case "Should have failed"
        
        testCase "isGreaterThan works correctly" <| fun _ ->
            Expect.isGreaterThan 30 20 "Should be greater"
        
        testCase "isGreaterThanOrEquals works correctly" <| fun _ ->
            Expect.isGreaterThanOrEqual 30 20 "Should be greater"
            Expect.isGreaterThanOrEqual 30 30 "Should be equal"
        
        testCase "isLessThan works correctly" <| fun _ ->
            Expect.isLessThan 20 30 "Should be less"
        
        testCase "isLessThanOrEqual works correctly" <| fun _ ->
            Expect.isLessThanOrEqual 20 30 "Should be less"
            Expect.isLessThanOrEqual 30 30 "Should be equal"
        
        testCaseAsync "testCaseAsync works" <|
            async {
                do! Async.Sleep 3000
                let! x = async { return 21 }
                let answer = x * 2
                Expect.equal answer 42 "Should be equal"
            }

        ptestCase "skipping this one" <| fun _ ->
            failwith "Shouldn't be running this test"

        ptestCaseAsync "skipping this one async" <|
            async {
                failwith "Shouldn't be running this test"
            }

        testCase "stringContains works correctly" <| fun _ ->
            let actual = Ok true
            Expect.stringContains "Hello, World!" "World" "Should contain string"

        testCase "stringContains fails correctly" <| fun _ ->
            let actual = Error "fails"
            try
                Expect.stringContains "Hello, Mocha!" "World" "Should fail"
                Expect.equal true false "Should not be tested"
            with
            | ex ->
                Expect.equal
                    ex.Message
                    "Should fail. Expected subject string 'Hello, Mocha!' to contain substring 'World'."
                    "Error messages should be the same"
    ]

let secondModuleTests =
    testList "second Module tests" [
        testCase "module works properly" <| fun _ ->
            let answer = 31415.0
            let pi = answer / 10000.0
            Expect.equal pi 3.1415 "Should be equal"
    ]

let structuralEqualityTests =
    testList "testing records" [
        testCase "they are equal" <| fun _ ->
            let expected = {| one = "one"; two = 2 |}
            let actual = {| one = "one"; two = 2 |}
            Expect.equal actual expected "Should be equal"

        testCase "they are not equal" <| fun _ ->
            let expected = {| one = "one"; two = 1 |}
            let actual = {| one = "one"; two = 2 |}
            Expect.notEqual actual expected "Should be equal"
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

let allTests = testList "All" [
    mochaTests
    secondModuleTests
    structuralEqualityTests
    nestedTestCase
    // focusedTestsCases
]

[<EntryPoint>]
let main args =
#if EXPECTO
    runTestsWithArgs defaultConfig args allTests
#else
    Mocha.runTests allTests
#endif