using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game072_DrumKit
{
    public class DrumManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private DrumKitGameManager _gameManager;
        [SerializeField, Tooltip("スネアスプライト")] private Sprite _snareSprite;
        [SerializeField, Tooltip("バスドラスプライト")] private Sprite _bassSprite;
        [SerializeField, Tooltip("ハイハットスプライト")] private Sprite _hihatSprite;
        [SerializeField, Tooltip("シンバルスプライト")] private Sprite _cymbalSprite;
        [SerializeField, Tooltip("タムスプライト")] private Sprite _tomSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int[] _pattern;
        private int _currentTarget;
        private GameObject[] _drums;
        private float _highlightTimer;
        private int _highlightIndex = -1;

        private static readonly string[] DrumNames = { "Snare", "Bass", "HiHat", "Cymbal", "Tom" };
        private static readonly Vector2[] DrumPositions = {
            new(-1.5f, -1f), new(0f, -3f), new(-3f, 1f), new(3f, 1f), new(1.5f, -1f)
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame(int patternLength)
        {
            _isActive = true;
            _drums = new GameObject[5];

            Sprite[] sprites = { _snareSprite, _bassSprite, _hihatSprite, _cymbalSprite, _tomSprite };

            for (int i = 0; i < 5; i++)
            {
                var obj = new GameObject(DrumNames[i]);
                obj.transform.position = DrumPositions[i];
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = sprites[i]; sr.sortingOrder = 3;
                var col = obj.AddComponent<CircleCollider2D>();
                col.radius = 0.4f;
                _drums[i] = obj;
            }

            // Generate random pattern
            _pattern = new int[patternLength];
            for (int i = 0; i < patternLength; i++)
                _pattern[i] = Random.Range(0, 5);

            _currentTarget = 0;
            ShowNextTarget();
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Highlight animation
            if (_highlightTimer > 0f)
            {
                _highlightTimer -= Time.deltaTime;
                if (_highlightTimer <= 0f && _highlightIndex >= 0 && _highlightIndex < _drums.Length)
                {
                    var sr = _drums[_highlightIndex].GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = Color.white;
                }
            }

            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null)
                {
                    for (int i = 0; i < _drums.Length; i++)
                    {
                        if (_drums[i] == hit.gameObject)
                        {
                            HitDrum(i);
                            break;
                        }
                    }
                }
            }
        }

        private void HitDrum(int index)
        {
            // Visual feedback
            var sr = _drums[index].GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.yellow;
            _highlightIndex = index;
            _highlightTimer = 0.15f;

            if (_currentTarget < _pattern.Length && index == _pattern[_currentTarget])
            {
                _currentTarget++;
                _gameManager.OnCorrectHit();
                if (_currentTarget < _pattern.Length) ShowNextTarget();
            }
            else
            {
                _gameManager.OnMiss();
            }
        }

        private void ShowNextTarget()
        {
            if (_currentTarget >= _pattern.Length) return;
            int target = _pattern[_currentTarget];
            // Briefly highlight target drum
            for (int i = 0; i < _drums.Length; i++)
            {
                var sr = _drums[i].GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = (i == target) ? new Color(0.5f, 1f, 0.5f) : Color.white;
            }
        }
    }
}
