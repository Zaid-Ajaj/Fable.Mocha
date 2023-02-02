module Tests

#if EXPECTO
open Expecto
#else
open Fable.Mocha
#endif

let mochaTests =
    testList "Mocha framework tests" [
        testList "Computation expressions" [            
            test "normal test" {
                Expect.isTrue true ""
            }

            testAsync "async test" {
                do! Async.Sleep 300
                Expect.isTrue true ""
            }
        ]

        testSequenced <| testList "Sequential" [
            testCaseAsync "one" <| async {
                do! Async.Sleep 1000
            }

            testCase "sync one" <| fun _ -> Expect.isTrue true "this should work"

            testCaseAsync "two" <| async {
                do! Async.Sleep 1000
            }

            testCase "sync two" <| fun _ -> Expect.isTrue true ""

            testList "Many" [
                testCase "syncThree" <| fun _ -> Expect.isTrue true ""
            ]

            testCaseAsync "three" <| async {
                do! Async.Sleep 1000
                Expect.isTrue true "this should work"
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

        testCase "isEmpty works correctly" <| fun _ ->
            let actual = []
            Expect.isEmpty actual "Should be empty"

        testCase "isEmpty fails correctly" <| fun _ ->
            let case () =
                let actual = [1]
                Expect.isEmpty actual "Should fail"
                Expect.equal true false "Should not be tested"
            let catch (exn: System.Exception) =
                Expect.equal exn.Message "Should fail. Should be empty." "Error messages should be the same"
            Expect.throwsC case catch

        testCase "isNonEmpty works correctly" <| fun _ ->
            let actual = [1]
            Expect.isNonEmpty actual "Should not be empty"

        testCase "isNonEmpty fails correctly" <| fun _ ->
            let case () =
                let actual = []
                Expect.isNonEmpty actual "Should fail"
                Expect.equal true false "Should not be tested"
            let catch (exn: System.Exception) =
                Expect.equal exn.Message "Should fail. Should not be empty." "Error messages should be the same"
            Expect.throwsC case catch
        
        testCase "wantOk works correctly" <| fun _ ->
            let actual = Ok true
            let actual' = Expect.wantOk actual "Should be Ok"
            Expect.equal actual' true "Should be true"

        testCase "wantOk fails correctly" <| fun _ ->
            let case () = 
                let actual = Error true
                Expect.wantOk actual "Should fail"
                Expect.equal true false "Should not be tested"
            let catch (exn: System.Exception) =
                Expect.stringContains exn.Message "Expected Ok" "Error contains the error message"
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
                Expect.stringContains exn.Message "Expected Error" "error message part is present"
            Expect.throwsC case catch

        testCase "wantError works correctly" <| fun _ ->
            let actual = Error true
            let actual' = Expect.wantError actual "Should be Error"
            Expect.equal actual' true "Should be true"

        testCase "wantError fails correctly" <| fun _ ->
            let case () = 
                let actual = Ok true
                Expect.wantError actual "Should fail"
                Expect.equal true false "Should not be tested"
            let catch (exn: System.Exception) =
                Expect.stringContains exn.Message "Expected Error" "Error message contains the correct error"
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
        
        testCase "wantSome works correctly" <| fun _ ->
            let actual = Some true
            let actual' = Expect.wantSome actual "Should be Some"
            Expect.equal actual' true "Should be true"
        
        testCase "wantSome fails correctly" <| fun _ ->
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
        
        testCase "failwith fails correctly" <| fun _ ->
            let case () =
                failwith "Should fail"
            let catch (exn: System.Exception) =
                Expect.equal exn.Message "Should fail" "Error messages should be the same"
            Expect.throwsC case catch
        
        testCase "failwithf fails correctly" <| fun _ ->
            let case () =
                failwithf "%s%s" "Should fail" "!"
            let catch (exn: System.Exception) =
                Expect.equal exn.Message "Should fail!" "Error messages should be the same"
            Expect.throwsC case catch
        
        testCase "isNotInfinity works correctly" <| fun _ ->
            let actual = 20.4
            Expect.isNotInfinity actual "Shouldn't be infinity"
        
        testCase "isNotInfinity fails correctly" <| fun _ ->
            let case () =
                let actual = infinity
                Expect.isNotInfinity actual "Should fail"
            Expect.throws case "Should have failed"
        
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
            Expect.stringContains "Hello, World!" "World" "Should contain string"

        testCase "stringContains fails correctly" <| fun _ ->
            try
                Expect.stringContains "Hello, Mocha!" "World" "Should fail"
                Expect.equal true false "Should not be tested"
            with
            | ex ->
                Expect.equal
                    ex.Message
                    "Should fail. Expected subject string 'Hello, Mocha!' to contain substring 'World'."
                    "Error messages should be the same"

        testList "containsAll" [
            test "identical sequence" {
                Expect.containsAll [|21;37|] [|21;37|] "Identical sequences"
            }

            test "sequence contains all in different order" {
                Expect.containsAll [|21;37|] [|37;21|] "Same elements in different order"
            }
        ]
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

let allTests =
    testList "All" [
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