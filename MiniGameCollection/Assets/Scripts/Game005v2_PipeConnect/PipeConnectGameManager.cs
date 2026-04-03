using System.Collections;
using UnityEngine;

namespace Game005v2_PipeConnect
{
    public enum GameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class PipeConnectGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private PipeManager _pipeManager;
        [SerializeField] private PipeConnectUI _ui;

        private GameState _state = GameState.WaitingInstruction;
        private int _score;
        private int _comboCount;
        private float _timeLeft;
        private bool _timerRunning;
        private bool _flowStarted;

        private static readonly float[] _stageTimes = { 90f, 80f, 70f, 60f, 50f };

        public bool IsPlaying => _state == GameState.Playing;

        private void Start()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _pipeManager.OnFlowComplete += OnFlowComplete;

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("005", "PipeConnect",
                    "パイプを回転させて水源から出口まで繋ぐパズル",
                    "タップでパイプを90度回転・水流ボタンで確認",
                    "制限時間内に水を出口まで届けよう");
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
            if (_ui != null) _ui.UpdateScore(0);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            _state = GameState.Playing;
            _flowStarted = false;
            _timeLeft = stageIndex < _stageTimes.Length ? _stageTimes[stageIndex] : 60f;
            _timerRunning = true;

            _pipeManager.SetupStage(stageIndex);

            if (_ui != null)
            {
                _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
                _ui.UpdateScore(_score);
                _ui.UpdateTimer(_timeLeft);
                _ui.HideAllPanels();
            }
        }

        private void OnAllStagesCleared()
        {
            _timerRunning = false;
            _state = GameState.Clear;
            _pipeManager.SetActive(false);
            if (_ui != null) _ui.ShowClearPanel(_score);
        }

        private void Update()
        {
            if (!_timerRunning || _state != GameState.Playing) return;
            _timeLeft -= Time.deltaTime;
            if (_ui != null) _ui.UpdateTimer(Mathf.Max(0f, _timeLeft));

            if (_timeLeft <= 0f)
            {
                _timerRunning = false;
                OnTimerEnd();
            }
        }

        private void OnTimerEnd()
        {
            if (!_flowStarted)
            {
                _flowStarted = true;
                _pipeManager.SetActive(false);
                _pipeManager.StartWaterFlow();
            }
            // _flowStarted == true の場合、水流はすでに進行中なので待機
        }

        private void OnFlowComplete(bool allConnected, int pathLength)
        {
            _pipeManager.SetActive(false);
            _timerRunning = false;

            if (allConnected)
            {
                // スコア計算
                int baseScore = Mathf.RoundToInt(Mathf.Max(0f, _timeLeft) * 10f) + pathLength * 50;
                _comboCount++;
                float multiplier = _comboCount >= 4 ? 3f : _comboCount == 3 ? 2f : _comboCount == 2 ? 1.5f : 1f;
                int gained = Mathf.RoundToInt(baseScore * multiplier);
                _score += gained;

                _state = GameState.StageClear;
                int stars = _timeLeft >= _stageTimes[Mathf.Min(_stageManager.CurrentStage, _stageTimes.Length - 1)] * 0.5f ? 3
                          : _timeLeft >= _stageTimes[Mathf.Min(_stageManager.CurrentStage, _stageTimes.Length - 1)] * 0.25f ? 2 : 1;

                if (_ui != null)
                {
                    _ui.UpdateScore(_score);
                    _ui.ShowStageClearPanel(_stageManager.CurrentStage + 1, _score, stars);
                }
            }
            else
            {
                _comboCount = 0;
                _state = GameState.GameOver;
                if (_ui != null) _ui.ShowGameOverPanel();
            }
        }

        public void OnFlowButton()
        {
            if (_state != GameState.Playing) return;
            _flowStarted = true;
            _timerRunning = false;
            _pipeManager.SetActive(false);
            _pipeManager.StartWaterFlow();
        }

        public void OnResetButton()
        {
            if (_state != GameState.Playing) return;
            _pipeManager.ResetPipes();
        }

        public void OnNextStageButtonPressed()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RestartStage()
        {
            if (_state != GameState.GameOver) return;
            _score = 0;
            _comboCount = 0;
            OnStageChanged(_stageManager.CurrentStage);
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
            if (_pipeManager != null)
                _pipeManager.OnFlowComplete -= OnFlowComplete;
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }
    }
}
