using UnityEngine;

namespace Game013_SymmetryDraw
{
    /// <summary>
    /// セル1つの表示制御。データ保持と描画状態の表示のみ担当。
    /// 入力処理は CanvasDrawManager が一元管理する。
    /// </summary>
    public class CellView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private bool _isPainted;

        public Vector2Int GridPosition { get; private set; }

        private static readonly Color UnpaintedColor = new Color(0.2f, 0.22f, 0.28f, 0.3f);
        private static readonly Color PaintedColor = new Color(0.3f, 0.7f, 1f, 1f);

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetGridPosition(Vector2Int pos)
        {
            GridPosition = pos;
        }

        public void SetPainted(bool painted)
        {
            _isPainted = painted;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = painted ? PaintedColor : UnpaintedColor;
            }
        }

        public bool IsPainted => _isPainted;
    }
}
