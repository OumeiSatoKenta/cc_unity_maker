using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game012v2_BridgeBuilder
{
    public class BridgeBuilderGameManager : MonoBehaviour
    {
        public enum GameState { WaitingInstruction, Building, Testing, StageClear, Clear }

        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BridgeManager _bridgeManager;
        [SerializeField] BridgeBuilderUI _ui;

        public GameState State { get; private set; } = GameState.WaitingInstruction;
        public int TotalScore { get; private set; }
        public int Combo { get; private set; }

        void Start()
        {
            _instructionPanel.OnDismissed += StartGame;
            _instructionPanel.Show(
                "012",
                "BridgeBuilder",
                "パーツを組み合わせて車が渡れる橋を作ろう",
                "パーツ選択 → 支点タップ2回で配置 → テストで走行確認",
                "予算内で橋を作り、車を安全に渡らせよう"
            );
        }

        void StartGame()
        {
            State = GameState.Building;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            State = GameState.Building;
            _bridgeManager.SetupStage(_stageManager.GetCurrentStageConfig(), stage + 1);
            _ui.UpdateStage(stage + 1, _stageManager.TotalStages);
            _ui.ShowStageClearPanel(false);
        }

        void OnAllStagesCleared()
        {
            State = GameState.Clear;
            _ui.ShowGameClearPanel(TotalScore);
        }

        public void OnTestStart()
        {
            if (State != GameState.Building) return;
            State = GameState.Testing;
            _bridgeManager.StartTest();
        }

        public void OnTestResult(bool passed, float budgetRatio)
        {
            if (State != GameState.Testing) return;

            if (!passed)
            {
                State = GameState.Building;
                _bridgeManager.ResetToBuilding();
                _ui.ShowTestFailedFeedback();
                Combo = 0;
                _ui.UpdateCombo(0);
                return;
            }

            Combo++;
            int baseScore = 1000 * (_stageManager.CurrentStage + 1);
            float budgetBonus = budgetRatio >= 0.5f ? 2.0f : budgetRatio >= 0.3f ? 1.5f : 1.0f;
            float comboMul = Mathf.Min(1.0f + (Combo - 1) * 0.3f, 3.0f);
            int stageScore = Mathf.RoundToInt(baseScore * budgetBonus * comboMul);
            TotalScore += stageScore;

            int stars = budgetRatio >= 0.5f ? 3 : budgetRatio >= 0.2f ? 2 : 1;

            State = GameState.StageClear;
            _ui.UpdateScore(TotalScore);
            _ui.UpdateCombo(Combo);
            _ui.ShowStageClearPanel(true, stageScore, stars);
        }

        public void OnNextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            if (State == GameState.Testing)
            {
                _bridgeManager.ResetToBuilding();
                State = GameState.Building;
            }
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }
    }
}
