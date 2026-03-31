using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game024_BubblePop
{
    public class BubbleManager : MonoBehaviour
    {
        [SerializeField] private GameObject _bubblePrefab;
        [SerializeField] private float _spawnInterval = 0.8f;
        [SerializeField] private float _bubbleSpeed = 1.5f;

        private readonly List<Bubble> _bubbles = new List<Bubble>();
        private float _spawnTimer;
        private bool _isRunning;
        private float _elapsedTime;

        private BubblePopGameManager _gameManager;
        private Camera _mainCamera;

        private static readonly string[] BubbleNames = { "red", "blue", "green", "yellow", "purple" };

        private void Awake()
        {
            _gameManager = GetComponentInParent<BubblePopGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;
            HandleInput();
            _elapsedTime += Time.deltaTime;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnBubble();
                float speedup = Mathf.Min(_elapsedTime * 0.01f, 0.5f);
                _spawnTimer = Mathf.Max(_spawnInterval - speedup, 0.3f);
            }

            CheckEscaped();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var bubble = hit.GetComponent<Bubble>();
                    if (bubble != null && !bubble.IsPopped)
                    {
                        bubble.Pop();
                        if (_gameManager != null) _gameManager.OnBubblePopped();
                    }
                }
            }
        }

        private void SpawnBubble()
        {
            if (_bubblePrefab == null) return;
            var obj = Instantiate(_bubblePrefab, transform);
            int colorIdx = Random.Range(0, BubbleNames.Length);

            // Spawn from random edge
            float side = Random.value;
            Vector2 pos, vel;
            float speed = _bubbleSpeed + _elapsedTime * 0.02f;

            if (side < 0.25f) { pos = new Vector2(-6f, Random.Range(-4f, 4f)); vel = new Vector2(speed, Random.Range(-0.5f, 0.5f)); }
            else if (side < 0.5f) { pos = new Vector2(6f, Random.Range(-4f, 4f)); vel = new Vector2(-speed, Random.Range(-0.5f, 0.5f)); }
            else if (side < 0.75f) { pos = new Vector2(Random.Range(-5f, 5f), -5f); vel = new Vector2(Random.Range(-0.5f, 0.5f), speed); }
            else { pos = new Vector2(Random.Range(-5f, 5f), 5f); vel = new Vector2(Random.Range(-0.5f, 0.5f), -speed); }

            obj.transform.position = pos;
            float scale = Random.Range(0.8f, 1.4f);
            obj.transform.localScale = Vector3.one * scale;

            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var sprite = Resources.Load<Sprite>($"Sprites/Game024_BubblePop/bubble_{BubbleNames[colorIdx]}");
                if (sprite != null) sr.sprite = sprite;
            }

            var bubble = obj.GetComponent<Bubble>();
            if (bubble != null) bubble.Initialize(colorIdx, vel);
            _bubbles.Add(bubble);
        }

        private void CheckEscaped()
        {
            for (int i = _bubbles.Count - 1; i >= 0; i--)
            {
                if (_bubbles[i] == null || _bubbles[i].IsPopped) { _bubbles.RemoveAt(i); continue; }
                var pos = _bubbles[i].transform.position;
                // Check if bubble crossed center and went past opposite side
                if (Mathf.Abs(pos.x) > 7f || Mathf.Abs(pos.y) > 6f)
                {
                    Destroy(_bubbles[i].gameObject);
                    _bubbles.RemoveAt(i);
                    if (_gameManager != null) _gameManager.OnBubbleEscaped();
                }
            }
        }

        public void StartGame()
        {
            ClearAll();
            _spawnTimer = 1f;
            _elapsedTime = 0f;
            _isRunning = true;
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var b in _bubbles) if (b != null) Destroy(b.gameObject);
            _bubbles.Clear();
        }
    }
}
