#!/usr/bin/env bash

set -eu
set -o pipefail

npm test
# npm run dotnet-test
npm run dotnet-expecto