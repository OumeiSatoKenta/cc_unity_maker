using UnityEngine;

namespace Game029_MeteorShield
{
    public class MeteorShieldGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("シールド管理コンポーネント")]
        private ShieldManager _shieldManager;

        [SerializeField, Tooltip("UI管理コンポーネント")]
        private MeteorShieldUI _ui;

        [SerializeField, Tooltip("星の最大HP")]
        private int _maxHp = 3;

        [SerializeField, Tooltip("クリアまでの時間(秒)")]
        private float _clearTime = 60f;

        private int _currentHp;
        private float _elapsed;
        private bool _isPlaying;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _currentHp = _maxHp;
            _elapsed = 0f;
            _isPlaying = true;

            if (_ui != null)
            {
                _ui.UpdateHp(_currentHp, _maxHp);
                _ui.UpdateTime(_clearTime);
                _ui.HidePanels();
            }

            if (_shieldManager != null)
                _shieldManager.StartGame();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0f, _clearTime - _elapsed);

            if (_ui != null)
                _ui.UpdateTime(remaining);

            if (_elapsed >= _clearTime)
            {
                OnTimeUp();
            }
        }

        public void OnMeteorHitStar()
        {
            if (!_isPlaying) return;

            _currentHp--;
            if (_ui != null)
                _ui.UpdateHp(_currentHp, _maxHp);

            if (_currentHp <= 0)
            {
                _isPlaying = false;
                if (_shieldManager != null)
                    _shieldManager.StopGame();
                if (_ui != null)
                    _ui.ShowGameOverPanel(_elapsed);
            }
        }

        private void OnTimeUp()
        {
            _isPlaying = false;
            if (_shieldManager != null)
                _shieldManager.StopGame();
            if (_ui != null)
                _ui.ShowClearPanel(_currentHp, _maxHp);
        }

        public void RestartGame()
        {
            StartGame();
        }

        public float ClearTime => _clearTime;
        public bool IsPlaying => _isPlaying;
    }
}
