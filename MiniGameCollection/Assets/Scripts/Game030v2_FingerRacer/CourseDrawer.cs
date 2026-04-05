using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game030v2_FingerRacer
{
    public class CourseDrawer : MonoBehaviour
    {
        [SerializeField] FingerRacerGameManager _gameManager;
        [SerializeField] LineRenderer _lineRenderer;
        [SerializeField] Transform _startMarker;
        [SerializeField] Transform _goalMarker;
        [SerializeField] CarController _carController;

        // Stage elements (set by SetupStage)
        Transform[] _checkpoints;
        Transform[] _obstacles;
        Transform[] _sandAreas;

        List<Vector3> _points = new List<Vector3>();
        bool _isDrawing;
        bool _drawingEnabled;

        const float MinPointDistance = 0.15f;

        void Update()
        {
            if (!_drawingEnabled) return;
            if (_gameManager.State != FingerRacerState.Drawing) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                StartDrawing();
            }
            else if (mouse.leftButton.isPressed && _isDrawing)
            {
                AddPoint(GetWorldPos(mouse.position.ReadValue()));
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _isDrawing)
            {
                _isDrawing = false;
            }
        }

        void StartDrawing()
        {
            _points.Clear();
            _isDrawing = true;
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
            }
            AddPoint(_startMarker != null ? _startMarker.position : Vector3.zero);
        }

        void AddPoint(Vector3 p)
        {
            p.z = 0f;
            if (_points.Count > 0 && Vector3.Distance(_points[_points.Count - 1], p) < MinPointDistance)
                return;
            _points.Add(p);
            UpdateLineRenderer();
        }

        void UpdateLineRenderer()
        {
            if (_lineRenderer == null) return;
            _lineRenderer.positionCount = _points.Count;
            for (int i = 0; i < _points.Count; i++)
                _lineRenderer.SetPosition(i, _points[i]);
        }

        Vector3 GetWorldPos(Vector2 screenPos)
        {
            var cam = Camera.main;
            if (cam == null) return Vector3.zero;
            Vector3 wp = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
            wp.z = 0f;
            return wp;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _drawingEnabled = true;
            _points.Clear();
            if (_lineRenderer != null) _lineRenderer.positionCount = 0;

            PositionMarkersForStage(stageIndex);
            SpawnCheckpointsForStage(config);
            SpawnObstaclesForStage(config, stageIndex);
            SpawnSandAreasForStage(config, stageIndex);
        }

        void PositionMarkersForStage(int stageIndex)
        {
            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camW = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);

            if (_startMarker != null)
                _startMarker.position = new Vector3(-camW * 0.65f, -camSize + 2.8f, 0f);
            if (_goalMarker != null)
                _goalMarker.position = new Vector3(camW * 0.65f, camSize - 1.8f, 0f);
        }

        void SpawnCheckpointsForStage(StageManager.StageConfig config)
        {
            DestroyGroup(ref _checkpoints);

            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camW = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);
            int count = config.countMultiplier;

            float startY = -camSize + 3.5f;
            float goalY = camSize - 2.5f;
            float yRange = goalY - startY;

            _checkpoints = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / (count + 1f);
                float y = startY + yRange * t;
                // zigzag x
                float x = (i % 2 == 0) ? camW * 0.3f : -camW * 0.3f;
                var go = new GameObject("Checkpoint_" + i);
                go.transform.position = new Vector3(x, y, 0f);
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
                // Visual feedback via SpriteRenderer
                var sr = go.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.2f, 0.9f, 0.4f, 0.7f);
                sr.sortingOrder = 3;
                var cp = go.AddComponent<CheckpointMarker>();
                cp.Index = i;
                cp.GameManager = _gameManager;
                _checkpoints[i] = go.transform;
            }
        }

        void SpawnObstaclesForStage(StageManager.StageConfig config, int stageIndex)
        {
            DestroyGroup(ref _obstacles);
            if (config.complexityFactor < 0.4f) return;

            int count = 3 + stageIndex - 2; // Stage3=3, Stage4=4, Stage5=5
            if (count <= 0) count = 3;

            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camW = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);

            _obstacles = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / (count + 1f);
                float y = -camSize + 3.0f + (camSize * 2f - 5.0f) * t;
                float x = ((i % 3) - 1) * camW * 0.4f;
                var go = new GameObject("Obstacle_" + i);
                go.transform.position = new Vector3(x, y, 0f);
                go.transform.localScale = Vector3.one * 0.6f;
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.4f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.9f, 0.2f, 0.2f, 0.85f);
                sr.sortingOrder = 4;
                _obstacles[i] = go.transform;
            }
        }

        void SpawnSandAreasForStage(StageManager.StageConfig config, int stageIndex)
        {
            DestroyGroup(ref _sandAreas);
            if (config.complexityFactor < 0.6f) return;

            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camW = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);

            _sandAreas = new Transform[2];
            float[] ys = { -camSize + 4.5f, -0.5f };
            float[] xs = { -camW * 0.25f, camW * 0.25f };
            for (int i = 0; i < 2; i++)
            {
                var go = new GameObject("SandArea_" + i);
                go.transform.position = new Vector3(xs[i], ys[i], 0f);
                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.8f, 1.2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.85f, 0.7f, 0.3f, 0.5f);
                sr.sortingOrder = 1;
                var sa = go.AddComponent<SandAreaMarker>();
                sa.CarController = _carController;
                _sandAreas[i] = go.transform;
            }
        }

        void DestroyGroup(ref Transform[] group)
        {
            if (group == null) return;
            foreach (var t in group)
                if (t != null) Destroy(t.gameObject);
            group = null;
        }

        public Vector3[] GetCoursePoints()
        {
            if (_points.Count < 5) return null;
            // Append goal marker as last point
            if (_goalMarker != null)
            {
                var pts = new List<Vector3>(_points);
                pts.Add(_goalMarker.position);
                return pts.ToArray();
            }
            return _points.ToArray();
        }

        public float ComputeSmoothness()
        {
            if (_points.Count < 3) return 1f;
            float totalAngle = 0f;
            for (int i = 1; i < _points.Count - 1; i++)
            {
                Vector3 a = (_points[i] - _points[i - 1]).normalized;
                Vector3 b = (_points[i + 1] - _points[i]).normalized;
                float angle = Vector3.Angle(a, b);
                totalAngle += angle;
            }
            float avgAngle = totalAngle / (_points.Count - 2);
            return Mathf.Clamp01(1f - avgAngle / 90f);
        }

        public Vector3 GoalPosition => _goalMarker != null ? _goalMarker.position : Vector3.zero;

        public void SetDrawingEnabled(bool enabled)
        {
            _drawingEnabled = enabled;
        }

        public void SetCarController(CarController cc) { _carController = cc; }
    }
}
