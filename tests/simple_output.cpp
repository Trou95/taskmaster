#include <iostream>
#include <unistd.h>
#include <string>
#include <cstdlib>

int main() {
    std::string message = "Simple output test program running";
    int count = 0;
    
    while(true) {
        std::cout << "[" << count++ << "] " << message << std::endl;
        std::cerr << "Error: This is a simulated error message." << std::endl;
        std::cout.flush();
        sleep(2);
    }
    
    return 0;
}