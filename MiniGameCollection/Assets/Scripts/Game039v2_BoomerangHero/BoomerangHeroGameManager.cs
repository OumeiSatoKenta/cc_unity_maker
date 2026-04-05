using UnityEngine;

namespace Game039v2_BoomerangHero
{
    public enum BoomerangHeroState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class BoomerangHeroGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BoomerangMechanic _mechanic;
        [SerializeField] BoomerangHeroUI _ui;

        BoomerangHeroState _state;
        int _totalScore;
        int _currentStage;

        public BoomerangHeroState State => _state;
        public int TotalScore => _totalScore;
        public int CurrentStage => _currentStage;

        void Start()
        {
            _state = BoomerangHeroState.WaitingInstruction;
            _instructionPanel.Show(
                "039v2",
                "BoomerangHero",
                "ブーメランを壁で反射させて敵を倒そう",
                "ドラッグで角度と力を調整、リリースで発射",
                "限られた弾数で全ての敵を倒そう"
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
            _state = BoomerangHeroState.Playing;
            if (stage == 0) _totalScore = 0;

            _mechanic.SetupStage(stage);
            _ui.UpdateStage(stage + 1);
            _ui.UpdateScore(_totalScore);
        }

        void OnAllStagesCleared()
        {
            _state = BoomerangHeroState.Clear;
            _mechanic.SetActive(false);
            _ui.ShowFinalClearPanel(_totalScore);
        }

        public void OnEnemyDefeated(int points)
        {
            if (_state != BoomerangHeroState.Playing) return;
            _totalScore += points;
            _ui.UpdateScore(_totalScore);
            _ui.ShowScorePopup(points);
        }

        public void OnAmmoUpdated(int remaining, int max)
        {
            _ui.UpdateAmmo(remaining, max);
        }

        public void OnEnemyCountUpdated(int remaining)
        {
            _ui.UpdateEnemyCount(remaining);
        }

        public void OnStageClear(int stageBonusScore)
        {
            if (_state != BoomerangHeroState.Playing) return;
            _state = BoomerangHeroState.StageClear;
            _totalScore += stageBonusScore;
            _mechanic.SetActive(false);
            _ui.UpdateScore(_totalScore);
            _ui.ShowStageClearPanel(_totalScore);
        }

        public void OnGameOver()
        {
            if (_state != BoomerangHeroState.Playing) return;
            _state = BoomerangHeroState.GameOver;
            _mechanic.SetActive(false);
            _ui.ShowGameOverPanel(_totalScore);
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
