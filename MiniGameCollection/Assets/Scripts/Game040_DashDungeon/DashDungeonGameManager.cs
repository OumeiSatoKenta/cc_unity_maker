using UnityEngine;

namespace Game040_DashDungeon
{
    public class DashDungeonGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private DungeonManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private DashDungeonUI _ui;

        [SerializeField, Tooltip("最大HP")]
        private int _maxHp = 5;

        private int _hp;
        private int _score;
        private bool _isPlaying;

        private void Start()
        {
            _hp = _maxHp;
            _score = 0;
            _isPlaying = true;
            _ui.UpdateHp(_hp, _maxHp);
            _ui.UpdateScore(_score);
            _manager.StartGame();
        }

        public void TakeDamage(int dmg)
        {
            if (!_isPlaying) return;
            _hp -= dmg;
            _ui.UpdateHp(Mathf.Max(0, _hp), _maxHp);
            if (_hp <= 0)
            {
                _isPlaying = false;
                _ui.ShowGameOver(_score);
            }
        }

        public void AddScore(int pts)
        {
            if (!_isPlaying) return;
            _score += pts;
            _ui.UpdateScore(_score);
        }

        public void OnReachExit()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _ui.ShowClear(_score);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
