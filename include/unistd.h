#ifndef _UNISTD_H_
#define _UNISTD_H_
#include <io.h>
#include <process.h>
#include <windows.h>
#define usleep(x) Sleep((x) / 1000)
#endif
