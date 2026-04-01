using UnityEngine;

namespace Game054_FruitSlash
{
    public class FruitSlashGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private SlashManager _slashManager;
        [SerializeField, Tooltip("UI管理")] private FruitSlashUI _ui;
        [SerializeField, Tooltip("目標スコア")] private int _targetScore = 30;
        [SerializeField, Tooltip("見逃し上限")] private int _maxMisses = 3;

        private int _score;
        private int _combo;
        private int _missCount;
        private bool _isPlaying;

        private void Start()
        {
            _score = 0; _combo = 0; _missCount = 0;
            _isPlaying = true;
            _slashManager.StartGame();
            _ui.UpdateScore(_score, _targetScore);
            _ui.UpdateCombo(_combo);
            _ui.UpdateMisses(_missCount, _maxMisses);
        }

        public void OnFruitSlashed()
        {
            if (!_isPlaying) return;
            _combo++;
            _score += _combo;
            _ui.UpdateScore(_score, _targetScore);
            _ui.UpdateCombo(_combo);

            if (_score >= _targetScore)
            {
                _isPlaying = false;
                _slashManager.StopGame();
                _ui.ShowClear(_score);
            }
        }

        public void OnFruitMissed()
        {
            if (!_isPlaying) return;
            _combo = 0;
            _missCount++;
            _ui.UpdateCombo(_combo);
            _ui.UpdateMisses(_missCount, _maxMisses);

            if (_missCount >= _maxMisses)
            {
                _isPlaying = false;
                _slashManager.StopGame();
                _ui.ShowGameOver(_score);
            }
        }

        public void OnBombHit()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _slashManager.StopGame();
            _ui.ShowGameOver(_score);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
