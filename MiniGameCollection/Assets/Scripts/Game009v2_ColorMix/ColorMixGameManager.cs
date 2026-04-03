using System.Collections;
using UnityEngine;

namespace Game009v2_ColorMix
{
    public enum GameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class ColorMixGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private ColorMixManager _colorMixManager;
        [SerializeField] private ColorMixUI _ui;

        private GameState _state = GameState.WaitingInstruction;
        private int _score;
        private int _combo;

        public bool IsPlaying => _state == GameState.Playing;
        public int Score => _score;

        private void Start()
        {
            if (_stageManager == null)
            {
                Debug.LogError("[ColorMixGameManager] _stageManager is not assigned.");
                return;
            }

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("009", "ColorMix",
                    "スライダーで色を混ぜて目標の色を再現するパズル",
                    "R/G/Bスライダーをドラッグして色を調整",
                    "目標色にできるだけ近い色を作ろう");
            }
            else
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            _score = 0;
            _combo = 0;
            if (_ui != null) _ui.UpdateScore(0);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            _state = GameState.Playing;

            if (_colorMixManager != null)
                _colorMixManager.SetupStage(stageIndex);

            if (_ui != null)
            {
                _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
                _ui.UpdateScore(_score);
                _ui.UpdateCombo(_combo);
                _ui.HideAllPanels();
                _ui.SetupSliders(stageIndex >= 2); // brightness slider from stage 3
                _ui.ShowJudgeCount(stageIndex >= 3, _colorMixManager != null ? _colorMixManager.MaxJudgments : -1,
                    _colorMixManager != null ? _colorMixManager.JudgmentsLeft : -1);
            }
        }

        private void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            if (_colorMixManager != null) _colorMixManager.SetActive(false);
            if (_ui != null) _ui.ShowClearPanel(_score);
        }

        public void OnJudgeResult(float deltaE, float allowedDeltaE, int judgesUsed)
        {
            if (_state != GameState.Playing) return;

            bool cleared = deltaE <= allowedDeltaE;

            if (cleared)
            {
                _state = GameState.StageClear;
                _combo++;

                int stageMultiplier = _stageManager.CurrentStage + 1;
                int baseScore = Mathf.RoundToInt(Mathf.Max(0f, 100f - deltaE) * 10f * stageMultiplier);
                float comboMultiplier = 1f + (_combo - 1) * 0.2f;
                float masterBonus = (judgesUsed == 1 && deltaE <= 5f) ? 3.0f : 1.0f;
                int gained = Mathf.RoundToInt(baseScore * comboMultiplier * masterBonus);
                _score += gained;

                int stars = deltaE <= 5f ? 3 : deltaE <= 15f ? 2 : 1;

                if (_ui != null)
                {
                    _ui.UpdateScore(_score);
                    _ui.UpdateCombo(_combo);
                    _ui.ShowStageClearPanel(_stageManager.CurrentStage + 1, _score, stars, Mathf.RoundToInt(deltaE));
                }
            }
            else
            {
                bool outOfJudges = _colorMixManager != null && _colorMixManager.MaxJudgments > 0
                    && _colorMixManager.JudgmentsLeft <= 0;
                if (outOfJudges)
                {
                    OnGameOver();
                }
                else
                {
                    if (_ui != null)
                    {
                        _ui.ShowJudgeCount(_stageManager.CurrentStage >= 3,
                            _colorMixManager != null ? _colorMixManager.MaxJudgments : -1,
                            _colorMixManager != null ? _colorMixManager.JudgmentsLeft : -1);
                    }
                }
            }
        }

        public void OnGameOver()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.GameOver;
            _combo = 0;
            if (_colorMixManager != null) _colorMixManager.SetActive(false);
            if (_ui != null) _ui.ShowGameOverPanel();
        }

        public void OnNextStageButtonPressed()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetryButtonPressed()
        {
            if (_state != GameState.GameOver) return;
            _state = GameState.Playing;
            if (_colorMixManager != null)
            {
                _colorMixManager.ResetForRetry();
            }
            if (_ui != null)
            {
                _ui.HideAllPanels();
                _ui.ShowJudgeCount(_stageManager.CurrentStage >= 3,
                    _colorMixManager != null ? _colorMixManager.MaxJudgments : -1,
                    _colorMixManager != null ? _colorMixManager.JudgmentsLeft : -1);
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
