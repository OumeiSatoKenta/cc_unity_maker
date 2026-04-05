using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game007v2_NumberFlow
{
    public class NumberFlowGameManager : MonoBehaviour
    {
        public enum GameState { WaitingInstruction, Playing, StageClear, Clear }

        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] NumberFlowManager _flowManager;
        [SerializeField] NumberFlowUI _ui;

        public GameState State { get; private set; } = GameState.WaitingInstruction;
        public int TotalScore { get; private set; }
        public int Combo { get; private set; }

        void Start()
        {
            _instructionPanel.OnDismissed += StartGame;
            _instructionPanel.Show(
                "007",
                "NumberFlow",
                "数字を順番に繋いで全マスを埋める一筆書きパズル",
                "タップまたはスワイプでマスを順に選択、最後のマスを再タップで1手戻る",
                "1から順に全マスを一筆書きで繋ごう"
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
            _flowManager.SetupStage(_stageManager.GetCurrentStageConfig(), stage);
            _ui.UpdateStage(stage, _stageManager.TotalStages);
            _ui.ShowStageClearPanel(false);
        }

        void OnAllStagesCleared()
        {
            State = GameState.Clear;
            _ui.ShowGameClearPanel(TotalScore);
        }

        public void OnStageClear(int baseScore, bool noUndo, bool fastClear)
        {
            if (State != GameState.Playing) return;

            Combo++;
            float comboMul = 1f + (Combo - 1) * 0.2f;
            float undoBonus = noUndo ? 2f : 1f;
            float fastBonus = fastClear ? 1.5f : 1f;
            int stageScore = Mathf.RoundToInt(baseScore * comboMul * undoBonus * fastBonus);
            TotalScore += stageScore;

            int stars = noUndo && fastClear ? 3
                      : noUndo || fastClear ? 2
                      : 1;

            State = GameState.StageClear;
            _ui.UpdateScore(TotalScore);
            _ui.UpdateCombo(Combo);
            _ui.ShowStageClearPanel(true, stageScore, stars);
        }

        public void OnNextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnReset()
        {
            if (State == GameState.Playing)
            {
                Combo = 0;
                _ui.UpdateCombo(0);
                _flowManager.ResetPath();
            }
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }
    }
}
