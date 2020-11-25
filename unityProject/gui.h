/**
 *
 * \brief This is library contains all functions relationed with User Interface
 *
 *
 *        All functionality implemented with Qt OpenCV GUI.
 *
 * \note This library can be extended in the future.
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

#ifndef GUI_H
#define GUI_H
#define KEYLEFT 65361
#define KEYRIGHT 65363
#define KEYENTER 13
#define KEYESCAPE 27
#define KEYUP 65362
#define KEYDOWN 65364
#define WAITMSTOREFRESH 15
#define PRUEBA 301
#include "libraries.h"
#include "conexion.h"
#include "varios.h"
#include "camara.h"

using namespace std;


Camara* insertCameraWithPlane(MiniMapInfo* imagesMap, char * image, int sock);
int showMiniMap(MiniMapInfo * imagesMap);
int showCameraScheme(MiniMapInfo* imagesMap);

#endif
