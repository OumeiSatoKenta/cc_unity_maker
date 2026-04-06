using UnityEngine;
using System.Collections;

namespace Game062v2_MagicForest
{
    public class MagicForestGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] ForestManager _forestManager;
        [SerializeField] MagicForestUI _ui;

        public enum GameState { Idle, Playing, StageClear, GameClear }
        GameState _state = GameState.Idle;

        void Start()
        {
            _instructionPanel.Show(
                "062v2",
                "MagicForest",
                "木を育てて魔法の森を広げよう",
                "木をタップして育てる、魔力でアップグレードを購入",
                "森の面積を目標まで広げてステージクリア"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            if (_instructionPanel != null) _instructionPanel.OnDismissed -= StartGame;
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
        }

        void StartGame()
        {
            _state = GameState.Playing;

            var configs = new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 0f,   countMultiplier = 10,  complexityFactor = 0f,  stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 0.2f, countMultiplier = 25,  complexityFactor = 0f,  stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 0.4f, countMultiplier = 50,  complexityFactor = 0.3f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 0.6f, countMultiplier = 80,  complexityFactor = 0.6f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 0.8f, countMultiplier = 120, complexityFactor = 1.0f, stageName = "Stage 5" },
            };
            _stageManager.SetConfigs(configs);

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _forestManager.SetupStage(stageIndex, config);
            _ui.UpdateStageDisplay(stageIndex + 1);
        }

        void OnAllStagesCleared()
        {
            _state = GameState.GameClear;
            _ui.ShowGameClear(_forestManager.TotalTrees);
        }

        public void OnStageClear()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;
            _ui.ShowStageClear(_forestManager.TotalTrees);
        }

        public void OnNextStage()
        {
            if (_state != GameState.StageClear) return;
            _state = GameState.Playing;
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            _state = GameState.Playing;
            _ui.HideGameClear();
            _forestManager.ResetAll();
            _stageManager.StartFromBeginning();
        }

        public void OnWorldTreeCompleted()
        {
            // Stage 5 world tree completion → go directly to game clear via stageManager
            if (_state != GameState.Playing) return;
            _stageManager.CompleteCurrentStage(); // triggers OnAllStagesCleared
        }

        public bool IsPlaying => _state == GameState.Playing;
    }
}
