using UnityEngine;

namespace Game051_DrawBridge
{
    public class DrawBridgeGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("橋描画管理")] private BridgeManager _bridgeManager;
        [SerializeField, Tooltip("UI管理")] private DrawBridgeUI _ui;
        [SerializeField, Tooltip("キャラクター")] private Rigidbody2D _character;
        [SerializeField, Tooltip("歩行速度")] private float _walkSpeed = 2f;

        private bool _isPlaying;
        private bool _isWalking;
        private float _elapsedTime;

        private void Start()
        {
            _isPlaying = true;
            _isWalking = false;
            _elapsedTime = 0f;
            _bridgeManager.StartGame();
            _ui.UpdateInk(1f);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _elapsedTime += Time.deltaTime;
            _ui.UpdateInk(_bridgeManager.InkRatio);

            if (_isWalking)
            {
                _character.linearVelocity = new Vector2(_walkSpeed, _character.linearVelocity.y);
            }
        }

        public void OnDrawingFinished()
        {
            if (!_isPlaying) return;
            _isWalking = true;
            _character.bodyType = RigidbodyType2D.Dynamic;
        }

        public void OnCharacterReachedGoal()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _isWalking = false;
            _bridgeManager.StopGame();
            _character.linearVelocity = Vector2.zero;
            _ui.ShowClear(_elapsedTime);
        }

        public void OnCharacterFallen()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _isWalking = false;
            _bridgeManager.StopGame();
            _ui.ShowGameOver();
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
