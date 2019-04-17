using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiangMad.AI
{
    public class AStarMng : MonoBehaviour
    {
        public Vector2 worldSize;
        public int gridWidth;
        public int gridHeight;

        private Vector2 m_spacing;
        private Node[] m_nodes;
        private Vector3 m_localOrigin;
        private Vector3 m_worldOrigin;

        public void Generate()
        {
            gridWidth = Mathf.Max(2, gridWidth);
            gridHeight = Mathf.Max(2, gridHeight);

            m_spacing.x = worldSize.x / (gridWidth - 1);
            m_spacing.y = worldSize.y / (gridHeight - 1);

            m_nodes = new Node[gridWidth * gridHeight];

            m_localOrigin = -worldSize * 0.5f;
            m_worldOrigin = transform.position + transform.TransformVector(m_localOrigin);

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var pos = new Vector3(x * m_spacing.x, y * m_spacing.y);
                    var finalPos = m_worldOrigin + transform.TransformVector(pos);

                    int index = y * gridWidth + x;
                    m_nodes[index] = new Node(index, x, y, finalPos);
                }
            }
        }

        private List<Vector3> path;

        public Node WorldPosToNode(Vector3 pos)
        {
            Vector3 inv = transform.InverseTransformVector(pos);

            float xRatio = (inv.x - m_localOrigin.x) / worldSize.x;
            float yRatio = (inv.y - m_localOrigin.y) / worldSize.y;

            xRatio = Mathf.Clamp01(xRatio);
            yRatio = Mathf.Clamp01(yRatio);

            int xIndex = Mathf.RoundToInt(xRatio * (gridWidth - 1));
            int yIndex = Mathf.RoundToInt(yRatio * (gridHeight - 1));

            return GetNode(xIndex, yIndex);
        }

        public List<Vector3> CalculatePath(Vector3 from, Vector3 to)
        {
            List<Node> open = new List<Node>();
            List<Node> close = new List<Node>();

            Node startNode = WorldPosToNode(from);
            Node endNode = WorldPosToNode(to);

            open.Add(startNode);

            while (open.Count > 0)
            {
                var current = GetLowestCostNode(open);

                open.Remove(current);
                close.Add(current);

                if (current == endNode)
                {
                    return RetracePath(startNode, current);
                }

                var neighbours = GetNeighbours(current);

                foreach (var n in neighbours)
                {
                    if (!n.isWalkable || close.Contains(n))
                        continue;
                    
                    var newGC = current.gCost + GetDistance(current, n);

                    if (newGC < n.gCost || !open.Contains(n))
                    {
                        n.gCost = newGC;
                        n.hCost = GetDistance(n, endNode);
                        n.parentNode = current;

                        if (!open.Contains(n))
                            open.Add(n);
                    }
                }
            }

            return null;
        }

        private List<Vector3> RetracePath(Node startNode, Node endNode)
        {
            List<Vector3> retval = new List<Vector3>();

            Node current = endNode;

            while (current != startNode)
            {
                retval.Add(current.worldPosition);
                current = current.parentNode;
            }

            retval.Add(startNode.worldPosition);

            retval.Reverse();

            return retval;
        }

        private Node GetLowestCostNode(List<Node> open)
        {
            Node retval = open[0];

            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].fCost < retval.fCost)
                {
                    retval = open[i];
                }
                else if (open[i].fCost == retval.fCost)
                {
                    if (open[i].hCost < retval.hCost)
                        retval = open[i];
                }
            }

            return retval;
        }

        public int GetDistance(Node nodeA, Node nodeB)
        {
            int dx = Mathf.Abs(nodeA.x - nodeB.x);
            int dy = Mathf.Abs(nodeA.y - nodeB.y);

            if (dx > dy)
                return 14 * dy + 10 * (dx - dy);

            return 14 * dx + 10 * (dy - dx);
        }

        public Node GetNode(int x, int y)
        {
            if (m_nodes == null)
                return null;

            return m_nodes[y * gridWidth + x];
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> retval = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int xIndex = node.x + x;
                    int yIndex = node.y + y;

                    if (xIndex < 0 || xIndex >= gridWidth)
                        continue;

                    if (yIndex < 0 || yIndex >= gridHeight)
                        continue;

                    retval.Add(GetNode(xIndex, yIndex));
                }
            }

            return retval;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.TransformVector(worldSize));
        }
    }
}