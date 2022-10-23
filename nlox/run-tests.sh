#!/bin/bash

dotnet build -c Release

../lox-tests/test-lox.sh ./bin/Release/net6.0/nlox
