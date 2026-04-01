using UnityEngine;
using UnityEngine.InputSystem;

namespace Game055_DustSweep
{
    public class SweepManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ダストスプライト")] private Sprite _dustSprite;
        [SerializeField, Tooltip("グリッド幅")] private int _gridWidth = 8;
        [SerializeField, Tooltip("グリッド高")] private int _gridHeight = 10;
        [SerializeField, Tooltip("ブラシサイズ")] private float _brushRadius = 0.8f;

        private Camera _mainCamera;
        private bool _isActive;
        private GameObject[,] _dustTiles;
        private bool[,] _cleaned;
        private int _totalTiles;
        private int _cleanedCount;
        private float _tileSize = 0.7f;
        private Vector2 _gridOrigin;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _totalTiles = _gridWidth * _gridHeight;
            _cleanedCount = 0;
            _dustTiles = new GameObject[_gridHeight, _gridWidth];
            _cleaned = new bool[_gridHeight, _gridWidth];

            float totalW = _gridWidth * _tileSize;
            float totalH = _gridHeight * _tileSize;
            _gridOrigin = new Vector2(-totalW / 2f + _tileSize / 2f, totalH / 2f - _tileSize / 2f);

            for (int r = 0; r < _gridHeight; r++)
                for (int c = 0; c < _gridWidth; c++)
                {
                    var obj = new GameObject($"Dust_{r}_{c}");
                    obj.transform.position = new Vector3(
                        _gridOrigin.x + c * _tileSize,
                        _gridOrigin.y - r * _tileSize, 0f);
                    obj.transform.localScale = new Vector3(_tileSize * 0.01f, _tileSize * 0.01f, 1f);

                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = _dustSprite;
                    sr.sortingOrder = 5;
                    // Slight color variation
                    float v = Random.Range(0.85f, 1f);
                    sr.color = new Color(v, v * 0.9f, v * 0.75f, Random.Range(0.7f, 1f));

                    _dustTiles[r, c] = obj;
                    _cleaned[r, c] = false;
                }
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                CleanArea(wp);
            }
        }

        private void CleanArea(Vector2 worldPos)
        {
            for (int r = 0; r < _gridHeight; r++)
                for (int c = 0; c < _gridWidth; c++)
                {
                    if (_cleaned[r, c]) continue;
                    float tx = _gridOrigin.x + c * _tileSize;
                    float ty = _gridOrigin.y - r * _tileSize;
                    if (Vector2.Distance(worldPos, new Vector2(tx, ty)) < _brushRadius)
                    {
                        _cleaned[r, c] = true;
                        _cleanedCount++;
                        if (_dustTiles[r, c] != null)
                            Destroy(_dustTiles[r, c]);
                    }
                }
        }

        public float CleanRatio => _totalTiles > 0 ? (float)_cleanedCount / _totalTiles : 0f;
    }
}
