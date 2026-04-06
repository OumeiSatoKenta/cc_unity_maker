using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace Game055v2_DustSweep
{
    public class DustBoard : MonoBehaviour
    {
        [SerializeField] DustSweepGameManager _gm;
        [SerializeField] SpriteRenderer _dustRenderer;
        [SerializeField] SpriteRenderer _hardDustRenderer;
        [SerializeField] SpriteRenderer _dangerRenderer;
        [SerializeField] SpriteRenderer _cleanRenderer;
        [SerializeField] Sprite[] _hiddenItemSprites; // 5 items
        [SerializeField] Transform _itemContainer;

        const int TEX_SIZE = 256;
        bool[,] _dustPixels = new bool[TEX_SIZE, TEX_SIZE];
        int[,] _hardness = new int[TEX_SIZE, TEX_SIZE]; // 0=normal,1=stubborn(need 2 swipes)
        bool[,] _dangerZone = new bool[TEX_SIZE, TEX_SIZE];
        Texture2D _dustTex;
        Texture2D _hardTex;
        Texture2D _dangerTex;

        int _totalDust;
        int _clearedDust;
        float _boardSize;
        float _timeLimit;
        float _remainingTime;
        bool _isActive;

        // Stage config
        bool _hasStubborn;
        bool _hasRedZone;
        bool _hasRespawn;
        int _totalItems;
        GameObject[] _itemObjects;
        bool[] _itemsRevealed;
        Vector2Int[] _itemPositions;
        int _brushRadius = 20;

        Coroutine _respawnCoroutine;

        public float RemainingTime => _remainingTime;
        public int TotalItems => _totalItems;

        public void SubtractTime(float seconds)
        {
            _remainingTime = Mathf.Max(0f, _remainingTime - seconds);
        }

        struct StageParams
        {
            public float dustRatio;
            public float timeLimit;
            public bool hasStubborn;
            public bool hasRedZone;
            public bool hasRespawn;
            public int itemCount;
        }

        static StageParams[] _stageParams = new StageParams[]
        {
            new StageParams { dustRatio=0.30f, timeLimit=60f, hasStubborn=false, hasRedZone=false, hasRespawn=false, itemCount=1 },
            new StageParams { dustRatio=0.50f, timeLimit=50f, hasStubborn=true,  hasRedZone=false, hasRespawn=false, itemCount=2 },
            new StageParams { dustRatio=0.55f, timeLimit=50f, hasStubborn=true,  hasRedZone=true,  hasRespawn=false, itemCount=2 },
            new StageParams { dustRatio=0.70f, timeLimit=60f, hasStubborn=false, hasRedZone=false, hasRespawn=true,  itemCount=3 },
            new StageParams { dustRatio=0.75f, timeLimit=70f, hasStubborn=true,  hasRedZone=true,  hasRespawn=true,  itemCount=5 },
        };

        public void SetupStage(StageManager.StageConfig config, int stageNumber = 1)
        {
            _isActive = false;
            if (_respawnCoroutine != null) StopCoroutine(_respawnCoroutine);

            // Clear old items
            if (_itemObjects != null)
                foreach (var obj in _itemObjects)
                    if (obj != null) Destroy(obj);

            int stageIdx = Mathf.Clamp(stageNumber - 1, 0, _stageParams.Length - 1);
            var p = _stageParams[stageIdx];
            _hasStubborn = p.hasStubborn;
            _hasRedZone = p.hasRedZone;
            _hasRespawn = p.hasRespawn;
            _totalItems = p.itemCount;
            _timeLimit = p.timeLimit;
            _remainingTime = _timeLimit;

            CalcBoardSize();
            InitDustMap(p.dustRatio, p.hasStubborn, p.hasRedZone);
            PlaceHiddenItems(p.itemCount);
            UpdateTextures();

            _isActive = true;
            if (_hasRespawn) _respawnCoroutine = StartCoroutine(RespawnLoop(config.speedMultiplier));

            _gm.OnCleanlinessUpdate(0f);
        }

        void CalcBoardSize()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.2f;
            float bottomMargin = 2.8f;
            float availH = camSize * 2f - topMargin - bottomMargin;
            float availW = camWidth * 2f - 0.4f;
            _boardSize = Mathf.Min(availH, availW);
            transform.localScale = Vector3.one * (_boardSize / 256f * 256f / TEX_SIZE);
            transform.position = new Vector3(0f, (camSize - topMargin - _boardSize / 2f - 0.2f) * -1f + (camSize - topMargin), 0f);
            // Center between top and bottom margins
            float centerY = camSize - topMargin - _boardSize / 2f;
            transform.position = new Vector3(0f, centerY, 0f);
        }

        void InitDustMap(float dustRatio, bool hasStubborn, bool hasRedZone)
        {
            _dustPixels = new bool[TEX_SIZE, TEX_SIZE];
            _hardness = new int[TEX_SIZE, TEX_SIZE];
            _dangerZone = new bool[TEX_SIZE, TEX_SIZE];
            _totalDust = 0;
            _clearedDust = 0;

            var rng = new System.Random(42);
            for (int y = 0; y < TEX_SIZE; y++)
                for (int x = 0; x < TEX_SIZE; x++)
                {
                    if (rng.NextDouble() < dustRatio)
                    {
                        _dustPixels[x, y] = true;
                        _totalDust++;
                        if (hasStubborn && rng.NextDouble() < 0.2f)
                            _hardness[x, y] = 1;
                    }
                }

            if (hasRedZone)
            {
                // place danger zone in a corner region
                int zx = TEX_SIZE / 4;
                int zy = TEX_SIZE / 4;
                int zw = TEX_SIZE / 5;
                int zh = TEX_SIZE / 5;
                for (int y = zy; y < zy + zh && y < TEX_SIZE; y++)
                    for (int x = zx; x < zx + zw && x < TEX_SIZE; x++)
                    {
                        _dangerZone[x, y] = true;
                        _dustPixels[x, y] = false; // danger zone has no dust
                    }
            }
        }

        void PlaceHiddenItems(int count)
        {
            _itemObjects = new GameObject[count];
            _itemsRevealed = new bool[count];
            _itemPositions = new Vector2Int[count];
            var rng = new System.Random(99);
            for (int i = 0; i < count; i++)
            {
                int px, py;
                int attempts = 0;
                do {
                    px = rng.Next(30, TEX_SIZE - 30);
                    py = rng.Next(30, TEX_SIZE - 30);
                    attempts++;
                } while (_dangerZone[px, py] && attempts < 100);

                _itemPositions[i] = new Vector2Int(px, py);
                // Ensure item area has dust
                for (int dy = -10; dy <= 10; dy++)
                    for (int dx = -10; dx <= 10; dx++)
                    {
                        int nx = px + dx; int ny = py + dy;
                        if (nx >= 0 && nx < TEX_SIZE && ny >= 0 && ny < TEX_SIZE)
                        {
                            if (!_dustPixels[nx, ny])
                            {
                                _dustPixels[nx, ny] = true;
                                _totalDust++;
                            }
                        }
                    }

                // Create hidden item object (initially invisible)
                var go = new GameObject($"HiddenItem{i}");
                go.transform.SetParent(_itemContainer);
                go.transform.position = TexToWorld(px, py);
                go.transform.localScale = Vector3.one * 0.3f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 1;
                if (_hiddenItemSprites != null && i < _hiddenItemSprites.Length)
                    sr.sprite = _hiddenItemSprites[i];
                sr.color = new Color(1, 1, 1, 0); // hidden
                _itemObjects[i] = go;
            }
        }

        void UpdateTextures()
        {
            if (_dustTex == null)
            {
                _dustTex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
                _dustTex.filterMode = FilterMode.Point;
            }
            if (_hardTex == null)
            {
                _hardTex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
                _hardTex.filterMode = FilterMode.Point;
            }
            if (_dangerTex == null)
            {
                _dangerTex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
                _dangerTex.filterMode = FilterMode.Point;
            }

            for (int y = 0; y < TEX_SIZE; y++)
                for (int x = 0; x < TEX_SIZE; x++)
                {
                    Color dustCol = _dustPixels[x, y] ? new Color(0.7f, 0.64f, 0.55f, 1f) : Color.clear;
                    _dustTex.SetPixel(x, y, dustCol);

                    Color hardCol = (_dustPixels[x, y] && _hardness[x, y] > 0) ? new Color(0.47f, 0.39f, 0.31f, 1f) : Color.clear;
                    _hardTex.SetPixel(x, y, hardCol);

                    Color dangerCol = _dangerZone[x, y] ? new Color(0.86f, 0.24f, 0.24f, 0.7f) : Color.clear;
                    _dangerTex.SetPixel(x, y, dangerCol);
                }

            _dustTex.Apply();
            _hardTex.Apply();
            _dangerTex.Apply();

            if (_dustRenderer != null)
                _dustRenderer.sprite = Sprite.Create(_dustTex, new Rect(0, 0, TEX_SIZE, TEX_SIZE), Vector2.one * 0.5f, TEX_SIZE);
            if (_hardDustRenderer != null)
                _hardDustRenderer.sprite = Sprite.Create(_hardTex, new Rect(0, 0, TEX_SIZE, TEX_SIZE), Vector2.one * 0.5f, TEX_SIZE);
            if (_dangerRenderer != null)
                _dangerRenderer.sprite = Sprite.Create(_dangerTex, new Rect(0, 0, TEX_SIZE, TEX_SIZE), Vector2.one * 0.5f, TEX_SIZE);
        }

        void ApplyBrush(Vector2Int center)
        {
            bool hitDanger = false;
            int cleared = 0;
            for (int dy = -_brushRadius; dy <= _brushRadius; dy++)
                for (int dx = -_brushRadius; dx <= _brushRadius; dx++)
                {
                    if (dx * dx + dy * dy > _brushRadius * _brushRadius) continue;
                    int nx = center.x + dx;
                    int ny = center.y + dy;
                    if (nx < 0 || nx >= TEX_SIZE || ny < 0 || ny >= TEX_SIZE) continue;

                    if (_dangerZone[nx, ny] && !_dustPixels[nx, ny]) hitDanger = true;

                    if (_dustPixels[nx, ny])
                    {
                        if (_hardness[nx, ny] > 0)
                        {
                            _hardness[nx, ny]--;
                            if (_hardness[nx, ny] <= 0)
                            {
                                _dustPixels[nx, ny] = false;
                                cleared++;
                                _clearedDust++;
                            }
                        }
                        else
                        {
                            _dustPixels[nx, ny] = false;
                            cleared++;
                            _clearedDust++;
                        }
                    }
                }

            if (hitDanger) _gm.OnDangerZoneHit();

            // Check item reveals
            if (_itemPositions != null)
                for (int i = 0; i < _itemPositions.Length; i++)
                {
                    if (_itemsRevealed[i]) continue;
                    var ip = _itemPositions[i];
                    int dist2 = (ip.x - center.x) * (ip.x - center.x) + (ip.y - center.y) * (ip.y - center.y);
                    if (dist2 <= (_brushRadius + 10) * (_brushRadius + 10) && !_dustPixels[ip.x, ip.y])
                    {
                        _itemsRevealed[i] = true;
                        StartCoroutine(RevealItem(i));
                        _gm.OnItemFound();
                    }
                }

            if (cleared > 0)
            {
                float cleanliness = (float)_clearedDust / Mathf.Max(_totalDust, 1);
                _gm.OnCleanlinessUpdate(cleanliness);
                if (cleanliness >= 1f) _gm.OnCleanlinessReached100();
            }

            UpdateTextures();
        }

        IEnumerator RevealItem(int i)
        {
            var sr = _itemObjects[i]?.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            float t = 0f;
            while (t < 0.3f)
            {
                float ratio = t / 0.3f;
                sr.color = new Color(1, 1, 1, ratio);
                float scale = 1f + Mathf.Sin(ratio * Mathf.PI) * 0.5f;
                _itemObjects[i].transform.localScale = Vector3.one * 0.3f * scale;
                t += Time.deltaTime;
                yield return null;
            }
            sr.color = Color.white;
            _itemObjects[i].transform.localScale = Vector3.one * 0.3f;
        }

        IEnumerator RespawnLoop(float speedMult)
        {
            float interval = Mathf.Max(3f, 10f / speedMult);
            while (true)
            {
                yield return new WaitForSeconds(interval);
                if (!_isActive) yield break;
                RespawnDust();
            }
        }

        void RespawnDust()
        {
            var rng = new System.Random();
            int count = TEX_SIZE * TEX_SIZE / 500;
            for (int i = 0; i < count; i++)
            {
                int x = rng.Next(0, TEX_SIZE);
                int y = rng.Next(0, TEX_SIZE);
                if (!_dangerZone[x, y] && !_dustPixels[x, y])
                {
                    _dustPixels[x, y] = true;
                    _clearedDust = Mathf.Max(0, _clearedDust - 1);
                }
            }
            UpdateTextures();
        }

        Vector3 TexToWorld(int px, int py)
        {
            float u = ((float)px / TEX_SIZE) - 0.5f;
            float v = ((float)py / TEX_SIZE) - 0.5f;
            return transform.position + new Vector3(u * _boardSize, v * _boardSize, -0.1f);
        }

        Vector2Int WorldToTex(Vector2 worldPos)
        {
            Vector2 local = worldPos - (Vector2)transform.position;
            int px = Mathf.RoundToInt((local.x / _boardSize + 0.5f) * TEX_SIZE);
            int py = Mathf.RoundToInt((local.y / _boardSize + 0.5f) * TEX_SIZE);
            return new Vector2Int(px, py);
        }

        bool _isDragging;
        Vector2 _lastMouseWorld;

        void Update()
        {
            if (!_isActive || !_gm.IsPlaying()) return;

            // Timer
            _remainingTime -= Time.deltaTime;
            _gm.OnTimerUpdate(_remainingTime);
            if (_remainingTime <= 0f)
            {
                _isActive = false;
                _gm.OnTimeUp();
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mouseScreen = mouse.position.ReadValue();
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _lastMouseWorld = mouseWorld;
            }
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }

            if (_isDragging && mouse.leftButton.isPressed)
            {
                float dist = Vector2.Distance(_lastMouseWorld, mouseWorld);
                _gm.OnSwipeDistance(dist);
                _lastMouseWorld = mouseWorld;

                var texPos = WorldToTex(mouseWorld);
                if (texPos.x >= 0 && texPos.x < TEX_SIZE && texPos.y >= 0 && texPos.y < TEX_SIZE)
                    ApplyBrush(texPos);
            }
        }

        public void SetBrushSize(int size)
        {
            // size: 0=small(10), 1=medium(18), 2=large(26)
            int[] radii = { 10, 18, 26 };
            _brushRadius = radii[Mathf.Clamp(size, 0, 2)];
        }

        void OnDestroy()
        {
            if (_dustTex != null) Destroy(_dustTex);
            if (_hardTex != null) Destroy(_hardTex);
            if (_dangerTex != null) Destroy(_dangerTex);
        }
    }
}
