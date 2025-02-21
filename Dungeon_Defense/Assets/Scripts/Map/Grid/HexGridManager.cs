using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    // ----------------------------- 网格生成设置 -----------------------------
    [Header("网格生成设置")]
    public GameObject hexPrefab;
    [Range(3, 20)] public int gridRadius = 5;
    public float hexSize = 1f;

    // ----------------------------- 调试设置 -----------------------------
    [Header("调试设置")]
    public int testUnitWeight = 1; // 测试用单位重量

    // ----------------------------- 运行时数据 -----------------------------
    public Dictionary<Vector3Int, Hexagon> _hexMap = new Dictionary<Vector3Int, Hexagon>();
    private Vector3Int? _startCoord;
    private Vector3Int? _endCoord;

    // ----------------------------- 地图扩展 -----------------------------
    [Header("地图扩展设置")]
    [Range(1, 5)] public int expandRadius = 1;  // 扩展半径
    public Material newHexMaterial;              // 新生成地块的材质

    // 新增方向定义（与A*算法一致）
    private static readonly Vector3Int[] Directions =
    {
        new Vector3Int(+1, -1, 0),  // 东
        new Vector3Int(+1, 0, -1),  // 东北
        new Vector3Int(0, +1, -1),  // 西北
        new Vector3Int(-1, +1, 0),  // 西
        new Vector3Int(-1, 0, +1),  // 西南
        new Vector3Int(0, -1, +1)   // 东南
    };

    private List<Vector3Int> current_path = null;   //当前的寻路路径


    // ----------------------------- 单例模式 -----------------------------
    public static HexGridManager Instance { get; private set; }

    private void Awake()
    {
        // 确保只有一个实例存在
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ----------------------------- 初始化 -----------------------------
    void Start() => GenerateHexGrid();

    // ----------------------------- 鼠标交互 -----------------------------
    void Update()
    {
        HandleMouseInput();
        HandleExpansionInput();     // 对着六边形按空格时扩展地图
        HandleRestPathInput();      // 按下重置键恢复被高光的寻路路径
        HandleStateSwitchInput();   // 切换游戏状态
    }

    // 按P切换游戏状态
    void HandleStateSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameState newState = GameStateManager.CurrentState == GameState.Preparation ?
                GameState.Battle : GameState.Preparation;
            GameStateManager.Instance.SwitchState(newState);
        }
    }

    void GenerateHexGrid()
    {
        _hexMap.Clear();

        // 使用轴向坐标生成算法
        for (int q = -gridRadius; q <= gridRadius; q++)
        {
            int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
            int r2 = Mathf.Min(gridRadius, -q + gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                int s = -q - r;
                CreateHexagon(new Vector3Int(q, r, s));
            }
        }
    }

    // 修改后的创建方法
    void CreateHexagon(Vector3Int qrs, bool isExpanding = false)
    {
        if (!HexMath.IsValidQrs(qrs)) return;

        Vector3 worldPos = HexMath.QrsToWorld(qrs, hexSize);
        GameObject hexObj = Instantiate(hexPrefab, worldPos, Quaternion.identity, transform);
        hexObj.name = $"Hex_{qrs.x}_{qrs.y}_{qrs.z}";

        Hexagon hex = hexObj.GetComponent<Hexagon>();
        hex.Initialize(qrs);

        if (isExpanding)
        {
            hex.SetTerrain(Hexagon.TerrainType.Mountain);
            hex.transform.Find("Center/ChangeColor").GetComponent<Renderer>().material = newHexMaterial;
        }

        _hexMap.Add(qrs, hex);
    }

    // 对着六边形按空格时扩展地图
    void HandleExpansionInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Hexagon targetHex = GetClickedHexagon();
            if (targetHex != null)
            {
                ExpandAroundHexagon(targetHex.qrsCoord);
            }
        }
    }

    // 按下重置键恢复被高光的寻路路径
    void HandleRestPathInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //重置选定的起始点和结束点
            if (_startCoord.HasValue) _hexMap[_startCoord.Value].ResetVisual();
            if (_endCoord.HasValue) _hexMap[_endCoord.Value].ResetVisual();
            _startCoord = null;
            _endCoord = null;

            // 清除旧路径
            if (current_path != null)
            {
                foreach (var coord in current_path)
                    _hexMap[coord].ResetVisual();
            }
        }
    }

    void ExpandAroundHexagon(Vector3Int center)
    {
        List<Vector3Int> coordsToProcess = GetHexRingCoords(center, expandRadius);

        foreach (Vector3Int coord in coordsToProcess)
        {
            if (_hexMap.ContainsKey(coord))
            {
                // 修改现有地块
                ModifyExistingHexagon(coord);
            }
            else
            {
                // 生成新地块
                CreateHexagon(coord, true);
            }
        }
    }

    List<Vector3Int> GetHexRingCoords(Vector3Int center, int radius)
    {
        List<Vector3Int> results = new List<Vector3Int>();
        Vector3Int current = center + Directions[4] * radius;

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                results.Add(current);
                current = GetNeighbor(current, i);
            }
        }
        return results;
    }

    Vector3Int GetNeighbor(Vector3Int coord, int directionIndex)
    {
        return coord + Directions[directionIndex];
    }

    void ModifyExistingHexagon(Vector3Int coord)
    {
        Hexagon hex = _hexMap[coord];
        // 示例修改：随机切换地形（可根据需求修改）
        Hexagon.TerrainType newType = (Hexagon.TerrainType)Random.Range(0, 3);
        hex.SetTerrain(newType);
        hex.UpdateDebugInfo();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        else if (Input.GetMouseButtonDown(1)) HandleRightClick();
    }

    void HandleLeftClick()
    {
        Hexagon hex = GetClickedHexagon();
        if (hex == null) return;

        //Debug.Log($"点击处理开始 - 当前状态: Start={_startCoord}, End={_endCoord}");

         if (hex.currentTerrain == Hexagon.TerrainType.Mountain)
        {
            Debug.LogWarning("不能选择山脉作为起点或终点！");
            return;
        }

        // 设置新选择
        if (!_startCoord.HasValue)
        {
            _startCoord = hex.qrsCoord;
            hex.SetHighlight();
        }
        else
        {
            if (_endCoord.HasValue) _hexMap[_endCoord.Value].ResetVisual();
            _endCoord = hex.qrsCoord;
            hex.SetHighlight();
            FindAndShowPath();
        }
        //Debug.Log($"点击处理结束 - 当前状态: Start={_startCoord}, End={_endCoord}");
    }


    void HandleRightClick()
    {
        Hexagon hex = GetClickedHexagon();
        if (hex == null) return;

        // 循环切换地形
        Hexagon.TerrainType newType = hex.currentTerrain switch
        {
            Hexagon.TerrainType.Grassland => Hexagon.TerrainType.Sand,
            Hexagon.TerrainType.Sand => Hexagon.TerrainType.Forest,
            Hexagon.TerrainType.Forest => Hexagon.TerrainType.Mountain,
            _ => Hexagon.TerrainType.Grassland
        };
        hex.SetTerrain(newType);
    }

    Hexagon GetClickedHexagon()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit) ?
            hit.collider.GetComponent<Hexagon>() : null;
    }

    // ----------------------------- 路径查找 -----------------------------
    void FindAndShowPath()
    {
        if (!_startCoord.HasValue || !_endCoord.HasValue) return;

        // 清除旧路径
        if (current_path != null)
        {
            foreach (var coord in current_path)
                _hexMap[coord].ResetVisual();
        }

        // 执行A*算法
        AStarPathfinder pathfinder = GetComponent<AStarPathfinder>();
        current_path = pathfinder.FindPath(
            _startCoord.Value,
            _endCoord.Value,
            testUnitWeight
        );

        // 显示新路径
        if (current_path != null)
        {
            foreach (Vector3Int coord in current_path)
            {
                Hexagon hex = _hexMap[coord];
                hex.SetPathHighlight();
            }
        }
    }

    // 在HexGridManager类中添加：
    public int GetNeighborCount(Vector3Int coord)
    {
        int count = 0;
        foreach (Vector3Int dir in Directions)
        {
            if (_hexMap.ContainsKey(coord + dir)) count++;
        }
        return count;
    }

    public IEnumerable<Vector3Int> GetNeighborCoords(Vector3Int coord)
    {
        foreach (Vector3Int dir in Directions)
        {
            Vector3Int neighborCoord = coord + dir;
            if (_hexMap.ContainsKey(neighborCoord))
                yield return neighborCoord;
        }
    }

    /// <summary>
    /// 更新地块承重状态
    /// </summary>
    public void UpdateHexWeight(Vector3Int coord, int weightDelta)
    {
        if (_hexMap.TryGetValue(coord, out Hexagon hex))
        {
            hex.currentWeight = Mathf.Max(0, hex.currentWeight + weightDelta);
        }
    }
}