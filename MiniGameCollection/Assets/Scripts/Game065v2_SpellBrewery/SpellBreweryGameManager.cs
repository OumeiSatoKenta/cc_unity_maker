using UnityEngine;

namespace Game065v2_SpellBrewery
{
    public class SpellBreweryGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BreweryManager _breweryManager;
        [SerializeField] SpellBreweryUI _ui;

        public enum GameState { Idle, Playing, StageClear, AllClear }
        public GameState State { get; private set; } = GameState.Idle;

        void Start()
        {
            _instructionPanel.Show(
                "065v2",
                "SpellBrewery",
                "材料を集めて魔法のポーションを作ろう",
                "材料をタップして釜に投入、醸造ボタンでポーション完成、販売してゴールド獲得",
                "ポーション販売目標を達成してステージクリア"
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
            int stage = stageIndex + 1;
            var config = _stageManager.GetCurrentStageConfig();
            _breweryManager.SetupStage(config, stage);
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
