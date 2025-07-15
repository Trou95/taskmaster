#include <iostream>
#include <unistd.h>
#include <signal.h>
#include <cstdlib>

volatile bool keep_running = true;

void signal_handler(int signum) {
    std::cout << "Received signal " << signum << ", preparing to exit gracefully..." << std::endl;
    std::cout.flush();
    
    switch(signum) {
        case SIGTERM:
        case SIGINT:
            keep_running = false;
            break;
#ifdef SIGUSR1
        case SIGUSR1:
            std::cout << "Received SIGUSR1 - continuing operation..." << std::endl;
            break;
#endif
#ifdef SIGUSR2
        case SIGUSR2:
            std::cout << "Received SIGUSR2 - continuing operation..." << std::endl;
            break;
#endif
        default:
            std::cout << "Received unknown signal " << signum << std::endl;
            break;
    }
}

int main() {
    // Set up signal handlers for cross-platform compatibility
    signal(SIGTERM, signal_handler);
    signal(SIGINT, signal_handler);
    
    std::cout << "Signal handler test started. PID: " << getpid() << std::endl;
    std::cout << "Registered signals: SIGTERM(" << SIGTERM << "), SIGINT(" << SIGINT << ")";
    
#ifdef SIGUSR1
    signal(SIGUSR1, signal_handler);
    std::cout << ", SIGUSR1(" << SIGUSR1 << ")";
#else
    std::cout << " (SIGUSR1 not available on this platform)";
#endif

#ifdef SIGUSR2
    signal(SIGUSR2, signal_handler);
    std::cout << ", SIGUSR2(" << SIGUSR2 << ")";
#else
    std::cout << " (SIGUSR2 not available on this platform)";
#endif

    std::cout << std::endl;
    std::cout.flush();
    
    int count = 0;
    while(keep_running) {
        std::cout << "[" << count++ << "] Still running, waiting for signal..." << std::endl;
        std::cout.flush();
        sleep(3);
        
        // Safety exit after 10 iterations for testing
        if(count > 30) {
            std::cout << "Test timeout reached, exiting..." << std::endl;
            break;
        }
    }
    
    std::cout << "Graceful shutdown completed. Exiting with code 0." << std::endl;
    std::cout.flush();
    return 0;
}