using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game051v2_DrawBridge
{
    public class DrawingManager : MonoBehaviour
    {
        [SerializeField] DrawBridgeGameManager _gameManager;
        [SerializeField] DrawBridgeUI _ui;
        [SerializeField] Material _lineMaterial;

        public float InkRemaining { get; private set; } = 1.0f;

        private float _maxInk = 1.0f;
        private float _inkCostPerUnit = 0.05f;
        private bool _isActive = false;
        private bool _isDrawingEnabled = false;
        private bool _isDrawing = false;

        private List<Vector2> _currentStroke = new List<Vector2>();
        private List<GameObject> _lineObjects = new List<GameObject>();
        private float _totalDrawnLength = 0f;

        // Stage features
        private bool _hasObstacle = false;
        private bool _hasWind = false;
        private bool _hasBreakable = false;
        private bool _hasDualGap = false;
        private float _breakableThreshold = 3f; // max line length before breaking
        private Vector2 _windForce = Vector2.zero;

        // Obstacle objects spawned by this manager
        private List<GameObject> _stageObjects = new List<GameObject>();

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            ClearLines();
            ClearStageObjects();

            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camWidth = Camera.main != null ? camSize * Camera.main.aspect : 2.8f;

            // Set ink based on stage
            switch (stageNumber)
            {
                case 1: _maxInk = 1.0f; break;
                case 2: _maxInk = 0.8f; break;
                case 3: _maxInk = 0.8f; break;
                case 4: _maxInk = 0.6f; break;
                case 5: _maxInk = 0.6f; break;
                default: _maxInk = 1.0f; break;
            }
            InkRemaining = _maxInk;
            _inkCostPerUnit = _maxInk / 8f; // 8 units of drawing max

            // Stage features from complexityFactor
            float cf = config.complexityFactor;
            _hasObstacle = cf >= 0.2f;
            _hasWind = cf >= 0.4f;
            _hasBreakable = cf >= 0.6f;
            _hasDualGap = cf >= 0.8f;

            _windForce = _hasWind ? new Vector2(-2f * config.speedMultiplier, 0f) : Vector2.zero;
            _breakableThreshold = _hasBreakable ? 2.5f : 100f;

            // Spawn obstacle if needed
            if (_hasObstacle && !_hasDualGap)
            {
                SpawnRock(new Vector2(0f, 0.5f));
            }

            _isActive = true;
            _isDrawingEnabled = true;
            _totalDrawnLength = 0f;
            UpdateUI();
        }

        private void SpawnRock(Vector2 position)
        {
            var rockSprite = Resources.Load<Sprite>("Sprites/Game051v2_DrawBridge/Rock");
            if (rockSprite == null) return;

            var rockObj = new GameObject("Rock");
            var sr = rockObj.AddComponent<SpriteRenderer>();
            sr.sprite = rockSprite;
            sr.sortingOrder = 2;
            rockObj.transform.position = position;
            rockObj.transform.localScale = Vector3.one * 0.6f;

            var col = rockObj.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;

            _stageObjects.Add(rockObj);
        }

        private void ClearStageObjects()
        {
            foreach (var obj in _stageObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _stageObjects.Clear();
        }

        public void SetDrawingEnabled(bool enabled)
        {
            _isDrawingEnabled = enabled;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            _isDrawingEnabled = active;
        }

        void Update()
        {
            if (!_isActive || !_isDrawingEnabled) return;
            if (Camera.main == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mousePos = mouse.position.ReadValue();
            Vector3 worldPos3 = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -Camera.main.transform.position.z));
            Vector2 worldPos = new Vector2(worldPos3.x, worldPos3.y);

            // Block drawing in bottom UI area
            float camSize = Camera.main.orthographicSize;
            float bottomLimit = -camSize + 2.8f;
            if (worldPos.y < bottomLimit)
            {
                if (_isDrawing) EndStroke();
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                StartStroke(worldPos);
            }
            else if (mouse.leftButton.isPressed && _isDrawing)
            {
                ContinueStroke(worldPos);
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _isDrawing)
            {
                EndStroke();
            }
        }

        private void StartStroke(Vector2 pos)
        {
            if (InkRemaining <= 0f) return;
            _isDrawing = true;
            _currentStroke.Clear();
            _currentStroke.Add(pos);
        }

        private void ContinueStroke(Vector2 pos)
        {
            if (_currentStroke.Count == 0) return;
            Vector2 last = _currentStroke[_currentStroke.Count - 1];
            float dist = Vector2.Distance(last, pos);
            if (dist < 0.05f) return;

            float inkCost = dist * _inkCostPerUnit;
            if (inkCost > InkRemaining) inkCost = InkRemaining;

            InkRemaining -= inkCost;
            InkRemaining = Mathf.Max(0f, InkRemaining);
            _totalDrawnLength += dist;
            _currentStroke.Add(pos);

            UpdateCurrentLineRenderer();
            UpdateUI();

            if (InkRemaining <= 0f)
            {
                EndStroke();
            }
        }

        private void EndStroke()
        {
            _isDrawing = false;
            if (_currentStroke.Count < 2)
            {
                _currentStroke.Clear();
                return;
            }

            CreatePhysicsLine(_currentStroke.ToArray());
            _currentStroke.Clear();
        }

        private GameObject _currentLineObj;

        private void UpdateCurrentLineRenderer()
        {
            if (_currentLineObj == null)
            {
                _currentLineObj = CreateLineRendererObject();
                _lineObjects.Add(_currentLineObj);
            }

            var lr = _currentLineObj.GetComponent<LineRenderer>();
            if (lr == null) return;

            lr.positionCount = _currentStroke.Count;
            for (int i = 0; i < _currentStroke.Count; i++)
                lr.SetPosition(i, new Vector3(_currentStroke[i].x, _currentStroke[i].y, 0f));
        }

        private void CreatePhysicsLine(Vector2[] points)
        {
            // Use existing or create new line object
            if (_currentLineObj == null)
            {
                _currentLineObj = CreateLineRendererObject();
                _lineObjects.Add(_currentLineObj);
            }

            float lineLength = 0f;
            for (int i = 1; i < points.Length; i++)
                lineLength += Vector2.Distance(points[i-1], points[i]);

            var ec = _currentLineObj.AddComponent<EdgeCollider2D>();
            ec.points = points;
            ec.edgeRadius = 0.05f;

            var lr = _currentLineObj.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.positionCount = points.Length;
                for (int i = 0; i < points.Length; i++)
                    lr.SetPosition(i, new Vector3(points[i].x, points[i].y, 0f));

                // Breakable: tint red if over threshold
                if (_hasBreakable && lineLength > _breakableThreshold)
                {
                    lr.startColor = new Color(1f, 0.3f, 0.3f, 0.9f);
                    lr.endColor = new Color(1f, 0.3f, 0.3f, 0.9f);
                    // Will break when ball weight exceeds threshold
                    var breakComp = _currentLineObj.AddComponent<BreakableLineComponent>();
                    breakComp.Initialize(lineLength, _breakableThreshold);
                }
            }

            _currentLineObj = null;
        }

        private GameObject CreateLineRendererObject()
        {
            var obj = new GameObject("DrawnLine");
            obj.layer = LayerMask.NameToLayer("Default");

            var lr = obj.AddComponent<LineRenderer>();
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.sortingOrder = 5;
            lr.useWorldSpace = true;
            lr.positionCount = 0;

            if (_lineMaterial != null)
            {
                lr.material = _lineMaterial;
            }
            else
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }

            // Casual category: green/yellow palette
            Color lineColor = new Color(0.3f, 0.7f, 0.3f, 0.9f);
            lr.startColor = lineColor;
            lr.endColor = lineColor;

            return obj;
        }

        public void ClearLines()
        {
            _isDrawing = false;
            _currentStroke.Clear();
            if (_currentLineObj != null)
            {
                if (!_lineObjects.Contains(_currentLineObj))
                    Destroy(_currentLineObj);
                _currentLineObj = null;
            }

            foreach (var obj in _lineObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _lineObjects.Clear();
            _totalDrawnLength = 0f;
            InkRemaining = _maxInk;
            UpdateUI();
        }

        public float GetEfficiencyRatio()
        {
            if (_totalDrawnLength <= 0f) return 1f;
            // Estimate minimum needed length as gap width (~3 units)
            float minNeeded = 3f;
            return _totalDrawnLength / minNeeded;
        }

        public Vector2 GetWindForce() => _windForce;
        public bool HasWind => _hasWind;

        private void UpdateUI()
        {
            if (_ui != null)
                _ui.UpdateInk(InkRemaining / _maxInk);
        }

        void OnDestroy()
        {
            ClearLines();
            ClearStageObjects();
        }
    }
}
