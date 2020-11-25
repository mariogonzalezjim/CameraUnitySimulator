/**
 * \file    TCPServerAsyn.cs
 *
 * \brief   Implements an asynchronous TCP server.
 */


using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;
using System.Net;
using System.Collections.Generic;


/**
 * \class   TCPServerAsyn
 *
 * \brief   An asynchronous TCP server for multiple clients. It Waits for new clients while can receiving and sending data.
 *
 * \author  Mario Gonzalez
 * \date    10/11/2016
 */
public class TCPServerAsyn
{



    //a true/false variable for connection status
    public bool socketReady = false;
    public bool openSocket = false;

    List<NetworkStream> theStream = new List<NetworkStream>();
    List<StreamWriter> theWriter = new List<StreamWriter>();
    List<StreamReader> theReader = new List<StreamReader>();
    //Para el servidos
    TcpListener serversocket;
    //port for the server, make sure to unblock this in your router firewall if you want to allow external connections
    Int32 port = 8889;
    IPAddress localAddr = IPAddress.Parse("127.0.0.1");
    Byte[] bytesserver = new Byte[1024];
    //String data = null;
    volatile bool flagconexion = true;
    // 0 is a client
    static int numClients = -1;



    /**
     * \fn  public setupSocket()
     *
     * \brief   Open the socket and start the server. The server enters in listening loop where it waits for new clients.
     *
     */
    public void setupSocket()
    {
        try
        {

            //serversocket = new TcpListener(localAddr, port);
            serversocket = new TcpListener(port);

            // Start listening for client requests.
            serversocket.Start();

            // Buffer for reading data
            bytesserver = new Byte[512];

            openSocket = true;
            // Enter the listening loop.
            while (flagconexion)
            {

                // Perform a blocking call to accept requests.
                TcpClient client = serversocket.AcceptTcpClient();
               
                numClients++;
                
                client.NoDelay = true;
                theStream.Add(client.GetStream());
                theWriter.Add(new StreamWriter(theStream[numClients]));

                theReader.Add(new StreamReader(theStream[numClients]));
                socketReady = true;
                //reply to client with its ID
                string data = numClients.ToString() + "\0";
                writeSocket(data, numClients);
                Debug.Log("Client connected to server!");
            }
        }
        catch (Exception e)
        {
            Debug.Log("Socket error:" + e);
        }
        finally
        {
            serversocket.Stop();
        }
    }


    /**
     * \fn  public writeSocket(string data, int cliente)
     *
     * \brief   send message to client.
     *
     * \param   data The data we want to send.
     * \param   client The client's identifier.
     */
    public void writeSocket(string data, int client)
    {
        if (!socketReady)
            return;


        //var stopwatch = new System.Diagnostics.Stopwatch();
        //stopwatch.Start();
        //theWriter[cliente].Write(data, 0, data.Length);
        //theWriter[cliente].Flush();
        //stopwatch.Stop();
        //var elapsed_time = stopwatch.ElapsedMilliseconds;
        //if (elapsed_time > 1)
        //    Debug.Log(elapsed_time + " " + cliente);

        // Option 2: Asyn
        //var stopwatch = new System.Diagnostics.Stopwatch();
        //stopwatch.Start();
        theStream[client].BeginWrite(System.Text.Encoding.ASCII.GetBytes(data), 0, System.Text.Encoding.ASCII.GetBytes(data).Length, new AsyncCallback(this.OnWriteComplete),
                                                   theStream[client]);
        //stopwatch.Stop();
        //var elapsed_time = stopwatch.ElapsedMilliseconds;
        //if (elapsed_time > 1)
        //    Debug.Log(elapsed_time + " " + cliente);

    }

    private void OnWriteComplete(IAsyncResult ar)
    {
        //networkStream.EndWrite(ar);
    }

    /**
     * \fn  public readSocket(int client)
     *
     * \brief   Check and read if data has been received for a specific client.
     *
     * \param   client The client's identifier.
     */
    public string readSocket(int client)
    {
        String result = "";

        if (theStream.Count < client)
            return result;

        if (theStream[client].DataAvailable)
        {
            int length = theStream[client].Read(bytesserver, 0, bytesserver.Length);
            byte[] bytesread = new byte[length];
            Buffer.BlockCopy(bytesserver, 0, bytesread, 0, length);
            result = System.Text.Encoding.ASCII.GetString(bytesread);
            theStream[client].Flush();
        }


        return result;
    }

    /**
     * \fn  public  closeSocket()
     *
     * \brief   Close socket.
     *
     */
    public void closeSocket()
    {
        if (!socketReady)
            return;
        for (int i = 0; i < theReader.Count; i++)
        {
            theWriter[i].Close();
            theReader[i].Close();
        }

        // mySocket.Close();
        serversocket.Stop();
        socketReady = false;
    }

    /**
     * \fn  public  maintainConnection()
     *
     * \brief   Keep connection alive, reconnect if connection lost.
     *
     */

    public void maintainConnection()
    {
        for (int i = 0; i < theStream.Count; i++)
            if (!theStream[i].CanRead)
            {
                setupSocket();
            }
    }

    /**
     * \fn  public  getNumClients()
     *
     * \brief   Return the number of clients connected.
     *
     */
    public int getNumClients()
    {
        return numClients + 1;
    }

    /**
    * \fn  public  stopListening()
    *
    * \brief   Stop listening for new clients.
    *
    */
    public void stopListening()
    {
        serversocket.Stop();
        flagconexion = false;
    }

    /**
    * \fn  public  deleteClient(int client)
    *
    * \brief   Delete a client connected.
    *
    * \param   client The client's identifier.
    */
    public void deleteClient(int client)
    {
        String str = "Deleting client ID=" + client.ToString();
        Debug.Log(str);
        theStream.RemoveAt(client);
        theWriter.RemoveAt(client);
        theReader.RemoveAt(client);
        numClients--;
    }
}