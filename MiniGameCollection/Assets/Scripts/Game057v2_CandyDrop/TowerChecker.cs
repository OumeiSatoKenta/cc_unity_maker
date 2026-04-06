using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game057v2_CandyDrop
{
    public class TowerChecker : MonoBehaviour
    {
        [SerializeField] CandyDropGameManager _gameManager;
        [SerializeField] Transform _groundTransform;
        [SerializeField] Transform _goalLineTransform;

        float _goalHeight;
        float _groundY;
        bool _isActive;
        bool _hasReachedGoal;
        float _complexityFactor;
        bool _oscillateGround;
        Coroutine _oscillateRoutine;

        // For collapse detection
        float _peakHeight;
        float _collapseThreshold = 1.5f;
        float _checkInterval = 0.5f;
        float _lastCheckTime;

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _complexityFactor = config.complexityFactor;
            _isActive = true;
            _hasReachedGoal = false;
            _peakHeight = 0f;

            if (Camera.main == null) return;
            float camSize = Camera.main.orthographicSize;
            _groundY = -camSize + 2.0f;

            // Goal heights per stage
            float[] goalHeights = { 4.0f, 5.0f, 6.0f, 6.5f, 7.0f };
            int idx = Mathf.Clamp(stageNumber - 1, 0, goalHeights.Length - 1);
            _goalHeight = goalHeights[idx];

            // Stop previous oscillation
            if (_oscillateRoutine != null)
            {
                StopCoroutine(_oscillateRoutine);
                _oscillateRoutine = null;
                if (_groundTransform != null)
                {
                    Vector3 pos = _groundTransform.position;
                    _groundTransform.position = new Vector3(0, pos.y, pos.z);
                }
            }

            // Update GoalLine position
            if (_goalLineTransform != null)
                _goalLineTransform.position = new Vector3(0, _groundY + _goalHeight, 0);

            // Stage 3+ oscillate ground
            _oscillateGround = _complexityFactor >= 0.6f && _groundTransform != null;
            if (_oscillateGround)
                _oscillateRoutine = StartCoroutine(OscillateGround());
        }

        public float GoalHeight => _goalHeight;
        public float GroundY => _groundY;

        IEnumerator OscillateGround()
        {
            float amplitude = 0.3f;
            float period = 2.0f;
            float elapsed = 0f;
            while (true)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Sin(elapsed / period * Mathf.PI * 2f) * amplitude;
                if (_groundTransform != null)
                {
                    Vector3 pos = _groundTransform.position;
                    _groundTransform.position = new Vector3(x, pos.y, pos.z);
                }
                yield return null;
            }
        }

        void Update()
        {
            if (!_isActive) return;
            if (!_gameManager.IsPlaying()) return;
            if (_hasReachedGoal) return;

            if (Time.time - _lastCheckTime < _checkInterval) return;
            _lastCheckTime = Time.time;

            float currentHeight = GetTowerHeight();
            float heightAboveGround = currentHeight - _groundY;

            // Update peak
            if (heightAboveGround > _peakHeight)
                _peakHeight = heightAboveGround;

            // Notify height change
            _gameManager.OnHeightChanged(heightAboveGround / _goalHeight);

            // Goal check
            if (heightAboveGround >= _goalHeight)
            {
                _hasReachedGoal = true;
                _isActive = false;
                if (_oscillateRoutine != null)
                {
                    StopCoroutine(_oscillateRoutine);
                    _oscillateRoutine = null;
                }
                _gameManager.OnGoalReached(heightAboveGround / _goalHeight);
                return;
            }

            // Collapse check: if height dropped significantly from peak
            if (_peakHeight > 1.5f && (heightAboveGround < _peakHeight - _collapseThreshold))
            {
                _isActive = false;
                if (_oscillateRoutine != null)
                {
                    StopCoroutine(_oscillateRoutine);
                    _oscillateRoutine = null;
                }
                _gameManager.OnTowerCollapsed();
            }
        }

        float GetTowerHeight()
        {
            var candies = FindObjectsByType<CandyController>(FindObjectsSortMode.None);
            float maxY = _groundY;
            foreach (var c in candies)
            {
                if (!c.HasLanded) continue;
                if (c.transform.position.y > maxY)
                    maxY = c.transform.position.y;
            }
            return maxY;
        }
    }
}
