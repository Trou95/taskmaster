#include <iostream>
#include <unistd.h>
#include <cstdlib>
#include <string>

int main() {
    std::cout << "Environment Variables Test Program" << std::endl;
    std::cout << "==================================" << std::endl;
    
    // Test specific environment variables
    const char* test_vars[] = {
        "STARTED_BY",
        "ANSWER", 
        "SERVICE_NAME",
        "WORKER_TYPE",
        "RESTART_POLICY",
        "MESSAGE",
        "SERVICE",
        "DURATION", 
        "SERVICE_TYPE",
        "FAIL_RATE",
        "TEST_MODE",
        "INSTANCE_COUNT",
        "PROCESS_TYPE",
        "PATH",
        "HOME",
        "USER"
    };
    
    for(const char* var : test_vars) {
        const char* value = std::getenv(var);
        if(value) {
            std::cout << var << "=" << value << std::endl;
        } else {
            std::cout << var << "=(not set)" << std::endl;
        }
    }
    
    std::cout << "==================================" << std::endl;
    std::cout << "PID: " << getpid() << std::endl;
    std::cout << "PPID: " << getppid() << std::endl;
    
    int count = 0;
    while(count < 10) {
        std::cout << "[" << count++ << "] Environment test running..." << std::endl;
        std::cout.flush();
        sleep(2);
    }
    
    return 0;
}