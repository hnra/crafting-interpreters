# Crafting Interpreters

This repo contains two Lox implementations written while following along the book ["Crafting Interpreters" by Robert Nystom](https://craftinginterpreters.com/).
The first implementation is NLox, a simple tree-walking interpreter written in C# (as opposed to Java which the book uses).
There is also a test project for NLox under `nlox-tests` which contain around ~100 tests for the implementation at various levels.

The second implementation is clox, a bytecode virtual machine written in C (same as in the book).
clox does not contain any extensions or modifications, it is identical to the one presented in the book.

## Instructions

Instructions for building and running the tests.

### Clox

Build:

```sh
$ cd clox
$ make [debug]
$ ./build/[release|debug]/clox
```

Run tests:

```sh
$ cd clox
$ ./run-tests.sh
```

### NLox

Build:

```sh
$ dotnet build -C [Debug|Release] nlox/nlox.csproj
$ ./nlox/bin/[Debug|Release]/net6.0/nlox
```

Run tests:

```sh
$ dotnet test nlox-tests/nlox-tests.csproj
$ cd nlox
$ ./run-tests.sh
```

Generate AST:

```sh
$ cd nlox
$ dotnet test --filter GenerateAst ../nlox-tests/nlox-tests.csproj
```

## NLox Extensions

NLox adds some extension to Lox, these include:

* Block comments
* import statements
* List type + syntax sugar for initialization
* Ternary conditional

NLox also does not allow use before initialization of variables.
