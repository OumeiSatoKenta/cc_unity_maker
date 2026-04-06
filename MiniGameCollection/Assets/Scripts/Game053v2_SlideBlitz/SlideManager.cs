using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace Game053v2_SlideBlitz
{
    /// <summary>
    /// スライドパズルのコアメカニクスを管理する。
    /// グリッド生成・タイル移動・入力処理を一元管理する。
    /// </summary>
    public class SlideManager : MonoBehaviour
    {
        [SerializeField] private Sprite _spriteNormal;
        [SerializeField] private Sprite _spriteFrozen;
        [SerializeField] private Sprite _spriteBlank;
        [SerializeField] private TMP_FontAsset _font;

        public event System.Action OnPuzzleSolved;
        public event System.Action<int> OnComboChanged;
        public event System.Action<int> OnMoveCountChanged;

        private int _gridSize;
        private int[] _grid;       // 0=blank, 1..N=tile numbers, negative=frozen
        private List<TileObject> _tileObjects = new List<TileObject>();
        private int _blankIndex;
        private bool _isActive;
        private bool _isSolved;

        private int _moveCount;
        private int _combo;
        private int _frozenCount;

        // Input
        private Vector2 _pointerDownPos;
        private bool _pointerDown;
        private const float SwipeThreshold = 0.3f;

        // World layout
        private float _cellSize;
        private Vector3 _gridOrigin;

        // Optimal move count (approximate: gridSize * gridSize * 2 as baseline)
        public int MoveCount => _moveCount;
        public int GridSize => _gridSize;

        private void Update()
        {
            if (!_isActive || _isSolved) return;
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _pointerDownPos = mouse.position.ReadValue();
                _pointerDown = true;
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _pointerDown)
            {
                _pointerDown = false;
                Vector2 upPos = mouse.position.ReadValue();
                Vector2 delta = upPos - _pointerDownPos;

                if (delta.magnitude < SwipeThreshold * Screen.height * 0.05f)
                {
                    // タップ判定: クリックしたタイルに隣接方向で移動
                    TryMoveByTap(_pointerDownPos);
                }
                else
                {
                    // スワイプ方向で移動
                    TryMoveBySwipe(_pointerDownPos, delta);
                }
            }
        }

        private void TryMoveByTap(Vector2 screenPos)
        {
            int tileIndex = GetTileIndexAtScreen(screenPos);
            if (tileIndex < 0) return;

            if (_grid[tileIndex] < 0) // frozen
            {
                PlayFrozenAt(tileIndex);
                return;
            }

            // タップしたタイルが空白に隣接していれば移動
            TryMoveToBlank(tileIndex);
        }

        private void TryMoveBySwipe(Vector2 screenPos, Vector2 delta)
        {
            int tileIndex = GetTileIndexAtScreen(screenPos);
            if (tileIndex < 0) return;

            if (_grid[tileIndex] < 0)
            {
                PlayFrozenAt(tileIndex);
                return;
            }

            // スワイプ方向に沿って移動
            int dirCol = 0, dirRow = 0;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                dirCol = delta.x > 0 ? 1 : -1;
            else
                dirRow = delta.y > 0 ? 1 : -1;

            int row = tileIndex / _gridSize;
            int col = tileIndex % _gridSize;
            int targetRow = row + dirRow;
            int targetCol = col + dirCol;

            if (targetRow < 0 || targetRow >= _gridSize || targetCol < 0 || targetCol >= _gridSize) return;
            int targetIndex = targetRow * _gridSize + targetCol;

            if (targetIndex == _blankIndex)
            {
                ExecuteMove(tileIndex, targetIndex);
            }
        }

        private void TryMoveToBlank(int tileIndex)
        {
            int row = tileIndex / _gridSize;
            int col = tileIndex % _gridSize;
            int blankRow = _blankIndex / _gridSize;
            int blankCol = _blankIndex % _gridSize;

            bool adjacent = (Mathf.Abs(row - blankRow) + Mathf.Abs(col - blankCol)) == 1;
            if (adjacent)
            {
                ExecuteMove(tileIndex, _blankIndex);
            }
        }

        private void ExecuteMove(int fromIndex, int toIndex)
        {
            // frozen タイルは移動不可
            if (_grid[fromIndex] < 0) return;

            // スワップ
            int temp = _grid[fromIndex];
            _grid[fromIndex] = _grid[toIndex];
            _grid[toIndex] = temp;
            _blankIndex = fromIndex;

            _moveCount++;
            OnMoveCountChanged?.Invoke(_moveCount);

            // ビジュアル更新
            UpdateTilePositions();

            // コンボ判定: 移動したタイルが正しい位置に収まったか（固定タイルは除外）
            int movedNumber = _grid[toIndex];
            if (movedNumber > 0 && movedNumber == toIndex + 1)
            {
                _combo++;
                _tileObjects[toIndex]?.PlayCorrectAnimation();
                OnComboChanged?.Invoke(_combo);
            }
            else
            {
                _combo = 0;
                OnComboChanged?.Invoke(0);
            }

            // 完成チェック
            if (CheckSolved())
            {
                _isSolved = true;
                PlayCompleteEffect();
                OnPuzzleSolved?.Invoke();
            }
        }

        private bool CheckSolved()
        {
            for (int i = 0; i < _grid.Length - 1; i++)
            {
                if (Mathf.Abs(_grid[i]) != i + 1) return false;
            }
            return _grid[_grid.Length - 1] == 0;
        }

        private void PlayCompleteEffect()
        {
            int n = _gridSize * _gridSize;
            for (int i = 0; i < n; i++)
            {
                if (_tileObjects[i] != null && !_tileObjects[i].IsBlank)
                {
                    float delay = i * 0.05f;
                    _tileObjects[i].PlayCompleteAnimation(delay);
                }
            }
        }

        private void PlayFrozenAt(int index)
        {
            if (_tileObjects[index] != null)
                _tileObjects[index].PlayFrozenAnimation();
        }

        public void SetupStage(StageManager.StageConfig config)
        {
            _isActive = false;
            _isSolved = false;
            _moveCount = 0;
            _combo = 0;

            // gridSizeはcountMultiplierに格納
            _gridSize = config.countMultiplier > 0 ? config.countMultiplier : 3;
            _frozenCount = Mathf.RoundToInt(config.complexityFactor * _gridSize * _gridSize);

            int shuffleCount = Mathf.RoundToInt(20f * config.speedMultiplier);

            ClearTiles();
            CalculateLayout();
            CreateGrid(shuffleCount);
            _isActive = true;
        }

        private void CalculateLayout()
        {
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[SlideManager] Camera.main not found"); return; }
            float camSize = cam.orthographicSize;
            float camWidth = camSize * cam.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
            float gridWorldSize = Mathf.Min(availableHeight, camWidth * 1.8f);
            _cellSize = gridWorldSize / _gridSize;

            float gridHalfSize = gridWorldSize * 0.5f;
            float centerY = (camSize - topMargin) - availableHeight * 0.5f - topMargin * 0.5f;
            // 簡略: 中央配置
            _gridOrigin = new Vector3(-gridHalfSize + _cellSize * 0.5f, gridHalfSize - _cellSize * 0.5f + (bottomMargin - topMargin) * 0.25f, 0f);
        }

        private void CreateGrid(int shuffleCount)
        {
            int total = _gridSize * _gridSize;
            _grid = new int[total];
            // 初期配置: 1..N-1, 0(blank)
            for (int i = 0; i < total - 1; i++) _grid[i] = i + 1;
            _grid[total - 1] = 0;
            _blankIndex = total - 1;

            // シャッフル（合法手のみ）
            ShuffleGrid(shuffleCount);

            // 固定タイル設定（Stage4: 非blankの位置から選ぶ）
            var frozenIndices = new HashSet<int>();
            if (_frozenCount > 0)
            {
                var candidates = new List<int>();
                for (int i = 0; i < total; i++)
                    if (_grid[i] != 0) candidates.Add(i);
                // Fisher-Yates シャッフル（推移律を満たさないランダムソートを避ける）
                for (int i = candidates.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    int tmp2 = candidates[i]; candidates[i] = candidates[j]; candidates[j] = tmp2;
                }
                for (int i = 0; i < Mathf.Min(_frozenCount, candidates.Count); i++)
                    frozenIndices.Add(candidates[i]);
            }

            // タイルオブジェクト生成
            for (int i = 0; i < total; i++)
            {
                bool isBlank = _grid[i] == 0;
                bool isFrozen = frozenIndices.Contains(i) && !isBlank;
                if (isFrozen) _grid[i] = -_grid[i]; // 負値で固定フラグ

                var go = new GameObject($"Tile_{i}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = GridIndexToLocal(i);
                go.transform.localScale = Vector3.one * _cellSize * 0.92f;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 5;

                var tile = go.AddComponent<TileObject>();
                tile.Setup(isBlank ? 0 : Mathf.Abs(_grid[i]), isBlank, isFrozen, _spriteNormal, _spriteFrozen, _spriteBlank);

                // 番号テキスト
                if (!isBlank)
                {
                    var textGo = new GameObject("Text");
                    textGo.transform.SetParent(go.transform, false);
                    textGo.transform.localPosition = Vector3.zero;
                    var tmp = textGo.AddComponent<TextMeshPro>();
                    tmp.text = Mathf.Abs(_grid[i]).ToString();
                    tmp.fontSize = _cellSize * 18f;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = isFrozen ? new Color(0.8f, 0.8f, 0.9f) : Color.white;
                    tmp.fontStyle = FontStyles.Bold;
                    if (_font != null) tmp.font = _font;
                    var rt = textGo.GetComponent<RectTransform>();
                    if (rt == null) rt = textGo.AddComponent<RectTransform>();
                    rt.sizeDelta = Vector2.one * _cellSize;
                    tmp.sortingOrder = 6;
                }

                _tileObjects.Add(tile);
            }
        }

        private void ShuffleGrid(int count)
        {
            int[] dirs = { -_gridSize, _gridSize, -1, 1 };
            for (int s = 0; s < count; s++)
            {
                var valid = new List<int>();
                int blankRow = _blankIndex / _gridSize;
                int blankCol = _blankIndex % _gridSize;
                foreach (int d in dirs)
                {
                    int ni = _blankIndex + d;
                    if (ni < 0 || ni >= _grid.Length) continue;
                    int nr = ni / _gridSize;
                    int nc = ni % _gridSize;
                    if (Mathf.Abs(nr - blankRow) + Mathf.Abs(nc - blankCol) != 1) continue;
                    valid.Add(ni);
                }
                if (valid.Count == 0) continue;
                int chosen = valid[Random.Range(0, valid.Count)];
                int tmp = _grid[_blankIndex];
                _grid[_blankIndex] = _grid[chosen];
                _grid[chosen] = tmp;
                _blankIndex = chosen;
            }
        }

        private void UpdateTilePositions()
        {
            for (int i = 0; i < _tileObjects.Count; i++)
            {
                if (_tileObjects[i] != null)
                    _tileObjects[i].transform.localPosition = GridIndexToLocal(i);
            }
        }

        private Vector3 GridIndexToLocal(int index)
        {
            int row = index / _gridSize;
            int col = index % _gridSize;
            return _gridOrigin + new Vector3(col * _cellSize, -row * _cellSize, 0f);
        }

        private int GetTileIndexAtScreen(Vector2 screenPos)
        {
            var cam = Camera.main;
            if (cam == null) return -1;
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
            worldPos.z = 0f;

            float half = _cellSize * 0.5f;
            for (int i = 0; i < _tileObjects.Count; i++)
            {
                if (_tileObjects[i] == null) continue;
                Vector3 tp = _tileObjects[i].transform.position;
                if (Mathf.Abs(worldPos.x - tp.x) <= half && Mathf.Abs(worldPos.y - tp.y) <= half)
                    return i;
            }
            return -1;
        }

        private void ClearTiles()
        {
            foreach (var t in _tileObjects)
                if (t != null) Destroy(t.gameObject);
            _tileObjects.Clear();
        }

        public void SetActive(bool active) => _isActive = active;

        public float GetComboMultiplier()
        {
            if (_combo >= 5) return 1.5f;
            if (_combo >= 3) return 1.2f;
            if (_combo >= 2) return 1.1f;
            return 1.0f;
        }

        private void OnDestroy()
        {
            ClearTiles();
        }
    }
}
