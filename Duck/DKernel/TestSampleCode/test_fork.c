#include <stdio.h>
#include <unistd.h>
#include <sys/types.h>

int main(void)
{
    pid_t pid = fork();

    if (pid < 0)
    {
        perror("fork failed");
        return 1;
    }
    else if (pid == 0)
    {
        // Child Process
        printf("=== Child Process ===\n");
        printf("PID  : %d\n", getpid());
        printf("PPID : %d\n", getppid());
    }
    else
    {
        // Parent Process
        printf("=== Parent Process ===\n");
        printf("PID  : %d\n", getpid());
        printf("Child PID : %d\n", pid);
    }

    return 0;
}