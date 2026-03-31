using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game057_CandyDrop
{
    public class CandyManager : MonoBehaviour
    {
        [SerializeField] private CandyDropGameManager _gameManager;

        private const float DropY = 4.5f;
        private const float MoveSpeed = 5f;
        private const float FallCheckDelay = 2f;
        private const float PlatformY = -3.5f;

        private Sprite[] _candySprites;
        private Sprite _platformSprite;
        private Camera _mainCamera;
        private GameObject _currentCandy;
        private Rigidbody2D _currentRb;
        private List<GameObject> _placedCandies = new List<GameObject>();
        private bool _dropping;
        private float _checkTimer;
        private GameObject _platform;

        public void Init()
        {
            _mainCamera = Camera.main;
            _candySprites = new Sprite[] {
                Resources.Load<Sprite>("Sprites/Game057_CandyDrop/candy_square"),
                Resources.Load<Sprite>("Sprites/Game057_CandyDrop/candy_circle"),
                Resources.Load<Sprite>("Sprites/Game057_CandyDrop/candy_tri"),
            };
            _platformSprite = Resources.Load<Sprite>("Sprites/Game057_CandyDrop/platform");

            CleanUp();

            _platform = new GameObject("Platform");
            _platform.transform.position = new Vector3(0f, PlatformY, 0f);
            _platform.transform.localScale = new Vector3(3f, 0.5f, 1f);
            var sr = _platform.AddComponent<SpriteRenderer>(); sr.sprite = _platformSprite; sr.sortingOrder = 0;
            var bc = _platform.AddComponent<BoxCollider2D>(); bc.size = new Vector2(1f, 0.3f);
            _platform.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            _dropping = false;
            SpawnCandy();
        }

        private void CleanUp()
        {
            if (_currentCandy != null) Destroy(_currentCandy);
            foreach (var c in _placedCandies) if (c != null) Destroy(c);
            _placedCandies.Clear();
            if (_platform != null) Destroy(_platform);
        }

        private void SpawnCandy()
        {
            int type = Random.Range(0, _candySprites.Length);
            _currentCandy = new GameObject("Candy");
            _currentCandy.transform.position = new Vector3(0f, DropY, 0f);
            _currentCandy.transform.localScale = Vector3.one * 1.2f;
            var sr = _currentCandy.AddComponent<SpriteRenderer>();
            sr.sprite = _candySprites[type]; sr.sortingOrder = 5;
            float hue = Random.Range(0f, 1f);
            sr.color = Color.HSVToRGB(hue, 0.5f, 1f);

            if (type == 1)
            {
                var cc = _currentCandy.AddComponent<CircleCollider2D>(); cc.radius = 0.4f;
            }
            else
            {
                var bc = _currentCandy.AddComponent<BoxCollider2D>(); bc.size = new Vector2(0.8f, 0.8f);
            }

            _currentRb = _currentCandy.AddComponent<Rigidbody2D>();
            _currentRb.gravityScale = 0f;
            _currentRb.freezeRotation = true;
            _dropping = false;
        }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsGameOver) return;

            if (!_dropping && _currentCandy != null)
            {
                float moveX = 0f;
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) moveX -= 1f;
                    if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) moveX += 1f;
                }
                if (Mouse.current != null && Mouse.current.leftButton.isPressed)
                {
                    var sp = Mouse.current.position.ReadValue();
                    Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(sp.x, sp.y, 0f));
                    moveX = Mathf.Sign(wp.x - _currentCandy.transform.position.x);
                }

                var pos = _currentCandy.transform.position;
                pos.x = Mathf.Clamp(pos.x + moveX * MoveSpeed * Time.deltaTime, -3f, 3f);
                _currentCandy.transform.position = pos;

                if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame ||
                    Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    _dropping = true;
                    _currentRb.gravityScale = 2f;
                    _currentRb.freezeRotation = false;
                    _checkTimer = FallCheckDelay;
                }
            }

            if (_dropping && _currentCandy != null)
            {
                _checkTimer -= Time.deltaTime;
                if (_checkTimer <= 0f && _currentRb.linearVelocity.sqrMagnitude < 0.5f)
                {
                    _placedCandies.Add(_currentCandy);
                    _currentCandy = null;
                    if (_gameManager != null) _gameManager.OnCandyLanded();
                    SpawnCandy();
                }

                if (_currentCandy != null && _currentCandy.transform.position.y < -6f)
                {
                    Destroy(_currentCandy); _currentCandy = null;
                    if (_gameManager != null) _gameManager.OnGameOver();
                }
            }

            for (int i = _placedCandies.Count - 1; i >= 0; i--)
            {
                if (_placedCandies[i] == null) { _placedCandies.RemoveAt(i); continue; }
                if (_placedCandies[i].transform.position.y < -6f)
                {
                    Destroy(_placedCandies[i]); _placedCandies.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnGameOver();
                    return;
                }
            }
        }
    }
}
