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
    private int penaltyMin = int.MaxValue;
    private int penaltyMax = int.MinValue;
    public int obstacleProximityPenalty = 10;
    public float nodeRadius;
    private float nodeDiameter;
    public TerrainType[] walkableRegions;
    private LayerMask walkableMask;
    private Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();


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
        foreach(TerrainType terrain in walkableRegions)
        {
            walkableMask |= terrain.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(terrain.terrainMask.value,2), terrain.terrainPenalty);
        }
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

                int movePenalty = 0;

                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 100, walkableMask))
                {
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movePenalty);
                }
                if (!walkable)
                {
                    movePenalty += obstacleProximityPenalty;
                }
                grid[i, j] = new Node(walkable, worldPoint,i,j,movePenalty);
            }
        }
        BlurPenaltyMap(3);
    }
    private void BlurPenaltyMap(int blurSize)
    {
        //각 blur노드하나에 생기는 커널. 중심에 정사각형 하나 포함되서 odd여야한다.
        int kernelSize = 2 * blurSize + 1;
        //kernel이 blur노드하나보다 늘어나는 사이즈
        int kernelExtents= (kernelSize-1)/2;

        int[,] penaltiesHorizontalPass = new int[gridXCnt, gridYCnt];
        int[,] penaltiesVerticalPass = new int[gridXCnt, gridYCnt];

        //원래는 각 노드에 주위 8개 노드의 penalty값을 다 더해서 blur을 만듬.
        //8개 노드 구하기보단 처음에 horizontal의 3개씩 더해서 수평값만 다 더한 blur값을 구하고, 
        //해당 값들을 수직으로 더하는 식으로 수평+수직 값을 연산해 계산을 줄임.
        for(int y = 0; y < gridYCnt; y++)
        {
            for(int x= -kernelExtents; x <= kernelExtents;  x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movePenalty;
            }
            for(int x = 1; x < gridXCnt; x++)
            {
                int removeIdx = Mathf.Clamp(x - kernelExtents - 1, 0, gridXCnt);
                int addIdx = Mathf.Clamp(x + kernelExtents, 0, gridXCnt - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIdx, y].movePenalty + grid[addIdx, y].movePenalty;
            }
        }
        for(int x = 0; x < gridXCnt; x++)
        {
            for(int y= -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x,sampleY];
            }
            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movePenalty = blurredPenalty;

            for (int y = 1; y < gridYCnt; y++)
            {
                int removeIdx = Mathf.Clamp(y - kernelExtents - 1, 0, gridYCnt);
                int addIdx = Mathf.Clamp(y + kernelExtents, 0, gridYCnt - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y-1] - penaltiesHorizontalPass[x,removeIdx] + penaltiesHorizontalPass[x,addIdx];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].movePenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax)
                {
                    penaltyMax = blurredPenalty;
                }
                if (blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;
                }
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
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movePenalty));
                    Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
                }
            }
    }
    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}

