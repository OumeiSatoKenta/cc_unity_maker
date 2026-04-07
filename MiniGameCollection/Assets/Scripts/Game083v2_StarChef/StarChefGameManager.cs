using UnityEngine;

namespace Game083v2_StarChef
{
    public class StarChefGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CookingManager _cookingManager;
        [SerializeField] StarChefUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "083",
                "StarChef",
                "宇宙の素材で銀河レシピを作ろう",
                "素材をタップして選択、ゲージが光ったらタップ！",
                "レストランを★5ランクに育てよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _cookingManager.ResetGame();
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _cookingManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _cookingManager.SetActive(false);
            _ui.ShowAllClear(_cookingManager.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void OnGameOver()
        {
            _cookingManager.SetActive(false);
            _ui.ShowGameOver(_cookingManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateFailDisplay(int fails) => _ui.UpdateFails(fails);

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
