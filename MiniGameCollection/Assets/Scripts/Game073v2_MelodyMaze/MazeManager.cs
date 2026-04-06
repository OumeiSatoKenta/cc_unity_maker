using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Common;

namespace Game073v2_MelodyMaze
{
    public class MazeManager : MonoBehaviour
    {
        [SerializeField] MelodyMazeGameManager _gameManager;
        [SerializeField] Sprite _spritePathTile;
        [SerializeField] Sprite _spriteWallTile;
        [SerializeField] Sprite _spriteJunctionTile;
        [SerializeField] Sprite _spriteNoteCorrect;
        [SerializeField] Sprite _spriteNoteDecoy;
        [SerializeField] Sprite _spriteNoteWrong;
        [SerializeField] Sprite _spriteChordNode;
        [SerializeField] Sprite _spritePlayer;
        [SerializeField] Sprite _spriteGoal;
        // _spriteTimingRing removed (visual timing feedback handled via color flash)

        // Stage params
        int _gridSize = 5;
        int _noteCount = 4;
        float _timeLimit = 60f;
        int _maxPreviewPlays = -1; // -1 = unlimited
        bool _hasChord = false;
        bool _hasDecoy = false;
        bool _hasMoving = false;

        // State
        bool _isActive = false;
        float _timer = 0f;
        int _score = 0;
        int _combo = 0;
        int _previewPlaysRemaining = 0;
        int _correctNotesHit = 0;
        int _totalCorrectNotes = 0;
        int _stageIndex = 0;
        int _accumulatedScore = 0;
        public int TotalScore => _accumulatedScore + _score;

        // Grid
        enum CellType { Empty, Wall, Path, Junction, NoteNode, Goal, Start, ChordNode, DecoyNode }
        CellType[,] _grid;
        int[,] _noteIndex; // which melody note (0-based), -1 if none

        // Player
        Vector2Int _playerPos;
        Vector2Int _goalPos;
        bool _isMoving = false;

        // Melody sequence (note indices 0..noteCount-1 for correct path)
        int[] _melody;

        // GameObjects
        GameObject[,] _cellObjects;
        GameObject _playerObj;
        SpriteRenderer _playerSr;
        GameObject _goalObj;

        // Note nodes
        List<NoteNodeData> _noteNodes = new List<NoteNodeData>();

        // Timing judgment
        bool _awaitingTimingTap = false;
        float _tapWindowStart = 0f;
        const float TapWindowDuration = 1.5f;
        const float PerfectWindow = 0.05f;
        const float GreatWindow = 0.12f;
        const float GoodWindow = 0.22f;

        // Swipe input
        Vector2 _swipeStart = Vector2.zero;
        bool _swipeStarted = false;
        const float SwipeThreshold = 30f;

        // Camera shake
        Camera _mainCam;
        Vector3 _camOrigin;
        bool _isShaking = false;

        // Moving notes (Stage5)
        float _moveNodeTimer = 0f;
        float _moveNodeInterval = 2.0f;

        class NoteNodeData
        {
            public Vector2Int gridPos;
            public int melodyIndex;   // which note in the melody (-1 = decoy/wrong)
            public bool isDecoy;
            public bool isChord;
            public bool isHit;
            public GameObject go;
            public SpriteRenderer sr;
        }

        void Awake()
        {
            _mainCam = Camera.main;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _stageIndex = stageIndex;
            _isActive = false;
            _accumulatedScore += _score;
            _score = 0;
            _combo = 0;
            _correctNotesHit = 0;
            _timer = 0f;
            _awaitingTimingTap = false;
            _swipeStarted = false;
            _isMoving = false;
            _moveNodeTimer = 0f;

            int[] gridSizes     = { 5, 6, 7, 7, 8 };
            int[] noteCounts    = { 4, 6, 8, 10, 12 };
            float[] timeLimits  = { 60f, 50f, 45f, 40f, 35f };
            int[] maxPreviews   = { -1, 5, 3, 2, 1 };

            _gridSize     = gridSizes[Mathf.Clamp(stageIndex, 0, 4)];
            _noteCount    = noteCounts[Mathf.Clamp(stageIndex, 0, 4)];
            _timeLimit    = timeLimits[Mathf.Clamp(stageIndex, 0, 4)] / Mathf.Max(0.1f, config.speedMultiplier);
            _maxPreviewPlays = maxPreviews[Mathf.Clamp(stageIndex, 0, 4)];
            _hasChord     = stageIndex >= 2;
            _hasDecoy     = stageIndex >= 3;
            _hasMoving    = stageIndex >= 4;
            _totalCorrectNotes = _noteCount;
            _previewPlaysRemaining = _maxPreviewPlays;

            _gameManager.UpdateScoreDisplay(_score);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateTimerDisplay(_timeLimit);
            _gameManager.UpdatePreviewPlays(_previewPlaysRemaining, _maxPreviewPlays);

            DestroyMaze();
            GenerateMaze();
            BuildMazeObjects();
            _isActive = true;
        }

        void DestroyMaze()
        {
            if (_cellObjects != null)
            {
                int rows = _cellObjects.GetLength(0);
                int cols = _cellObjects.GetLength(1);
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        if (_cellObjects[r, c] != null) Destroy(_cellObjects[r, c]);
            }
            if (_playerObj != null) Destroy(_playerObj);
            if (_goalObj != null) Destroy(_goalObj);
            _noteNodes.Clear();
        }

        void GenerateMaze()
        {
            int n = _gridSize;
            _grid = new CellType[n, n];
            _noteIndex = new int[n, n];
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    _grid[r, c] = CellType.Wall;
                    _noteIndex[r, c] = -1;
                }

            // Carve path with DFS
            var visited = new bool[n, n];
            var path = new List<Vector2Int>();
            var start = new Vector2Int(0, 0);
            _playerPos = start;
            _goalPos = new Vector2Int(n - 1, n - 1);

            CarvePathDFS(start, visited, path);
            if (path.Count < _noteCount + 2)
            {
                // Fallback: straight path
                path.Clear();
                for (int r = 0; r < n; r++) path.Add(new Vector2Int(r, 0));
                for (int c = 1; c < n; c++) path.Add(new Vector2Int(n - 1, c));
            }

            _grid[start.x, start.y] = CellType.Start;
            _grid[_goalPos.x, _goalPos.y] = CellType.Goal;

            // Place note nodes along correct path (skip start/goal)
            _melody = new int[_noteCount];
            for (int i = 0; i < _noteCount; i++) _melody[i] = i;
            ShuffleArray(_melody);

            int noteStep = Mathf.Max(1, (path.Count - 2) / _noteCount);
            for (int i = 0; i < _noteCount; i++)
            {
                int pathIdx = 1 + i * noteStep;
                pathIdx = Mathf.Clamp(pathIdx, 1, path.Count - 2);
                var pos = path[pathIdx];
                bool isChord = _hasChord && i == _noteCount / 2;
                _grid[pos.x, pos.y] = isChord ? CellType.ChordNode : CellType.NoteNode;
                _noteIndex[pos.x, pos.y] = i; // melody sequence index
            }

            // Place decoy nodes (Stage4+)
            if (_hasDecoy)
            {
                int decoyCount = Mathf.Min(3, _noteCount / 3);
                int placed = 0;
                for (int r = 0; r < n && placed < decoyCount; r++)
                    for (int c = 0; c < n && placed < decoyCount; c++)
                        if (_grid[r, c] == CellType.Path)
                        {
                            _grid[r, c] = CellType.DecoyNode;
                            _noteIndex[r, c] = -2; // decoy
                            placed++;
                        }
            }

            // Mark junctions (cells with 3+ open neighbors)
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    if (_grid[r, c] == CellType.Path)
                    {
                        int openNeighbors = CountOpenNeighbors(r, c);
                        if (openNeighbors >= 3)
                            _grid[r, c] = CellType.Junction;
                    }
        }

        bool CarvePathDFS(Vector2Int pos, bool[,] visited, List<Vector2Int> path)
        {
            int n = _gridSize;
            visited[pos.x, pos.y] = true;
            _grid[pos.x, pos.y] = CellType.Path;
            path.Add(pos);

            if (pos == _goalPos) return true;

            var dirs = new Vector2Int[] {
                new Vector2Int(1, 0), new Vector2Int(0, 1),
                new Vector2Int(-1, 0), new Vector2Int(0, -1)
            };
            Shuffle(dirs);

            foreach (var d in dirs)
            {
                var next = pos + d;
                if (next.x >= 0 && next.x < n && next.y >= 0 && next.y < n && !visited[next.x, next.y])
                {
                    if (CarvePathDFS(next, visited, path)) return true;
                }
            }
            path.RemoveAt(path.Count - 1);
            return false;
        }

        int CountOpenNeighbors(int r, int c)
        {
            int n = _gridSize;
            int count = 0;
            if (r > 0 && _grid[r-1, c] != CellType.Wall) count++;
            if (r < n-1 && _grid[r+1, c] != CellType.Wall) count++;
            if (c > 0 && _grid[r, c-1] != CellType.Wall) count++;
            if (c < n-1 && _grid[r, c+1] != CellType.Wall) count++;
            return count;
        }

        void BuildMazeObjects()
        {
            float camSize = _mainCam.orthographicSize;
            float camWidth = camSize * _mainCam.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            float cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, 1.4f);
            float mazeOriginY = (topMargin - bottomMargin) * 0.5f;

            float mazeW = cellSize * _gridSize;
            float mazeH = cellSize * _gridSize;
            float startX = -mazeW / 2f + cellSize / 2f;
            float startY = mazeOriginY + mazeH / 2f - cellSize / 2f;

            int n = _gridSize;
            _cellObjects = new GameObject[n, n];

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    float wx = startX + c * cellSize;
                    float wy = startY - r * cellSize;

                    CellType ct = _grid[r, c];
                    if (ct == CellType.Wall) continue;

                    var go = new GameObject($"Cell_{r}_{c}");
                    go.transform.SetParent(transform, false);
                    go.transform.position = new Vector3(wx, wy, 0f);
                    float scale = cellSize / 0.64f;
                    go.transform.localScale = new Vector3(scale, scale, 1f);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 0;

                    switch (ct)
                    {
                        case CellType.Path:
                            sr.sprite = _spritePathTile; break;
                        case CellType.Junction:
                            sr.sprite = _spriteJunctionTile; break;
                        case CellType.Start:
                            sr.sprite = _spritePathTile; break;
                        case CellType.Goal:
                            sr.sprite = _spritePathTile; break;
                        case CellType.NoteNode:
                            sr.sprite = _spriteNoteCorrect;
                            sr.sortingOrder = 1;
                            RegisterNoteNode(r, c, go, sr, false, false);
                            break;
                        case CellType.ChordNode:
                            sr.sprite = _spriteChordNode;
                            sr.sortingOrder = 1;
                            RegisterNoteNode(r, c, go, sr, false, true);
                            break;
                        case CellType.DecoyNode:
                            sr.sprite = _spriteNoteDecoy;
                            sr.sortingOrder = 1;
                            RegisterNoteNode(r, c, go, sr, true, false);
                            break;
                    }
                    _cellObjects[r, c] = go;
                }
            }

            // Goal object
            {
                int r = _goalPos.x, c = _goalPos.y;
                float wx = startX + c * cellSize;
                float wy = startY - r * cellSize;
                _goalObj = new GameObject("Goal");
                _goalObj.transform.SetParent(transform, false);
                _goalObj.transform.position = new Vector3(wx, wy, 0f);
                float scale = cellSize / 1.28f;
                _goalObj.transform.localScale = new Vector3(scale, scale, 1f);
                var sr = _goalObj.AddComponent<SpriteRenderer>();
                sr.sprite = _spriteGoal;
                sr.sortingOrder = 2;
            }

            // Player object
            {
                int r = _playerPos.x, c = _playerPos.y;
                float wx = startX + c * cellSize;
                float wy = startY - r * cellSize;
                _playerObj = new GameObject("Player");
                _playerObj.transform.SetParent(transform, false);
                _playerObj.transform.position = new Vector3(wx, wy, 0.1f);
                float scale = cellSize / 1.28f;
                _playerObj.transform.localScale = new Vector3(scale, scale, 1f);
                _playerSr = _playerObj.AddComponent<SpriteRenderer>();
                _playerSr.sprite = _spritePlayer;
                _playerSr.sortingOrder = 5;
            }

            // Store cell size for later movement
            _cellSize = cellSize;
            _mazeStartX = startX;
            _mazeStartY = startY;
        }

        float _cellSize;
        float _mazeStartX;
        float _mazeStartY;

        void RegisterNoteNode(int r, int c, GameObject go, SpriteRenderer sr, bool isDecoy, bool isChord)
        {
            var node = new NoteNodeData
            {
                gridPos = new Vector2Int(r, c),
                melodyIndex = _noteIndex[r, c],
                isDecoy = isDecoy,
                isChord = isChord,
                isHit = false,
                go = go,
                sr = sr
            };
            _noteNodes.Add(node);
        }

        public void SetActive(bool active) => _isActive = active;

        void Update()
        {
            if (!_isActive) return;

            // Timer countdown
            _timer += Time.deltaTime;
            float remaining = _timeLimit - _timer;
            _gameManager.UpdateTimerDisplay(remaining);

            if (remaining <= 0f)
            {
                _isActive = false;
                _gameManager.OnGameOver();
                return;
            }

            // Moving notes (Stage5)
            if (_hasMoving)
            {
                _moveNodeTimer += Time.deltaTime;
                if (_moveNodeTimer >= _moveNodeInterval)
                {
                    _moveNodeTimer = 0f;
                    SwapRandomNoteNodes();
                }
            }

            if (_isMoving) return;

            // Handle timing tap first (takes priority over swipe during timing window)
            if (_awaitingTimingTap)
            {
                HandleTimingTap();
                float elapsed = Time.time - _tapWindowStart;
                if (elapsed > TapWindowDuration + GoodWindow)
                {
                    _awaitingTimingTap = false;
                    RegisterTimingMiss();
                }
                return; // skip swipe input while awaiting timing tap
            }

            // Handle swipe input
            HandleSwipeInput();
        }

        void HandleSwipeInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _swipeStart = mouse.position.ReadValue();
                _swipeStarted = true;
                return;
            }

            if (_swipeStarted && mouse.leftButton.wasReleasedThisFrame)
            {
                _swipeStarted = false;
                Vector2 swipeEnd = mouse.position.ReadValue();
                Vector2 delta = swipeEnd - _swipeStart;

                if (delta.magnitude < SwipeThreshold) return;

                Vector2Int dir;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    dir = delta.x > 0 ? new Vector2Int(0, 1) : new Vector2Int(0, -1);
                else
                    dir = delta.y > 0 ? new Vector2Int(-1, 0) : new Vector2Int(1, 0);

                TryMovePlayer(dir);
            }
        }

        void TryMovePlayer(Vector2Int dir)
        {
            var next = _playerPos + dir;
            int n = _gridSize;
            if (next.x < 0 || next.x >= n || next.y < 0 || next.y >= n) return;
            if (_grid[next.x, next.y] == CellType.Wall) return;

            StartCoroutine(MovePlayerTo(next));
        }

        IEnumerator MovePlayerTo(Vector2Int target)
        {
            _isMoving = true;
            if (_playerObj == null) { _isMoving = false; yield break; }
            Vector3 startPos = _playerObj.transform.position;
            Vector3 endPos = GetWorldPos(target);
            float t = 0f;
            float duration = 0.15f;

            while (t < duration)
            {
                t += Time.deltaTime;
                _playerObj.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
                yield return null;
            }
            _playerObj.transform.position = endPos;
            _playerPos = target;
            _isMoving = false;

            OnPlayerArrived(target);
        }

        Vector3 GetWorldPos(Vector2Int gridPos)
        {
            float wx = _mazeStartX + gridPos.y * _cellSize;
            float wy = _mazeStartY - gridPos.x * _cellSize;
            return new Vector3(wx, wy, 0.1f);
        }

        void OnPlayerArrived(Vector2Int pos)
        {
            CellType ct = _grid[pos.x, pos.y];

            // Check goal
            if (pos == _goalPos)
            {
                _isActive = false;
                float accuracy = _totalCorrectNotes > 0
                    ? (float)_correctNotesHit / _totalCorrectNotes
                    : 0f;
                if (accuracy >= 0.5f)
                    _gameManager.OnStageClear();
                else
                    _gameManager.OnGameOver();
                return;
            }

            // Check note nodes
            if (ct == CellType.NoteNode || ct == CellType.ChordNode || ct == CellType.DecoyNode)
            {
                var node = FindNodeAt(pos);
                if (node != null && !node.isHit)
                {
                    if (node.isDecoy)
                    {
                        // Wrong node
                        node.isHit = true;
                        _combo = 0;
                        _score = Mathf.Max(0, _score - 50);
                        _gameManager.UpdateScoreDisplay(_score);
                        _gameManager.UpdateComboDisplay(_combo);
                        _gameManager.ShowJudgement("WRONG!", new Color(1f, 0.3f, 0.3f));
                        StartCoroutine(WrongNodeFeedback(node));
                        StartCoroutine(ShakeCamera());
                    }
                    else
                    {
                        // Correct node: start timing window
                        node.isHit = true;
                        _correctNotesHit++;
                        _combo++;
                        float multiplier = Mathf.Min(3.0f, 1.0f + _combo * 0.15f);
                        _score += Mathf.RoundToInt(150f * multiplier);
                        _gameManager.UpdateScoreDisplay(_score);
                        _gameManager.UpdateComboDisplay(_combo);
                        _awaitingTimingTap = true;
                        _tapWindowStart = Time.time;
                        StartCoroutine(NodeHitFeedback(node));
                    }
                }
            }
        }

        void HandleTimingTap()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (!mouse.leftButton.wasPressedThisFrame) return;

            float elapsed = Time.time - _tapWindowStart - (TapWindowDuration * 0.5f);
            float delta = Mathf.Abs(elapsed);

            _awaitingTimingTap = false;

            string judgement;
            Color color;
            int bonus;
            float multiplier = Mathf.Min(3.0f, 1.0f + _combo * 0.15f);

            if (delta <= PerfectWindow)
            {
                judgement = "PERFECT!";
                color = new Color(0f, 1f, 1f);
                bonus = Mathf.RoundToInt(80f * multiplier);
            }
            else if (delta <= GreatWindow)
            {
                judgement = "GREAT";
                color = new Color(0.5f, 1f, 0.5f);
                bonus = Mathf.RoundToInt(40f * multiplier);
            }
            else if (delta <= GoodWindow)
            {
                judgement = "GOOD";
                color = new Color(1f, 1f, 0.3f);
                bonus = 10;
            }
            else
            {
                RegisterTimingMiss();
                return;
            }

            _score += bonus;
            _gameManager.UpdateScoreDisplay(_score);
            _gameManager.ShowJudgement(judgement, color);
        }

        void RegisterTimingMiss()
        {
            _combo = 0;
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.ShowJudgement("MISS", new Color(1f, 0.4f, 0.4f));
        }

        NoteNodeData FindNodeAt(Vector2Int pos)
        {
            foreach (var node in _noteNodes)
                if (node.gridPos == pos) return node;
            return null;
        }

        IEnumerator NodeHitFeedback(NoteNodeData node)
        {
            if (node.go == null) yield break;
            Vector3 orig = node.go.transform.localScale;
            Color flashColor = new Color(1f, 1f, 0.5f);
            node.sr.color = flashColor;

            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float s = ratio < 0.5f ? 1f + 0.3f * (ratio / 0.5f) : 1.3f - 0.3f * ((ratio - 0.5f) / 0.5f);
                if (node.go != null) node.go.transform.localScale = orig * s;
                yield return null;
            }
            if (node.go != null)
            {
                node.go.transform.localScale = orig;
                node.sr.color = new Color(0.5f, 1f, 0.5f);
            }
        }

        IEnumerator WrongNodeFeedback(NoteNodeData node)
        {
            if (node.go == null) yield break;
            node.sr.color = new Color(1f, 0.2f, 0.2f);
            yield return new WaitForSeconds(0.3f);
            if (node.sr != null) node.sr.color = new Color(0.8f, 0.3f, 0.3f);
        }

        IEnumerator ShakeCamera()
        {
            if (_isShaking) yield break;
            if (_mainCam == null) yield break;
            _isShaking = true;
            _camOrigin = _mainCam.transform.position;
            float duration = 0.12f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float strength = 0.07f * (1f - t / duration);
                _mainCam.transform.position = _camOrigin + new Vector3(
                    Random.Range(-strength, strength),
                    Random.Range(-strength, strength), 0f);
                yield return null;
            }
            _mainCam.transform.position = _camOrigin;
            _isShaking = false;
        }

        void SwapRandomNoteNodes()
        {
            if (_noteNodes.Count < 2) return;
            int a = Random.Range(0, _noteNodes.Count);
            int b = Random.Range(0, _noteNodes.Count);
            if (a == b) return;

            var nodeA = _noteNodes[a];
            var nodeB = _noteNodes[b];

            if (nodeA.isHit || nodeB.isHit) return;
            if (nodeA.gridPos == _playerPos || nodeB.gridPos == _playerPos) return;

            // Swap positions
            Vector3 posA = GetWorldPos(nodeA.gridPos);
            Vector3 posB = GetWorldPos(nodeB.gridPos);
            nodeA.go.transform.position = posB;
            nodeB.go.transform.position = posA;

            Vector2Int tmp = nodeA.gridPos;
            nodeA.gridPos = nodeB.gridPos;
            nodeB.gridPos = tmp;

            // Update grid
            _grid[nodeA.gridPos.x, nodeA.gridPos.y] = nodeA.isDecoy ? CellType.DecoyNode :
                (nodeA.isChord ? CellType.ChordNode : CellType.NoteNode);
            _grid[nodeB.gridPos.x, nodeB.gridPos.y] = nodeB.isDecoy ? CellType.DecoyNode :
                (nodeB.isChord ? CellType.ChordNode : CellType.NoteNode);
            _noteIndex[nodeA.gridPos.x, nodeA.gridPos.y] = nodeA.melodyIndex;
            _noteIndex[nodeB.gridPos.x, nodeB.gridPos.y] = nodeB.melodyIndex;
        }

        void Shuffle<T>(T[] arr)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }

        void ShuffleArray(int[] arr) => Shuffle(arr);

        public void OnPreviewButtonPressed()
        {
            if (!_isActive) return;
            if (_maxPreviewPlays >= 0 && _previewPlaysRemaining <= 0) return;
            if (_maxPreviewPlays >= 0)
            {
                _previewPlaysRemaining--;
                _gameManager.UpdatePreviewPlays(_previewPlaysRemaining, _maxPreviewPlays);
            }
            // Visual preview: flash all correct note nodes in melody order
            StartCoroutine(PlayMelodyPreview());
        }

        IEnumerator PlayMelodyPreview()
        {
            // Sort note nodes by melody index and flash them in order
            var orderedNodes = new System.Collections.Generic.List<NoteNodeData>(_noteNodes);
            orderedNodes.Sort((a, b) => a.melodyIndex.CompareTo(b.melodyIndex));

            foreach (var node in orderedNodes)
            {
                if (node.isDecoy || node.go == null || node.sr == null) continue;
                Color orig = node.sr.color;
                Vector3 origScale = node.go.transform.localScale;
                node.sr.color = new Color(1f, 1f, 0.3f);
                float t = 0f;
                while (t < 0.25f)
                {
                    t += Time.deltaTime;
                    if (node.go == null || node.sr == null) break;
                    float s = 1f + 0.2f * Mathf.Sin(t / 0.25f * Mathf.PI);
                    node.go.transform.localScale = origScale * s;
                    yield return null;
                }
                if (node.sr != null) node.sr.color = orig;
                if (node.go != null) node.go.transform.localScale = origScale;
                yield return new WaitForSeconds(0.1f);
            }
        }

        void OnDestroy()
        {
            DestroyMaze();
        }
    }
}
