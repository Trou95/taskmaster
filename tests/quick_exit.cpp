#include <iostream>
#include <unistd.h>
#include <cstdlib>

int main() {
    std::cout << "Quick exit test - this program will exit after 5 seconds" << std::endl;
    std::cout << "PID: " << getpid() << std::endl;
    std::cout.flush();
    
    for(int i = 5; i > 0; i--) {
        std::cout << "Exiting in " << i << " seconds..." << std::endl;
        std::cout.flush();
        sleep(1);
    }
    
    std::cout << "Quick exit completed successfully" << std::endl;
    std::cout.flush();
    return 0;
}