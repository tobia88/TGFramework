using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiangMad.AI
{
    public class Node
    {
        public bool isWalkable;
        public int fCost { get { return gCost + hCost; } }
        public int gCost;
        public int hCost;
        public int id;
        public Node parentNode;
        public int x;
        public int y;
        public Vector3 worldPosition;

        public Node(int id, int x, int y, Vector3 worldPosition)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.worldPosition = worldPosition;
            this.isWalkable = true;
        }
    }

}