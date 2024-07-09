#include <sys/stat.h>

#include <iostream>

int main() {
    int current_umask = umask(0);
    printf("Current Umask: %04o\n", current_umask);
}

