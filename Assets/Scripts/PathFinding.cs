using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public Transform seeker, target;

    private Agrid agrid;
    private void Awake()
    {
        agrid = GetComponent<Agrid>();
    }
    private void Update()
    {
        if(Input.GetButtonDown("Jump"))
        FindPath(seeker.position, target.position);
    }
    /// <summary>
    /// startPos���� targetPos���� a* �˰����� ���� ������ ��θ� ã��(list<node>����) retracepath�Լ��� ȣ���� agridŬ������ �Ѱ���
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="targetPos"></param>
    private void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw= new Stopwatch();
        sw.Start();
        Node startNode = agrid.GetNodeFromWorldPoint(startPos);
        Node targetNode = agrid.GetNodeFromWorldPoint(targetPos);

        Heap<Node> openSet = new Heap<Node>(agrid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            //open Set���� fcost�� �������� ���� ã�� curNode �� ����, fcost�� ������ hcost�� �� ���� ���� ��
            //Node curNode = openSet[0];
            //for(int i = 1; i < openSet.Count; i++)
            //{
            //    if (openSet[i].fCost< curNode.fCost || (openSet[i].fCost==curNode.fCost && openSet[i].hCost < curNode.hCost))
            //    {
            //        curNode = openSet[i];
            //    }
            //}
            Node curNode = openSet.RemoveFirst();
            closedSet.Add(curNode);

            if (curNode == targetNode)
            {
                sw.Stop();
                UnityEngine.Debug.Log("path found " + sw.ElapsedMilliseconds+"ms");
                RetracePath(startNode, targetNode) ;
                return;
            }

            foreach(Node elem in agrid.GetNeighbours(curNode))
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
    /// <summary>
    /// StartNode���� endNode���� a* �˰������� ã�� ���(list<Node>)�� agrid�� path�� �־��ش�.
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node curNode = endNode;
        while (curNode != startNode)
        {
            path.Add(curNode);
            curNode = curNode.Parent;
        }
        path.Reverse();
        agrid.path = path;
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
