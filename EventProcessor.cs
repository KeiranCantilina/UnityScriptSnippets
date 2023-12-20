using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventProcessor : MonoBehaviour
{
    public Text TextDebug;
    public Transform CubeOrientation;

    private System.Object _queueLock = new System.Object();
    
    List<byte[]> _queuedData = new List<byte[]>();
    List<byte[]> _processingData = new List<byte[]>();

    public void QueueData(byte[] data)
    {
        lock (_queueLock)
        {
            _queuedData.Add(data);
        }
    }


    void Update()
    {
        MoveQueuedEventsToExecuting();
        while (_processingData.Count > 0)
        {
            var byteData = _processingData[0];
            _processingData.RemoveAt(0);
            try
            {
                var IMUData = IMU_DataPacket.ParseDataPacket(byteData);
                TextDebug.text = IMUData.ToString();
                //UPDATE CUBE ORIENTATION HERE
                CubeOrientation.rotation.Set(IMUData.rX, IMUData.rY, IMUData.rZ, IMUData.rW); 
            }
            catch (Exception e)
            {
                TextDebug.text = "Error: " + e.Message;
            }
        }
    }


    private void MoveQueuedEventsToExecuting()
    {
        lock (_queueLock)
        {
            while (_queuedData.Count > 0)
            {
                byte[] data = _queuedData[0];
                _processingData.Add(data);
                _queuedData.RemoveAt(0);
            }
        }
    }
}