#ifndef clox_common_h
#define clox_common_h

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

#include "time.h"

#ifdef DEBUG
#define DEBUG_PRINT_CODE
#define DEBUG_TRACE_EXECUTION
// #define DEBUG_PRINT_TOKEN
#endif

#define NOW() (unsigned long)time(NULL)

#endif
