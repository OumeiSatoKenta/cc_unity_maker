using UnityEngine;
using System;

namespace Game003v2_GravitySwitch
{
    public enum GameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }
    public enum GravityDirection { Up, Down, Left, Right }

    public class GravitySwitchGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private GravityManager _gravityManager;
        [SerializeField] private GravitySwitchUI _ui;

        private GameState _state = GameState.WaitingInstruction;
        private int _score;
        private int _comboCount;

        public bool IsPlaying => _state == GameState.Playing;
        public GameState State => _state;
        public int Score => _score;

        private void Start()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _gravityManager.OnMovesChanged += (moves, limit) => _ui?.UpdateMoves(moves, limit);

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("003", "GravitySwitch",
                    "重力を切り替えてボールをゴールに導くパズル",
                    "4方向ボタンで重力切替",
                    "ボールをゴールまで転がそう");
            }
            else
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            _score = 0;
            _comboCount = 0;
            if (_ui != null) _ui.UpdateScore(_score);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            _state = GameState.Playing;
            _gravityManager.SetupStage(stageIndex);
            if (_ui != null)
            {
                _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
                _ui.HideAllPanels();
            }
        }

        private void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            if (_ui != null) _ui.ShowClearPanel(_score);
        }

        public void OnGravityButton(GravityDirection dir)
        {
            if (_state != GameState.Playing) return;
            _gravityManager.ApplyGravity(dir);
        }

        public void OnReachGoal(int movesUsed, int minMoves, int moveLimit)
        {
            if (_state != GameState.Playing) return;

            _comboCount++;
            int maxMoves = moveLimit > 0 ? moveLimit : 20;
            int moveBonus = Mathf.Max(0, maxMoves - movesUsed) * 100;
            int stageNum = _stageManager.CurrentStage + 1;
            float comboMultiplier = 1f + (_comboCount - 1) * 0.2f;
            int stageScore = (int)((moveBonus + 500) * comboMultiplier * stageNum);
            _score += stageScore;

            int starRating = movesUsed <= minMoves ? 3
                           : movesUsed <= minMoves + 2 ? 2 : 1;

            _state = GameState.StageClear;
            if (_ui != null)
            {
                _ui.UpdateScore(_score);
                _ui.ShowStageClearPanel(stageNum, stageScore, starRating);
            }
        }

        public void OnFallIntoHole()
        {
            if (_state != GameState.Playing) return;
            _comboCount = 0;
            _state = GameState.GameOver;
            if (_ui != null) _ui.ShowGameOverPanel();
        }

        public void OnMoveLimitExceeded()
        {
            if (_state != GameState.Playing) return;
            _comboCount = 0;
            _state = GameState.GameOver;
            if (_ui != null) _ui.ShowGameOverPanel();
        }

        public void OnNextStageButtonPressed()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RestartStage()
        {
            if (_state != GameState.GameOver && _state != GameState.Playing) return;
            _state = GameState.Playing;
            _gravityManager.ResetStage();
            if (_ui != null) _ui.HideAllPanels();
        }

        public void RestartGame()
        {
            _score = 0;
            _comboCount = 0;
            if (_ui != null) _ui.UpdateScore(_score);
            if (_ui != null) _ui.HideAllPanels();
            _stageManager.StartFromBeginning();
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
            {
                _instructionPanel.OnDismissed -= StartGame;
            }
        }
    }
}
