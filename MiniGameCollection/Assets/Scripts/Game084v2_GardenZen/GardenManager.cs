using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game084v2_GardenZen
{
    public enum PlacementType { None = 0, Stone1 = 1, Stone2 = 2, Stone3 = 3, Plant1 = 4, Plant2 = 5, Decoration = 6, Sand = 7 }

    public class GardenManager : MonoBehaviour
    {
        [SerializeField] GardenZenGameManager _gameManager;
        [SerializeField] GardenZenUI _ui;

        [SerializeField] Sprite _gridCellSprite;
        [SerializeField] Sprite _gridTargetSprite;
        [SerializeField] Sprite _stone1Sprite;
        [SerializeField] Sprite _stone2Sprite;
        [SerializeField] Sprite _stone3Sprite;
        [SerializeField] Sprite _plant1Sprite;
        [SerializeField] Sprite _plant2Sprite;
        [SerializeField] Sprite _decorationSprite;
        [SerializeField] Sprite _sandPatternSprite;

        // Grid state
        int _gridSize;
        PlacementType[,] _currentGrid;
        PlacementType[,] _targetGrid;
        bool[,] _sandGrid;

        // Cell visual objects
        GameObject[,] _cellBases;
        GameObject[,] _cellObjects;
        GameObject[,] _sandObjects;
        GameObject[,] _targetMarkers;

        float _cellSize;
        Vector3 _gridOrigin;

        public PlacementType SelectedType { get; set; } = PlacementType.Stone1;
        bool _isDrawingSand;
        bool _isActive;

        int _totalScore;
        float _comboMultiplier = 1.0f;
        int _stageIndex;

        public int TotalScore => _totalScore;

        // Stage configs
        static readonly int[] GridSizes = { 4, 4, 5, 5, 6 };
        static readonly PlacementType[][] TargetPatterns = new PlacementType[5][];

        void Awake()
        {
            InitTargetPatterns();
        }

        static void InitTargetPatterns()
        {
            // Stage 0 (4x4): 3 stones
            TargetPatterns[0] = new PlacementType[]
            {
                PlacementType.None,   PlacementType.Stone1, PlacementType.None,  PlacementType.None,
                PlacementType.None,   PlacementType.None,   PlacementType.Stone2,PlacementType.None,
                PlacementType.Stone3, PlacementType.None,   PlacementType.None,  PlacementType.None,
                PlacementType.None,   PlacementType.None,   PlacementType.None,  PlacementType.None
            };
            // Stage 1 (4x4): 3 stones + 2 plants (symmetric)
            TargetPatterns[1] = new PlacementType[]
            {
                PlacementType.None,   PlacementType.Stone1, PlacementType.Stone1, PlacementType.None,
                PlacementType.Plant1, PlacementType.None,   PlacementType.None,   PlacementType.Plant1,
                PlacementType.None,   PlacementType.Stone2, PlacementType.Stone2, PlacementType.None,
                PlacementType.None,   PlacementType.None,   PlacementType.None,   PlacementType.None
            };
            // Stage 2 (5x5): 4 stones + 2 plants + 1 deco
            TargetPatterns[2] = new PlacementType[]
            {
                PlacementType.None,    PlacementType.Stone1,  PlacementType.None,       PlacementType.Stone1,  PlacementType.None,
                PlacementType.Plant1,  PlacementType.None,    PlacementType.Decoration,  PlacementType.None,    PlacementType.Plant1,
                PlacementType.None,    PlacementType.Stone2,  PlacementType.None,        PlacementType.Stone2,  PlacementType.None,
                PlacementType.None,    PlacementType.None,    PlacementType.None,        PlacementType.None,    PlacementType.None,
                PlacementType.None,    PlacementType.None,    PlacementType.None,        PlacementType.None,    PlacementType.None
            };
            // Stage 3 (5x5): 5 stones + 3 plants + 2 deco
            TargetPatterns[3] = new PlacementType[]
            {
                PlacementType.Stone3,  PlacementType.None,    PlacementType.Stone1,      PlacementType.None,    PlacementType.Stone3,
                PlacementType.None,    PlacementType.Plant2,  PlacementType.None,        PlacementType.Plant2,  PlacementType.None,
                PlacementType.Stone2,  PlacementType.None,    PlacementType.Decoration,  PlacementType.None,    PlacementType.Stone2,
                PlacementType.None,    PlacementType.Plant1,  PlacementType.None,        PlacementType.Plant1,  PlacementType.None,
                PlacementType.None,    PlacementType.None,    PlacementType.Decoration,  PlacementType.None,    PlacementType.None
            };
            // Stage 4 (6x6): 6 stones + 4 plants + 3 deco (free creativity)
            TargetPatterns[4] = new PlacementType[]
            {
                PlacementType.Stone3,  PlacementType.None,    PlacementType.Plant1,  PlacementType.Plant1,  PlacementType.None,    PlacementType.Stone3,
                PlacementType.None,    PlacementType.Stone1,  PlacementType.None,    PlacementType.None,    PlacementType.Stone1,  PlacementType.None,
                PlacementType.Plant2,  PlacementType.None,    PlacementType.Decoration,PlacementType.Decoration,PlacementType.None, PlacementType.Plant2,
                PlacementType.None,    PlacementType.Stone2,  PlacementType.None,    PlacementType.None,    PlacementType.Stone2,  PlacementType.None,
                PlacementType.Plant1,  PlacementType.None,    PlacementType.Stone1,  PlacementType.Stone1,  PlacementType.None,    PlacementType.Plant1,
                PlacementType.None,    PlacementType.None,    PlacementType.Decoration,PlacementType.Decoration,PlacementType.None, PlacementType.None
            };
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _isActive = true;
            _gridSize = GridSizes[Mathf.Clamp(stageIndex, 0, GridSizes.Length - 1)];

            ClearGrid();
            BuildGridObjects();
            SetupTargetGrid(stageIndex);
            ShowTargetPreview();
        }

        void BuildGridObjects()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            float maxCellSize = 1.2f;
            _cellSize = Mathf.Min(availableHeight / _gridSize, camWidth * 2f / _gridSize, maxCellSize);

            float gridW = _cellSize * _gridSize;
            float gridH = _cellSize * _gridSize;
            _gridOrigin = new Vector3(-gridW / 2f + _cellSize / 2f, camSize - topMargin - _cellSize / 2f, 0f);

            _currentGrid = new PlacementType[_gridSize, _gridSize];
            _targetGrid = new PlacementType[_gridSize, _gridSize];
            _sandGrid = new bool[_gridSize, _gridSize];
            _cellBases = new GameObject[_gridSize, _gridSize];
            _cellObjects = new GameObject[_gridSize, _gridSize];
            _sandObjects = new GameObject[_gridSize, _gridSize];
            _targetMarkers = new GameObject[_gridSize, _gridSize];

            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    Vector3 pos = GetCellWorldPos(row, col);

                    // Cell base
                    var cell = new GameObject($"Cell_{row}_{col}");
                    cell.transform.position = pos;
                    var sr = cell.AddComponent<SpriteRenderer>();
                    sr.sprite = _gridCellSprite;
                    sr.sortingOrder = 0;
                    sr.drawMode = SpriteDrawMode.Sliced;
                    sr.size = new Vector2(_cellSize * 0.95f, _cellSize * 0.95f);
                    cell.AddComponent<BoxCollider2D>().size = new Vector2(_cellSize, _cellSize);
                    _cellBases[row, col] = cell;

                    // Target marker (hidden initially)
                    var marker = new GameObject($"Marker_{row}_{col}");
                    marker.transform.position = pos + new Vector3(0, 0, -0.1f);
                    var msr = marker.AddComponent<SpriteRenderer>();
                    msr.sprite = _gridTargetSprite;
                    msr.sortingOrder = 1;
                    msr.drawMode = SpriteDrawMode.Sliced;
                    msr.size = new Vector2(_cellSize * 0.95f, _cellSize * 0.95f);
                    marker.SetActive(false);
                    _targetMarkers[row, col] = marker;
                }
            }
        }

        void SetupTargetGrid(int stageIndex)
        {
            var pattern = TargetPatterns[Mathf.Clamp(stageIndex, 0, TargetPatterns.Length - 1)];
            for (int row = 0; row < _gridSize; row++)
                for (int col = 0; col < _gridSize; col++)
                {
                    int idx = row * _gridSize + col;
                    _targetGrid[row, col] = idx < pattern.Length ? pattern[idx] : PlacementType.None;
                }
        }

        void ShowTargetPreview()
        {
            // Show target markers for non-empty cells
            for (int row = 0; row < _gridSize; row++)
                for (int col = 0; col < _gridSize; col++)
                    _targetMarkers[row, col].SetActive(_targetGrid[row, col] != PlacementType.None);
        }

        void Update()
        {
            if (!_isActive) return;

            var cam = Camera.main;
            if (cam == null) return;

            Vector3 worldPos = Vector3.zero;
            bool pressed = false, held = false, released = false;

            var mouse = Mouse.current;
            var touch = Touchscreen.current;
            if (mouse != null)
            {
                var mp = mouse.position.ReadValue();
                worldPos = cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, 0));
                pressed = mouse.leftButton.wasPressedThisFrame;
                held = mouse.leftButton.isPressed;
                released = mouse.leftButton.wasReleasedThisFrame;
            }
            else if (touch != null && touch.primaryTouch.press.ReadValue() > 0)
            {
                var tp = touch.primaryTouch.position.ReadValue();
                worldPos = cam.ScreenToWorldPoint(new Vector3(tp.x, tp.y, 0));
                pressed = touch.primaryTouch.press.wasPressedThisFrame;
                held = touch.primaryTouch.press.isPressed;
                released = touch.primaryTouch.press.wasReleasedThisFrame;
            }
            else return;

            worldPos.z = 0;

            if (pressed)
            {
                if (SelectedType == PlacementType.Sand)
                    _isDrawingSand = true;
                else
                    HandlePlacement(worldPos);
            }

            if (held && _isDrawingSand)
                HandleSandDraw(worldPos);

            if (released)
                _isDrawingSand = false;
        }

        void HandlePlacement(Vector3 worldPos)
        {
            if (!WorldToGrid(worldPos, out int row, out int col)) return;

            if (_currentGrid[row, col] == SelectedType)
            {
                // Remove existing
                RemoveCellObject(row, col);
                _currentGrid[row, col] = PlacementType.None;
            }
            else
            {
                // Place new
                RemoveCellObject(row, col);
                _currentGrid[row, col] = SelectedType;
                PlaceCellObject(row, col, SelectedType);
            }

            UpdateMatchRate();
        }

        void HandleSandDraw(Vector3 worldPos)
        {
            if (!WorldToGrid(worldPos, out int row, out int col)) return;
            if (_sandGrid[row, col]) return;
            if (_currentGrid[row, col] != PlacementType.None) return; // don't draw on objects

            _sandGrid[row, col] = true;
            PlaceSandObject(row, col);
            UpdateMatchRate();
        }

        void PlaceCellObject(int row, int col, PlacementType type)
        {
            Sprite sp = GetSprite(type);
            if (sp == null) return;

            Vector3 pos = GetCellWorldPos(row, col);
            var go = new GameObject($"Obj_{row}_{col}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = 5;
            float scale = _cellSize * 0.85f / Mathf.Max(sp.bounds.size.x, sp.bounds.size.y);
            go.transform.localScale = Vector3.one * scale;
            _cellObjects[row, col] = go;

            StartCoroutine(PopAnimation(go.transform));
        }

        void PlaceSandObject(int row, int col)
        {
            if (_sandPatternSprite == null) return;
            Vector3 pos = GetCellWorldPos(row, col) + new Vector3(0, 0, 0.05f);
            var go = new GameObject($"Sand_{row}_{col}");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sandPatternSprite;
            sr.sortingOrder = 3;
            sr.color = new Color(1f, 1f, 1f, 0f);
            float scale = _cellSize * 0.9f / Mathf.Max(_sandPatternSprite.bounds.size.x, _sandPatternSprite.bounds.size.y);
            go.transform.localScale = Vector3.one * scale;
            _sandObjects[row, col] = go;

            StartCoroutine(FadeIn(sr, 0.15f));
        }

        void RemoveCellObject(int row, int col)
        {
            if (_cellObjects[row, col] != null)
            {
                Destroy(_cellObjects[row, col]);
                _cellObjects[row, col] = null;
            }
        }

        Sprite GetSprite(PlacementType type) => type switch
        {
            PlacementType.Stone1      => _stone1Sprite,
            PlacementType.Stone2      => _stone2Sprite,
            PlacementType.Stone3      => _stone3Sprite,
            PlacementType.Plant1      => _plant1Sprite,
            PlacementType.Plant2      => _plant2Sprite,
            PlacementType.Decoration  => _decorationSprite,
            _ => null
        };

        void UpdateMatchRate()
        {
            int targetCount = 0;
            int matchCount = 0;

            for (int row = 0; row < _gridSize; row++)
                for (int col = 0; col < _gridSize; col++)
                    if (_targetGrid[row, col] != PlacementType.None)
                    {
                        targetCount++;
                        if (_currentGrid[row, col] == _targetGrid[row, col])
                            matchCount++;
                    }

            float rate = targetCount > 0 ? (float)matchCount / targetCount : 0f;
            _gameManager.UpdateMatchRateDisplay(rate);
        }

        public void SubmitGarden()
        {
            if (!_isActive) return;

            int targetCount = 0;
            int matchCount = 0;
            int extraCount = 0;
            int sandCount = 0;

            for (int row = 0; row < _gridSize; row++)
                for (int col = 0; col < _gridSize; col++)
                {
                    PlacementType target = _targetGrid[row, col];
                    PlacementType current = _currentGrid[row, col];

                    if (target != PlacementType.None)
                    {
                        targetCount++;
                        if (current == target)
                        {
                            matchCount++;
                            FlashCell(row, col, Color.green);
                        }
                        else if (current != PlacementType.None)
                        {
                            FlashCell(row, col, Color.red);
                        }
                    }
                    else if (current != PlacementType.None)
                    {
                        extraCount++;
                    }

                    if (_sandGrid[row, col]) sandCount++;
                }

            float matchRate = targetCount > 0 ? (float)matchCount / targetCount : 0f;

            // Star rating (determine before score to avoid double-add on retry)
            int stars;
            if (matchRate >= 0.95f) stars = 3;
            else if (matchRate >= 0.85f) stars = 2;
            else if (matchRate >= 0.70f) stars = 1;
            else stars = 0;

            if (stars >= 2)
            {
                // Score calculation - only add on clear
                int baseScore = Mathf.RoundToInt(matchRate * 1000f);
                int sandBonus = sandCount * 5;
                int arrangeBonus = Mathf.Min(extraCount * 2, 100);
                int perfectBonus = matchRate >= 0.9999f ? 500 : 0;
                int rawScore = baseScore + sandBonus + arrangeBonus + perfectBonus;
                int stageScore = Mathf.RoundToInt(rawScore * _comboMultiplier);
                _totalScore += stageScore;
                _gameManager.UpdateScoreDisplay(_totalScore);

                _isActive = false;
                _gameManager.OnStageClear(stars);
            }
            else
            {
                // Allow retry - no score added
                _ui.ShowRetryMessage(matchRate);
            }
        }

        public void ResetCurrentStage()
        {
            for (int row = 0; row < _gridSize; row++)
                for (int col = 0; col < _gridSize; col++)
                {
                    RemoveCellObject(row, col);
                    _currentGrid[row, col] = PlacementType.None;

                    if (_sandObjects[row, col] != null)
                    {
                        Destroy(_sandObjects[row, col]);
                        _sandObjects[row, col] = null;
                    }
                    _sandGrid[row, col] = false;
                }

            UpdateMatchRate();
        }

        void FlashCell(int row, int col, Color flashColor)
        {
            var obj = _cellObjects[row, col];
            if (obj == null) obj = _cellBases[row, col];
            if (obj == null) return;
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null) return;
            StartCoroutine(ColorFlash(sr, flashColor, 0.3f));
        }

        IEnumerator PopAnimation(Transform t)
        {
            float dur = 0.2f;
            float elapsed = 0f;
            Vector3 original = t.localScale;
            while (elapsed < dur)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float ratio = elapsed / dur;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.3f, ratio * 2f)
                    : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                t.localScale = original * scale;
                yield return null;
            }
            if (t != null) t.localScale = original;
        }

        IEnumerator FadeIn(SpriteRenderer sr, float dur)
        {
            float elapsed = 0f;
            while (elapsed < dur)
            {
                if (sr == null) yield break;
                elapsed += Time.deltaTime;
                sr.color = new Color(1f, 1f, 1f, Mathf.Clamp01(elapsed / dur));
                yield return null;
            }
            if (sr != null) sr.color = Color.white;
        }

        IEnumerator ColorFlash(SpriteRenderer sr, Color flashColor, float dur)
        {
            if (sr == null) yield break;
            Color original = sr.color;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                if (sr == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                sr.color = Color.Lerp(flashColor, original, t);
                yield return null;
            }
            if (sr != null) sr.color = original;
        }

        bool WorldToGrid(Vector3 worldPos, out int row, out int col)
        {
            float dx = worldPos.x - (_gridOrigin.x - _cellSize / 2f);
            float dy = _gridOrigin.y + _cellSize / 2f - worldPos.y;
            col = Mathf.FloorToInt(dx / _cellSize);
            row = Mathf.FloorToInt(dy / _cellSize);
            return row >= 0 && row < _gridSize && col >= 0 && col < _gridSize;
        }

        Vector3 GetCellWorldPos(int row, int col)
        {
            return new Vector3(
                _gridOrigin.x + col * _cellSize,
                _gridOrigin.y - row * _cellSize,
                0f
            );
        }

        void ClearGrid()
        {
            if (_cellBases != null)
                for (int r = 0; r < _cellBases.GetLength(0); r++)
                    for (int c = 0; c < _cellBases.GetLength(1); c++)
                    {
                        if (_cellBases[r, c] != null) Destroy(_cellBases[r, c]);
                        if (_cellObjects != null && _cellObjects[r, c] != null) Destroy(_cellObjects[r, c]);
                        if (_sandObjects != null && _sandObjects[r, c] != null) Destroy(_sandObjects[r, c]);
                        if (_targetMarkers != null && _targetMarkers[r, c] != null) Destroy(_targetMarkers[r, c]);
                    }
        }

        public void ResetScore() => _totalScore = 0;
        public void SetComboMultiplier(float m) => _comboMultiplier = m;
        public void SetActive(bool v) => _isActive = v;

        void OnDestroy()
        {
            ClearGrid();
        }
    }
}
