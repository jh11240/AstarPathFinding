using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public Node Parent;
    public int fCost { get { return gCost + hCost; } }

    public Node(bool tmpWalkable, Vector3 tmpWorldPosition,int tmpGridX, int tmpGridY)
    {
        walkable = tmpWalkable;
        worldPosition = tmpWorldPosition;
        gridX = tmpGridX;
        gridY = tmpGridY;
    }
}