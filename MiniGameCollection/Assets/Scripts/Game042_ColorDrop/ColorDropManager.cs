using UnityEngine;
using UnityEngine.InputSystem;

namespace Game042_ColorDrop
{
    public class ColorDropManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private ColorDropGameManager _gameManager;

        [SerializeField, Tooltip("ドロップスプライト赤")] private Sprite _dropRed;
        [SerializeField, Tooltip("ドロップスプライト青")] private Sprite _dropBlue;
        [SerializeField, Tooltip("ドロップスプライト緑")] private Sprite _dropGreen;
        [SerializeField, Tooltip("ドロップスプライト黄")] private Sprite _dropYellow;
        [SerializeField, Tooltip("バケツスプライト赤")] private Sprite _bucketRed;
        [SerializeField, Tooltip("バケツスプライト青")] private Sprite _bucketBlue;
        [SerializeField, Tooltip("バケツスプライト緑")] private Sprite _bucketGreen;
        [SerializeField, Tooltip("バケツスプライト黄")] private Sprite _bucketYellow;

        private Camera _mainCamera;
        private Transform _currentDrop;
        private int _currentColor; // 0=red, 1=blue, 2=green, 3=yellow
        private int _spawnedCount;
        private float _fallSpeed = 2f;
        private bool _isDragging;

        private static readonly float[] BucketXPositions = { -2.7f, -0.9f, 0.9f, 2.7f };
        private const float BucketY = -4f;
        private const float SpawnY = 5.5f;

        private Sprite[] _dropSprites;
        private Sprite[] _bucketSprites;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void StartGame()
        {
            _dropSprites = new[] { _dropRed, _dropBlue, _dropGreen, _dropYellow };
            _bucketSprites = new[] { _bucketRed, _bucketBlue, _bucketGreen, _bucketYellow };
            _spawnedCount = 0;
            SpawnBuckets();
            SpawnNextDrop();
        }

        private void SpawnBuckets()
        {
            for (int i = 0; i < 4; i++)
            {
                var obj = new GameObject($"Bucket_{i}");
                obj.transform.position = new Vector3(BucketXPositions[i], BucketY, 0f);
                obj.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _bucketSprites[i];
                sr.sortingOrder = 1;
                obj.transform.SetParent(transform);
            }
        }

        private void SpawnNextDrop()
        {
            if (_spawnedCount >= _gameManager.TotalDrops || !_gameManager.IsPlaying) return;

            _currentColor = Random.Range(0, 4);
            var obj = new GameObject("Drop");
            obj.transform.position = new Vector3(0f, SpawnY, 0f);
            obj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _dropSprites[_currentColor];
            sr.sortingOrder = 5;
            _currentDrop = obj.transform;
            _spawnedCount++;
            _isDragging = false;
            _fallSpeed = 2f + _spawnedCount * 0.1f;
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying || _currentDrop == null) return;

            // 落下
            _currentDrop.position += new Vector3(0f, -_fallSpeed * Time.deltaTime, 0f);

            // ドラッグ
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame) _isDragging = true;
                if (Mouse.current.leftButton.wasReleasedThisFrame) _isDragging = false;

                if (_isDragging && Mouse.current.leftButton.isPressed)
                {
                    Vector3 mp = Mouse.current.position.ReadValue();
                    mp.z = -_mainCamera.transform.position.z;
                    Vector3 wp = _mainCamera.ScreenToWorldPoint(mp);
                    var pos = _currentDrop.position;
                    pos.x = Mathf.Clamp(wp.x, -4f, 4f);
                    _currentDrop.position = pos;
                }
            }

            // バケツに到達
            if (_currentDrop.position.y <= BucketY)
            {
                JudgeBucket();
            }
        }

        private void JudgeBucket()
        {
            float dropX = _currentDrop.position.x;
            int closestBucket = 0;
            float closestDist = float.MaxValue;
            for (int i = 0; i < BucketXPositions.Length; i++)
            {
                float d = Mathf.Abs(dropX - BucketXPositions[i]);
                if (d < closestDist) { closestDist = d; closestBucket = i; }
            }

            bool correct = closestBucket == _currentColor;
            var sr = _currentDrop.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = correct ? Color.white : new Color(0.3f, 0.3f, 0.3f);

            Destroy(_currentDrop.gameObject, 0.3f);
            _currentDrop = null;

            if (correct) _gameManager.OnCorrectDrop();
            else _gameManager.OnWrongDrop();

            CancelInvoke(nameof(SpawnNextDrop));
            if (_gameManager.IsPlaying) Invoke(nameof(SpawnNextDrop), 0.4f);
        }
    }
}
