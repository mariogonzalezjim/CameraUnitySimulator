/**
 * \file    CameraSimulator.cs
 *
 * \brief   Implements the Camera class.
 */

using UnityEngine;
using System;
using System.Threading;

/**
 * \class   CameraSimulator
 *
 * \brief   This class is an Unity camera's extension. It provides new features that default cameras not include   
 *          and allow to control the camera's properties. 
 *
 * \author  Mario gonzalez
 * \date    14/11/2016
 */

[System.Serializable]
public class CameraSimulator : MonoBehaviour
{

    /* Camera properties  */
    float positionX = 15;
    float positionY = 3;
    float positionZ = 5;
    float rotationX = 10;
    float rotationY = 10;
    float rotationZ = 0;
    float view = 60;
    int resWidth = 300;
    int resHeight = 300;
    int fps;
    int id = 0;
    int format;
    int quality;

    /*used for frame capture*/
    RenderTexture rtPipeline1 = null;
    RenderTexture rt = null;
    RenderTexture rtPipeline2 = null;
    Texture2D screenShotPipeline1 = null;
    Texture2D screenShotPipeline2 = null;

    /* used in code */
    float framecontador = 0;
    int frameunitycontador = 0;
    int frameimgcontador = 0;
    FrameCaptureStep step = 0;
    int stepParalelo = 0;
    int flagOnGUI = 0;

    DateTime endTime;
    TimeSpan elapsed;
    BufferCamera buffer = null;
    Camera camaraUnityObject;
    Thread threadBuffer;

    enum FrameCaptureStep
    {
        SaveCameraRender = 0,
        ReadCameraRender = 1,
        ExtractBytes = 2,
        WaitOneFrame = 3
    };


    /**
     * \fn  public void CreateCamera()
     *
     * \brief   Used for initialization. This function must be called after you have adjusted camera properties.
     *
     */

    public void createCamera()
    {
        //find the unity object(a camera) which this script is attachment
        camaraUnityObject = GameObject.Find(gameObject.name).GetComponent<Camera>();

        //set camera properties
        Vector3 temp = new Vector3(positionX, positionY, positionZ);
        gameObject.transform.position = temp;
        Vector3 rot = new Vector3(rotationX, rotationY, rotationZ);
        gameObject.transform.rotation = Quaternion.Euler(rot);
        camaraUnityObject.fieldOfView = view;

        // creates gpu variables that they will be used on frame capture logic
        rtPipeline1 = RenderTexture.GetTemporary(resWidth, resHeight, 16);
        rtPipeline2 = RenderTexture.GetTemporary(resWidth, resHeight, 16);
        screenShotPipeline1 = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);

        // Start Camera buffer thread because runs on pararell to this class
        buffer = new BufferCamera(resWidth, resHeight, name, id, format, quality, this);
        ThreadStart ts0 = new ThreadStart(buffer.startThread);
        threadBuffer = new Thread(ts0);
        threadBuffer.Priority = System.Threading.ThreadPriority.Highest;
        threadBuffer.Start();

        //send buffer class to command gestor 
        GameObject multiCameraSimulator = GameObject.Find("MultiCameraSimulator");
        StartApp app = multiCameraSimulator.GetComponent<StartApp>();
        app.addBufferCameraToCommandsGestor(buffer);

    }


    /**
     * \fn  void Update()
     *
     * \brief   This function is called automatically once per frame. Implements the logic to capture camera frames.
     *
     */

    void Update()
    {
        //   framecontador++;


        if (fps >= Application.targetFrameRate)
        {

            rt = RenderTexture.GetTemporary(resWidth, resHeight, 16);
            camaraUnityObject.targetTexture = rt;
            screenShotPipeline1 = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camaraUnityObject.Render();
            RenderTexture.active = rt;

            var timer2 = System.Diagnostics.Stopwatch.StartNew();
            screenShotPipeline1.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camaraUnityObject.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            RenderTexture.ReleaseTemporary(rt);
            timer2.Stop();
            //Debug.Log(timer2.ElapsedMilliseconds);

            byte[] bytes = screenShotPipeline1.GetRawTextureData();
            DestroyObject(screenShotPipeline1);
            DestroyObject(rt);
            //primer byte, id de la cam para asociar el frame
            bytes[0] = Convert.ToByte(id);
            sendFrameDataToBuffer(bytes);
        }
        else {
            framecontador += (float)fps / Application.targetFrameRate;

            if (framecontador >= 1)
            {

                rt = RenderTexture.GetTemporary(resWidth, resHeight, 16);
                camaraUnityObject.targetTexture = rt;
                screenShotPipeline1 = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                camaraUnityObject.Render();
                RenderTexture.active = rt;

                screenShotPipeline1.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                camaraUnityObject.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                RenderTexture.ReleaseTemporary(rt);

                byte[] bytes = screenShotPipeline1.GetRawTextureData();
                DestroyObject(screenShotPipeline1);
                DestroyObject(rt);
                //primer byte, id de la cam para asociar el frame
                bytes[0] = Convert.ToByte(id);
                sendFrameDataToBuffer(bytes);
                framecontador--;
            }

        }


    }

    /**
     * \fn  void saveTextureFromCamera(RenderTexture dst)
     *
     * \brief   Render the camera which is attachment to this class and save it to RenderTexture.
     *
     *
     * \param   dst Texture where camera render will be save.
     */

    void saveTextureFromCamera(RenderTexture dst)
    {
        camaraUnityObject.targetTexture = dst;
        camaraUnityObject.Render();
        camaraUnityObject.targetTexture = null;
        // Graphics.Blit(camara.targetTexture, dst);
    }


    byte[] frameDataFromTexture(Texture2D txt)
    {
        byte[] bytes = txt.GetRawTextureData();
        //primer byte, id de la camara para asociar el frame
        bytes[0] = Convert.ToByte(id);
        return bytes;
    }

    /**
     * \fn  void sendFrameDataToBuffer(byte[] data)
     *
     * \brief   Sends frame (image) data to buffer camera.
     *
     *
     * \param   data    byte array with the image data.
     */

    void sendFrameDataToBuffer(byte[] data)
    {
        buffer.addNewFrame(data, data.Length);
    }



    float fpsToMs(float fps)
    {
        float ms = 1f / fps;
        return ms;
    }

    /*********************
    *  GETTERS Y SETTERS *
    **********************/

    public void setNewFps(int fpsnew)
    {

        fps = fpsnew;
        return;
    }

    public int getFps()
    {
        return fps;
    }

    public void setNewWidth(int newidth)
    {
        resWidth = newidth;
        if (buffer != null)
            buffer.setWidth(newidth);
        return;
    }

    public void setNewHeight(int newheight)
    {
        resHeight = newheight;
        if (buffer != null)
            buffer.setHeight(newheight);
        return;
    }

    public void setNewPositionX(float a)
    {
        positionX = a;
        Vector3 temp = new Vector3(positionX, positionY, positionZ);
        gameObject.transform.position = temp;
        return;
    }

    public void setNewPositionY(float a)
    {
        //positionY = a - 9.7f;
        positionY = a;
        Vector3 temp = new Vector3(positionX, positionY, positionZ);
        gameObject.transform.position = temp;
        return;
    }
    public void setNewPositionZ(float a)
    {
        positionZ = a;
        Vector3 temp = new Vector3(positionX, positionY, positionZ);
        gameObject.transform.position = temp;
        return;
    }

    public void setNewRotateX(float a)
    {
        rotationX = a;
        Vector3 rot = new Vector3(rotationX, rotationY, rotationZ);
        gameObject.transform.rotation = Quaternion.Euler(rot);
        return;
    }

    public void setNewRotateY(float a)
    {
        rotationY = a;
        Vector3 rot = new Vector3(rotationX, rotationY, rotationZ);
        gameObject.transform.rotation = Quaternion.Euler(rot);
        return;
    }

    public void setNewRotateZ(float a)
    {
        rotationZ = a;
        Vector3 rot = new Vector3(rotationX, rotationY, rotationZ);
        gameObject.transform.rotation = Quaternion.Euler(rot);
        return;
    }

    public void movePositionX(int a)
    {
        positionX += a;
        Vector3 temp = new Vector3(positionX, positionY, positionZ);
        gameObject.transform.position = temp;
        return;
    }

    public void movePositionY(int a)
    {
        positionY += a * 0.5f;
        Vector3 temp = new Vector3(positionX, positionY, positionZ);
        gameObject.transform.position = temp;
        return;
    }
    public void movePositionZ(int a)
    {
        positionZ += a;
        Vector3 temp = new Vector3(positionX, positionY, positionZ);
        gameObject.transform.position = temp;
        return;
    }

    public void moveRotateX(int a)
    {
        rotationX += a;
        Vector3 rot = new Vector3(rotationX, rotationY, rotationZ);
        gameObject.transform.rotation = Quaternion.Euler(rot);
        return;
    }

    public void moveRotateY(int a)
    {
        rotationY += a;
        Vector3 rot = new Vector3(rotationX, rotationY, rotationZ);
        gameObject.transform.rotation = Quaternion.Euler(rot);
        return;
    }

    public void changeFieldOfView(int a)
    {
        float aux = (float)a * 0.1f;
        camaraUnityObject.fieldOfView = camaraUnityObject.fieldOfView + a;
        return;
    }


    public void setId(int id)
    {
        this.id = id;
    }

    public int getId()
    {
        return id;
    }
    public int getWidth()
    {
        return resWidth;
    }
    public int getHeight()
    {
        return resHeight;
    }

    public int getFormat()
    {
        return format;
    }

    public int getQualityJPEG()
    {
        return quality;
    }

    public float getPositionX()
    {
        return positionX;
    }

    public float getPositionY()
    {
        return positionY;
    }

    public float getPositionZ()
    {
        return positionZ;
    }

    public float getRotationX()
    {
        return rotationX;
    }

    public float getRotationY()
    {
        return rotationY;
    }

    public float getRotationZ()
    {
        return rotationZ;
    }

    public void setImageFormat(int f)
    {
        this.format = f;
    }

    public void setImageQuality(int q) { this.quality = q; }

    public string getCameraDetails()
    {
        String msg = this.getId().ToString() + "/" +
                     this.getFps().ToString() + "/" +
                     this.getWidth().ToString() + "/" +
                     this.getHeight().ToString() + "/" +
                     this.getFormat().ToString() + "/" +
                     this.getQualityJPEG().ToString() + "/" +
                     this.getPositionX().ToString() + "/" +
                     this.getPositionY().ToString() + "/" +
                     this.getPositionZ().ToString() + "/" +
                     this.getRotationX().ToString() + "/" +
                     this.getRotationY().ToString() + "/" +
                     this.getRotationZ().ToString();
        return msg;

    }

    public string getName() {

        return name;
    }


    /**
     * \fn  void OnDestroy()
     *
     * \brief   This function is called automatically when the camera is being deleted.
     *
     */

    void OnDestroy()
    {
        //Debug.Log(framecontador + " " + frameimgcontador);
        //Debug.Log(resultado);
        //stop buffer thread
        if (threadBuffer != null)
        {
            buffer.RequestStop();
            buffer.awakeThread();
            threadBuffer.Join();
        }

        Destroy(rtPipeline1);
        Destroy(rtPipeline2);
        Destroy(screenShotPipeline1);
        threadBuffer = null;
        buffer = null;
    }
}



