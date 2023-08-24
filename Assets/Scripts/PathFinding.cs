using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathFinding : MonoBehaviour
{
    PathRequestManager requestManager;

    private Agrid agrid;
    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        agrid = GetComponent<Agrid>();
    }

    public void StartFindPath(Vector3 startNode, Vector3 endNode)
    {
        StartCoroutine(FindPath(startNode,endNode));
    }

    /// <summary>
    /// startPos에서 targetPos까지 a* 알고리즘을 통해 최적의 경로를 찾아(list<node>형태) retracepath함수를 호출해 agrid클래스에 넘겨줌
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="targetPos"></param>
    private IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw= new Stopwatch();
        sw.Start();

        Vector3[] wayPoints=new Vector3[0];
        bool pathSuccess=false;

        Node startNode = agrid.GetNodeFromWorldPoint(startPos);
        Node targetNode = agrid.GetNodeFromWorldPoint(targetPos);

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(agrid.MaxSize);
            //List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                ////open Set에서 fcost가 제일작은 값을 찾아 curNode 에 넣음, fcost가 같을땐 hcost가 더 작은 값을 고름
                //Node curNode = openSet[0];
                //for (int i = 1; i < openSet.Count; i++)
                //{
                //    if (openSet[i].fCost < curNode.fCost || (openSet[i].fCost == curNode.fCost && openSet[i].hCost < curNode.hCost))
                //    {
                //        curNode = openSet[i];
                //    }
                //}
                //openSet.Remove(curNode);
                Node curNode = openSet.RemoveFirst();
                closedSet.Add(curNode);

                if (curNode == targetNode)
                {
                    sw.Stop();
                    UnityEngine.Debug.Log("path found " + sw.ElapsedMilliseconds + "ms");
                    pathSuccess = true;
                    break;
                }

                foreach (Node elem in agrid.GetNeighbours(curNode))
                {
                    //통과하지 못하거날 이미 처리한 노드라면 contineu
                    if (!elem.walkable || closedSet.Contains(elem)) continue;

                    int newGCost = curNode.gCost + GetDistance(elem, curNode);
                    if (newGCost < elem.gCost || !openSet.Contains(elem))
                    {
                        elem.gCost = newGCost;
                        elem.hCost = GetDistance(elem, targetNode);
                        elem.Parent = curNode;

                        if (!openSet.Contains(elem))
                        {
                            openSet.Add(elem);
                        }
                    }
                }
            }
        }
         yield return null;
        if (pathSuccess)
        {
            wayPoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(wayPoints, pathSuccess);
    }
    /// <summary>
    /// StartNode부터 endNode까지 a* 알고리즘으로 찾은 경로(list<Node>)를 agrid의 path에 넣어준다.
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node curNode = endNode;
        while (curNode != startNode)
        {
            path.Add(curNode);
            curNode = curNode.Parent;
        }
        Vector3[] wayPoints = SimplifyPath(path);
        Array.Reverse(wayPoints);
        return wayPoints;
    }
    /// <summary>
    /// 모든 노드벡터를 다 지닐 필요는 없다. 방향이 바뀔때만 저장
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> wayPoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        for(int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if(directionOld!= directionNew)
            {
                //삽질
                wayPoints.Add(path[i].worldPosition);
                directionOld = directionNew;
            }
            //만약 일직선이라면 어떻게 되는지 체크
        }
        return wayPoints.ToArray();
    }
    /// <summary>
    /// 노드끼리 바로 옆에 붙어있으면 길이를 10으로 가정하면, 대각선은 10루트2라서 대충 14로 정함.
    /// A와 B의 최소길이는 A부터 B까지 대각선으로 먼저가서 x축이나 y축을 맞춘 후 남은 거리만큼 직선거리로 가면됨.
    /// 이 거리가 distY가 더 작을때 14*distY + 10(distX-distY) 로 표현됨
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns></returns>
    private int GetDistance(Node A, Node B)
    {
        int distX = Mathf.Abs(A.gridX - B.gridX);
        int distY = Mathf.Abs(A.gridY - B.gridY);

        if (distX > distY)
            return 14 * distY + 10*(distX - distY);
        return 14 * distX + 10* (distY - distX);
    }
}
