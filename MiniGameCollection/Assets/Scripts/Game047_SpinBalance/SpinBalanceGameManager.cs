using UnityEngine;

namespace Game047_SpinBalance
{
    public class SpinBalanceGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private BalanceManager _balanceManager;

        [SerializeField, Tooltip("UI管理")]
        private SpinBalanceUI _ui;

        [SerializeField, Tooltip("制限時間")]
        private float _timeLimit = 30f;

        private int _currentPieceCount;
        private float _timer;
        private bool _isPlaying;

        private void Start()
        {
            _timer = _timeLimit;
            _isPlaying = true;
            _balanceManager.StartGame();
            _currentPieceCount = _balanceManager.CurrentPieceCount;
            _ui.UpdatePieceCount(_currentPieceCount);
            _ui.UpdateTimer(_timer);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _timer -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0f, _timer));

            _currentPieceCount = _balanceManager.CurrentPieceCount;
            _ui.UpdatePieceCount(_currentPieceCount);

            if (_timer <= 0f)
            {
                _isPlaying = false;
                _balanceManager.StopGame();
                int score = _currentPieceCount * 100;
                _ui.ShowClear(score);
                return;
            }

            if (_currentPieceCount <= 0)
            {
                _isPlaying = false;
                _balanceManager.StopGame();
                _ui.ShowGameOver();
            }
        }

        public void OnPieceFallen()
        {
            // BalanceManager handles count; Update() reads it each frame
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
