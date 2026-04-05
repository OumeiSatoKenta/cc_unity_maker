using UnityEngine;

namespace Game037v2_ZapChain
{
    public enum ZapChainState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class ZapChainGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] ZapMechanic _zapMechanic;
        [SerializeField] ZapChainUI _ui;

        ZapChainState _state;
        int _totalScore;
        int _comboCount;

        void Start()
        {
            _state = ZapChainState.WaitingInstruction;
            _instructionPanel.Show(
                "037v2",
                "ZapChain",
                "電撃を連鎖させて全ノードを接続しよう",
                "ノードをタップしてドラッグで隣接ノードへ連鎖",
                "一筆書きで全ノードを接続しよう"
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
            _state = ZapChainState.Playing;
            _totalScore = 0;
            _comboCount = 0;
            _zapMechanic.SetupStage(stage);
            _ui.UpdateStage(stage + 1);
            _ui.UpdateScore(0);
            _ui.UpdateEnergy(_zapMechanic.CurrentEnergy, _zapMechanic.EnergyMax);
            _ui.UpdateChain(0, _zapMechanic.GetNonObstacleCount());
        }

        void OnAllStagesCleared()
        {
            _state = ZapChainState.Clear;
            _zapMechanic.SetActive(false);
            _ui.ShowFinalClearPanel(_totalScore);
        }

        public void OnNodeConnected(int chainLength)
        {
            if (_state != ZapChainState.Playing) return;
            _comboCount = chainLength;
            int multiplier = chainLength >= 10 ? 3 : chainLength >= 5 ? 2 : 1;
            int gained = 10 * multiplier;
            _totalScore += gained;
            _ui.UpdateScore(_totalScore);
            _ui.UpdateEnergy(_zapMechanic.CurrentEnergy, _zapMechanic.EnergyMax);
            _ui.UpdateChain(_zapMechanic.ConnectedNodes, _zapMechanic.GetNonObstacleCount());
        }

        public void OnChainCompleted(int chainLength, bool allConnected)
        {
            if (_state != ZapChainState.Playing) return;

            if (allConnected)
            {
                if (chainLength >= _zapMechanic.GetNonObstacleCount())
                    _totalScore += 500;

                _ui.UpdateScore(_totalScore);
                _state = ZapChainState.StageClear;
                _zapMechanic.SetActive(false);
                _ui.ShowStageClearPanel(_totalScore);
            }
            else
            {
                _ui.UpdateEnergy(_zapMechanic.CurrentEnergy, _zapMechanic.EnergyMax);
                _ui.UpdateChain(_zapMechanic.ConnectedNodes, _zapMechanic.GetNonObstacleCount());
            }
        }

        public void OnEnergyEmpty()
        {
            if (_state != ZapChainState.Playing) return;
            _state = ZapChainState.GameOver;
            _zapMechanic.SetActive(false);
            _ui.ShowGameOverPanel(_totalScore);
        }

        public void AdvanceToNextStage()
        {
            _state = ZapChainState.Playing;
            _stageManager.CompleteCurrentStage();
        }

        public void RetryGame()
        {
            _totalScore = 0;
            _comboCount = 0;
            _stageManager.StartFromBeginning();
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
