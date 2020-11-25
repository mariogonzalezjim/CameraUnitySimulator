#include "gui.h"
//This parameter corresponds to the minimum pixel height-coordinates (in the displayed image for the "showCameraScheme")
#define SECOND_IMG_OFFSET 95.0

//This parameter correspond to the distance between min & max user inputs for the 2nd screen ("showCameraScheme" --> cameraHeight-GUI)
#define SECOND_IMG_WIDTH 550.0f



Camara* insertCameraWithPlane(MiniMapInfo* imagesMap, char * image, int sock)
{
    Camara * cam;

    //decode image received
    std::string s( base64_decode(image) );
    vector <uchar> prueba( s.begin(), s.end() );
    cv::Mat source = cv::imdecode( cv::Mat(prueba), cv::IMREAD_COLOR);

    if(source.empty())
        return NULL;

    //prepare struct
    cv::Mat mImg=source.clone();
    imagesMap->source=&source;
    imagesMap->dst=&mImg;
    cv::Mat cameraIcon= cv::imread("./resources/cameraIcon.png", -1);
    imagesMap->icon=&cameraIcon;


    //step 1
    if((showMiniMap(imagesMap))==-1) return NULL;

    //coordinates recalcule
    imagesMap->pointXCamRelative =  imagesMap->pointXCamRelative / imagesMap->source->cols;
    imagesMap->pointYCamRelative =  imagesMap->pointYCamRelative / imagesMap->source->rows;
    imagesMap->angleCam= imagesMap->angleCam*-1;

    //step 2
    if((showCameraScheme(imagesMap))==-1) return NULL;

    //coordinates recalcule
    imagesMap->heightCamRelative =  (imagesMap->heightCamRelative-SECOND_IMG_OFFSET) / SECOND_IMG_WIDTH;
    imagesMap->heightAngleCam=-1*imagesMap->heightAngleCam;

    //coordinates conversion
    float positionX= coordinateXConversion(imagesMap->xMinAbsolute, imagesMap->xMaxAbsolute, imagesMap->pointXCamRelative);
    float positionY= coordinateYConversion(imagesMap->yMinAbsolute, imagesMap->yMaxAbsolute, imagesMap->pointYCamRelative);
    float positionHeight= coordinateHeightConversion(imagesMap->floorAbsolute, imagesMap->ceilingAbsolute, imagesMap->heightCamRelative);


    //camera has been confirmated, lets go to create it
    cam=crearCamaraConsole(sock);
    cam->posicionX=positionX;
    cam->posicionZ=positionY;
    cam->posicionY=positionHeight;
    cam->rotacionX=imagesMap->heightAngleCam;
    cam->rotacionY=imagesMap->angleCam;

    createCameraExtendedServer(cam, sock);

    return cam;
}







int showMiniMap(MiniMapInfo* imagesMap)
{

    //create a window for display.
    cv::namedWindow("Mapa", cv::WINDOW_AUTOSIZE );
    //set the callback function for mouse event
    cv::setMouseCallback("Mapa", OnClickMap, imagesMap);

    cv::imshow("Mapa", *(imagesMap->dst) );
    cv::displayStatusBar("Mapa", "Instrucciones:\n - Teclas de direccion <- y -> para rotar  - Enter para confirmar  - Esc para cancelar",0 );
    cv::moveWindow("Mapa", 100, 100);
    int tecla=-1;
    tecla= cv::waitKey(WAITMSTOREFRESH);


    while(tecla!=KEYESCAPE && (tecla!=KEYENTER ) )
    {
        //-90, -100 to center image on cursor
        overlayImage(*imagesMap->source, *imagesMap->icon, *imagesMap->dst, cv::Point(imagesMap->pointXCamRelative-96,imagesMap->pointYCamRelative-105));
        cv::imshow("Mapa", *imagesMap->dst );
        tecla= cv::waitKey(WAITMSTOREFRESH);


        //left
        if(tecla==KEYLEFT)
        {

            imagesMap->angleCam= imagesMap->angleCam +1;
            cv::Mat original= cv::imread("./resources/cameraIcon.png", -1);

            rotateImage(original, imagesMap->angleCam, *imagesMap->icon);

        } //right
        else if(tecla==KEYRIGHT)
        {

            imagesMap->angleCam=imagesMap->angleCam -1;
            cv::Mat original= cv::imread("./resources/cameraIcon.png", -1);

            rotateImage(original, imagesMap->angleCam , *imagesMap->icon);

        }
    }

    cv::destroyWindow("Mapa");

    if(tecla==KEYENTER)
        return 1;
    else
        return -1;

}



int showCameraScheme(MiniMapInfo* imagesMap)
{
    // Create a window for display.
    cv::namedWindow("Altura", cv::WINDOW_AUTOSIZE );
    cv::Mat background= cv::imread("./resources/backgroundCamera.jpg");

    imagesMap->source=&background;
    cv::Mat copyBackground= background.clone();
    imagesMap->dst=&copyBackground;
    cv::Mat cameraIcon2= cv::imread("./resources/cameraIcon2.png", -1);
    imagesMap->icon=&cameraIcon2;

    cv::setMouseCallback("Altura", OnClickHeightMap, imagesMap);

    cv::imshow("Altura", background );
    cv::moveWindow("Altura", 100, 100);
    cv::waitKey(WAITMSTOREFRESH);
    int tecla=-1;

    while(tecla!=KEYESCAPE && tecla!=KEYENTER)
    {
        overlayImage(*imagesMap->source, *imagesMap->icon, *imagesMap->dst, cv::Point(-100,imagesMap->heightCamRelative-281));
        cv::imshow("Altura", *imagesMap->dst );
        tecla= cv::waitKey(WAITMSTOREFRESH);

        // up
        if(tecla==KEYUP)
        {

            imagesMap->heightAngleCam= imagesMap->heightAngleCam +1;
            cv::Mat original= cv::imread("./resources/cameraIcon2.png", -1);

            rotateImage(original, imagesMap->heightAngleCam, *imagesMap->icon);

        } //down
        else if(tecla==KEYDOWN)
        {
            imagesMap->heightAngleCam=imagesMap->heightAngleCam -1;
            cv::Mat original= cv::imread("./resources/cameraIcon2.png", -1);

            rotateImage(original, imagesMap->heightAngleCam , *imagesMap->icon);

        }
    }
    cv::destroyWindow("Altura");

    if(tecla==KEYENTER)
        return 1;
    else
        return -1;

}
