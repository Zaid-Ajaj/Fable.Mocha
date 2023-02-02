namespace Fable.Mocha

open System
open Fable.Core.Testing
open Fable.Core

type FocusState =
    | Normal
    | Pending
    | Focused

type TestCase =
    | SyncTest of string * (unit -> unit) * FocusState
    | AsyncTest of string * Async<unit> * FocusState
    | TestList of string * TestCase list
    | TestListSequential of string * TestCase list

[<AutoOpen>]
module Test =
    let testCase name body = SyncTest(name, body, Normal)
    let ptestCase name body = SyncTest(name, body, Pending)
    let ftestCase name body = SyncTest(name, body, Focused)
    let testCaseAsync name body = AsyncTest(name, body, Normal)
    let ptestCaseAsync name body = AsyncTest(name, body, Pending)
    let ftestCaseAsync name body = AsyncTest(name, body, Focused)
    let testList name tests = TestList(name, tests)
    let testSequenced test =
        match test with
        | SyncTest(name, test, state) -> TestListSequential(name, [ SyncTest(name, test, state) ])
        | AsyncTest(name, test, state) ->  TestListSequential(name, [ AsyncTest(name, test, state) ])
        | TestList(name, tests) -> TestListSequential(name, tests)
        | TestListSequential(name, tests) -> TestListSequential(name, tests)
    
    /// Test case computation expression builder
    type TestCaseBuilder (name: string, focusState: FocusState) =
        member _.Zero () = ()
        member _.Delay fn = fn
        member _.Using (disposable: #IDisposable, fn) = using disposable fn
        member _.While (condition, fn) = while condition() do fn()
        member _.For (sequence, fn) = for i in sequence do fn i
        member _.Combine (fn1, fn2) = fn2(); fn1
        member _.TryFinally (fn, compensation) =
            try fn()
            finally compensation()
        member _.TryWith (fn, catchHandler) =
            try fn()
            with e -> catchHandler e
        member _.Run fn = SyncTest (name, fn, focusState)

    /// Builds a test case
    let inline test name =
        TestCaseBuilder (name, Normal)
    /// Builds a test case that will ignore other unfocused tests
    let inline ftest name =
        TestCaseBuilder (name, Focused)
    /// Builds a test case that will be ignored
    let inline ptest name =
        TestCaseBuilder (name, Pending)

    /// Async test case computation expression builder
    type TestAsyncBuilder (name: string, focusState: FocusState) =
        member _.Zero () = async.Zero ()
        member _.Delay fn = async.Delay fn
        member _.Return x = async.Return x
        member _.ReturnFrom x = async.ReturnFrom x
        member _.Bind (computation, fn) = async.Bind (computation, fn)
        member _.Using (disposable: #IDisposable, fn) = async.Using (disposable, fn)
        member _.While (condition, fn) = async.While (condition, fn)
        member _.For (sequence, fn) = async.For (sequence, fn)
        member _.Combine (fn1, fn2) = async.Combine (fn1, fn2)
        member _.TryFinally (fn, compensation) = async.TryFinally (fn, compensation)
        member _.TryWith (fn, catchHandler) = async.TryWith (fn, catchHandler)
        member _.Run fn = AsyncTest (name, fn, focusState)

    /// Builds an async test case
    let inline testAsync name =
        TestAsyncBuilder (name, Normal)
    /// Builds an async test case that will ignore other unfocused tests
    let inline ftestAsync name =
        TestAsyncBuilder (name, Focused)
    /// Builds an async test case that will be ignored
    let inline ptestAsync name =
        TestAsyncBuilder (name, Pending)

    let failtest msg = failwith msg
    let failtestf fmt msg = failwithf fmt msg

[<RequireQualifiedAccess>]
module Env =
    [<Emit("new Function(\"try {return this===window;}catch(e){ return false;}\")")>]
    let isBrowser : unit -> bool = jsNative
    let insideBrowser = isBrowser()
    [<Emit("typeof WorkerGlobalScope !== 'undefined' && self instanceof WorkerGlobalScope")>]
    let insideWorker :  bool = jsNative

[<RequireQualifiedAccess>]
module Expect =
    let inline equal (actual: 'a) (expected: 'a) msg : unit =
        if actual = expected || not (Env.isBrowser()) then
            Assert.AreEqual(actual, expected, msg)
        else
            let valueType = actual.GetType()
            let primitiveTypes = [ typeof<int>; typeof<bool>; typeof<double>; typeof<string>; typeof<decimal>; typeof<Guid> ]
            let errorMsg =
                if List.contains valueType primitiveTypes then
                    sprintf "<span style='color:black'>Expected:</span> <br /><div style='margin-left:20px; color:crimson'>%s</div><br /><span style='color:black'>Actual:</span> </br ><div style='margin-left:20px;color:crimson'>%s</div><br /><span style='color:black'>Message:</span> </br ><div style='margin-left:20px; color:crimson'>%s</div>" (string expected) (string actual) msg
                else
                    sprintf "<span style='color:black'>Expected:</span> <br /><div style='margin-left:20px; color:crimson'>%A</div><br /><span style='color:black'>Actual:</span> </br ><div style='margin-left:20px;color:crimson'>%A</div><br /><span style='color:black'>Message:</span> </br ><div style='margin-left:20px; color:crimson'>%s</div>" expected actual msg

            raise (Exception(errorMsg))
    let notEqual actual expected msg : unit =
        Assert.NotEqual(actual, expected, msg)
    let private isNull' cond =
        match cond with
        | null -> true
        | _ -> false
    let isNull cond = equal (isNull' cond) true
    let isNotNull cond = notEqual (isNull' cond) true
    let isNotNaN cond msg = if Double.IsNaN cond then failwith msg
    let isNotInfinity cond msg = if Double.IsInfinity cond then failwith msg 
    let isTrue cond = equal cond true
    let isFalse cond = equal cond false
    let isZero cond = equal cond 0
    let isEmpty (x: 'a seq) msg = if not (Seq.isEmpty x) then failwithf "%s. Should be empty." msg
    let pass() = equal true true "The test passed"
    let passWithMsg (message: string) = equal true true message
    let exists (x: 'a seq) (a: 'a -> bool) msg = if not (Seq.exists a x) then failwith msg
    let all (x: 'a seq) (a: 'a -> bool) msg = if not (Seq.forall a x) then failwith msg
    /// Expect the passed sequence not to be empty.
    let isNonEmpty (x: 'a seq) msg = if Seq.isEmpty x then failwithf "%s. Should not be empty." msg
    /// Expects x to be not null nor empty
    let isNotEmpty (x: 'a seq) msg =
        isNotNull x msg
        isNonEmpty x msg
    /// Expects x to be a sequence of length `number`
    let hasLength x number msg = equal (Seq.length x) number (sprintf "%s. Expected %A to have length %i" msg x number)
    /// Expects x to be Result.Ok
    let isOk x message =
        match x with
        | Ok _ -> passWithMsg message
        | Error x' -> failwithf "%s. Expected Ok, was Error(\"%A\")." message x'
    /// Expects the value to be a Result.Ok value and returns it or fails the test
    let wantOk x message =
        match x with
        | Ok x' ->
            passWithMsg message
            x'
        | Error x' -> failwithf "%s. Expected Ok, was Error(\"%A\")." message x'
    let stringContains (subject: string) (substring: string) message =
        if not (subject.Contains(substring))
        then failwithf "%s. Expected subject string '%s' to contain substring '%s'." message subject substring
        else passWithMsg message

    /// Expects x to be Result.Error
    let isError x message =
        match x with
        | Error _ -> passWithMsg message
        | Ok x' -> failwithf "%s. Expected Error _, was Ok(%A)." message x'
    let isSome x message =
        match x with
        | Some _ -> passWithMsg message
        | None -> failwithf "%s. Expected Some _, was None." message
    /// Expects the value to be a Some x value and returns x or fails the test
    let wantSome x message =
        match x with
        | Some x' ->
            passWithMsg message
            x'
        | None -> failwithf "%s. Expected Some _, was None." message
    /// Expects the value to be a Result.Error value and returns it or fails the test
    let wantError (x: Result<'a, 'b>) (message: string) =
        match x with
        | Error value ->
            passWithMsg message
            value
        | Ok value -> failwithf "%s. Expected Error _, was Ok(%A)." message value
    let isNone x message =
        match x with
        | None -> passWithMsg message
        | Some x' -> failwithf "%s. Expected None, was Some(%A)." message x'
    let private throws' f =
        try f ()
            None
        with exn ->
            Some exn
    /// Expects the passed function to throw an exception
    let throws f msg =
        match throws' f with
        | None -> failwithf "%s. Expected f to throw." msg
        | Some _ -> ()
    /// Expects the passed function to throw, then calls `cont` with the exception
    let throwsC f cont =
        match throws' f with
        | None -> failwithf "Expected f to throw."
        | Some exn -> cont exn

    /// Expects the `actual` sequence to contain all elements from `expected`
    /// It doesn't take into account the number of occurrences and the order of elements.
    /// Calling this function will enumerate both sequences; they have to be finite.
    let containsAll (actual : _ seq) (expected : _ seq) message =
        let actualEls, expectedEls = List.ofSeq actual, List.ofSeq expected
        let matchingEls =
            actualEls
            |> List.filter (fun a -> expectedEls |> List.contains a)
        
        let extraEls =
            actualEls
            |> List.filter (fun a -> not (matchingEls |> List.contains a))
        let missingEls =
            expectedEls
            |> List.filter (fun e -> not (matchingEls |> List.contains e))

        if List.isEmpty missingEls then
            ()
        else
            sprintf
                "%s. Sequence `actual` does not contain all `expected` elements. Missing elements from `actual`: %A. Extra elements in `actual`: %A"
                message
                missingEls
                extraEls
            |> failtest

module private Html =
    type Node = {
        Tag: string;
        Attributes: (string * string) list;
        Content: string
        Children: Node list
    }

    type IDocElement = interface end

    [<Emit("document.createElement($0)")>]
    let createElement (tag: string) : IDocElement = jsNative
    [<Emit("$2.setAttribute($0, $1)")>]
    let setAttr (name: string) (value: string) (el: IDocElement) : unit = jsNative
    [<Emit("$0.appendChild($1)")>]
    let appendChild (parent: IDocElement) (child: IDocElement) : unit = jsNative
    [<Emit("document.getElementById($0)")>]
    let findElement (id: string) : IDocElement = jsNative
    [<Emit("document.getElementsByClassName($0).length")>]
    let countElementsByClass (value: string) : int = jsNative
    [<Emit("document.body")>]
    let body : IDocElement = jsNative
    [<Emit("$1.innerHTML = $0")>]
    let setInnerHtml (html: string) (el: IDocElement) : unit = jsNative
    let rec createNode (node: Node) =
        let el = createElement node.Tag
        setInnerHtml node.Content el
        for (attrName, attrValue) in node.Attributes do
            setAttr attrName attrValue el
        for child in node.Children do
            let childElement = createNode child
            appendChild el childElement
        el

    let simpleDiv attrs content = { Tag = "div"; Attributes = attrs; Content = content; Children = [] }
    let div attrs children = { Tag = "div"; Attributes = attrs; Content = ""; Children = children }

module Mocha =
    let [<Global>] private describe (name: string) (f: unit->unit) = jsNative
    let [<Global>] private it (msg: string) (f: unit->unit) = jsNative
    let [<Emit("it.skip($0, $1)")>] private itSkip (msg: string) (f: unit->unit) = jsNative
    let [<Emit("it.only($0, $1)")>] private itOnly (msg: string) (f: unit->unit) = jsNative
    let [<Emit("it($0, $1)")>] private itAsync msg (f: unit -> JS.Promise<unit>) = jsNative
    let [<Emit("it.skip($0, $1)")>] private itSkipAsync msg (f: unit -> JS.Promise<unit>) = jsNative
    let [<Emit("it.only($0, $1)")>] private itOnlyAsync msg (f: unit -> JS.Promise<unit>) = jsNative

    let rec isFocused (test: TestCase ) =
        match test with
        | SyncTest(_,_,Focused) -> true
        | AsyncTest(_,_,Focused) -> true
        | TestList(_,tests) -> List.exists isFocused tests
        | TestListSequential(_, tests) -> List.exists isFocused tests
        | _ -> false

    let private runSyncTestInBrowser name test padding =
        try
            test()
            Html.simpleDiv [
                ("data-test", name)
                ("class", "passed")
                ("style",sprintf "font-size:16px; padding-left:%dpx; color:green" padding)
            ] (sprintf "✔ %s" name)
        with
        | ex ->
            let error : Html.Node = {
                Tag = "div";
                Attributes = [ "style", "font-size:16px;color:red;margin:10px; padding:10px; border: 1px solid red; border-radius: 10px;" ];
                Content = ex.Message;
                Children = []
            }

            Html.div [ ] [
                Html.simpleDiv [
                    ("data-test", name)
                    ("class", "failed");
                    ("style",sprintf "font-size:16px; padding-left:%dpx; color:red" padding)
                ] (sprintf "✘ %s" name)
                error
            ]

    let private runAsyncTestInBrowser name test padding =
        let id = Guid.NewGuid().ToString()
        async {
            do! Async.Sleep 1000
            match! Async.Catch(test) with
            | Choice1Of2 () ->
                let div = Html.findElement id
                Html.setInnerHtml (sprintf "✔ %s" name) div
                Html.setAttr "class" "passed" div
                Html.setAttr "style" (sprintf "font-size:16px; padding-left:%dpx;color:green" padding) div
            | Choice2Of2 err ->
                let div = Html.findElement id
                Html.setInnerHtml (sprintf "✘ %s" name) div
                let error : Html.Node = { Tag = "div"; Attributes = [ "style", "margin:10px; padding:10px; border: 1px solid red; border-radius: 10px" ]; Content = err.Message; Children = [] }
                Html.setAttr "style" (sprintf "font-size:16px; padding-left:%dpx;color:red" padding) div
                Html.setAttr "class" "failed" div
                Html.appendChild div (Html.createNode error)
        } |> Async.StartImmediate
        Html.simpleDiv [
            ("id", id);
            ("data-test", name)
            ("class", "executing");
            ("style",sprintf "font-size:16px; padding-left:%dpx;color:gray" padding)
        ] (sprintf "⏳ %s" name)

    let private runAsyncSequentialTestInBrowser name test padding =
        let id = Guid.NewGuid().ToString()
        async {
            do! Async.Sleep 1000
            match! Async.Catch(test) with
            | Choice1Of2 () ->
                let div = Html.findElement id
                Html.setInnerHtml (sprintf "✔ %s" name) div
                Html.setAttr "class" "passed" div
                Html.setAttr "style" (sprintf "font-size:16px; padding-left:%dpx;color:green" padding) div
            | Choice2Of2 err ->
                let div = Html.findElement id
                Html.setInnerHtml (sprintf "✘ %s" name) div
                let error : Html.Node = { Tag = "div"; Attributes = [ "style", "margin:10px; padding:10px; border: 1px solid red; border-radius: 10px" ]; Content = err.Message; Children = [] }
                Html.setAttr "style" (sprintf "font-size:16px; padding-left:%dpx;color:red" padding) div
                Html.setAttr "class" "failed" div
                Html.appendChild div (Html.createNode error)
        },
        Html.simpleDiv [
            ("id", id);
            ("data-test", name)
            ("class", "executing");
            ("style",sprintf "font-size:16px; padding-left:%dpx;color:gray" padding)
        ] (sprintf "⏳ %s" name)

    let rec private flattenTests lastName = function
        | SyncTest(name, test, state) ->
            let modifiedName = if String.IsNullOrWhiteSpace lastName then name else sprintf "%s - %s" lastName name
            [ SyncTest(modifiedName, test, state) ]

        | AsyncTest(name, test, state) ->
            let modifiedName = if String.IsNullOrWhiteSpace lastName then name else sprintf "%s - %s" lastName name
            [ AsyncTest(modifiedName, test, state) ]

        | TestList (name, tests) ->
            [ for test in tests do yield! flattenTests name test ]

        | TestListSequential (name, tests) ->
            [ for test in tests do yield! flattenTests name test ]

    let rec private renderBrowserTests (hasFocusedTests : bool) (tests: TestCase list) (padding: int) : Html.Node list =
        tests
        |> List.map(function
            | SyncTest (name, test, focus) ->
                match focus with
                | Normal when hasFocusedTests ->
                    Html.simpleDiv [
                        ("class", "pending")
                        ("data-test", name)
                        ("style",sprintf "font-size:16px; padding-left:%dpx; color:#B8860B" padding)
                    ] (sprintf "🚧 skipping '%s' due to other focused tests" name)
                | Normal ->
                    runSyncTestInBrowser name test padding
                | Pending ->
                    Html.simpleDiv [
                        ("class", "pending")
                        ("data-test", name)
                        ("style",sprintf "font-size:16px; padding-left:%dpx; color:#B8860B" padding)
                    ] (sprintf "🚧 skipping '%s' due to it being marked as pending" name)
                | Focused ->
                    runSyncTestInBrowser name test padding

            | AsyncTest (name, test, focus) ->
                match focus with
                | Normal when hasFocusedTests ->
                    Html.simpleDiv [
                        ("class", "pending")
                        ("data-test", name)
                        ("style",sprintf "font-size:16px; padding-left:%dpx; color:#B8860B" padding)
                    ] (sprintf "🚧 skipping '%s' due to other focused tests" name)
                | Normal ->
                    runAsyncTestInBrowser name test padding
                | Pending ->
                    Html.simpleDiv [
                        ("class", "pending")
                        ("data-test", name)
                        ("style",sprintf "font-size:16px; padding-left:%dpx; color:#B8860B" padding)
                    ] (sprintf "🚧 skipping '%s' due to it being marked as pending" name)
                | Focused ->
                    runAsyncTestInBrowser name test padding
            | TestList (name, testCases) ->
                let tests = Html.div [] (renderBrowserTests hasFocusedTests testCases (padding + 10))
                let header : Html.Node = {
                    Tag = "div";
                    Attributes = [
                        ("class", "module")
                        ("data-module", name)
                        ("style", sprintf "font-size:20px; padding:%dpx" padding) ];
                    Content = name;
                    Children = [ tests ] }
                Html.div [ ] [ header ]

            | TestListSequential (name, testCases) ->
                let xs =
                    flattenTests "" (TestListSequential ("", testCases))
                    |> List.choose (function
                        | SyncTest (testName, actualTest, focusedState) ->
                            match focusedState with
                            | Normal when hasFocusedTests ->
                                let op = async { do! Async.Sleep 10 }
                                let result =
                                    op, Html.simpleDiv [
                                        ("class", "pending")
                                        ("data-test", name)
                                        ("style",sprintf "font-size:16px; padding-left:%dpx; color:#B8860B" padding)
                                    ] (sprintf "🚧 skipping '%s' due to other focused tests" name)

                                Some result

                            | Pending ->
                                let op = async { do! Async.Sleep 10 }
                                let result =
                                    op, Html.simpleDiv [
                                        ("class", "pending")
                                        ("data-test", name)
                                        ("style",sprintf "font-size:16px; padding-left:%dpx; color:#B8860B" padding)
                                    ] (sprintf "🚧 skipping '%s' due to it being marked as pending" name)
                                Some result
                            | Focused
                            | Normal ->
                                let operation = async {
                                    do! Async.Sleep 10
                                    do actualTest()
                                }

                                Some (runAsyncSequentialTestInBrowser testName operation (padding + 10))

                        | AsyncTest (testName, actualTest, focusedState) ->
                            match focusedState with
                            | Normal when hasFocusedTests ->
                                let op = async { do! Async.Sleep 10 }
                                let result =
                                    op, Html.simpleDiv [
                                        ("class", "pending")
                                        ("data-test", name)
                                        ("style",sprintf "font-size:16px; padding-left:%dpx; color:#B8860B" padding)
                                    ] (sprintf "🚧 skipping '%s' due to other focused tests" name)

                                Some result

                            | Pending ->
                                let op = async { do! Async.Sleep 10 }
                                let result =
                                    op, Html.simpleDiv [
                                        ("class", "pending")
                                        ("data-test", name)
                                        ("style",sprintf "font-size:16px; padding-left:%dpx; color:#B8860B" padding)
                                    ] (sprintf "🚧 skipping '%s' due to it being marked as pending" name)
                                Some result
                            | Focused
                            | Normal ->
                                Some (runAsyncSequentialTestInBrowser testName actualTest (padding + 10))
                        | _ ->
                            None)

                let nodes = List.map snd xs

                let tests = Html.div [] nodes
                let header : Html.Node = {
                    Tag = "div";
                    Attributes = [
                        ("class", "module")
                        ("data-module", name)
                        ("style", sprintf "font-size:20px; padding:%dpx" padding) ];
                    Content = name;
                    Children = [ tests ] }

                let asyncOps = List.map fst xs
                async {
                    for op in asyncOps do
                        let! _ = op
                        ()

                    return ()
                }

                |> Async.Ignore
                |> Async.StartImmediate

                Html.div [ ] [ header ])

    let private configureAsyncTest (test: Async<unit>) =
        fun () -> test |> Async.StartAsPromise

    let rec invalidateTestResults() =
        async {
            let passedCount = Html.countElementsByClass "passed"
            let failedCount = Html.countElementsByClass "failed"
            let executingCount = Html.countElementsByClass "executing"
            let skippedCount = Html.countElementsByClass "pending"
            let total = passedCount + failedCount + executingCount + skippedCount
            Html.setInnerHtml (sprintf "Test Results (%d total)" total) (Html.findElement "total-tests")
            Html.setInnerHtml (sprintf "✔ %d passed" passedCount) (Html.findElement "passed-tests")
            Html.setInnerHtml (sprintf "✘ %d failed" failedCount) (Html.findElement "failed-tests")
            Html.setInnerHtml (sprintf "⏳ %d being executed (async)" executingCount) (Html.findElement "executing-tests")
            Html.setInnerHtml (sprintf "🚧 %d pending" skippedCount) (Html.findElement "skipped-tests")
            if executingCount > 0 then
                do! Async.Sleep 50
                do invalidateTestResults()
            else
                return ()
        }
        |> Async.StartImmediate

    let rec private runViaMocha (test: TestCase) =
        match test with
            | SyncTest (msg, test, focus) ->
                match focus with
                | Normal -> it msg test
                | Pending -> itSkip msg test
                | Focused -> itOnly msg test

            | AsyncTest (msg, test, focus) ->
                match focus with
                | Normal -> itAsync msg (configureAsyncTest test)
                | Pending -> itSkipAsync msg (configureAsyncTest test)
                | Focused -> itOnlyAsync msg (configureAsyncTest test)

            | TestList (name, testCases) ->
                describe name <| fun () ->
                    testCases
                    |> List.iter (runViaMocha)

            | TestListSequential (name, testCases) ->
                describe name <| fun () ->
                    testCases
                    |> List.iter (runViaMocha)

    let runViaDotnet (test: TestCase) =
        raise (NotImplementedException("Currently not implemented, use Expecto for now."))
        1

    let rec runTests (test: TestCase) : int=
        #if FABLE_COMPILER
        if Env.insideBrowser || Env.insideWorker then
            let hasFocusedTests = isFocused test
            let renderedTests = renderBrowserTests hasFocusedTests [test] 0
            let testResults =
                Html.div [ ("style", "margin-bottom: 20px") ] [
                    Html.simpleDiv [ ("id", "total-tests"); ("style", "font-size:20px; margin-bottom:5px") ] "Test Results"
                    Html.simpleDiv [ ("id", "passed-tests"); ("style", "color:green; margin-left:5px;") ] "Passed"
                    Html.simpleDiv [ ("id", "skipped-tests"); ("style", "color:#B8860B") ] "Pending"
                    Html.simpleDiv [ ("id", "failed-tests"); ("style", "color:red;margin-left:5px") ] "Failed"
                    Html.simpleDiv [ ("id", "executing-tests"); ("style", "color:gray;margin-left:5px") ] "Executing"
                ]

            let container = Html.div [ ("style", "padding:20px;") ] [ yield testResults; yield! renderedTests ]
            let element = Html.createNode container
            Html.appendChild Html.body element
            invalidateTestResults()
            0 // Shouldn't return error codes for this path
        else
            runViaMocha test
            0 // Shouldn't return error codes for this path
        #else
        runViaDotnet test
        #endif
