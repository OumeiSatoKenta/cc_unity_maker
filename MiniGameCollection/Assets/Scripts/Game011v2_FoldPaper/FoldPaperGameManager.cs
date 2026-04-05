using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game011v2_FoldPaper
{
    public class FoldPaperGameManager : MonoBehaviour
    {
        public enum GameState { WaitingInstruction, Playing, StageClear, Clear }

        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] FoldPaperManager _foldPaperManager;
        [SerializeField] FoldPaperUI _ui;

        public GameState State { get; private set; } = GameState.WaitingInstruction;
        public int TotalScore { get; private set; }
        public int Combo { get; private set; }

        void Start()
        {
            _instructionPanel.OnDismissed += StartGame;
            _instructionPanel.Show(
                "011",
                "FoldPaper",
                "折り線をタップして紙を折り、お手本の形を作ろう",
                "折り線タップ→選択 / 紙の上下タップ→折る方向決定",
                "手数以内に目標のシルエットと同じ形を作ろう"
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
            _foldPaperManager.SetupStage(_stageManager.GetCurrentStageConfig(), stage);
            _ui.UpdateStage(stage, _stageManager.TotalStages);
            _ui.ShowStageClearPanel(false);
        }

        void OnAllStagesCleared()
        {
            State = GameState.Clear;
            _ui.ShowGameClearPanel(TotalScore);
        }

        public void OnFoldResult(bool clearReached, int movesUsed, int movesLimit, int undoUsed, int minMoves)
        {
            if (!clearReached || State != GameState.Playing) return;

            int baseScore = 1000 * (_stageManager.CurrentStage + 1);
            int movesLeft = movesLimit - movesUsed;
            int movesBonus = Mathf.Max(0, movesLeft) * 50;
            float undoBonus = undoUsed == 0 ? 1.5f : 1.0f;
            Combo++;
            float comboMul = Mathf.Min(1.0f + (Combo - 1) * 0.2f, 3.0f);

            int stageScore = Mathf.RoundToInt((baseScore + movesBonus) * undoBonus * comboMul);
            TotalScore += stageScore;

            int stars = movesUsed <= minMoves ? 3
                      : movesUsed <= Mathf.RoundToInt(movesLimit * 0.7f) ? 2
                      : 1;

            State = GameState.StageClear;
            _ui.UpdateScore(TotalScore);
            _ui.UpdateCombo(Combo);
            _ui.ShowStageClearPanel(true, stageScore, stars);
        }

        public void OnMoveFailed()
        {
            Combo = 0;
            _ui.UpdateCombo(0);
        }

        public void OnNextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            if (State == GameState.Playing)
                _foldPaperManager.ResetStage();
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }
    }
}
