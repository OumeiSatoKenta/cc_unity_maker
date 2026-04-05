using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game023v2_ChainSlash
{
    public enum ChainSlashGameState { WaitingInstruction, Playing, StageClear, Clear }

    public class ChainSlashGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] ChainSlashController _controller;
        [SerializeField] ChainSlashUI _ui;

        public ChainSlashGameState State { get; private set; } = ChainSlashGameState.WaitingInstruction;

        int _totalScore;
        float _comboMultiplier = 1f;
        int _comboStreak;
        float _comboTimer;
        const float ComboWindow = 3f;

        void Start()
        {
            _instructionPanel.Show(
                "023v2",
                "ChainSlash",
                "敵をなぞって繋げ、指を離すと一気に斬れるぞ！",
                "敵をドラッグしてなぞり鎖で繋ぐ → 指を離すと一斉に斬撃！\n多く繋ぐほど二乗でスコアがアップ！",
                "制限時間内に最大チェインを狙い高スコアを獲得しよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _totalScore = 0;
            _comboMultiplier = 1f;
            _comboStreak = 0;
            _comboTimer = 0f;
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            State = ChainSlashGameState.Playing;
            _comboMultiplier = 1f;
            _comboStreak = 0;
            _comboTimer = 0f;
            var config = _stageManager.GetCurrentStageConfig();
            _controller.SetupStage(config, stage + 1); // stage is 0-based, pass 1-based
            _ui.UpdateStageDisplay(stage + 1, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_comboMultiplier, _comboStreak);
        }

        void OnAllStagesCleared()
        {
            State = ChainSlashGameState.Clear;
            _controller.StopGame();
            _ui.ShowClearPanel(_totalScore);
        }

        void Update()
        {
            if (State != ChainSlashGameState.Playing) return;
            if (_comboStreak > 0)
            {
                _comboTimer += Time.deltaTime;
                if (_comboTimer >= ComboWindow)
                    ResetCombo();
            }
        }

        void ResetCombo()
        {
            _comboStreak = 0;
            _comboMultiplier = 1f;
            _comboTimer = 0f;
            _ui.UpdateCombo(_comboMultiplier, _comboStreak);
        }

        public void OnSlashExecuted(int chainCount, bool allSameColor)
        {
            if (State != ChainSlashGameState.Playing) return;

            int baseScore = chainCount * chainCount * 10;
            if (allSameColor) baseScore = Mathf.RoundToInt(baseScore * 1.5f);
            int finalScore = Mathf.RoundToInt(baseScore * _comboMultiplier);
            _totalScore += finalScore;

            _comboStreak++;
            _comboTimer = 0f;
            UpdateComboMultiplier();

            _ui.ShowSlashBonus(finalScore, chainCount, allSameColor);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateCombo(_comboMultiplier, _comboStreak);
        }

        void UpdateComboMultiplier()
        {
            if (_comboStreak >= 5) _comboMultiplier = 3.0f;
            else if (_comboStreak >= 3) _comboMultiplier = 2.0f;
            else if (_comboStreak >= 2) _comboMultiplier = 1.5f;
            else _comboMultiplier = 1.0f;
        }

        public void OnTimeUp()
        {
            if (State != ChainSlashGameState.Playing) return;
            StageClear();
        }

        void StageClear()
        {
            if (State != ChainSlashGameState.Playing) return;
            State = ChainSlashGameState.StageClear;
            _controller.StopGame();
            _ui.ShowStageClearPanel(_totalScore, _comboStreak);
        }

        public void OnNextStage()
        {
            if (State != ChainSlashGameState.StageClear) return;
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
                    "023v2",
                    "ChainSlash",
                    "敵をなぞって繋げ、指を離すと一気に斬れるぞ！",
                    "敵をドラッグしてなぞり鎖で繋ぐ → 指を離すと一斉に斬撃！\n多く繋ぐほど二乗でスコアがアップ！",
                    "制限時間内に最大チェインを狙い高スコアを獲得しよう"
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
