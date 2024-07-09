#include <iostream>
#include <cstring>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>

int main() {
    const char* server_ip = "127.0.0.1";
    int server_port = 8080;

    int sock = socket(AF_INET, SOCK_STREAM, 0);
    if (sock < 0) {
        std::cerr << "Socket oluşturulamadı." << std::endl;
        return 1;
    }

    sockaddr_in server_address;
    std::memset(&server_address, 0, sizeof(server_address));
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(server_port);
    if (inet_pton(AF_INET, server_ip, &server_address.sin_addr) <= 0) {
        std::cerr << "Geçersiz adres." << std::endl;
        close(sock);
        return 1;
    }

    if (connect(sock, (sockaddr*)&server_address, sizeof(server_address)) < 0) {
        std::cerr << "Bağlantı hatası." << std::endl;
        close(sock);
        return 1;
    }

    std::cout << "Sunucuya bağlandı." << std::endl;

    while (true) {
        std::string input;
        std::cout << "Sunucuya göndermek için mesaj girin ('q' ile çıkış): ";
        std::getline(std::cin, input);

        if (input == "q") {
            break;
        }

        send(sock, input.c_str(), input.length(), 0);
    }

    close(sock);
    return 0;
}

