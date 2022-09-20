#include "chunk.h"

#include <stdlib.h>

#include "memory.h"
#include "string.h"

void initChunk(Chunk *chunk) {
    chunk->count = 0;
    chunk->capacity = 0;
    chunk->code = NULL;
    chunk->lineCount = 0;
    chunk->lineCapacity = 0;
    chunk->lines = NULL;
    initValueArray(&chunk->constants);
}

static void addLine(Chunk *chunk, int line) {
    line = line - 1;
    if (line < 0) {
        exit(1);
    }

    // Grow the array.
    if (line >= chunk->lineCapacity) {
        int oldLineCapacity = chunk->lineCapacity;
        if (line == 0) {
            chunk->lineCapacity = 8;
        } else {
            chunk->lineCapacity = line * 2;
        }
        chunk->lines = GROW_ARRAY(int, chunk->lines, oldLineCapacity, chunk->lineCapacity);
    }

    // Zero out the unset lines.
    if (line >= chunk->lineCount) {
        memset(&chunk->lines[chunk->lineCount], 0, line - chunk->lineCount + 1);
        chunk->lineCount = line + 1;
    }

    chunk->lines[line]++;
}

void writeChunk(Chunk *chunk, uint8_t byte, int line) {
    if (chunk->capacity < chunk->count + 1) {
        int oldCapacity = chunk->capacity;
        chunk->capacity = GROW_CAPACITY(oldCapacity);
        chunk->code = GROW_ARRAY(uint8_t, chunk->code, oldCapacity, chunk->capacity);
    }

    addLine(chunk, line);

    chunk->code[chunk->count] = byte;
    chunk->count++;
}

void freeChunk(Chunk *chunk) {
    FREE_ARRAY(uint8_t, chunk->code, chunk->capacity);
    FREE_ARRAY(int, chunk->lines, chunk->lineCapacity);
    freeValueArray(&chunk->constants);
    initChunk(chunk);
}

int addConstant(Chunk *chunk, Value value) {
    writeValueArray(&chunk->constants, value);
    return chunk->constants.count - 1;
}

int getLine(Chunk *chunk, int offset) {
    int sum = 0;
    for (int i = 0; i < chunk->lineCount; i++) {
        sum += chunk->lines[i];
        if (sum > offset) {
            return i + 1;
        }
    }
    return 0;
}
