# Crafting Interpreters

This repo contains two Lox implementations written while following along the book ["Crafting Interpreters" by Robert Nystom](https://craftinginterpreters.com/).
The first implementation is NLox, a simple tree-walking interpreter written in C# (as opposed to Java which the book uses).
There is also a test project for NLox under `nlox-tests` which contain around ~100 tests for the implementation at various levels.
The second implementation is clox, a bytecode virtual machine written C (same as in the book).

## NLox Extensions

NLox adds some extension to Lox, these include:

* Block comments
* import statements
* List type + syntax sugar for initialization

NLox also does not allow use before initialization of variables.
