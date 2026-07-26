#ifndef _MMSYSTEM_H_
#define _MMSYSTEM_H_

#include <windows.h>

#define WAVE_MAPPER ((UINT)-1)

#define CALLBACK_NULL      0x00000000l
#define CALLBACK_WINDOW    0x00010000l
#define CALLBACK_THREAD    0x00020000l
#define CALLBACK_FUNCTION  0x00030000l
#define CALLBACK_EVENT     0x00050000l

#define WHDR_DONE      0x00000001
#define WHDR_PREPARED  0x00000002
#define WHDR_BEGINLOOP 0x00000004
#define WHDR_ENDLOOP   0x00000008
#define WHDR_INQUEUE   0x00000010

#define WAVE_FORMAT_PCM 1
#define MMSYSERR_NOERROR 0

typedef void *HWAVEOUT;
typedef UINT MMRESULT;

#pragma pack(push, 1)
typedef struct waveformat_tag {
    WORD  wFormatTag;
    WORD  nChannels;
    DWORD nSamplesPerSec;
    DWORD nAvgBytesPerSec;
    WORD  nBlockAlign;
    WORD  wBitsPerSample;
    WORD  cbSize;
} WAVEFORMATEX, *PWAVEFORMATEX, *NPWAVEFORMATEX, *LPWAVEFORMATEX;

typedef struct wavehdr_tag {
    LPSTR      lpData;
    DWORD      dwBufferLength;
    DWORD      dwBytesRecorded;
    DWORD_PTR  dwUser;
    DWORD      dwFlags;
    DWORD      dwLoops;
    struct wavehdr_tag *lpNext;
    DWORD_PTR  reserved;
} WAVEHDR, *PWAVEHDR, *NPWAVEHDR, *LPWAVEHDR;
#pragma pack(pop)

#ifdef __cplusplus
extern "C" {
#endif

MMRESULT WINAPI waveOutOpen(HWAVEOUT *lphwo, UINT uDeviceID, const WAVEFORMATEX *pwfx, DWORD_PTR dwCallback, DWORD_PTR dwInstance, DWORD fdwOpen);
MMRESULT WINAPI waveOutClose(HWAVEOUT hwo);
MMRESULT WINAPI waveOutPrepareHeader(HWAVEOUT hwo, WAVEHDR *pwh, UINT cbwh);
MMRESULT WINAPI waveOutUnprepareHeader(HWAVEOUT hwo, WAVEHDR *pwh, UINT cbwh);
MMRESULT WINAPI waveOutWrite(HWAVEOUT hwo, WAVEHDR *pwh, UINT cbwh);
MMRESULT WINAPI waveOutReset(HWAVEOUT hwo);

#ifdef __cplusplus
}
#endif

#endif // _MMSYSTEM_H_
