#include <iostream>
#include <unistd.h>
#include <cstdlib>
#include <random>

int main() {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> dis(1, 5);
    
    int failure_mode = dis(gen);
    
    switch(failure_mode) {
        case 1:
            std::cerr << "FAILURE: Immediate exit with code 1" << std::endl;
            return 1;
        case 2:
            std::cerr << "FAILURE: Immediate exit with code 2" << std::endl;
            return 2;
        case 3:
            std::cerr << "FAILURE: Exit after 3 seconds with code 3" << std::endl;
            sleep(3);
            return 3;
        case 4:
            std::cerr << "FAILURE: Segmentation fault simulation" << std::endl;
            sleep(1);
            return 139; // Simulate segfault exit code
        default:
            std::cerr << "FAILURE: Unknown error code 255" << std::endl;
            return 255;
    }
}