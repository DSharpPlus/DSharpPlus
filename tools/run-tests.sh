#!/bin/bash

# This script is used by the CI to run only the required tests instead of running all the tests.
set -e

# First we're going to figure out which projects were changed.
# All projects start with the `DSharpPlus` prefix, so we can filter the changed files by that.
CHANGED_FILES="$(git diff --name-only origin/master | grep -E 'DSharpPlus.*\.(cs|csproj)' | grep -v 'DSharpPlus.Tests')"
readarray -t REQUIRED_TESTS <<< "$CHANGED_FILES"

echo "Running tests under these namespaces:"
for ((i=0; i<${#REQUIRED_TESTS[@]}; i++)); do
    # Get the dir name of the file
    REQUIRED_TESTS[$i]="$(dirname "${REQUIRED_TESTS[$i]}")"

    # Replace first occurance of DSharpPlus with DSharpPlus.Tests
    REQUIRED_TESTS[$i]="$(echo "${REQUIRED_TESTS[$i]}" | sed 's/DSharpPlus/DSharpPlus.Tests/')"

    # Replace all `/` with `.`
    REQUIRED_TESTS[$i]="$(echo "${REQUIRED_TESTS[$i]}" | sed 's/\//\./g')"

    echo "- ${REQUIRED_TESTS[$i]}"
done

# Print a newline for readability
echo

# Combine all the namespaces into a single string, putting `--filter "FullyQualifiedName~$namespace"` between each one
REQUIRED_TESTS="$(printf -- '--filter "FullyQualifiedName~%s" ' "${REQUIRED_TESTS[@]}")"

# Run all the required tests with a default timeout of 30 seconds
dotnet test --blame-crash --blame-hang --blame-hang-timeout "30s" $REQUIRED_TESTS