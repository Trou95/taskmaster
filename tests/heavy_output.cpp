#include <iostream>
#include <unistd.h>
#include <string>
#include <cstdlib>

int main() {
    std::string heavy_line = "This is a heavy output test with lots of data: ";
    for(int i = 0; i < 50; i++) {
        heavy_line += "DATA_CHUNK_" + std::to_string(i) + " ";
    }
    
    int count = 0;
    while(true) {
        for(int i = 0; i < 10; i++) {
            std::cout << "[" << count++ << "] " << heavy_line << std::endl;
            std::cerr << "[ERR-" << count << "] Error output for testing stderr redirect" << std::endl;
        }
        std::cout.flush();
        std::cerr.flush();
        usleep(500000); // 0.5 seconds
    }
    
    return 0;
}