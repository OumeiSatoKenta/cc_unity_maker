using UnityEngine;

namespace Game057_CandyDrop
{
    public class CandyDropGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private DropManager _dropManager;
        [SerializeField, Tooltip("UI管理")] private CandyDropUI _ui;
        [SerializeField, Tooltip("目標高度")] private float _targetHeight = 5f;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _dropManager.StartGame();
            _ui.UpdateHeight(0f, _targetHeight);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            float h = _dropManager.TowerHeight;
            _ui.UpdateHeight(h, _targetHeight);

            if (h >= _targetHeight)
            {
                _isPlaying = false;
                _dropManager.StopGame();
                _ui.ShowClear(Mathf.RoundToInt(h));
            }
        }

        public void OnTowerCollapsed()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _dropManager.StopGame();
            _ui.ShowGameOver(_dropManager.TowerHeight);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
