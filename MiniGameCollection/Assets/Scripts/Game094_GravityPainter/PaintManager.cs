using UnityEngine;

namespace Game094_GravityPainter
{
    public class PaintManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private GravityPainterGameManager _gameManager;
        [SerializeField, Tooltip("セルスプライト")] private Sprite _cellSprite;

        private const int GridSize = 8;
        private const float CellSize = 0.52f;
        private const int ColorWhite  = 0;
        private const int ColorRed    = 1;
        private const int ColorBlue   = 2;
        private const int ColorGreen  = 3;
        private const int ColorYellow = 4;
        private const int PatternCount = 5;

        private static readonly Color[] PaintColors = {
            new Color(0.95f, 0.95f, 0.95f), // 0: White
            new Color(0.9f,  0.2f,  0.2f),  // 1: Red
            new Color(0.2f,  0.4f,  0.9f),  // 2: Blue
            new Color(0.2f,  0.8f,  0.3f),  // 3: Green
            new Color(0.95f, 0.85f, 0.1f),  // 4: Yellow
        };

        private int _selectedColorIdx;
        private int[] _canvasColors;
        private int[] _targetColors;
        private SpriteRenderer[] _cellRenderers;
        private bool _isActive;

        public void StartGame()
        {
            if (_cellSprite == null) { Debug.LogError("[PaintManager] _cellSprite が未アサインです"); return; }
            _isActive = true;
            _selectedColorIdx = ColorRed;
            _canvasColors = new int[GridSize * GridSize];
            _targetColors = new int[GridSize * GridSize];

            int patternIdx = Random.Range(0, PatternCount);
            SetupTarget(patternIdx);
            CreateGrid();
            CreatePreview();
        }

        public void StopGame() { _isActive = false; }

        public void SelectColor(int idx)
        {
            if (idx >= ColorRed && idx <= ColorYellow)
                _selectedColorIdx = idx;
        }

        public void DropPaint(int direction)
        {
            if (!_isActive) return;
            if (direction == 0) // up: 各列、上端から最初の白セルを塗る
            {
                for (int c = 0; c < GridSize; c++)
                    for (int r = 0; r < GridSize; r++)
                        if (_canvasColors[r * GridSize + c] == ColorWhite)
                        { _canvasColors[r * GridSize + c] = _selectedColorIdx; break; }
            }
            else if (direction == 1) // down: 各列、下端から最初の白セルを塗る
            {
                for (int c = 0; c < GridSize; c++)
                    for (int r = GridSize - 1; r >= 0; r--)
                        if (_canvasColors[r * GridSize + c] == ColorWhite)
                        { _canvasColors[r * GridSize + c] = _selectedColorIdx; break; }
            }
            else if (direction == 2) // left: 各行、左端から最初の白セルを塗る
            {
                for (int r = 0; r < GridSize; r++)
                    for (int c = 0; c < GridSize; c++)
                        if (_canvasColors[r * GridSize + c] == ColorWhite)
                        { _canvasColors[r * GridSize + c] = _selectedColorIdx; break; }
            }
            else if (direction == 3) // right: 各行、右端から最初の白セルを塗る
            {
                for (int r = 0; r < GridSize; r++)
                    for (int c = GridSize - 1; c >= 0; c--)
                        if (_canvasColors[r * GridSize + c] == ColorWhite)
                        { _canvasColors[r * GridSize + c] = _selectedColorIdx; break; }
            }
            else
            {
                Debug.LogWarning($"[PaintManager] 不正な direction: {direction}");
                return;
            }

            RefreshGrid();
            // 塗れた・塗れなかった(全塗り済み)に関わらず操作回数を消費する
            _gameManager.OnPaintDropped();
        }

        public float CalculateMatchRate()
        {
            int match = 0;
            for (int i = 0; i < _canvasColors.Length; i++)
                if (_canvasColors[i] == _targetColors[i]) match++;
            return (float)match / _canvasColors.Length;
        }

        private void SetupTarget(int patternIdx)
        {
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                {
                    int i = r * GridSize + c;
                    switch (patternIdx)
                    {
                        case 0: _targetColors[i] = r < 4 ? ColorRed : ColorBlue; break;
                        case 1: _targetColors[i] = c < 4 ? ColorGreen : ColorYellow; break;
                        case 2: _targetColors[i] = (r == 0 || r == 7 || c == 0 || c == 7) ? ColorRed : ColorBlue; break;
                        case 3: _targetColors[i] = (r == 3 || r == 4 || c == 3 || c == 4) ? ColorGreen : ColorYellow; break;
                        case 4: _targetColors[i] = (r == c || r + c == 7) ? ColorRed : ColorBlue; break;
                        default: _targetColors[i] = ColorRed; break;
                    }
                }
        }

        private void CreateGrid()
        {
            _cellRenderers = new SpriteRenderer[GridSize * GridSize];
            float startX = -(GridSize * CellSize) / 2f + CellSize / 2f;
            float startY = (GridSize * CellSize) / 2f - CellSize / 2f;

            for (int i = 0; i < GridSize * GridSize; i++)
            {
                int r = i / GridSize, c = i % GridSize;
                var obj = new GameObject($"Cell_{r}_{c}");
                obj.transform.SetParent(transform);
                obj.transform.position = new Vector3(startX + c * CellSize, startY - r * CellSize, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _cellSprite;
                sr.color = PaintColors[_canvasColors[i]];
                sr.sortingOrder = 2;
                obj.transform.localScale = Vector3.one * (CellSize / 0.64f);
                _cellRenderers[i] = sr;
            }
        }

        private void RefreshGrid()
        {
            if (_cellRenderers == null) return;
            for (int i = 0; i < _cellRenderers.Length; i++)
                if (_cellRenderers[i] != null) _cellRenderers[i].color = PaintColors[_canvasColors[i]];
        }

        private void CreatePreview()
        {
            const float ps = 0.22f;
            float px = 3.6f, py = 4.8f;

            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                {
                    var obj = new GameObject($"Preview_{r}_{c}");
                    obj.transform.SetParent(transform);
                    obj.transform.position = new Vector3(px + c * ps, py - r * ps, 0f);
                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = _cellSprite;
                    sr.color = PaintColors[_targetColors[r * GridSize + c]];
                    sr.sortingOrder = 3;
                    obj.transform.localScale = Vector3.one * (ps / 0.64f);
                }
        }
    }
}
