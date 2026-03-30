using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game013_SymmetryDraw
{
    /// <summary>
    /// 描画キャンバスの管理。入力処理を一元管理し、
    /// 左半分への描画を右半分に対称コピーする。
    /// お手本パターンとの一致判定も担当。
    /// </summary>
    public class CanvasDrawManager : MonoBehaviour
    {
        [SerializeField, Tooltip("セルのプレハブ")]
        private GameObject _cellPrefab;

        [SerializeField, Tooltip("お手本セルのプレハブ")]
        private GameObject _targetCellPrefab;

        [SerializeField, Tooltip("セルサイズ（ワールド座標）")]
        private float _cellSize = 0.5f;

        [SerializeField, Tooltip("対称線のSpriteRenderer")]
        private SpriteRenderer _symmetryLine;

        private CellView[,] _cells;
        private bool[,] _painted;
        private HashSet<Vector2Int> _targetCells;
        private SymmetryDrawGameManager _gameManager;

        private bool _isDrawing;
        private bool _hasNewStroke;

        private void Start()
        {
            _gameManager = GetComponentInParent<SymmetryDrawGameManager>();
        }

        private void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// お手本パターンを読み込んでキャンバスを初期化する。
        /// </summary>
        public void Initialize(List<Vector2Int> leftPattern)
        {
            ClearCanvas();

            int w = StageData.GridWidth;
            int h = StageData.GridHeight;
            _cells = new CellView[w, h];
            _painted = new bool[w, h];

            // お手本パターン（左右対称の完成形）
            var fullPattern = StageData.MirrorPattern(leftPattern);
            _targetCells = new HashSet<Vector2Int>(fullPattern);

            float offsetX = (w - 1) * _cellSize * 0.5f;
            float offsetY = (h - 1) * _cellSize * 0.5f;

            // お手本を薄く表示
            foreach (var pos in fullPattern)
            {
                if (_targetCellPrefab != null)
                {
                    var targetObj = Instantiate(_targetCellPrefab, transform);
                    targetObj.name = $"Target_{pos.x}_{pos.y}";
                    targetObj.transform.position = new Vector3(
                        pos.x * _cellSize - offsetX,
                        pos.y * _cellSize - offsetY,
                        0.1f
                    );
                    targetObj.transform.localScale = new Vector3(_cellSize * 0.9f, _cellSize * 0.9f, 1f);
                }
            }

            // 描画用セルを全マスに配置
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (_cellPrefab == null) continue;

                    var obj = Instantiate(_cellPrefab, transform);
                    obj.name = $"Cell_{x}_{y}";
                    obj.transform.position = new Vector3(
                        x * _cellSize - offsetX,
                        y * _cellSize - offsetY,
                        0f
                    );
                    obj.transform.localScale = new Vector3(_cellSize * 0.95f, _cellSize * 0.95f, 1f);

                    var cellView = obj.GetComponent<CellView>();
                    if (cellView != null)
                    {
                        cellView.SetGridPosition(new Vector2Int(x, y));
                        cellView.SetPainted(false);
                    }
                    _cells[x, y] = cellView;
                }
            }

            // 対称線の位置設定
            if (_symmetryLine != null)
            {
                _symmetryLine.transform.position = new Vector3(0f, 0f, -0.1f);
            }
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDrawing = true;
                _hasNewStroke = false;
                TryPaintAt(mouse.position.ReadValue());
            }
            else if (mouse.leftButton.isPressed && _isDrawing)
            {
                TryPaintAt(mouse.position.ReadValue());
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDrawing)
            {
                _isDrawing = false;
                if (_hasNewStroke && _gameManager != null)
                {
                    _gameManager.OnStrokeCompleted();
                }
            }
        }

        private void TryPaintAt(Vector2 screenPos)
        {
            if (_cells == null) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var cellView = hit.GetComponent<CellView>();
            if (cellView == null) return;

            var gridPos = cellView.GridPosition;
            int halfWidth = StageData.GridWidth / 2;

            // 左半分のみ描画可能
            if (gridPos.x >= halfWidth) return;

            // すでに塗り済みなら無視
            if (_painted[gridPos.x, gridPos.y]) return;

            // 左半分を塗る
            PaintCell(gridPos.x, gridPos.y);

            // 右半分にミラー塗り
            int mirrorX = StageData.GridWidth - 1 - gridPos.x;
            PaintCell(mirrorX, gridPos.y);

            _hasNewStroke = true;
        }

        private void PaintCell(int x, int y)
        {
            if (x < 0 || x >= StageData.GridWidth || y < 0 || y >= StageData.GridHeight) return;
            if (_painted[x, y]) return;

            _painted[x, y] = true;
            if (_cells[x, y] != null)
            {
                _cells[x, y].SetPainted(true);
            }
        }

        /// <summary>
        /// お手本と描画が一致しているかチェックする。
        /// お手本セルが全て塗られ、かつお手本外のセルが塗られていなければ一致。
        /// </summary>
        public bool CheckMatch()
        {
            if (_painted == null || _targetCells == null) return false;

            int w = StageData.GridWidth;
            int h = StageData.GridHeight;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var pos = new Vector2Int(x, y);
                    bool isTarget = _targetCells.Contains(pos);
                    bool isPainted = _painted[x, y];

                    if (isTarget && !isPainted) return false;
                    if (!isTarget && isPainted) return false;
                }
            }

            return true;
        }

        private void ClearCanvas()
        {
            // 既存の子オブジェクトを削除
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child != _symmetryLine?.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            _cells = null;
            _painted = null;
            _targetCells = null;
        }
    }
}
