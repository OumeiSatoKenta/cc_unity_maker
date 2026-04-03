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

        private static readonly string[] SpriteNames =
        {
            "Sprites/Game001_BlockFlow/block_red",
            "Sprites/Game001_BlockFlow/block_blue",
            "Sprites/Game001_BlockFlow/block_green",
            "Sprites/Game001_BlockFlow/block_yellow",
            "Sprites/Game001_BlockFlow/block_purple",
        };

        public int ColorId => _colorId;
        public Vector2Int GridPosition { get; private set; }

        public void Initialize(int colorId, Vector2Int gridPos)
        {
            _colorId = colorId;
            GridPosition = gridPos;

            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) return;

            if (colorId >= 0 && colorId < SpriteNames.Length)
            {
                var sprite = Resources.Load<Sprite>(SpriteNames[colorId]);
                if (sprite != null)
                {
                    _spriteRenderer.sprite = sprite;
                    _spriteRenderer.color = Color.white;
                }
                else
                {
                    Debug.LogWarning($"[BlockController] スプライト '{SpriteNames[colorId]}' が見つかりません。フォールバック色を使用します");
                    _spriteRenderer.color = GetFallbackColor(colorId);
                }
            }
        }

        private static Color GetFallbackColor(int colorId)
        {
            return colorId switch
            {
                0 => new Color(0.95f, 0.3f, 0.3f),
                1 => new Color(0.3f, 0.55f, 0.95f),
                2 => new Color(0.3f, 0.85f, 0.4f),
                3 => new Color(0.95f, 0.85f, 0.2f),
                4 => new Color(0.85f, 0.4f, 0.85f),
                _ => Color.white,
            };
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
