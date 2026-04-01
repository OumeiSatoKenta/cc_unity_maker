using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game051_DrawBridge
{
    public class BridgeManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private DrawBridgeGameManager _gameManager;
        [SerializeField, Tooltip("インク最大量")] private float _inkMax = 80f;
        [SerializeField, Tooltip("線幅")] private float _lineWidth = 0.12f;

        private Camera _mainCamera;
        private Material _lineMaterial;
        private float _inkRemaining;
        private bool _isActive;
        private bool _isDrawing;
        private bool _drawingComplete;
        private List<Vector2> _currentPoints;
        private GameObject _currentBridge;
        private LineRenderer _currentLine;

        private const float MinPointDistance = 0.15f;
        private const float InkCostPerUnit = 4f;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _lineMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null) Destroy(_lineMaterial);
        }

        public void StartGame()
        {
            _inkRemaining = _inkMax;
            _isActive = true;
            _drawingComplete = false;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive || _drawingComplete) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame && _inkRemaining > 0f)
            {
                StartNewBridge();
            }

            if (Mouse.current.leftButton.isPressed && _isDrawing)
            {
                ContinueBridge();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && _isDrawing)
            {
                FinishBridge();
            }

            // Double tap or second release to start walking
            if (!_isDrawing && !_drawingComplete && _inkRemaining <= 0f)
            {
                _drawingComplete = true;
                _gameManager.OnDrawingFinished();
            }
        }

        public void FinishDrawingAndGo()
        {
            if (_isDrawing) FinishBridge();
            _drawingComplete = true;
            _gameManager.OnDrawingFinished();
        }

        private void StartNewBridge()
        {
            _isDrawing = true;
            _currentPoints = new List<Vector2>();
            _currentBridge = new GameObject("Bridge");
            _currentLine = _currentBridge.AddComponent<LineRenderer>();
            _currentLine.startWidth = _lineWidth;
            _currentLine.endWidth = _lineWidth;
            _currentLine.sharedMaterial = _lineMaterial;
            _currentLine.startColor = new Color(0.6f, 0.4f, 0.2f, 0.9f);
            _currentLine.endColor = new Color(0.5f, 0.35f, 0.15f, 0.9f);
            _currentLine.sortingOrder = 3;
            _currentLine.positionCount = 0;

            Vector2 wp = GetWorldMousePos();
            AddPoint(wp);
        }

        private void ContinueBridge()
        {
            if (_inkRemaining <= 0f) { FinishBridge(); return; }
            Vector2 wp = GetWorldMousePos();
            if (_currentPoints.Count > 0)
            {
                float dist = Vector2.Distance(wp, _currentPoints[_currentPoints.Count - 1]);
                if (dist >= MinPointDistance)
                {
                    float cost = dist * InkCostPerUnit;
                    if (cost > _inkRemaining) { FinishBridge(); return; }
                    _inkRemaining -= cost;
                    AddPoint(wp);
                }
            }
        }

        private void FinishBridge()
        {
            _isDrawing = false;
            if (_currentBridge != null && _currentPoints != null && _currentPoints.Count >= 2)
            {
                var edgeCol = _currentBridge.AddComponent<EdgeCollider2D>();
                edgeCol.points = _currentPoints.ToArray();
            }
            else if (_currentBridge != null)
            {
                Destroy(_currentBridge);
            }
            _currentBridge = null;
            _currentLine = null;
            _currentPoints = null;
        }

        private void AddPoint(Vector2 pos)
        {
            if (_currentLine == null || _currentPoints == null) return;
            _currentPoints.Add(pos);
            _currentLine.positionCount = _currentPoints.Count;
            _currentLine.SetPosition(_currentPoints.Count - 1, new Vector3(pos.x, pos.y, 0f));
        }

        private Vector2 GetWorldMousePos()
        {
            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            return _mainCamera.ScreenToWorldPoint(mp);
        }

        public float InkRatio => _inkMax > 0f ? _inkRemaining / _inkMax : 0f;
    }
}
