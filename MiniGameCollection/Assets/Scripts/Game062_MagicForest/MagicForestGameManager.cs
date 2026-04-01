using UnityEngine;

namespace Game062_MagicForest
{
    public class MagicForestGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private ForestManager _forestManager;
        [SerializeField, Tooltip("UI管理")] private MagicForestUI _ui;
        [SerializeField, Tooltip("目標本数")] private int _targetTrees = 20;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _forestManager.StartGame();
            _ui.UpdateTrees(_forestManager.TreeCount, _targetTrees);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _ui.UpdateTrees(_forestManager.TreeCount, _targetTrees);

            if (_forestManager.TreeCount >= _targetTrees)
            {
                _isPlaying = false;
                _forestManager.StopGame();
                _ui.ShowClear(_forestManager.TreeCount);
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
