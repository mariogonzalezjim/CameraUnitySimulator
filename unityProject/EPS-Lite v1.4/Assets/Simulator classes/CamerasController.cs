/**
 * \file    CamerasController.cs
 *
 * \brief   Implements the Cameras controller.
 */

using UnityEngine;
using System;
using System.Collections.Generic;


/**
* \class   CamerasController
*
* \brief   This class is a gestor for multiple cameras. It receives commands from another classes throught a ThreadPool. 
*          Implemented funcionality: create cameras, delete, change properties.
*
* \author    Mario gonzalez
* \date      14/11/2016
*/
public class CamerasController : MonoBehaviour
{

    ThreadPool comunication;
    string command = "";
    string header = "";
    int camIndex = 0;
    private Transform cameraObject;

    List<CameraSimulator> camaras = new List<CameraSimulator>();
    string resultado = "";
    int contador2 = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame 
    void Update()
    {

        //checking the pool for instructions
        if (comunication.getStatusPool() == true && comunication != null)
        {

            string message = comunication.readIncomingData();
            if (!String.IsNullOrEmpty(message))
            {

                string[] arrayCommands = message.Split('$');
                foreach (string command in arrayCommands)
                {

                    header = command.Split('-')[0];
                    if (header == "CREACAMARA")
                    {
                        //create new object on unity with camera type
                        Transform newcamera = (Transform)GameObject.Instantiate(cameraObject, new Vector3(0, 0, 0), Quaternion.identity);
                        newcamera.gameObject.name = command.Split('-')[1];

                        int cameraframerate = Int32.Parse(command.Split('-')[2]);
                        // 30 max framerate per camera
                        if (cameraframerate > 30)
                        {
                            cameraframerate = 30;
                        }

                        //adjust unity framerate with camera framerate
                        if (camIndex <= 0)
                        {
                            Application.targetFrameRate = cameraframerate;
                        }
                        else if (cameraframerate > Application.targetFrameRate)
                        {
                            Application.targetFrameRate = cameraframerate;
                        }

                        //acess camera's class to set properties
                        CameraSimulator camera = newcamera.gameObject.GetComponent<CameraSimulator>();
                        camaras.Add(camera);
                        camera.setNewFps(cameraframerate);
                        camera.setNewWidth(Int32.Parse(command.Split('-')[3]));
                        camera.setNewHeight(Int32.Parse(command.Split('-')[4]));
                        camera.setImageFormat(Int32.Parse(command.Split('-')[5]));
                        camera.setImageQuality(Int32.Parse(command.Split('-')[6]));
                        camera.setId(Int32.Parse(command.Split('-')[7]));

                        camera.createCamera();
                        camIndex++;
                    }
                    else if (header == "CREATECAMERAEXTENDED")
                    {

                        //create new object on unity with camera type
                        Transform newcamera = (Transform)GameObject.Instantiate(cameraObject, new Vector3(0, 0, 0), Quaternion.identity);
                        newcamera.gameObject.name = command.Split('-')[1].Split('/')[0];
                        int cameraframerate = Int32.Parse(command.Split('/')[1]);
                        // 30 max framerate per camera
                        if (cameraframerate > 30)
                        {
                            cameraframerate = 30;
                        }

                        //adjust unity framerate with camera framerate
                        if (camIndex <= 0)
                        {
                            Application.targetFrameRate = cameraframerate;
                        }
                        else if (cameraframerate > Application.targetFrameRate)
                        {
                            Application.targetFrameRate = cameraframerate;
                        }

                        //acess camera's class to set properties
                        CameraSimulator camera = newcamera.gameObject.GetComponent<CameraSimulator>();
                        camaras.Add(camera);
                        camera.setNewFps(cameraframerate);
                        //images resolution
                        camera.setNewWidth(Int32.Parse(command.Split('/')[2]));
                        camera.setNewHeight(Int32.Parse(command.Split('/')[3]));
                        camera.setImageFormat(Int32.Parse(command.Split('/')[4]));
                        camera.setImageQuality(Int32.Parse(command.Split('/')[5]));
                        //position on 3d space
                        camera.setNewPositionX((float)(Convert.ToDouble((command.Split('/')[6]).Replace(',', '.'))));
                        camera.setNewPositionY((float)Convert.ToDouble(command.Split('/')[7].Replace(',', '.')));
                        camera.setNewPositionZ((float)Convert.ToDouble(command.Split('/')[8].Replace(',', '.')));
                        camera.setNewRotateX((float)Convert.ToDouble(command.Split('/')[9].Replace(',', '.')));
                        camera.setNewRotateY((float)Convert.ToDouble(command.Split('/')[10].Replace(',', '.')));
                        camera.setNewRotateZ((float)Convert.ToDouble(command.Split('/')[11].Replace(',', '.')));

                        //Debug.Log(command.Split('/')[12]);
                        camera.setId(Int32.Parse(command.Split('/')[12]));

                        camera.createCamera();
                        camIndex++;
                    }
                    else if (header == "DELETECAMARA")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        Destroy(cameraObject);
                        camIndex--;
                        //readjust unity framerate
                        adjustUnityFramerate();
                       
                    }
                    else if (header=="CHANGECAMERAWIDTH") {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("setNewWidth", Int32.Parse(command.Split('-')[2]));
                    }
                    else if (header == "CHANGECAMERAHEIGHT")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("setNewHeight", Int32.Parse(command.Split('-')[2]));
                    }
                    else if (header == "MOVECAMARALEFT")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("movePositionX", -1);

                    }
                    else if (header == "MOVECAMARARIGHT")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("movePositionX", 1);

                    }
                    else if (header == "MOVECAMARAUP")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("movePositionY", 1);

                    }
                    else if (header == "OPENFIELDOFVIEW")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("changeFieldOfView", 2);
                    }
                    else if (header == "CLOSEFIELDOFVIEW")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("changeFieldOfView", -2);
                    }
                    else if (header == "MOVECAMARADOWN")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("movePositionY", -1);

                    }
                    else if (header == "MOVECAMARAAHEAD")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("movePositionZ", 1);

                    }
                    else if (header == "MOVECAMARABACK")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("movePositionZ", -1);

                    }
                    else if (header == "ROTATECAMARALEFT")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("moveRotateY", -2);

                    }
                    else if (header == "ROTATECAMARARIGHT")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("moveRotateY", 2);

                    }
                    else if (header == "ROTATECAMARAUP")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("moveRotateX", -2);

                    }
                    else if (header == "ROTATECAMARADOWN")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("moveRotateX", 2);

                    }
                    else if (header == "CHANGECAMARAFPS")
                    {
                        GameObject cameraObject = GameObject.Find(command.Split('-')[1]);
                        cameraObject.SendMessage("setNewFps", Int32.Parse(command.Split('-')[2]));                        
                        adjustUnityFramerate();

                    }
                    else if (header == "RESET")
                    {
                        //delete all cameras
                        deleteAllCameras();
                        Application.targetFrameRate = 30;
                    }
                }
            }


        }
        else {

        }

        //if (camaras.Count > 2) {
        //    Camara aux1 = camaras[0];
        //    Camara aux2 = camaras[1];
        //    Camara aux3 = camaras[2];
        //    Debug.Log((aux1.tiempoFrame + aux2.tiempoFrame + aux3.tiempoFrame) + " 640x480@3cams metodoFinal");
        //}
        //if ((contador2 % 10) == 0)
        //{

        //    resultado += (1.0f / Time.deltaTime) + " " + camaras.Count + "\n";
        //}
        //contador2++;
    }

    void OnDestroy()
    {
    
    }

    // adjust unity framerate with existing cameras
    public void adjustUnityFramerate()
    {
        int fpsmax = 0;
        GameObject[] arrayCameras = GameObject.FindGameObjectsWithTag("CameraTag");

        foreach (GameObject camObject in arrayCameras)
        {
            CameraSimulator camClass = camObject.GetComponent<CameraSimulator>();
            if (fpsmax < camClass.getFps())
            {
                fpsmax = camClass.getFps();
            }
        }

        Application.targetFrameRate = fpsmax;

    }


    /**
     * \fn  void deleteAllCameras()
     *
     * \brief   Deletes all cameras on active Unity's scene.
     *
     */
    public void deleteAllCameras()
    {
        GameObject[] arrayCameras = GameObject.FindGameObjectsWithTag("CameraTag");

        foreach (GameObject camObject in arrayCameras)
        {
            Destroy(camObject);
        }

        camIndex = 0;
    }

    /**
     * \fn  public void setCameraObject(Transform camera)
     *
     * \brief   Sets camera object.
     *
     *
     * \param   camera  The Unity prebaf that it contains our custom camera.
     */

    public void setCameraObject(Transform camera)
    {
        cameraObject = camera;
    }



    public void setThreadPool(ThreadPool aux)
    {
        comunication = aux;
    }


    public int getNumCameras() {
        return camIndex;
    }

}
