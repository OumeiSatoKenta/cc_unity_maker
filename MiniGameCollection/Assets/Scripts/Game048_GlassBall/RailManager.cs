using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game048_GlassBall
{
    public class RailManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")] private GlassBallGameManager _gameManager;
        [SerializeField, Tooltip("インク最大量")] private float _inkMax = 100f;
        [SerializeField, Tooltip("レール幅")] private float _railWidth = 0.15f;

        private Camera _mainCamera;
        private float _inkRemaining;
        private bool _isActive;
        private bool _isDrawing;
        private List<Vector2> _currentPoints;
        private GameObject _currentRail;
        private LineRenderer _currentLine;

        private const float MinPointDistance = 0.2f;
        private const float InkCostPerUnit = 5f;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _inkRemaining = _inkMax;
            _isActive = true;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame && _inkRemaining > 0f)
            {
                StartNewRail();
            }

            if (Mouse.current.leftButton.isPressed && _isDrawing)
            {
                ContinueRail();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && _isDrawing)
            {
                FinishRail();
            }
        }

        private void StartNewRail()
        {
            _isDrawing = true;
            _currentPoints = new List<Vector2>();

            _currentRail = new GameObject("Rail");
            _currentLine = _currentRail.AddComponent<LineRenderer>();
            _currentLine.startWidth = _railWidth;
            _currentLine.endWidth = _railWidth;
            _currentLine.material = new Material(Shader.Find("Sprites/Default"));
            _currentLine.startColor = new Color(0.6f, 0.8f, 1f, 0.8f);
            _currentLine.endColor = new Color(0.4f, 0.6f, 0.9f, 0.8f);
            _currentLine.sortingOrder = 5;
            _currentLine.positionCount = 0;

            Vector2 worldPos = GetWorldMousePos();
            AddPoint(worldPos);
        }

        private void ContinueRail()
        {
            if (_inkRemaining <= 0f) { FinishRail(); return; }

            Vector2 worldPos = GetWorldMousePos();
            if (_currentPoints.Count > 0)
            {
                float dist = Vector2.Distance(worldPos, _currentPoints[_currentPoints.Count - 1]);
                if (dist >= MinPointDistance)
                {
                    float cost = dist * InkCostPerUnit;
                    if (cost > _inkRemaining) { FinishRail(); return; }
                    _inkRemaining -= cost;
                    AddPoint(worldPos);
                }
            }
        }

        private void FinishRail()
        {
            _isDrawing = false;
            if (_currentRail != null && _currentPoints != null && _currentPoints.Count >= 2)
            {
                var edgeCol = _currentRail.AddComponent<EdgeCollider2D>();
                edgeCol.points = _currentPoints.ToArray();
            }
            else if (_currentRail != null)
            {
                Destroy(_currentRail);
            }
            _currentRail = null;
            _currentLine = null;
            _currentPoints = null;
        }

        private void AddPoint(Vector2 pos)
        {
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

        public float InkRatio => _inkRemaining / _inkMax;
    }
}
