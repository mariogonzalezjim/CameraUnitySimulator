/**
 * \file    buffercamera.cs
 *
 * \brief    This class is a buffer implementation for Camera class. 
 */

using UnityEngine;
using System;
using System.Threading;
using Emgu.CV;

/**
 * \class   BufferCamera
 *
 * \brief   Must be created with an independiente thread for background image processing.
 *          Each camera has its own buffer.  
 *
 * \author  Mario Gonzalez
 * \date    14/11/2016
 */

public class BufferCamera
{

    //this is the buffer, only storage last frame encoded
    volatile String dataimage = "";
    //true if a new frame is on queue for encoding
    volatile bool _newFrame = false;
    volatile byte[] nextframe;
    AutoResetEvent _waitHandle = new AutoResetEvent(true);
    int width, height, id = 0;
    string name;
    CustomEncoder myEncoder;
    volatile bool _stop;
    System.Diagnostics.Stopwatch timer;
    long realFps=1;
    bool activeGetRealFps = false;
    int i=0;
    int format;
    int quality;

    CameraSimulator cam;


    public BufferCamera(int width_, int height_, string name_, int id_, int _format, int _quality, CameraSimulator _cam)
    {

        width = width_;
        height = height_;
        format = _format;
        quality = _quality;
        myEncoder = new CustomEncoder(width, height, quality, format);
        name = name_;
        id = id_;
        cam = _cam;
    }

    /**
     * \fn  public void startThread()
     *
     * \brief   This function must be call when the thread starts. It checks for new incoming frames and encode them.
     *
     */

    public void startThread()
    {
        _stop = false;

        while (!_stop)
        {

            if (_newFrame == true)
            {
                EncodeFrame();

            }
            else {
                //wait for new frame
                this._waitHandle.WaitOne();
            }
        }
        this.DestroyThread();
    }

    /**
     * \fn  void EncodeFrame()
     *
     * \brief   Encode last frame received and save it on the buffer.
     *
     */
    void EncodeFrame()
    {
     
        try
        {
            
            Image<Emgu.CV.Structure.Rgb, Byte> image = new Image<Emgu.CV.Structure.Rgb, Byte>(width, height);
            image.Bytes = nextframe; // byte array

            dataimage = myEncoder.encodeFrame(image);
            _newFrame = false;
            image = null;
            //to calcule camera realtime framerate
            if (activeGetRealFps == true)
            {
                if (i % 20 == 0)
                {
                    timer.Stop();
                    realFps = (20000 / timer.ElapsedMilliseconds);
                    timer = System.Diagnostics.Stopwatch.StartNew();
                    i = 0;
                }
                i++;
            }
        }
        catch (ArgumentException a)
        {
            Debug.Log("Ha saltado una excepcion en la camara" + a);
        }


    }

    /**
     * \fn  public void addNewFrame(byte[] newFrame, int length)
     *
     * \brief   Adds a new frame to buffer and awake thread to encode it.
     *
     *
     * \param   newFrame    Byte array with the image.
     * \param   length  Byte array length.
     */

    public void addNewFrame(byte[] newFrame, int length)
    {
        nextframe = new byte[length];
        Buffer.BlockCopy(newFrame, 0, nextframe, 0, length);
        _newFrame = true;
        this.awakeThread();

    }

    /**
     * \fn  public void awakeThread()
     *
     * \brief   Awake thread.
     *
     */

    public void awakeThread()
    {
        _waitHandle.Set();
    }

    /**
     * \fn  public void RequestStop()
     *
     * \brief   Request stop thread.
     *
     */

    public void RequestStop() {
        _stop = true;
        this.awakeThread();
    }


    private void DestroyThread() {
        dataimage = null;
        myEncoder = null;
        GC.SuppressFinalize(this);
        Debug.Log("Destroy buffer camera " + name);

    }

    /*********************
    *  GETTERS Y SETTERS *
    **********************/

    public string getFrame()
    {
        return dataimage;
    }

    public void setFrame(String img)
    {
        dataimage = img;
    }

    public void setName(string nm)
    {
        name = nm;
    }

    public void setId(int newid)
    {
        id = newid;
    }

    public void setWidth(int wdth)
    {
        width = wdth;
    }

    public void setHeight(int newheigth)
    {
        height = newheigth;
    }

    public int getId()
    {
        return id;
    }

    public void setQuality(int cal)
    {
        myEncoder.setQuality(cal);
    }

    public string getName() { return name; }

    public void setImageFormat(int newformat) {
        myEncoder.setImageFormat(newformat);
    }

    public int getRealTimeFps() { return (int) realFps; }

    public void setActiveGetRealFps() {
        timer= System.Diagnostics.Stopwatch.StartNew();
        activeGetRealFps = true;
    }

    public void setInactiveGetRealFps()
    {
        timer.Stop();
        activeGetRealFps = false;
    }

    public CameraSimulator getCameraSource()
    {
        return cam;
    }
}
