using UnityEngine;

namespace Game090v2_StarshipCrew
{
    public class StarshipCrewGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CrewManager _crewManager;
        [SerializeField] StarshipCrewUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;
        int _missionsClearedThisStage;
        int _consecutiveFailures;

        // Stage clear requirements per stage (0-indexed)
        static readonly int[] RequiredClears = { 2, 3, 4, 5, 6 };
        static readonly int[] FailureLimits  = { 3, 3, 3, 2, 2 };

        public int CurrentCombo => _combo;
        public float ScoreMultiplier => _scoreMultiplier;
        public bool IsPlaying => _isPlaying;
        public int GetCurrentScore() => _totalScore;
        public int GetCurrentStageIndex() => _currentStageIndex;
        public int GetMissionsClearedThisStage() => _missionsClearedThisStage;
        public int GetRequiredClears() => RequiredClears[Mathf.Clamp(_currentStageIndex, 0, RequiredClears.Length - 1)];

        void Start()
        {
            _instructionPanel.Show(
                "090",
                "StarshipCrew",
                "クルーを育てて銀河探検に出発しよう",
                "クルーをタップして選択、ミッションをタップして派遣",
                "最強のクルー編成で全ミッションをクリアしよう"
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
            _missionsClearedThisStage = 0;
            _consecutiveFailures = 0;
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
            _missionsClearedThisStage = 0;
            _consecutiveFailures = 0;
            _isPlaying = true;
            var config = _stageManager.GetCurrentStageConfig();
            _crewManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _crewManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by CrewManager when a mission succeeds.</summary>
        public void OnMissionSuccess(int baseScore, bool isPerfect, int synergyCount, float difficultyMultiplier)
        {
            if (!_isPlaying) return;

            _combo++;
            _consecutiveFailures = 0;
            _scoreMultiplier = _combo >= 4 ? 2.0f
                             : _combo >= 3 ? 1.5f
                             : _combo >= 2 ? 1.2f
                             : 1.0f;

            int synergyBonus = synergyCount >= 3 ? 80
                             : synergyCount >= 2 ? 30
                             : 0;
            int perfectBonus = isPerfect ? 50 : 0;

            // Stage 5: synergy bonus x1.5
            if (_currentStageIndex >= 4)
                synergyBonus = Mathf.RoundToInt(synergyBonus * 1.5f);

            int earned = Mathf.RoundToInt(baseScore * _scoreMultiplier * difficultyMultiplier)
                       + synergyBonus + perfectBonus;
            _totalScore += earned;
            _missionsClearedThisStage++;

            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowMissionResult(true, isPerfect, earned, synergyBonus);

            CheckStageClear();
        }

        /// <summary>Called by CrewManager when a mission fails.</summary>
        public void OnMissionFailed()
        {
            if (!_isPlaying) return;

            _combo = 0;
            _scoreMultiplier = 1.0f;
            _consecutiveFailures++;

            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowMissionResult(false, false, 0, 0);

            int failLimit = FailureLimits[Mathf.Clamp(_currentStageIndex, 0, FailureLimits.Length - 1)];
            if (_consecutiveFailures >= failLimit)
            {
                _isPlaying = false;
                _crewManager.SetActive(false);
                _ui.ShowGameOver(_totalScore);
            }
        }

        void CheckStageClear()
        {
            int required = RequiredClears[Mathf.Clamp(_currentStageIndex, 0, RequiredClears.Length - 1)];
            if (_missionsClearedThisStage >= required)
            {
                _isPlaying = false;
                _crewManager.SetActive(false);
                _ui.ShowStageClear(_currentStageIndex + 1, _totalScore);
            }
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
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
