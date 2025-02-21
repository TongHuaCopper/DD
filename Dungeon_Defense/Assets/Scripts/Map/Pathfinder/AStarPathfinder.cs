using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    // 六边形的六个邻居方向（qrs坐标系）
    private static readonly Vector3Int[] Directions =
    {
        new Vector3Int(+1, -1, 0),  // 东
        new Vector3Int(+1, 0, -1),  // 东北
        new Vector3Int(0, +1, -1),  // 西北
        new Vector3Int(-1, +1, 0),  // 西
        new Vector3Int(-1, 0, +1),  // 西南
        new Vector3Int(0, -1, +1)   // 东南
    };

    /// <summary>
    /// 执行A*路径查找主方法
    /// </summary>
    /// <param name="start">起点坐标</param>
    /// <param name="end">终点坐标</param>
    /// <param name="unitWeight">单位重量</param>
    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end, int unitWeight)
    {
        HexGridManager grid = GetComponent<HexGridManager>();
        if (grid == null)
        {
            Debug.LogError("找不到HexGridManager组件！");
            return null;
        }

        // ----------------- 初始化数据结构 -----------------
        Dictionary<Vector3Int, Node> nodeMap = new Dictionary<Vector3Int, Node>();  // 所有已探索节点
        Heap<Node> openSet = new Heap<Node>(grid.gridRadius * grid.gridRadius * 6);  // 待探索节点（使用堆优化）
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();                   // 已处理节点

        // 初始化起点
        Node startNode = new Node(start)
        {
            gCost = 0,  // 起点到自身的代价为0
            hCost = CalculateH(start, end)  // 启发式估计代价
        };
        openSet.Add(startNode);
        nodeMap.Add(start, startNode);

        int maxIterations = 10000;  // 安全阀值，防止无限循环
        int currentIteration = 0;

        // ----------------- 主循环 -----------------
        while (openSet.Count > 0 && currentIteration++ < maxIterations)
        {
            // 取出当前最优节点
            Node currentNode = openSet.RemoveFirst();

            // 找到终点时回溯路径
            if (currentNode.position == end)
            {
                return RetracePath(startNode, currentNode);
            }

            // 将当前节点标记为已处理
            closedSet.Add(currentNode.position);

            // 遍历所有邻居方向
            foreach (Vector3Int dir in Directions)
            {
                Vector3Int neighborPos = currentNode.position + dir;

                // ---------- 邻居有效性检查 ----------
                // 1. 检查是否存在该地块
                if (!grid._hexMap.ContainsKey(neighborPos)) continue;

                Hexagon neighborHex = grid._hexMap[neighborPos];

                // 2. 检查是否可承载单位重量
                if (!neighborHex.CanAcceptWeight(unitWeight)) continue;

                // 3. 检查是否不可通行（如山脉）
                if (neighborHex.currentMoveCost == int.MaxValue) continue;

                // 4. 检查是否已处理过
                if (closedSet.Contains(neighborPos)) continue;

                // ---------- 计算移动代价 ----------
                int moveCost = neighborHex.currentMoveCost;  // 地形基础代价
                int tentativeGCost = currentNode.gCost + moveCost;

                // ---------- 获取或创建节点 ----------
                if (!nodeMap.TryGetValue(neighborPos, out Node neighborNode))
                {
                    neighborNode = new Node(neighborPos);
                    nodeMap.Add(neighborPos, neighborNode);
                }

                // ---------- 路径优化逻辑 ----------
                if (tentativeGCost < neighborNode.gCost || !openSet.Contains(neighborNode))
                {
                    neighborNode.parent = currentNode;     // 更新父节点
                    neighborNode.gCost = tentativeGCost;   // 更新实际代价
                    neighborNode.hCost = CalculateH(neighborPos, end);  // 更新启发代价

                    // 维护堆结构
                    if (openSet.Contains(neighborNode))
                    {
                        openSet.UpdateItem(neighborNode);  // 已有节点更新位置
                    }
                    else
                    {
                        openSet.Add(neighborNode);         // 新节点加入开放集
                    }
                }
            }
        }

        Debug.LogWarning($"未找到路径，迭代次数：{currentIteration}");
        return null;
    }

    /// <summary>
    /// 计算启发式代价（六边形曼哈顿距离）
    /// 公式：(|q1-q2| + |r1-r2| + |s1-s2|) / 2
    /// </summary>
    private int CalculateH(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + 
               Mathf.Abs(a.y - b.y) + 
               Mathf.Abs(a.z - b.z)) / 2;
    }

    /// <summary>
    /// 回溯生成路径（从终点到起点）
    /// </summary>
    private List<Vector3Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Node currentNode = endNode;

        // 通过父节点指针回溯
        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;

            // 安全保护
            if (currentNode == null)
            {
                Debug.LogError("路径回溯时遇到空父节点！");
                break;
            }
        }

        path.Reverse();  // 反转得到从起点到终点的路径
        return path;
    }

    /// <summary>
    /// 路径节点类（实现堆接口）
    /// </summary>
    private class Node : IHeapItem<Node>
    {
        public Vector3Int position;  // 六边形坐标
        public int gCost;            // 起点到当前节点的实际代价
        public int hCost;            // 当前节点到终点的估计代价
        public Node parent;          // 父节点（用于回溯路径）
        public int FCost => gCost + hCost;  // 总优先级

        public Node(Vector3Int pos)
        {
            position = pos;
        }

        // 堆索引接口实现
        public int HeapIndex { get; set; }

        /// <summary>
        /// 比较方法（决定节点在堆中的顺序）
        /// 1. 优先比较总代价FCost
        /// 2. FCost相同时比较hCost
        /// </summary>
        public int CompareTo(Node other)
        {
            int compare = FCost.CompareTo(other.FCost);
            return compare == 0 ? hCost.CompareTo(other.hCost) : compare;
        }
    }
}