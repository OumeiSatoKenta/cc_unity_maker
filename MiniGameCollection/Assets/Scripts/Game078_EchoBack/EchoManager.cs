using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game078_EchoBack
{
    public class EchoManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private EchoBackGameManager _gameManager;
        [SerializeField, Tooltip("赤キー")] private Sprite _keyRedSprite;
        [SerializeField, Tooltip("青キー")] private Sprite _keyBlueSprite;
        [SerializeField, Tooltip("緑キー")] private Sprite _keyGreenSprite;
        [SerializeField, Tooltip("黄キー")] private Sprite _keyYellowSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private GameObject[] _keys;
        private SpriteRenderer[] _keySrs;
        private List<int> _pattern = new List<int>();
        private int _inputIndex;
        private bool _isShowingPattern;
        private float _showTimer;
        private int _showIndex;
        private bool _waitingForInput;

        private static readonly Color[] KeyColors = {
            new Color(0.86f, 0.24f, 0.24f), new Color(0.24f, 0.39f, 0.86f),
            new Color(0.24f, 0.71f, 0.24f), new Color(0.86f, 0.78f, 0.16f)
        };
        private static readonly Color[] KeyHighlight = {
            new Color(1f, 0.5f, 0.5f), new Color(0.5f, 0.6f, 1f),
            new Color(0.5f, 1f, 0.5f), new Color(1f, 0.95f, 0.5f)
        };
        private static readonly Vector2[] KeyPositions = {
            new(-1.5f, 1f), new(1.5f, 1f), new(-1.5f, -1.5f), new(1.5f, -1.5f)
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            Sprite[] sprites = { _keyRedSprite, _keyBlueSprite, _keyGreenSprite, _keyYellowSprite };
            _keys = new GameObject[4];
            _keySrs = new SpriteRenderer[4];

            for (int i = 0; i < 4; i++)
            {
                var obj = new GameObject($"Key_{i}");
                obj.transform.position = KeyPositions[i];
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = sprites[i]; sr.sortingOrder = 3;
                sr.color = KeyColors[i];
                var col = obj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.75f, 0.75f);
                _keys[i] = obj;
                _keySrs[i] = sr;
            }

            NextRound(0);
        }

        public void StopGame() { _isActive = false; }

        public void NextRound(int round)
        {
            // Add one more to pattern
            _pattern.Add(Random.Range(0, 4));
            ShowPattern();
        }

        public void ReplayPattern()
        {
            ShowPattern();
        }

        private void ShowPattern()
        {
            _isShowingPattern = true;
            _waitingForInput = false;
            _showIndex = 0;
            _showTimer = 0.8f;

            // Reset all keys
            for (int i = 0; i < 4; i++)
                _keySrs[i].color = KeyColors[i];
        }

        private void Update()
        {
            if (!_isActive) return;

            if (_isShowingPattern)
            {
                _showTimer -= Time.deltaTime;
                if (_showTimer <= 0f)
                {
                    // Reset previous highlight
                    if (_showIndex > 0)
                        _keySrs[_pattern[_showIndex - 1]].color = KeyColors[_pattern[_showIndex - 1]];

                    if (_showIndex < _pattern.Count)
                    {
                        _keySrs[_pattern[_showIndex]].color = KeyHighlight[_pattern[_showIndex]];
                        _showIndex++;
                        _showTimer = 0.6f;
                    }
                    else
                    {
                        // Pattern shown, wait for input
                        for (int i = 0; i < 4; i++) _keySrs[i].color = KeyColors[i];
                        _isShowingPattern = false;
                        _waitingForInput = true;
                        _inputIndex = 0;
                    }
                }
                return;
            }

            if (!_waitingForInput) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (_keys[i] == hit.gameObject)
                        {
                            HandleKeyPress(i);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleKeyPress(int key)
        {
            // Flash
            _keySrs[key].color = KeyHighlight[key];
            Invoke(nameof(ResetKeyColors), 0.2f);

            if (key == _pattern[_inputIndex])
            {
                _inputIndex++;
                if (_inputIndex >= _pattern.Count)
                {
                    _waitingForInput = false;
                    _gameManager.OnPatternCorrect();
                }
            }
            else
            {
                _waitingForInput = false;
                _gameManager.OnPatternWrong();
            }
        }

        private void ResetKeyColors()
        {
            for (int i = 0; i < 4; i++) _keySrs[i].color = KeyColors[i];
        }
    }
}
