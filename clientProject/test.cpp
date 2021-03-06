/* Author: Mario Gonzalez
  10-10-2016

  This app is a client that connect to Unity Simulator and
  create a camera you can you can use.

*/

#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include <stdio.h>
#include <iostream>
#include <string>
#include <opencv2/core/core.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/highgui/highgui.hpp>
#include<arpa/inet.h> //inet_addr
#include <fstream>
#include <sys/time.h>
#include "gui.h"
#include "gui.cpp"
#include "menu.h"
#include "varios.h"
#include "conexion.h"
#include "camara.h"
#include "opencv2/objdetect/objdetect.hpp"
using namespace cv;
using namespace std;



//global variables
Camara *cam;
int sock=-1;



int main(int argc, char**argv)
{
    int connfd,n, i;
    socklen_t clilen;
    pid_t     childpid;
    char mesg[8000000];
    Mat image2;
    char entrada[50];
    char message[1000]="prueba\n";
    Mat auxinputarray= Mat(100, 100, CV_8UC3);
    int puerto=8889;
    int formato=0;
    char ipdireccion[]="192.168.75.1";
    int idpadre;
    char prueba[1];
    int mover=1;
    // variable que va a contar cuantas imagenes nos llegan mal
    int numImgErrores=0;
    int tamanio;
    int tamanioActual;
    FILE *log;
    struct timeval t1, t2, tv;
    double elapsedTime=0, elapsedTime2=0;
    double waitTime;
    int baseline=0;
    ostringstream convert;   // stream used for the conversion
    int realFpsServer, realFpsStreaming;
    int flagShowFps=0;
    int cambiarRes=0;

    /*menu*/
    while(1)
    {
        mostrarMenuPrincipal();
        scanf("%s", entrada);
        if(atoi(entrada)==CERRARPROGRAMA)
        {
            borrarCamaraServidor(cam, sock);
            close(connfd);

            return 0;
        }
        else if(atoi(entrada)==CONECTSERVIDOR)
        {
            sock= crearConexion(puerto, argv[1]);
            if (sock == -1)
            {
                printf("Error al crear el socket.\n");
                return 0;
            }
            puts("Conectado\n");

        }
        else if(atoi(entrada)==CREARCAMARA)
        {
            cam=crearCamaraConsole(sock);
            crearCamaraServidor(cam,sock);


        }
        else if(atoi(entrada)==LOADCAMERASFROMFILE)
        {

            FILE *fp;
            char buff[255];
            fp=fopen("./cameras.txt", "r");

            while(fgets(buff, 255, (FILE*)fp)!=NULL)
            {
                if(buff[0]!='/')
                {

                    cam= createCameraFromFile(sock, buff);
                    createCameraExtendedServer(cam, sock);
                }
            }

            fclose(fp);
        }
        else if(atoi(entrada)==CHANGEFRAMERATE)
        {

            printf("\nIntroduzca el nuevo framerate de la cámara: ");
            scanf("%d", &formato);
            cambiarFPSServidor(formato, cam);

        }
        else if(atoi(entrada)==CHANGEFORMAT)
        {

            printf("\nIntroduzca el nuevo formato (0=JPG, 1=PNG): ");
            scanf("%d", &formato);
            cambiarCalidadJPGServidor(formato, cam);

        }
        else if(atoi(entrada)==GETMINIMAP)
        {

            //initialization
            MiniMapInfo* imagesMap = createMiniMapInfo();

            //ask server for minimap info
            if(getMiniMapInfoServer(sock, imagesMap)==-1) return -1;
printf("Estoy aqui3");
            //ask server for minimap image
            if(getMiniMapServer(sock, &mesg[0], sizeof mesg)==-1) return -1;

            cam=insertCameraWithPlane(imagesMap, mesg, sock);

            free(imagesMap);


        }
        else if(atoi(entrada)==STARTSTREAM)
        {
            pid_t pid = fork();

            if (pid == 0)
            {
                // child process
                break;
            }
            else if (pid > 0)
            {
                // parent process
                //nothing
            }
            else
            {
                // fork failed
                //salir programa
            }
        }



    }



    idpadre=getppid();
    realFpsStreaming=cam->fps;
    realFpsServer=cam->fps;
    i=1;

    //Construir el mensaje a enviar
    sprintf(message, "GETFRAME-%s$", cam->nombre);

    //tenemos un log para el registro de errores
    log = freopen("my_log.txt", "w", stderr);

    // Create a window for display.
    namedWindow(cam->nombre, WINDOW_AUTOSIZE );

    //create controls
    createTrackbar("Izquierda/Derecha", cam->nombre, &mover, 2, moverDerechaIzquierdaServidor, cam);
    createTrackbar("Arriba/Abajo", cam->nombre, &mover, 2, moverArribaAbajoServidor, cam);
    createTrackbar("Adelante/Atras", cam->nombre, &mover, 2, moverAdelanteAtrasServidor, cam);
    createTrackbar("RotarDerecha/RotarIzquierda", cam->nombre, &mover, 2, rotarDerechaIzquierdaServidor, cam);
    createTrackbar("RotarArriba/RotarAbajo", cam->nombre, &mover, 2, rotarArribaAbajoServidor, cam);
    createTrackbar("Fps", cam->nombre, &(cam->fps), 30, cambiarFPSServidor, cam);
    createTrackbar("Calidad", cam->nombre, &(cam->qualityjpg), 100, cambiarCalidadJPGServidor, cam);
    createTrackbar("FieldOfView", cam->nombre, &(cam->fieldofview), 2, changeFieldOfViewServer, cam);

    /*manejo de fallo de imagenes*/
    tamanio=contarNumLineas(log);
    tamanioActual=contarNumLineas(log);

    /* Configuracion de timeout para el socket */

    tv.tv_sec = 1; // 1 sec
    tv.tv_usec = 0;
    setsockopt(sock, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv));
    setsockopt(sock, SOL_SOCKET, SO_SNDTIMEO, &tv, sizeof(tv));

    if(flagShowFps==1)
    {
        setActiveGetRealFPS(cam);
    }

    HOGDescriptor hog;
    hog.setSVMDetector(HOGDescriptor::getDefaultPeopleDetector());


    while(1)
    {

        try
        {


            // start timer
            gettimeofday(&t1, NULL);


            if(flagShowFps==1)
            {
                //ask once per 20 frames
                if(i==20)
                {
                    flushSocket(cam);
                    //ask for realtime framerate
                    realFpsServer=getCameraRealTimeFps(cam);


                }

            }         //Send get frame command
            if( send(sock , message , strlen(message) , 0) < 0)
            {
                puts("Send failed");
                return 1;
            }

            //Receive data from the server
            int length= recv(sock , mesg , sizeof mesg , 0);
            if(  length< 0)
            {
                puts("recv failed");
                break;
            }





            std::string s( base64_decode(mesg) );
            vector <uchar> prueba( s.begin(), s.end() );

            cv::Mat mImg = cv::imdecode( cv::Mat(prueba), cv::IMREAD_COLOR); // Read the image

            /*Voy a comprobar si la imagen es invalidad*/
            tamanioActual=contarNumLineas(log);
            if(tamanioActual<=tamanio && !mImg.empty())
            {
                //flip because it's invert
                cv::flip(mImg, mImg, 0);

                //draw fps
                if(flagShowFps==1)
                {
                    // draw text
                    std::string text = static_cast<ostringstream*>( &(ostringstream() << (realFpsServer)) )->str() + '/' +static_cast<ostringstream*>( &(ostringstream() << (realFpsStreaming)) )->str();
                    //Size textSize = getTextSize(text, FONT_HERSHEY_SIMPLEX, 1, 1, &baseline);

                    Point textOrg(mImg.cols/20,(mImg.rows-mImg.rows/20));

                    putText(mImg, text, textOrg,  FONT_HERSHEY_SCRIPT_SIMPLEX, 0.6, Scalar(0,255,0));
                }

                //people detection
                /*
                vector<Rect> found, found_filtered;
                hog.detectMultiScale(mImg, found, 0, Size(8,8), Size(32,32), 1.05, 2);

                size_t i, j;
                for (i=0; i<found.size(); i++)
                {
                    Rect r = found[i];
                    for (j=0; j<found.size(); j++)
                        if (j!=i && (r & found[j])==r)
                            break;
                    if (j==found.size())
                        found_filtered.push_back(r);
                }
                for (i=0; i<found_filtered.size(); i++)
                {
                    Rect r = found_filtered[i];
                    r.x += cvRound(r.width*0.1);
                    r.width = cvRound(r.width*0.8);
                    r.y += cvRound(r.height*0.06);
                    r.height = cvRound(r.height*0.9);
                    rectangle(mImg, r.tl(), r.br(), cv::Scalar(0,255,0), 2);
                } /**/

                //show
                imshow(cam->nombre, mImg );
                waitKey(5);

            }
            else
            {
                tamanio=tamanioActual;
            }
        }
        catch( cv::Exception& e )
        {
            printf("ERROR\n");
        }




        // stop timer
        gettimeofday(&t2, NULL);

        // compute the elapsed time in millisec
        elapsedTime = (t2.tv_sec - t1.tv_sec) * 1000.0;      // sec to ms
        elapsedTime += (t2.tv_usec - t1.tv_usec) / 1000.0;   // us to ms

        if(elapsedTime>(1000/cam->fps))
        {
            waitTime=0;
        }
        else
        {
            waitTime= ((1000/cam->fps) - elapsedTime)*1000;
        }

        //for calcule real fps
        if(flagShowFps==1)
        {
            elapsedTime2 += elapsedTime + (waitTime/1000);
            if(i==30)
            {
                realFpsStreaming = 30000/elapsedTime2;

                elapsedTime2=0;
                i=0;
            }
        }

        usleep(waitTime);

        if(getppid()!=idpadre)
        {
            exit(0);
        }
        i++;


    }


    close(connfd);
}






