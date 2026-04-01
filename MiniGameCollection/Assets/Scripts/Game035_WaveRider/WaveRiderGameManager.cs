using UnityEngine;

namespace Game035_WaveRider
{
    public class WaveRiderGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private WaveManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private WaveRiderUI _ui;

        [SerializeField, Tooltip("目標距離")]
        private float _goalDistance = 200f;

        private int _score;
        private float _distance;
        private bool _isPlaying;

        private void Start()
        {
            _score = 0;
            _distance = 0f;
            _isPlaying = true;
            _ui.UpdateScore(_score);
            _ui.UpdateDistance(_distance, _goalDistance);
            _manager.StartGame();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _distance += Time.deltaTime * 5f;
            _ui.UpdateDistance(_distance, _goalDistance);

            if (_distance >= _goalDistance)
            {
                _isPlaying = false;
                _ui.ShowClear(_score);
            }
        }

        public void AddTrickScore(int points)
        {
            if (!_isPlaying) return;
            _score += points;
            _ui.UpdateScore(_score);
        }

        public void OnCrash()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
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
