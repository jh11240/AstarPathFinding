using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathResult> results = new Queue<PathResult>();

    public static PathRequestManager instance;

    private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    private PathRequest curPathRequest;

    private PathFinding pathFinding;

    private bool isProcessingPath=false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
           Destroy(gameObject);
        pathFinding = GetComponent<PathFinding>();
    }
    private void Update()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            lock (results)
            {
                for(int i=0;i < itemsInQueue; ++i)
                {
                    PathResult result = results.Dequeue();
                    result.callBack(result.path, result.success);
                }
            }
        }
    }
    public static void RequestPath(PathRequest request)
    {

        //PathRequest newRequest = new PathRequest(pathStart, pathEnd, callBack);
        //instance.pathRequestQueue.Enqueue(newRequest);
        //instance.TryProcessNext();
        ThreadStart threadStart = delegate
        {
            instance.pathFinding.FindPath(request, instance.FinishedProcessingPath);
        };
        threadStart.Invoke();
    }
    //private void TryProcessNext()
    //{
    //    if(!isProcessingPath && pathRequestQueue.Count > 0)
    //    {
    //        isProcessingPath = true;
    //        curPathRequest = pathRequestQueue.Dequeue();
    //        pathFinding.StartFindPath(curPathRequest.pathStart, curPathRequest.pathEnd);
    //    }
    //}

    public void FinishedProcessingPath(PathResult result)
    {
        //curPathRequest.callBack(path, success);
        //isProcessingPath = false;
        //TryProcessNext();
        //PathResult result = new PathResult(path,success,originalRequest.callBack);
        lock (results)
        {
            results.Enqueue(result);
        }
    }


}
public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callBack;

    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callBack)
    {
        this.path = path;
        this.success = success;
        this.callBack = callBack;
    }
}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callBack;

    public PathRequest(Vector3 _pathStart, Vector3 _pathEnd, Action<Vector3[], bool> _callBack)
    {
        pathStart = _pathStart;
        pathEnd = _pathEnd;
        callBack = _callBack;
    }
}