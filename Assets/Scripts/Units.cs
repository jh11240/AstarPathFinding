using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Units : MonoBehaviour
{
    public Transform target;
    private float speed=5f;
    Vector3[] path;
    int targetIndex=0;

    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");    
            StartCoroutine("FollowPath");    
        }
    }
    private IEnumerator FollowPath()
    {
        Vector3 currentWayPoint = path[0];

        while (true)
        {
            if(transform.position == currentWayPoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                    yield break;
                currentWayPoint = path[targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWayPoint, speed * Time.deltaTime);
            yield return null;
        }
    }
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for(int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                    Gizmos.DrawLine(path[i - 1], path[i]);
                Gizmos.DrawCube(path[i], Vector3.one);
            }
        }
    }
}
