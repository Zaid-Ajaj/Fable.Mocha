### 2.11.0

* Added more options to Expect module

### 2.10.0

* Add Expecto.stringContains and Expecto.passWithMsg

### 2.9.1

* Relax FSharp.Core dependency to >= 4.7.0

### 2.9.0

* Flip order of arguments when checking equality of expected vs actual value

### 2.8.0

* Nicer error messages showing expected and actual values
* Implement `testSequenced` for sequentially processed tests with Expecto compatibility.

### 2.7.0

* Add `Expect.isOk` to match Expecto's API, thanks to @rfrerebe. See #8


### 2.6.0

* Switch actual and expected paramter placement for consistency with Expecto's API, see #7

### 2.5.0

* Add Femto metadata for Fable.Mocha
* Do not re-describe synchrounous tests
* Fix Puppeteer runner on non-windows machines

### 2.4.0

* Add proper data attributes to the browser runner

### 2.3.0

* Unify the API to match that of Expecto by @TheAngryByrd (see #6)

### 2.2.0

* Add test results overview and classes to test elements making them queryable from the document.

### 2.1.0

* Focused States support both for Mocha and browser by @TheAngryByrd

### 2.0.0

* Supporting arbitrary nested test lists and test cases
* Add built-in browser support

### 1.1.0

* Use `Assert.AreEqual` from Fable's testing utilities by @alfonsogarciacaro

### 1.0.0

* Initial stable release