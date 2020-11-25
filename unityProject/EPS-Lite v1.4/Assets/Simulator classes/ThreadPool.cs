/**
 * \file    ThreadPool.cs
 *
 * \brief   Implements the Thread Pool Class.
 */


/**
 * \class   ThreadPool
 *
 * \brief   With this class, two or more threads can share data using a ThreadPool.
 *
 * \author  Mario Gonzalez
 * \date    10/11/2016
 */

public class ThreadPool  {

    bool poolReady=false;
    volatile string queueData = "";

    /**
     * \fn  public ThreadPool()
     *
     * \brief   Default constructor.
     *
     */

    public ThreadPool() {

    } 


    /**
     * \fn  public bool getStatusPool()
     *
     * \brief   Gets status pool.
     *
     *
     * \return  True if threadpool is activated, false if not.
     */

    public bool getStatusPool() {
        return poolReady;
    }

    /**
     * \fn  public bool readIncomingData()
     *
     * \brief   Read data stored in the buffer and flush it.
     *
     *
     * \return  Data.
     */
    public string readIncomingData() {
        
        //copy to an auxiliar buffer
        string aux = queueData;
        //flush buffer
        queueData = "";
        return aux;
    }

    /**
     * \fn  public void addData(string newdata)
     *
     * \brief   Adds data to the threadpool buffer if threadpool is marked as active.
     *
     *
     * \param   newdata The new data it'll be add to pool buffer.
     */

    public void addData(string newdata) {
        if (poolReady == false)
            return;
        queueData += newdata;
    }

    /**
     * \fn  public void setPoolActive()
     *
     * \brief   Sets pool as active and ready to use.
     *
     */

    public void setPoolActive() {
        poolReady = true;
    }

    /**
     * \fn  public void setPoolInactive()
     *
     * \brief   Sets pool as inactive. Please note you can't write on the pool buffer data if it's inactive.
     *
     */

    public void setPoolInactive() {
        poolReady = false;
    }

}
