using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game001v2_BlockFlow
{
    public class BlockFlowGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("盤面管理")] private BoardManager _boardManager;
        [SerializeField, Tooltip("UI管理")] private BlockFlowUI _ui;
        [SerializeField, Tooltip("ステージ管理")] private StageManager _stageManager;
        [SerializeField, Tooltip("チュートリアルパネル")] private InstructionPanel _instructionPanel;

        private int _score;
        private int _totalMoves;
        private bool _usedReset;
        private bool _isPlaying;

        // ステージ定義
        private static readonly int[] BoardSizes = { 3, 4, 5, 5, 6 };
        private static readonly int[] ColorCounts = { 2, 3, 3, 4, 4 };
        private static readonly int[] MoveLimits = { 0, 0, 0, 15, 20 }; // 0=無制限
        private static readonly int[] FixedCounts = { 0, 2, 2, 3, 3 };
        private static readonly int[] WarpPairCounts = { 0, 0, 1, 1, 2 };
        private static readonly int[] IceCounts = { 0, 0, 0, 0, 4 };

        private void Start()
        {
            if (_boardManager == null || _ui == null || _stageManager == null)
            {
                Debug.LogError("[BlockFlowGM] 必須コンポーネントが未アサイン");
                return;
            }

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("001", "ブロックフロー",
                    "同じ色のブロックを隣り合わせにするパズル",
                    "スワイプでブロックを移動。壁や障害物に当たるまで滑ります",
                    "全ての同色ブロックを隣接させよう");
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
            _usedReset = false;
            _totalMoves = 0;
            _ui.HideAllPanels();
            _ui.UpdateScore(_score);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            int idx = Mathf.Clamp(stageIndex, 0, 4);
            _usedReset = false;
            _totalMoves = 0;

            _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
            _ui.UpdateMoves(0, MoveLimits[idx]);

            _boardManager.SetupStage(
                BoardSizes[idx], ColorCounts[idx], MoveLimits[idx],
                FixedCounts[idx], WarpPairCounts[idx], IceCounts[idx]);
        }

        public void OnMoveMade()
        {
            if (!_isPlaying) return;
            _totalMoves++;
            int idx = Mathf.Clamp(_stageManager.CurrentStage, 0, 4);
            _ui.UpdateMoves(_totalMoves, MoveLimits[idx]);

            if (MoveLimits[idx] > 0 && _totalMoves >= MoveLimits[idx])
            {
                if (!_boardManager.CheckClear())
                {
                    OnGameOver();
                }
            }
        }

        public void OnBoardCleared(int colorsUsed)
        {
            if (!_isPlaying) return;

            int idx = Mathf.Clamp(_stageManager.CurrentStage, 0, 4);
            int stageScore = 1000;
            int remainingMoves = MoveLimits[idx] > 0 ? MoveLimits[idx] - _totalMoves : 0;
            int moveBonus = remainingMoves * 200;
            int colorBonus = colorsUsed * 500;
            int subtotal = stageScore + moveBonus + colorBonus;

            float multiplier = _usedReset ? 1.0f : 1.5f;
            int finalScore = Mathf.RoundToInt(subtotal * multiplier);

            _score += finalScore;
            _ui.UpdateScore(_score);

            _isPlaying = false;
            _ui.ShowStageClearPanel(_stageManager.CurrentStage + 1, _score,
                !_usedReset ? "ノーリセットボーナス x1.5!" : "");
        }

        public void OnResetUsed()
        {
            _usedReset = true;
            _totalMoves = 0;
            int idx = Mathf.Clamp(_stageManager.CurrentStage, 0, 4);
            _ui.UpdateMoves(0, MoveLimits[idx]);
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
            _ui.ShowClearPanel(_score);
        }

        private void OnGameOver()
        {
            _isPlaying = false;
            _boardManager.SetActive(false);
            _ui.ShowGameOverPanel(_score, _stageManager.CurrentStage + 1);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
