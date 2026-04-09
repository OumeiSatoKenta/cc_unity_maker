using UnityEngine;

namespace Game100v2_DreamRun
{
    public class DreamRunGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DreamRunManager _dreamRunManager;
        [SerializeField] DreamRunUI _ui;

        int _totalScore;
        bool _isPlaying;
        int _currentStageIndex;
        int _comboCount;
        int _lives;
        const int MaxLives = 3;

        public bool IsPlaying => _isPlaying;
        public int GetTotalScore() => _totalScore;

        void Start()
        {
            if (_stageManager == null || _instructionPanel == null || _dreamRunManager == null || _ui == null)
            {
                Debug.LogError("[DreamRunGameManager] Required fields not assigned.");
                enabled = false;
                return;
            }
            _instructionPanel.Show(
                "100",
                "DreamRun",
                "夢の世界を走り抜けよう",
                "画面左タップでレーン左移動、中央タップでジャンプ、右タップでレーン右移動",
                "夢の断片をすべて集めてストーリーを完成させよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _comboCount = 0;
            _lives = MaxLives;

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _isPlaying = true;
            _comboCount = 0;
            _lives = MaxLives;
            var config = _stageManager.GetCurrentStageConfig();
            _dreamRunManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateLives(_lives, MaxLives);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _dreamRunManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        /// <summary>Called by DreamRunManager when a fragment is collected.</summary>
        public void OnFragmentCollected(bool isNearMiss, int collectedCount, int totalCount)
        {
            if (!_isPlaying) return;

            _comboCount++;
            float comboMultiplier = Mathf.Min(1.0f + _comboCount * 0.1f, 2.0f);
            int baseScore = 100 * (_currentStageIndex + 1);
            int score = Mathf.RoundToInt(baseScore * comboMultiplier);

            if (isNearMiss)
                score += 10 * _comboCount;

            _totalScore += score;
            _ui.UpdateScore(_totalScore);
            _ui.UpdateFragments(collectedCount, totalCount);
            _ui.ShowComboIfNeeded(_comboCount);
        }

        /// <summary>Called by DreamRunManager when all fragments in stage are collected.</summary>
        public void OnStageClear(int stage)
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _dreamRunManager.SetActive(false);
            _ui.ShowStageClear(stage, _totalScore);
        }

        /// <summary>Called by DreamRunManager on obstacle hit.</summary>
        public void OnObstacleHit()
        {
            if (!_isPlaying) return;

            _lives--;
            _comboCount = 0;
            _ui.UpdateLives(_lives, MaxLives);

            if (_lives <= 0)
            {
                _isPlaying = false;
                _dreamRunManager.SetActive(false);
                _ui.ShowGameOver(_totalScore);
            }
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void RestartGame()
        {
            _ui.HideGameOver();
            _totalScore = 0;
            _currentStageIndex = 0;
            _isPlaying = true;
            _comboCount = 0;
            _lives = MaxLives;
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
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
