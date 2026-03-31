using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game018_TimeRewind
{
    public class TimeRewindManager : MonoBehaviour
    {
        [SerializeField] private int _gridWidth = 7;
        [SerializeField] private int _gridHeight = 7;
        [SerializeField] private float _cellSize = 1.0f;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _piecePrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private GameObject _ghostPrefab;

        private CellType[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();
        private readonly Stack<Vector2Int> _moveHistory = new Stack<Vector2Int>();
        private readonly List<GameObject> _ghostObjects = new List<GameObject>();

        private GameObject _pieceObj;
        private Vector2Int _piecePos;
        private Vector2Int _goalPos;
        private bool _isDragging;
        private Vector2 _dragStart;

        private TimeRewindGameManager _gameManager;
        private Camera _mainCamera;

        public static int StageCount => 3;

        public enum CellType { Floor, Wall, Goal }

        private void Awake()
        {
            _gameManager = GetComponentInParent<TimeRewindGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _dragStart = mouse.position.ReadValue();
                _isDragging = true;
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
                Vector2 dragEnd = mouse.position.ReadValue();
                Vector2 delta = dragEnd - _dragStart;
                if (delta.magnitude < 30f) return;

                Vector2Int direction;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                else
                    direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

                TryMovePiece(direction);
            }
        }

        private void TryMovePiece(Vector2Int direction)
        {
            Vector2Int newPos = _piecePos + direction;
            if (!IsInBounds(newPos)) return;
            if (_grid[newPos.x, newPos.y] == CellType.Wall) return;

            _moveHistory.Push(_piecePos);
            _piecePos = newPos;

            if (_pieceObj != null)
                _pieceObj.transform.position = GridToWorld(_piecePos);

            // Add ghost at old position
            if (_ghostPrefab != null)
            {
                var ghost = Instantiate(_ghostPrefab, transform);
                ghost.transform.position = GridToWorld(_moveHistory.Peek());
                _ghostObjects.Add(ghost);
                _stageObjects.Add(ghost);
            }

            if (_gameManager != null)
            {
                _gameManager.OnPieceMoved(_moveHistory.Count);
                if (_piecePos == _goalPos)
                    _gameManager.OnPuzzleSolved();
            }
        }

        public void Rewind()
        {
            if (_moveHistory.Count == 0) return;

            _piecePos = _moveHistory.Pop();
            if (_pieceObj != null)
                _pieceObj.transform.position = GridToWorld(_piecePos);

            // Remove last ghost
            if (_ghostObjects.Count > 0)
            {
                var lastGhost = _ghostObjects[_ghostObjects.Count - 1];
                _ghostObjects.RemoveAt(_ghostObjects.Count - 1);
                if (lastGhost != null)
                {
                    _stageObjects.Remove(lastGhost);
                    Destroy(lastGhost);
                }
            }

            if (_gameManager != null)
                _gameManager.OnRewind(_moveHistory.Count);
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            _moveHistory.Clear();
            var data = GetStageData(stageIndex);
            _gridWidth = data.width;
            _gridHeight = data.height;
            _grid = new CellType[_gridWidth, _gridHeight];
            _goalPos = data.goalPos;
            BuildStage(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            _ghostObjects.Clear();
            _pieceObj = null;
        }

        private void BuildStage(StageData data)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _grid[x, y] = CellType.Floor;
                    if (_cellPrefab != null)
                    {
                        var obj = Instantiate(_cellPrefab, transform);
                        obj.transform.position = GridToWorld(new Vector2Int(x, y));
                        _stageObjects.Add(obj);
                    }
                }
            }

            foreach (var wp in data.walls)
            {
                if (!IsInBounds(wp)) continue;
                _grid[wp.x, wp.y] = CellType.Wall;
                if (_wallPrefab != null)
                {
                    var obj = Instantiate(_wallPrefab, transform);
                    obj.transform.position = GridToWorld(wp);
                    _stageObjects.Add(obj);
                }
            }

            _grid[data.goalPos.x, data.goalPos.y] = CellType.Goal;
            if (_goalPrefab != null)
            {
                var obj = Instantiate(_goalPrefab, transform);
                obj.transform.position = GridToWorld(data.goalPos);
                _stageObjects.Add(obj);
            }

            _piecePos = data.startPos;
            if (_piecePrefab != null)
            {
                _pieceObj = Instantiate(_piecePrefab, transform);
                _pieceObj.transform.position = GridToWorld(_piecePos);
                _stageObjects.Add(_pieceObj);
            }
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = (_gridWidth - 1) * _cellSize * 0.5f;
            float offsetY = (_gridHeight - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offsetX, gridPos.y * _cellSize - offsetY, 0f);
        }

        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _gridWidth && pos.y >= 0 && pos.y < _gridHeight;
        }

        #region Stage Data

        private struct StageData
        {
            public int width, height;
            public Vector2Int startPos, goalPos;
            public List<Vector2Int> walls;
        }

        private StageData GetStageData(int index)
        {
            switch (index % StageCount)
            {
                case 0: return GetStage1();
                case 1: return GetStage2();
                case 2: return GetStage3();
                default: return GetStage1();
            }
        }

        private StageData GetStage1()
        {
            var walls = new List<Vector2Int>();
            for (int x = 0; x < 5; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 4)); }
            for (int y = 1; y < 4; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(4, y)); }
            walls.Add(new Vector2Int(2, 2));
            return new StageData { width = 5, height = 5, startPos = new Vector2Int(1, 1), goalPos = new Vector2Int(3, 3), walls = walls };
        }

        private StageData GetStage2()
        {
            var walls = new List<Vector2Int>();
            for (int x = 0; x < 6; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 5)); }
            for (int y = 1; y < 5; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(5, y)); }
            walls.Add(new Vector2Int(2, 3)); walls.Add(new Vector2Int(3, 2)); walls.Add(new Vector2Int(3, 4));
            return new StageData { width = 6, height = 6, startPos = new Vector2Int(1, 4), goalPos = new Vector2Int(4, 1), walls = walls };
        }

        private StageData GetStage3()
        {
            var walls = new List<Vector2Int>();
            for (int x = 0; x < 7; x++) { walls.Add(new Vector2Int(x, 0)); walls.Add(new Vector2Int(x, 6)); }
            for (int y = 1; y < 6; y++) { walls.Add(new Vector2Int(0, y)); walls.Add(new Vector2Int(6, y)); }
            walls.Add(new Vector2Int(2, 2)); walls.Add(new Vector2Int(3, 3)); walls.Add(new Vector2Int(4, 4));
            walls.Add(new Vector2Int(2, 4)); walls.Add(new Vector2Int(4, 2));
            return new StageData { width = 7, height = 7, startPos = new Vector2Int(1, 1), goalPos = new Vector2Int(5, 5), walls = walls };
        }

        #endregion
    }
}
