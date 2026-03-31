/**
* @file dllmain.cpp
* @brief RC+80 Custom Vision Object AutoContrast
* Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
*/

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "RCPlus80CustomVisionObject.h"

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

CVOAPI int32_t CVOGetAPIVersion()
{
    return CVO_API_VERSION;
}

CVOAPI int32_t CVOGetProfile(CVO_PROFILE* profile)
{
    strcpy_s(profile->Name, CVO_VALUE_MAX_LENGTH, "AutoContrast");
    strcpy_s(profile->NamePrefix, CVO_VALUE_MAX_LENGTH, "AutoCont");
    strcpy_s(profile->IconFileName, CVO_VALUE_MAX_LENGTH, "cvo.ico");
    profile->GrayscaleOnly = false;
    profile->UpdateImage = true;
    profile->DetectItems = false;
    profile->HasTeach = false;
    profile->SaveModelFileInProjectFolder = false;
    strcpy_s(profile->Properties, CVO_VALUE_MAX_LENGTH, "");
    strcpy_s(profile->ResultProperties, CVO_VALUE_MAX_LENGTH, "");

    return CVO_NOERROR;
}

CVOAPI int32_t CVOTeach(CVO_IMG* img, CVO_MODEL* mdl)
{
    return CVO_NOERROR;
}

CVOAPI int32_t CVORun(CVO_IN_PARAMS* inParams, CVO_OUT_PARAMS* outParams, CVO_DETECT_ITEMS* detectItems, CVO_IMG* img, CVO_MODEL* mdl)
{
    long hist[256] = {};
    double lut[256] = {};

    // Create Histogram
    for (int y = 0; y < img->Height; y++)
    {
        for (int x = 0; x < img->Width; x++)
        {
            int idx = img->Stride * y + img->BytesPerPixel * x;

            if (img->BytesPerPixel == 3)
            {
                auto blue = img->pBuffer[idx + 0];
                auto green = img->pBuffer[idx + 1];
                auto red = img->pBuffer[idx + 2];
                double grayscale = 0.2989 * red + 0.5870 * green + 0.1140 * blue;

                if (grayscale > 255)
                {
                    grayscale = 255;
                }

                hist[(unsigned char)grayscale]++;
            }
            else if (img->BytesPerPixel == 1)
            {
                hist[img->pBuffer[idx]]++;
            }
        }
    }

    // Create Lookup Table
    for (int idxColor = 0; idxColor < 256; idxColor++)
    {
        if (idxColor == 0)
        {
            lut[idxColor] = 0;
            continue;
        }

        lut[idxColor] = lut[idxColor - 1] + (double)hist[idxColor] * 256 / (img->Width * img->Height);
    }

    for (int idxColor = 0; idxColor < 256; idxColor++)
    {
        if (lut[idxColor] > 255)
        {
            lut[idxColor] = 255;
        }
    }

    // Convert Color
    for (int y = 0; y < img->Height; y++)
    {
        for (int x = 0; x < img->Width; x++)
        {
            int idx = img->Stride * y + img->BytesPerPixel * x;

            if (img->BytesPerPixel == 3)
            {
                img->pBuffer[idx + 0] = (unsigned char)lut[img->pBuffer[idx + 0]];
                img->pBuffer[idx + 1] = (unsigned char)lut[img->pBuffer[idx + 1]];
                img->pBuffer[idx + 2] = (unsigned char)lut[img->pBuffer[idx + 2]];
            }
            else if (img->BytesPerPixel == 1)
            {
                img->pBuffer[idx] = (unsigned char)lut[img->pBuffer[idx]];
            }
        }
    }

    return CVO_NOERROR;
}
