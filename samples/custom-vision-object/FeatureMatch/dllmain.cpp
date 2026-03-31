/**
* @file dllmain.cpp
* @brief RC+80 Custom Vision Object FeatureMatch
* Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
*/

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "RCPlus80CustomVisionObject.h"
#include "opencv2/opencv.hpp"
#include "opencv2/features2d.hpp"
#include <cmath>

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
    strcpy_s(profile->Name, CVO_VALUE_MAX_LENGTH, "FeatureMatch");
    strcpy_s(profile->NamePrefix, CVO_VALUE_MAX_LENGTH, "FeatureM");
    strcpy_s(profile->IconFileName, CVO_VALUE_MAX_LENGTH, "cvo.ico");
    profile->GrayscaleOnly = true;
    profile->UpdateImage = false;
    profile->DetectItems = true;
    profile->HasTeach = true;
    profile->SaveModelFileInProjectFolder = true;
    strcpy_s(profile->ResultProperties, CVO_VALUE_MAX_LENGTH, "Angle,Scale");

    return CVO_NOERROR;
}

CVOAPI int32_t CVOTeach(CVO_IMG* img, CVO_MODEL* mdl)
{
    int32_t width = img->Width;
    int32_t height = img->Height;
    int32_t stride = img->Stride;

    mdl->Size = 4 + 4 + 4 + stride * height;
    memcpy(mdl->pBuffer + 0, &width, 4);
    memcpy(mdl->pBuffer + 4, &height, 4);
    memcpy(mdl->pBuffer + 8, &stride, 4);
    memcpy(mdl->pBuffer + 12, img->pBuffer, stride * height);

    return CVO_NOERROR;
}

CVOAPI int32_t CVORun(CVO_IN_PARAMS* inParams, CVO_OUT_PARAMS* outParams, CVO_DETECT_ITEMS* detectItems, CVO_IMG* img, CVO_MODEL* mdl)
{
    if (mdl->Size <= 0)
    {
        return 0;
    }
    
    const int MaxNum = 20;
    auto numberToFind = inParams->NumberToFind;

    if (numberToFind == 0 || numberToFind > MaxNum)
    {
        numberToFind = MaxNum;
    }

    int32_t mdlWidth;
    int32_t mdlHeight;
    int32_t mdlStride;

    memcpy(&mdlWidth, mdl->pBuffer + 0, 4);
    memcpy(&mdlHeight, mdl->pBuffer + 4, 4);
    memcpy(&mdlStride, mdl->pBuffer + 8, 4);

    if (mdlWidth > img->Width || mdlHeight > img->Height) {
        return 0;
    }

    if (img->BytesPerPixel != 1) {
        return 0;
    }

    unsigned char* mdlBuf = mdl->pBuffer + 12;

    cv::Mat source_image(img->Height, img->Width, CV_8UC1, img->pBuffer);
    cv::Mat template_image(mdlHeight, mdlWidth, CV_8UC1, mdlBuf);

    try {
        std::vector<cv::KeyPoint> source_keypoints;
        std::vector<cv::KeyPoint> template_keypoints;
        cv::Mat source_descriptors;
        cv::Mat template_descriptors;

        cv::Ptr<cv::AKAZE> akaze = cv::AKAZE::create();

        akaze->detectAndCompute(source_image, cv::noArray(), source_keypoints, source_descriptors);
        akaze->detectAndCompute(template_image, cv::noArray(), template_keypoints, template_descriptors);

        for (auto i = 0; i < numberToFind; i++)
        {
            cv::Mat tmp;
            akaze->compute(source_image, source_keypoints, tmp);

            cv::BFMatcher matcher(cv::NORM_HAMMING);
            std::vector<std::vector<cv::DMatch>> nn_matches;

            matcher.knnMatch(template_descriptors, tmp, nn_matches, 2);

            const float ratio_thresh = 0.5f;
            std::vector<cv::DMatch> good_matches;

            for (size_t i = 0; i < nn_matches.size(); i++)
            {
                if (nn_matches[i].size() > 1 && nn_matches[i][0].distance < ratio_thresh * nn_matches[i][1].distance)
                {
                    good_matches.push_back(nn_matches[i][0]);
                }
            }

            std::vector<cv::Point2f> obj;
            std::vector<cv::Point2f> scene;

            for (int i = 0; i < good_matches.size(); i++)
            {
                obj.push_back(template_keypoints[good_matches[i].queryIdx].pt);
                scene.push_back(source_keypoints[good_matches[i].trainIdx].pt);
            }

            if (good_matches.size() > 2) {

                cv::Mat H = cv::findHomography(obj, scene, cv::RANSAC);

                if (!H.empty()) {
                    std::vector<cv::Point2f> obj_corners(4);
                    obj_corners[0] = cv::Point2f(0.0, 0.0);
                    obj_corners[1] = cv::Point2f((float)template_image.cols, 0.0);
                    obj_corners[2] = cv::Point2f((float)template_image.cols, (float)template_image.rows);
                    obj_corners[3] = cv::Point2f(0.0, (float)template_image.rows);

                    std::vector<cv::Point2f> scene_corners(4);
                    cv::perspectiveTransform(obj_corners, scene_corners, H);

                    int cx = (int)((scene_corners[0].x + scene_corners[2].x) / 2);
                    int cy = (int)((scene_corners[0].y + scene_corners[2].y) / 2);
                    int hfw = mdlWidth / 2;
                    int hfh = mdlHeight / 2;

                    double targetWidth = sqrt(pow(scene_corners[0].x - scene_corners[1].x, 2) + pow(scene_corners[0].y - scene_corners[1].y, 2));
                    double targetHeight = sqrt(pow(scene_corners[1].x - scene_corners[2].x, 2) + pow(scene_corners[1].y - scene_corners[2].y, 2));

                    if (detectItems->Count < numberToFind)
                    {
                        detectItems->Items[detectItems->Count].Angle = -(atan2(scene_corners[1].y - scene_corners[0].y, scene_corners[1].x - scene_corners[0].x) * 180 / 3.1415);
                        detectItems->Items[detectItems->Count].PixelX = cx;
                        detectItems->Items[detectItems->Count].PixelY = cy;
                        detectItems->Items[detectItems->Count].Scale = (targetWidth * targetHeight) / (mdlWidth * mdlHeight);

                        detectItems->Count++;
                    }

                    auto it = source_keypoints.begin();
                    while (it != source_keypoints.end())
                    {
                        auto pt = (*it).pt;

                        if ((cx - hfw) < pt.x && pt.x < (cx + hfw) && (cy - hfh) < pt.y && pt.y < (cy + hfh))
                        {
                            it = source_keypoints.erase(it);
                        }
                        else
                        {
                            it++;
                        }
                    }
                }
            }
        }
    }
    catch (const std::exception& e) {
        std::cout << e.what() << std::endl;
    }

    return CVO_NOERROR;
}
