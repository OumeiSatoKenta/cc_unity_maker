using UnityEngine;

namespace Game080v2_FreqFight
{
    public class FreqFightGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] FreqFightManager _freqManager;
        [SerializeField] FreqFightUI _ui;

        void Start()
        {
            _instructionPanel.Show(
                "080",
                "FreqFight",
                "敵の周波数に合わせてスライダーを調整し、ビートに乗ってロックオン攻撃！",
                "スライダーをドラッグして敵と同じ音程に合わせよう。ビートに同期して自動判定される",
                "5ステージの敵を全滅させて周波数マスターになれ！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _freqManager.ResetScore();
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _freqManager.SetupStage(config, stageIndex);
            _ui.UpdateStage(stageIndex + 1, 5);
        }

        void OnAllStagesCleared()
        {
            _freqManager.SetActive(false);
            _ui.ShowAllClear(_freqManager.TotalScore);
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
            _freqManager.SetActive(false);
            _ui.ShowGameOver(_freqManager.TotalScore);
        }

        public void UpdateScoreDisplay(int score) => _ui.UpdateScore(score);
        public void UpdateComboDisplay(int combo) => _ui.UpdateCombo(combo);
        public void ShowJudgement(string text, Color color) => _ui.ShowJudgement(text, color);
        public void UpdatePhase(string phase) => _ui.UpdatePhase(phase);
        public void UpdatePlayerHp(float ratio) => _ui.UpdatePlayerHp(ratio);
        public void UpdateEnemyHp(float ratio, int enemyIndex = 0) => _ui.UpdateEnemyHp(ratio, enemyIndex);
        public void UpdateEnemyFreqMarker(float normalizedFreq, int enemyIndex = 0) => _ui.UpdateEnemyFreqMarker(normalizedFreq, enemyIndex);
        public void PulseBeat() => _ui.PulseBeat();
        public void ShakeEnemy(int enemyIndex = 0) => _ui.ShakeEnemy(enemyIndex);

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
