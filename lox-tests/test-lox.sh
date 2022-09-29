#!/bin/bash

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
lox="$1"

if ! command -v "$lox" > /dev/null; then
    echo "Invalid lox $1"
    exit 1
fi

fail=0
success=0
for file in $SCRIPT_DIR/*.lox; do
    expected_out=$(echo $file | sed 's/\.lox$/.out/')
    expected=$(cat "$expected_out")
    actual=$($lox $file 2>&1)
    if [[ "$expected" == "$actual" ]]; then
        echo "SUCCESS - $file"
        ((success=success+1))
    else
        echo "FAIL - $file"
        echo "Expected: '$expected'"
        echo "Actual: '$actual'"
        ((fail=fail+1))
    fi
done
