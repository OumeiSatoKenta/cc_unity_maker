using UnityEngine;

namespace Game001_BlockFlow
{
    /// <summary>
    /// ブロック1つの制御。色情報の保持と表示を担当する。
    /// 入力処理は BoardManager が一元管理する。
    /// </summary>
    public class BlockController : MonoBehaviour
    {
        [SerializeField, Tooltip("ブロックの色ID（0始まり）")]
        private int _colorId;

        private SpriteRenderer _spriteRenderer;

        private static readonly Color[] BlockColors =
        {
            new Color(0.95f, 0.3f, 0.3f),  // 赤
            new Color(0.3f, 0.55f, 0.95f),  // 青
            new Color(0.3f, 0.85f, 0.4f),   // 緑
            new Color(0.95f, 0.85f, 0.2f),  // 黄
            new Color(0.85f, 0.4f, 0.85f),  // 紫
        };

        public int ColorId => _colorId;
        public Vector2Int GridPosition { get; private set; }

        public void Initialize(int colorId, Vector2Int gridPos)
        {
            _colorId = colorId;
            GridPosition = gridPos;

            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null && colorId >= 0 && colorId < BlockColors.Length)
            {
                _spriteRenderer.color = BlockColors[colorId];
            }
        }

        public void SetGridPosition(Vector2Int newPos)
        {
            GridPosition = newPos;
        }

        public void UpdateWorldPosition(Vector3 worldPos)
        {
            transform.position = worldPos;
        }
    }
}
