using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game030_FingerRacer
{
    public class RaceManager : MonoBehaviour
    {
        [SerializeField, Tooltip("車のTransform")]
        private Transform _carTransform;

        [SerializeField, Tooltip("ゴールマーカーのTransform")]
        private Transform _finishLineTransform;

        [SerializeField, Tooltip("基本走行速度 (units/s)")]
        private float _baseSpeed = 3f;

        [SerializeField, Tooltip("最小パス長 (units)")]
        private float _minPathLength = 5f;

        private FingerRacerGameManager _gameManager;
        private Camera _mainCamera;
        private LineRenderer _lineRenderer;
        private List<Vector3> _pathPoints = new List<Vector3>();
        private float _totalPathLength;

        private bool _isDrawing;
        private bool _isRacing;
        private float _progress; // 0〜1

        private void Awake()
        {
            _gameManager = GetComponentInParent<FingerRacerGameManager>();
            _mainCamera = Camera.main;

            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
                _lineRenderer = gameObject.AddComponent<LineRenderer>();

            _lineRenderer.startWidth = 0.2f;
            _lineRenderer.endWidth = 0.2f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            _lineRenderer.endColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            _lineRenderer.sortingOrder = 5;
        }

        public void StartDrawing()
        {
            _isDrawing = true;
            _isRacing = false;
            _pathPoints.Clear();
            _totalPathLength = 0f;
            _progress = 0f;
            _lineRenderer.positionCount = 0;

            if (_carTransform != null)
                _carTransform.gameObject.SetActive(false);

            if (_finishLineTransform != null)
                _finishLineTransform.gameObject.SetActive(false);
        }

        public void StopGame()
        {
            _isDrawing = false;
            _isRacing = false;
        }

        private void Update()
        {
            if (_isDrawing)
            {
                HandleDrawInput();
            }
            else if (_isRacing)
            {
                UpdateCarMovement();
            }
        }

        private void HandleDrawInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.isPressed)
            {
                Vector3 screenPos = mouse.position.ReadValue();
                screenPos.z = -_mainCamera.transform.position.z;
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
                worldPos.z = 0f;

                if (_pathPoints.Count == 0)
                {
                    _pathPoints.Add(worldPos);
                    UpdateLineRenderer();
                }
                else
                {
                    float dist = Vector3.Distance(_pathPoints[_pathPoints.Count - 1], worldPos);
                    if (dist >= 0.1f)
                    {
                        _totalPathLength += dist;
                        _pathPoints.Add(worldPos);
                        UpdateLineRenderer();
                    }
                }
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                CheckPathAndStartRace();
            }
        }

        private void CheckPathAndStartRace()
        {
            if (_totalPathLength < _minPathLength)
            {
                // パスが短すぎるのでリセット
                _pathPoints.Clear();
                _totalPathLength = 0f;
                _lineRenderer.positionCount = 0;
                return;
            }

            StartRacing();
        }

        private void StartRacing()
        {
            _isDrawing = false;
            _isRacing = true;
            _progress = 0f;

            // 車をパスの始点に移動
            if (_carTransform != null && _pathPoints.Count > 0)
            {
                _carTransform.position = _pathPoints[0];
                _carTransform.gameObject.SetActive(true);
            }

            // ゴールをパスの終点に配置
            if (_finishLineTransform != null && _pathPoints.Count > 0)
            {
                _finishLineTransform.position = _pathPoints[_pathPoints.Count - 1];
                _finishLineTransform.gameObject.SetActive(true);
            }

            if (_gameManager != null)
                _gameManager.OnRacingStarted();
        }

        private void UpdateCarMovement()
        {
            if (_carTransform == null || _pathPoints.Count < 2) return;

            float speed = CalculateCurrentSpeed();
            float distToMove = speed * Time.deltaTime;

            // 現在のパス上の位置をdistToMove分進める
            float totalLength = _totalPathLength;
            float coveredLength = _progress * totalLength;
            coveredLength += distToMove;
            _progress = Mathf.Clamp01(coveredLength / totalLength);

            // パス上の実際のワールド座標を計算
            Vector3 newPos = GetPositionOnPath(_progress);
            Vector3 dir = (newPos - _carTransform.position).normalized;
            _carTransform.position = newPos;

            // 車の向きを進行方向に合わせる
            if (dir != Vector3.zero)
                _carTransform.up = dir;

            if (_progress >= 1f)
            {
                _isRacing = false;
                _gameManager?.OnRaceComplete();
            }
        }

        private float CalculateCurrentSpeed()
        {
            if (_pathPoints.Count < 3) return _baseSpeed;
            int idx = Mathf.RoundToInt(_progress * (_pathPoints.Count - 1));
            idx = Mathf.Clamp(idx, 1, _pathPoints.Count - 2);

            Vector2 dir1 = (_pathPoints[idx] - _pathPoints[idx - 1]).normalized;
            Vector2 dir2 = (_pathPoints[idx + 1] - _pathPoints[idx]).normalized;
            float angle = Vector2.Angle(dir1, dir2);
            float curveFactor = 1f - Mathf.Clamp01(angle / 180f) * 0.5f;

            return _baseSpeed * curveFactor;
        }

        private Vector3 GetPositionOnPath(float t)
        {
            if (_pathPoints.Count == 0) return Vector3.zero;
            if (_pathPoints.Count == 1) return _pathPoints[0];

            float targetLength = t * _totalPathLength;
            float accumulated = 0f;

            for (int i = 1; i < _pathPoints.Count; i++)
            {
                float segLen = Vector3.Distance(_pathPoints[i - 1], _pathPoints[i]);
                if (accumulated + segLen >= targetLength)
                {
                    float segT = (targetLength - accumulated) / segLen;
                    return Vector3.Lerp(_pathPoints[i - 1], _pathPoints[i], segT);
                }
                accumulated += segLen;
            }

            return _pathPoints[_pathPoints.Count - 1];
        }

        private void UpdateLineRenderer()
        {
            _lineRenderer.positionCount = _pathPoints.Count;
            _lineRenderer.SetPositions(_pathPoints.ToArray());
        }
    }
}
