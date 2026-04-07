using UnityEngine;

namespace Game075v2_SoundGarden
{
    public class SoundGardenGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] GardenController _gardenController;
        [SerializeField] SoundGardenUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "075",
                "SoundGarden",
                "植物をリズムに合わせてタップして育てよう",
                "植物が光ったらタイミングよくタップ！Perfect判定で大きく成長するよ",
                "全ての植物を最大成長させてハーモニーを完成させよう"
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
            _gardenController.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _gardenController.SetActive(false);
            _ui.ShowAllClear(_gardenController.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void OnGameOver()
        {
            _gardenController.SetActive(false);
            _ui.ShowGameOver(_gardenController.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateTimerDisplay(float time) => _ui.UpdateTimer(time);
        public void ShowJudgement(string text, UnityEngine.Color color) => _ui.ShowJudgement(text, color);

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
