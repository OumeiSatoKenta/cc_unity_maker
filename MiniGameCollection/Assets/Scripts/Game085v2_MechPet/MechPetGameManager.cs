using UnityEngine;

namespace Game085v2_MechPet
{
    public class MechPetGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] MechPetManager _mechPetManager;
        [SerializeField] MechPetUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;

        void Start()
        {
            _instructionPanel.Show(
                "085",
                "MechPet",
                "メカパーツでロボットペットを組み立てよう",
                "スロットをタップしてパーツ切り替え\nエネルギーをチャージしてロボットを強化",
                "ミッションをクリアして最強のメカペットを目指そう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            // Reset per-stage score tracking
            _combo = 0;
            _scoreMultiplier = 1.0f;
            var config = _stageManager.GetCurrentStageConfig();
            _mechPetManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        void OnAllStagesCleared()
        {
            _mechPetManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        public void OnMissionResult(bool success, int baseScore)
        {
            if (success)
            {
                _combo++;
                _scoreMultiplier = Mathf.Min(1.0f + _combo * 0.1f, 2.0f);
                int earned = Mathf.RoundToInt(baseScore * _scoreMultiplier);
                _totalScore += earned;
                _ui.UpdateScore(_totalScore);
                _ui.UpdateCombo(_combo, _scoreMultiplier);

                int targetScore = _mechPetManager.StageTargetScore;
                if (_totalScore >= targetScore)
                {
                    _mechPetManager.SetActive(false);
                    _ui.ShowStageClear(_stageManager.CurrentStage + 1);
                }
            }
            else
            {
                _combo = 0;
                _scoreMultiplier = 1.0f;
                _ui.UpdateCombo(_combo, _scoreMultiplier);
                _ui.ShowMissionFailed();
            }
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateSynergyDisplay(string text) => _ui.UpdateSynergy(text);
        public void UpdateEnergyDisplay(float normalized) => _ui.UpdateEnergy(normalized);

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
