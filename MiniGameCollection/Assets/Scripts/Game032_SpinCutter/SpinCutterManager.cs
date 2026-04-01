using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game032_SpinCutter
{
    public class SpinCutterManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private SpinCutterGameManager _gameManager;

        [SerializeField, Tooltip("刃の回転中心")]
        private Transform _pivot;

        [SerializeField, Tooltip("半径スライダー")]
        private Slider _radiusSlider;

        [SerializeField, Tooltip("速度スライダー")]
        private Slider _speedSlider;

        [SerializeField, Tooltip("発射ボタン")]
        private Button _launchButton;

        [SerializeField, Tooltip("軌道プレビュー用LineRenderer")]
        private LineRenderer _orbitPreview;

        [SerializeField, Tooltip("刃スプライト")]
        private Sprite _bladeSprite;

        [SerializeField, Tooltip("敵スプライト")]
        private Sprite _enemySprite;

        private BladeController _blade;
        private List<Enemy> _enemies = new List<Enemy>();
        private bool _isLaunched;

        // 敵の固定配置座標（ピボット(0,0.5)基準ではなくワールド座標で指定）
        private static readonly Vector2[] EnemyPositions = {
            new Vector2(-2.0f,  4.0f), new Vector2(0.0f, 4.5f), new Vector2(2.0f, 4.0f),
            new Vector2(-3.5f,  2.0f), new Vector2(3.5f, 2.0f),
            new Vector2(-2.5f, -0.5f), new Vector2(0.0f, 0.0f), new Vector2(2.5f, -0.5f),
        };

        private void Start()
        {
            if (_radiusSlider != null)
            {
                _radiusSlider.minValue = 0.5f;
                _radiusSlider.maxValue = 4.0f;
                _radiusSlider.value = 2.0f;
                _radiusSlider.onValueChanged.AddListener(_ => UpdateOrbitPreview());
            }
            if (_speedSlider != null)
            {
                _speedSlider.minValue = 1.0f;
                _speedSlider.maxValue = 6.0f;
                _speedSlider.value = 3.0f;
            }
            if (_launchButton != null)
                _launchButton.onClick.AddListener(OnLaunchButtonPressed);
        }

        public void StartStage()
        {
            SpawnEnemies();
            SpawnBlade();
            UpdateOrbitPreview();
        }

        private void SpawnEnemies()
        {
            foreach (var e in _enemies)
                if (e != null) Destroy(e.gameObject);
            _enemies.Clear();

            for (int i = 0; i < EnemyPositions.Length; i++)
            {
                var obj = new GameObject($"Enemy_{i}");
                obj.transform.position = new Vector3(EnemyPositions[i].x, EnemyPositions[i].y, 0f);
                obj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                obj.transform.SetParent(transform);

                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 2;
                var col = obj.AddComponent<CircleCollider2D>();
                var enemy = obj.AddComponent<Enemy>();
                enemy.Initialize(_enemySprite, OnEnemyKilled);
                _enemies.Add(enemy);
            }
        }

        private void SpawnBlade()
        {
            if (_blade != null) { Destroy(_blade.gameObject); _blade = null; }

            var bladeObj = new GameObject("Blade");
            bladeObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var sr = bladeObj.AddComponent<SpriteRenderer>();
            sr.sprite = _bladeSprite;
            sr.sortingOrder = 5;

            bladeObj.AddComponent<CircleCollider2D>();
            _blade = bladeObj.AddComponent<BladeController>();
            _blade.Initialize(_pivot, OnBladeFinished);
        }

        private void OnLaunchButtonPressed()
        {
            if (_isLaunched || !_gameManager.IsPlaying) return;
            if (_blade == null || _blade.IsActive) return;

            _isLaunched = true;
            SetSlidersInteractable(false);
            if (_launchButton != null) _launchButton.interactable = false;

            float radius = _radiusSlider != null ? _radiusSlider.value : 2.0f;
            float speed = _speedSlider != null ? _speedSlider.value : 3.0f;
            _blade.Launch(radius, speed);
        }

        private void OnBladeFinished()
        {
            _isLaunched = false;
            _gameManager.OnLaunchUsed();

            if (_gameManager.IsPlaying)
            {
                SetSlidersInteractable(true);
                if (_launchButton != null) _launchButton.interactable = true;
                SpawnBlade();
                UpdateOrbitPreview();
            }
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            _enemies.Remove(enemy);
            _gameManager.AddKill();
        }

        private void UpdateOrbitPreview()
        {
            if (_orbitPreview == null || _pivot == null) return;
            float radius = _radiusSlider != null ? _radiusSlider.value : 2.0f;
            int segments = 64;
            _orbitPreview.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                float x = _pivot.position.x + Mathf.Cos(angle) * radius;
                float y = _pivot.position.y + Mathf.Sin(angle) * radius;
                _orbitPreview.SetPosition(i, new Vector3(x, y, 0f));
            }
        }

        private void SetSlidersInteractable(bool interactable)
        {
            if (_radiusSlider != null) _radiusSlider.interactable = interactable;
            if (_speedSlider != null) _speedSlider.interactable = interactable;
        }
    }
}
