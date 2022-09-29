TARGET_EXEC ?= clox

RELEASE_DIR ?= ./build/release
DEBUG_DIR ?= ./build/debug
SRC_DIRS ?= ./src

SRCS := $(shell find $(SRC_DIRS) -name *.c)
RELEASE_OBJS := $(SRCS:%=$(RELEASE_DIR)/%.o)
DEBUG_OBJS := $(SRCS:%=$(DEBUG_DIR)/%.o)
RELEASE_DEPS := $(DEBUG_OBJS:.o=.d)
DEBUG_DEPS := $(RELEASE_OBJS:.o=.d)

INC_DIRS := $(shell find $(SRC_DIRS) -type d)
INC_FLAGS := $(addprefix -I,$(INC_DIRS))

CPPFLAGS ?= $(INC_FLAGS) -MMD -MP
CFLAGS += -Wall -std=c99 -Wextra

release: CFLAGS += -O2
release: $(RELEASE_DIR)/$(TARGET_EXEC)

debug: CFLAGS += -g
debug: CFLAGS += -Og
debug: CFLAGS += -D DEBUG
debug: $(DEBUG_DIR)/$(TARGET_EXEC)

$(RELEASE_DIR)/$(TARGET_EXEC): $(RELEASE_OBJS)
	$(CC) $(RELEASE_OBJS) -o $@ $(LDFLAGS)

$(RELEASE_DIR)/%.c.o: %.c
	$(MKDIR_P) $(dir $@)
	$(CC) $(CPPFLAGS) $(CFLAGS) -c $< -o $@

$(DEBUG_DIR)/$(TARGET_EXEC): $(DEBUG_OBJS)
	$(CC) $(DEBUG_OBJS) -o $@ $(LDFLAGS)

$(DEBUG_DIR)/%.c.o: %.c
	$(MKDIR_P) $(dir $@)
	$(CC) $(CPPFLAGS) $(CFLAGS) -c $< -o $@

.PHONY: clean

clean:
	$(RM) -r $(DEBUG_DIR)
	$(RM) -r $(RELEASE_DIR)

-include $(DEPS)

MKDIR_P ?= mkdir -p
