using UnityEngine;

namespace Game037_ZapChain
{
    public class ZapChainGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス管理")]
        private ChainManager _manager;

        [SerializeField, Tooltip("UI管理")]
        private ZapChainUI _ui;

        [SerializeField, Tooltip("総ノード数")]
        private int _totalNodes = 12;

        [SerializeField, Tooltip("最大発動回数")]
        private int _maxZaps = 3;

        private int _connectedCount;
        private int _zapCount;
        private bool _isPlaying;

        private void Start()
        {
            _connectedCount = 0;
            _zapCount = 0;
            _isPlaying = true;
            _ui.UpdateConnected(0, _totalNodes);
            _ui.UpdateZaps(_maxZaps - _zapCount);
            _manager.StartStage();
        }

        public void OnNodeZapped()
        {
            if (!_isPlaying) return;
            _connectedCount++;
            _ui.UpdateConnected(_connectedCount, _totalNodes);

            if (_connectedCount >= _totalNodes)
            {
                _isPlaying = false;
                int stars = _zapCount <= 1 ? 3 : (_zapCount == 2 ? 2 : 1);
                _ui.ShowClear(stars);
            }
        }

        public void OnZapUsed()
        {
            if (!_isPlaying) return;
            _zapCount++;
            _ui.UpdateZaps(_maxZaps - _zapCount);
        }

        public void OnChainEnded()
        {
            if (!_isPlaying && _connectedCount >= _totalNodes) return;
            if (_zapCount >= _maxZaps && _connectedCount < _totalNodes)
            {
                _isPlaying = false;
                _ui.ShowGameOver();
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int MaxZaps => _maxZaps;
        public int ZapCount => _zapCount;
    }
}
