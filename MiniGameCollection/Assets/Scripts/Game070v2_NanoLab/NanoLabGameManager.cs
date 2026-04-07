using UnityEngine;

namespace Game070v2_NanoLab
{
    public class NanoLabGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] NanoMachineManager _nanoManager;
        [SerializeField] NanoLabUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "070",
                "NanoLab",
                "ナノマシンを増やして科学技術を進化させよう",
                "タップで増殖、ボタンで研究・時代進化",
                "時代目標を達成してステージクリア"
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
            _nanoManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _nanoManager.SetActive(false);
            _ui.ShowAllClear(_nanoManager.TotalScore);
        }

        public void OnStageClear()
        {
            _ui.ShowStageClear(_stageManager.CurrentStage + 1);
        }

        public void NextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void UpdateNanoCount(long count) => _ui.UpdateNanoCount(count);
        public void UpdateEra(int era, string eraName) => _ui.UpdateEra(era, eraName);
        public void UpdateAutoRate(float rate) => _ui.UpdateAutoRate(rate);
        public void UpdatePrestigeMultiplier(float mult) => _ui.UpdatePrestigeMultiplier(mult);
        public void UpdateScore(long score) => _ui.UpdateScore(score);
        public void UpdateTechNodes(TechNodeData[] nodes, long nanoCount) => _ui.UpdateTechNodes(nodes, nanoCount);
        public void UpdatePrestigeButton(bool available, long cost) => _ui.UpdatePrestigeButton(available, cost);
        public void ShowMutationEvent(bool isPositive, string description) => _ui.ShowMutationEvent(isPositive, description);

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
