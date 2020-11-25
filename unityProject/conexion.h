/**********************************
* Revisar algunas cosas relacionadas con los controles de la camara con JC
* El tema de los ejes XYZ es un lio por ejemplo
**********************************/

/**
 *
 * \brief This library contains all functions we need for communication with VMCS
 *
 * With this library we can send instructions to VMCS and also
 * receive data.
 *
 *
 * \note This library must be extended in the future
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

#ifndef CONEXION_H
#define CONEXION_H
#include "libraries.h"
#include "camara.h"


using namespace std;

/** \brief This function must be the first used. It creates the socket.
 *
 * \param puerto Port number
 * \param ip Ip where VMCS is running
 * \return -1 if socket has not been created
 *
 */
int crearConexion(int puerto, char* ip);

/** \brief This function sends to VMCS the command which creates a camera
 *         with some basic properties specified on cam
 *
 * \param cam pointer to Camera struct that will be used for read properties
 * \param sock socket with connection to VMCS
 * \return -1 if error
 *
 */
int crearCamaraServidor(Camara * cam, int sock);

/** \brief This function sends to VMCS the command which creates a camera
 *         with all properties specified on cam
 *
 * \param cam pointer to Camera struct that will be used for read properties
 * \param sock socket with connection to VMCS
 * \return -1 if error
 *
 */
int createCameraExtendedServer(Camara * cam, int sock);

/** \brief This function sends to VMCS the command which deletes a camera
 *         with the name specified on cam
 *
 * \param pointer to Camera struct that will be used for read camera name
 * \param sock socket with connection to VMCS
 * \return -1 if error
 *
 */
int borrarCamaraServidor(Camara *cam, int sock);

/******* revisar mas adelante *******/
void flushSocket(void*cam);

/** \brief This function receives from VMCS info that will be used for
 *          visual insertion cameras.
 *
 * \param sock socket with connection to VMCS
 * \param info pointer to MiniMapInfo struct
 * \return -1 if error
 *
 */
int getMiniMapInfoServer(int sock, MiniMapInfo * info);

/** \brief This function receives from VMCS an image with the cenital plane
 *         of VMCS's scenario.
 *
 * \param sock socket with connection to VMCS
 * \param info pointer to MiniMapInfo struct
 * \return -1 if error
 *
 */
int getMiniMapServer(int sock, char * buffer, int size);

/*********** Camera properties **********
*   ALL THESE FUNCTION ARE USED BY GUI  *
*****************************************/

/** \brief This function moves a camera in the X axis (left right)
 *
 * \param value 0 if we want to move 1 unit left, 2 if right
 * \param cam pointer to Camera struct
 *
 */
void moverDerechaIzquierdaServidor(int value,void* cam);

/** \brief This function moves a camera in the Y axis (up down)
 *
 * \param value 0 if we want to move 1 unit up, 2 if down
 * \param cam pointer to Camera struct
 *
 */
void moverArribaAbajoServidor(int value,void* cam);

/** \brief This function moves a camera in the z axis (forward back)
 *
 * \param value 0 if we want to move 1 unit ahead, 2 if back
 * \param cam pointer to Camera struct
 *
 */
void moverAdelanteAtrasServidor(int value,void* cam);

/** \brief This function rotates a camera in the X axis (left right)
 *
 * \param value 0 if we want to move 1 unit left, 2 if right
 * \param cam pointer to Camera struct
 *
 */
void rotarDerechaIzquierdaServidor(int value,void* cam);

/** \brief This function rotates a camera in the Y axis (up down)
 *
 * \param value 0 if we want to move 1 unit up, 2 if down
 * \param cam pointer to Camera struct
 *
 */
void rotarArribaAbajoServidor(int value,void* cam);

/** \brief This function changes framerate camera
 *
 * \param value 0 framerate, between 1 and 30
 * \param cam pointer to Camera struct
 *
 */
void cambiarFPSServidor(int value, void* cam);

/** \brief This function changes jpg images receive quality
 *
 * \param value 0 quality, between 1 and 100 (recommended 70-80)
 * \param cam pointer to Camera struct
 *
 */
void cambiarCalidadJPGServidor(int value, void* cam);
/***** revisar en el futuro ***/
void cambiarFormatoServidor(int value, void*cam);

/** \brief VMCS has for each camera a timer which calcules framerate in real time
 *         (real framerate may be differente than configured because perfomance problems).
 *         This function activates the timer. By default it's disabled for better perfomance.
 *
 * \param cam pointer to Camera struct
 *
 */
void setActiveGetRealFPS(void* cam);

/** \brief VMCS has for each camera a timer which calcules framerate in real time
 *         (real framerate may be differente than configured because perfomance problems).
 *         This function disables the timer. By default it's disabled for better perfomance.
 *
 * \param cam pointer to Camera struct
 *
 */
void setInactiveGetRealFPS(void* cam);

/** \brief This function asks VMCS for camera real framerate which is calculed by a timer
 *         in real time. It must be enabled.
 *
 *
 * \param cam pointer to Camera struct
 * \return -1 if error, otherwise the camera framerate
 */
int getCameraRealTimeFps(void* cam);

/** \brief This function changes the field of view of a camera. Field of view is
 *         equivalent to zoom.
 *
 *
 * \param value 0 open field of view, 2 close it
 * \param cam pointer to Camera struct
 *
 */
void changeFieldOfViewServer(int value, void*cam);

/*************** para el final porque esto no va aqui   ****/
//put cameras with map
void OnClickMap(int event, int x, int y, int flags, void* data);
void OnClickHeightMap(int event, int x, int y, int flags, void* data);
#endif
