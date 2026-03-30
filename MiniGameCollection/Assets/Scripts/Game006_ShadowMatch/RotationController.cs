using UnityEngine;
using UnityEngine.InputSystem;

namespace Game006_ShadowMatch
{
    /// <summary>
    /// 画面ドラッグで object と shadow スプライトを同期回転させ、
    /// ターゲット角度との一致率を計算する。入力処理を一元管理。
    /// </summary>
    public class RotationController : MonoBehaviour
    {
        [SerializeField] private ShadowMatchGameManager _gameManager;
        [SerializeField] private ShadowMatchUI _ui;
        [SerializeField] private Transform _objectTransform;
        [SerializeField] private Transform _shadowTransform;

        private float _currentAngle;
        private float _targetAngle;
        private bool _isDragging;
        private Vector2 _lastPos;
        private bool _solved;

        private const float RotSpeed = 0.4f;
        private const float MatchThreshold = 0.88f;
        private const float MatchRange = 25f; // ±25° = 0%, 0° = 100%

        public void SetLevel(float targetAngle)
        {
            _targetAngle = targetAngle;
            _currentAngle = 0f;
            _solved = false;
            ApplyRotation();
            _ui?.UpdateMatch(0f);
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsPlaying || _solved) return;

            var mouse = Mouse.current;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _lastPos = mouse.position.ReadValue();
            }
            if (mouse.leftButton.wasReleasedThisFrame)
                _isDragging = false;

            if (!_isDragging) return;

            Vector2 pos = mouse.position.ReadValue();
            float dx = pos.x - _lastPos.x;
            _currentAngle += dx * RotSpeed;
            _lastPos = pos;
            ApplyRotation();

            float match = CalcMatch();
            _ui?.UpdateMatch(match);

            if (match >= MatchThreshold)
            {
                _solved = true;
                _gameManager.OnSolved();
            }
        }

        private void ApplyRotation()
        {
            var rot = Quaternion.Euler(0f, 0f, _currentAngle);
            if (_objectTransform) _objectTransform.rotation = rot;
            if (_shadowTransform) _shadowTransform.rotation = rot;
        }

        private float CalcMatch()
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _targetAngle));
            return Mathf.Clamp01(1f - diff / MatchRange);
        }
    }
}
