#!/bin/bash

start=$(date +%s)

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
lox="$1"

if ! command -v "$lox" > /dev/null; then
    echo "Invalid lox $1"
    exit 1
fi

echoG() {
    echo -e "\033[0;32m$@\033[0m"
}

echoR() {
    echo -e "\033[0;31m$@\033[0m"
}

fail=0
success=0
for file in $SCRIPT_DIR/*.lox; do
    expected_out=$(echo $file | sed 's/\.lox$/.out/')
    expected=$(cat "$expected_out")
    actual=$($lox $file 2>&1)
    if [[ "$expected" == "$actual" ]]; then
        echoG "SUCCESS - $file"
        ((success=success+1))
    else
        echoR "FAIL - $file"
        echo "Expected: '$expected'"
        echo "Actual: '$actual'"
        ((fail=fail+1))
    fi
done

now=$(date +%s)
elapsed_s=$(( (now - start) % 86400 ))
echo "Time elapsed: $elapsed_s seconds"

((total=success+fail))
if [[ $fail == 0 ]]; then
    echoG "All $total tests succeeded."
else
    echoR "$fail out of $total tests failed."
fi
