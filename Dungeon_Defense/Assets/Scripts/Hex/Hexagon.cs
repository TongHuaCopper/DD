using TMPro;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    // ----------------------------- 坐标系设置 -----------------------------
    public Vector3Int qrsCoord; // 立方坐标系 q + r + s = 0

    // ----------------------------- 地形系统 -----------------------------
    public enum TerrainType
    {
        Grassland = 0,  // 草原
        Sand = 1,       // 沙地
        Forest = 2,     // 森林
        Mountain = 3    // 山脉（始终不可通行）
    }

    [System.Serializable]
    public struct TerrainConfig
    {
        public TerrainType type;
        public Material material;
        public int moveCost;
    }

    [Header("地形配置")]
    public TerrainConfig[] terrainConfigs;

    [Header("当前状态")]
    public TerrainType currentTerrain;
    public int currentMoveCost;

    // ----------------------------- 称重系统 -----------------------------
    [Header("称重设置")]
    public int maxWeightCapacity = 10;  // 最大承载重量
    public int currentWeight = 0;       // 当前承载重量

    // ----------------------------- 可视化设置 -----------------------------
    private MeshRenderer _renderer_center;
    private MeshRenderer _renderer_base;
    private Material _base_originalMat;
    [SerializeField] private Material _highlightMat;
    [SerializeField] private Material _pathHighlightMat;

    [Header("调试信息")]
    [SerializeField] private TMP_Text coordinateText; // 需在预制体添加TextMesh子物体



    void Awake()
    {
        _renderer_center = transform.Find("Center/ChangeColor").GetComponent<MeshRenderer>();
        _renderer_base = transform.Find("Base/ChangeColor").GetComponent<MeshRenderer>();
        _base_originalMat = _renderer_base.material;
    }

    // ----------------------------- 公有方法 -----------------------------
    public void Initialize(Vector3Int coord)
    {
        qrsCoord = coord;
        RandomizeTerrain();
        UpdateDebugInfo();
    }

    public void UpdateDebugInfo()
    {
        coordinateText.text = $"{qrsCoord.x},{qrsCoord.y},{qrsCoord.z}";
        coordinateText.gameObject.SetActive(Debug.isDebugBuild);
    }

    public void RandomizeTerrain()
    {
        float rand = Random.value;
        if (rand < 0.2f) SetTerrain(TerrainType.Sand);
        else if (rand < 0.5f) SetTerrain(TerrainType.Forest);
        else if (rand < 0.55f) SetTerrain(TerrainType.Mountain);
        else SetTerrain(TerrainType.Grassland);
    }

    public void SetTerrain(TerrainType type)
    {
        currentTerrain = type;
        foreach (var config in terrainConfigs)
        {
            if (config.type == type)
            {
                currentMoveCost = type == TerrainType.Mountain ?
                    int.MaxValue : config.moveCost;
                _renderer_center.material = config.material;
                break;
            }
        }
    }

    /// <summary>
    /// 检查是否可以承载指定重量的单位
    /// </summary>
    public bool CanAcceptWeight(int unitWeight)
    {
        // 山脉始终不可通行
        if (currentTerrain == TerrainType.Mountain) return false;

        // 计算预测总重量
        int projectedWeight = currentWeight + unitWeight;

        // 判断是否超载
        bool willOverload = projectedWeight > maxWeightCapacity;

        return willOverload ?
            (unitWeight < 2) : // 超载时只允许重量<2的单位
            true;              // 未超载时允许任何单位
    }

    public void SetHighlight() => _renderer_base.material = _highlightMat;
    public void SetPathHighlight() => _renderer_base.material = _pathHighlightMat;
    public void ResetVisual()
    {
        _renderer_base.material = _base_originalMat;
    }
}