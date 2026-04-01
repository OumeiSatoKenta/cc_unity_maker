using UnityEngine;

namespace Game071_BeatTiles
{
    public class BeatTilesGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private RhythmManager _rhythmManager;
        [SerializeField, Tooltip("UI管理")] private BeatTilesUI _ui;
        [SerializeField, Tooltip("ミス上限")] private int _maxMisses = 10;
        [SerializeField, Tooltip("楽曲長(秒)")] private float _songLength = 30f;

        private int _score;
        private int _combo;
        private int _missCount;
        private float _songTimer;
        private bool _isPlaying;

        private void Start()
        {
            _score = 0; _combo = 0; _missCount = 0;
            _songTimer = 0f;
            _isPlaying = true;
            _rhythmManager.StartGame();
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_combo);
            _ui.UpdateMisses(_missCount, _maxMisses);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _songTimer += Time.deltaTime;

            if (_songTimer >= _songLength)
            {
                _isPlaying = false;
                _rhythmManager.StopGame();
                _ui.ShowClear(_score, _combo);
                return;
            }

            _ui.UpdateProgress(_songTimer / _songLength);
        }

        public void OnPerfect()
        {
            if (!_isPlaying) return;
            _combo++;
            _score += 100 + _combo * 10;
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_combo);
            _ui.ShowJudge("Perfect!");
        }

        public void OnGreat()
        {
            if (!_isPlaying) return;
            _combo++;
            _score += 50 + _combo * 5;
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_combo);
            _ui.ShowJudge("Great!");
        }

        public void OnMiss()
        {
            if (!_isPlaying) return;
            _combo = 0;
            _missCount++;
            _ui.UpdateCombo(_combo);
            _ui.UpdateMisses(_missCount, _maxMisses);
            _ui.ShowJudge("Miss...");

            if (_missCount >= _maxMisses)
            {
                _isPlaying = false;
                _rhythmManager.StopGame();
                _ui.ShowGameOver(_score);
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
