using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game096_DualControl
{
    public class DualManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private DualControlGameManager _gameManager;
        [SerializeField, Tooltip("キャラクタースプライト")] private Sprite _characterSprite;
        [SerializeField, Tooltip("障害物スプライト")] private Sprite _obstacleSprite;
        [SerializeField, Tooltip("ゴール数（片側）")] private int _goalCount = 8;
        [SerializeField, Tooltip("初期障害物速度")] private float _initialSpeed = 2.5f;
        [SerializeField, Tooltip("最大障害物速度")] private float _maxSpeed = 5.0f;
        [SerializeField, Tooltip("初期スポーン間隔")] private float _initialInterval = 1.8f;
        [SerializeField, Tooltip("最小スポーン間隔")] private float _minInterval = 0.7f;

        private static readonly float[] LeftColX  = { -2.4f, -1.6f, -0.8f };
        private static readonly float[] RightColX = {  0.8f,  1.6f,  2.4f };
        private const float CharY        = -3.5f;
        private const float SpawnY       =  6.0f;
        private const float DestroyY     = -4.8f;
        private const float HitThreshold =  0.45f;

        private int _leftCol;
        private int _rightCol;
        private int _leftAvoided;
        private int _rightAvoided;
        private float _obstacleSpeed;
        private float _leftSpawnTimer;
        private float _rightSpawnTimer;
        private float _leftSpawnInterval;
        private float _rightSpawnInterval;
        private bool _isActive;
        private Camera _mainCamera;

        private GameObject _leftChar;
        private GameObject _rightChar;

        private class ObstacleData
        {
            public GameObject obj;
            public int side; // 0=left, 1=right
            public int col;  // 0,1,2
        }
        private readonly List<ObstacleData> _obstacles = new List<ObstacleData>();
        private readonly List<ObstacleData> _toRemove  = new List<ObstacleData>();

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            if (_characterSprite == null) { Debug.LogError("[DualManager] _characterSprite が未アサイン"); return; }
            if (_obstacleSprite  == null) { Debug.LogError("[DualManager] _obstacleSprite が未アサイン");  return; }

            _isActive            = true;
            _leftCol             = 1;
            _rightCol            = 1;
            _leftAvoided         = 0;
            _rightAvoided        = 0;
            _obstacleSpeed       = _initialSpeed;
            _leftSpawnInterval   = _initialInterval;
            _rightSpawnInterval  = _initialInterval;
            _leftSpawnTimer      = _leftSpawnInterval;
            _rightSpawnTimer     = _rightSpawnInterval + 0.5f; // 右レーンを少しずらす

            foreach (var od in _obstacles) if (od.obj != null) Destroy(od.obj);
            _obstacles.Clear();

            CreateCharacters();
            _gameManager?.UpdateStage(_leftAvoided, _rightAvoided, _goalCount);
        }

        public void StopGame()
        {
            _isActive = false;
            foreach (var od in _obstacles) if (od.obj != null) Destroy(od.obj);
            _obstacles.Clear();
        }

        private void CreateCharacters()
        {
            if (_leftChar  != null) Destroy(_leftChar);
            if (_rightChar != null) Destroy(_rightChar);

            _leftChar  = CreateSprite("CharLeft",  _characterSprite, new Color(0.3f, 0.7f, 1.0f), LeftColX[_leftCol],   CharY, 2);
            _rightChar = CreateSprite("CharRight", _characterSprite, new Color(1.0f, 0.6f, 0.2f), RightColX[_rightCol], CharY, 2);
        }

        private GameObject CreateSprite(string name, Sprite sprite, Color color, float x, float y, int sortOrder)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(transform);
            obj.transform.position = new Vector3(x, y, 0f);
            obj.transform.localScale = Vector3.one * 0.9f;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortOrder;
            return obj;
        }

        private void UpdateCharVisuals()
        {
            if (_leftChar  != null) _leftChar.transform.position  = new Vector3(LeftColX[_leftCol],   CharY, 0f);
            if (_rightChar != null) _rightChar.transform.position = new Vector3(RightColX[_rightCol], CharY, 0f);
        }

        private void Update()
        {
            if (!_isActive) return;
            HandleInput();
            UpdateSpawning();
            UpdateObstacles();
        }

        private void HandleInput()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                ProcessScreenInput(Mouse.current.position.ReadValue());

            var ts = Touchscreen.current;
            if (ts != null)
            {
                foreach (var touch in ts.touches)
                {
                    if (touch.press.wasPressedThisFrame)
                        ProcessScreenInput(touch.position.ReadValue());
                }
            }
        }

        private void ProcessScreenInput(Vector2 sp)
        {
            float halfW = Screen.width * 0.5f;
            float colW  = halfW / 3f;
            if (sp.x < halfW)
            {
                _leftCol = Mathf.Clamp((int)(sp.x / colW), 0, 2);
            }
            else
            {
                _rightCol = Mathf.Clamp((int)((sp.x - halfW) / colW), 0, 2);
            }
            UpdateCharVisuals();
        }

        private void UpdateSpawning()
        {
            _leftSpawnTimer  -= Time.deltaTime;
            _rightSpawnTimer -= Time.deltaTime;

            if (_leftSpawnTimer <= 0f)
            {
                SpawnObstacle(0);
                _leftSpawnTimer     = _leftSpawnInterval;
                _leftSpawnInterval  = Mathf.Max(_leftSpawnInterval - 0.05f, _minInterval);
            }
            if (_rightSpawnTimer <= 0f)
            {
                SpawnObstacle(1);
                _rightSpawnTimer    = _rightSpawnInterval;
                _rightSpawnInterval = Mathf.Max(_rightSpawnInterval - 0.05f, _minInterval);
            }

            _obstacleSpeed = Mathf.Min(_obstacleSpeed + 0.002f, _maxSpeed);
        }

        private void SpawnObstacle(int side)
        {
            int col   = Random.Range(0, 3);
            float x   = side == 0 ? LeftColX[col] : RightColX[col];
            var color = side == 0 ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.7f, 0.2f, 0.9f);
            var obj   = CreateSprite($"Obs_{side}_{col}", _obstacleSprite, color, x, SpawnY, 1);
            _obstacles.Add(new ObstacleData { obj = obj, side = side, col = col });
        }

        private void UpdateObstacles()
        {
            _toRemove.Clear();
            foreach (var od in _obstacles)
            {
                if (od.obj == null) { _toRemove.Add(od); continue; }

                od.obj.transform.Translate(Vector3.down * _obstacleSpeed * Time.deltaTime);
                float y = od.obj.transform.position.y;

                // ヒット判定
                int charCol = od.side == 0 ? _leftCol : _rightCol;
                if (od.col == charCol && y <= CharY + HitThreshold && y >= CharY - HitThreshold)
                {
                    _isActive = false;
                    Destroy(od.obj);
                    _toRemove.Add(od);
                    _gameManager?.OnCharacterHit();
                    foreach (var r in _toRemove) _obstacles.Remove(r);
                    return;
                }

                // 回避判定
                if (y < DestroyY)
                {
                    Destroy(od.obj);
                    _toRemove.Add(od);
                    if (od.side == 0) _leftAvoided++;
                    else              _rightAvoided++;
                    _gameManager?.UpdateStage(_leftAvoided, _rightAvoided, _goalCount);
                    CheckGoal();
                }
            }
            foreach (var od in _toRemove) _obstacles.Remove(od);
        }

        private void CheckGoal()
        {
            if (_leftAvoided >= _goalCount && _rightAvoided >= _goalCount && _isActive)
            {
                _isActive = false;
                _gameManager?.OnGoalReached();
            }
        }

        public int LeftAvoided  => _leftAvoided;
        public int RightAvoided => _rightAvoided;
    }
}
