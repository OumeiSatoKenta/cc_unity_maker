using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game057v2_CandyDrop
{
    public class CandySpawner : MonoBehaviour
    {
        [SerializeField] CandyDropGameManager _gameManager;
        [SerializeField] CandyDropUI _ui;

        // Sprites assigned by SceneSetup
        [SerializeField] Sprite[] _circleSprites;   // 4 colors
        [SerializeField] Sprite[] _squareSprites;   // 4 colors
        [SerializeField] Sprite[] _triangleSprites; // 4 colors
        [SerializeField] Sprite[] _starSprites;     // 4 colors
        [SerializeField] Sprite _meltSprite;
        [SerializeField] Sprite _giantSprite;

        public enum CandyShape { Circle, Square, Triangle, Star }
        public enum CandyColor { Red, Blue, Green, Yellow }

        class CandyData
        {
            public CandyShape shape;
            public CandyColor color;
            public bool isMelt;
            public bool isGiant;
        }

        GameObject _currentCandy;
        CandyData _nextCandyData;
        bool _isSpawning;
        bool _isDragging;
        float _spawnX;
        float _spawnY;
        float _halfWidth;
        int _stageNumber;
        float _complexityFactor;
        bool _windEnabled;
        float _windForce;

        // physics
        List<Rigidbody2D> _allCandyBodies = new List<Rigidbody2D>();

        static readonly Color[] CandyColors = {
            new Color(1f, 0.39f, 0.39f),   // Red
            new Color(0.39f, 0.71f, 1f),   // Blue
            new Color(0.39f, 0.86f, 0.39f), // Green
            new Color(1f, 0.78f, 0.31f),   // Yellow
        };

        void Awake()
        {
            _isSpawning = false;
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            _stageNumber = stageNumber;
            _complexityFactor = config.complexityFactor;
            _windEnabled = _complexityFactor >= 1.0f;
            _windForce = _windEnabled ? 1.5f : 0f;

            if (Camera.main == null) return;
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            _spawnY = camSize - 1.0f;
            _halfWidth = camWidth - 0.8f;

            // Clear existing candies when stage changes (except first)
            foreach (var body in _allCandyBodies)
            {
                if (body != null) Destroy(body.gameObject);
            }
            _allCandyBodies.Clear();

            if (_currentCandy != null)
            {
                Destroy(_currentCandy);
                _currentCandy = null;
            }

            _isSpawning = true;
            _nextCandyData = GenerateCandyData();
            UpdateNextPreview();
            SpawnNextCandy();
        }

        public void StopSpawning()
        {
            _isSpawning = false;
        }

        public void RegisterBody(Rigidbody2D rb)
        {
            _allCandyBodies.Add(rb);
        }

        CandyData GenerateCandyData()
        {
            var data = new CandyData();
            // Shape selection based on stage
            int maxShapeIndex = _stageNumber <= 1 ? 1 : (_stageNumber == 2 ? 2 : 3);
            data.shape = (CandyShape)Random.Range(0, maxShapeIndex + 1);
            data.color = (CandyColor)Random.Range(0, 4);
            data.isMelt = _complexityFactor >= 0.8f && Random.value < 0.3f;
            data.isGiant = _complexityFactor >= 1.0f && Random.value < 0.2f;
            return data;
        }

        void UpdateNextPreview()
        {
            if (_ui == null || _nextCandyData == null) return;
            Sprite previewSprite = GetSprite(_nextCandyData);
            Color previewColor = _nextCandyData.isMelt ? new Color(0.59f, 0.86f, 1f) :
                                 _nextCandyData.isGiant ? new Color(1f, 0.63f, 0.2f) :
                                 CandyColors[(int)_nextCandyData.color];
            _ui.UpdateNextPreview(previewSprite, previewColor);
        }

        Sprite GetSprite(CandyData data)
        {
            if (data.isMelt && _meltSprite != null) return _meltSprite;
            if (data.isGiant && _giantSprite != null) return _giantSprite;
            int colorIdx = (int)data.color;
            return data.shape switch
            {
                CandyShape.Circle => (_circleSprites != null && _circleSprites.Length > colorIdx) ? _circleSprites[colorIdx] : null,
                CandyShape.Square => (_squareSprites != null && _squareSprites.Length > colorIdx) ? _squareSprites[colorIdx] : null,
                CandyShape.Triangle => (_triangleSprites != null && _triangleSprites.Length > colorIdx) ? _triangleSprites[colorIdx] : null,
                CandyShape.Star => (_starSprites != null && _starSprites.Length > colorIdx) ? _starSprites[colorIdx] : null,
                _ => null
            };
        }

        void SpawnNextCandy()
        {
            if (!_isSpawning) return;
            var currentData = _nextCandyData;
            _nextCandyData = GenerateCandyData();
            UpdateNextPreview();

            _spawnX = 0f;
            float scale = currentData.isGiant ? 1.5f : 0.6f;

            var go = new GameObject("Candy");
            go.transform.position = new Vector3(_spawnX, _spawnY, 0);
            go.transform.localScale = Vector3.one * scale;
            go.layer = LayerMask.NameToLayer("Default");
            go.tag = "Untagged";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite(currentData);
            sr.sortingOrder = 2;
            if (currentData.isMelt)
                sr.color = new Color(0.59f, 0.86f, 1f, 0.85f);
            else if (currentData.isGiant)
                sr.color = new Color(1f, 0.63f, 0.2f);
            else
                sr.color = CandyColors[(int)currentData.color];

            // Collider based on shape
            switch (currentData.shape)
            {
                case CandyShape.Circle:
                    var cc = go.AddComponent<CircleCollider2D>();
                    cc.radius = 0.45f;
                    break;
                case CandyShape.Square:
                    var bc = go.AddComponent<BoxCollider2D>();
                    bc.size = new Vector2(0.85f, 0.85f);
                    break;
                case CandyShape.Triangle:
                    var pc = go.AddComponent<PolygonCollider2D>();
                    pc.SetPath(0, new Vector2[]{ new Vector2(0,0.48f), new Vector2(-0.42f,-0.42f), new Vector2(0.42f,-0.42f) });
                    break;
                case CandyShape.Star:
                    var sc = go.AddComponent<CircleCollider2D>();
                    sc.radius = 0.4f;
                    break;
            }

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // not falling yet, waiting for drop
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            if (currentData.isGiant) rb.mass = 3f;
            else if (currentData.shape == CandyShape.Triangle) rb.mass = 0.7f;
            else rb.mass = 1f;

            var ctrl = go.AddComponent<CandyController>();
            ctrl.Init(currentData.isMelt ? 5f : 0f, currentData.color, currentData.shape, _gameManager, this);

            _currentCandy = go;
            _isDragging = false;
        }

        void Update()
        {
            if (!_isSpawning || _currentCandy == null) return;
            if (!_gameManager.IsPlaying()) return;

            var mouse = Mouse.current;
            if (mouse == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            Vector2 mousePos = mouse.position.ReadValue();
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));

            if (mouse.leftButton.isPressed)
            {
                _isDragging = true;
                float newX = Mathf.Clamp(worldPos.x, -_halfWidth, _halfWidth);
                _currentCandy.transform.position = new Vector3(newX, _spawnY, 0);
            }

            if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
            {
                DropCandy();
            }
        }

        void DropCandy()
        {
            if (_currentCandy == null) return;
            var rb = _currentCandy.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            rb.constraints = RigidbodyConstraints2D.None;
            rb.gravityScale = 2.0f;
            _allCandyBodies.Add(rb);
            _currentCandy = null;
            _isDragging = false;

            if (_isSpawning)
                StartCoroutine(SpawnAfterDelay(0.8f));
        }

        IEnumerator SpawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_isSpawning && _gameManager.IsPlaying())
                SpawnNextCandy();
        }

        void FixedUpdate()
        {
            if (!_windEnabled) return;
            float windDir = Mathf.Sin(Time.time * 0.5f);
            foreach (var rb in _allCandyBodies)
            {
                if (rb != null && !rb.isKinematic)
                    rb.AddForce(new Vector2(windDir * _windForce * rb.mass * Time.fixedDeltaTime, 0));
            }
        }
    }
}
