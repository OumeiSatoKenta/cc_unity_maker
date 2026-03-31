using UnityEngine;

namespace Game036_CoinStack
{
    public class CoinStackGameManager : MonoBehaviour
    {
        [SerializeField] private StackManager _stackManager;
        [SerializeField] private CoinStackUI _ui;
        private int _height; private int _perfectCount; private bool _isGameOver;

        private void Start() { StartGame(); }
        public void StartGame()
        {
            _height = 0; _perfectCount = 0; _isGameOver = false;
            if (_stackManager != null) _stackManager.StartGame();
            if (_ui != null) { _ui.UpdateHeight(0); _ui.UpdatePerfect(0); _ui.HideGameOverPanel(); }
        }
        public void OnCoinStacked(int height, float offset)
        {
            if (_isGameOver) return;
            _height = height;
            if (offset < 0.3f) _perfectCount++;
            if (_ui != null) { _ui.UpdateHeight(_height); _ui.UpdatePerfect(_perfectCount); }
        }
        public void OnStackFall()
        {
            _isGameOver = true;
            if (_stackManager != null) _stackManager.StopGame();
            if (_ui != null) _ui.ShowGameOverPanel(_height, _perfectCount);
        }
        public void RestartGame() { StartGame(); }
    }
}
