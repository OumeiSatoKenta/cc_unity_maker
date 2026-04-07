using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game086v2_CityBonsai
{
    public class CityBonsaiManager : MonoBehaviour
    {
        public enum BuildingType { None, House, Shop, Public, Shrine, Park }

        [System.Serializable]
        public class BranchSlot
        {
            public SpriteRenderer renderer;
            public BoxCollider2D collider;
            public BuildingType building;
            public bool hasFlower;
            public bool isDisabled;
            public int slotIndex;
        }

        [SerializeField] CityBonsaiGameManager _gameManager;
        [SerializeField] CityBonsaiUI _ui;

        [SerializeField] SpriteRenderer _trunkRenderer;
        [SerializeField] SpriteRenderer[] _slotRenderers;
        [SerializeField] BoxCollider2D[] _slotColliders;

        [SerializeField] Sprite[] _buildingSprites; // House, Shop, Public, Shrine, Park
        [SerializeField] Sprite _flowerSprite;
        [SerializeField] Sprite _emptySlotSprite;

        readonly List<BranchSlot> _slots = new List<BranchSlot>();
        BuildingType _selectedBuilding = BuildingType.House;
        bool _isPruningMode;
        bool _isActive;

        int _population;
        float _beauty;
        float _satisfaction;
        int _turn;

        // Stage params
        int _stageIndex;
        int _activeSlotCount;
        int _populationTarget;
        float _beautyTarget;
        int _availableBuildingTypes; // how many types unlocked
        bool _seasonEnabled;
        bool _disasterEnabled;
        bool _rivalEnabled;
        float _rivalPopBoost;
        float _rivalBeautyBoost;
        bool _demandEnabled;

        // Demand
        BuildingType _currentDemand;
        bool _hasDemand;

        // Season
        int _season; // 0=spring, 1=summer, 2=autumn, 3=winter
        static readonly string[] SeasonNames = { "春", "夏", "秋", "冬" };
        static readonly float[] SeasonBeautyMod = { 5f, 0f, 3f, -3f };

        static readonly int[] PopTargets = { 8, 15, 22, 30, 40 };
        static readonly float[] BeautyTargets = { 30f, 40f, 55f, 65f, 80f };
        static readonly int[] SlotCounts = { 6, 8, 9, 10, 12 };
        static readonly int[] BuildingTypeCounts = { 1, 2, 3, 5, 5 };

        // Building params: pop, satisfaction, beauty
        static readonly int[] BuildPop = { 0, 3, 0, 1, 2, 0 };
        static readonly float[] BuildSat = { 0, 0, 15, 5, 10, 20 };
        static readonly float[] BuildBeauty = { 0, -2, -3, 5, 10, 8 };

        void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            // Find which slot was tapped
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.collider == null) continue;
                if (hit.gameObject.name != slot.collider.gameObject.name) continue;
                if (slot.isDisabled) { _ui.ShowMessage("この枝は折れています！"); return; }

                if (_isPruningMode)
                    PruneSlot(slot);
                else
                    PlaceBuilding(slot);
                return;
            }
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _isActive = true;
            _isPruningMode = false;
            _selectedBuilding = BuildingType.House;
            _turn = 0;
            _season = 0;

            _activeSlotCount = SlotCounts[Mathf.Clamp(stageIndex, 0, 4)];
            _populationTarget = PopTargets[Mathf.Clamp(stageIndex, 0, 4)];
            _beautyTarget = BeautyTargets[Mathf.Clamp(stageIndex, 0, 4)];
            _availableBuildingTypes = BuildingTypeCounts[Mathf.Clamp(stageIndex, 0, 4)];

            _seasonEnabled = stageIndex >= 2;
            _disasterEnabled = stageIndex >= 3;
            _rivalEnabled = stageIndex >= 4;
            _demandEnabled = stageIndex >= 1;
            _rivalPopBoost = 0;
            _rivalBeautyBoost = 0;
            _hasDemand = false;

            _population = 0;
            _beauty = 10f; // Start with base beauty
            _satisfaction = 50f;

            // Initialize slots
            _slots.Clear();
            for (int i = 0; i < _slotRenderers.Length; i++)
            {
                bool active = i < _activeSlotCount;
                if (_slotRenderers[i] != null)
                {
                    _slotRenderers[i].gameObject.SetActive(active);
                    if (active)
                    {
                        _slotRenderers[i].sprite = _emptySlotSprite;
                        _slotRenderers[i].color = Color.white;
                    }
                }
                if (_slotColliders[i] != null)
                    _slotColliders[i].enabled = active;

                if (active)
                {
                    _slots.Add(new BranchSlot
                    {
                        renderer = _slotRenderers[i],
                        collider = _slotColliders[i],
                        building = BuildingType.None,
                        hasFlower = false,
                        isDisabled = false,
                        slotIndex = i
                    });
                }
            }

            PositionSlots();
            UpdateBuildingButtons();
            RefreshUI();

            if (_demandEnabled)
                GenerateDemand();
        }

        void PositionSlots()
        {
            if (Camera.main == null) return;
            float camSize = Camera.main.orthographicSize;
            float centerY = camSize * 0.15f;
            float radius = camSize * 0.45f;
            float slotScale = Mathf.Min(camSize * 0.15f, 0.8f);

            if (_trunkRenderer != null)
            {
                _trunkRenderer.transform.localPosition = new Vector3(0, centerY, 0);
                float trunkScale = camSize * 0.18f;
                _trunkRenderer.transform.localScale = new Vector3(trunkScale, trunkScale, 1f);
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                float angle = (360f / _slots.Count) * i - 90f;
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Cos(rad) * radius;
                float y = Mathf.Sin(rad) * radius + centerY;

                if (_slots[i].renderer != null)
                {
                    _slots[i].renderer.transform.localPosition = new Vector3(x, y, 0);
                    _slots[i].renderer.transform.localScale = new Vector3(slotScale, slotScale, 1f);
                }
                if (_slots[i].collider != null)
                {
                    _slots[i].collider.size = new Vector2(1.2f, 1.2f);
                }
            }
        }

        void PlaceBuilding(BranchSlot slot)
        {
            if (slot.building != BuildingType.None)
            {
                _ui.ShowMessage("すでに建物があります！");
                return;
            }
            if (slot.hasFlower)
            {
                // Remove flower to place building
                slot.hasFlower = false;
                _beauty -= 5f;
            }

            slot.building = _selectedBuilding;
            int typeIdx = (int)_selectedBuilding;

            // Apply building effects
            _population += BuildPop[typeIdx];
            _satisfaction = Mathf.Clamp(_satisfaction + BuildSat[typeIdx], 0, 100);
            _beauty = Mathf.Clamp(_beauty + BuildBeauty[typeIdx], 0, 100);

            // Adjacency bonus
            float adjBonus = CalculateAdjacencyBonus(slot);
            if (adjBonus > 1f)
            {
                int extraPop = Mathf.RoundToInt((BuildPop[typeIdx]) * (adjBonus - 1f));
                _population += extraPop;
                if (extraPop > 0) _ui.ShowMessage($"隣接ボーナス！ 人口+{extraPop}");
            }

            // Update sprite
            UpdateSlotVisual(slot);

            // Score
            _gameManager.OnActionScore(10, false);

            // Check demand
            if (_hasDemand && _selectedBuilding == _currentDemand)
            {
                _satisfaction = Mathf.Clamp(_satisfaction + 20f, 0, 100);
                _gameManager.OnActionScore(20, false);
                _ui.ShowMessage("要望達成！ 満足度+20");
                _hasDemand = false;
            }

            // Visual feedback - scale pulse
            if (slot.renderer != null)
                StartCoroutine(PulseSlot(slot.renderer, new Color(0.5f, 1f, 0.5f)));

            RefreshUI();
            CheckGoals();
        }

        void PruneSlot(BranchSlot slot)
        {
            float beautyGain;
            if (slot.building != BuildingType.None)
            {
                // Prune with building → high risk high reward
                int typeIdx = (int)slot.building;
                _population = Mathf.Max(0, _population - BuildPop[typeIdx]);
                _satisfaction = Mathf.Clamp(_satisfaction - 10f, 0, 100);
                beautyGain = 10f;
                slot.building = BuildingType.None;
            }
            else
            {
                beautyGain = 5f;
            }

            _beauty = Mathf.Clamp(_beauty + beautyGain, 0, 100);
            slot.hasFlower = true;

            // Update visual
            UpdateSlotVisual(slot);

            _gameManager.OnActionScore(5, true);

            // Visual feedback
            if (slot.renderer != null)
                StartCoroutine(PruneAnimation(slot.renderer));

            RefreshUI();
            CheckGoals();
        }

        public void AdvanceTurn()
        {
            if (!_isActive) return;

            _turn++;

            // Satisfaction decay per turn
            _satisfaction = Mathf.Clamp(_satisfaction - 3f, 0, 100);

            // Season effects
            if (_seasonEnabled)
            {
                _season = _turn % 4;
                _beauty = Mathf.Clamp(_beauty + SeasonBeautyMod[_season], 0, 100);
                _ui.ShowMessage($"季節: {SeasonNames[_season]}");
            }

            // Disaster
            if (_disasterEnabled && Random.value < 0.25f)
            {
                TriggerDisaster();
            }

            // Rival
            if (_rivalEnabled)
            {
                _rivalPopBoost += 1f;
                _rivalBeautyBoost += 1f;
            }

            // Generate new demand
            if (_demandEnabled && !_hasDemand && Random.value < 0.5f)
            {
                GenerateDemand();
            }

            // Turn survival score
            _gameManager.OnActionScore(3, false);

            RefreshUI();

            // Check game over
            if (_satisfaction <= 0f)
            {
                _gameManager.OnGameOver();
                return;
            }

            CheckGoals();
        }

        void TriggerDisaster()
        {
            // Find a random active non-disabled slot
            var candidates = new List<BranchSlot>();
            foreach (var s in _slots)
                if (!s.isDisabled) candidates.Add(s);

            if (candidates.Count <= 2) return; // Don't destroy if too few

            var target = candidates[Random.Range(0, candidates.Count)];
            target.isDisabled = true;

            if (target.building != BuildingType.None)
            {
                int typeIdx = (int)target.building;
                _population = Mathf.Max(0, _population - BuildPop[typeIdx]);
                target.building = BuildingType.None;
            }
            target.hasFlower = false;

            if (target.renderer != null)
            {
                target.renderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                StartCoroutine(CameraShake());
            }

            _ui.ShowMessage("台風！ 枝が折れた！");
            RefreshUI();
        }

        void GenerateDemand()
        {
            // Generate demand for an available building type
            BuildingType[] types = { BuildingType.House, BuildingType.Shop, BuildingType.Public, BuildingType.Shrine, BuildingType.Park };
            int maxType = Mathf.Min(_availableBuildingTypes, types.Length);
            _currentDemand = types[Random.Range(0, maxType)];
            _hasDemand = true;
            string demandName = GetBuildingName(_currentDemand);
            _ui.UpdateDemand($"要望: {demandName}がほしい！");
        }

        float CalculateAdjacencyBonus(BranchSlot slot)
        {
            int idx = _slots.IndexOf(slot);
            if (idx < 0) return 1f;

            int prevIdx = (idx - 1 + _slots.Count) % _slots.Count;
            int nextIdx = (idx + 1) % _slots.Count;

            int sameCount = 0;
            if (_slots[prevIdx].building == slot.building) sameCount++;
            if (_slots[nextIdx].building == slot.building) sameCount++;

            return sameCount > 0 ? 1.2f : 1.0f;
        }

        void UpdateSlotVisual(BranchSlot slot)
        {
            if (slot.renderer == null) return;

            if (slot.isDisabled)
            {
                slot.renderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                return;
            }

            if (slot.hasFlower && slot.building == BuildingType.None)
            {
                slot.renderer.sprite = _flowerSprite;
                slot.renderer.color = Color.white;
            }
            else if (slot.building != BuildingType.None)
            {
                int spriteIdx = (int)slot.building - 1; // -1 because None=0
                if (spriteIdx >= 0 && spriteIdx < _buildingSprites.Length)
                    slot.renderer.sprite = _buildingSprites[spriteIdx];
                slot.renderer.color = Color.white;
            }
            else
            {
                slot.renderer.sprite = _emptySlotSprite;
                slot.renderer.color = Color.white;
            }
        }

        void UpdateBuildingButtons()
        {
            string[] names = { "住宅", "商業", "公共", "神社", "公園" };
            for (int i = 0; i < 5; i++)
            {
                bool unlocked = i < _availableBuildingTypes;
                _ui.SetBuildingButton(i, unlocked, names[i]);
            }
        }

        void RefreshUI()
        {
            int effectivePop = _population;
            float effectiveBeauty = _beauty;
            int popTarget = _populationTarget + Mathf.RoundToInt(_rivalPopBoost);
            float beautyTarget = _beautyTarget + _rivalBeautyBoost;

            _ui.UpdatePopulation(effectivePop, popTarget);
            _ui.UpdateBeauty(effectiveBeauty, beautyTarget);
            _ui.UpdateSatisfaction(_satisfaction);
            _ui.UpdateTurn(_turn);

            if (_seasonEnabled)
                _ui.UpdateSeason(SeasonNames[_season]);

            if (!_hasDemand)
                _ui.UpdateDemand("");
        }

        void CheckGoals()
        {
            // Check if all slots disabled → unwinnable → game over
            int enabledSlots = 0;
            foreach (var s in _slots)
                if (!s.isDisabled) enabledSlots++;
            if (enabledSlots == 0)
            {
                _gameManager.OnGameOver();
                return;
            }

            int popTarget = _populationTarget + Mathf.RoundToInt(_rivalPopBoost);
            float beautyTarget = _beautyTarget + _rivalBeautyBoost;

            if (_population >= popTarget && _beauty >= beautyTarget)
            {
                _gameManager.OnBothGoalsMet();
            }
        }

        public void SelectBuilding(int typeIndex)
        {
            if (!_isActive) return;
            // typeIndex: 0=House, 1=Shop, 2=Public, 3=Shrine, 4=Park
            BuildingType[] types = { BuildingType.House, BuildingType.Shop, BuildingType.Public, BuildingType.Shrine, BuildingType.Park };
            if (typeIndex >= 0 && typeIndex < types.Length)
            {
                _selectedBuilding = types[typeIndex];
                _isPruningMode = false;
                _ui.SetPruneMode(false);
                _ui.HighlightBuildingButton(typeIndex);
            }
        }

        public void TogglePruneMode()
        {
            if (!_isActive) return;
            _isPruningMode = !_isPruningMode;
            _ui.SetPruneMode(_isPruningMode);
        }

        static string GetBuildingName(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.House: return "住宅";
                case BuildingType.Shop: return "商業";
                case BuildingType.Public: return "公共";
                case BuildingType.Shrine: return "神社";
                case BuildingType.Park: return "公園";
                default: return "";
            }
        }

        public void SetActive(bool active) => _isActive = active;

        // Visual feedback coroutines
        IEnumerator PulseSlot(SpriteRenderer sr, Color flashColor)
        {
            if (sr == null) yield break;
            Vector3 orig = sr.transform.localScale;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.2f;
                float scale = ratio < 0.5f ? Mathf.Lerp(1f, 1.3f, ratio * 2f) : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                sr.transform.localScale = orig * scale;
                sr.color = Color.Lerp(Color.white, flashColor, Mathf.Sin(ratio * Mathf.PI));
                yield return null;
            }
            sr.transform.localScale = orig;
            sr.color = Color.white;
        }

        IEnumerator PruneAnimation(SpriteRenderer sr)
        {
            if (sr == null) yield break;
            // Red flash
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                sr.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), Color.white, elapsed / 0.15f);
                yield return null;
            }
            sr.color = Color.white;

            // Flower bloom (scale 0 → 1)
            Vector3 orig = sr.transform.localScale;
            sr.transform.localScale = Vector3.zero;
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                // Overshoot easing
                float scale = t < 0.7f ? Mathf.Lerp(0f, 1.15f, t / 0.7f) : Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f);
                sr.transform.localScale = orig * scale;
                yield return null;
            }
            sr.transform.localScale = orig;
        }

        IEnumerator CameraShake()
        {
            if (Camera.main == null) yield break;
            var camT = Camera.main.transform;
            Vector3 origPos = camT.position;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float intensity = (1f - elapsed / 0.3f) * 0.15f;
                camT.position = origPos + new Vector3(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity), 0);
                yield return null;
            }
            camT.position = origPos;
        }

        void OnDestroy()
        {
            // No dynamic textures to clean up
        }
    }
}
