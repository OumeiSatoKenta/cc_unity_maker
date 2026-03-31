using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game006_ShadowMatch
{
    public class ShadowManager : MonoBehaviour
    {
        [SerializeField] private ShapeController _shape;
        [SerializeField] private SpriteRenderer _targetShadow;
        [SerializeField] private float _matchThreshold = 15f;

        private ShadowMatchGameManager _gameManager;
        private Camera _mainCamera;
        private bool _isDragging;
        private float _lastMouseAngle;
        private float _targetAngle;
        private int _currentStage;

        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<ShadowMatchGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null || _shape == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null && hit.GetComponent<ShapeController>() != null)
                {
                    _isDragging = true;
                    _lastMouseAngle = GetMouseAngle(mouse);
                }
            }

            if (_isDragging && mouse.leftButton.isPressed)
            {
                float currentAngle = GetMouseAngle(mouse);
                float delta = currentAngle - _lastMouseAngle;
                _shape.AddAngle(-delta);
                _lastMouseAngle = currentAngle;

                if (_gameManager != null) _gameManager.OnShapeRotated();
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
            }
        }

        private float GetMouseAngle(Mouse mouse)
        {
            Vector3 sp = mouse.position.ReadValue();
            sp.z = -_mainCamera.transform.position.z;
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
            Vector2 shapePos = _shape.transform.position;
            Vector2 dir = worldPos - shapePos;
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        public void SetupStage(int stageIndex)
        {
            _currentStage = stageIndex;
            var data = GetStageData(stageIndex);
            _targetAngle = data.targetAngle;

            // Set shape sprite
            if (_shape != null)
            {
                var sr = _shape.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    var sprite = Resources.Load<Sprite>(data.shapeSpritePath);
                    if (sprite != null) sr.sprite = sprite;
                }
                _shape.SetAngle(data.startAngle);
            }

            // Set target shadow sprite and rotation
            if (_targetShadow != null)
            {
                var sprite = Resources.Load<Sprite>(data.shadowSpritePath);
                if (sprite != null) _targetShadow.sprite = sprite;
                _targetShadow.transform.rotation = Quaternion.Euler(0, 0, -_targetAngle);
            }
        }

        public bool IsMatched()
        {
            if (_shape == null) return false;
            float diff = Mathf.Abs(Mathf.DeltaAngle(_shape.CurrentAngle, _targetAngle));
            return diff <= _matchThreshold;
        }

        #region Stage Data

        private struct StageData
        {
            public string shapeSpritePath;
            public string shadowSpritePath;
            public float targetAngle;
            public float startAngle;
        }

        private StageData GetStageData(int index)
        {
            switch (index % StageCount)
            {
                case 0: return new StageData {
                    shapeSpritePath = "Sprites/Game006_ShadowMatch/shape_triangle",
                    shadowSpritePath = "Sprites/Game006_ShadowMatch/shadow_triangle",
                    targetAngle = 90f, startAngle = 0f
                };
                case 1: return new StageData {
                    shapeSpritePath = "Sprites/Game006_ShadowMatch/shape_arrow",
                    shadowSpritePath = "Sprites/Game006_ShadowMatch/shadow_arrow",
                    targetAngle = 180f, startAngle = 45f
                };
                case 2: return new StageData {
                    shapeSpritePath = "Sprites/Game006_ShadowMatch/shape_lblock",
                    shadowSpritePath = "Sprites/Game006_ShadowMatch/shadow_lblock",
                    targetAngle = 270f, startAngle = 30f
                };
                default: return GetStageData(0);
            }
        }

        #endregion
    }
}
