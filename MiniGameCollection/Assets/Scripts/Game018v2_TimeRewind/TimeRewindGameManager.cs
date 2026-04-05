using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game018v2_TimeRewind
{
    public class TimeRewindGameManager : MonoBehaviour
    {
        public enum GameState
        {
            WaitingInstruction,
            Playing,
            StageClear,
            Clear,
            GameOver
        }

        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BoardManager _boardManager;
        [SerializeField] TimeRewindUI _ui;

        GameState _state = GameState.WaitingInstruction;
        int _totalScore;
        int _combo;
        int _currentStage;

        public GameState State => _state;
        public int TotalScore => _totalScore;
        public int Combo => _combo;

        void Start()
        {
            _instructionPanel.OnDismissed += StartGame;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            _instructionPanel.Show(
                "018v2",
                "TimeRewind",
                "行き詰まったら時間を巻き戻して別ルートを試そう",
                "スワイプで移動、⏪ボタンで時間を戻す",
                "巻き戻し回数を節約しつつゴールに到達しよう"
            );
        }

        void StartGame()
        {
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _currentStage = stage;
            _state = GameState.Playing;
            var config = _stageManager.GetCurrentStageConfig();
            _boardManager.SetupStage(config, stage + 1);
            _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.HideAllPanels();
        }

        void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            _ui.ShowClearPanel(_totalScore);
        }

        public void OnStageClear(int rewindsUsed, int rewindsAllowed, int moveCount, int optimalMoves)
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;

            int baseScore = 1000 * (_currentStage + 1);
            float noRewindBonus = (rewindsUsed == 0) ? baseScore * 3.0f : 0f;
            float rewindRemainingBonus = (rewindsAllowed - rewindsUsed) * 200f;
            float shortestBonus = (moveCount <= optimalMoves) ? baseScore * 0.5f : 0f;

            _combo++;
            float comboMul = _combo >= 3 ? 1.5f : _combo >= 2 ? 1.2f : 1.0f;
            int stageScore = Mathf.RoundToInt((baseScore + noRewindBonus + rewindRemainingBonus + shortestBonus) * comboMul);
            _totalScore += stageScore;

            _ui.UpdateScore(_totalScore);

            int stars = rewindsUsed == 0 ? 3 : rewindsUsed == 1 ? 2 : 1;
            _ui.ShowStageClearPanel(stageScore, _combo, stars);
        }

        public void OnGameOver()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.GameOver;
            _combo = 0;
            _ui.ShowGameOverPanel();
        }

        public void OnNextStage()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            if (_state != GameState.GameOver && _state != GameState.StageClear) return;
            _state = GameState.Playing;
            _combo = 0;
            var config = _stageManager.GetCurrentStageConfig();
            _boardManager.SetupStage(config, _currentStage + 1);
            _ui.HideAllPanels();
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }

        public void ShowInstructions()
        {
            _instructionPanel.Show(
                "018v2",
                "TimeRewind",
                "行き詰まったら時間を巻き戻して別ルートを試そう",
                "スワイプで移動、⏪ボタンで時間を戻す",
                "巻き戻し回数を節約しつつゴールに到達しよう"
            );
        }
    }
}
