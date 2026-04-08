using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game089v2_IslandHop
{
    public enum FacilityType
    {
        None = 0,
        Cottage = 1,
        Pier = 2,
        Garden = 3,
        Restaurant = 4,
        Observation = 5,
        Spa = 6,
        Marina = 7,
        Hotel = 8,
        Lighthouse = 9,
        Casino = 10,
        Aquarium = 11
    }

    public enum IslandType { Forest, Rocky, Tropical, Volcanic, Coral }

    [System.Serializable]
    public class FacilityData
    {
        public FacilityType type;
        public string displayName;
        public int woodCost;
        public int stoneCost;
        public int baseScore;
        public FacilityType[] synergyWith;

        public static FacilityData[] All = new FacilityData[]
        {
            new FacilityData { type = FacilityType.Cottage,      displayName = "コテージ",   woodCost = 2, stoneCost = 1, baseScore = 15, synergyWith = new[]{FacilityType.Garden} },
            new FacilityData { type = FacilityType.Pier,         displayName = "桟橋",       woodCost = 1, stoneCost = 2, baseScore = 10, synergyWith = new[]{FacilityType.Restaurant, FacilityType.Marina} },
            new FacilityData { type = FacilityType.Garden,       displayName = "花壇",       woodCost = 1, stoneCost = 0, baseScore = 10, synergyWith = new[]{FacilityType.Cottage, FacilityType.Spa} },
            new FacilityData { type = FacilityType.Restaurant,   displayName = "レストラン", woodCost = 3, stoneCost = 2, baseScore = 30, synergyWith = new[]{FacilityType.Pier, FacilityType.Hotel} },
            new FacilityData { type = FacilityType.Observation,  displayName = "展望台",     woodCost = 2, stoneCost = 3, baseScore = 25, synergyWith = new[]{FacilityType.Spa} },
            new FacilityData { type = FacilityType.Spa,          displayName = "スパ",       woodCost = 3, stoneCost = 2, baseScore = 30, synergyWith = new[]{FacilityType.Garden, FacilityType.Observation} },
            new FacilityData { type = FacilityType.Marina,       displayName = "マリーナ",   woodCost = 2, stoneCost = 4, baseScore = 35, synergyWith = new[]{FacilityType.Pier, FacilityType.Aquarium} },
            new FacilityData { type = FacilityType.Hotel,        displayName = "ホテル",     woodCost = 5, stoneCost = 5, baseScore = 50, synergyWith = new[]{FacilityType.Restaurant} },
            new FacilityData { type = FacilityType.Lighthouse,   displayName = "灯台",       woodCost = 3, stoneCost = 4, baseScore = 20, synergyWith = new[]{FacilityType.Pier, FacilityType.Marina} },
            new FacilityData { type = FacilityType.Casino,       displayName = "カジノ",     woodCost = 8, stoneCost = 6, baseScore = 80, synergyWith = new[]{FacilityType.Hotel} },
            new FacilityData { type = FacilityType.Aquarium,     displayName = "水族館",     woodCost = 5, stoneCost = 5, baseScore = 60, synergyWith = new[]{FacilityType.Marina, FacilityType.Pier} },
        };

        public static FacilityData Get(FacilityType t)
        {
            foreach (var d in All) if (d.type == t) return d;
            return null;
        }
    }

    public class IslandManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] IslandHopGameManager _gameManager;
        [SerializeField] IslandHopUI _ui;

        [Header("Sprites - Islands")]
        [SerializeField] Sprite _sprIslandForest;
        [SerializeField] Sprite _sprIslandRocky;
        [SerializeField] Sprite _sprIslandTropical;
        [SerializeField] Sprite _sprIslandVolcanic;
        [SerializeField] Sprite _sprIslandCoral;

        [Header("Sprites - Facilities")]
        [SerializeField] Sprite _sprCottage;
        [SerializeField] Sprite _sprPier;
        [SerializeField] Sprite _sprGarden;
        [SerializeField] Sprite _sprRestaurant;
        [SerializeField] Sprite _sprObservation;
        [SerializeField] Sprite _sprSpa;
        [SerializeField] Sprite _sprMarina;
        [SerializeField] Sprite _sprHotel;
        [SerializeField] Sprite _sprLighthouse;
        [SerializeField] Sprite _sprCasino;
        [SerializeField] Sprite _sprAquarium;
        [SerializeField] Sprite _sprSlotEmpty;

        [Header("Sprites - Resources")]
        [SerializeField] Sprite _sprWood;
        [SerializeField] Sprite _sprStone;
        [SerializeField] Sprite _sprFood;
        [SerializeField] Sprite _sprGold;

        // Runtime state
        bool _isActive;
        int _stageIndex;
        int _maxIslands;
        int _slotsPerIsland;
        int _availableFacilityCount; // how many facility types available
        bool _hasGuestSystem;
        bool _hasWeatherSystem;
        float _resourceSpeed;

        // Resources
        int _wood, _stone, _food, _gold;

        // Island objects
        IslandData[] _islands;
        int _selectedIslandIndex = -1;
        int _selectedSlotIndex = -1;
        bool _showingBuildPanel;

        // Guest requests (stage 3+)
        FacilityType _currentGuestRequest = FacilityType.None;
        float _guestRequestTimer;

        // Weather (stage 4+)
        float _weatherTimer;
        bool _weatherWarningActive;
        bool _hasDefense; // lighthouse counts as defense

        // Coroutine references for cleanup
        List<Coroutine> _activeCoroutines = new List<Coroutine>();

        // Cached camera reference
        Camera _mainCamera;

        // Stage target scores
        static readonly int[] StageTargets = { 50, 120, 200, 300, 420 };

        // Island positions per stage (world coords)
        static readonly Vector2[][] IslandPositions =
        {
            new Vector2[]{ new(0f, 1f) },
            new Vector2[]{ new(-2.5f, 1f), new(2.5f, 1f) },
            new Vector2[]{ new(-3f, 1.5f), new(0f, 2.5f), new(3f, 1.5f) },
            new Vector2[]{ new(-3f, 2f), new(-1f, 0.5f), new(1f, 0.5f), new(3f, 2f) },
            new Vector2[]{ new(-3.5f, 2f), new(-1.5f, 0f), new(0f, 2.5f), new(1.5f, 0f), new(3.5f, 2f) },
        };

        static readonly IslandType[] IslandTypes = {
            IslandType.Forest, IslandType.Rocky, IslandType.Tropical,
            IslandType.Volcanic, IslandType.Coral
        };

        class IslandData
        {
            public GameObject root;
            public SpriteRenderer islandSR;
            public FacilityType[] slots;
            public GameObject[] slotObjects;
            public SpriteRenderer[] slotSRs;
            public IslandType type;
            public int index;
        }

        public void SetActive(bool v) { _isActive = v; }

        public int GetStageTargetScore(int stageIdx)
        {
            if (stageIdx < 0 || stageIdx >= StageTargets.Length) return 9999;
            return StageTargets[stageIdx];
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            // Cache camera reference
            _mainCamera = Camera.main;

            // Clear old islands
            if (_islands != null)
            {
                foreach (var island in _islands)
                    if (island?.root != null) Destroy(island.root);
            }
            foreach (var c in _activeCoroutines) if (c != null) StopCoroutine(c);
            _activeCoroutines.Clear();

            _stageIndex = stageIndex;
            _maxIslands = stageIndex + 1;
            _slotsPerIsland = 3 + stageIndex;                   // 3→4→5→6→7
            _availableFacilityCount = 3 + stageIndex * 2;       // 3→5→7→9→11
            _hasGuestSystem = stageIndex >= 2;
            _hasWeatherSystem = stageIndex >= 3;
            _resourceSpeed = config.speedMultiplier;

            // Initial resources (more in early stages to be generous)
            _wood  = 8 + (4 - stageIndex) * 2;
            _stone = 6 + (4 - stageIndex) * 2;
            _food  = 5;
            _gold  = stageIndex >= 3 ? 3 : 0;

            _selectedIslandIndex = -1;
            _selectedSlotIndex = -1;
            _showingBuildPanel = false;
            _currentGuestRequest = FacilityType.None;
            _weatherWarningActive = false;
            _hasDefense = false;

            BuildIslands();
            UpdateResourceUI();

            _isActive = true;
            _activeCoroutines.Add(StartCoroutine(ResourceGenLoop()));
            if (_hasGuestSystem) _activeCoroutines.Add(StartCoroutine(GuestRequestLoop()));
            if (_hasWeatherSystem) _activeCoroutines.Add(StartCoroutine(WeatherEventLoop()));
        }

        void BuildIslands()
        {
            var positions = IslandPositions[Mathf.Min(_stageIndex, IslandPositions.Length - 1)];
            _maxIslands = Mathf.Min(_maxIslands, positions.Length); // guard against array bounds
            _islands = new IslandData[_maxIslands];

            for (int i = 0; i < _maxIslands; i++)
            {
                var data = new IslandData();
                data.type = IslandTypes[i % IslandTypes.Length];
                data.index = i;
                data.slots = new FacilityType[_slotsPerIsland];
                data.slotObjects = new GameObject[_slotsPerIsland];
                data.slotSRs = new SpriteRenderer[_slotsPerIsland];

                // Island root
                data.root = new GameObject($"Island_{i}");
                data.root.transform.position = new Vector3(positions[i].x, positions[i].y, 0f);

                // Island sprite
                var islandObj = new GameObject("IslandSprite");
                islandObj.transform.SetParent(data.root.transform);
                islandObj.transform.localPosition = Vector3.zero;
                data.islandSR = islandObj.AddComponent<SpriteRenderer>();
                data.islandSR.sprite = GetIslandSprite(data.type);
                data.islandSR.sortingOrder = 0;

                var col = islandObj.AddComponent<CircleCollider2D>();
                col.radius = 0.8f;

                // Facility slots arranged around island
                for (int s = 0; s < _slotsPerIsland; s++)
                {
                    float angle = (s / (float)_slotsPerIsland) * 360f - 90f;
                    float rad = Mathf.Deg2Rad * angle;
                    float slotRadius = 1.1f;
                    Vector3 slotPos = new Vector3(
                        Mathf.Cos(rad) * slotRadius,
                        Mathf.Sin(rad) * slotRadius * 0.5f,
                        0f
                    );
                    var slotObj = new GameObject($"Slot_{s}");
                    slotObj.transform.SetParent(data.root.transform);
                    slotObj.transform.localPosition = slotPos;

                    var sr = slotObj.AddComponent<SpriteRenderer>();
                    sr.sprite = _sprSlotEmpty;
                    sr.sortingOrder = 1;
                    slotObj.transform.localScale = Vector3.one * 0.35f;

                    var sc = slotObj.AddComponent<BoxCollider2D>();
                    sc.size = Vector2.one * 2.8f;

                    data.slotObjects[s] = slotObj;
                    data.slotSRs[s] = sr;
                    data.slots[s] = FacilityType.None;
                }

                _islands[i] = data;
            }
        }

        void Update()
        {
            if (!_isActive) return;
            HandleInput();
        }

        void HandleInput()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
            if (_mainCamera == null) { _mainCamera = Camera.main; if (_mainCamera == null) return; }

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            worldPos.z = 0f;

            // If build panel showing, let UI handle it
            if (_showingBuildPanel) return;

            // Check slot hits
            for (int i = 0; i < _islands.Length; i++)
            {
                var island = _islands[i];
                for (int s = 0; s < island.slotObjects.Length; s++)
                {
                    var col = island.slotObjects[s].GetComponent<Collider2D>();
                    if (col != null && col.OverlapPoint(worldPos))
                    {
                        _selectedIslandIndex = i;
                        _selectedSlotIndex = s;
                        if (island.slots[s] == FacilityType.None)
                        {
                            ShowBuildPanel(i, s);
                        }
                        return;
                    }
                }
            }

            // Check island hits (for selection highlight)
            for (int i = 0; i < _islands.Length; i++)
            {
                var col = _islands[i].root.GetComponentInChildren<Collider2D>();
                if (col != null && col.OverlapPoint(worldPos))
                {
                    _selectedIslandIndex = i;
                    _selectedSlotIndex = -1;
                    HighlightIsland(i);
                    return;
                }
            }

            // Clicked elsewhere, deselect
            _selectedIslandIndex = -1;
            _selectedSlotIndex = -1;
        }

        void ShowBuildPanel(int islandIdx, int slotIdx)
        {
            _showingBuildPanel = true;
            // Get buildable facilities (within available count, and we can afford)
            var buildable = new List<(FacilityType type, string name, int wood, int stone, bool canAfford)>();
            for (int f = 0; f < Mathf.Min(_availableFacilityCount, FacilityData.All.Length); f++)
            {
                var fd = FacilityData.All[f];
                bool afford = _wood >= fd.woodCost && _stone >= fd.stoneCost;
                buildable.Add((fd.type, fd.displayName, fd.woodCost, fd.stoneCost, afford));
            }
            _ui.ShowBuildPanel(buildable, (FacilityType chosen) =>
            {
                _showingBuildPanel = false;
                if (chosen != FacilityType.None)
                    TryBuild(islandIdx, slotIdx, chosen);
                _ui.HideBuildPanel();
            });
        }

        void TryBuild(int islandIdx, int slotIdx, FacilityType facility)
        {
            var fd = FacilityData.Get(facility);
            if (fd == null) return;
            if (_wood < fd.woodCost || _stone < fd.stoneCost)
            {
                _ui.ShowNotEnoughResources();
                return;
            }

            _wood -= fd.woodCost;
            _stone -= fd.stoneCost;
            UpdateResourceUI();

            var island = _islands[islandIdx];
            island.slots[slotIdx] = facility;
            island.slotSRs[slotIdx].sprite = GetFacilitySprite(facility);
            island.slotSRs[slotIdx].color = Color.white;

            // Defense check
            if (facility == FacilityType.Lighthouse) _hasDefense = true;

            // Synergy count
            int synergyCount = CountSynergies(island, slotIdx, facility);
            bool hasSynergy = synergyCount > 0;

            // Guest request fulfillment
            if (_hasGuestSystem && _currentGuestRequest == facility)
            {
                _currentGuestRequest = FacilityType.None;
                _gameManager.OnGuestRequestFulfilled(30);
                _ui.HideGuestRequest();
            }

            // Visual feedback (tracked for cleanup on stage transition)
            _activeCoroutines.Add(StartCoroutine(BuildPulse(island.slotObjects[slotIdx].transform, hasSynergy)));
            if (hasSynergy) _activeCoroutines.Add(StartCoroutine(SynergyEffect(island, slotIdx, facility)));

            _gameManager.OnFacilityBuilt(fd.baseScore, hasSynergy, synergyCount);

            // Check game over (no more buildable slots + not enough resources for any facility)
            CheckGameOver();
        }

        int CountSynergies(IslandData island, int placedSlot, FacilityType placed)
        {
            var fd = FacilityData.Get(placed);
            if (fd == null) return 0;
            int count = 0;
            // Check adjacent slots (±1 in circular arrangement)
            int n = island.slots.Length;
            int prev = (placedSlot - 1 + n) % n;
            int next = (placedSlot + 1) % n;
            foreach (int adj in new[]{prev, next})
            {
                FacilityType neighbor = island.slots[adj];
                if (neighbor == FacilityType.None) continue;
                foreach (var syn in fd.synergyWith)
                    if (syn == neighbor) { count++; break; }
            }
            return count;
        }

        void CheckGameOver()
        {
            // If all slots are filled and the target score hasn't been reached → game over
            bool hasEmptySlot = false;
            foreach (var island in _islands)
                foreach (var slot in island.slots)
                    if (slot == FacilityType.None) hasEmptySlot = true;

            if (hasEmptySlot) return;

            // All slots filled; check if stage target was met (if not, it's a dead end)
            int currentScore = _gameManager != null
                ? _gameManager.GetCurrentScore()
                : 0;
            int targetScore = GetStageTargetScore(_stageIndex);
            if (currentScore < targetScore)
            {
                _gameManager?.OnGameOver();
            }
        }

        IEnumerator ResourceGenLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(3f / _resourceSpeed);
                if (!_isActive) yield break;
                _wood += 1;
                _stone += 1;
                if (_stageIndex >= 2) _food += 1;
                if (_stageIndex >= 3) _gold += 1;
                UpdateResourceUI();
            }
        }

        IEnumerator GuestRequestLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(8f);
                if (!_isActive || !_hasGuestSystem) yield break;
                if (_currentGuestRequest == FacilityType.None)
                {
                    // Pick a random available facility type as request
                    int idx = Random.Range(0, Mathf.Min(_availableFacilityCount, FacilityData.All.Length));
                    _currentGuestRequest = FacilityData.All[idx].type;
                    _ui.ShowGuestRequest(_currentGuestRequest, FacilityData.Get(_currentGuestRequest)?.displayName ?? "");
                }
            }
        }

        IEnumerator WeatherEventLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(15f);
                if (!_isActive || !_hasWeatherSystem) yield break;

                // Show warning
                _weatherWarningActive = true;
                _ui.ShowWeatherWarning(5f);
                yield return new WaitForSeconds(5f);
                if (!_isActive) yield break;

                _weatherWarningActive = false;
                if (!_hasDefense)
                {
                    _gameManager.OnWeatherPenalty(30);
                }
                else
                {
                    _ui.ShowWeatherBlocked();
                }
            }
        }

        IEnumerator BuildPulse(Transform t, bool hasSynergy)
        {
            float duration = 0.25f;
            float elapsed = 0f;
            Vector3 orig = t.localScale;
            float peakScale = hasSynergy ? 1.5f : 1.3f;
            Color flashColor = hasSynergy ? new Color(1f, 0.9f, 0.2f) : new Color(0.8f, 1f, 0.8f);

            var sr = t.GetComponent<SpriteRenderer>();

            while (elapsed < duration)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float ratio = elapsed / duration;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, peakScale, ratio * 2f)
                    : Mathf.Lerp(peakScale, 1f, (ratio - 0.5f) * 2f);
                t.localScale = orig * scale;
                if (sr != null) sr.color = Color.Lerp(flashColor, Color.white, ratio);
                yield return null;
            }
            if (t != null) t.localScale = orig;
            if (sr != null) sr.color = Color.white;
        }

        IEnumerator SynergyEffect(IslandData island, int slotIdx, FacilityType placed)
        {
            // Flash adjacent synergy slots
            int n = island.slots.Length;
            int prev = (slotIdx - 1 + n) % n;
            int next = (slotIdx + 1) % n;

            foreach (int adj in new[]{prev, next})
            {
                if (island.slots[adj] == FacilityType.None) continue;
                var adjSR = island.slotSRs[adj];
                if (adjSR == null) continue;
                float t = 0f;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    adjSR.color = Color.Lerp(new Color(1f, 0.85f, 0.1f), Color.white, t / 0.3f);
                    yield return null;
                }
                adjSR.color = Color.white;
            }
        }

        void HighlightIsland(int idx)
        {
            for (int i = 0; i < _islands.Length; i++)
                _islands[i].islandSR.color = i == idx ? new Color(1.2f, 1.2f, 0.8f) : Color.white;
        }

        void UpdateResourceUI()
        {
            _ui?.UpdateResources(_wood, _stone, _food, _gold);
        }

        Sprite GetIslandSprite(IslandType t)
        {
            return t switch
            {
                IslandType.Forest   => _sprIslandForest,
                IslandType.Rocky    => _sprIslandRocky,
                IslandType.Tropical => _sprIslandTropical,
                IslandType.Volcanic => _sprIslandVolcanic,
                IslandType.Coral    => _sprIslandCoral,
                _ => _sprIslandForest
            };
        }

        Sprite GetFacilitySprite(FacilityType t)
        {
            return t switch
            {
                FacilityType.Cottage     => _sprCottage,
                FacilityType.Pier        => _sprPier,
                FacilityType.Garden      => _sprGarden,
                FacilityType.Restaurant  => _sprRestaurant,
                FacilityType.Observation => _sprObservation,
                FacilityType.Spa         => _sprSpa,
                FacilityType.Marina      => _sprMarina,
                FacilityType.Hotel       => _sprHotel,
                FacilityType.Lighthouse  => _sprLighthouse,
                FacilityType.Casino      => _sprCasino,
                FacilityType.Aquarium    => _sprAquarium,
                _ => _sprSlotEmpty
            };
        }

        void OnDestroy()
        {
            foreach (var c in _activeCoroutines) if (c != null) StopCoroutine(c);
        }
    }
}
