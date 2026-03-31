using UnityEngine;
namespace Game037_ZapChain
{
    public class ZapChainGameManager : MonoBehaviour
    {
        [SerializeField] private ZapManager _zapManager;
        [SerializeField] private ZapChainUI _ui;
        private int _score; private int _maxChain; private bool _isGameOver;
        private void Start() { StartGame(); }
        public void StartGame()
        {
            _score = 0; _maxChain = 0; _isGameOver = false;
            if (_zapManager != null) _zapManager.StartGame();
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateChain(0); _ui.UpdateTime(30f); _ui.HideResultPanel(); }
        }
        public void OnChainZap(int chain)
        {
            if (_isGameOver) return;
            int points = chain * chain * 10;
            _score += points;
            if (chain > _maxChain) _maxChain = chain;
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateChain(chain); }
        }
        public void OnTimeUpdate(float t) { if (_ui != null) _ui.UpdateTime(t); }
        public void OnTimeUp()
        {
            _isGameOver = true; if (_zapManager != null) _zapManager.StopGame();
            if (_ui != null) _ui.ShowResultPanel(_score, _maxChain);
        }
        public void RestartGame() { StartGame(); }
    }
}
