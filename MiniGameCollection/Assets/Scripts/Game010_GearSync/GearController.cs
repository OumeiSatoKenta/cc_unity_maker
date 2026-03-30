using UnityEngine;

namespace Game010_GearSync
{
    /// <summary>
    /// 歯車1個のデータ保持・表示を担当する。
    /// 回転状態(向き)と接続情報を保持する。
    /// 入力処理は GearManager に一元管理する。
    /// </summary>
    public class GearController : MonoBehaviour
    {
        // 歯車の向き（出力ピンの方向）: 0=右,1=上,2=左,3=下
        private int _currentDirection = 0;
        private int _targetDirection = 0;
        private int _gearId;
        private bool _isDriver = false;  // 駆動歯車（電源側）
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _arrowRenderer;

        // 隣接する歯車との噛み合い関係
        public int GearId => _gearId;
        public int CurrentDirection => _currentDirection;
        public int TargetDirection => _targetDirection;
        public bool IsDriver => _isDriver;
        public bool IsSynced => _currentDirection == _targetDirection;

        private static readonly float[] Angles = { 0f, 90f, 180f, 270f };

        public void Initialize(int id, int targetDir, bool isDriver, Sprite gearSprite, Sprite arrowSprite)
        {
            _gearId = id;
            _targetDirection = targetDir;
            _isDriver = isDriver;
            _currentDirection = isDriver ? targetDir : Random.Range(0, 4);

            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = gearSprite;
            _spriteRenderer.sortingOrder = 0;

            if (isDriver)
            {
                _spriteRenderer.color = new Color(0.3f, 0.8f, 0.4f); // 緑: 駆動歯車
            }
            else
            {
                _spriteRenderer.color = new Color(0.7f, 0.7f, 0.8f); // 薄青灰: 通常歯車
            }

            // 矢印（ターゲット方向表示）
            if (!isDriver)
            {
                var arrowGo = new GameObject("Arrow");
                arrowGo.transform.SetParent(transform, false);
                arrowGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                _arrowRenderer = arrowGo.AddComponent<SpriteRenderer>();
                _arrowRenderer.sprite = arrowSprite;
                _arrowRenderer.sortingOrder = 1;
                _arrowRenderer.color = new Color(1f, 0.85f, 0.2f, 0.8f); // 黄色
                arrowGo.transform.localRotation = Quaternion.Euler(0f, 0f, Angles[_targetDirection]);
            }

            ApplyVisual();
        }

        /// <summary>クリックで時計回りに90度回転（駆動歯車は回転不可）</summary>
        public void RotateClockwise()
        {
            if (_isDriver) return;
            _currentDirection = (_currentDirection + 3) % 4; // -1 mod 4 = 3
            ApplyVisual();
            UpdateColor();
        }

        private void ApplyVisual()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, Angles[_currentDirection]);
        }

        private void UpdateColor()
        {
            if (_isDriver) return;
            _spriteRenderer.color = IsSynced
                ? new Color(0.3f, 0.8f, 0.4f)   // 揃った: 緑
                : new Color(0.7f, 0.7f, 0.8f);   // 未揃い: 薄青灰
        }

        /// <summary>コライダーをセットアップする（クリック判定用）</summary>
        public void SetupCollider()
        {
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
        }
    }
}
