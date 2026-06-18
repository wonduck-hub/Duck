#include <stdio.h>

typedef enum {
    TASK_READY,
    TASK_RUNNING,
    TASK_SLEEPING,
    TAKS_ZOMBIE,
} task_state_t;

typedef struct task task_t;
typedef struct task {
    int pid;
    char name[32];
    task_state_t state;
    int priority;
    int time_slice;
    task_t* parent;
    task_t* next;
} task_t;