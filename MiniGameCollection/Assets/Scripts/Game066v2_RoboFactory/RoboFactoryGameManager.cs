using UnityEngine;

namespace Game066v2_RoboFactory
{
    public class RoboFactoryGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] FactoryManager _factoryManager;
        [SerializeField] RoboFactoryUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "066",
                "RoboFactory",
                "ロボットを作って都市を建設しよう",
                "ボタンでロボット製造・建設・研究を指示",
                "都市レベル目標を達成してステージクリア"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _factoryManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _factoryManager.SetActive(false);
            _ui.ShowAllClear(_factoryManager.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void AddScore(int delta)
        {
            _ui.UpdateScore(_factoryManager.TotalScore);
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
    }
}
