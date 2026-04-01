using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game039_BoomerangHero
{
    public class BoomerangManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private BoomerangHeroGameManager _gameManager;

        [SerializeField, Tooltip("ブーメランスプライト")]
        private Sprite _boomerangSprite;

        [SerializeField, Tooltip("敵スプライト")]
        private Sprite _enemySprite;

        private Camera _mainCamera;
        private List<Enemy> _enemies = new List<Enemy>();
        private Transform _boomerang;
        private bool _isFlying;
        private Vector2 _heroPos = new Vector2(0f, -3.5f);

        private static readonly Vector2[] EnemyPositions = {
            new Vector2(-3f, 3f), new Vector2(-1f, 3.5f), new Vector2(1.5f, 3f), new Vector2(3f, 2.5f),
            new Vector2(-2.5f, 1f), new Vector2(0.5f, 1.5f), new Vector2(2.5f, 0.5f), new Vector2(-1f, 0f),
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartStage()
        {
            SpawnEnemies();
            SpawnBoomerang();
        }

        private void SpawnEnemies()
        {
            foreach (var e in _enemies) if (e != null) Destroy(e.gameObject);
            _enemies.Clear();
            for (int i = 0; i < EnemyPositions.Length; i++)
            {
                var obj = new GameObject($"Enemy_{i}");
                obj.transform.position = new Vector3(EnemyPositions[i].x, EnemyPositions[i].y, 0f);
                obj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                obj.transform.SetParent(transform);
                obj.AddComponent<SpriteRenderer>().sortingOrder = 2;
                obj.AddComponent<CircleCollider2D>().radius = 0.5f;
                var enemy = obj.AddComponent<Enemy>();
                enemy.Initialize(_enemySprite, OnEnemyKilled);
                _enemies.Add(enemy);
            }
        }

        private void SpawnBoomerang()
        {
            if (_boomerang != null) Destroy(_boomerang.gameObject);
            var obj = new GameObject("Boomerang");
            obj.transform.position = new Vector3(_heroPos.x, _heroPos.y, 0f);
            obj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _boomerangSprite; sr.sortingOrder = 5;
            _boomerang = obj.transform;
            _isFlying = false;
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying || _isFlying) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector3 wp = _mainCamera.ScreenToWorldPoint(mp);
                Vector2 dir = ((Vector2)wp - _heroPos).normalized;
                _gameManager.OnThrowUsed();
                StartCoroutine(FlyBoomerang(dir));
            }
        }

        private IEnumerator FlyBoomerang(Vector2 direction)
        {
            _isFlying = true;
            float time = 0f;
            float duration = 1.5f;
            float speed = 8f;
            float curveStrength = 4f;
            Vector2 startPos = _heroPos;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                // 弧を描く軌道
                Vector2 forward = direction * speed * t;
                Vector2 curve = perpendicular * Mathf.Sin(t * Mathf.PI) * curveStrength;
                Vector2 returnPull = -direction * speed * t * t * 0.5f;
                Vector2 pos = startPos + forward + curve + returnPull;
                if (_boomerang != null)
                {
                    _boomerang.position = new Vector3(pos.x, pos.y, 0f);
                    _boomerang.Rotate(0f, 0f, 720f * Time.deltaTime);
                }

                // 衝突判定
                var hits = Physics2D.OverlapCircleAll(pos, 0.35f);
                foreach (var hit in hits)
                {
                    var enemy = hit.GetComponent<Enemy>();
                    if (enemy != null) enemy.Hit();
                }

                yield return null;
            }

            _isFlying = false;
            _gameManager.OnBoomerangReturned();
            if (_gameManager.IsPlaying) SpawnBoomerang();
        }

        private void OnEnemyKilled(Enemy e)
        {
            _enemies.Remove(e);
            _gameManager.AddKill();
        }
    }
}
