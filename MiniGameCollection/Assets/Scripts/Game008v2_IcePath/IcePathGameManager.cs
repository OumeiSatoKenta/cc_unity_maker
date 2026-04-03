using System.Collections;
using UnityEngine;

namespace Game008v2_IcePath
{
    public enum GameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class IcePathGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private IceBoardManager _boardManager;
        [SerializeField] private IcePathUI _ui;

        private GameState _state = GameState.WaitingInstruction;
        private int _score;
        private int _moveCount;

        public bool IsPlaying => _state == GameState.Playing;
        public int Score => _score;
        public int MoveCount => _moveCount;

        private void Start()
        {
            if (_stageManager == null)
            {
                Debug.LogError("[IcePathGameManager] _stageManager is not assigned.");
                return;
            }

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("008", "IcePath",
                    "氷の上を滑って全マスを通過する一筆書きパズル",
                    "スワイプで移動方向を指定（上下左右）",
                    "全ての氷マスを1回ずつ通過しよう");
            }
            else
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            _score = 0;
            _moveCount = 0;
            if (_ui != null) _ui.UpdateScore(0);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            _state = GameState.Playing;
            _moveCount = 0;

            if (_boardManager != null)
                _boardManager.SetupStage(stageIndex);

            if (_ui != null)
            {
                _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
                _ui.UpdateScore(_score);
                _ui.UpdateMoveCount(0);
                _ui.HideAllPanels();
            }
        }

        private void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            if (_boardManager != null) _boardManager.SetActive(false);
            if (_ui != null) _ui.ShowClearPanel(_score);
        }

        public void OnMoved(int remainingCells)
        {
            if (_state != GameState.Playing) return;
            _moveCount++;
            if (_ui != null)
            {
                _ui.UpdateMoveCount(_moveCount);
                _ui.UpdateRemaining(remainingCells);
            }
        }

        public void OnStageClear(int minMoves)
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;

            int stageMultiplier = _stageManager.CurrentStage + 1;
            int baseScore = 1000 * stageMultiplier;
            bool perfect = _moveCount <= minMoves;
            int bonus = perfect ? 500 : 0;
            // Efficiency bonus: fewer moves = more points
            float efficiency = _moveCount > 0 ? Mathf.Clamp01((float)minMoves / _moveCount) : 1f;
            int gained = Mathf.RoundToInt(baseScore * efficiency) + bonus;
            _score += gained;

            int stars = _moveCount <= minMoves ? 3 : _moveCount <= minMoves + 2 ? 2 : 1;

            if (_ui != null)
            {
                _ui.UpdateScore(_score);
                _ui.ShowStageClearPanel(_stageManager.CurrentStage + 1, _score, stars);
            }
        }

        public void OnGameOver()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.GameOver;
            if (_ui != null) _ui.ShowGameOverPanel();
        }

        public void OnNextStageButtonPressed()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetryButtonPressed()
        {
            if (_state != GameState.GameOver && _state != GameState.Playing) return;
            _moveCount = 0;
            if (_boardManager != null) _boardManager.ResetBoard();
            _state = GameState.Playing;
            if (_ui != null)
            {
                _ui.UpdateMoveCount(0);
                _ui.HideAllPanels();
            }
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu");
        }

        private void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }
    }
}
