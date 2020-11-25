#include "camara.h"


Camara* crearCamaraConsole(int sock)
{

    Camara * cam= (Camara*)malloc(sizeof(Camara));

    cam->socket=sock;
    printf("\nNombre de la camara: ");
    scanf("%s", cam->nombre);
    printf("\nFps de la camara: ");
    scanf("%d", &cam->fps);
    printf("\nAncho de la imagen: ");
    scanf("%d", &cam->widthResolution);
    printf("\nAlto de la imagen: ");
    scanf("%d", &cam->heightResolution);
    cam->qualityjpg=75;
    cam->formato=0; ///< jpg
    cam->activeGetRealFPS=-1;
    cam->posicionX=0;
    cam->posicionY=0;
    cam->posicionZ=0;
    cam->rotacionX=0;
    cam->rotacionY=0;
    cam->fieldofview=1;

    return cam;
}


Camara* createCameraDefault(int sock)
{

    Camara * cam= (Camara*)malloc(sizeof(Camara));

    cam->socket=sock;
    // by default
    strcpy (cam->nombre,"default");
    cam->fps=10;
    cam->widthResolution=640;
    cam->heightResolution=480;
    cam->formato=0;
    cam->activeGetRealFPS=-1;
    cam->qualityjpg=75;
    cam->formato=0; ///< jpg
    cam->posicionX=0;
    cam->posicionY=0;
    cam->posicionZ=0;
    cam->rotacionX=0;
    cam->rotacionY=0;
    cam->fieldofview=1;

    return cam;
}

void deleteCamera(Camara* cam)
{
    free(cam);
    return;
}

MiniMapInfo* createMiniMapInfo()
{

    MiniMapInfo* imagesMap= (MiniMapInfo*)malloc(sizeof(MiniMapInfo));
    imagesMap->pointXCamRelative=-1;
    imagesMap->pointYCamRelative=-1;
    imagesMap->angleCam=0;
    imagesMap->heightCamRelative=0;
    imagesMap->heightAngleCam=0;
    imagesMap->positionFixed=false;


    return imagesMap;
}

Camara* createCameraFromFile(int sock, char* info)
{

    int i=0;
    Camara * cam= (Camara*)malloc(sizeof(Camara));

    cam->socket=sock;

    char* chars_array = strtok(info, "/");

    while(chars_array)
    {
        if(i==0)
        {
            strcpy (cam->nombre,chars_array);
        }
        else if(i==1)
        {
            cam->fps = atoi(chars_array);
        }
        else if(i==2)
        {
            cam->widthResolution= atoi(chars_array);
        }
        else if(i==3)
        {
            cam->heightResolution= atoi(chars_array);
        }
        else if(i==4)
        {
            cam->formato= atoi(chars_array);
        }
        else if(i==5)
        {
            cam->qualityjpg= atoi(chars_array);
        }
        else if(i==6)
        {
            cam->posicionX= atof(chars_array);
        }
        else if(i==7)
        {
            cam->posicionY= atof(chars_array);
        }
        else if(i==8)
        {
            cam->posicionZ= atof(chars_array);
        }
        else if(i==9)
        {
            cam->rotacionX= atof(chars_array);
        }
        else if(i==10)
        {
            cam->rotacionY= atof(chars_array);
        }


        chars_array = strtok(NULL, "/");
        i++;
    }
    return cam;

}



float coordinateXConversion(float min, float max, float pointRelative)
{
    float distancia=0;

    if(min<0 && max>=0)
    {
        distancia= -(min) +  max;
    }
    else if(min<0 && max<0)
    {
        distancia= ((max) - (min))*-1 ;
    }
    else
    {
        distancia= (max) - (min);
    }
    return  (min + (distancia * pointRelative));

}


float coordinateYConversion(float min, float max, float pointRelative)
{

    float distancia=0;

    //coordinate y == coordinate z in unity (3 dimensions)
    if(min<0 && max>=0)
    {
        distancia= -(min) +  max;
    }
    else if(min<0 && max<0)
    {
        distancia= ((max) - (min))*-1 ;
    }
    else
    {
        distancia= (max) - (min);
    }
    return  (max - (distancia * pointRelative) );

}

float coordinateHeightConversion(float floor, float ceiling, float pointRelative)
{

    float distancia=0;

    //height
    if(floor<0 && ceiling>=0)
    {
        distancia= -(floor) + ceiling;
    }
    else if(floor<0 && ceiling<0)
    {
        distancia= ((ceiling) - (floor))*-1 ;
    }
    else
    {
        distancia= (ceiling) - (floor);
    }
    return  (ceiling - (distancia * pointRelative));

}
