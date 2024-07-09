#include <unistd.h>

int main()
{
	for(int i = 0; i < 10; i++) {
		write(1, "a", 1);
	}

	usleep(1000 * 1000 * 5);
	
	for(int i = 0; i < 10; i++) {
		write(2, "b", 1);
	}
	return 1;
}
