using UnityEngine;

namespace Game080_FreqFight
{
    public class FreqFightGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private FreqManager _freqManager;
        [SerializeField, Tooltip("UI管理")] private FreqFightUI _ui;
        [SerializeField, Tooltip("全敵数")] private int _totalEnemies = 5;
        [SerializeField, Tooltip("制限時間")] private float _timeLimit = 45f;

        private int _defeatedCount;
        private float _timer;
        private int _playerHP;
        private bool _isPlaying;

        private void Start()
        {
            _defeatedCount = 0; _timer = _timeLimit; _playerHP = 3;
            _isPlaying = true;
            _freqManager.StartGame();
            _ui.UpdateEnemies(_defeatedCount, _totalEnemies);
            _ui.UpdateTimer(_timer);
            _ui.UpdateHP(_playerHP);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _timer -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0f, _timer));

            if (_timer <= 0f)
            {
                _isPlaying = false;
                _freqManager.StopGame();
                _ui.ShowGameOver(_defeatedCount);
            }
        }

        public void OnEnemyDefeated()
        {
            if (!_isPlaying) return;
            _defeatedCount++;
            _ui.UpdateEnemies(_defeatedCount, _totalEnemies);

            if (_defeatedCount >= _totalEnemies)
            {
                _isPlaying = false;
                _freqManager.StopGame();
                _ui.ShowClear(_defeatedCount, _playerHP);
                return;
            }
            _freqManager.NextEnemy();
        }

        public void OnPlayerDamaged()
        {
            if (!_isPlaying) return;
            _playerHP--;
            _ui.UpdateHP(_playerHP);
            if (_playerHP <= 0)
            {
                _isPlaying = false;
                _freqManager.StopGame();
                _ui.ShowGameOver(_defeatedCount);
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
