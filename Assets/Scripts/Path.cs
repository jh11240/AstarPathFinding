using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path 
{
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;

    public Path(Vector3[] lookPoints, Vector3 startPos , float turnDist, float stoppingDst)
    {
        this.lookPoints = lookPoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex= turnBoundaries.Length-1;

        Vector2 previousPoint = V3ToV2(startPos);
        for(int i = 0; i < lookPoints.Length; i++)
        {
            Vector2 currentPoint= V3ToV2(lookPoints[i]);
            Vector2 dirToCurrentPoint= (currentPoint - previousPoint).normalized;
            //i가 lookPoints의 마지막 인덱스일때 해당벡터에서 turnDIst를 뺀 벡터로 향하는게 아닌 마지막 벡터로 곧장 가야함.  
            Vector2 turnBoundaryPoint =(i==finishLineIndex)? currentPoint : currentPoint - dirToCurrentPoint * turnDist;
            //만약 turnDist가 previousPoint~ currentPoint까지의 거리보다 더 길어버리면 Line에서 approachside를 잘못 지정함. 
            //따라서 turnBoundaryPoint에서 previousPoint로 가는게 아니라 previousPoint - turnDist값으로 가야함
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDist);
            previousPoint = turnBoundaryPoint;
        }
        float dstFromEndPoint = 0;
        //끝점에서부터 시작점까지 순회하며 stoppingDst보다 큰 첫번째 index가 속도줄어드는 구간의 시작
        for(int i = lookPoints.Length - 1; i > 0; i--)
        {
            dstFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);
            if(dstFromEndPoint > stoppingDst)
            {
                slowDownIndex = i;
                break;
            }
        }
    }
    //게임 시점에서 y축을 나타내는 축이 z축이다.
    private Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }
    public void DrawWithGizmos()
    {
        Gizmos.color = Color.black;
        foreach(Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.up, Vector3.one);
        }
        Gizmos.color = Color.white;
        foreach(Line l in turnBoundaries)
        {
            l.DrawWithGizmos(10);
        }
    }
}
