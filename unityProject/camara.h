/**
 * \class Camera
 *
 *
 * \brief Implements the camera class
 *
 * I follow oriented-object programming. In this class I define
 * Camera struct and MinimapInfo struct which is used for visual
 * insertion cameras. Camera struct is a representation of VMCS
 * camera object. I include some basic methods (create, delete)
 * and others relationated with cameras.
 *
 * \note
 *
 * \author $Author: Mario Gonzalez $
 *
 * \version $Revision: 0.5 $
 *
 * \date $Date: 2016/12/14 $
 *
 * Contact: mario.gonzalez.jim@gmail.com
 *
 */
#ifndef CAMARA_H
#define CAMARA_H
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "libraries.h"

using namespace std;


/** \brief This struct is equivalent to VMCS camera object.
 *
 *  It's used for handle a camera on client-side.
 */

struct Camara
{
    char nombre[50]; ///< camera name, must be unique
    float posicionX; ///< axis X on 3D world space
    float posicionY; ///< axis Y on 3D world space
    float posicionZ; ///< axis Z on 3D world space
    float rotacionX; ///< left and right rotation
    float rotacionY; ///< up and down rotation
    int heightResolution; ///< size in pixels for image frames
    int widthResolution; ///< size in pixels for image frames
    int fps; ///< framerate, between 1 and 30.
    int socket; ///< connection with VMCS
    int qualityjpg; ///< only used for jpg, value between 1 and 100
    int formato; ///< 0=jpg, 1=png, 2=raw
    int activeGetRealFPS; ///< if not -1, VMCS calcules camera framerate in real time ********* revisar
    int fieldofview; ///< revisar esto mas adelante
} ;

/*
*  Dejarlo para mas adelante
*
*/
struct MiniMapInfo
{
    float pointXCamRelative;
    float pointYCamRelative;
    float heightCamRelative;
    cv::Mat *source;
    cv::Mat *icon;
    cv::Mat *dst;
    int angleCam;
    int heightAngleCam;
    bool positionFixed;
    bool heightFixed;
    int lenghtImage;
    float xMinAbsolute;
    float xMaxAbsolute;
    float yMinAbsolute;
    float yMaxAbsolute;
    float floorAbsolute;
    float ceilingAbsolute;

} ;


/** \brief Create a camera and ask user for basic properties (name, fps...) trought consle
 *
 * \param sock this is the VMCS connection
 * \return pointer to camera object generated
 *
 */
Camara* crearCamaraConsole(int sock);

/** \brief Create a camera with default properties
 *
 * \param sock this is the VMCS connection
 * \return pointer to camera object generated
 *
 */
Camara* createCameraDefault(int sock);

/** \brief Deallocates the memory previously allocated by createCamera
 *
 * \param cam pointer to camera we want delete
 *
 */
void deleteCamera(Camara* cam);

/** \brief Allocate memory and inizialite struct
 *
 * \return pointer to MiniMapInfo object generated
 *
 */
MiniMapInfo* createMiniMapInfo();

/** \brief Camera will be created trought /resources/cameras.txt file
 *
 * \param sock this is the VMCS connection
 * \param info char array with one line file read (each line it's one camera)
 * \return  pointer to camera object generated
 *
 */
Camara* createCameraFromFile(int sock, char* info);

/** \brief This function does a conversion between a point selected on local coordinates and point on
 *         world Unity coordinates in the X axis.
 *
 * \param min minimum world Unity coordinate
 * \param max minimum world Unity coordinate
 * \param pointRelative point selected on local coordinates
 * \return
 *
 */
float coordinateXConversion(float min, float max, float pointRelative);

/** \brief This function does a conversion between a point selected on local coordinates and point on
 *         world Unity coordinates in the Y axis.
 *
 * \param min minimum world Unity coordinate
 * \param max minimum world Unity coordinate
 * \param pointRelative point selected on local coordinates
 * \return
 *
 */
float coordinateYConversion(float min, float max, float pointRelative);

/** \brief This function does a conversion between a point selected on local coordinates and point on
 *         world Unity coordinates in the Z axis.
 *
 * \param min minimum world Unity coordinate
 * \param max minimum world Unity coordinate
 * \param pointRelative point selected on local coordinates
 * \return
 *
 */
float coordinateHeightConversion(float floor, float ceiling, float pointRelative);

#endif
