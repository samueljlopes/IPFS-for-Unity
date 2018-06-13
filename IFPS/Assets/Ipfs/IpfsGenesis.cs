using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

public class IpfsGenesis : MonoBehaviour {

    public bool Write;
    //If not write, read.

    // Use this for initialization
    void Start()
    {
        if (Write)
        {
            Task t = Extensions.Iterate(WriteInSequence());
        }
        else
        {
            string hash = PlayerPrefs.GetString("StartHash");
            GameObject newobject = new GameObject();
            IpfsGenerated gen = newobject.AddComponent<IpfsGenerated>();

            Thread readthread = new Thread(() => IpfsIO.ReadObject(hash, gen));
            readthread.Start();
        }
    }

    IEnumerable<Task> WriteInSequence()
    {
        IpfsObject[] ipfsObjects = FindObjectsOfType<IpfsObject>();
        var aResult = ipfsObjects[0].Write(this, null);
        yield return aResult;

        for (int i = 1; i < ipfsObjects.Length; i++)
        {
            aResult = ipfsObjects[i].Write(this, aResult.Result);
            yield return aResult;
            if (i == ipfsObjects.Length - 1)
            {
                PlayerPrefs.SetString("StartHash", aResult.Result);
            }
        }
    }

    // Update is called once per frame
    void Update () {
	}
}

static class Extensions
{
    public static Task Iterate(IEnumerable<Task> asyncIterator)
    {
        if (asyncIterator == null) throw new ArgumentNullException("asyncIterator");

        var enumerator = asyncIterator.GetEnumerator();
        if (enumerator == null) throw new InvalidOperationException("Invalid enumerable - GetEnumerator returned null");

        var tcs = new TaskCompletionSource<object>();
        tcs.Task.ContinueWith(_ => enumerator.Dispose(), TaskContinuationOptions.ExecuteSynchronously);

        Action<Task> recursiveBody = null;
        recursiveBody = delegate {
            try
            {
                if (enumerator.MoveNext()) enumerator.Current.ContinueWith(recursiveBody, TaskContinuationOptions.ExecuteSynchronously);
                else tcs.TrySetResult(null);
            }
            catch (Exception exc) { tcs.TrySetException(exc); }
        };

        recursiveBody(null);
        return tcs.Task;
    }
}
