#include <stdio.h>
#include <unistd.h>
#include <string.h>

int main() {
    char msg[] = "Hello, World!\n";
    while(1) {
        write(STDOUT_FILENO, msg, strlen(msg));
        sleep(1); 
    }
    return 0;
}