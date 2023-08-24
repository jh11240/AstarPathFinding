using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
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

    public static void RequestPath(Vector3 pathStart,Vector3 pathEnd,Action<Vector3[], bool> callBack)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callBack);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }
    private void TryProcessNext()
    {
        if(!isProcessingPath && pathRequestQueue.Count > 0)
        {
            isProcessingPath = true;
            curPathRequest = pathRequestQueue.Dequeue();
            pathFinding.StartFindPath(curPathRequest.pathStart, curPathRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        curPathRequest.callBack(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callBack;

        public PathRequest(Vector3 _pathStart, Vector3 _pathEnd, Action<Vector3[],bool> _callBack)
        {
            pathStart = _pathStart;
            pathEnd = _pathEnd;
            callBack = _callBack;
        }
    }
}
