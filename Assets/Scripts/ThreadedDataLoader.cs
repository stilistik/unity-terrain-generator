using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ThreadedDataLoader : MonoBehaviour
{
    static ThreadedDataLoader instance;
    Queue<ThreadInfo> mapDataQueue = new Queue<ThreadInfo>();

    void Awake()
    {
        instance = FindObjectOfType<ThreadedDataLoader>();
    }

    void Update()
    {
        lock (mapDataQueue)
        {
            if (mapDataQueue.Count > 0)
            {
                while (mapDataQueue.Count > 0)
                {
                    ThreadInfo threadInfo = mapDataQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }

        }
    }

    public static void RequestData(Func<object> generateData, System.Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.DataThread(generateData, callback);
        };

        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, System.Action<object> callback)
    {
        object data = generateData();
        ThreadInfo threadInfo = new ThreadInfo(data, callback);
        lock (mapDataQueue)
        {
            mapDataQueue.Enqueue(threadInfo);
        }
    }

}


public struct ThreadInfo
{
    public object parameter;
    public System.Action<object> callback;

    public ThreadInfo(object parameter, System.Action<object> callback)
    {
        this.parameter = parameter;
        this.callback = callback;
    }
}

