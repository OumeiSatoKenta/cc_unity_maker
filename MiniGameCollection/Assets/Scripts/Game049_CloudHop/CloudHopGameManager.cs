using UnityEngine;

namespace Game049_CloudHop
{
    public class CloudHopGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private HopManager _hopManager;
        [SerializeField, Tooltip("UI管理")] private CloudHopUI _ui;
        [SerializeField, Tooltip("プレイヤー")] private Transform _player;
        [SerializeField, Tooltip("目標高度")] private float _goalHeight = 50f;

        private Camera _mainCamera;
        private float _startY;
        private float _maxHeight;
        private bool _isPlaying;

        private void Start()
        {
            _mainCamera = Camera.main;
            _startY = _player.position.y;
            _maxHeight = 0f;
            _isPlaying = true;
            _hopManager.StartGame();
            _ui.UpdateHeight(0f);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            float currentHeight = _player.position.y - _startY;
            if (currentHeight > _maxHeight) _maxHeight = currentHeight;
            _ui.UpdateHeight(_maxHeight);

            // Camera follows player upward
            float targetCamY = Mathf.Max(_mainCamera.transform.position.y, _player.position.y);
            var camPos = _mainCamera.transform.position;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * 3f);
            _mainCamera.transform.position = camPos;

            // Clear check
            if (_maxHeight >= _goalHeight)
            {
                _isPlaying = false;
                _hopManager.StopGame();
                _ui.ShowClear(Mathf.RoundToInt(_maxHeight));
                return;
            }

            // Fall check - below camera bottom
            float camBottom = _mainCamera.transform.position.y - _mainCamera.orthographicSize - 1f;
            if (_player.position.y < camBottom)
            {
                _isPlaying = false;
                _hopManager.StopGame();
                _ui.ShowGameOver(Mathf.RoundToInt(_maxHeight));
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
