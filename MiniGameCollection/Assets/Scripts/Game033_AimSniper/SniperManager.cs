using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game033_AimSniper
{
    public class SniperManager : MonoBehaviour
    {
        [SerializeField] private GameObject _crosshairPrefab;
        [SerializeField] private GameObject _targetPrefab;
        [SerializeField] private float _swayAmount = 1.5f;
        [SerializeField] private float _swaySpeed = 2f;
        [SerializeField] private float _gameTime = 20f;

        private GameObject _crosshairObj;
        private readonly List<GameObject> _targets = new List<GameObject>();
        private float _swayPhaseX, _swayPhaseY;
        private float _timeRemaining;
        private bool _isRunning;
        private Vector2 _baseAimPos;

        private AimSniperGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<AimSniperGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f) { _isRunning = false; if (_gameManager != null) _gameManager.OnTimeUp(); return; }
            if (_gameManager != null) _gameManager.OnTimeUpdate(_timeRemaining);

            UpdateAim();
            HandleInput();
            MoveTargets();
        }

        private void UpdateAim()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null || _crosshairObj == null) return;

            Vector3 sp = mouse.position.ReadValue();
            sp.z = -_mainCamera.transform.position.z;
            _baseAimPos = _mainCamera.ScreenToWorldPoint(sp);

            _swayPhaseX += _swaySpeed * Time.deltaTime * 1.3f;
            _swayPhaseY += _swaySpeed * Time.deltaTime;

            float swayX = Mathf.Sin(_swayPhaseX) * _swayAmount;
            float swayY = Mathf.Cos(_swayPhaseY) * _swayAmount * 0.7f;

            Vector2 aimPos = _baseAimPos + new Vector2(swayX, swayY);
            _crosshairObj.transform.position = new Vector3(aimPos.x, aimPos.y, 0);
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 shotPos = _crosshairObj != null ? (Vector2)_crosshairObj.transform.position : _baseAimPos;
                bool hit = false;

                for (int i = _targets.Count - 1; i >= 0; i--)
                {
                    if (_targets[i] == null) { _targets.RemoveAt(i); continue; }
                    if (Vector2.Distance(shotPos, _targets[i].transform.position) < 0.6f)
                    {
                        Destroy(_targets[i]); _targets.RemoveAt(i);
                        hit = true;
                        if (_gameManager != null) _gameManager.OnTargetHit();
                        SpawnTarget();
                        break;
                    }
                }

                if (!hit && _gameManager != null) _gameManager.OnMiss();
            }
        }

        private void MoveTargets()
        {
            float t = Time.time;
            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i] == null) continue;
                var pos = _targets[i].transform.position;
                float speed = 1f + i * 0.5f;
                pos.x += Mathf.Sin(t * speed + i * 2f) * 2f * Time.deltaTime;
                pos.y += Mathf.Cos(t * speed * 0.7f + i * 3f) * 1.5f * Time.deltaTime;
                pos.x = Mathf.Clamp(pos.x, -5f, 5f);
                pos.y = Mathf.Clamp(pos.y, -3.5f, 3.5f);
                _targets[i].transform.position = pos;
            }
        }

        private void SpawnTarget()
        {
            if (_targetPrefab == null) return;
            var obj = Instantiate(_targetPrefab, transform);
            obj.transform.position = new Vector3(Random.Range(-4f, 4f), Random.Range(-3f, 3f), 0);
            _targets.Add(obj);
        }

        public void StartGame()
        {
            ClearAll();
            _timeRemaining = _gameTime;
            _swayPhaseX = Random.Range(0f, 6.28f);
            _swayPhaseY = Random.Range(0f, 6.28f);
            _isRunning = true;

            if (_crosshairPrefab != null)
            {
                _crosshairObj = Instantiate(_crosshairPrefab, transform);
                _crosshairObj.transform.position = Vector3.zero;
            }

            for (int i = 0; i < 3; i++) SpawnTarget();
        }

        public void StopGame() { _isRunning = false; }

        private void ClearAll()
        {
            foreach (var t in _targets) if (t != null) Destroy(t);
            _targets.Clear();
            if (_crosshairObj != null) { Destroy(_crosshairObj); _crosshairObj = null; }
        }
    }
}
