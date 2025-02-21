// GameStateManager.cs
using UnityEngine;

/// <summary>
/// 游戏状态管理器
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // ----------------------------- 单例模式 -----------------------------
    private static GameStateManager _instance;
    public static GameStateManager Instance => _instance;

    // ----------------------------- 状态管理 -----------------------------
    public static GameState CurrentState { get; private set; } = GameState.Preparation;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// 切换游戏状态
    /// </summary>
    public void SwitchState(GameState newState)
    {
        if (CurrentState == newState) return;

        Debug.Log($"切换游戏状态：{CurrentState} -> {newState}");
        CurrentState = newState;
    }
}

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    Preparation, // 准备阶段
    Battle       // 战斗阶段
}