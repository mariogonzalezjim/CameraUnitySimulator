#include "conexion.h"
#include "varios.h"

int crearConexion(int puerto, char* mensaje)
{
    struct sockaddr_in servaddr,cliaddr;
    int sock= socket(AF_INET,SOCK_STREAM,0);

    if(sock==-1)
        return -1;

    bzero(&servaddr,sizeof(servaddr));
    servaddr.sin_family = AF_INET;
    servaddr.sin_addr.s_addr = inet_addr(mensaje);
    servaddr.sin_port=htons(puerto);

    //Connect to remote server
    if (connect(sock , (struct sockaddr *)&servaddr , sizeof(servaddr)) < 0)
    {
        return -1;
    }

    return sock;
}

int crearCamaraServidor(Camara * cam, int sock)
{

    char mensaje[100];

    sprintf(mensaje, "CREACAMARA-%s-%d-%d-%d-%d-%d$", cam->nombre, cam->fps, cam->widthResolution ,cam->heightResolution, cam->formato, cam->qualityjpg);

    if( send(sock , mensaje , strlen(mensaje) , 0) < 0)
    {
        puts("Send failed");
        return -1;
    }


    return 0;
}


int createCameraExtendedServer(Camara * cam, int sock)
{

    char mensaje[200];

    sprintf(mensaje, "CREATECAMERAEXTENDED-%s/%d/%d/%d/%d/%d/%f/%f/%f/%f/%f$", cam->nombre, cam->fps, cam->widthResolution ,cam->heightResolution, cam->formato, cam->qualityjpg,
            cam->posicionX, cam->posicionY, cam->posicionZ, cam->rotacionX, cam->rotacionY);


    if( send(sock , mensaje , strlen(mensaje) , 0) < 0)
    {
        puts("Send failed");
        return -1;
    }
    printf("Se ha creado una camara con la siguiente informacion:\n%s\n", mensaje);

    return 0;
}

int borrarCamaraServidor(Camara *cam, int sock)
{

    char mensaje[100];

    sprintf(mensaje, "DELETECAMARA-%s$", cam->nombre);

    if( send(sock , mensaje , strlen(mensaje) , 0) < 0)
    {
        puts("Send failed");
        return -1;
    }


    return 0;

}


int getMiniMapInfoServer(int sock, MiniMapInfo * info)
{

   flushSocket(NULL);
    char mensaje[200];
    int i=0;

    sprintf(mensaje, "GETMINIMAPINFO-$");
    if( send(sock , mensaje , strlen(mensaje)  , 0) < 0)
    {
        puts("Send failed");
        return -1;
    }


    //Receive data from the server
    int length= recv(sock , mensaje , sizeof mensaje , 0);
    if( length < 0)
    {
        puts("recv failed");
        return -1;
    }
printf("%s----", mensaje);
    char* chars_array = strtok(mensaje, "/");

    while(chars_array)
    {
        if(i==0)
        {
            //header
            if(std::strcmp(chars_array, "MINIMAPINFO") != 0)
                return -1;
        }
        else if(i==1)
        {
            //lenght image
            info->lenghtImage= std::atoi( chars_array );
        }
        else if(i==2)
        {
            info->xMinAbsolute= strtof( chars_array, NULL );
        }
        else if(i==3)
        {
            info->xMaxAbsolute= strtof( chars_array, NULL );
        }
        else if(i==4)
        {
            info->yMinAbsolute= strtof( chars_array, NULL );
        }
        else if(i==5)
        {
            info->yMaxAbsolute= strtof( chars_array, NULL );
        }
        else if(i==6)
        {
            info->floorAbsolute = strtof( chars_array, NULL );
        }
        else if(i==7)
        {
            info->ceilingAbsolute = strtof( chars_array, NULL );
        }


        chars_array = strtok(NULL, "/");
        i++;
    }

    return 0;
}


int getMiniMapServer(int sock, char  buffer[], int size)
{
    char mensaje[100];

    sprintf(mensaje, "GETMINIMAP-$");
    if( send(sock , mensaje , strlen(mensaje)  , 0) < 0)
    {
        puts("Send failed");
        return -1;
    }

    //wait for 1sec
    usleep(1000000);

    //Receive data from the server
    int length= recv(sock , buffer , size , 0);
    if( length < 0)
    {
        puts("recv failed");
        return -1;
    }


    return 0;
}


void moverDerechaIzquierdaServidor(int value,void* cam)
{

    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    if(value==0)
    {
        cv::setTrackbarPos("Izquierda/Derecha", aux->nombre, 1);
        sprintf(mensaje, "MOVECAMARALEFT-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }
    else if(value==2)
    {
        cv::setTrackbarPos("Izquierda/Derecha", aux->nombre, 1);
        sprintf(mensaje, "MOVECAMARARIGHT-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }

}


void moverArribaAbajoServidor(int value,void* cam)
{

    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    if(value==0)
    {
        cv::setTrackbarPos("Arriba/Abajo", aux->nombre, 1);
        sprintf(mensaje, "MOVECAMARAUP-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;

        }
    }
    else if(value==2)
    {
        cv::setTrackbarPos("Arriba/Abajo", aux->nombre, 1);
        sprintf(mensaje, "MOVECAMARADOWN-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje), 0) < 0)
        {
            puts("Send failed");
            return ;

        }
    }

}

void moverAdelanteAtrasServidor(int value,void* cam)
{

    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    if(value==0)
    {
        cv::setTrackbarPos("Adelante/Atras", aux->nombre, 1);
        sprintf(mensaje, "MOVECAMARAAHEAD-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }
    else if(value==2)
    {
        cv::setTrackbarPos("Adelante/Atras", aux->nombre, 1);
        sprintf(mensaje, "MOVECAMARABACK-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }

    return;
}

void rotarDerechaIzquierdaServidor(int value,void* cam)
{
    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    if(value==0)
    {
        cv::setTrackbarPos("RotarDerecha/RotarIzquierda", aux->nombre, 1);
        sprintf(mensaje, "ROTATECAMARALEFT-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }

    }
    else if(value==2)
    {
        cv::setTrackbarPos("RotarDerecha/RotarIzquierda", aux->nombre, 1);
        sprintf(mensaje, "ROTATECAMARARIGHT-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }


}



void rotarArribaAbajoServidor(int value,void* cam)
{
    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    if(value==0)
    {
        cv::setTrackbarPos("RotarArriba/RotarAbajo", aux->nombre, 1);
        sprintf(mensaje, "ROTATECAMARAUP-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }
    else if(value==2)
    {
        cv::setTrackbarPos("RotarArriba/RotarAbajo", aux->nombre, 1);
        sprintf(mensaje, "ROTATECAMARADOWN-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }

    return;
}

void cambiarFPSServidor(int value, void* cam)
{
    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    aux->fps=value;

    sprintf(mensaje, "CHANGECAMARAFPS-%s-%d$", aux->nombre, value);

    if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
    {
        puts("Send failed");
        return ;
    }

    return;

}


void cambiarCalidadJPGServidor(int value, void* cam)
{
    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    aux->qualityjpg=value;

    sprintf(mensaje, "CHANGECAMARACALIDADJPG-%s-%d$", aux->nombre, value);

    if( send(aux->socket , mensaje , strlen(mensaje)  , 0) < 0)
    {
        puts("Send failed");
        return ;
    }

    return;

}


void changeFieldOfViewServer(int value, void*cam)
{
    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    if(value==0)
    {
        cv::setTrackbarPos("FieldOfView", aux->nombre, 1);
        sprintf(mensaje, "CLOSEFIELDOFVIEW-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }
    else if(value==2)
    {
        cv::setTrackbarPos("FieldOfView", aux->nombre, 1);
        sprintf(mensaje, "OPENFIELDOFVIEW-%s$", aux->nombre);

        if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
        {
            puts("Send failed");
            return ;
        }
    }

    return;
}


void cambiarFormatoServidor(int value, void* cam)
{
    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    /*****************************  revisar aqui *************/
    //aux->calidadjpg=a;

    sprintf(mensaje, "CHANGECAMARAFORMAT-%s-%d$", aux->nombre, value);

    if( send(aux->socket , mensaje , strlen(mensaje)  , 0) < 0)
    {
        puts("Send failed");
        return ;
    }

    return;

}

int getCameraRealTimeFps(void* cam)
{
    char mensaje[100000];
    Camara* aux= ((Camara *) cam);

    sprintf(mensaje, "GETCAMERAREALTIMEFPS-%s$", aux->nombre);

    //Send get frame command
    if( send(aux->socket , mensaje , strlen(mensaje) , 0) < 0)
    {
        puts("Send failed");
        return 1;
    }

    //Receive data from the server
    int length= recv(aux->socket , mensaje , 100 , 0);
    if( length < 0)
    {
        puts("recv failed");
        return -1;
    }


    return atoi( &(mensaje[0]) );

}

void flushSocket(void*cam)
{
    char mensaje[100000];
    Camara* aux= ((Camara *) cam);

    int length=recv( aux->socket, mensaje, sizeof(mensaje), MSG_DONTWAIT );
    // printf("flush---%d-----\n", length);
}

void setActiveGetRealFPS(void* cam)
{

    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    aux->activeGetRealFPS=0;

    sprintf(mensaje, "SETACTIVEGETREALFPS-%s$", aux->nombre);
    if( send(aux->socket , mensaje , strlen(mensaje)  , 0) < 0)
    {
        puts("Send failed");
        return ;
    }

    return;
}
void setInactiveGetRealFPS(void* cam)
{
    char mensaje[100];
    Camara* aux= ((Camara *) cam);
    aux->activeGetRealFPS=-1;

    sprintf(mensaje, "SETINACTIVEGETREALFPS-%s$", aux->nombre);
    if( send(aux->socket , mensaje , strlen(mensaje)  , 0) < 0)
    {
        puts("Send failed");
        return ;
    }

    return;
}




void OnClickMap(int event, int x, int y, int flags, void* data)
{
    MiniMapInfo* imagenes= ((MiniMapInfo *) data);

    if(imagenes->positionFixed==false)
    {
        imagenes->pointXCamRelative=x;
        imagenes->pointYCamRelative=y;
    }


    if  ( event == cv::EVENT_LBUTTONDOWN )
    {
        imagenes->positionFixed=true;

    }

}

void OnClickHeightMap(int event, int x, int y, int flags, void* data)
{
    MiniMapInfo* imagenes= ((MiniMapInfo *) data);

    if(imagenes->heightFixed==false)
    {
        if(y>650)
            y=650;
        else if(y<95)
            y=95;
        imagenes->heightCamRelative=y;
    }
    if  ( event == cv::EVENT_LBUTTONDOWN )
    {
        imagenes->heightFixed=true;
    }

}
