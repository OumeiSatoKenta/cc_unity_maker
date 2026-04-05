using UnityEngine;
using UnityEngine.SceneManagement;
using Game010v2_GearSync;

namespace Game010v2_GearSync
{
    public class GearSyncGameManager : MonoBehaviour
    {
        public enum GameState { WaitingInstruction, Playing, StageClear, Clear }

        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] GearSyncManager _gearSyncManager;
        [SerializeField] GearSyncUI _ui;

        public GameState State { get; private set; } = GameState.WaitingInstruction;
        public int TotalScore { get; private set; }
        public int Combo { get; private set; }

        void Start()
        {
            _instructionPanel.OnDismissed += StartGame;
            _instructionPanel.Show(
                "010",
                "GearSync",
                "歯車を配置して全てを連動させる機械パズル",
                "パーツをタップして選択、グリッドをタップして配置。配置済みをタップで回収",
                "全ての歯車を噛み合わせて機械を起動しよう"
            );
        }

        void StartGame()
        {
            State = GameState.Playing;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            State = GameState.Playing;
            _gearSyncManager.SetupStage(_stageManager.GetCurrentStageConfig(), stage);
            _ui.UpdateStage(stage, _stageManager.TotalStages);
            _ui.ShowStageClearPanel(false);
        }

        void OnAllStagesCleared()
        {
            State = GameState.Clear;
            _ui.ShowGameClearPanel(TotalScore);
        }

        public void OnTestResult(bool success, int testCount, int partsUsed, int partsMin)
        {
            if (!success || State != GameState.Playing) return;

            int baseScore = 1000 * (_stageManager.CurrentStage + 1);
            int testBonus = Mathf.Max(0, 5 - testCount) * 100;
            float masterBonus = testCount == 1 ? 3.0f : 1.0f;
            Combo++;
            float comboMul = 1.0f + (Combo - 1) * 0.2f;

            int stageScore = Mathf.RoundToInt((baseScore + testBonus) * masterBonus * comboMul);
            TotalScore += stageScore;

            int stars = testCount == 1 && partsUsed <= partsMin ? 3
                      : testCount <= 3 ? 2
                      : 1;

            State = GameState.StageClear;
            _ui.UpdateScore(TotalScore);
            _ui.UpdateCombo(Combo);
            _ui.ShowStageClearPanel(true, stageScore, stars);
        }

        public void OnTestFailed()
        {
            Combo = 0;
            _ui.UpdateCombo(0);
        }

        public void OnNextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }
    }
}
