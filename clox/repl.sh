#!/bin/bash

if [[ "$1" == "debug" ]]; then
    make debug
    ./build/debug/clox
else
    make release
    ./build/release/clox
fi
