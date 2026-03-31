using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game045_FingerPaint
{
    public class PaintManager : MonoBehaviour
    {
        [SerializeField] private FingerPaintGameManager _gameManager;

        private const float BrushSize = 0.15f;
        private const float MinDistance = 0.05f;
        private const float RhythmBPM = 120f;

        private Sprite _brushSprite;
        private Camera _mainCamera;
        private List<GameObject> _paintDots = new List<GameObject>();
        private bool _isPainting;
        private Vector3 _lastPos;
        private float _rhythmTimer;
        private float _currentHue;
        private bool _strokeStarted;

        public float RhythmPhase => (_rhythmTimer % (60f / RhythmBPM)) / (60f / RhythmBPM);

        public void Init()
        {
            _mainCamera = Camera.main;
            _brushSprite = Resources.Load<Sprite>("Sprites/Game045_FingerPaint/brush_dot");

            foreach (var d in _paintDots) if (d != null) Destroy(d);
            _paintDots.Clear();

            _isPainting = false;
            _rhythmTimer = 0f;
            _currentHue = 0f;
            _strokeStarted = false;
        }

        private void Update()
        {
            if (_gameManager != null && _gameManager.IsFinished) return;

            _rhythmTimer += Time.deltaTime;
            _currentHue = (_currentHue + Time.deltaTime * 0.1f) % 1f;

            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _isPainting = true;
                _strokeStarted = false;
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                wp.z = 0f;
                _lastPos = wp;
                PlaceDot(wp);
            }

            if (Mouse.current.leftButton.isPressed && _isPainting)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                wp.z = 0f;

                if (Vector3.Distance(wp, _lastPos) >= MinDistance)
                {
                    float dist = Vector3.Distance(wp, _lastPos);
                    int steps = Mathf.CeilToInt(dist / MinDistance);
                    for (int i = 1; i <= steps; i++)
                    {
                        Vector3 p = Vector3.Lerp(_lastPos, wp, (float)i / steps);
                        PlaceDot(p);
                    }
                    _lastPos = wp;

                    if (!_strokeStarted)
                    {
                        _strokeStarted = true;
                        if (_gameManager != null) _gameManager.OnStroke();
                    }
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isPainting = false;
            }
        }

        private void PlaceDot(Vector3 pos)
        {
            if (pos.x < -4.5f || pos.x > 4.5f || pos.y < -3.5f || pos.y > 3.5f) return;

            var go = new GameObject("Dot");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * BrushSize;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _brushSprite;
            sr.sortingOrder = 5;

            float beatPhase = RhythmPhase;
            float onBeat = 1f - Mathf.Abs(beatPhase - 0.5f) * 2f;
            float saturation = Mathf.Lerp(0.2f, 1f, onBeat);
            float brightness = Mathf.Lerp(0.5f, 1f, onBeat);
            sr.color = Color.HSVToRGB(_currentHue, saturation, brightness);

            _paintDots.Add(go);
        }
    }
}
