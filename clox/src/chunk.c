#include "chunk.h"

#include <stdlib.h>

#include "memory.h"
#include "string.h"
#include "vm.h"

static void initLineArray(LineArray *lineArray) {
    lineArray->lineCapacity = 0;
    lineArray->lineCount = 0;
    lineArray->lines = NULL;
}

void initChunk(Chunk *chunk) {
    chunk->count = 0;
    chunk->capacity = 0;
    chunk->code = NULL;
    chunk->lines = malloc(sizeof(LineArray));
    initLineArray(chunk->lines);
    initValueArray(&chunk->constants);
}

static void addLine(LineArray *array, int line) {
    line = line - 1;
    if (line < 0) {
        exit(1);
    }

    // Grow the array.
    if (line >= array->lineCapacity) {
        int oldLineCapacity = array->lineCapacity;
        if (line == 0) {
            array->lineCapacity = 8;
        } else {
            array->lineCapacity = line * 2;
        }
        array->lines = GROW_ARRAY(int, array->lines, oldLineCapacity, array->lineCapacity);
    }

    // Zero out the unset lines.
    if (line >= array->lineCount) {
        memset(&array->lines[array->lineCount], 0, line - array->lineCount + 1);
        array->lineCount = line + 1;
    }

    array->lines[line]++;
}

void writeChunk(Chunk *chunk, uint8_t byte, int line) {
    if (chunk->capacity < chunk->count + 1) {
        int oldCapacity = chunk->capacity;
        chunk->capacity = GROW_CAPACITY(oldCapacity);
        chunk->code = GROW_ARRAY(uint8_t, chunk->code, oldCapacity, chunk->capacity);
    }

    addLine(chunk->lines, line);

    chunk->code[chunk->count] = byte;
    chunk->count++;
}

static void freeLineArray(LineArray *lineArray) {
    FREE_ARRAY(int, lineArray->lines, lineArray->lineCapacity);
}

void freeChunk(Chunk *chunk) {
    FREE_ARRAY(uint8_t, chunk->code, chunk->capacity);
    freeLineArray(chunk->lines);
    free(chunk->lines);
    freeValueArray(&chunk->constants);
    initChunk(chunk);
}

int addConstant(Chunk *chunk, Value value) {
    push(value);
    writeValueArray(&chunk->constants, value);
    pop();
    return chunk->constants.count - 1;
}

int getLine(LineArray *array, int offset) {
    int sum = 0;
    for (int i = 0; i < array->lineCount; i++) {
        sum += array->lines[i];
        if (sum > offset) {
            return i + 1;
        }
    }
    return 0;
}
