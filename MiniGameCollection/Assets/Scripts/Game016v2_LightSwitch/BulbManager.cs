using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game016v2_LightSwitch
{
    public enum BulbType { Normal, ColoredRed, ColoredBlue, Warp, Delayed }

    public class BulbManager : MonoBehaviour
    {
        [SerializeField] LightSwitchGameManager _gameManager;
        [SerializeField] LightSwitchUI _ui;

        [SerializeField] Sprite _spBulbOn;
        [SerializeField] Sprite _spBulbOff;
        [SerializeField] Sprite _spBulbRed;
        [SerializeField] Sprite _spBulbRedOff;
        [SerializeField] Sprite _spBulbBlue;
        [SerializeField] Sprite _spBulbBlueOff;
        [SerializeField] Sprite _spBulbWarp;
        [SerializeField] Sprite _spBulbWarpOff;
        [SerializeField] Sprite _spBulbDelayed;
        [SerializeField] Sprite _spBulbDelayedOff;

        // Stage config
        int _gridSize;
        int _maxMoves;
        int _maxUndo;
        int _stageNum;
        bool _hasColored;
        bool _hasWarp;
        bool _hasDelayed;
        bool _hasShiftTarget;
        int _shiftInterval = 5;

        // State
        bool[] _currentState;
        bool[] _targetPattern;
        BulbType[] _bulbTypes;
        int _moveCount;
        int _undoUsedCount;
        bool _isActive;

        // Undo history: each entry stores state snapshot + delay queue snapshot + shiftCount
        struct UndoFrame
        {
            public bool[] state;
            public List<(int idx, int trigger)> delays;
            public int shiftCount;
        }
        Stack<UndoFrame> _history = new Stack<UndoFrame>();
        Queue<(int idx, int triggerMove)> _pendingDelays = new Queue<(int, int)>();

        GameObject[] _bulbObjects;
        SpriteRenderer[] _bulbRenderers;

        int _shiftCount;
        bool[][] _shiftPatterns;

        StageManager.StageConfig _cachedConfig;

        void Update()
        {
            if (!_isActive) return;
            if (_gameManager.State != LightSwitchGameManager.GameState.Playing) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    int idx = System.Array.IndexOf(_bulbObjects, hit.gameObject);
                    if (idx >= 0) OnBulbTapped(idx);
                }
            }
        }

        public void SetupStage(StageManager.StageConfig config, int stageNum)
        {
            _cachedConfig = config;
            _stageNum = stageNum;
            _isActive = false;
            ClearBulbs();
            _history.Clear();
            _pendingDelays.Clear();

            switch (stageNum)
            {
                case 1: _gridSize = 3; _maxMoves = 10; _maxUndo = 3; _hasColored = false; _hasWarp = false; _hasDelayed = false; _hasShiftTarget = false; break;
                case 2: _gridSize = 4; _maxMoves = 18; _maxUndo = 3; _hasColored = true;  _hasWarp = false; _hasDelayed = false; _hasShiftTarget = false; break;
                case 3: _gridSize = 4; _maxMoves = 20; _maxUndo = 2; _hasColored = false; _hasWarp = true;  _hasDelayed = false; _hasShiftTarget = false; break;
                case 4: _gridSize = 5; _maxMoves = 28; _maxUndo = 2; _hasColored = false; _hasWarp = false; _hasDelayed = true;  _hasShiftTarget = false; break;
                case 5: _gridSize = 5; _maxMoves = 32; _maxUndo = 1; _hasColored = true;  _hasWarp = true;  _hasDelayed = true;  _hasShiftTarget = true;  break;
                default: _gridSize = 3; _maxMoves = 10; _maxUndo = 3; _hasColored = false; _hasWarp = false; _hasDelayed = false; _hasShiftTarget = false; break;
            }

            _moveCount = 0;
            _undoUsedCount = 0;
            _shiftCount = 0;

            int n = _gridSize * _gridSize;
            _currentState = new bool[n];
            _targetPattern = new bool[n];
            _bulbTypes = new BulbType[n];
            _bulbObjects = new GameObject[n];
            _bulbRenderers = new SpriteRenderer[n];

            AssignBulbTypes();
            GenerateTargetPattern();
            GenerateInitialState();
            if (_hasShiftTarget) GenerateShiftPatterns();

            SpawnBulbs();
            UpdateAllSprites();

            _ui.UpdateMoves(_maxMoves - _moveCount, _maxMoves);
            _ui.UpdateUndo(_maxUndo - _undoUsedCount);
            _ui.UpdateTargetPattern(_targetPattern, _gridSize);

            _isActive = true;
        }

        public void ResetStage()
        {
            if (_stageNum == 0) return;
            SetupStage(_cachedConfig, _stageNum);
        }

        void AssignBulbTypes()
        {
            int n = _gridSize * _gridSize;
            for (int i = 0; i < n; i++) _bulbTypes[i] = BulbType.Normal;

            if (_hasColored)
            {
                // Row-based coloring: even rows = Red, odd rows = Blue
                // This ensures adjacent cells are same color within a row, enabling propagation
                for (int r = 0; r < _gridSize; r++)
                    for (int c = 0; c < _gridSize; c++)
                        _bulbTypes[r * _gridSize + c] = (r % 2 == 0) ? BulbType.ColoredRed : BulbType.ColoredBlue;
            }

            if (_hasWarp)
            {
                // Corner bulbs are Warp type (8-direction flip)
                int[] corners = { 0, _gridSize - 1, _gridSize * (_gridSize - 1), _gridSize * _gridSize - 1 };
                foreach (int ci in corners) _bulbTypes[ci] = BulbType.Warp;
            }

            if (_hasDelayed)
            {
                // Center column is Delayed type
                for (int r = 0; r < _gridSize; r++)
                    _bulbTypes[r * _gridSize + _gridSize / 2] = BulbType.Delayed;
            }
        }

        void GenerateTargetPattern()
        {
            int n = _gridSize * _gridSize;
            if (_stageNum == 1)
            {
                // Stage 1: all off (lights-out goal)
                for (int i = 0; i < n; i++) _targetPattern[i] = false;
                return;
            }
            for (int i = 0; i < n; i++) _targetPattern[i] = Random.value > 0.5f;
        }

        void GenerateInitialState()
        {
            int n = _gridSize * _gridSize;
            System.Array.Copy(_targetPattern, _currentState, n);
            int shuffleMoves = _stageNum * 3 + 3;
            for (int k = 0; k < shuffleMoves; k++)
            {
                int idx = Random.Range(0, n);
                ApplyFlip(idx);
            }
            // Ensure initial state != target (avoid instant-clear at start)
            int attempts = 0;
            while (CheckComplete() && attempts < 20)
            {
                ApplyFlip(Random.Range(0, n));
                attempts++;
            }
        }

        void GenerateShiftPatterns()
        {
            _shiftPatterns = new bool[4][];
            int n = _gridSize * _gridSize;
            for (int p = 0; p < 4; p++)
            {
                _shiftPatterns[p] = new bool[n];
                for (int i = 0; i < n; i++) _shiftPatterns[p][i] = Random.value > 0.5f;
            }
            _targetPattern = _shiftPatterns[0];
        }

        void SpawnBulbs()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float available = (camSize * 2f) - topMargin - bottomMargin;
            float cellSize = Mathf.Min(available / _gridSize, (camWidth * 2f - 0.4f) / _gridSize, 1.8f);

            float startX = -((_gridSize - 1) * cellSize) * 0.5f;
            float startY = camSize - topMargin - cellSize * 0.5f;

            for (int i = 0; i < _gridSize * _gridSize; i++)
            {
                int row = i / _gridSize;
                int col = i % _gridSize;

                var go = new GameObject($"Bulb_{row}_{col}");
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(startX + col * cellSize, startY - row * cellSize, 0f);
                go.transform.localScale = Vector3.one * cellSize * 0.85f;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 1;
                var col2d = go.AddComponent<CircleCollider2D>();
                col2d.radius = 0.5f;

                _bulbObjects[i] = go;
                _bulbRenderers[i] = sr;
            }
        }

        void OnBulbTapped(int idx)
        {
            // Save undo frame
            var snapshot = new bool[_currentState.Length];
            System.Array.Copy(_currentState, snapshot, _currentState.Length);
            _history.Push(new UndoFrame
            {
                state = snapshot,
                delays = _pendingDelays.Select(e => (e.idx, e.triggerMove)).ToList(),
                shiftCount = _shiftCount,
            });

            if (_bulbTypes[idx] == BulbType.Delayed)
            {
                // Self flips immediately; neighbor flip is deferred
                _currentState[idx] = !_currentState[idx];
                // Only enqueue if not already pending for this index
                if (!_pendingDelays.Any(e => e.idx == idx))
                    _pendingDelays.Enqueue((idx, _moveCount + 1));
            }
            else
            {
                ApplyFlip(idx);
            }

            _moveCount++;
            ProcessDelayedFlips();

            // Shift target pattern every N moves (Stage 5)
            if (_hasShiftTarget && _shiftPatterns != null && _shiftPatterns.Length > 0
                && _moveCount > 0 && _moveCount % _shiftInterval == 0)
            {
                _shiftCount = (_shiftCount + 1) % _shiftPatterns.Length;
                _targetPattern = _shiftPatterns[_shiftCount];
                _ui.UpdateTargetPattern(_targetPattern, _gridSize);
            }

            UpdateAllSprites();
            StartCoroutine(TapPulse(idx));
            _ui.UpdateMoves(_maxMoves - _moveCount, _maxMoves);

            if (CheckComplete())
            {
                _isActive = false;
                StartCoroutine(ClearAnimation());
            }
            else if (_moveCount >= _maxMoves)
            {
                _isActive = false;
                StartCoroutine(FlashAllRed());
                _gameManager.OnGameOver();
            }
        }

        // Flip a bulb and its neighbors according to type rules
        void ApplyFlip(int idx)
        {
            int row = idx / _gridSize;
            int col = idx % _gridSize;
            BulbType type = _bulbTypes[idx];

            _currentState[idx] = !_currentState[idx];

            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };
            for (int d = 0; d < 4; d++)
            {
                int nr = row + dr[d];
                int nc = col + dc[d];
                if (nr < 0 || nr >= _gridSize || nc < 0 || nc >= _gridSize) continue;
                int ni = nr * _gridSize + nc;

                // Colored: only same-color neighbors are flipped
                if (type == BulbType.ColoredRed && _bulbTypes[ni] != BulbType.ColoredRed) continue;
                if (type == BulbType.ColoredBlue && _bulbTypes[ni] != BulbType.ColoredBlue) continue;

                _currentState[ni] = !_currentState[ni];
            }

            // Warp: additionally flip diagonal neighbors (total 8 directions)
            if (type == BulbType.Warp)
            {
                int[] ddr = { -1, -1, 1, 1 };
                int[] ddc = { -1, 1, -1, 1 };
                for (int d = 0; d < 4; d++)
                {
                    int nr = row + ddr[d];
                    int nc = col + ddc[d];
                    if (nr < 0 || nr >= _gridSize || nc < 0 || nc >= _gridSize) continue;
                    _currentState[nr * _gridSize + nc] = !_currentState[nr * _gridSize + nc];
                }
            }
        }

        // Apply only neighbor flips (for delayed bulbs triggered after N moves)
        void ApplyNeighborFlip(int idx)
        {
            int row = idx / _gridSize;
            int col = idx % _gridSize;
            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };
            for (int d = 0; d < 4; d++)
            {
                int nr = row + dr[d];
                int nc = col + dc[d];
                if (nr < 0 || nr >= _gridSize || nc < 0 || nc >= _gridSize) continue;
                _currentState[nr * _gridSize + nc] = !_currentState[nr * _gridSize + nc];
            }
        }

        void ProcessDelayedFlips()
        {
            int count = _pendingDelays.Count;
            for (int i = 0; i < count; i++)
            {
                var (idx, trigger) = _pendingDelays.Dequeue();
                if (_moveCount >= trigger)
                    ApplyNeighborFlip(idx);
                else
                    _pendingDelays.Enqueue((idx, trigger));
            }
        }

        bool CheckComplete()
        {
            for (int i = 0; i < _currentState.Length; i++)
                if (_currentState[i] != _targetPattern[i]) return false;
            return true;
        }

        void UpdateAllSprites()
        {
            for (int i = 0; i < _bulbObjects.Length; i++)
            {
                if (_bulbRenderers[i] == null) continue;
                _bulbRenderers[i].sprite = GetSprite(_bulbTypes[i], _currentState[i]);
            }
        }

        Sprite GetSprite(BulbType type, bool on)
        {
            return type switch
            {
                BulbType.ColoredRed  => on ? _spBulbRed     : _spBulbRedOff,
                BulbType.ColoredBlue => on ? _spBulbBlue    : _spBulbBlueOff,
                BulbType.Warp        => on ? _spBulbWarp    : _spBulbWarpOff,
                BulbType.Delayed     => on ? _spBulbDelayed : _spBulbDelayedOff,
                _                    => on ? _spBulbOn      : _spBulbOff,
            };
        }

        public void UndoMove()
        {
            if (_history.Count == 0) return;
            if (_undoUsedCount >= _maxUndo) return;
            if (_gameManager.State != LightSwitchGameManager.GameState.Playing) return;

            _undoUsedCount++;
            var frame = _history.Pop();
            System.Array.Copy(frame.state, _currentState, frame.state.Length);
            _moveCount = Mathf.Max(0, _moveCount - 1);

            // Restore pending delays and shift count
            _pendingDelays = new Queue<(int, int)>(frame.delays.Select(e => (e.idx, e.trigger)));
            _shiftCount = frame.shiftCount;
            if (_hasShiftTarget && _shiftPatterns != null && _shiftPatterns.Length > 0)
            {
                _targetPattern = _shiftPatterns[_shiftCount];
                _ui.UpdateTargetPattern(_targetPattern, _gridSize);
            }

            UpdateAllSprites();
            StartCoroutine(UndoPulse());
            _ui.UpdateMoves(_maxMoves - _moveCount, _maxMoves);
            _ui.UpdateUndo(_maxUndo - _undoUsedCount);
        }

        void ClearBulbs()
        {
            if (_bulbObjects == null) return;
            foreach (var go in _bulbObjects)
                if (go != null) Destroy(go);
        }

        IEnumerator TapPulse(int idx)
        {
            if (idx < 0 || idx >= _bulbObjects.Length || _bulbObjects[idx] == null) yield break;
            var t = _bulbObjects[idx].transform;
            Vector3 orig = t.localScale;
            float dur = 0.1f, elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                t.localScale = orig * (1f + 0.15f * Mathf.Sin(elapsed / dur * Mathf.PI));
                yield return null;
            }
            t.localScale = orig;
        }

        IEnumerator ClearAnimation()
        {
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    int i = row * _gridSize + col;
                    if (_bulbObjects[i] != null) StartCoroutine(ScalePulse(_bulbObjects[i].transform, 1.3f, 0.2f));
                }
                yield return new WaitForSeconds(0.05f);
            }
            for (int i = 0; i < _bulbRenderers.Length; i++)
                if (_bulbRenderers[i] != null) _bulbRenderers[i].color = new Color(1f, 0.9f, 0.2f);
            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < _bulbRenderers.Length; i++)
                if (_bulbRenderers[i] != null) _bulbRenderers[i].color = Color.white;

            _gameManager.OnStageClear(_maxMoves - _moveCount, _maxMoves, _undoUsedCount > 0);
        }

        IEnumerator ScalePulse(Transform t, float peak, float dur)
        {
            if (t == null) yield break;
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                t.localScale = orig * (1f + (peak - 1f) * Mathf.Sin(elapsed / dur * Mathf.PI));
                yield return null;
            }
            t.localScale = orig;
        }

        IEnumerator FlashAllRed()
        {
            for (int i = 0; i < _bulbRenderers.Length; i++)
                if (_bulbRenderers[i] != null) _bulbRenderers[i].color = new Color(1f, 0.2f, 0.2f);
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < _bulbRenderers.Length; i++)
                if (_bulbRenderers[i] != null) _bulbRenderers[i].color = Color.white;
        }

        IEnumerator UndoPulse()
        {
            for (int i = 0; i < _bulbRenderers.Length; i++)
                if (_bulbRenderers[i] != null) _bulbRenderers[i].color = new Color(0.3f, 0.7f, 1f);
            yield return new WaitForSeconds(0.2f);
            for (int i = 0; i < _bulbRenderers.Length; i++)
                if (_bulbRenderers[i] != null) _bulbRenderers[i].color = Color.white;
        }

        void OnDestroy()
        {
            ClearBulbs();
        }
    }
}
