using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agrid : MonoBehaviour
{
    public bool displayGridGizmos;
    public GameObject player;
    public LayerMask UnWalkableLayer;
    public Vector2 gridWorldSize;
    public Node[,] grid;
    public int gridXCnt;
    public int gridYCnt;
    public float nodeRadius;
    private float nodeDiameter;

    public Vector3 worldBottomLeft;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        nodeDiameter = nodeRadius * 2;
        gridXCnt = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridYCnt = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }
    public int MaxSize
    {
        get {
            return gridXCnt * gridYCnt; 
        }
    }
    private void CreateGrid()
    {
        grid = new Node[gridXCnt, gridYCnt];
        //현재 포지션에서 x축으로 gridWorldSize의 x값/2 만큼 빼고 z축으로 gridWorldSize.y/2만큼 빼야함.
        worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int i = 0; i < gridXCnt; i++)
        {
            for (int j = 0; j < gridYCnt; j++)
            {
                Vector3 worldPoint = worldBottomLeft + (i * nodeDiameter + nodeRadius) * Vector3.right + (j * nodeDiameter + nodeRadius) * Vector3.forward;
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, UnWalkableLayer);
                grid[i, j] = new Node(walkable, worldPoint,i,j);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                //자기 자신 노드는 컨티뉴
                if (i == 0 && j == 0) continue;

                int nextX = node.gridX + i;
                int nextY = node.gridY + j;

                if (nextX < 0 || nextY < 0 || nextX >= gridXCnt || nextY >= gridYCnt) continue;
                neighbours.Add(grid[nextX, nextY]);
            }
        }
        return neighbours;
    }
    public Node GetNodeFromWorldPoint(Vector3 worldPos)
    {
        float percentX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPos.z + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridXCnt - 1) * percentX);
        int y = Mathf.RoundToInt((gridYCnt - 1) * percentY);
        return grid[x, y];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (grid != null && displayGridGizmos)
            {
                foreach (Node n in grid)
                {
                    Gizmos.color = (n.walkable) ? Color.green : Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                }
            }
    }
}
