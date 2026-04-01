using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game057_CandyDrop
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private CandyDropGameManager _gameManager;
        [SerializeField, Tooltip("丸キャンディ")] private Sprite _candyRoundSprite;
        [SerializeField, Tooltip("四角キャンディ")] private Sprite _candySquareSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private GameObject _currentCandy;
        private List<GameObject> _placedCandies = new List<GameObject>();
        private float _baseY = -4f;
        private float _collapseCheckTimer;

        private static readonly Color[] CandyColors = {
            new Color(1f, 0.5f, 0.7f), new Color(0.5f, 0.8f, 1f),
            new Color(1f, 0.8f, 0.3f), new Color(0.6f, 1f, 0.5f),
            new Color(0.8f, 0.5f, 1f),
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            SpawnCandy();
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            // Move current candy horizontally with mouse
            if (_currentCandy != null)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);
                var pos = _currentCandy.transform.position;
                pos.x = Mathf.Clamp(wp.x, -3f, 3f);
                _currentCandy.transform.position = pos;

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    DropCandy();
                }
            }

            // Check for collapse
            _collapseCheckTimer -= Time.deltaTime;
            if (_collapseCheckTimer <= 0f)
            {
                _collapseCheckTimer = 1f;
                CheckCollapse();
            }
        }

        private void SpawnCandy()
        {
            bool isRound = Random.value > 0.5f;
            _currentCandy = new GameObject("Candy");
            _currentCandy.transform.position = new Vector3(0f, 4f, 0f);

            var sr = _currentCandy.AddComponent<SpriteRenderer>();
            sr.sprite = isRound ? _candyRoundSprite : _candySquareSprite;
            sr.sortingOrder = 5;
            sr.color = CandyColors[Random.Range(0, CandyColors.Length)];

            float scale = Random.Range(0.8f, 1.3f);
            _currentCandy.transform.localScale = new Vector3(scale, scale, 1f);

            var rb = _currentCandy.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.mass = 0.5f;

            if (isRound)
            {
                var col = _currentCandy.AddComponent<CircleCollider2D>();
                col.radius = 0.22f;
            }
            else
            {
                var col = _currentCandy.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.44f, 0.44f);
            }
        }

        private void DropCandy()
        {
            if (_currentCandy == null) return;
            var rb = _currentCandy.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.5f;
            _placedCandies.Add(_currentCandy);
            _currentCandy = null;

            // Spawn next after delay
            Invoke(nameof(SpawnCandy), 0.8f);
        }

        private void CheckCollapse()
        {
            // If any candy fell below base, tower collapsed
            for (int i = _placedCandies.Count - 1; i >= 0; i--)
            {
                if (_placedCandies[i] == null) { _placedCandies.RemoveAt(i); continue; }
                if (_placedCandies[i].transform.position.y < _baseY - 2f)
                {
                    _gameManager.OnTowerCollapsed();
                    return;
                }
            }
        }

        public float TowerHeight
        {
            get
            {
                float maxY = 0f;
                foreach (var c in _placedCandies)
                {
                    if (c == null) continue;
                    var rb = c.GetComponent<Rigidbody2D>();
                    if (rb != null && rb.linearVelocity.magnitude < 0.1f)
                    {
                        float h = c.transform.position.y - _baseY;
                        if (h > maxY) maxY = h;
                    }
                }
                return maxY;
            }
        }
    }
}
