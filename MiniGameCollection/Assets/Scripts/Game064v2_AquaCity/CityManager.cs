using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game064v2_AquaCity
{
    public class CityManager : MonoBehaviour
    {
        [SerializeField] AquaCityGameManager _gameManager;
        [SerializeField] AquaCityUI _ui;

        // Sprites
        [SerializeField] Sprite _houseSprite;
        [SerializeField] Sprite _plazaSprite;
        [SerializeField] Sprite _coralSprite;
        [SerializeField] Sprite _decoSprite;
        [SerializeField] Sprite _aquariumSprite;
        [SerializeField] Sprite _deepBaseSprite;
        [SerializeField] Sprite[] _fishSprites;
        [SerializeField] Sprite _sharkSprite;

        // Grid
        const int GridSize = 3;
        readonly BuildingType?[] _grid = new BuildingType?[GridSize * GridSize];
        readonly SpriteRenderer[] _cellRenderers = new SpriteRenderer[GridSize * GridSize];
        readonly GameObject[] _cellObjects = new GameObject[GridSize * GridSize];
        GameObject _gridRoot;

        // Economy
        long _coins = 20;
        long _population = 0;
        long _targetPopulation = 50;

        // Combo (fish tap)
        int _combo = 0;
        float _comboTimer = 0f;
        const float ComboWindow = 1.5f;

        // Stage config
        float _speedMultiplier = 1f;
        float _countMultiplier = 1f;
        bool _autoCollectEnabled = false;
        bool _adjacencyBonusEnabled = false;
        bool _sharkEnabled = false;
        bool _deepSeaEnabled = false;
        bool _isActive = false;
        int _currentStage = 1;

        // Shark
        bool _sharkActive = false;
        float _sharkTimer = 0f;
        const float SharkInterval = 20f;
        const float SharkDuration = 5f;

        // Fish objects
        readonly List<FishData> _fish = new();
        Coroutine _autoCoroutine;
        Coroutine _fishSpawnCoroutine;
        Coroutine _sharkCoroutine;

        // Building costs & income
        public static readonly long[] BuildingCost = { 0, 10, 20, 30, 50, 200 };
        static readonly long[] BuildingIncome = { 2, 5, 8, 3, 15, 50 };
        static readonly long[] BuildingPop = { 5, 10, 15, 5, 25, 100 };

        public enum BuildingType { House = 0, Plaza, Coral, Deco, Aquarium, DeepBase }

        class FishData
        {
            public GameObject obj;
            public SpriteRenderer sr;
            public Vector2 vel;
            public int fishIndex;
            public bool tapped;
        }

        // Cell size computed from camera
        float _cellSize = 1.6f;
        Vector2 _gridOrigin;

        void Start()
        {
            _gridRoot = new GameObject("Grid");
            _gridRoot.transform.SetParent(transform);
        }

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            _currentStage = stage;
            _speedMultiplier = config.speedMultiplier;
            _countMultiplier = config.countMultiplier;

            switch (stage)
            {
                case 1:
                    _targetPopulation = 50;
                    _autoCollectEnabled = false;
                    _adjacencyBonusEnabled = false;
                    _sharkEnabled = false;
                    _deepSeaEnabled = false;
                    break;
                case 2:
                    _targetPopulation = 200;
                    _autoCollectEnabled = true;
                    _adjacencyBonusEnabled = false;
                    _sharkEnabled = false;
                    _deepSeaEnabled = false;
                    _ui.ShowDecoUnlocked();
                    break;
                case 3:
                    _targetPopulation = 1000;
                    _autoCollectEnabled = true;
                    _adjacencyBonusEnabled = true;
                    _sharkEnabled = false;
                    _deepSeaEnabled = false;
                    _ui.ShowAdjacencyUnlocked();
                    break;
                case 4:
                    _targetPopulation = 5000;
                    _autoCollectEnabled = true;
                    _adjacencyBonusEnabled = true;
                    _sharkEnabled = true;
                    _deepSeaEnabled = false;
                    break;
                case 5:
                    _targetPopulation = 20000;
                    _autoCollectEnabled = true;
                    _adjacencyBonusEnabled = true;
                    _sharkEnabled = true;
                    _deepSeaEnabled = true;
                    _ui.ShowDeepSeaUnlocked();
                    break;
            }

            // Compute grid layout from camera
            float camSize = Camera.main.orthographicSize;
            float topMargin = 1.2f;
            float bottomMargin = 3.0f;
            float availH = (camSize * 2f) - topMargin - bottomMargin;
            _cellSize = Mathf.Min(availH / GridSize, Camera.main.aspect * camSize * 2f / GridSize, 2.0f);
            float gridW = _cellSize * GridSize;
            float gridH = _cellSize * GridSize;
            _gridOrigin = new Vector2(-gridW / 2f + _cellSize / 2f, -camSize + bottomMargin + _cellSize / 2f);

            // Rebuild grid visual
            foreach (var go in _cellObjects)
                if (go != null) Destroy(go);
            System.Array.Clear(_cellObjects, 0, _cellObjects.Length);
            System.Array.Clear(_cellRenderers, 0, _cellRenderers.Length);
            System.Array.Clear(_grid, 0, _grid.Length);

            for (int i = 0; i < GridSize * GridSize; i++)
            {
                int col = i % GridSize;
                int row = i / GridSize;
                var pos = new Vector3(_gridOrigin.x + col * _cellSize, _gridOrigin.y + row * _cellSize, 0f);
                var cell = new GameObject($"Cell_{i}");
                cell.transform.SetParent(_gridRoot.transform);
                cell.transform.position = pos;
                var sr = cell.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 2;
                // empty cell placeholder (faint)
                sr.color = new Color(1, 1, 1, 0.2f);
                var col2d = cell.AddComponent<BoxCollider2D>();
                col2d.size = new Vector2(_cellSize * 0.9f, _cellSize * 0.9f);
                _cellObjects[i] = cell;
                _cellRenderers[i] = sr;
            }

            // Reset state
            _coins = 20 + (stage - 1) * 10;
            _population = 0;
            _combo = 0;
            _comboTimer = 0f;
            _sharkActive = false;
            _sharkTimer = 0f;

            // Stop previous coroutines
            if (_autoCoroutine != null) StopCoroutine(_autoCoroutine);
            if (_fishSpawnCoroutine != null) StopCoroutine(_fishSpawnCoroutine);
            if (_sharkCoroutine != null) StopCoroutine(_sharkCoroutine);
            foreach (var fd in _fish) if (fd.obj != null) Destroy(fd.obj);
            _fish.Clear();

            _isActive = true;

            if (_autoCollectEnabled)
                _autoCoroutine = StartCoroutine(AutoCollectLoop());
            _fishSpawnCoroutine = StartCoroutine(FishSpawnLoop());

            _ui.UpdateCoins(_coins);
            _ui.UpdatePopulation(_population, _targetPopulation);
            _ui.UpdateCombo(0);
            _ui.ShowSharkWarning(false);
            _ui.UpdateShopAvailability(stage, _coins);
        }

        void RecalcPopulation()
        {
            long pop = 0;
            for (int i = 0; i < _grid.Length; i++)
            {
                if (_grid[i] == null) continue;
                int bt = (int)_grid[i].Value;
                long basePop = BuildingPop[bt];
                if (_adjacencyBonusEnabled)
                {
                    float bonus = GetAdjacencyBonus(i);
                    basePop = (long)(basePop * bonus);
                }
                pop += basePop;
            }
            _population = pop;
            _ui.UpdatePopulation(_population, _targetPopulation);
            if (_population >= _targetPopulation && _isActive)
            {
                _isActive = false;
                _gameManager.OnStageClear();
            }
        }

        float GetAdjacencyBonus(int idx)
        {
            if (!_adjacencyBonusEnabled) return 1f;
            if (_grid[idx] == null) return 1f;
            int col = idx % GridSize;
            int row = idx / GridSize;
            int[] neighbors = { idx - 1, idx + 1, idx - GridSize, idx + GridSize };
            int[] nCols = { col - 1, col + 1, col, col };
            int[] nRows = { row, row, row - 1, row + 1 };
            bool hasAdj = false;
            for (int n = 0; n < 4; n++)
            {
                int ni = neighbors[n];
                if (ni < 0 || ni >= _grid.Length) continue;
                if (nCols[n] < 0 || nCols[n] >= GridSize) continue;
                if (nRows[n] < 0 || nRows[n] >= GridSize) continue;
                if (_grid[ni] == _grid[idx]) { hasAdj = true; break; }
            }
            return hasAdj ? 1.5f : 1f;
        }

        void Update()
        {
            // Combo decay always runs (not gated by _isActive)
            if (_combo > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    _combo = 0;
                    _ui.UpdateCombo(0);
                }
            }

            if (!_isActive) return;

            // Shark timer
            if (_sharkEnabled && _isActive)
            {
                _sharkTimer += Time.deltaTime;
                if (!_sharkActive && _sharkTimer >= SharkInterval)
                    _sharkCoroutine = StartCoroutine(SharkAttack());
            }

            // Move fish
            float camSize = Camera.main.orthographicSize;
            float camW = camSize * Camera.main.aspect;
            foreach (var fd in _fish)
            {
                if (fd.obj == null || fd.tapped) continue;
                fd.obj.transform.position += (Vector3)(fd.vel * Time.deltaTime);
                var p = fd.obj.transform.position;
                if (p.x > camW + 1f) fd.vel.x = -Mathf.Abs(fd.vel.x);
                if (p.x < -camW - 1f) fd.vel.x = Mathf.Abs(fd.vel.x);
                if (p.y > camSize - 0.5f) fd.vel.y = -Mathf.Abs(fd.vel.y);
                if (p.y < -camSize + 3.5f) fd.vel.y = Mathf.Abs(fd.vel.y);
                // flip sprite
                fd.sr.flipX = fd.vel.x < 0;
            }

            // Input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                var worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                worldPos.z = 0f;
                HandleTap(worldPos);
            }
        }

        void HandleTap(Vector3 worldPos)
        {
            // Check fish first
            foreach (var fd in _fish)
            {
                if (fd.obj == null || fd.tapped) continue;
                float dist = Vector2.Distance(worldPos, fd.obj.transform.position);
                if (dist < 0.6f)
                {
                    TapFish(fd);
                    return;
                }
            }

            if (!_isActive) return;

            // Shark tap
            if (_sharkActive)
            {
                // shark is a special fish entry at index -1 stored as first fish with fishIndex = -1
                foreach (var fd in _fish)
                {
                    if (fd.obj == null || fd.fishIndex != -1) continue;
                    float dist = Vector2.Distance(worldPos, fd.obj.transform.position);
                    if (dist < 1.0f)
                    {
                        RepelShark(fd);
                        return;
                    }
                }
            }

            // Check building cells
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null)
            {
                for (int i = 0; i < _cellObjects.Length; i++)
                {
                    if (_cellObjects[i] == null) continue;
                    if (hit.gameObject == _cellObjects[i])
                    {
                        TapCell(i);
                        return;
                    }
                }
            }
        }

        void TapCell(int idx)
        {
            if (_grid[idx] == null) return;
            // Collect income
            int bt = (int)_grid[idx].Value;
            long income = BuildingIncome[bt];
            float comboMult = GetComboMult();
            income = (long)(income * comboMult * _countMultiplier);
            _coins += income;
            _ui.UpdateCoins(_coins);
            _ui.UpdateShopAvailability(_currentStage, _coins);
            StartCoroutine(CellPulse(idx, false));
            _ui.ShowFloatingText($"+{income}", _cellObjects[idx].transform.position);
        }

        void TapFish(FishData fd)
        {
            _combo++;
            _comboTimer = ComboWindow;
            _ui.UpdateCombo(_combo);

            long bonus = 5 * GetComboMult_int();
            _coins += bonus;
            _ui.UpdateCoins(_coins);
            _ui.UpdateShopAvailability(_currentStage, _coins);

            StartCoroutine(FishFlash(fd));
            _ui.ShowFloatingText($"+{bonus}", fd.obj.transform.position);
        }

        long GetComboMult_int()
        {
            if (_combo >= 5) return 5;
            if (_combo >= 4) return 3;
            if (_combo >= 3) return 2;
            if (_combo >= 2) return 2;
            return 1;
        }
        float GetComboMult()
        {
            if (_combo >= 5) return 5f;
            if (_combo >= 4) return 3f;
            if (_combo >= 3) return 2f;
            if (_combo >= 2) return 1.5f;
            return 1f;
        }

        IEnumerator AutoCollectLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f / _speedMultiplier);
                if (!_isActive || _gameManager.State != AquaCityGameManager.GameState.Playing) continue;
                long total = 0;
                for (int i = 0; i < _grid.Length; i++)
                {
                    if (_grid[i] == null) continue;
                    int bt = (int)_grid[i].Value;
                    total += BuildingIncome[bt];
                }
                if (total > 0)
                {
                    _coins += total;
                    _ui.UpdateCoins(_coins);
                    _ui.UpdateAutoRate(total, _speedMultiplier);
                    _ui.UpdateShopAvailability(_currentStage, _coins);
                }
            }
        }

        IEnumerator FishSpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(4f / _speedMultiplier);
                float camSize = Camera.main.orthographicSize;
                float camW = camSize * Camera.main.aspect;
                if (_fish.Count >= 5) continue;
                if (_fishSprites == null || _fishSprites.Length == 0) continue;
                int fi = Random.Range(0, Mathf.Min(_fishSprites.Length, _currentStage + 1));
                if (_fishSprites[fi] == null) continue;

                var go = new GameObject($"Fish_{fi}");
                go.transform.SetParent(transform);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _fishSprites[fi];
                sr.sortingOrder = 5;
                float scale = _cellSize * 0.5f / Mathf.Max(_fishSprites[fi].bounds.size.x, 0.01f);
                go.transform.localScale = new Vector3(scale, scale, 1f);

                float startX = Random.value > 0.5f ? camW + 0.5f : -camW - 0.5f;
                float startY = Random.Range(-camSize + 3.5f, camSize - 0.5f);
                go.transform.position = new Vector3(startX, startY, -1f);

                float spd = Random.Range(0.8f, 1.5f) * _speedMultiplier;
                var vel = new Vector2(startX > 0 ? -spd : spd, Random.Range(-0.3f, 0.3f));

                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;

                _fish.Add(new FishData { obj = go, sr = sr, vel = vel, fishIndex = fi });
            }
        }

        IEnumerator SharkAttack()
        {
            _sharkActive = true;
            _sharkTimer = 0f;
            _ui.ShowSharkWarning(true);
            StartCoroutine(CameraShake());

            if (_sharkSprite == null)
            {
                yield return new WaitForSeconds(SharkDuration);
                SharkLeave(null);
                yield break;
            }

            var go = new GameObject("Shark");
            go.transform.SetParent(transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sharkSprite;
            sr.color = new Color(1f, 0.3f, 0.3f, 1f);
            sr.sortingOrder = 6;
            float camW = Camera.main.orthographicSize * Camera.main.aspect;
            go.transform.position = new Vector3(camW + 1f, 0f, -0.5f);
            float scale = _cellSize * 0.8f / Mathf.Max(_sharkSprite.bounds.size.x, 0.01f);
            go.transform.localScale = new Vector3(scale, scale, 1f);

            var fd = new FishData { obj = go, sr = sr, vel = new Vector2(-1.5f, 0), fishIndex = -1 };
            _fish.Add(fd);

            yield return new WaitForSeconds(SharkDuration);
            if (!fd.tapped)
            {
                // Shark not repelled: lose some coins
                _coins = System.Math.Max(0, _coins - 50);
                _ui.UpdateCoins(_coins);
                _ui.ShowFloatingText("-50", go.transform.position);
            }
            SharkLeave(fd);
        }

        void RepelShark(FishData fd)
        {
            fd.tapped = true;
            _sharkActive = false;
            _sharkTimer = 0f;
            _ui.ShowSharkWarning(false);
            _coins += 20;
            _ui.UpdateCoins(_coins);
            _ui.ShowFloatingText("+20", fd.obj.transform.position);
            if (fd.obj != null) Destroy(fd.obj);
            _fish.Remove(fd);
        }

        void SharkLeave(FishData fd)
        {
            _sharkActive = false;
            _sharkTimer = 0f;
            _ui.ShowSharkWarning(false);
            if (fd != null)
            {
                _fish.Remove(fd);
                if (fd.obj != null) Destroy(fd.obj);
            }
        }

        public bool TryBuyBuilding(BuildingType type)
        {
            long cost = BuildingCost[(int)type];
            if (_coins < cost) return false;

            // Find empty cell
            int emptyIdx = -1;
            for (int i = 0; i < _grid.Length; i++)
            {
                if (_grid[i] == null) { emptyIdx = i; break; }
            }
            if (emptyIdx < 0) return false;

            _coins -= cost;
            PlaceBuilding(emptyIdx, type);
            _ui.UpdateCoins(_coins);
            _ui.UpdateShopAvailability(_currentStage, _coins);
            RecalcPopulation();
            return true;
        }

        void PlaceBuilding(int idx, BuildingType type)
        {
            _grid[idx] = type;
            var sr = _cellRenderers[idx];
            sr.color = Color.white;
            sr.sprite = GetSprite(type);
            float s = _cellSize * 0.85f / Mathf.Max(sr.sprite != null ? sr.sprite.bounds.size.x : 1f, 0.01f);
            _cellObjects[idx].transform.localScale = new Vector3(s, s, 1f);
            StartCoroutine(CellPulse(idx, true));
        }

        Sprite GetSprite(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => _houseSprite,
                BuildingType.Plaza => _plazaSprite,
                BuildingType.Coral => _coralSprite,
                BuildingType.Deco => _decoSprite,
                BuildingType.Aquarium => _aquariumSprite,
                BuildingType.DeepBase => _deepBaseSprite,
                _ => null
            };
        }

        IEnumerator CellPulse(int idx, bool big)
        {
            if (_cellObjects[idx] == null) yield break;
            var t = _cellObjects[idx].transform;
            var orig = t.localScale;
            float target = big ? 1.3f : 1.25f;
            float dur = 0.1f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(1f, target, elapsed / dur);
                t.localScale = orig * s;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(target, 1f, elapsed / dur);
                t.localScale = orig * s;
                yield return null;
            }
            t.localScale = orig;
        }

        IEnumerator FishFlash(FishData fd)
        {
            if (fd.obj == null) yield break;
            fd.sr.color = Color.yellow;
            yield return new WaitForSeconds(0.12f);
            fd.sr.color = Color.white;
            // Remove fish after tap
            fd.tapped = true;
            yield return new WaitForSeconds(0.3f);
            if (fd.obj != null) Destroy(fd.obj);
            _fish.Remove(fd);
        }

        IEnumerator CameraShake()
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            var origPos = cam.transform.localPosition;
            float dur = 0.4f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float str = 0.15f * (1f - t / dur);
                cam.transform.localPosition = origPos + (Vector3)Random.insideUnitCircle * str;
                yield return null;
            }
            cam.transform.localPosition = origPos;
        }

        void OnDestroy()
        {
            if (_autoCoroutine != null) StopCoroutine(_autoCoroutine);
            if (_fishSpawnCoroutine != null) StopCoroutine(_fishSpawnCoroutine);
            if (_sharkCoroutine != null) StopCoroutine(_sharkCoroutine);
        }
    }
}
