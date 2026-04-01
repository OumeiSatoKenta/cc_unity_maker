using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game100_DreamRun
{
    public class RunManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private DreamRunGameManager _gameManager;
        [SerializeField, Tooltip("キャラクタースプライト")] private Sprite _characterSprite;
        [SerializeField, Tooltip("障害物スプライト")] private Sprite _obstacleSprite;
        [SerializeField, Tooltip("オーブスプライト")] private Sprite _orbSprite;

        private static readonly float[] LaneY = { 2f, 0f, -2f };
        private const float CharX = -3f;
        private const float SpawnX = 8f;
        private const float DestroyX = -7f;
        private const float HitDistance = 0.8f;

        private int _currentLane = 1;
        private GameObject _character;
        private SpriteRenderer _charRenderer;
        private bool _isRunning;
        private bool _isInvincible;
        private float _obstacleSpeed = 4f;
        private float _spawnTimer;
        private float _spawnInterval = 1.5f;
        private float _orbTimer;
        private float _runDistance;

        private readonly List<GameObject> _obstacles = new List<GameObject>();
        private readonly List<int> _obstacleLanes = new List<int>();
        private readonly List<GameObject> _orbs = new List<GameObject>();
        private readonly List<int> _orbLanes = new List<int>();
        private readonly List<GameObject> _toDestroy = new List<GameObject>();

        public void StartRun()
        {
            _isRunning = true;
            _currentLane = 1;
            _obstacleSpeed = 4f;
            _spawnInterval = 1.5f;
            _spawnTimer = _spawnInterval;
            _orbTimer = 3f;
            _runDistance = 0f;

            ClearObjects();
            CreateCharacter();
        }

        public void StopRun()
        {
            _isRunning = false;
        }

        private void CreateCharacter()
        {
            if (_character != null) Destroy(_character);
            _character = new GameObject("DreamRunner");
            _character.transform.SetParent(transform);
            _character.transform.position = new Vector3(CharX, LaneY[_currentLane], 0f);
            _character.transform.localScale = Vector3.one * 1.2f;
            _charRenderer = _character.AddComponent<SpriteRenderer>();
            _charRenderer.sprite = _characterSprite;
            _charRenderer.color = Color.white;
            _charRenderer.sortingOrder = 5;
        }

        private void ClearObjects()
        {
            foreach (var o in _obstacles) if (o != null) Destroy(o);
            _obstacles.Clear();
            _obstacleLanes.Clear();
            foreach (var o in _orbs) if (o != null) Destroy(o);
            _orbs.Clear();
            _orbLanes.Clear();
        }

        private void Update()
        {
            if (!_isRunning || _gameManager == null || !_gameManager.IsPlaying) return;

            HandleInput();
            UpdateSpawning();
            UpdateObjects();
            UpdateDifficulty();
        }

        private void HandleInput()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                float screenY = Mouse.current.position.ReadValue().y;
                ProcessLaneInput(screenY);
            }

            var ts = Touchscreen.current;
            if (ts != null)
            {
                foreach (var touch in ts.touches)
                {
                    if (touch.press.wasPressedThisFrame)
                    {
                        ProcessLaneInput(touch.position.ReadValue().y);
                        break;
                    }
                }
            }
        }

        private void ProcessLaneInput(float screenY)
        {
            float halfH = Screen.height * 0.5f;
            if (screenY > halfH)
            {
                // 上半分タップ → レーン上へ
                if (_currentLane > 0) _currentLane--;
            }
            else
            {
                // 下半分タップ → レーン下へ
                if (_currentLane < 2) _currentLane++;
            }
            UpdateCharacterPosition();
        }

        private void UpdateCharacterPosition()
        {
            if (_character != null)
                _character.transform.position = new Vector3(CharX, LaneY[_currentLane], 0f);
        }

        private void UpdateSpawning()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnObstacle();
                _spawnTimer = _spawnInterval;
            }

            // オーブスポーン
            if (_gameManager.Fragments < DreamRunGameManager.TotalFragments)
            {
                _orbTimer -= Time.deltaTime;
                if (_orbTimer <= 0f)
                {
                    SpawnOrb();
                    _orbTimer = Random.Range(4f, 7f);
                }
            }
        }

        private void SpawnObstacle()
        {
            int lane = Random.Range(0, 3);
            var obj = CreateWorldSprite("Obstacle", _obstacleSprite, new Color(0.9f, 0.3f, 0.4f),
                SpawnX, LaneY[lane], 3);
            _obstacles.Add(obj);
            _obstacleLanes.Add(lane);
        }

        private void SpawnOrb()
        {
            int lane = Random.Range(0, 3);
            var obj = CreateWorldSprite("Orb", _orbSprite, Color.white, SpawnX, LaneY[lane], 4);
            _orbs.Add(obj);
            _orbLanes.Add(lane);
        }

        private GameObject CreateWorldSprite(string name, Sprite sprite, Color color, float x, float y, int order)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(transform);
            obj.transform.position = new Vector3(x, y, 0f);
            obj.transform.localScale = Vector3.one;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return obj;
        }

        private void UpdateObjects()
        {
            float speed = _obstacleSpeed * Time.deltaTime;
            Vector3 charPos = _character != null ? _character.transform.position : Vector3.zero;

            // 障害物移動と衝突判定
            _toDestroy.Clear();
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                if (_obstacles[i] == null) { _obstacles.RemoveAt(i); _obstacleLanes.RemoveAt(i); continue; }
                _obstacles[i].transform.Translate(Vector3.left * speed);

                if (_obstacles[i].transform.position.x < DestroyX)
                {
                    Destroy(_obstacles[i]);
                    _obstacles.RemoveAt(i);
                    _obstacleLanes.RemoveAt(i);
                    continue;
                }

                // 衝突判定
                if (!_isInvincible && _obstacleLanes[i] == _currentLane &&
                    Vector2.Distance(_obstacles[i].transform.position, charPos) < HitDistance)
                {
                    Destroy(_obstacles[i]);
                    _obstacles.RemoveAt(i);
                    _obstacleLanes.RemoveAt(i);
                    _gameManager.OnHitObstacle();
                    StartCoroutine(InvincibilityCoroutine());
                    return;
                }
            }

            // オーブ移動と取得判定
            for (int i = _orbs.Count - 1; i >= 0; i--)
            {
                if (_orbs[i] == null) { _orbs.RemoveAt(i); _orbLanes.RemoveAt(i); continue; }
                _orbs[i].transform.Translate(Vector3.left * speed * 0.8f);

                if (_orbs[i].transform.position.x < DestroyX)
                {
                    Destroy(_orbs[i]);
                    _orbs.RemoveAt(i);
                    _orbLanes.RemoveAt(i);
                    continue;
                }

                if (_orbLanes[i] == _currentLane &&
                    Vector2.Distance(_orbs[i].transform.position, charPos) < HitDistance)
                {
                    Destroy(_orbs[i]);
                    _orbs.RemoveAt(i);
                    _orbLanes.RemoveAt(i);
                    _gameManager.OnCollectFragment();
                    return;
                }
            }
        }

        private IEnumerator InvincibilityCoroutine()
        {
            _isInvincible = true;
            float elapsed = 0f;
            float duration = 1.5f;
            while (elapsed < duration && _charRenderer != null)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.PingPong(elapsed * 8f, 1f) > 0.5f ? 1f : 0.3f;
                _charRenderer.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
            if (_charRenderer != null) _charRenderer.color = Color.white;
            _isInvincible = false;
        }

        private void UpdateDifficulty()
        {
            _runDistance += Time.deltaTime;
            _obstacleSpeed = Mathf.Min(4f + _runDistance * 0.05f, 7f);
            _spawnInterval = Mathf.Max(1.5f - _runDistance * 0.02f, 0.8f);
        }
    }
}
