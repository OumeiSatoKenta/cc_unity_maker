using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game095_SilentWorld
{
    public class SilentWorldGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private WorldManager _worldManager;
        [SerializeField, Tooltip("UI管理")] private SilentWorldUI _ui;
        [SerializeField, Tooltip("全アイテム数")] private int _totalItems = 3;

        private int _collectedItems;
        private float _elapsedTime;
        private bool _isPlaying;

        private void Start()
        {
            if (_worldManager == null) { Debug.LogError("[SilentWorldGameManager] _worldManager が未アサイン"); return; }
            if (_ui == null) { Debug.LogError("[SilentWorldGameManager] _ui が未アサイン"); return; }
            _collectedItems = 0;
            _elapsedTime = 0f;
            _isPlaying = true;
            _worldManager.StartGame();
            _ui.UpdateTimer(0f);
            _ui.UpdateItems(_collectedItems, _totalItems);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _elapsedTime += Time.deltaTime;
            _ui.UpdateTimer(_elapsedTime);
        }

        public void OnItemCollected()
        {
            if (!_isPlaying) return;
            _collectedItems++;
            _ui.UpdateItems(_collectedItems, _totalItems);
        }

        public void OnExitReached()
        {
            if (!_isPlaying) return;
            if (_collectedItems >= _totalItems)
            {
                _isPlaying = false;
                _worldManager.StopGame();
                _ui.ShowClear(_elapsedTime);
            }
        }

        public void OnTrapHit()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _worldManager.StopGame();
            _ui.ShowGameOver();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void UpdateHintDisplay(int remaining)
        { _ui?.UpdateHint(remaining); }

        public bool IsPlaying => _isPlaying;
        public int CollectedItems => _collectedItems;
        public int TotalItems => _totalItems;
    }
}
