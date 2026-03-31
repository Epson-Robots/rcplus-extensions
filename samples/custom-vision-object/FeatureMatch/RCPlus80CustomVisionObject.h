/**
* @file RCPlus80CustomVisionObject.h
* @brief RC+80 Custom Vision Object Interface
* Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
*/

#ifndef __CUSTOMVISIONOBJECT_H__
#define __CUSTOMVISIONOBJECT_H__

#ifdef WIN32
#include "stdint.h"
#else
#include <stdint.h>
#endif

/**
* @def CVO_API_VERSION
* @brief Custom Vision Object API version
*/
#define CVO_API_VERSION 2

/**
* @def CVO_VALUE_MAX_LENGTH
* @brief Value max length
*/
#define CVO_VALUE_MAX_LENGTH 260

/**
* @def CVO_DETECT_ITEM_MAX
* @brief Detect item max count
*/
#define CVO_DETECT_ITEM_MAX 1000

/**
* @def CVO_MODEL_BUFFER_MAX
* @brief Model buffer max size
*/
#define CVO_MODEL_BUFFER_MAX 10000000

#ifdef __cplusplus
extern "C" {
#endif

#ifndef CVOAPI
/**
* @def CVOAPI
* @brief Please add "CVOAPI=__declspec(dllexport)" to preprocessor definitinon
*/
#define CVOAPI
#endif

#pragma pack(push, 4)

/**
* @struct CVO_PROFILE
* @brief Custom Vision Object profile data
*/
typedef struct _CVO_PROFILE {
	/** Name of custom vision object  (Maximum 16 characters) */
	char Name[CVO_VALUE_MAX_LENGTH];
	/** Prefix name when create object  (Maximum 14 characters) */
	char NamePrefix[CVO_VALUE_MAX_LENGTH];
	/** Icon file name used in vision guide */
	char IconFileName[CVO_VALUE_MAX_LENGTH];
	/** Whether color target is only grayscale */
	bool GrayscaleOnly;
	/** Whether update image when run */
	bool UpdateImage;
	/** Whether detect items when run */
	bool DetectItems;
	/** Has teach or not */
	bool HasTeach;
	/** Save model file in project Folder */
	bool SaveModelFileInProjectFolder;
	/** Properties used by this Custom Vision Object (Set the name separated by commas) */
	char Properties[CVO_VALUE_MAX_LENGTH];
	/** Resutlt properties used by this Custom Vision Object (Set the name separated by commas) */
	char ResultProperties[CVO_VALUE_MAX_LENGTH];
} CVO_PROFILE;

/**
* @struct CVO_IMG
* @brief Image data
*/
typedef struct _CVO_IMG {
	/** Image width */
	int32_t Width;
	/** Image height */
	int32_t Height;
	/** Image row stride byte size */
	int32_t Stride;
	/** Image buffer */
	uint8_t* pBuffer;
	/** Bytes per pixel
	- 1:GrayScale
	- 3:Color (Byte order is BGR) */
	uint32_t BytesPerPixel;
} CVO_IMG;

/**
* @struct CVO_MODEL
* @brief Model data
*/
typedef struct _CVO_MODEL {
	/** Model data size */
	int32_t Size;
	/** Model data buffer */
	uint8_t* pBuffer;
} CVO_MODEL;

/**
* @struct CVO_IN_PARAMS
* @brief In parameters
*/
typedef struct _CVO_IN_PARAMS {
	/** Accept */
	int32_t Accept;
	/** Description */
	char Description[CVO_VALUE_MAX_LENGTH];
	/** NumberToFind (0 means All) */
	int32_t NumberToFind;
	/** ThresholdHigh */
	int32_t ThresholdHigh;
	/** ThresholdLow */
	int32_t ThresholdLow;
} CVO_IN_PARAMS;

/**
* @struct CVO_OUT_PARAMS
* @brief Out parameters
*/
typedef struct _CVO_OUT_PARAMS {
	/** Text */
	char Text[CVO_VALUE_MAX_LENGTH];
} CVO_OUT_PARAMS;

/**
* @struct CVO_DETECT_ITEM
* @brief Detect item data
*/
typedef struct _CVO_DETECT_ITEM {
	/** Detect item location angle */
	double Angle;
	/** Detect item location X */
	int32_t PixelX;
	/** Detect item location Y */
	int32_t PixelY;
	/** Detect item location minimum X */
	int32_t MinX;
	/** Detect item location minimum Y */
	int32_t MinY;
	/** Detect item location maximum X */
	int32_t MaxX;
	/** Detect item location maximum Y */
	int32_t MaxY;
	/** Detect item scale */
	double Scale;
	/** Detect item score */
	int32_t Score;
	/** Detect item text */
	char Text[CVO_VALUE_MAX_LENGTH];
} CVO_DETECT_ITEM;

/**
* @struct CVO_DETECT_ITEMS
* @brief Detect items data
*/
typedef struct _CVO_DETECT_ITEMS {
	/** Detect items count */
	int32_t Count;
	/** Detect item data array */
	CVO_DETECT_ITEM Items[CVO_DETECT_ITEM_MAX];
} CVO_DETECT_ITEMS;


#pragma pack(pop)

/**
* @fn
* CVOGetAPIVersion
* @brief Get custom vision object api version (Please return CVO_API_VERSION)
* @return CVO_API_VERSION
*/
CVOAPI int32_t CVOGetAPIVersion();

/**
* @fn
* CVOGetProfile
* @brief Get custom vision object profile
* @param profile : Profile data
* @return Return code 
*/
CVOAPI int32_t CVOGetProfile(CVO_PROFILE* profile);

/**
* @fn
* CVOTeach
* @brief Custom vision object teach
* @param img : Model window image data
* @param mdl : Model data
* @return Return code 
*/
CVOAPI int32_t CVOTeach(CVO_IMG* img, CVO_MODEL* mdl);

/**
* @fn
* CVORun
* @brief Custom vision object run
* @param inParams : Input parameters
* @param outParams : Output parameters
* @param detectItems : Detect Items
* @param img : Search window image data
* @param mdl : Model data
* @return Return code 
*/
CVOAPI int32_t CVORun(CVO_IN_PARAMS* inParams, CVO_OUT_PARAMS* outParams, CVO_DETECT_ITEMS* detectItems, CVO_IMG* img, CVO_MODEL* mdl);


///////////////////////////////////////////////////////////////////////
// error codes
#define CVO_NOERROR						(0)
#define CVO_ERROR						(-1)

#ifdef  __cplusplus
}
#endif

#endif