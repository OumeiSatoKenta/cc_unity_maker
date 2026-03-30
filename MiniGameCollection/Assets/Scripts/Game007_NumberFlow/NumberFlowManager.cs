using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Game007_NumberFlow
{
    /// <summary>
    /// 4×4グリッドのタップ入力を一元管理し、経路バリデーションを行う。
    /// 隣接チェック・次番号チェック・全マス訪問チェックを担当。
    /// </summary>
    public class NumberFlowManager : MonoBehaviour
    {
        [SerializeField] private NumberFlowGameManager _gameManager;
        [SerializeField] private NumberFlowUI _ui;
        [SerializeField] private NumberCell[] _cells; // 16 cells, row*4+col order

        public UnityEvent OnCleared = new();
        public UnityEvent OnInvalidMove = new();

        private const int GridSize = 4;
        private int _currentStep;   // next number to visit
        private int _lastRow = -1;
        private int _lastCol = -1;
        private int _maxNumber;

        public void LoadGrid(int[,] grid)
        {
            _maxNumber = GridSize * GridSize;
            _currentStep = 1;
            _lastRow = -1;
            _lastCol = -1;

            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    _cells[r * GridSize + c].Init(grid[r, c], r, c, _maxNumber);

            _ui?.UpdateStep(0, _maxNumber);
        }

        public void ResetGrid()
        {
            _currentStep = 1;
            _lastRow = -1;
            _lastCol = -1;
            foreach (var cell in _cells) cell.Reset(_maxNumber);
            _ui?.UpdateStep(0, _maxNumber);
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsPlaying) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var cell = hit.GetComponent<NumberCell>();
            if (cell == null) return;

            ProcessTap(cell);
        }

        private void ProcessTap(NumberCell cell)
        {
            // Must tap the next number in sequence
            if (cell.Number != _currentStep)
            {
                OnInvalidMove?.Invoke();
                return;
            }

            // First cell: no adjacency check required
            if (_lastRow >= 0 && !IsAdjacent(_lastRow, _lastCol, cell.Row, cell.Col))
            {
                OnInvalidMove?.Invoke();
                return;
            }

            // Mark previously current cell as just visited
            if (_lastRow >= 0)
            {
                var prev = _cells[_lastRow * GridSize + _lastCol];
                prev.MarkVisited(false);
            }

            cell.MarkVisited(true);
            _lastRow = cell.Row;
            _lastCol = cell.Col;
            _currentStep++;

            _ui?.UpdateStep(_currentStep - 1, _maxNumber);

            if (_currentStep > _maxNumber)
                OnCleared?.Invoke();
        }

        private static bool IsAdjacent(int r1, int c1, int r2, int c2)
        {
            int dr = Mathf.Abs(r1 - r2);
            int dc = Mathf.Abs(c1 - c2);
            return (dr == 0 && dc == 1) || (dr == 1 && dc == 0);
        }
    }
}
