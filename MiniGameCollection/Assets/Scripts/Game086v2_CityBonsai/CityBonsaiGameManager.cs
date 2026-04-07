using UnityEngine;

namespace Game086v2_CityBonsai
{
    public class CityBonsaiGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CityBonsaiManager _bonsaiManager;
        [SerializeField] CityBonsaiUI _ui;

        int _combo;
        float _scoreMultiplier;
        int _totalScore;
        bool _isPlaying;

        void Start()
        {
            _instructionPanel.Show(
                "086",
                "CityBonsai",
                "盆栽の中に小さな都市を育てよう",
                "建物ボタンで選択、枝をタップで配置\n剪定ボタンで枝を整えて美しさUP",
                "人口と美しさを両立して理想の盆栽都市を作ろう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _instructionPanel.OnDismissed -= StartGame;
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _totalScore = 0;
            _isPlaying = true;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _combo = 0;
            _scoreMultiplier = 1.0f;
            _isPlaying = true;
            var config = _stageManager.GetCurrentStageConfig();
            _bonsaiManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        void OnAllStagesCleared()
        {
            _isPlaying = false;
            _bonsaiManager.SetActive(false);
            _ui.ShowAllClear(_totalScore);
        }

        public void OnActionScore(int baseScore, bool isPrune)
        {
            if (!_isPlaying) return;

            if (isPrune)
            {
                _combo++;
                _scoreMultiplier = _combo >= 4 ? 2.0f
                                 : _combo >= 3 ? 1.5f
                                 : _combo >= 2 ? 1.3f
                                 : 1.0f;
            }
            else
            {
                _combo = 0;
                _scoreMultiplier = 1.0f;
            }

            int earned = Mathf.RoundToInt(baseScore * _scoreMultiplier);
            _totalScore += earned;
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_combo, _scoreMultiplier);
        }

        public void OnBothGoalsMet()
        {
            if (!_isPlaying) return;
            // Both population + beauty achieved → bonus + stage clear
            int bonus = Mathf.RoundToInt(_totalScore * 0.5f);
            _totalScore += bonus;
            _ui.UpdateScore(_totalScore);
            _ui.ShowMessage($"両立ボーナス！ +{bonus}pt");

            _isPlaying = false;
            _bonsaiManager.SetActive(false);
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void OnGameOver()
        {
            _isPlaying = false;
            _bonsaiManager.SetActive(false);
            _ui.ShowGameOver();
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
