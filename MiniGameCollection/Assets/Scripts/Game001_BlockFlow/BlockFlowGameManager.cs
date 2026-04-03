using UnityEngine;

namespace Game001_BlockFlow
{
    /// <summary>
    /// BlockFlow のゲーム全体を制御する。
    /// 盤面の初期化、手数管理、クリア判定を担当する。
    /// </summary>
    public class BlockFlowGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("盤面管理コンポーネント")]
        private BoardManager _boardManager;

        [SerializeField, Tooltip("UI管理コンポーネント")]
        private BlockFlowUI _ui;

        private int _moveCount;
        private bool _isCleared;

        public int MoveCount => _moveCount;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _moveCount = 0;
            _isCleared = false;

            if (_boardManager != null) _boardManager.GenerateBoard();
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);
            if (_ui != null) _ui.HideClearPanel();
        }

        /// <summary>
        /// ブロックが移動したときに呼ばれる。手数を加算しクリア判定を行う。
        /// </summary>
        public void OnBlockMoved()
        {
            if (_isCleared) return;

            _moveCount++;
            if (_ui != null) _ui.UpdateMoveCount(_moveCount);

            if (_boardManager != null && _boardManager.CheckAllGrouped())
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_moveCount);
            }
        }

        public void RestartGame()
        {
            StartGame();
        }
    }
}
