#!/bin/bash

make release

../lox-tests/test-lox.sh ./build/release/clox
