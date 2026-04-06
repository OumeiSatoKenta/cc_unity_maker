using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game048v2_GlassBall
{
    /// <summary>
    /// Handles rail drawing with LineRenderer and ball launch logic.
    /// Rail is purely visual; ball uses physics with AddForce toward next waypoints.
    /// </summary>
    public class RailManager : MonoBehaviour
    {
        [SerializeField] GlassBallGameManager _gameManager;
        [SerializeField] GlassBallController _ballController;
        [SerializeField] GlassBallUI _ui;
        [SerializeField] LineRenderer _lineRenderer;

        public const float MaxInkLength = 25f;

        private List<Vector3> _points = new List<Vector3>();
        private float _inkUsed;
        private bool _isDrawing;
        private bool _canDraw = true;
        private float _lastClickTime;
        private const float DoubleClickInterval = 0.3f;

        // Stage config
        private float _speedMultiplier = 1f;

        void Update()
        {
            if (!_canDraw) return;
            if (_gameManager.State != GameState.Drawing) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                // Double-click detection
                if (Time.time - _lastClickTime < DoubleClickInterval)
                {
                    ClearRail();
                    _lastClickTime = 0f;
                    return;
                }
                _lastClickTime = Time.time;

                _isDrawing = true;
                _points.Clear();
                Vector3 worldPos = GetWorldPos(mouse.position.ReadValue());
                _points.Add(worldPos);
                RefreshLineRenderer();
            }
            else if (mouse.leftButton.isPressed && _isDrawing)
            {
                Vector3 worldPos = GetWorldPos(mouse.position.ReadValue());
                if (_points.Count == 0 || Vector3.Distance(_points[_points.Count - 1], worldPos) > 0.1f)
                {
                    // Check ink
                    float addLen = _points.Count > 0 ? Vector3.Distance(_points[_points.Count - 1], worldPos) : 0f;
                    if (_inkUsed + addLen <= MaxInkLength)
                    {
                        _inkUsed += addLen;
                        _points.Add(worldPos);
                        RefreshLineRenderer();
                        if (_ui != null) _ui.UpdateInk(1f - _inkUsed / MaxInkLength);
                    }
                    else
                    {
                        _isDrawing = false;
                    }
                }
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDrawing = false;
            }
        }

        Vector3 GetWorldPos(Vector2 screenPos)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane + 10f));
            pos.z = 0f;
            return pos;
        }

        void RefreshLineRenderer()
        {
            if (_lineRenderer == null) return;
            _lineRenderer.positionCount = _points.Count;
            _lineRenderer.SetPositions(_points.ToArray());
        }

        public void ClearRail()
        {
            if (_gameManager.State == GameState.GameOver ||
                _gameManager.State == GameState.AllClear) return;

            _points.Clear();
            _inkUsed = 0f;
            RefreshLineRenderer();
            if (_ui != null) _ui.UpdateInk(1f);
            _ballController.ResetBall();
        }

        public void LaunchBall()
        {
            if (_gameManager.State != GameState.Drawing) return;
            if (_points.Count < 2) return;

            _canDraw = false;
            _ballController.Launch(_points, _speedMultiplier);
            _gameManager.OnBallLaunched();
            _ui.SetLaunchButtonInteractable(false);
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _speedMultiplier = config.speedMultiplier;
            _canDraw = true;
            ClearRail();
            _ui.SetLaunchButtonInteractable(true);
        }

        public float GetInkPercent()
        {
            return Mathf.Clamp01(1f - _inkUsed / MaxInkLength) * 100f;
        }

        public void EnableDrawing()
        {
            _canDraw = true;
            _ui.SetLaunchButtonInteractable(true);
        }
    }
}
