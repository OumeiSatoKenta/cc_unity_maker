using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game101_ChainReactor
{
    public class ChainReactorGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("リアクターマネージャー")] private ReactorManager _reactorManager;
        [SerializeField, Tooltip("UI管理")] private ChainReactorUI _ui;
        [SerializeField, Tooltip("ステージ管理")] private StageManager _stageManager;
        [SerializeField, Tooltip("チュートリアルパネル")] private InstructionPanel _instructionPanel;

        private int _score;
        private int _combo;
        private int _maxComboThisTap;
        private int _orbsDestroyedThisTap;
        private int _remainingTaps;
        private bool _isPlaying;
        private float _stageTimer;
        private bool _hasTimer;

        // ステージ定義
        private static readonly int[] OrbCounts = { 8, 12, 15, 18, 22 };
        private static readonly int[] TapCounts = { 3, 3, 2, 2, 2 };
        private static readonly float[] BlastRadii = { 1.8f, 1.5f, 1.5f, 1.3f, 1.2f };
        private static readonly float[] TimeLimits = { 0, 0, 0, 20f, 20f };
        private static readonly float[] MoveOrbRatio = { 0f, 0.2f, 0.15f, 0.15f, 0.2f };
        private static readonly float[] ShieldOrbRatio = { 0f, 0f, 0.15f, 0.15f, 0.15f };
        private static readonly float[] BonusOrbRatio = { 0f, 0f, 0f, 0f, 0.1f };

        private void Start()
        {
            if (_reactorManager == null || _ui == null || _stageManager == null)
            {
                Debug.LogError("[ChainReactorGM] 必須コンポーネントが未アサイン");
                return;
            }

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("101", "チェインリアクター",
                    "タップで爆発を起こし、連鎖反応で全オーブを消そう！",
                    "画面をタップして爆発を起こす。爆発範囲内のオーブは連鎖爆発する",
                    "限られたタップ数で全てのオーブを消せばクリア");
            }
            else
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            _score = 0;
            _isPlaying = true;
            _ui.HideAllPanels();
            _ui.UpdateScore(_score);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            int idx = Mathf.Clamp(stageIndex, 0, 4);
            _remainingTaps = TapCounts[idx];
            _hasTimer = TimeLimits[idx] > 0;
            _stageTimer = TimeLimits[idx];
            _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
            _ui.UpdateTaps(_remainingTaps);
            _ui.UpdateTimer(_hasTimer ? _stageTimer : -1f);
            _ui.UpdateChain(0);

            _reactorManager.SetupStage(
                OrbCounts[idx], BlastRadii[idx],
                MoveOrbRatio[idx], ShieldOrbRatio[idx], BonusOrbRatio[idx]);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            if (_hasTimer && _stageTimer > 0)
            {
                _stageTimer -= Time.deltaTime;
                _ui.UpdateTimer(_stageTimer);
                if (_stageTimer <= 0)
                {
                    _stageTimer = 0;
                    OnGameOver();
                }
            }
        }

        public void OnTapUsed()
        {
            _remainingTaps--;
            _orbsDestroyedThisTap = 0;
            _maxComboThisTap = 0;
            _ui.UpdateTaps(_remainingTaps);
        }

        public void OnOrbExploded(int chainDepth, bool isBonus)
        {
            if (!_isPlaying) return;
            _orbsDestroyedThisTap++;
            if (chainDepth > _maxComboThisTap) _maxComboThisTap = chainDepth;

            // スコア計算
            int baseScore = 100;
            int chainBonus = chainDepth * 100;
            int total = baseScore + chainBonus;
            if (isBonus) total *= 3;
            _score += total;

            _ui.UpdateScore(_score);
            _ui.UpdateChain(chainDepth);
            _ui.UpdateOrbCount(_reactorManager.RemainingOrbs, _reactorManager.TotalOrbs);
        }

        public void OnChainComplete()
        {
            if (!_isPlaying) return;

            // タップ倍率ボーナス
            int multiplier = 1;
            if (_orbsDestroyedThisTap >= 15) multiplier = 5;
            else if (_orbsDestroyedThisTap >= 10) multiplier = 3;
            else if (_orbsDestroyedThisTap >= 5) multiplier = 2;

            if (multiplier > 1)
            {
                int bonus = _orbsDestroyedThisTap * 100 * (multiplier - 1);
                _score += bonus;
                _ui.UpdateScore(_score);
                _ui.ShowMultiplier(multiplier);
            }

            // 全消しチェック
            if (_reactorManager.RemainingOrbs <= 0)
            {
                int tapBonus = _remainingTaps * 500;
                _score += tapBonus;
                _ui.UpdateScore(_score);
                OnStageClear();
                return;
            }

            // タップ数切れチェック
            if (_remainingTaps <= 0)
            {
                OnGameOver();
            }
        }

        private void OnStageClear()
        {
            _isPlaying = false;
            _reactorManager.StopStage();
            _ui.ShowStageClearPanel(_stageManager.CurrentStage + 1, _score);
        }

        public void OnNextStageButtonPressed()
        {
            _isPlaying = true;
            _ui.HideAllPanels();
            _stageManager.CompleteCurrentStage();
        }

        private void OnAllStagesCleared()
        {
            _isPlaying = false;
            _reactorManager.StopStage();
            _ui.ShowClearPanel(_score);
        }

        private void OnGameOver()
        {
            _isPlaying = false;
            _reactorManager.StopStage();
            _ui.ShowGameOverPanel(_score, _stageManager.CurrentStage + 1);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
