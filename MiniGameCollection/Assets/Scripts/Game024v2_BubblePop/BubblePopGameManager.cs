using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game024v2_BubblePop
{
    public enum BubblePopGameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class BubblePopGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BubbleController _controller;
        [SerializeField] BubblePopUI _ui;

        public BubblePopGameState State { get; private set; } = BubblePopGameState.WaitingInstruction;

        int _totalScore;
        int _lives = 3;
        int _comboStreak;
        float _comboMultiplier = 1f;
        int _consecutivePops;
        bool _feverActive;
        float _feverTimer;
        const float FeverDuration = 10f;
        const int FeverThreshold = 20;

        void Start()
        {
            _instructionPanel.Show(
                "024v2",
                "BubblePop",
                "浮かんでくるバブルをタップして割ろう！",
                "バブルをタップして破裂させよう。同じ色を連続タップで連鎖ボーナス！",
                "ライフを守りながら全5ステージをクリアしよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _totalScore = 0;
            _lives = 3;
            _comboMultiplier = 1f;
            _comboStreak = 0;
            _consecutivePops = 0;
            _feverActive = false;
            _feverTimer = 0f;
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            State = BubblePopGameState.Playing;
            _comboMultiplier = 1f;
            _comboStreak = 0;
            _consecutivePops = 0;
            _feverActive = false;
            _feverTimer = 0f;
            var config = _stageManager.GetCurrentStageConfig();
            _controller.SetupStage(config, stage + 1);
            _ui.UpdateStageDisplay(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateLives(_lives);
            _ui.UpdateCombo(_comboMultiplier, _comboStreak);
            _ui.UpdateFever(false, 0f);
        }

        void OnAllStagesCleared()
        {
            State = BubblePopGameState.Clear;
            _controller.StopGame();
            _ui.ShowClearPanel(_totalScore);
        }

        void Update()
        {
            if (State != BubblePopGameState.Playing) return;
            if (_feverActive)
            {
                _feverTimer -= Time.deltaTime;
                _ui.UpdateFever(true, _feverTimer / FeverDuration);
                if (_feverTimer <= 0f)
                    EndFever();
            }
        }

        void EndFever()
        {
            _feverActive = false;
            _consecutivePops = 0;
            _ui.UpdateFever(false, 0f);
        }

        // Called by BubbleController when a bubble is tapped
        public void OnBubblePopped(BubbleType type, BubbleColor color, float spawnElapsed, BubbleColor lastPoppedColor)
        {
            if (State != BubblePopGameState.Playing) return;

            int baseScore = type switch
            {
                BubbleType.Iron => 30,
                BubbleType.Split => 25,
                BubbleType.Ghost => 20,
                _ => 10
            };

            // Speed bonus
            if (spawnElapsed <= 1f)
                baseScore += 20;

            // Same color chain (only for Normal/Ghost, 3-color mode)
            bool sameColor = (color != BubbleColor.None && color == lastPoppedColor);
            if (sameColor)
            {
                _comboStreak++;
                _comboMultiplier = _comboStreak >= 4 ? 3.0f : _comboStreak == 3 ? 2.0f : 1.5f;
            }
            else
            {
                _comboStreak = 0;
                _comboMultiplier = 1f;
            }

            // Consecutive pops for fever
            _consecutivePops++;
            if (!_feverActive && _consecutivePops >= FeverThreshold)
            {
                _feverActive = true;
                _feverTimer = FeverDuration;
                _ui.ShowFeverStart();
            }

            float multiplier = _comboMultiplier * (_feverActive ? 2f : 1f);
            int finalScore = Mathf.RoundToInt(baseScore * multiplier);
            _totalScore += finalScore;

            _ui.ShowBubbleBonus(finalScore, _comboStreak, _feverActive);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_comboMultiplier, _comboStreak);
        }

        // Called by BubbleController when bubble reaches top
        public void OnBubbleEscaped()
        {
            if (State != BubblePopGameState.Playing) return;
            _lives--;
            _comboStreak = 0;
            _comboMultiplier = 1f;
            _consecutivePops = 0;
            _ui.UpdateLives(_lives);
            _ui.UpdateCombo(1f, 0);
            _ui.ShowLifeLost();

            if (_lives <= 0)
            {
                GameOver();
            }
        }

        void GameOver()
        {
            State = BubblePopGameState.GameOver;
            _controller.StopGame();
            _ui.ShowGameOverPanel(_totalScore, _comboStreak);
        }

        // Called by BubbleController when stage time runs out
        public void OnStageTimeUp()
        {
            if (State != BubblePopGameState.Playing) return;
            State = BubblePopGameState.StageClear;
            _controller.StopGame();
            _ui.ShowStageClearPanel(_totalScore);
        }

        public void OnNextStage()
        {
            if (State != BubblePopGameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("CollectionSelect");
        }

        public void ShowInstructions()
        {
            if (_instructionPanel != null)
                _instructionPanel.Show(
                    "024v2",
                    "BubblePop",
                    "浮かんでくるバブルをタップして割ろう！",
                    "バブルをタップして破裂させよう。同じ色を連続タップで連鎖ボーナス！",
                    "ライフを守りながら全5ステージをクリアしよう"
                );
        }

        void OnDestroy()
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
