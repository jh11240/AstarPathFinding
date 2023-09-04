using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Units : MonoBehaviour
{
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public Transform target;
    public float speed=5f;
    public float turnSpeed = 3f;
    public float turnDst=5f;
    //for easying
    public float stoppingDist = 10;

    //Vector3[] path;
    //int targetIndex=0;
    Path path; 

    private void Start()
    {
        StartCoroutine(UpdatePath());
    }
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            //path = newPath;
            //targetIndex = 0;
            path = new Path(newPath, transform.position, turnDst,stoppingDist);
            StopCoroutine("FollowPath");    
            StartCoroutine("FollowPath");    
        }
    }

    private IEnumerator UpdatePath()
    {
        //시작하고 몇프레임동안은 time.deltatime이 비정상적
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(transform.position,target.position, OnPathFound);

        float sqrMoveThresHold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if((target.position-targetPosOld).magnitude > sqrMoveThresHold)
            {
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                targetPosOld=target.position;
            }
        }
    }
    private IEnumerator FollowPath()
    {
        //Vector3 currentWayPoint = path[0];
        bool followingPath = true;
        int pathIndex = 0;
        if (path !=null)
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            //속도를 증가시키면 한프레임당 여러 포인트를 뛰어넘을수있어서 if 를 while로 
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                    pathIndex++;
            }
            if (followingPath)
            {
                //slowDownIndex부터 속도 줄이기시작
                if (pathIndex >= path.slowDownIndex && stoppingDist > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDist);
                    //너무 느려져서 해당 점에 도달하기까지 많이 걸릴수 있으므로 적당히 느려지면 컷
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                    }
                }
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed*speedPercent, Space.Self);
            }
            //if(transform.position == currentWayPoint)
            //{
            //    targetIndex++;
            //    if (targetIndex >= path.Length)
            //        yield break;
            //    currentWayPoint = path[targetIndex];
            //}

            //transform.position = Vector3.MoveTowards(transform.position, currentWayPoint, speed * Time.deltaTime);
            yield return null;
        }
    }
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            //for(int i = targetIndex; i < path.Length; i++)
            //{
            //    Gizmos.color = Color.black;

            //    if (i == targetIndex)
            //    {
            //        Gizmos.DrawLine(transform.position, path[i]);
            //    }
            //    else
            //        Gizmos.DrawLine(path[i - 1], path[i]);
            //    Gizmos.DrawCube(path[i], Vector3.one);
            //}
            path.DrawWithGizmos();
        }
    }
}
