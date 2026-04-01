using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game033_AimSniper
{
    public class SniperManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private AimSniperGameManager _gameManager;

        [SerializeField, Tooltip("スコープのTransform")]
        private Transform _scopeTransform;

        [SerializeField, Tooltip("十字線スプライト")]
        private Sprite _crosshairSprite;

        [SerializeField, Tooltip("ターゲットスプライト")]
        private Sprite _targetSprite;

        private Camera _mainCamera;
        private List<Target> _targets = new List<Target>();
        private float _perlinSeedX;
        private float _perlinSeedY;

        private const float SwayAmplitude = 0.15f;
        private const float SwayFrequency = 1.5f;
        private const float HitRadius = 0.4f;

        private static readonly (Vector2 pos, float speed, float minX, float maxX)[] TargetData = {
            (new Vector2(0f, 3.5f),   0.8f, -3f, 3f),
            (new Vector2(-2f, 2.0f),  1.2f, -3.5f, 0f),
            (new Vector2(2f, 2.0f),   1.0f, 0f, 3.5f),
            (new Vector2(0f, 0.5f),   1.5f, -3f, 3f),
            (new Vector2(-1f, -1.0f), 0.6f, -3f, 2f),
        };

        private void Awake()
        {
            _mainCamera = Camera.main;
            _perlinSeedX = Random.Range(0f, 100f);
            _perlinSeedY = Random.Range(0f, 100f);
        }

        public void StartStage()
        {
            SpawnTargets();
        }

        private void SpawnTargets()
        {
            foreach (var t in _targets)
                if (t != null) Destroy(t.gameObject);
            _targets.Clear();

            for (int i = 0; i < TargetData.Length; i++)
            {
                var data = TargetData[i];
                var obj = new GameObject($"Target_{i}");
                obj.transform.position = new Vector3(data.pos.x, data.pos.y, 0f);
                obj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                obj.transform.SetParent(transform);

                obj.AddComponent<SpriteRenderer>();
                obj.AddComponent<CircleCollider2D>();
                var target = obj.AddComponent<Target>();
                target.Initialize(_targetSprite, data.speed, data.minX, data.maxX, OnTargetKilled);
                _targets.Add(target);
            }
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying) return;
            UpdateScope();
            CheckShoot();
        }

        private void UpdateScope()
        {
            if (_scopeTransform == null || Mouse.current == null) return;

            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = -_mainCamera.transform.position.z;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

            // Perlin noise で揺れを追加
            float swayX = (Mathf.PerlinNoise(Time.time * SwayFrequency, _perlinSeedX) - 0.5f) * 2f * SwayAmplitude;
            float swayY = (Mathf.PerlinNoise(_perlinSeedY, Time.time * SwayFrequency) - 0.5f) * 2f * SwayAmplitude;

            _scopeTransform.position = new Vector3(worldPos.x + swayX, worldPos.y + swayY, 0f);
        }

        private void CheckShoot()
        {
            if (_scopeTransform == null || Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (_gameManager.RemainingBullets <= 0) return;

            // OnShot を先に呼び弾数を消費してから命中判定
            // （最後の弾で最後の敵を撃った場合、AddHit→Clear→_isPlaying=false で
            //   後続の OnShot がスキップされるのを防ぐ）
            _gameManager.OnShot();

            Vector2 scopeCenter = _scopeTransform.position;
            var colliders = Physics2D.OverlapCircleAll(scopeCenter, HitRadius);
            foreach (var col in colliders)
            {
                var target = col.GetComponent<Target>();
                if (target != null)
                {
                    target.Hit();
                    break; // 1発で1体のみ
                }
            }
        }

        private void OnTargetKilled(Target target)
        {
            _targets.Remove(target);
            _gameManager.AddHit();
        }
    }
}
