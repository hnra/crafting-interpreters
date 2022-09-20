#include <stdio.h>

#include "chunk.h"
#include "common.h"
#include "debug.h"

int main(int argc, const char *argv[]) {
    Chunk chunk;
    initChunk(&chunk);
    int constant = addConstant(&chunk, 1.2);
    writeChunk(&chunk, OP_RETURN, 1);
    writeChunk(&chunk, OP_RETURN, 2);
    writeChunk(&chunk, OP_CONSTANT, 123);
    writeChunk(&chunk, constant, 123);
    writeChunk(&chunk, OP_RETURN, 123);
    writeChunk(&chunk, OP_RETURN, 500);
    disassembleChunk(&chunk, "test chunk");
    freeChunk(&chunk);
    printf("Size of Chunk: %lu\n", sizeof(Chunk));
    return 0;
}
