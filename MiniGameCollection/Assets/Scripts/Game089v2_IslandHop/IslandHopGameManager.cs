using UnityEngine;

namespace Game089v2_IslandHop
{
    public class IslandHopGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] IslandManager _islandManager;
        [SerializeField] IslandHopUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;

        public int CurrentCombo => _combo;
        public float ScoreMultiplier => _scoreMultiplier;
        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;

        void Start()
        {
            _instructionPanel.Show(
                "089",
                "IslandHop",
                "島を開拓してリゾートアイランドを作ろう",
                "島をタップして選択、建設スロットをタップして施設を配置",
                "複数の島を開発して最高のリゾートを完成させよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _isPlaying = true;
            var config = _stageManager.GetCurrentStageConfig();
            _islandManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _islandManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        public void OnFacilityBuilt(int facilityScore, bool hasSynergy, int synergyCount)
        {
            if (!_isPlaying) return;
            _combo++;
            _scoreMultiplier = _combo >= 4 ? 2.0f
                             : _combo >= 3 ? 1.6f
                             : _combo >= 2 ? 1.3f
                             : 1.0f;

            int synergyBonus = synergyCount >= 3 ? 100
                             : synergyCount >= 2 ? 50
                             : synergyCount >= 1 ? 20
                             : 0;

            // Stage 5: synergy bonus x1.5
            if (_currentStageIndex >= 4)
                synergyBonus = Mathf.RoundToInt(synergyBonus * 1.5f);

            int earned = Mathf.RoundToInt(facilityScore * _scoreMultiplier) + synergyBonus;
            _totalScore += earned;

            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowBuildFeedback(earned, hasSynergy);

            CheckStageClear();
        }

        public void OnGuestRequestFulfilled(int bonusScore)
        {
            if (!_isPlaying) return;
            _totalScore += bonusScore;
            _ui.UpdateScore(_totalScore);
        }

        public void OnWeatherPenalty(int penalty)
        {
            if (!_isPlaying) return;
            _totalScore = Mathf.Max(0, _totalScore - penalty);
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        public void OnResourceHarvested(int resourceValue)
        {
            if (!_isPlaying) return;
            // Resources don't directly add score but enable building
        }

        void CheckStageClear()
        {
            int targetScore = _islandManager.GetStageTargetScore(_currentStageIndex);
            if (_totalScore >= targetScore)
            {
                _isPlaying = false;
                _islandManager.SetActive(false);
                _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
            }
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void OnGameOver()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _islandManager.SetActive(false);
            _ui.ShowGameOver(_totalScore);
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
