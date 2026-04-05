using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game096_DualControl
{
    public class DualControlGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private DualManager _dualManager;
        [SerializeField, Tooltip("UI管理")] private DualControlUI _ui;

        private float _elapsedTime;
        private bool _isPlaying;

        private void Start()
        {
            if (_dualManager == null) { Debug.LogError("[DualControlGameManager] _dualManager が未アサイン"); return; }
            if (_ui == null) { Debug.LogError("[DualControlGameManager] _ui が未アサイン"); return; }
            _elapsedTime = 0f;
            _isPlaying = true;
            _dualManager.StartGame();
            _ui.UpdateTimer(0f);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _elapsedTime += Time.deltaTime;
            _ui.UpdateTimer(_elapsedTime);
        }

        public void UpdateStage(int left, int right, int goal)
        {
            _ui?.UpdateStage(left, right, goal);
        }

        public void OnGoalReached()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _dualManager.StopGame();
            _ui.ShowClear(_elapsedTime);
        }

        public void OnCharacterHit()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _dualManager.StopGame();
            _ui.ShowGameOver();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
