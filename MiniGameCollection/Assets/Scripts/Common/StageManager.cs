using UnityEngine;
using System;

/// <summary>
/// 5ステージ進行を管理する汎用コンポーネント。
/// 各ゲームのGameManagerがイベントを購読してステージ再構築を行う。
/// </summary>
public class StageManager : MonoBehaviour
{
    [SerializeField, Tooltip("総ステージ数")] private int _totalStages = 5;

    /// <summary>ステージごとの難易度設定</summary>
    [Serializable]
    public struct StageConfig
    {
        public float speedMultiplier;
        public int countMultiplier;
        public float complexityFactor;
        public string stageName;
    }

    private int _currentStage;
    private StageConfig[] _configs;

    /// <summary>ステージ変更時に発火（引数: 新ステージインデックス 0-based）</summary>
    public event Action<int> OnStageChanged;

    /// <summary>全ステージクリア時に発火</summary>
    public event Action OnAllStagesCleared;

    public int CurrentStage => _currentStage;
    public int TotalStages => _totalStages;
    public bool IsAllCleared => _currentStage >= _totalStages;

    private void Awake()
    {
        InitializeDefaultConfigs();
    }

    private void InitializeDefaultConfigs()
    {
        _configs = new StageConfig[_totalStages];
        float[] speeds = { 1.0f, 1.2f, 1.5f, 1.8f, 2.2f };
        int[] counts = { 1, 1, 2, 2, 3 };
        float[] complexity = { 0.0f, 0.2f, 0.4f, 0.7f, 1.0f };

        for (int i = 0; i < _totalStages; i++)
        {
            _configs[i] = new StageConfig
            {
                speedMultiplier = i < speeds.Length ? speeds[i] : 1.0f + i * 0.3f,
                countMultiplier = i < counts.Length ? counts[i] : 1 + i / 2,
                complexityFactor = i < complexity.Length ? complexity[i] : Mathf.Clamp01(i * 0.25f),
                stageName = $"Stage {i + 1}"
            };
        }
    }

    /// <summary>
    /// カスタムステージ設定を上書きする。ゲーム固有の難易度カーブに使用。
    /// </summary>
    public void SetConfigs(StageConfig[] configs)
    {
        _configs = configs;
        _totalStages = configs.Length;
    }

    /// <summary>現在のステージ設定を取得</summary>
    public StageConfig GetCurrentStageConfig()
    {
        if (_configs == null || _currentStage >= _configs.Length)
            return new StageConfig { speedMultiplier = 1f, countMultiplier = 1, complexityFactor = 0f, stageName = "Stage ?" };
        return _configs[_currentStage];
    }

    /// <summary>指定ステージの設定を取得</summary>
    public StageConfig GetStageConfig(int stageIndex)
    {
        if (_configs == null || stageIndex < 0 || stageIndex >= _configs.Length)
            return new StageConfig { speedMultiplier = 1f, countMultiplier = 1, complexityFactor = 0f, stageName = "Stage ?" };
        return _configs[stageIndex];
    }

    /// <summary>最初のステージから開始</summary>
    public void StartFromBeginning()
    {
        _currentStage = 0;
        OnStageChanged?.Invoke(_currentStage);
    }

    /// <summary>指定ステージにジャンプ</summary>
    public void GoToStage(int stageIndex)
    {
        _currentStage = Mathf.Clamp(stageIndex, 0, _totalStages - 1);
        OnStageChanged?.Invoke(_currentStage);
    }

    /// <summary>
    /// 現在のステージをクリアして次へ進む。
    /// 全ステージクリア時は OnAllStagesCleared を発火。
    /// </summary>
    public void CompleteCurrentStage()
    {
        _currentStage++;
        if (_currentStage >= _totalStages)
        {
            OnAllStagesCleared?.Invoke();
        }
        else
        {
            OnStageChanged?.Invoke(_currentStage);
        }
    }
}
