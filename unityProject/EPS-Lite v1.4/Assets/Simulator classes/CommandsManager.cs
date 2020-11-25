/**
 * \file    CommandsManager.cs
 *
 * \brief   Implements the commands manager.
 */
using UnityEngine;
using System.Collections;
using System.Threading;
using System;


/**
* \class   CommandsManager
*
* \brief   This class manages the commands received from clients and processes them. 
*          It is created with an independiente thread for background command processing.
*
* \author    Mario gonzalez
* \date      14/11/2016
*/
public class CommandsManager
{


    TCPServerAsyn server;
    volatile bool _stop;
    int numClients, i, idCam = 0;
    string header;
    ThreadPool comunication;
    ArrayList camerasBuffer = new ArrayList();

    int contador = 0;
    MinimapSceneInfo minimap;

    /**
     * \fn  void startThread()
     *
     * \brief  This function is called when the thread starts. It checks incoming data on background from the tcp server. 
     *
     */
    public void startThread()
    {

        //wait for inialization
        Thread.Sleep(2000);

        comunication.setPoolActive();

        continuousThread();

        //finishing thread
        comunication.setPoolInactive();

    }


    /**
     * \fn  void Update()
     *
     * \brief   This function has an infinite loop that calls the Update function and checks the _stop variable. 
     *
     */
    void continuousThread()
    {

        //check messages from clients using the server thread
        while (!_stop)
        {
            Update();
            Thread.Sleep(1);

        }

    }


    /**
     * \fn  void Update()
     *
     * \brief   This function reads for incoming commands from tcp server and manages them. 
     *
     */
    void Update()
    {

        try
        {

            if (server.socketReady == true)
            {

                numClients = server.getNumClients();
                if (numClients == 0)
                    return;

                //I read client's one by one
                for (i = 0; i < numClients; i++)
                {
                    //read client i
                    string messages = server.readSocket(i);

                    if (!String.IsNullOrEmpty(messages))
                    {
                        Debug.Log("command received:" + messages);
                        //I split because I can have one or more commands
                        string[] array = messages.Split('$');

                        // Process client's commands one by one
                        foreach (string command in array)
                        {
                            //read command header
                            header = command.Split('-')[0];

                            if (header == "GETFRAME")
                            {

                                foreach (BufferCamera cam in camerasBuffer)
                                {
                                    if (cam.getName() == command.Split('-')[1])
                                    {
                                        // NO ACK: comment since the two replies get mixed in the socket                                    
                                        //Reply to client with frame
                                        string frame = cam.getFrame();
                                        server.writeSocket(frame.Length + "#" + frame + "\0", i);

                                    }
                                }


                            }
                            else if (header == "CREACAMARA")
                            {

                                comunication.addData(command + "-" + idCam.ToString() + "$");

                                //Reply to client with the ID of the camera                            
                                server.writeSocket(idCam.ToString() + "\0", i);
                                idCam++;
                            }
                            else if (header == "CREATECAMERAEXTENDED")
                            {
                                comunication.addData(command + "/" + idCam.ToString() + "$");

                                //Reply to client with the ID of the camera                            
                                server.writeSocket(idCam.ToString() + "\0", i);
                                idCam++;
                            }
                            else if (header == "CHANGECAMARACALIDADJPG")
                            {
                                foreach (BufferCamera cam in camerasBuffer)
                                {
                                    if (cam.getName() == command.Split('-')[1])
                                    {
                                        cam.setQuality(Int32.Parse(command.Split('-')[2]));

                                        //Reply to client with an ACK
                                        server.writeSocket("OK\0", i);
                                    }
                                }
                            }
                            else if (header == "CHANGECAMARAFORMAT")
                            {
                                foreach (BufferCamera cam in camerasBuffer)
                                {
                                    if (cam.getName() == command.Split('-')[1])
                                    {
                                        cam.setImageFormat(Int32.Parse(command.Split('-')[2]));

                                        //Reply to client with an ACK
                                        server.writeSocket("OK\0", i);
                                    }
                                }
                            }
                            else if (header == "DELETECLIENT")
                            {

                                //Reply to client with an ACK
                                server.writeSocket("OK\0", i);

                                server.deleteClient(i);
                                GC.Collect();
                            }
                            else if (header == "DELETECAMARA")
                            {

                                int indice = 0;

                                foreach (BufferCamera cam in camerasBuffer)
                                {

                                    if (cam.getName() == command.Split('-')[1])
                                    {

                                        //delete buffer 
                                        //delete cam (camerascontroller)
                                        comunication.addData(command + "$");
                                        cam.RequestStop();
                                        camerasBuffer[indice] = null;
                                        camerasBuffer.RemoveAt(indice);

                                        //Reply to client with an ACK
                                        server.writeSocket("OK\0", i);
                                        break;
                                    }
                                    else
                                    {
                                        indice++;
                                    }

                                }

                            }
                            else if (header == "GETCAMERAREALTIMEFPS")
                            {
                                foreach (BufferCamera cam in camerasBuffer)
                                {
                                    if (cam.getName() == command.Split('-')[1])
                                    {
                                        int fps = cam.getRealTimeFps();
                                        server.writeSocket(fps.ToString() + "\0", i);
                                    }
                                }
                            }
                            else if (header == "GETDETAILSALLCAMERAS")
                            {

                                string response = "";
                                foreach (BufferCamera cam in camerasBuffer)
                                {
                                    response += cam.getName() + "/" + cam.getCameraSource().getCameraDetails() + "#";
                                }
                                server.writeSocket(response + "\0", i);
                            }
                            else if (header == "GETDETAILS")
                            {

                                foreach (BufferCamera cam in camerasBuffer)
                                {
                                    if (cam.getName() == command.Split('-')[1])
                                    {
                                        server.writeSocket(cam.getCameraSource().getCameraDetails() + "\0", i);
                                    }

                                }

                            }
                            else if (header == "SETACTIVEGETREALFPS")
                            {
                                int ok = -1;
                                foreach (BufferCamera cam in camerasBuffer)
                                {
                                    if (cam.getName() == command.Split('-')[1])
                                    {
                                        cam.setActiveGetRealFps();
                                        ok++;
                                        break;
                                    }
                                }
                                if (ok != -1)
                                    server.writeSocket("OK\0", i); //Reply ACK to client
                                else
                                    server.writeSocket("ERR\0", i); //Reply ACK to client
                            }
                            else if (header == "SETINACTIVEGETREALFPS")
                            {
                                int ok = -1;
                                foreach (BufferCamera cam in camerasBuffer)
                                {
                                    if (cam.getName() == command.Split('-')[1])
                                    {
                                        cam.setInactiveGetRealFps();
                                        ok++;
                                    }
                                }
                                if (ok != -1)
                                    server.writeSocket("OK\0", i); //Reply ACK to client
                                else
                                    server.writeSocket("ERR\0", i); //Reply ACK to client
                            }
                            else if (header == "GETMINIMAP")
                            {
                                server.writeSocket(minimap.minimapImage.Length + "#" + minimap.minimapImage + "\0", i);

                            }
                            else if (header == "GETMINIMAPINFO")
                            {
                                string msg = "MINIMAPINFO/" + minimap.minimapImage.Length + "/" + minimap.xmin + "/" + minimap.xmax + "/" + minimap.zmin
                                           + "/" + minimap.zmax + "/" + minimap.floor + "/" + minimap.ceiling + "/$";
                                server.writeSocket(msg + "\0", i);

                            }
                            else if (header == "CHANGECAMARAFPS")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "MOVECAMARALEFT")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "MOVECAMARARIGHT")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "MOVECAMARAUP")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "MOVECAMARADOWN")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "MOVECAMARAAHEAD")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "MOVECAMARABACK")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed                       
                            }
                            else if (header == "ROTATECAMARALEFT")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "ROTATECAMARARIGHT")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "ROTATECAMARAUP")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed
                            }
                            else if (header == "ROTATECAMARADOWN")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed                            
                            }
                            else if (header == "CLOSEFIELDOFVIEW")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed                            
                            }
                            else if (header == "OPENFIELDOFVIEW")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed                            
                            }
                            else if (header == " " || header == "")
                            {
                                //empty command do to the string splitting  operation
                                // ???????????????????                           
                            }
                            else if (header == "CHANGECAMERAWIDTH")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed 
                            }
                            else if (header == "CHANGECAMERAHEIGHT")
                            {
                                server.writeSocket("OK\0", i); //Reply ACK to client
                                comunication.addData(command + "$");//add command to be processed 
                            }
                            else if (header == "RESET")
                            {
                                this.deleteAllCameras();
                                server.writeSocket("OK\0", i); //Reply ACK to client

                            }
                            else
                            {
                                //   Debug.Log("Unknown command!!!" + command);
                                server.writeSocket("ERR\0", i); //Reply ACK to client
                            }

                        }
                    }
                }
            }
            numClients = 0;

        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
        return;
    }



    /**
     * \fn  public void RequestStop()
     *
     * \brief   Stop thread
     *
     */
    public void RequestStop()
    {
        _stop = true;
    }


    /**
    * \fn  public void setThreadPool(ThreadPool pool)
    *
    * \brief   
    *
    */
    public void setThreadPool(ThreadPool pool)
    {
        comunication = pool;
    }


    /**
    * \fn  public void setTCPServer(TCPServerAsyn tcp)
    *
    * \brief   
    *
    */
    public void setTCPServer(TCPServerAsyn tcp)
    {
        server = tcp;
    }


    /**
    * \fn  public void addBufferCamera(BufferCamera buffer)
    *
    * \brief   
    *
    */
    public void addBufferCamera(BufferCamera buffer)
    {
        Debug.Log("Added to buffer camera" + buffer.getName());
        camerasBuffer.Add(buffer);

    }


    /**
    * \fn  public void addMiniMap(MinimapSceneInfo info)
    *
    * \brief   
    *
    */
    public void addMiniMap(MinimapSceneInfo info) { minimap = info; }


    /**
    * \fn  public void deleteAllCameras()
    *
    * \brief   This function send an intruction to the cameras controller to delete all cameras and buffers created. 
    *
    */
    public void deleteAllCameras()
    {

        comunication.addData("RESET$");
        foreach (BufferCamera cam in camerasBuffer)
        {
            //delete buffer               
            cam.RequestStop();
        }
        camerasBuffer = null;
        camerasBuffer = new ArrayList();

    }
}
