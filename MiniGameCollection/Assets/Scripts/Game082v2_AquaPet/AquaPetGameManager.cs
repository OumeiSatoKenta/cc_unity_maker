using UnityEngine;

namespace Game082v2_AquaPet
{
    public class AquaPetGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] AquariumManager _aquariumManager;
        [SerializeField] AquaPetUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "082",
                "AquaPet",
                "水槽で魚を育ててコレクションしよう",
                "餌やり・掃除・繁殖ボタンをタップして魚を管理しよう",
                "レアな魚を繁殖させて図鑑をコンプリートしよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _aquariumManager.ResetScore();
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _aquariumManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _aquariumManager.SetActive(false);
            _ui.ShowAllClear(_aquariumManager.TotalScore);
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
            _aquariumManager.SetActive(false);
            _ui.ShowGameOver(_aquariumManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void UpdateWaterQualityDisplay(float quality) => _ui.UpdateWaterQuality(quality);
        public void UpdateAverageHealthDisplay(float health) => _ui.UpdateAverageHealth(health);
        public void UpdateFeedCountDisplay(int count) => _ui.UpdateFeedCount(count);
        public void UpdateCollectionDisplay(int collected, int total) => _ui.UpdateCollection(collected, total);

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
