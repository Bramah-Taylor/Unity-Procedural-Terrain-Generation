using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

// Class for managing threading using function objects: one for the thread's functionality, and another as a callback function
public class ThreadedDataRequester : MonoBehaviour
{
	static ThreadedDataRequester instance;
	Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

	void Awake()
    {
		instance = FindObjectOfType<ThreadedDataRequester>();
	}

    // Set up thread using input function object and callback function object
	public static void RequestData(Func<object> generateData, Action<object> callback)
    {
		ThreadStart threadStart = delegate 
        {
			instance.DataThread(generateData, callback);
		};

        // Begin execution on the new thread
		new Thread(threadStart).Start();
	}

    // Function wrapper to encapsulate the genericized functionality of the input function object
	void DataThread(Func<object> generateData, Action<object> callback)
    {
        // Wait for data to be generated, then add this thread to the queue to indicate that we're ready to call the callback function
		object data = generateData();
		lock (dataQueue)
        {
			dataQueue.Enqueue(new ThreadInfo(callback, data));
		}
	}
		

	void Update()
    {
        // Check if there are any thread objects waiting
		if (dataQueue.Count > 0)
        {
            // If there are any, remove them from the queue and call their callback function
			for (int i = 0; i < dataQueue.Count; i++)
            {
				ThreadInfo threadInfo = dataQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}

	struct ThreadInfo
    {
		public readonly Action<object> callback;
		public readonly object parameter;

		public ThreadInfo (Action<object> callback, object parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}
