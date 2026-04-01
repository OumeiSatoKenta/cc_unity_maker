using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game045_FingerPaint
{
    public class PaintManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private FingerPaintGameManager _gameManager;

        [SerializeField, Tooltip("ブラシスプライト")]
        private Sprite _brushSprite;

        [SerializeField, Tooltip("ターゲットスプライト")]
        private Sprite _targetSprite;

        private Camera _mainCamera;
        private List<Vector2> _paintedPoints = new List<Vector2>();
        private List<GameObject> _paintDots = new List<GameObject>();
        private bool _isPainting;
        private Color _currentColor = Color.red;

        // ターゲット領域（星形の中心と半径で近似）
        private static readonly Vector2 TargetCenter = Vector2.zero;
        private const float TargetRadius = 2.5f;
        private const float InkPerSecond = 15f;
        private const float DotInterval = 0.15f;

        private float _dotTimer;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            // ターゲットを半透明で表示
            var targetObj = new GameObject("Target");
            targetObj.transform.position = new Vector3(TargetCenter.x, TargetCenter.y + 0.5f, 0f);
            targetObj.transform.localScale = new Vector3(5f, 5f, 1f);
            var sr = targetObj.AddComponent<SpriteRenderer>();
            sr.sprite = _targetSprite;
            sr.sortingOrder = 0;
            sr.color = new Color(1f, 1f, 1f, 0.4f);
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
                _isPainting = true;
            if (Mouse.current.leftButton.wasReleasedThisFrame)
                _isPainting = false;

            if (_isPainting && Mouse.current.leftButton.isPressed)
            {
                _gameManager.ConsumeInk(InkPerSecond * Time.deltaTime);
                _dotTimer += Time.deltaTime;
                if (_dotTimer >= DotInterval)
                {
                    _dotTimer = 0f;
                    PlaceDot();
                }
            }
        }

        private void PlaceDot()
        {
            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            Vector3 wp = _mainCamera.ScreenToWorldPoint(mp);

            var dotObj = new GameObject("Dot");
            dotObj.transform.position = new Vector3(wp.x, wp.y, 0f);
            dotObj.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            var sr = dotObj.AddComponent<SpriteRenderer>();
            sr.sprite = _brushSprite;
            sr.sortingOrder = 2;
            sr.color = _currentColor;
            _paintDots.Add(dotObj);
            _paintedPoints.Add(new Vector2(wp.x, wp.y));
        }

        public float CalculateMatch()
        {
            if (_paintedPoints.Count == 0) return 0f;

            // ターゲット領域内に描画されたドットの割合で一致率を算出
            int inTarget = 0;
            Vector2 center = new Vector2(TargetCenter.x, TargetCenter.y + 0.5f);
            foreach (var p in _paintedPoints)
            {
                if (Vector2.Distance(p, center) <= TargetRadius)
                    inTarget++;
            }
            float precision = (float)inTarget / _paintedPoints.Count * 100f;

            // 密度ボーナス: ドット数が多いほどカバレッジ高い
            float coverage = Mathf.Min(1f, _paintedPoints.Count / 50f);
            return Mathf.Min(100f, precision * coverage);
        }

        public void SetColor(Color color) { _currentColor = color; }
    }
}
