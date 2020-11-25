/**
 * \file    StartApp.cs
 *
 * \brief   Implements the main application class.
 */

using UnityEngine;
using System.Collections;
using System.Threading;

/**
 * \class   StartApp
 *
 * \brief   This class is equivalent to main class. Its function is to inizializate all classes and threads that will be used when we start the simulator.
 *          It also destroys classes and threads generated when exit application. It includes a simple GUI with one buttom on screen and display the app status.

 * \author  Mario Gonzalez
 * \date    14/11/2016
 */

public class StartApp : MonoBehaviour
{

    CommandsManager gestor;
    TCPServerAsyn server;
    Thread threadServer = null;
    Thread threadGestor = null;
    CamerasController camerasController;
    ThreadPool comunicationThreads;
    FPSDisplay fps;

    bool connected = false;
    bool waiting = false;

    //this is vital to configure!!! 
    public Transform cameraObject;

    private Rect windowRect = new Rect((Screen.width / 3), 15, (Screen.width / 3), (Screen.height / 5));

    // Update is called once per frame
    void Update()
    {


    }

    // Use this for initialization
    void Start()
    {

        Application.runInBackground = true;
        Application.targetFrameRate = 30;
        QualitySettings.maxQueuedFrames = -1;
        QualitySettings.vSyncCount = 0;
        gestor = new CommandsManager();
        server = new TCPServerAsyn();
        gestor.setTCPServer(server);
        camerasController = this.gameObject.AddComponent<CamerasController>();
        camerasController.setCameraObject(cameraObject);
        fps = this.gameObject.AddComponent<FPSDisplay>();
        comunicationThreads = new ThreadPool();
        gestor.setThreadPool(comunicationThreads);
        camerasController.setThreadPool(comunicationThreads);

    }


    // display app status on screen
    void OnGUI()
    {
       windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "APP Status");
    }


    void DoMyWindow(int windowID)
    {
        var style = new GUIStyle(GUI.skin.button);
        GUI.color = Color.clear;
        GUILayout.Box("");

        GUI.color = Color.cyan;
        //if connection has not been made, display button to enable the simulator
        if (connected == false && waiting == false)
        {

            style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = Color.yellow;
            style.fontSize = 16;
            GUI.color = Color.white;
            if (GUILayout.Button("Start", style, GUILayout.Width(Screen.width / 3), GUILayout.Height(Screen.height / 6)))
            {
                Debug.Log("Starting application");


                //open thread for tcp server			
                ThreadStart ts = new ThreadStart(server.setupSocket);
                threadServer = new Thread(ts);
                threadServer.Start();

                //open thread for app controller
                ThreadStart ts2 = new ThreadStart(gestor.startThread);
                threadGestor = new Thread(ts2);
                threadGestor.Start();

                waiting = true;
            }

        }//waiting for connection
        else if (waiting == true)
        {
            style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = Color.green;
            style.fontSize = 15;
            GUILayout.Button("Waiting", style);
            if (server.openSocket == true)
            {
                waiting = false;
                connected = true;
            }
        }
        //connection has been made, display reset buttom
        else {
            style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = Color.red;
            style.fontSize = 15;
            GUI.color = Color.white;
            if (GUILayout.Button("Reset", style, GUILayout.Width(Screen.width / 3), GUILayout.Height(Screen.height / 6)))
            {
                Debug.Log("Reset application");
                gestor.deleteAllCameras();
                //connected = false;
                //waiting = false;
            }

        }

        GUI.color = Color.clear;
        GUILayout.Box("");
        GUI.color = Color.white;
        style = new GUIStyle();
        style.fontSize = 16; //change the font size
        style.normal.textColor = Color.white;

        if (server.openSocket == true)
            GUILayout.Label("Server: ON", style);
        else
            GUILayout.Label("Server: OFF", style);
        GUILayout.Label("Cameras running: " + camerasController.getNumCameras(), style);
        GUILayout.Label("Clients connected: " + server.getNumClients(), style);
        GUILayout.Label("FPS: " + fps.getFPS().ToString("#.##"), style);

    }
    // close application
    void OnApplicationQuit()
    {

        if (threadGestor != null)
        {
            gestor.RequestStop();
            threadGestor.Join();
        }

        // wait fpr listening thread to terminate 
        if (threadServer != null)
        {
            server.stopListening();
            server.closeSocket();
            threadServer.Join();
        }
        Debug.Log("App closed successfully");
    }

    public void addBufferCameraToCommandsGestor(BufferCamera buffer)
    {
        gestor.addBufferCamera(buffer);
    }

    public void addMiniMapInfoStruct(MinimapSceneInfo info)
    {
        gestor.addMiniMap(info);
    }
}
