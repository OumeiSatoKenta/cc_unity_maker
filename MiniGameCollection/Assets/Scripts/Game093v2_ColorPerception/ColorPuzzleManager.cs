using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game093v2_ColorPerception
{
    /// <summary>
    /// VisibilityMask: for each view index, whether a tile is passable (true) or a wall (false).
    /// Index 0=normal, 1=ColorVisionA, 2=ColorVisionB
    /// </summary>
    public enum TileKind { Path, Wall, Start, Goal, ColorZone }

    public class ColorPuzzleManager : MonoBehaviour
    {
        [SerializeField] ColorPerceptionGameManager _gameManager;
        [SerializeField] ColorPerceptionUI _ui;
        [SerializeField] Sprite _sprPlayer;
        [SerializeField] Sprite _sprGoal;
        [SerializeField] Sprite _sprColorZone;

        bool _isActive;
        int _gridSize;
        int _movesLimit;
        int _movesUsed;
        int _viewSwitchCount;
        int _currentStageIndex;
        int _viewCount;
        int _currentView;
        int _turnsSinceChange;
        int _changePeriod;
        bool _hasCycleChange;

        Vector2Int _playerPos;
        Vector2Int _goalPos;

        Coroutine _viewSwitchCoroutine;
        Coroutine _bumpCoroutine;

        // Base tile kinds
        TileKind[,] _baseMap;

        // Per-tile per-view: true=passable, false=wall
        // _viewMasks[r, c, v]
        bool[,,] _viewMasks;

        // Track which tiles have been modified by color zones
        bool[,] _colorZoneTriggered;

        List<GameObject> _tileObjects = new List<GameObject>();
        SpriteRenderer[,] _tileRenderers;
        GameObject _playerObj;
        GameObject _goalObj;

        float _cellSize;
        float _zoneCenter;

        Camera _mainCamera;

        // Color palette per view
        static readonly Color[] ViewWallColors = {
            new Color(0.1f, 0.1f, 0.1f, 1f),           // view0: dark gray wall
            new Color(0.5f, 0f, 0.5f, 1f),              // view1: purple wall
            new Color(0f, 0.4f, 0.5f, 1f),              // view2: teal wall
        };
        static readonly Color[] ViewPathColors = {
            new Color(0.15f, 0.15f, 0.15f, 0.4f),       // view0: dim path
            new Color(0.4f, 0f, 0.4f, 0.4f),            // view1: dim purple path
            new Color(0f, 0.3f, 0.4f, 0.4f),            // view2: dim teal path
        };
        static readonly Color[] ViewPassableColors = {
            new Color(0.18f, 0.85f, 0.12f, 0.7f),       // view0: neon green passable
            new Color(0.83f, 0f, 0.98f, 0.7f),          // view1: neon purple passable
            new Color(0.01f, 0.86f, 1f, 0.7f),          // view2: cyan passable
        };

        // ===================== Stage map data =====================
        // Encoding: 0=Path, 1=Wall, 2=Start, 3=Goal, 4=ColorZone
        // Per-view masks: for each cell [v0passable, v1passable, v2passable]
        // true=can walk, false=wall in that view

        // Stage 1: 5x5, 2 views
        static readonly TileKind[,] Base1 = {
            { TileKind.Start, TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Path  },
            { TileKind.Wall,  TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Path  },
            { TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Wall,  TileKind.Path  },
            { TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Path,  TileKind.Path  },
            { TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Goal  },
        };
        // view0=normal sees walls as walls, view1=ColorA sees walls as passages
        static readonly bool[,,] Masks1 = {
            // row0
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
            // row1
            {{false,true,false},{true,true,true},{false,false,true},{true,true,true},{true,true,true}},
            // row2
            {{true,true,true},{true,true,true},{true,true,true},{false,true,false},{true,true,true}},
            // row3
            {{true,true,true},{false,true,false},{true,true,true},{true,true,true},{true,true,true}},
            // row4
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
        };

        // Stage 2: 5x5, 2 views, moves limit
        static readonly TileKind[,] Base2 = {
            { TileKind.Start, TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Path  },
            { TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Wall,  TileKind.Path  },
            { TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Path  },
            { TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Wall,  TileKind.Path  },
            { TileKind.Path,  TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Goal  },
        };
        static readonly bool[,,] Masks2 = {
            {{true,true,true},{true,true,true},{false,true,false},{true,true,true},{true,true,true}},
            {{true,true,true},{false,true,false},{true,true,true},{false,false,true},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
            {{true,true,true},{false,false,true},{true,true,true},{false,true,false},{true,true,true}},
            {{true,true,true},{true,true,true},{false,true,false},{true,true,true},{true,true,true}},
        };

        // Stage 3: 6x6, 3 views
        static readonly TileKind[,] Base3 = {
            { TileKind.Start, TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Path  },
            { TileKind.Wall,  TileKind.Wall,  TileKind.Path,  TileKind.Path,  TileKind.Wall,  TileKind.Path  },
            { TileKind.Path,  TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Path,  TileKind.Path  },
            { TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Path  },
            { TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Wall,  TileKind.Path  },
            { TileKind.Path,  TileKind.Path,  TileKind.Path,  TileKind.Wall,  TileKind.Path,  TileKind.Goal  },
        };
        // 3 views: view0 sees v0walls, view1 sees v1walls, view2 sees v2walls
        static readonly bool[,,] Masks3 = {
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
            {{false,true,false},{false,false,true},{true,true,true},{true,true,true},{false,true,false},{true,true,true}},
            {{true,true,true},{true,true,true},{false,true,false},{true,true,true},{true,true,true},{true,true,true}},
            {{true,true,true},{false,false,true},{true,true,true},{false,true,false},{true,true,true},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{false,true,false},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{false,false,true},{true,true,true},{true,true,true}},
        };

        // Stage 4: 6x6, 3 views, color zones
        static readonly TileKind[,] Base4 = {
            { TileKind.Start,     TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path      },
            { TileKind.Wall,      TileKind.Path,      TileKind.Wall,      TileKind.Path,      TileKind.Wall,      TileKind.Path      },
            { TileKind.Path,      TileKind.ColorZone, TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path      },
            { TileKind.Path,      TileKind.Wall,      TileKind.Path,      TileKind.Wall,      TileKind.Path,      TileKind.Path      },
            { TileKind.Path,      TileKind.Path,      TileKind.ColorZone, TileKind.Path,      TileKind.Wall,      TileKind.Path      },
            { TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Goal      },
        };
        static readonly bool[,,] Masks4 = {
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
            {{false,true,false},{true,true,true},{false,false,true},{true,true,true},{false,true,false},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
            {{true,true,true},{false,true,false},{true,true,true},{false,false,true},{true,true,true},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{false,true,false},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
        };

        // Stage 5: 7x7, 3 views, color zones + cycle change
        static readonly TileKind[,] Base5 = {
            { TileKind.Start,     TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path      },
            { TileKind.Wall,      TileKind.Path,      TileKind.Wall,      TileKind.Path,      TileKind.Wall,      TileKind.Path,      TileKind.Path      },
            { TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.ColorZone, TileKind.Path,      TileKind.Path,      TileKind.Path      },
            { TileKind.Path,      TileKind.Wall,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Wall,      TileKind.Path      },
            { TileKind.Path,      TileKind.Path,      TileKind.ColorZone, TileKind.Path,      TileKind.Wall,      TileKind.Path,      TileKind.Path      },
            { TileKind.Path,      TileKind.Wall,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.ColorZone, TileKind.Path      },
            { TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Path,      TileKind.Goal      },
        };
        static readonly bool[,,] Masks5 = {
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
            {{false,true,false},{true,true,true},{false,false,true},{true,true,true},{false,true,false},{true,true,true},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
            {{true,true,true},{false,true,false},{true,true,true},{true,true,true},{true,true,true},{false,false,true},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{false,true,false},{true,true,true},{true,true,true}},
            {{true,true,true},{false,false,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
            {{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true},{true,true,true}},
        };

        void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _movesUsed = 0;
            _viewSwitchCount = 0;
            _currentView = 0;
            _turnsSinceChange = 0;

            // Stage-specific settings
            switch (stageIndex)
            {
                case 0: _gridSize = 5; _movesLimit = 0; _viewCount = 2; _hasCycleChange = false; _changePeriod = 0; break;
                case 1: _gridSize = 5; _movesLimit = 20; _viewCount = 2; _hasCycleChange = false; _changePeriod = 0; break;
                case 2: _gridSize = 6; _movesLimit = 15; _viewCount = 3; _hasCycleChange = false; _changePeriod = 0; break;
                case 3: _gridSize = 6; _movesLimit = 12; _viewCount = 3; _hasCycleChange = false; _changePeriod = 0; break;
                case 4: _gridSize = 7; _movesLimit = 10; _viewCount = 3; _hasCycleChange = true; _changePeriod = 3; break;
                default: _gridSize = 5; _movesLimit = 0; _viewCount = 2; _hasCycleChange = false; _changePeriod = 0; break;
            }

            // Clear previous objects
            foreach (var go in _tileObjects) if (go != null) Destroy(go);
            _tileObjects.Clear();
            if (_playerObj != null) Destroy(_playerObj);
            if (_goalObj != null) Destroy(_goalObj);

            // Load map data
            _baseMap = GetBaseMap(stageIndex);
            var srcMasks = GetMasks(stageIndex);
            _viewMasks = CopyMasks(srcMasks, _gridSize);
            _colorZoneTriggered = new bool[_gridSize, _gridSize];

            // Ensure camera reference
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) { Debug.LogError("[ColorPuzzleManager] Main Camera not found"); return; }

            // Responsive layout
            float camSize = _mainCamera.orthographicSize;
            float camWidth = camSize * _mainCamera.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 3.5f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float maxCell = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, 0.9f);
            _cellSize = maxCell;
            _zoneCenter = camSize - topMargin - availableHeight / 2f;

            // Find special tiles
            _playerPos = FindTile(_baseMap, TileKind.Start);
            _goalPos = FindTile(_baseMap, TileKind.Goal);

            // Build tile visuals
            _tileRenderers = new SpriteRenderer[_gridSize, _gridSize];
            BuildGridVisuals();

            // Create player and goal objects
            _goalObj = CreateSpriteObj("Goal", _sprGoal, CellToWorld(_goalPos.x, _goalPos.y), 2);
            _playerObj = CreateSpriteObj("Player", _sprPlayer, CellToWorld(_playerPos.x, _playerPos.y), 5);

            _ui.UpdateMoves(_movesUsed, _movesLimit);
            _ui.UpdateViewButtons(_currentView, _viewCount);
            if (_hasCycleChange)
                _ui.UpdateCycleCountdown(_changePeriod - _turnsSinceChange);

            _isActive = true;
        }

        void BuildGridVisuals()
        {
            float cs = _cellSize * 0.92f;
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    TileKind kind = _baseMap[r, c];
                    if (kind == TileKind.Goal || kind == TileKind.Start) kind = TileKind.Path;

                    Vector3 pos = CellToWorld(r, c);

                    // Create one visual layer per view (we'll show/hide based on current view)
                    // Actually: create one tile object, update color per view change
                    var go = new GameObject($"Tile_{r}_{c}");
                    go.transform.position = pos;
                    go.transform.localScale = Vector3.one * cs;
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 1;

                    // Set tile sprite based on kind
                    if (kind == TileKind.ColorZone && _sprColorZone != null)
                    {
                        sr.sprite = _sprColorZone;
                        sr.color = new Color(0.18f, 0.85f, 0.12f, 0.8f);
                    }
                    else
                    {
                        // Create a simple quad by using a white texture
                        sr.sprite = CreateSquareSprite();
                        bool passable = _viewMasks[r, c, _currentView];
                        sr.color = passable ? ViewPassableColors[_currentView] : ViewWallColors[_currentView];
                    }

                    _tileObjects.Add(go);
                    _tileRenderers[r, c] = sr;
                }
            }
        }

        void RefreshTileVisuals()
        {
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    TileKind kind = _baseMap[r, c];
                    if (kind == TileKind.Goal || kind == TileKind.Start) continue;

                    var sr = _tileRenderers[r, c];
                    if (sr == null) continue;

                    if (kind == TileKind.ColorZone && !_colorZoneTriggered[r, c])
                    {
                        sr.color = new Color(0.18f, 0.85f, 0.12f, 0.8f);
                    }
                    else
                    {
                        bool passable = _viewMasks[r, c, _currentView];
                        sr.color = passable ? ViewPassableColors[_currentView] : ViewWallColors[_currentView];
                    }
                }
            }
        }

        Sprite _squareSprite;
        Sprite CreateSquareSprite()
        {
            if (_squareSprite != null) return _squareSprite;
            var tex = new Texture2D(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    bool border = x == 0 || x == 15 || y == 0 || y == 15;
                    tex.SetPixel(x, y, border ? new Color(1f, 1f, 1f, 0.5f) : Color.white);
                }
            tex.Apply();
            _squareSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            return _squareSprite;
        }

        Vector3 CellToWorld(int r, int c)
        {
            float startX = -(_gridSize * _cellSize) / 2f + _cellSize / 2f;
            float startY = (_gridSize * _cellSize) / 2f - _cellSize / 2f;
            return new Vector3(startX + c * _cellSize, _zoneCenter + startY - r * _cellSize, 0f);
        }

        Vector2Int FindTile(TileKind[,] map, TileKind target)
        {
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    if (map[r, c] == target) return new Vector2Int(r, c);
            return Vector2Int.zero;
        }

        GameObject CreateSpriteObj(string name, Sprite sprite, Vector3 pos, int order)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            float sc = _cellSize * 0.88f;
            go.transform.localScale = Vector3.one * sc;
            return go;
        }

        /// <summary>Called by UI buttons for view switching.</summary>
        public void SwitchView(int viewIndex)
        {
            if (!_isActive) return;
            if (viewIndex == _currentView) return;
            if (viewIndex < 0 || viewIndex >= _viewCount) return;

            _currentView = viewIndex;
            _viewSwitchCount++;
            _ui.UpdateViewButtons(_currentView, _viewCount);
            if (_viewSwitchCoroutine != null) StopCoroutine(_viewSwitchCoroutine);
            _viewSwitchCoroutine = StartCoroutine(ViewSwitchAnim());
        }

        IEnumerator ViewSwitchAnim()
        {
            // Fade all tiles out then in with new colors
            float t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0.2f, t / 0.1f);
                SetAllTileAlpha(alpha);
                yield return null;
            }
            RefreshTileVisuals();
            t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0.2f, 1f, t / 0.1f);
                SetAllTileAlpha(alpha);
                yield return null;
            }
            SetAllTileAlpha(1f);
        }

        void SetAllTileAlpha(float alpha)
        {
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                {
                    var sr = _tileRenderers[r, c];
                    if (sr != null)
                    {
                        var col = sr.color;
                        col.a = col.a > 0.01f ? alpha * (col.a > 0.5f ? 1f : 0.5f) : col.a;
                        sr.color = col;
                    }
                }
        }

        /// <summary>Called by UI directional buttons.</summary>
        public void TryMove(int dr, int dc)
        {
            if (!_isActive) return;

            int newR = _playerPos.x + dr;
            int newC = _playerPos.y + dc;

            // Boundary check
            if (newR < 0 || newR >= _gridSize || newC < 0 || newC >= _gridSize)
            {
                if (_bumpCoroutine != null) StopCoroutine(_bumpCoroutine);
                _bumpCoroutine = StartCoroutine(BumpAnim());
                return;
            }

            // Wall check (current view)
            if (!_viewMasks[newR, newC, _currentView])
            {
                // Hint: flash wall tile
                StartCoroutine(WallHintAnim(newR, newC));
                if (_bumpCoroutine != null) StopCoroutine(_bumpCoroutine);
                _bumpCoroutine = StartCoroutine(BumpAnim());
                return;
            }

            _playerPos = new Vector2Int(newR, newC);
            _playerObj.transform.position = CellToWorld(_playerPos.x, _playerPos.y);

            _movesUsed++;
            _ui.UpdateMoves(_movesUsed, _movesLimit);

            // Cycle change (Stage 5)
            if (_hasCycleChange)
            {
                _turnsSinceChange++;
                if (_turnsSinceChange >= _changePeriod)
                {
                    _turnsSinceChange = 0;
                    CycleMasks();
                    RefreshTileVisuals();
                }
                _ui.UpdateCycleCountdown(_changePeriod - _turnsSinceChange);
            }

            // Color zone check
            TileKind currentKind = _baseMap[_playerPos.x, _playerPos.y];
            if (currentKind == TileKind.ColorZone && !_colorZoneTriggered[_playerPos.x, _playerPos.y])
            {
                TriggerColorZone(_playerPos.x, _playerPos.y);
            }

            // Goal check
            if (_playerPos == _goalPos)
            {
                StartCoroutine(SuccessAnim());
                return;
            }

            // Moves exceeded check (after move)
            if (_movesLimit > 0 && _movesUsed >= _movesLimit)
            {
                if (_playerPos != _goalPos)
                {
                    StartCoroutine(GameOverAnim());
                }
                return;
            }
        }

        void TriggerColorZone(int zr, int zc)
        {
            _colorZoneTriggered[zr, zc] = true;
            // Toggle wall/passable for adjacent tiles (within 2 cells)
            for (int r = Mathf.Max(0, zr - 2); r <= Mathf.Min(_gridSize - 1, zr + 2); r++)
            {
                for (int c = Mathf.Max(0, zc - 2); c <= Mathf.Min(_gridSize - 1, zc + 2); c++)
                {
                    if (_baseMap[r, c] == TileKind.Wall || _baseMap[r, c] == TileKind.Path)
                    {
                        // Flip all view masks for that tile
                        for (int v = 0; v < _viewCount; v++)
                            _viewMasks[r, c, v] = !_viewMasks[r, c, v];
                    }
                }
            }
            StartCoroutine(ColorZoneFlashAnim(zr, zc));
            RefreshTileVisuals();
        }

        void CycleMasks()
        {
            // Rotate view masks: view0 gets view1's masks, view1 gets view2's, view2 gets view0's
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    bool v0 = _viewMasks[r, c, 0];
                    bool v1 = _viewCount > 1 ? _viewMasks[r, c, 1] : v0;
                    bool v2 = _viewCount > 2 ? _viewMasks[r, c, 2] : v0;
                    _viewMasks[r, c, 0] = v1;
                    if (_viewCount > 1) _viewMasks[r, c, 1] = v2;
                    if (_viewCount > 2) _viewMasks[r, c, 2] = v0;
                }
            }
        }

        IEnumerator SuccessAnim()
        {
            _isActive = false;
            Vector3 baseScale = _playerObj.transform.localScale;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.3f;
                float sc = ratio < 0.5f ? Mathf.Lerp(1f, 1.4f, ratio * 2f) : Mathf.Lerp(1.4f, 1f, (ratio - 0.5f) * 2f);
                if (_playerObj != null) _playerObj.transform.localScale = baseScale * sc;
                float flash = Mathf.Sin(ratio * Mathf.PI);
                if (_playerObj != null)
                    _playerObj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.yellow, flash);
                yield return null;
            }
            if (_playerObj != null)
            {
                _playerObj.transform.localScale = baseScale;
                _playerObj.GetComponent<SpriteRenderer>().color = Color.white;
            }
            _gameManager.OnGoalReached(_movesUsed, _movesLimit, _viewSwitchCount);
        }

        IEnumerator GameOverAnim()
        {
            _isActive = false;
            float t = 0f;
            Vector3 camBase = _mainCamera.transform.position;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.4f;
                float flash = Mathf.Sin(ratio * Mathf.PI * 3f);
                if (_playerObj != null)
                    _playerObj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.red, Mathf.Abs(flash));
                float shake = (1f - ratio) * 0.15f;
                _mainCamera.transform.position = camBase + new Vector3(Random.Range(-shake, shake), Random.Range(-shake, shake), 0f);
                yield return null;
            }
            _mainCamera.transform.position = camBase;
            if (_playerObj != null) _playerObj.GetComponent<SpriteRenderer>().color = Color.white;
            _gameManager.OnMovesExceeded();
        }

        IEnumerator BumpAnim()
        {
            if (_playerObj == null) yield break;
            Vector3 baseScale = _playerObj.transform.localScale;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                if (_playerObj == null) yield break;
                float ratio = elapsed / 0.15f;
                float sc = ratio < 0.5f ? Mathf.Lerp(1f, 0.85f, ratio * 2f) : Mathf.Lerp(0.85f, 1f, (ratio - 0.5f) * 2f);
                _playerObj.transform.localScale = baseScale * sc;
                _playerObj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.red, Mathf.Sin(ratio * Mathf.PI));
                yield return null;
            }
            if (_playerObj != null)
            {
                _playerObj.transform.localScale = baseScale;
                _playerObj.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        IEnumerator WallHintAnim(int r, int c)
        {
            var sr = _tileRenderers[r, c];
            if (sr == null) yield break;
            Color baseColor = sr.color;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.2f;
                sr.color = Color.Lerp(baseColor, Color.white, Mathf.Sin(ratio * Mathf.PI));
                yield return null;
            }
            sr.color = baseColor;
        }

        IEnumerator ColorZoneFlashAnim(int r, int c)
        {
            var sr = _tileRenderers[r, c];
            if (sr == null) yield break;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.3f;
                sr.color = Color.Lerp(new Color(0.18f, 0.85f, 0.12f, 0.8f), Color.white, Mathf.Sin(ratio * Mathf.PI));
                yield return null;
            }
            if (sr != null) sr.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // zone is spent
        }

        // === Map data accessors ===
        TileKind[,] GetBaseMap(int stageIndex)
        {
            switch (stageIndex)
            {
                case 0: return CopyBaseMap(Base1);
                case 1: return CopyBaseMap(Base2);
                case 2: return CopyBaseMap(Base3);
                case 3: return CopyBaseMap(Base4);
                case 4: return CopyBaseMap(Base5);
                default: return CopyBaseMap(Base1);
            }
        }

        bool[,,] GetMasks(int stageIndex)
        {
            switch (stageIndex)
            {
                case 0: return Masks1;
                case 1: return Masks2;
                case 2: return Masks3;
                case 3: return Masks4;
                case 4: return Masks5;
                default: return Masks1;
            }
        }

        TileKind[,] CopyBaseMap(TileKind[,] src)
        {
            int rows = src.GetLength(0), cols = src.GetLength(1);
            var dst = new TileKind[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    dst[r, c] = src[r, c];
            return dst;
        }

        bool[,,] CopyMasks(bool[,,] src, int size)
        {
            var dst = new bool[size, size, 3];
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    for (int v = 0; v < 3; v++)
                    {
                        if (r < src.GetLength(0) && c < src.GetLength(1) && v < src.GetLength(2))
                            dst[r, c, v] = src[r, c, v];
                        else
                            dst[r, c, v] = true;
                    }
            return dst;
        }

        void OnDestroy()
        {
            if (_squareSprite != null)
            {
                if (_squareSprite.texture != null) Destroy(_squareSprite.texture);
                Destroy(_squareSprite);
            }
        }
    }
}
