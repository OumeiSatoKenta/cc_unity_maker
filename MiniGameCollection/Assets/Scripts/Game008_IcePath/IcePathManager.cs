using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Game008_IcePath
{
    /// <summary>
    /// IcePath のコア入力・ロジック管理。
    /// 方向キー/クリックで氷上を滑らせ、全マス通過でクリア判定を行う。
    /// 入力処理を一元管理する。
    /// </summary>
    public class IcePathManager : MonoBehaviour
    {
        [SerializeField] private IcePathGameManager _gameManager;
        [SerializeField] private IcePathUI _ui;
        [SerializeField] private IceCell[] _cells; // GridSize*GridSize, row*GridSize+col

        public UnityEvent OnCleared = new();

        private const int GridSize = 5;

        // Current player position
        private int _playerRow;
        private int _playerCol;
        private int _visitedCount;
        private int _totalIceCells;

        // Level layout: 0=ice, 1=wall
        private int[,] _currentLayout;

        // Arrow key / WASD tracking
        private bool _upPrev, _downPrev, _leftPrev, _rightPrev;

        // Player visual
        private GameObject _playerGo;

        public void LoadLevel(int[,] layout, int startRow, int startCol)
        {
            _currentLayout = layout;
            _visitedCount = 0;
            _totalIceCells = 0;

            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    if (layout[r, c] == 0) _totalIceCells++;

            // Init cells
            foreach (var cell in _cells)
                cell.Reset();

            // Set player
            _playerRow = startRow;
            _playerCol = startCol;
            MarkVisited(_playerRow, _playerCol);

            UpdatePlayerVisual();
            _ui?.UpdateProgress(_visitedCount, _totalIceCells);
        }

        public void ResetLevel()
        {
            if (_currentLayout == null) return;
            // Find start position (will be re-supplied by GameManager)
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsPlaying) return;

            // Keyboard input (arrow keys / WASD)
            bool up    = Keyboard.current != null && (Keyboard.current.upArrowKey.isPressed    || Keyboard.current.wKey.isPressed);
            bool down  = Keyboard.current != null && (Keyboard.current.downArrowKey.isPressed  || Keyboard.current.sKey.isPressed);
            bool left  = Keyboard.current != null && (Keyboard.current.leftArrowKey.isPressed  || Keyboard.current.aKey.isPressed);
            bool right = Keyboard.current != null && (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed);

            if (up    && !_upPrev)    TrySlide(-1, 0);
            if (down  && !_downPrev)  TrySlide(1, 0);
            if (left  && !_leftPrev)  TrySlide(0, -1);
            if (right && !_rightPrev) TrySlide(0, 1);

            _upPrev = up; _downPrev = down; _leftPrev = left; _rightPrev = right;

            // Mouse click on direction arrows (touch/click the UI buttons)
            // Direction button clicks are wired via UnityEvent in Setup
        }

        /// <summary>
        /// 指定方向に壁にぶつかるまで滑る。
        /// </summary>
        public void TrySlide(int dr, int dc)
        {
            if (_currentLayout == null) return;

            int r = _playerRow;
            int c = _playerCol;

            // Slide until wall or boundary
            while (true)
            {
                int nr = r + dr;
                int nc = c + dc;

                if (nr < 0 || nr >= GridSize || nc < 0 || nc >= GridSize) break;
                if (_currentLayout[nr, nc] == 1) break; // wall

                r = nr;
                c = nc;

                if (!GetCell(r, c).IsVisited)
                    MarkVisited(r, c);
            }

            if (r == _playerRow && c == _playerCol) return; // didn't move

            _playerRow = r;
            _playerCol = c;
            UpdatePlayerVisual();
            _ui?.UpdateProgress(_visitedCount, _totalIceCells);

            if (_visitedCount >= _totalIceCells)
                OnCleared?.Invoke();
        }

        // Direction wrappers for UI button events
        public void SlideUp()    => TrySlide(-1, 0);
        public void SlideDown()  => TrySlide(1, 0);
        public void SlideLeft()  => TrySlide(0, -1);
        public void SlideRight() => TrySlide(0, 1);

        private void MarkVisited(int r, int c)
        {
            var cell = GetCell(r, c);
            if (!cell.IsVisited)
            {
                cell.SetVisited(true);
                _visitedCount++;
            }
        }

        private IceCell GetCell(int r, int c) => _cells[r * GridSize + c];

        private void UpdatePlayerVisual()
        {
            if (_playerGo == null) return;
            var cell = GetCell(_playerRow, _playerCol);
            _playerGo.transform.position = cell.transform.position + new Vector3(0, 0, -0.5f);
        }

        public void SetPlayerGo(GameObject go) => _playerGo = go;

        public void SetCells(IceCell[] cells) => _cells = cells;
    }
}
