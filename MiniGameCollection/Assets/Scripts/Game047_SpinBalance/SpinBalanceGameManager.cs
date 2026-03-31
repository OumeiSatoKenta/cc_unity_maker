using UnityEngine;

namespace Game047_SpinBalance
{
    public class SpinBalanceGameManager : MonoBehaviour
    {
        [SerializeField] private BalanceManager _balanceManager;
        [SerializeField] private SpinBalanceUI _ui;

        private float _timer;
        private int _pieceCount;
        private bool _isGameOver;

        public bool IsGameOver => _isGameOver;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _timer = 0f;
            _pieceCount = 0;
            _isGameOver = false;
            if (_ui != null)
            {
                _ui.UpdateTimer(_timer);
                _ui.UpdatePieces(_pieceCount);
                _ui.HideGameOverPanel();
            }
            if (_balanceManager != null) _balanceManager.Init();
        }

        private void Update()
        {
            if (_isGameOver) return;
            _timer += Time.deltaTime;
            if (_ui != null) _ui.UpdateTimer(_timer);
        }

        public void OnPieceAdded(int total)
        {
            _pieceCount = total;
            if (_ui != null) _ui.UpdatePieces(_pieceCount);
        }

        public void OnGameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;
            if (_ui != null) _ui.ShowGameOverPanel(_timer, _pieceCount);
        }
    }
}
