using UnityEngine;

public static class HexMath
{
    /// <summary>
    /// 立方坐标转世界坐标（尖顶布局）
    /// </summary>
    public static Vector3 QrsToWorld(Vector3Int qrs, float hexSize)
    {
        float x = hexSize * (Mathf.Sqrt(3) * qrs.x + Mathf.Sqrt(3)/2 * qrs.y);
        float z = hexSize * (3f/2f * qrs.y);
        return new Vector3(x, 0, z);
    }

    /// <summary>
    /// 世界坐标转立方坐标
    /// </summary>
    public static Vector3Int WorldToQrs(Vector3 worldPos, float hexSize)
    {
        float q = (worldPos.x * Mathf.Sqrt(3)/3 - worldPos.z / 3) / hexSize;
        float r = (worldPos.z * 2/3) / hexSize;
        return RoundToQrs(new Vector3(q, r, -q - r));
    }

    // 在HexMath.cs中添加
    public static Vector3Int WorldToQrs(Vector3 position)
    {
        return WorldToQrs(position, 1f); // 使用默认hexSize
    }

    private static Vector3Int RoundToQrs(Vector3 fractional)
    {
        int q = Mathf.RoundToInt(fractional.x);
        int r = Mathf.RoundToInt(fractional.y);
        int s = Mathf.RoundToInt(fractional.z);

        float qDiff = Mathf.Abs(q - fractional.x);
        float rDiff = Mathf.Abs(r - fractional.y);
        float sDiff = Mathf.Abs(s - fractional.z);

        if (qDiff > rDiff && qDiff > sDiff)
            q = -r - s;
        else if (rDiff > sDiff)
            r = -q - s;
        else
            s = -q - r;

        return new Vector3Int(q, r, s);
    }

     /// <summary>
    /// 验证坐标是否有效
    /// </summary>
    public static bool IsValidQrs(Vector3Int qrs)
    {
        return qrs.x + qrs.y + qrs.z == 0;
    }

    /// <summary>
    /// 获取两个坐标之间的距离
    /// </summary>
    public static int Distance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    }
}