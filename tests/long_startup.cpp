#include <iostream>
#include <unistd.h>
#include <cstdlib>

int main() {
    std::cout << "Long startup test - simulating slow initialization" << std::endl;
    std::cout << "PID: " << getpid() << std::endl;
    std::cout.flush();
    
    // Simulate long startup process
    for(int i = 1; i <= 10; i++) {
        std::cout << "Initialization step " << i << "/10..." << std::endl;
        std::cout.flush();
        sleep(1);
    }
    
    std::cout << "Startup completed! Now running normally..." << std::endl;
    std::cout.flush();
    
    // Normal operation
    int count = 0;
    while(true) {
        std::cout << "[" << count++ << "] Long startup process running normally" << std::endl;
        std::cout.flush();
        sleep(3);
    }
    
    return 0;
}