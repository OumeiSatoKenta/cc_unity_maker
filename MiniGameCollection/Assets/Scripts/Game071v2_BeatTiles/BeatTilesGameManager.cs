using UnityEngine;
using Common;

namespace Game071v2_BeatTiles
{
    public class BeatTilesGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] NoteManager _noteManager;
        [SerializeField] BeatTilesUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "071",
                "BeatTiles",
                "リズムに合わせてタイルをタップしよう",
                "判定ラインにノーツが重なったらタップ！Perfectを狙ってコンボを繋げよう",
                "5ステージを全てクリアしてリズムマスターになろう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _noteManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _noteManager.SetActive(false);
            _ui.ShowAllClear(_noteManager.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnGameOver()
        {
            _ui.ShowGameOver(_noteManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateLifeDisplay(float life) => _ui.UpdateLife(life);
        public void ShowJudgement(string text, UnityEngine.Color color) => _ui.ShowJudgement(text, color);

        void OnDestroy()
        {
            if (_instructionPanel != null) _instructionPanel.OnDismissed -= StartGame;
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
        }
    }
}
