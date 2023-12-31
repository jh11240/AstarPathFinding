using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node:IHeapItem<Node>
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int movePenalty;

    public int gCost;
    public int hCost;
    public Node Parent;
    public int fCost { get { return gCost + hCost; } }

    public Node(bool tmpWalkable, Vector3 tmpWorldPosition,int tmpGridX, int tmpGridY, int tmpMovePenalty)
    {
        walkable = tmpWalkable;
        worldPosition = tmpWorldPosition;
        gridX = tmpGridX;
        gridY = tmpGridY;
        movePenalty = tmpMovePenalty;
    }
    public int heapIndex { get; set; }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return compare;
    }
}