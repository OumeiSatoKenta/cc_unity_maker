using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game056_InflateFloat
{
    public class BalloonManager : MonoBehaviour
    {
        [SerializeField] private InflateFloatGameManager _gameManager;

        private const float MinSize = 0.5f;
        private const float MaxSize = 2f;
        private const float InflateRate = 0.8f;
        private const float DeflateRate = 0.3f;
        private const float BuoyancyBase = 3f;
        private const float HorizontalSpeed = 3f;
        private const float Gravity = 4f;

        private GameObject _balloon;
        private float _size;
        private float _velocityY;
        private Vector2 _goalPos;
        private List<GameObject> _obstacles = new List<GameObject>();
        private Sprite _balloonSprite, _spikeSprite, _cloudSprite, _goalSprite;
        private Camera _mainCamera;

        private struct StageData { public Vector2 Start, Goal; public Vector2[] Spikes; public Vector2[] Clouds; }
        private static readonly StageData[] Stages = {
            new StageData { Start = new Vector2(-3f, 0f), Goal = new Vector2(3f, 1f),
                Spikes = new Vector2[] { new Vector2(0f, 3f), new Vector2(1f, -2f) },
                Clouds = new Vector2[] { new Vector2(-1f, -1f), new Vector2(2f, 0f) } },
            new StageData { Start = new Vector2(-3.5f, -1f), Goal = new Vector2(3.5f, 2f),
                Spikes = new Vector2[] { new Vector2(-1f, 2f), new Vector2(1f, -1f), new Vector2(2.5f, 3f) },
                Clouds = new Vector2[] { new Vector2(0f, 0f), new Vector2(-2f, 1f) } },
            new StageData { Start = new Vector2(-4f, 0f), Goal = new Vector2(4f, 0f),
                Spikes = new Vector2[] { new Vector2(-2f, 3f), new Vector2(0f, -2f), new Vector2(2f, 2.5f), new Vector2(3f, -1f) },
                Clouds = new Vector2[] { new Vector2(-1f, 1f), new Vector2(1f, -0.5f), new Vector2(3f, 1f) } },
        };

        public void Init(int stage)
        {
            _mainCamera = Camera.main;
            _balloonSprite = Resources.Load<Sprite>("Sprites/Game056_InflateFloat/balloon");
            _spikeSprite = Resources.Load<Sprite>("Sprites/Game056_InflateFloat/spike");
            _cloudSprite = Resources.Load<Sprite>("Sprites/Game056_InflateFloat/cloud");
            _goalSprite = Resources.Load<Sprite>("Sprites/Game056_InflateFloat/goal");

            CleanUp();

            int idx = (stage - 1) % Stages.Length;
            var s = Stages[idx];
            _goalPos = s.Goal;

            foreach (var sp in s.Spikes)
            {
                var go = new GameObject("Spike"); go.transform.position = new Vector3(sp.x, sp.y, 0f);
                go.transform.localScale = Vector3.one * 1.2f;
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _spikeSprite; sr.sortingOrder = 2;
                _obstacles.Add(go);
            }
            foreach (var cp in s.Clouds)
            {
                var go = new GameObject("Cloud"); go.transform.position = new Vector3(cp.x, cp.y, 0f);
                go.transform.localScale = new Vector3(2f, 0.5f, 1f);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _cloudSprite; sr.sortingOrder = 1;
                _obstacles.Add(go);
            }

            var gGo = new GameObject("Goal"); gGo.transform.position = new Vector3(s.Goal.x, s.Goal.y, 0f);
            gGo.transform.localScale = Vector3.one * 1.3f;
            var gsr = gGo.AddComponent<SpriteRenderer>(); gsr.sprite = _goalSprite; gsr.sortingOrder = 3;
            _obstacles.Add(gGo);

            _balloon = new GameObject("Balloon");
            _balloon.transform.position = new Vector3(s.Start.x, s.Start.y, 0f);
            var bsr = _balloon.AddComponent<SpriteRenderer>(); bsr.sprite = _balloonSprite; bsr.sortingOrder = 10;
            _size = 1f; _velocityY = 0f;
            UpdateBalloonScale();
        }

        private void CleanUp()
        {
            foreach (var o in _obstacles) if (o != null) Destroy(o);
            _obstacles.Clear();
            if (_balloon != null) { Destroy(_balloon); _balloon = null; }
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsActive || _balloon == null) return;

            bool pressing = Mouse.current != null && Mouse.current.leftButton.isPressed;
            if (pressing)
                _size += InflateRate * Time.deltaTime;
            else
                _size -= DeflateRate * Time.deltaTime;
            _size = Mathf.Clamp(_size, MinSize * 0.8f, MaxSize * 1.1f);

            if (_size > MaxSize) { if (_gameManager != null) _gameManager.OnBalloonPopped(); return; }

            float buoyancy = ((_size - MinSize) / (MaxSize - MinSize)) * BuoyancyBase * 2f - BuoyancyBase * 0.5f;
            _velocityY += (buoyancy - Gravity) * Time.deltaTime;
            _velocityY *= 0.95f;

            float moveX = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) moveX -= 1f;
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) moveX += 1f;
            }
            if (moveX == 0f && pressing && Mouse.current != null)
            {
                var sp = Mouse.current.position.ReadValue();
                if (sp.x < Screen.width * 0.35f) moveX = -1f;
                else if (sp.x > Screen.width * 0.65f) moveX = 1f;
            }

            var pos = _balloon.transform.position;
            pos.x += moveX * HorizontalSpeed * Time.deltaTime;
            pos.y += _velocityY * Time.deltaTime;
            _balloon.transform.position = pos;
            UpdateBalloonScale();

            if (pos.y < -6f) { if (_gameManager != null) _gameManager.OnBalloonFell(); return; }
            if (Vector2.Distance(pos, _goalPos) < 1f) { if (_gameManager != null) _gameManager.OnReachGoal(); return; }

            foreach (var o in _obstacles)
            {
                if (o == null || o.name != "Spike") continue;
                if (Vector2.Distance(pos, o.transform.position) < 0.8f)
                {
                    if (_gameManager != null) _gameManager.OnBalloonPopped(); return;
                }
            }
        }

        private void UpdateBalloonScale()
        {
            if (_balloon != null)
                _balloon.transform.localScale = Vector3.one * _size;
        }
    }
}
