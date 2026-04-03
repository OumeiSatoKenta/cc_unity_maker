using UnityEngine;
using System;

namespace Game002v2_MirrorMaze
{
    public enum GameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class MirrorMazeGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private MirrorMazeUI _ui;

        private GameState _state = GameState.WaitingInstruction;
        private int _score;

        public bool IsPlaying => _state == GameState.Playing;
        public GameState State => _state;
        public int Score => _score;

        private void Start()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("002", "MirrorMaze",
                    "鏡を配置してレーザーをゴールに導くパズル",
                    "ドラッグで鏡を配置、タップで回転、発射ボタンで確認",
                    "レーザーを全てのゴールに到達させよう");
            }
            else
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            _score = 0;
            if (_ui != null) _ui.UpdateScore(_score);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            _state = GameState.Playing;
            _gridManager.SetupStage(stageIndex);
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

        public void OnLaserResult(bool success, int reflections, int unusedMirrors)
        {
            if (_state != GameState.Playing) return;

            if (success)
            {
                int stageNum = _stageManager.CurrentStage + 1;
                float comboMultiplier = 1f;
                if (reflections >= 5) comboMultiplier = 2.0f;
                else if (reflections >= 3) comboMultiplier = 1.5f;

                int stageScore = (int)((500 + reflections * 100 + unusedMirrors * 200) * comboMultiplier * stageNum);
                _score += stageScore;
                if (_ui != null) _ui.UpdateScore(_score);

                _state = GameState.StageClear;
                if (_ui != null) _ui.ShowStageClearPanel(stageNum, stageScore);
            }
            else
            {
                _gridManager.OnLaserFailed();
            }
        }

        public void OnNextStageButtonPressed()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RestartStage()
        {
            _state = GameState.Playing;
            _gridManager.ResetMirrors();
            if (_ui != null) _ui.HideAllPanels();
        }

        public void RestartGame()
        {
            _score = 0;
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
