using UnityEngine;

namespace Game038v2_FlyBird
{
    public enum FlyBirdState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class FlyBirdGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BirdController _birdController;
        [SerializeField] PipeSpawner _pipeSpawner;
        [SerializeField] FlyBirdUI _ui;

        FlyBirdState _state;
        int _totalScore;
        int _comboCount;
        int _consecutivePass;

        static readonly int[] TargetCounts = { 5, 7, 8, 9, 10 };

        public FlyBirdState State => _state;
        public int TotalScore => _totalScore;

        void Start()
        {
            _state = FlyBirdState.WaitingInstruction;
            _instructionPanel.Show(
                "038v2",
                "FlyBird",
                "タップで鳥を飛ばして障害物を避けよう",
                "タップで羽ばたき、離すと降下",
                "障害物にぶつからずにゴールまで飛ぼう"
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
            _ui.HideAllPanels();
            _state = FlyBirdState.Playing;
            // Only reset score on first stage (full retry); preserve cumulative score on stage advance
            if (stage == 0) _totalScore = 0;
            _comboCount = 0;
            _consecutivePass = 0;

            _pipeSpawner.SetupStage(stage);
            _birdController.SetupStage(stage);
            _birdController.ResetBird();
            _pipeSpawner.ClearPipes();

            int target = stage < TargetCounts.Length ? TargetCounts[stage] : TargetCounts[TargetCounts.Length - 1];
            _ui.UpdateStage(stage + 1);
            _ui.UpdateScore(0);
            _ui.UpdateProgress(0, target);
            _ui.UpdateCombo(0);
        }

        void OnAllStagesCleared()
        {
            _state = FlyBirdState.Clear;
            _pipeSpawner.SetActive(false);
            _birdController.SetActive(false);
            _ui.ShowFinalClearPanel(_totalScore);
        }

        public void OnPipePassed(int currentStage)
        {
            if (_state != FlyBirdState.Playing) return;

            _consecutivePass++;
            int multiplier = _consecutivePass >= 10 ? 3 : _consecutivePass >= 5 ? 2 : 1;
            int gained = 10 * multiplier;
            _totalScore += gained;
            _comboCount = _consecutivePass;

            int target = currentStage < TargetCounts.Length ? TargetCounts[currentStage] : TargetCounts[TargetCounts.Length - 1];
            _ui.UpdateScore(_totalScore);
            _ui.UpdateProgress(_consecutivePass, target);
            _ui.UpdateCombo(_comboCount);

            if (_consecutivePass >= target)
            {
                _totalScore += 100; // stage clear bonus
                _state = FlyBirdState.StageClear;
                _pipeSpawner.SetActive(false);
                _birdController.SetActive(false);
                _ui.UpdateScore(_totalScore);
                _ui.ShowStageClearPanel(_totalScore);
            }
        }

        public void OnCoinCollected()
        {
            if (_state != FlyBirdState.Playing) return;
            _totalScore += 5;
            _ui.UpdateScore(_totalScore);
        }

        public void OnBirdDied()
        {
            if (_state != FlyBirdState.Playing) return;
            _state = FlyBirdState.GameOver;
            _pipeSpawner.SetActive(false);
            _birdController.SetActive(false);
            _ui.ShowGameOverPanel(_totalScore);
        }

        public void AdvanceToNextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void RetryGame()
        {
            _totalScore = 0;
            _comboCount = 0;
            _consecutivePass = 0;
            _stageManager.StartFromBeginning();
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
