using UnityEngine;

namespace Game064v2_AquaCity
{
    public class AquaCityGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CityManager _cityManager;
        [SerializeField] AquaCityUI _ui;

        public enum GameState { Idle, Playing, StageClear, AllClear }
        public GameState State { get; private set; } = GameState.Idle;

        void Start()
        {
            _instructionPanel.Show(
                "064v2",
                "AquaCity",
                "海底に都市を作って魚を集めよう",
                "建物をタップしてコイン回収、ボタンで建物を購入",
                "人口目標を達成して次のステージへ進もう"
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
            _cityManager.SetupStage(config, stage);
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
