using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game030_FingerRacer
{
    public class RaceManager : MonoBehaviour
    {
        [SerializeField] private GameObject _carPrefab;
        [SerializeField] private GameObject _checkpointPrefab;
        [SerializeField] private float _carSpeed = 5f;
        [SerializeField] private float _raceTime = 15f;

        private GameObject _carObj;
        private readonly List<Vector2> _path = new List<Vector2>();
        private readonly List<GameObject> _checkpoints = new List<GameObject>();
        private readonly List<GameObject> _trailObjects = new List<GameObject>();
        private int _pathIndex;
        private int _checkpointsHit;
        private int _totalCheckpoints;
        private float _timeRemaining;
        private bool _isDrawing;
        private bool _isRacing;
        private bool _isRunning;

        private FingerRacerGameManager _gameManager;
        private Camera _mainCamera;

        private void Awake()
        {
            _gameManager = GetComponentInParent<FingerRacerGameManager>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isRunning) return;

            if (!_isRacing)
                HandleDrawing();
            else
                UpdateRace();
        }

        private void HandleDrawing()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDrawing = true;
                _path.Clear();
                ClearTrail();
            }

            if (_isDrawing && mouse.leftButton.isPressed)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);

                if (_path.Count == 0 || Vector2.Distance(worldPos, _path[_path.Count - 1]) > 0.2f)
                    _path.Add(worldPos);
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDrawing)
            {
                _isDrawing = false;
                if (_path.Count > 5)
                    StartRace();
            }
        }

        private void StartRace()
        {
            _isRacing = true;
            _pathIndex = 0;
            _timeRemaining = _raceTime;

            if (_carPrefab != null && _path.Count > 0)
            {
                _carObj = Instantiate(_carPrefab, transform);
                _carObj.transform.position = _path[0];
            }
        }

        private void UpdateRace()
        {
            _timeRemaining -= Time.deltaTime;
            if (_gameManager != null) _gameManager.OnTimeUpdate(_timeRemaining);

            if (_timeRemaining <= 0f)
            {
                _isRunning = false;
                if (_gameManager != null) _gameManager.OnRaceEnd(_checkpointsHit);
                return;
            }

            if (_carObj == null || _pathIndex >= _path.Count) return;

            Vector2 target = _path[_pathIndex];
            Vector2 current = _carObj.transform.position;
            Vector2 newPos = Vector2.MoveTowards(current, target, _carSpeed * Time.deltaTime);
            _carObj.transform.position = newPos;

            // Rotation
            Vector2 dir = (target - current).normalized;
            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                _carObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (Vector2.Distance(newPos, target) < 0.1f)
                _pathIndex++;

            // Check checkpoints
            for (int i = _checkpoints.Count - 1; i >= 0; i--)
            {
                if (_checkpoints[i] == null) continue;
                if (Vector2.Distance(newPos, _checkpoints[i].transform.position) < 0.5f)
                {
                    Destroy(_checkpoints[i]);
                    _checkpoints.RemoveAt(i);
                    _checkpointsHit++;
                    if (_gameManager != null) _gameManager.OnCheckpointHit(_checkpointsHit, _totalCheckpoints);
                }
            }

            if (_pathIndex >= _path.Count)
            {
                _isRunning = false;
                if (_gameManager != null) _gameManager.OnRaceEnd(_checkpointsHit);
            }
        }

        public void StartGame()
        {
            ClearAll();
            _isRacing = false;
            _isDrawing = false;
            _checkpointsHit = 0;
            _isRunning = true;

            // Spawn checkpoints randomly
            _totalCheckpoints = 5;
            for (int i = 0; i < _totalCheckpoints; i++)
            {
                if (_checkpointPrefab == null) continue;
                var obj = Instantiate(_checkpointPrefab, transform);
                obj.transform.position = new Vector3(Random.Range(-4f, 4f), Random.Range(-3f, 3f), 0);
                _checkpoints.Add(obj);
            }
        }

        public void StopGame() { _isRunning = false; }

        private void ClearTrail()
        {
            foreach (var t in _trailObjects) if (t != null) Destroy(t);
            _trailObjects.Clear();
        }

        private void ClearAll()
        {
            ClearTrail();
            foreach (var c in _checkpoints) if (c != null) Destroy(c);
            _checkpoints.Clear();
            _path.Clear();
            if (_carObj != null) { Destroy(_carObj); _carObj = null; }
        }
    }
}
