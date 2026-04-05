using UnityEngine;

namespace Game040v2_DashDungeon
{
    public enum DashDungeonState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class DashDungeonGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DashDungeonMechanic _mechanic;
        [SerializeField] DashDungeonUI _ui;

        DashDungeonState _state;
        int _totalScore;
        int _currentStage;

        public DashDungeonState State => _state;
        public int TotalScore => _totalScore;
        public int CurrentStage => _currentStage;

        void Start()
        {
            _state = DashDungeonState.WaitingInstruction;
            _instructionPanel.Show(
                "040v2",
                "DashDungeon",
                "ダッシュしてダンジョンを攻略しよう",
                "上下左右ボタンで壁まで直進",
                "トラップを避けて出口にたどり着こう"
            );
            _instructionPanel.OnDismissed += StartGame;
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

        void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _currentStage = stage;
            _ui.HideAllPanels();
            _state = DashDungeonState.Playing;
            if (stage == 0) _totalScore = 0;

            _mechanic.SetupStage(stage);
            _ui.UpdateStage(stage + 1);
            _ui.UpdateScore(_totalScore);
        }

        void OnAllStagesCleared()
        {
            _state = DashDungeonState.Clear;
            _mechanic.SetActive(false);
            _ui.ShowFinalClearPanel(_totalScore);
        }

        public void OnStageClear(int stageBonusScore)
        {
            if (_state != DashDungeonState.Playing) return;
            _state = DashDungeonState.StageClear;
            _totalScore += stageBonusScore;
            _mechanic.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(_totalScore);
        }

        public void OnGameOver()
        {
            if (_state != DashDungeonState.Playing) return;
            _state = DashDungeonState.GameOver;
            _mechanic.SetActive(false);
            _ui.ShowGameOverPanel(_totalScore);
        }

        public void OnHpChanged(int hp, int maxHp)
        {
            _ui.UpdateHp(hp, maxHp);
        }

        public void OnMovesChanged(int moves, int minMoves)
        {
            _ui.UpdateMoves(moves, minMoves);
        }

        public void AdvanceToNextStage()
        {
            _mechanic.SetActive(true);
            _stageManager.CompleteCurrentStage();
        }

        public void RetryGame()
        {
            _totalScore = 0;
            _mechanic.SetActive(true);
            _stageManager.StartFromBeginning();
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
