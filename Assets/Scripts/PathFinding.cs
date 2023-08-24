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
    /// startPos���� targetPos���� a* �˰����� ���� ������ ��θ� ã��(list<node>����) retracepath�Լ��� ȣ���� agridŬ������ �Ѱ���
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
                ////open Set���� fcost�� �������� ���� ã�� curNode �� ����, fcost�� ������ hcost�� �� ���� ���� ��
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
                    //������� ���ϰų� �̹� ó���� ����� contineu
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
    /// StartNode���� endNode���� a* �˰������� ã�� ���(list<Node>)�� agrid�� path�� �־��ش�.
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
    /// ��� ��庤�͸� �� ���� �ʿ�� ����. ������ �ٲ𶧸� ����
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
                //����
                wayPoints.Add(path[i].worldPosition);
                directionOld = directionNew;
            }
            //���� �������̶�� ��� �Ǵ��� üũ
        }
        return wayPoints.ToArray();
    }
    /// <summary>
    /// ��峢�� �ٷ� ���� �پ������� ���̸� 10���� �����ϸ�, �밢���� 10��Ʈ2�� ���� 14�� ����.
    /// A�� B�� �ּұ��̴� A���� B���� �밢������ �������� x���̳� y���� ���� �� ���� �Ÿ���ŭ �����Ÿ��� �����.
    /// �� �Ÿ��� distY�� �� ������ 14*distY + 10(distX-distY) �� ǥ����
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
