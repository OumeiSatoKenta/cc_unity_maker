using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game091v2_TimeBlender
{
    public enum TileType
    {
        Empty = 0,
        Wall = 1,
        Goal = 2,
        Start = 3,
        Paradox = 4,
        BridgePast = 5,   // Wall in past, Empty in future
        TreePast = 6,     // Empty in past, Wall in future
    }

    public enum Era
    {
        Past = 0,
        Present = 1,
        Future = 2,
    }

    public class PuzzleManager : MonoBehaviour
    {
        [SerializeField] TimeBlenderGameManager _gameManager;
        [SerializeField] TimeBlenderUI _ui;
        [SerializeField] Sprite _sprEmpty;
        [SerializeField] Sprite _sprWall;
        [SerializeField] Sprite _sprWallFuture;
        [SerializeField] Sprite _sprBridge;
        [SerializeField] Sprite _sprTree;
        [SerializeField] Sprite _sprGoal;
        [SerializeField] Sprite _sprStart;
        [SerializeField] Sprite _sprParadox;
        [SerializeField] Sprite _sprPlayer;

        bool _isActive;
        int _gridSize;
        int _movesLimit;
        int _movesUsed;
        int _paradoxLimit;
        int _paradoxCount;
        int _currentStageIndex;
        bool _hasThreeEras;

        Era _currentEra = Era.Past;
        Vector2Int _playerPos;
        Coroutine _popCoroutine;

        // 3D array: [era][row][col]
        TileType[,,] _baseMap;
        SpriteRenderer[,] _tileRenderers;
        SpriteRenderer _playerRenderer;
        GameObject _playerObj;

        float _cellSize;
        Vector2 _gridOrigin;

        // Stage data: [stageIndex][era][row][col]
        static readonly TileType[][,,] StageData = BuildStageData();

        static TileType[][,,] BuildStageData()
        {
            var stages = new TileType[5][,,];

            // Stage 1: 4x4, 2 eras (past/future), simple 1-switch puzzle
            // W=Wall, E=Empty, G=Goal, S=Start, P=Paradox, B=BridgePast, T=TreePast
            stages[0] = new TileType[2, 4, 4]
            {
                // Past (era 0)
                {
                    { TileType.Start, TileType.Empty, TileType.Wall,  TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty },
                    { TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Goal  },
                },
                // Future (era 2)
                {
                    { TileType.Start, TileType.Wall,  TileType.Empty, TileType.Empty },
                    { TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Goal  },
                },
            };

            // Stage 2: 5x5, 2 eras, bridge mechanic
            stages[1] = new TileType[2, 5, 5]
            {
                // Past
                {
                    { TileType.Start, TileType.Empty, TileType.TreePast, TileType.Wall,  TileType.Wall  },
                    { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Goal  },
                },
                // Future
                {
                    { TileType.Start, TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Wall  },
                    { TileType.Wall,  TileType.Empty, TileType.BridgePast, TileType.Empty, TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Goal  },
                },
            };

            // Stage 3: 5x5, 2 eras, causal chain (3 objects affect each other)
            stages[2] = new TileType[2, 5, 5]
            {
                // Past
                {
                    { TileType.Start, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Wall  },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.TreePast, TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.TreePast, TileType.Goal },
                },
                // Future
                {
                    { TileType.Start, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Wall  },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.BridgePast },
                    { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Goal  },
                },
            };

            // Stage 4: 6x6, 2 eras, paradox zones added, limit=12, paradox_limit=2
            stages[3] = new TileType[2, 6, 6]
            {
                // Past
                {
                    { TileType.Start, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Wall  },
                    { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Paradox, TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Paradox, TileType.Empty },
                    { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Goal  },
                },
                // Future
                {
                    { TileType.Start, TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Wall  },
                    { TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty, TileType.Paradox, TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.Paradox, TileType.Empty, TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Goal  },
                },
            };

            // Stage 5: 6x6, 3 eras (past/present/future), hard puzzle
            stages[4] = new TileType[3, 6, 6]
            {
                // Past (era 0)
                {
                    { TileType.Start, TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Wall,  TileType.Wall  },
                    { TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.TreePast, TileType.Empty, TileType.Wall,  TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Paradox, TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Goal  },
                },
                // Present (era 1)
                {
                    { TileType.Start, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Wall  },
                    { TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Wall  },
                    { TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Wall,  TileType.Empty, TileType.Goal  },
                },
                // Future (era 2)
                {
                    { TileType.Start, TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Wall  },
                    { TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Wall  },
                    { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.BridgePast, TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Empty },
                    { TileType.Empty, TileType.Empty, TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty },
                    { TileType.Wall,  TileType.Wall,  TileType.Empty, TileType.Empty, TileType.Empty, TileType.Goal  },
                },
            };

            return stages;
        }

        static readonly int[] GridSizes  = { 4, 5, 5, 6, 6 };
        static readonly int[] MoveLimits = { 0, 20, 15, 12, 10 };
        static readonly int[] ParadoxLimits = { 3, 3, 3, 2, 2 };

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _currentStageIndex = stageIndex;
            _gridSize = GridSizes[Mathf.Clamp(stageIndex, 0, GridSizes.Length - 1)];
            _movesLimit = MoveLimits[Mathf.Clamp(stageIndex, 0, MoveLimits.Length - 1)];
            _paradoxLimit = ParadoxLimits[Mathf.Clamp(stageIndex, 0, ParadoxLimits.Length - 1)];
            _paradoxCount = 0;
            _movesUsed = 0;
            _hasThreeEras = stageIndex >= 4;
            _currentEra = Era.Past;
            _isActive = true;

            _baseMap = StageData[Mathf.Clamp(stageIndex, 0, StageData.Length - 1)];

            ClearTileObjects();
            BuildGrid();

            _ui.UpdateMoves(_movesUsed, _movesLimit);
            _ui.UpdateParadox(_paradoxCount, _paradoxLimit);
            _ui.UpdateEra(_currentEra, _hasThreeEras);
        }

        void ClearTileObjects()
        {
            if (_tileRenderers != null)
            {
                foreach (var sr in _tileRenderers)
                    if (sr != null) Destroy(sr.gameObject);
            }
            if (_playerObj != null) Destroy(_playerObj);
        }

        void BuildGrid()
        {
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[PuzzleManager] Camera.main is null"); return; }
            float camSize = cam.orthographicSize;
            float camWidth = camSize * cam.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float availH = camSize * 2f - topMargin - bottomMargin;
            float maxCellSize = 1.5f;
            _cellSize = Mathf.Min(availH / _gridSize, camWidth * 2f / _gridSize, maxCellSize);

            float totalW = _cellSize * _gridSize;
            float totalH = _cellSize * _gridSize;
            _gridOrigin = new Vector2(-totalW * 0.5f + _cellSize * 0.5f,
                                      (camSize - topMargin) - _cellSize * 0.5f);

            _tileRenderers = new SpriteRenderer[_gridSize, _gridSize];

            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    var go = new GameObject($"Tile_{row}_{col}");
                    go.transform.SetParent(transform, false);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 0;
                    go.transform.localPosition = GridToWorld(row, col);
                    go.transform.localScale = Vector3.one * _cellSize * 0.95f;

                    _tileRenderers[row, col] = sr;

                    TileType tileType = GetTile(row, col, _currentEra);
                    sr.sprite = GetSpriteForTile(tileType, _currentEra);

                    if (tileType == TileType.Start)
                        _playerPos = new Vector2Int(row, col);
                }
            }

            // Player object
            _playerObj = new GameObject("Player");
            _playerObj.transform.SetParent(transform, false);
            _playerRenderer = _playerObj.AddComponent<SpriteRenderer>();
            _playerRenderer.sprite = _sprPlayer;
            _playerRenderer.sortingOrder = 5;
            _playerObj.transform.localScale = Vector3.one * _cellSize * 0.85f;
            _playerObj.transform.localPosition = GridToWorld(_playerPos.x, _playerPos.y);
        }

        Vector3 GridToWorld(int row, int col)
        {
            return new Vector3(
                _gridOrigin.x + col * _cellSize,
                _gridOrigin.y - row * _cellSize,
                0f
            );
        }

        TileType GetTile(int row, int col, Era era)
        {
            int eraIndex = (int)era;
            int eraCount = _baseMap.GetLength(0);
            int eraIdx = Mathf.Clamp(eraIndex, 0, eraCount - 1);
            return _baseMap[eraIdx, row, col];
        }

        bool IsPassable(TileType t, Era era)
        {
            switch (t)
            {
                case TileType.Empty:
                case TileType.Goal:
                case TileType.Start:
                case TileType.Paradox:
                    return true;
                case TileType.Wall:
                    return false;
                case TileType.BridgePast:
                    // Wall in past, Empty/bridge in future
                    return era == Era.Future || era == Era.Present;
                case TileType.TreePast:
                    // Tree (passable) in past, Wall in future
                    return era == Era.Past || era == Era.Present;
                default:
                    return false;
            }
        }

        Sprite GetSpriteForTile(TileType t, Era era)
        {
            switch (t)
            {
                case TileType.Empty: return _sprEmpty;
                case TileType.Wall:
                    return era == Era.Future ? _sprWallFuture : _sprWall;
                case TileType.Goal: return _sprGoal;
                case TileType.Start: return _sprStart;
                case TileType.Paradox: return _sprParadox;
                case TileType.BridgePast:
                    return (era == Era.Future || era == Era.Present) ? _sprBridge : _sprWall;
                case TileType.TreePast:
                    return (era == Era.Past) ? _sprTree : _sprWall;
                default: return _sprEmpty;
            }
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        void Update()
        {
            if (!_isActive) return;
            if (!_gameManager.IsPlaying) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                var cam = Camera.main;
                if (cam == null) return;
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
                HandleWorldTap(worldPos);
            }
        }

        void HandleWorldTap(Vector3 worldPos)
        {
            // Find which tile was tapped
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    Vector3 tilePos = GridToWorld(row, col);
                    float half = _cellSize * 0.5f;
                    if (worldPos.x >= tilePos.x - half && worldPos.x <= tilePos.x + half &&
                        worldPos.y >= tilePos.y - half && worldPos.y <= tilePos.y + half)
                    {
                        TryMovePlayer(new Vector2Int(row, col));
                        return;
                    }
                }
            }
        }

        void TryMovePlayer(Vector2Int target)
        {
            // Must be adjacent (4-directional)
            int dr = Mathf.Abs(target.x - _playerPos.x);
            int dc = Mathf.Abs(target.y - _playerPos.y);
            if ((dr == 1 && dc == 0) || (dr == 0 && dc == 1))
            {
                TileType targetTile = GetTile(target.x, target.y, _currentEra);
                if (IsPassable(targetTile, _currentEra))
                {
                    _playerPos = target;
                    _playerObj.transform.localPosition = GridToWorld(_playerPos.x, _playerPos.y);

                    bool paradoxFree = targetTile != TileType.Paradox;
                    _movesUsed++;

                    if (_movesLimit > 0)
                        _ui.UpdateMoves(_movesUsed, _movesLimit);

                    _gameManager.OnPlayerMoved(paradoxFree);

                    // Pop animation (individual coroutine to avoid stopping Flash/ScreenFlash)
                    if (_popCoroutine != null) StopCoroutine(_popCoroutine);
                    _popCoroutine = StartCoroutine(PopAnimation(_playerObj.transform, _cellSize * 0.85f));

                    if (targetTile == TileType.Paradox)
                    {
                        _paradoxCount++;
                        StartCoroutine(FlashTile(target, Color.magenta));
                        _gameManager.OnParadoxOccurred(_paradoxLimit - _paradoxCount);
                        _ui.UpdateParadox(_paradoxCount, _paradoxLimit);
                    }
                    else if (targetTile == TileType.Goal)
                    {
                        _gameManager.OnGoalReached(_movesUsed, _movesLimit);
                    }

                    // Moves limit check
                    if (_movesLimit > 0 && _movesUsed >= _movesLimit && targetTile != TileType.Goal)
                    {
                        if (_gameManager.IsPlaying)
                        {
                            _isActive = false;
                            _gameManager.OnParadoxOccurred(0);
                        }
                    }
                }
                else
                {
                    StartCoroutine(FlashTile(target, Color.red));
                }
            }
        }

        public void SwitchEra(Era newEra)
        {
            if (!_isActive) return;
            if (!_gameManager.IsPlaying) return;
            if (newEra == _currentEra) return;

            // Only allow 3-era mode in stage 5
            if (newEra == Era.Present && !_hasThreeEras) return;

            _currentEra = newEra;
            _gameManager.OnEraSwitched();

            // Update all tile sprites
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    TileType tileType = GetTile(row, col, _currentEra);
                    _tileRenderers[row, col].sprite = GetSpriteForTile(tileType, _currentEra);
                }
            }

            _ui.UpdateEra(_currentEra, _hasThreeEras);

            // Check if player is now on a wall (paradox)
            TileType playerTile = GetTile(_playerPos.x, _playerPos.y, _currentEra);
            if (!IsPassable(playerTile, _currentEra))
            {
                _paradoxCount++;
                _ui.UpdateParadox(_paradoxCount, _paradoxLimit);
                StartCoroutine(ScreenFlash(GetEraColor(newEra) * 0.5f + Color.red * 0.5f));
                _gameManager.OnParadoxOccurred(_paradoxLimit - _paradoxCount);
            }
            else
            {
                StartCoroutine(ScreenFlash(GetEraColor(newEra)));
                StartCoroutine(GridWaveAnimation());
            }
        }

        Color GetEraColor(Era era)
        {
            return era switch
            {
                Era.Past => new Color(1f, 0.6f, 0.2f, 0.4f),
                Era.Present => new Color(0.3f, 0.9f, 0.5f, 0.4f),
                Era.Future => new Color(0.3f, 0.7f, 1f, 0.4f),
                _ => Color.white * 0.3f,
            };
        }

        IEnumerator PopAnimation(Transform t, float baseScale)
        {
            float dur = 0.2f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                if (t == null) yield break;
                float ratio = elapsed / dur;
                float s = ratio < 0.5f
                    ? Mathf.Lerp(baseScale, baseScale * 1.3f, ratio * 2f)
                    : Mathf.Lerp(baseScale * 1.3f, baseScale, (ratio - 0.5f) * 2f);
                t.localScale = Vector3.one * s;
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (t != null) t.localScale = Vector3.one * baseScale;
        }

        IEnumerator FlashTile(Vector2Int pos, Color flashColor)
        {
            var sr = _tileRenderers[pos.x, pos.y];
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = flashColor;
            yield return new WaitForSeconds(0.3f);
            sr.color = orig;
        }

        IEnumerator ScreenFlash(Color col)
        {
            // Flash all tiles briefly
            Color[] origColors = new Color[_gridSize * _gridSize];
            int idx = 0;
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    origColors[idx++] = _tileRenderers[r, c].color;

            idx = 0;
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    _tileRenderers[r, c].color = col;

            yield return new WaitForSeconds(0.15f);

            idx = 0;
            for (int r = 0; r < _gridSize; r++)
                for (int c = 0; c < _gridSize; c++)
                    _tileRenderers[r, c].color = origColors[idx++];
        }

        IEnumerator GridWaveAnimation()
        {
            float baseScale = _cellSize * 0.95f;
            float peakScale = baseScale * 1.1f;
            int steps = _gridSize * 2 - 1;

            for (int wave = 0; wave < steps; wave++)
            {
                if (_tileRenderers == null) yield break;
                for (int r = 0; r < _gridSize; r++)
                    for (int c = 0; c < _gridSize; c++)
                        if (r + c == wave && _tileRenderers[r, c] != null)
                            _tileRenderers[r, c].transform.localScale = Vector3.one * peakScale;

                yield return new WaitForSeconds(0.04f);

                if (_tileRenderers == null) yield break;
                for (int r = 0; r < _gridSize; r++)
                    for (int c = 0; c < _gridSize; c++)
                        if (r + c == wave && _tileRenderers[r, c] != null)
                            _tileRenderers[r, c].transform.localScale = Vector3.one * baseScale;
            }
        }
    }
}
