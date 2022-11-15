#ifndef clox_common_h
#define clox_common_h

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

#include "time.h"

#ifdef DEBUG
#define DEBUG_PRINT_CODE
#define DEBUG_TRACE_EXECUTION
// #define DEBUG_STRESS_GC
// #define DEBUG_LOG_GC
// #define DEBUG_LOG_ALLOC
#define DEBUG_PRINT_TOKEN
#endif

#define UINT8_COUNT (UINT8_MAX + 1)

#define NOW() (unsigned long)time(NULL)

#endif
