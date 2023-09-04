using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Line
{
    const float verticalLineGradient = 1e5f;

    float gradient;
    //y=ax+b 에서 b를 말하는 부분 y축과 닿아서 y_intercept라고 하나봄
    float y_intercept;

    //현재 Line위의 점 두개
    Vector2 pointOnLine_1;
    Vector2 pointOnLine_2;

    float gradientPerpendicular;
    bool approachSide;

    //점두개로 선분구성하는 함수
    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
    {
        float dx= pointOnLine.x - pointPerpendicularToLine.x;
        float dy= pointOnLine.y - pointPerpendicularToLine.y;

        
        if (dx == 0)
        {
            gradientPerpendicular = verticalLineGradient;
        }
        else
        {
            gradientPerpendicular = dy / dx;

        }

        if (gradientPerpendicular == 0)
        {
            gradient = verticalLineGradient;
        }
        else
        {
            gradient = -1 / gradientPerpendicular;
        }

        y_intercept = pointOnLine.y - gradient * pointOnLine.x;
        pointOnLine_1 = pointOnLine;
        pointOnLine_2 = pointOnLine + new Vector2(1,gradient) ;

        approachSide = false;
        approachSide = GetSide(pointPerpendicularToLine);
    }

    bool GetSide(Vector2 p)
    {
        return (p.x-pointOnLine_1.x) * (pointOnLine_2.y- pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x- pointOnLine_1.x);
    }

    public bool HasCrossedLine(Vector2 p)
    {
        return GetSide(p) != approachSide;
    }

    public float DistanceFromPoint(Vector2 p)
    {
        float yInterceptPerpendicular = p.y - gradientPerpendicular * p.x;
        float intersectX = (yInterceptPerpendicular - y_intercept) / (gradient - gradientPerpendicular);
        float intersectY = gradient * intersectX + y_intercept;
        return Vector2.Distance(p, new Vector2(intersectX,intersectY));
    }
    public void DrawWithGizmos(float length)
    {
        //기울기 방향으로 라인
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;

        Vector3 lineCentre = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
        //lineCentre에서 length만큼 좌우로 그리는 연산
        Gizmos.DrawLine(lineCentre- lineDir*length/2f, lineCentre+ lineDir*length/2f);
    }
}
