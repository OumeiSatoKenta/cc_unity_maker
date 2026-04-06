using UnityEngine;

namespace Game063v2_StarMiner
{
    public class StarMinerGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] MiningManager _miningManager;
        [SerializeField] StarMinerUI _ui;

        public enum GameState { Idle, Playing, StageClear, AllClear }
        public GameState State { get; private set; } = GameState.Idle;

        void Start()
        {
            _instructionPanel.Show(
                "063v2",
                "StarMiner",
                "宇宙で鉱石を掘って宇宙船を強化しよう",
                "タップで採掘、ボタンでアップグレード",
                "採掘目標を達成して新しい星系を開拓しよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            State = GameState.Playing;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            int stage = stageIndex + 1; // 0-based → 1-based
            var config = _stageManager.GetCurrentStageConfig();
            _miningManager.SetupStage(config, stage);
            _ui.UpdateStage(stage, _stageManager.TotalStages);
            State = GameState.Playing;
        }

        void OnAllStagesCleared()
        {
            State = GameState.AllClear;
            _ui.ShowAllClear();
        }

        public void OnStageClear()
        {
            if (State != GameState.Playing) return;
            State = GameState.StageClear;
            int displayStage = _stageManager.CurrentStage + 1;
            _ui.ShowStageClear(displayStage, _stageManager.TotalStages);
        }

        public void NextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed -= StartGame;
            }
        }
    }
}
