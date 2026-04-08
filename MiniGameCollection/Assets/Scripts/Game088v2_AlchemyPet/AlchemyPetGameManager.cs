using UnityEngine;

namespace Game088v2_AlchemyPet
{
    public class AlchemyPetGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] AlchemyManager _alchemyManager;
        [SerializeField] AlchemyPetUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;
        int _missCount;
        const int MaxMiss = 3;

        public int CurrentCombo => _combo;

        void Start()
        {
            _instructionPanel.Show(
                "088",
                "AlchemyPet",
                "錬金術でユニークなペットを生み出そう",
                "素材ボタンで選択して投入\n錬金ボタンで合成実行",
                "素材を組み合わせてペット図鑑をコンプリートしよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _missCount = 0;
            _isPlaying = true;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _missCount = 0;
            _isPlaying = true;
            var config = _stageManager.GetCurrentStageConfig();
            _alchemyManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.UpdateMiss(_missCount, MaxMiss);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _alchemyManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        public void OnPetDiscovered(int petId, bool isRare, bool isLegend)
        {
            if (!_isPlaying) return;
            _combo++;
            _scoreMultiplier = _combo >= 4 ? 2.0f
                             : _combo >= 3 ? 1.6f
                             : _combo >= 2 ? 1.3f
                             : 1.0f;

            int baseScore = isLegend ? 100 : isRare ? 50 : 20;
            int earned = Mathf.RoundToInt(baseScore * _scoreMultiplier);
            _totalScore += earned;
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.ShowDiscovery(petId, earned);

            // Check stage clear
            if (_alchemyManager.IsStageGoalMet())
            {
                _isPlaying = false;
                _alchemyManager.SetActive(false);
                _ui.ShowStageClear(_stageManager.CurrentStage + 1);
            }
        }

        public void OnAlreadyKnownRecipe(int petId)
        {
            if (!_isPlaying) return;
            // Known recipe still gives small score, no combo
            int earned = 5;
            _totalScore += earned;
            _combo = Mathf.Max(0, _combo - 1);
            _scoreMultiplier = _combo >= 4 ? 2.0f
                             : _combo >= 3 ? 1.6f
                             : _combo >= 2 ? 1.3f
                             : 1.0f;
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        public void OnExplosion()
        {
            if (!_isPlaying) return;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _missCount++;
            _ui.UpdateCombo(_combo, _scoreMultiplier);
            _ui.UpdateMiss(_missCount, MaxMiss);

            if (_missCount >= MaxMiss)
            {
                _isPlaying = false;
                _alchemyManager.SetActive(false);
                _ui.ShowGameOver();
            }
        }

        public void OnFeedReward(int bonusScore)
        {
            if (!_isPlaying) return;
            _totalScore += bonusScore;
            _ui.UpdateScore(_totalScore);
        }

        public void OnRecipeHintFound(int bonusScore)
        {
            if (!_isPlaying) return;
            _totalScore += bonusScore;
            _ui.UpdateScore(_totalScore);
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
