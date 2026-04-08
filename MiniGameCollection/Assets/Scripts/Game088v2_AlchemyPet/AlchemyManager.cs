using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Game088v2_AlchemyPet
{
    /// <summary>
    /// コアメカニクス: 素材管理・合成ロジック・ペット育成・5ステージ対応
    /// </summary>
    public class AlchemyManager : MonoBehaviour
    {
        [SerializeField] AlchemyPetGameManager _gameManager;
        [SerializeField] AlchemyPetUI _ui;
        [SerializeField] Button[] _materialButtons;
        [SerializeField] Button[] _clearSlotButtons;
        [SerializeField] Button _combineButton;
        [SerializeField] Button _feedButton;

        // --- Material definitions ---
        public static readonly string[] MaterialNames = {
            "fire", "water", "earth", "wind",       // Stage1  (0-3)
            "flame", "ice", "sand", "storm",         // Stage2  (4-7)
            "rock", "thunder", "poison", "light",    // Stage3  (8-11)
            "bomb", "powder",                         // Stage4  (12-13)
            "stardust", "scale"                       // Stage5  (14-15)
        };

        // --- Pet recipe database ---
        // Key: sorted material IDs joined by '_'. Value: PetDefinition
        public struct PetDefinition
        {
            public int id;
            public string name;
            public bool isRare;      // uses dangerous material
            public bool isLegend;    // requires combo x3+
            public Sprite sprite;
        }

        // Inventory: materialId -> count
        readonly Dictionary<int, int> _inventory = new();
        // Discovered pets: petId -> count discovered
        readonly HashSet<int> _discoveredPets = new();
        // All pet recipes
        readonly Dictionary<string, PetDefinition> _recipes = new();
        // All pet sprites loaded
        Sprite[] _petSprites;
        Sprite[] _materialSprites;
        Sprite _silhouetteSprite;

        // Stage state
        int _currentStageIndex;
        int _slotCount;
        int _maxMaterialId;
        int _stageGoal;
        bool _isActive;
        bool _legendConditionEnabled;

        // Slot selections (materialId, -1 = empty)
        int[] _slots = new int[3] { -1, -1, -1 };

        // Pet currently being raised
        int _activePetId = -1;
        int _activePetLevel;
        const int MaxPetLevel = 3;

        int _targetSlotIndex; // which slot the next material click goes to

        void Awake()
        {
            BuildRecipes();
        }

        void Start()
        {
            WireButtons();
        }

        void WireButtons()
        {
            if (_materialButtons != null)
            {
                for (int i = 0; i < _materialButtons.Length; i++)
                {
                    if (_materialButtons[i] == null) continue;
                    int captured = i;
                    _materialButtons[i].onClick.AddListener(() => OnMaterialButtonClicked(captured));
                }
            }
            if (_clearSlotButtons != null)
            {
                for (int s = 0; s < _clearSlotButtons.Length; s++)
                {
                    if (_clearSlotButtons[s] == null) continue;
                    int captured = s;
                    _clearSlotButtons[s].onClick.AddListener(() => ClearSlot(captured));
                }
            }
            if (_combineButton != null)
                _combineButton.onClick.AddListener(TryCombine);
            if (_feedButton != null)
                _feedButton.onClick.AddListener(FeedActivePet);
        }

        void OnMaterialButtonClicked(int materialId)
        {
            if (!_isActive || _slotCount <= 0) return;
            // Try to place in target slot first, then find next empty, then replace target
            int slotCount = Mathf.Min(_slotCount, _slots.Length);
            if (!SelectMaterialForSlot(_targetSlotIndex, materialId))
            {
                // target slot had enough materials, find empty
                bool placed = false;
                for (int s = 0; s < slotCount; s++)
                {
                    if (_slots[s] < 0 && SelectMaterialForSlot(s, materialId))
                    {
                        placed = true;
                        _targetSlotIndex = (s + 1) % slotCount;
                        break;
                    }
                }
                if (!placed)
                {
                    // All filled — overwrite target
                    _slots[_targetSlotIndex] = -1;
                    SelectMaterialForSlot(_targetSlotIndex, materialId);
                    _targetSlotIndex = (_targetSlotIndex + 1) % slotCount;
                }
            }
            else
            {
                _targetSlotIndex = (_targetSlotIndex + 1) % slotCount;
            }
        }

        void BuildRecipes()
        {
            // 25 pets with recipes
            // Format: "mat1_mat2" or "mat1_mat2_mat3" (ids sorted ascending)
            AddRecipe("0_1",      0, "salamander",  false, false);
            AddRecipe("2_3",      1, "phoenix",     false, false);
            AddRecipe("0_2",      2, "golem",       false, false);
            AddRecipe("1_4",      3, "aquadrake",   false, false);
            AddRecipe("3_5",      4, "windbird",    false, false);
            AddRecipe("1_5",      5, "frostcat",    false, false);
            AddRecipe("2_8",      6, "rocktor",     false, false);
            AddRecipe("3_9",      7, "thunderwolf", false, false);
            AddRecipe("2_10",     8, "poisonfox",   false, false);
            AddRecipe("0_11",     9, "lightbun",    false, false);
            AddRecipe("4_7",     10, "stormeagle",  false, false);
            AddRecipe("0_4_11",  11, "flamelion",   false, false);
            AddRecipe("2_5_8",   12, "earthboar",   false, false);
            AddRecipe("1_6_9",   13, "icepanda",    false, false);
            AddRecipe("6_7_10",  14, "sandcrab",    false, false);
            AddRecipe("4_12",    15, "rarebear",    true,  false);
            AddRecipe("7_13",    16, "voidcat",     true,  false);
            AddRecipe("5_9_13",  17, "crystalbird", true,  false);
            AddRecipe("0_4_12",  18, "magmadragon", true,  false);
            AddRecipe("3_7_12",  19, "shadowwolf",  true,  false);
            AddRecipe("8_13_14", 20, "starphoenix", true,  false);
            AddRecipe("11_14_15",21, "legenddrake", false, true);
            AddRecipe("9_13_14", 22, "ancientowl",  false, true);
            AddRecipe("7_12_15", 23, "voiddragon",  false, true);
            AddRecipe("0_6_15",  24, "cosmiccat",   false, true);
        }

        void AddRecipe(string key, int id, string name, bool isRare, bool isLegend)
        {
            _recipes[key] = new PetDefinition { id = id, name = name, isRare = isRare, isLegend = isLegend };
        }

        public void Initialize(Sprite[] petSprites, Sprite[] matSprites, Sprite silhouette)
        {
            _petSprites = petSprites;
            _materialSprites = matSprites;
            _silhouetteSprite = silhouette;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _currentStageIndex = stageIndex;
            _isActive = true;
            _activePetId = -1;
            _activePetLevel = 0;
            _discoveredPets.Clear();

            // complexityFactor: 0=stage1, 0.25=stage2, 0.5=stage3, 0.75=stage4, 1.0=stage5
            float cf = config.complexityFactor;
            _slotCount = cf >= 0.5f ? 3 : 2;
            _legendConditionEnabled = cf >= 1.0f;

            // Max material id available per stage
            _maxMaterialId = cf < 0.25f ? 3
                           : cf < 0.5f ? 7
                           : cf < 0.75f ? 11
                           : cf < 1.0f ? 13
                           : 15;

            // Stage goal (how many unique pets to discover)
            _stageGoal = cf < 0.25f ? 2
                       : cf < 0.5f ? 5
                       : cf < 0.75f ? 8
                       : cf < 1.0f ? 12
                       : 15;

            // Give starting materials
            _inventory.Clear();
            GiveStartingMaterials(stageIndex);

            // Reset slots
            for (int i = 0; i < 3; i++) _slots[i] = -1;

            // Update UI
            _ui?.SetSlotCount(_slotCount);
            _ui?.RefreshInventory(_inventory, _materialSprites, _maxMaterialId);
            _ui?.UpdatePetDisplay(-1, null);
            _ui?.UpdateStageGoal(_discoveredPets.Count, _stageGoal);
        }

        void GiveStartingMaterials(int stageIndex)
        {
            int baseCount = 5 + stageIndex * 2;
            int matRange = _maxMaterialId + 1;
            for (int m = 0; m <= _maxMaterialId; m++)
            {
                // Earlier materials more abundant
                int count = m <= 3 ? baseCount : Mathf.Max(1, baseCount - m);
                _inventory[m] = count;
            }
        }

        public void SetActive(bool active) => _isActive = active;

        public bool IsStageGoalMet() => _discoveredPets.Count >= _stageGoal;

        // Called when player taps a material button
        public bool SelectMaterialForSlot(int slotIndex, int materialId)
        {
            if (!_isActive) return false;
            if (slotIndex >= _slotCount) return false;
            if (materialId > _maxMaterialId) return false;
            if (!_inventory.TryGetValue(materialId, out int count) || count <= 0) return false;

            _slots[slotIndex] = materialId;
            _ui?.UpdateSlot(slotIndex, materialId, _materialSprites != null && materialId < _materialSprites.Length ? _materialSprites[materialId] : null);
            return true;
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount) return;
            _slots[slotIndex] = -1;
            _ui?.UpdateSlot(slotIndex, -1, null);
        }

        // Try to combine current slot contents
        public void TryCombine()
        {
            if (!_isActive) return;

            var filledSlots = new List<int>();
            for (int i = 0; i < _slotCount; i++)
            {
                if (_slots[i] >= 0) filledSlots.Add(_slots[i]);
            }

            if (filledSlots.Count < 2)
            {
                _ui?.ShowMessage("素材が足りません！");
                return;
            }

            // Check if player has all materials
            var usage = new Dictionary<int, int>();
            foreach (int m in filledSlots)
            {
                usage.TryGetValue(m, out int c);
                usage[m] = c + 1;
            }
            foreach (var kv in usage)
            {
                if (!_inventory.TryGetValue(kv.Key, out int have) || have < kv.Value)
                {
                    _ui?.ShowMessage("素材が足りません！");
                    return;
                }
            }

            // Consume materials
            foreach (var kv in usage)
                _inventory[kv.Key] -= kv.Value;

            // Build recipe key (sorted)
            filledSlots.Sort();
            string key = string.Join("_", filledSlots);

            // Dangerous material explosion check (Stage4+)
            bool hasDangerous = filledSlots.Contains(12) || filledSlots.Contains(13);
            if (hasDangerous && Random.value < 0.35f)
            {
                // Explosion!
                ResetSlots();
                _ui?.RefreshInventory(_inventory, _materialSprites, _maxMaterialId);
                StartCoroutine(ExplosionEffect());
                _gameManager.OnExplosion();
                return;
            }

            // Look up recipe
            if (_recipes.TryGetValue(key, out PetDefinition pet))
            {
                // Legend requires combo x3+ when legendCondition is active
                if (pet.isLegend && _legendConditionEnabled)
                {
                    if (_gameManager.CurrentCombo < 3)
                    {
                        // Condition not met — soft fail, return consumed materials
                        foreach (var kv in usage)
                        {
                            _inventory.TryGetValue(kv.Key, out int have2);
                            _inventory[kv.Key] = have2 + kv.Value;
                        }
                        ResetSlots();
                        _ui?.RefreshInventory(_inventory, _materialSprites, _maxMaterialId);
                        _ui?.ShowMessage("伝説ペットはコンボ×3以上が必要！");
                        return;
                    }
                }

                ResetSlots();
                _ui?.RefreshInventory(_inventory, _materialSprites, _maxMaterialId);

                bool isNewDiscovery = _discoveredPets.Add(pet.id);
                int petIdx = pet.id;
                Sprite petSprite = (_petSprites != null && petIdx < _petSprites.Length) ? _petSprites[petIdx] : null;

                if (isNewDiscovery)
                {
                    StartCoroutine(PetDiscoveryEffect(petSprite));
                    _activePetId = pet.id;
                    _activePetLevel = 0;
                    _ui?.UpdatePetDisplay(pet.id, petSprite);
                    _ui?.UpdateStageGoal(_discoveredPets.Count, _stageGoal);
                    _gameManager.OnPetDiscovered(pet.id, pet.isRare, pet.isLegend && _legendConditionEnabled);
                }
                else
                {
                    _ui?.ShowMessage($"{GetPetDisplayName(pet.name)} はもう発見済み！");
                    _gameManager.OnAlreadyKnownRecipe(pet.id);
                }
            }
            else
            {
                // Unknown combination — explosion if dangerous, otherwise just fail
                if (hasDangerous)
                {
                    ResetSlots();
                    _ui?.RefreshInventory(_inventory, _materialSprites, _maxMaterialId);
                    StartCoroutine(ExplosionEffect());
                    _gameManager.OnExplosion();
                }
                else
                {
                    // Soft fail — materials consumed, hint given
                    ResetSlots();
                    _ui?.RefreshInventory(_inventory, _materialSprites, _maxMaterialId);
                    _ui?.ShowMessage("合成失敗... ヒント: 別の組み合わせを試して！");
                    // Small hint bonus
                    _gameManager.OnRecipeHintFound(15);
                }
            }
        }

        string GetPetDisplayName(string name)
        {
            // Capitalize first letter
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToUpper(name[0]) + name.Substring(1);
        }

        void ResetSlots()
        {
            for (int i = 0; i < 3; i++) _slots[i] = -1;
            for (int i = 0; i < _slotCount; i++)
                _ui?.UpdateSlot(i, -1, null);
        }

        // Feed the active pet (Stage2+ unlocks material drops)
        public void FeedActivePet()
        {
            if (!_isActive || _activePetId < 0) return;
            if (_currentStageIndex < 1) return; // Stage1 doesn't have feeding

            _activePetLevel = Mathf.Min(_activePetLevel + 1, MaxPetLevel);
            _ui?.UpdatePetLevel(_activePetLevel, MaxPetLevel);

            // Drop a random material
            if (_maxMaterialId >= 0)
            {
                int droppedMat = Random.Range(0, Mathf.Min(4, _maxMaterialId + 1));
                _inventory.TryGetValue(droppedMat, out int c);
                _inventory[droppedMat] = c + 1;
                _ui?.RefreshInventory(_inventory, _materialSprites, _maxMaterialId);
                _ui?.ShowMessage($"素材を入手: {MaterialNames[droppedMat]}");
                _gameManager.OnFeedReward(10);

                if (_activePetLevel >= MaxPetLevel)
                {
                    _ui?.ShowMessage("育成完了！ボーナス +30pt");
                    _gameManager.OnFeedReward(30);
                }
            }
        }

        IEnumerator PetDiscoveryEffect(Sprite sprite)
        {
            // Scale pulse: 0 → 1.3 → 1.0
            _ui?.PlayDiscoveryEffect();
            yield return null;
        }

        IEnumerator ExplosionEffect()
        {
            _ui?.PlayExplosionEffect();
            yield return null;
        }

        public int GetSlotCount() => _slotCount;
        public int GetMaxMaterialId() => _maxMaterialId;
        public int GetDiscoveredCount() => _discoveredPets.Count;
        public int GetStageGoal() => _stageGoal;
        public int GetActivePetId() => _activePetId;
        public Dictionary<int, int> GetInventory() => _inventory;
    }
}
