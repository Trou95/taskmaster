#include <iostream>

using namespace std;
extern char **environ;


int main(void) {
    char **env = environ;

    while (*env) {
        auto str = string(*env);
        
        auto index = str.find("exit_code");
        if(index != string::npos) {
           int exit_code = stoi(str.substr(10));
           exit(exit_code);
        }
        env++;
    }

    return 0;
}
