module Tests 

open Fable.Mocha

let mochaTests = 
    testList "Mocha framework tests" [
        testCase "testCase works" <| fun () -> 
            Expect.areEqual (1 + 1) 2
   
        testCase "isFalse works" <| fun () -> 
            Expect.isFalse (1 = 2)

        testCaseAsync "testCaseAsync works" <| fun () ->
            async {
                let! x = async { return 21 }
                let answer = x * 2
                Expect.areEqual 42 answer
            }
    ]

let secondModuleTests = 
    testList "secondModuleModule" [
        testCase "module works properly" <| fun _ ->
            let answer = 31415.0
            let pi = answer / 10000.0
            Expect.areEqual 3.1415 pi 
    ]

Mocha.runTests [ mochaTests; secondModuleTests ]