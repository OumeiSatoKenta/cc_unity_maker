using UnityEngine;
using System.Collections.Generic;

namespace Game025v2_TowerDefend
{
    public enum EnemyType { Normal, Fast, Flying, Breaker }

    public class Enemy : MonoBehaviour
    {
        public EnemyType Type { get; private set; }
        public float TravelDistance { get; private set; }
        public float DirectDistance { get; private set; }

        WaveManager _waveManager;
        TowerDefendGameManager _gameManager;
        WallManager _wallManager;
        List<Vector3> _path = new();
        int _pathIndex;
        float _speed;
        bool _isActive;
        SpriteRenderer _sr;
        Camera _mainCam;
        Vector3 _goalPosition;

        // For breaker
        float _wallBreakCooldown;
        const float WallBreakInterval = 0.5f;

        // For visual feedback
        float _hitFlashTimer;

        public void Initialize(EnemyType type, List<Vector3> path, float speed,
            TowerDefendGameManager gm, WaveManager wm, WallManager wallManager, Vector3 goalPos)
        {
            Type = type;
            _path = new List<Vector3>(path);
            _pathIndex = 0;
            _speed = speed;
            _gameManager = gm;
            _waveManager = wm;
            _wallManager = wallManager;
            _mainCam = Camera.main;
            _sr = GetComponent<SpriteRenderer>();
            _isActive = true;

            _goalPosition = goalPos;
            DirectDistance = path.Count > 0
                ? Vector3.Distance(path[0], goalPos)
                : Vector3.Distance(transform.position, goalPos);
            TravelDistance = 0f;

            if (transform.position != path[0])
                transform.position = path[0];
        }

        public void SetPath(List<Vector3> newPath)
        {
            _path = new List<Vector3>(newPath);
            _pathIndex = 0;
        }

        public void StopMoving()
        {
            _isActive = false;
        }

        // Called when enemy path only has goal remaining but is actually blocked
        public void OnBlocked()
        {
            Defeat();
        }

        void Update()
        {
            if (!_isActive) return;

            if (_hitFlashTimer > 0f)
            {
                _hitFlashTimer -= Time.deltaTime;
                if (_hitFlashTimer <= 0f && _sr != null)
                    _sr.color = Color.white;
            }

            if (_wallBreakCooldown > 0f)
                _wallBreakCooldown -= Time.deltaTime;

            if (_pathIndex >= _path.Count)
            {
                ReachGoal();
                return;
            }

            Vector3 target = _path[_pathIndex];
            Vector3 prev = transform.position;
            float step = _speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            TravelDistance += Vector3.Distance(prev, transform.position);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                _pathIndex++;
                if (_pathIndex >= _path.Count)
                {
                    ReachGoal();
                    return;
                }

                // Recalculate path when not flying (walls may have changed)
                if (Type != EnemyType.Flying && _wallManager != null)
                {
                    var newPath = _wallManager.CalculatePath(transform.position, _goalPosition);
                    if (newPath != null && newPath.Count > 0)
                    {
                        _path = newPath;
                        _pathIndex = 0;
                    }
                }
            }

            // Breaker: check for wall collision
            if (Type == EnemyType.Breaker && _wallBreakCooldown <= 0f && _wallManager != null)
            {
                if (_wallManager.TryBreakWallAt(transform.position))
                {
                    _wallBreakCooldown = WallBreakInterval;
                    FlashColor(new Color(1f, 0.5f, 0f));
                }
            }
        }

        void ReachGoal()
        {
            _isActive = false;
            _gameManager?.OnEnemyReachedGoal(this);
            _waveManager?.OnEnemyRemoved(this);
            Destroy(gameObject);
        }

        public void Defeat()
        {
            _isActive = false;
            // Pop animation then destroy
            StartCoroutine(PopAndDestroy());
        }

        System.Collections.IEnumerator PopAndDestroy()
        {
            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.3f;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.4f, ratio * 2f)
                    : Mathf.Lerp(1.4f, 0f, (ratio - 0.5f) * 2f);
                transform.localScale = startScale * scale;
                yield return null;
            }
            _gameManager?.OnEnemyDefeated(this);
            _waveManager?.OnEnemyRemoved(this);
            Destroy(gameObject);
        }

        void FlashColor(Color c)
        {
            if (_sr != null)
            {
                _sr.color = c;
                _hitFlashTimer = 0.15f;
            }
        }
    }
}
