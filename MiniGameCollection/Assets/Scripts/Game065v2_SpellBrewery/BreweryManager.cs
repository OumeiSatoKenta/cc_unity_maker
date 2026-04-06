using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game065v2_SpellBrewery
{
    public class BreweryManager : MonoBehaviour
    {
        [SerializeField] SpellBreweryGameManager _gameManager;
        [SerializeField] SpellBreweryUI _ui;
        [SerializeField] GameObject _cauldronObj;
        [SerializeField] Sprite[] _ingredientSprites; // 5: Fire, Water, Earth, Air, Light
        [SerializeField] Sprite[] _potionSprites;     // 8: Fire,Water,Earth,Air,Light,Storm,Nature,Legendary

        public enum IngredientType { Fire = 0, Water, Earth, Air, Light }
        public enum PotionType { FirePotion = 0, WaterPotion, EarthPotion, AirPotion, LightPotion, StormPotion, NaturePotion, LegendaryPotion }

        static readonly string[] IngredientNames = { "Fire", "Water", "Earth", "Air", "Light" };
        static readonly string[] PotionNames = { "Fire Potion", "Water Potion", "Earth Potion", "Air Potion", "Light Potion", "Storm Potion", "Nature Potion", "Legendary Potion" };
        static readonly long[] PotionPrices = { 30, 30, 30, 30, 50, 120, 150, 2000 };

        // Recipes: ingredient bitmask (bit 0=Fire, 1=Water, 2=Earth, 3=Air, 4=Light) → PotionType
        // Fire+Water=0b00011=3, Water+Earth=0b00110=6, Earth+Air=0b01100=12, Air+Light=0b11000=24, Fire+Air=0b01001=9
        // Fire+Water+Air=0b01011=11, Water+Earth+Light=0b10110=22, all=0b11111=31
        static readonly Dictionary<int, PotionType> Recipes = new()
        {
            { (1<<0)|(1<<1),           PotionType.FirePotion    }, // Fire+Water
            { (1<<1)|(1<<2),           PotionType.WaterPotion   }, // Water+Earth
            { (1<<2)|(1<<3),           PotionType.EarthPotion   }, // Earth+Air
            { (1<<3)|(1<<4),           PotionType.AirPotion     }, // Air+Light
            { (1<<0)|(1<<3),           PotionType.LightPotion   }, // Fire+Air
            { (1<<0)|(1<<1)|(1<<3),    PotionType.StormPotion   }, // Fire+Water+Air
            { (1<<1)|(1<<2)|(1<<4),    PotionType.NaturePotion  }, // Water+Earth+Light
            { (1<<0)|(1<<1)|(1<<2)|(1<<3)|(1<<4), PotionType.LegendaryPotion }, // All 5
        };

        // State
        int[] _ingredientStock = new int[5]; // per IngredientType
        int[] _potionStock = new int[8];     // per PotionType
        List<IngredientType> _cauldronSlots = new(5);
        long _gold = 0;
        long _goldTarget = 100;
        int _combo = 0;
        float _comboTimer = 0f;
        const float ComboDecay = 30f;

        bool _isActive = false;
        bool _isBrewing = false;
        int _currentStage = 1;
        float _speedMultiplier = 0f;
        float _countMultiplier = 1f;
        bool _autoCollectEnabled = false;
        bool _failureEnabled = false;
        bool _orderEnabled = false;
        bool _legendaryEnabled = false;

        // Order system
        PotionType _currentOrder = PotionType.FirePotion;
        float _orderTimer = 0f;
        const float OrderDuration = 30f;
        bool _hasOrder = false;

        Coroutine _autoCoroutine;
        Coroutine _orderCoroutine;

        SpriteRenderer _cauldronSR;

        void Start()
        {
            if (_cauldronObj != null)
                _cauldronSR = _cauldronObj.GetComponent<SpriteRenderer>();
        }

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            _currentStage = stage;
            _speedMultiplier = config.speedMultiplier;
            _countMultiplier = config.countMultiplier;

            // Stop previous coroutines
            if (_autoCoroutine != null) StopCoroutine(_autoCoroutine);
            if (_orderCoroutine != null) StopCoroutine(_orderCoroutine);

            // Reset state
            System.Array.Clear(_ingredientStock, 0, _ingredientStock.Length);
            System.Array.Clear(_potionStock, 0, _potionStock.Length);
            _cauldronSlots.Clear();
            _gold = 0;
            _combo = 0;
            _comboTimer = 0f;
            _hasOrder = false;
            _orderTimer = 0f;
            _isBrewing = false;

            switch (stage)
            {
                case 1:
                    _goldTarget = 100;
                    _autoCollectEnabled = false;
                    _failureEnabled = false;
                    _orderEnabled = false;
                    _legendaryEnabled = false;
                    // Give starter ingredients
                    _ingredientStock[0] = 5; // Fire
                    _ingredientStock[1] = 5; // Water
                    _ingredientStock[2] = 5; // Earth
                    break;
                case 2:
                    _goldTarget = 1000;
                    _autoCollectEnabled = true;
                    _failureEnabled = false;
                    _orderEnabled = false;
                    _legendaryEnabled = false;
                    _ingredientStock[0] = 3;
                    _ingredientStock[1] = 3;
                    _ingredientStock[2] = 3;
                    _ingredientStock[3] = 2; // Air unlocked
                    _ingredientStock[4] = 2; // Light unlocked
                    _ui?.ShowNewElement("自動収集解放！新材料 Air & Light 追加！");
                    break;
                case 3:
                    _goldTarget = 10000;
                    _autoCollectEnabled = true;
                    _failureEnabled = true;
                    _orderEnabled = false;
                    _legendaryEnabled = false;
                    _ingredientStock[0] = 3;
                    _ingredientStock[1] = 3;
                    _ingredientStock[2] = 3;
                    _ingredientStock[3] = 3;
                    _ingredientStock[4] = 3;
                    _ui?.ShowNewElement("失敗ペナルティ追加！組み合わせを慎重に！");
                    break;
                case 4:
                    _goldTarget = 50000;
                    _autoCollectEnabled = true;
                    _failureEnabled = true;
                    _orderEnabled = true;
                    _legendaryEnabled = false;
                    for (int i = 0; i < 5; i++) _ingredientStock[i] = 5;
                    _ui?.ShowNewElement("注文システム解放！指定ポーションで2倍報酬！");
                    break;
                case 5:
                    _goldTarget = 200000;
                    _autoCollectEnabled = true;
                    _failureEnabled = true;
                    _orderEnabled = true;
                    _legendaryEnabled = true;
                    for (int i = 0; i < 5; i++) _ingredientStock[i] = 5;
                    _ui?.ShowNewElement("伝説レシピ解放！全材料×1で2000Gポーション！");
                    break;
            }

            _isActive = true;
            _ui.UpdateGold(_gold, _goldTarget);
            _ui.UpdateAllIngredients(_ingredientStock, _currentStage);
            _ui.UpdateCauldronSlots(_cauldronSlots);
            _ui.UpdatePotionInventory(_potionStock, PotionPrices);
            _ui.UpdateCombo(0);
            _ui.UpdateBrewButton(false);
            _ui.HideOrder();

            if (_autoCollectEnabled)
                _autoCoroutine = StartCoroutine(AutoCollectLoop());
            if (_orderEnabled)
                _orderCoroutine = StartCoroutine(OrderLoop());
        }

        void Update()
        {
            if (!_isActive) return;

            // Combo decay
            if (_combo > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    _combo = 0;
                    _ui.UpdateCombo(0);
                }
            }

            // Order timer
            if (_hasOrder && _orderEnabled)
            {
                _orderTimer -= Time.deltaTime;
                _ui.UpdateOrderTimer(_orderTimer / OrderDuration);
                if (_orderTimer <= 0f)
                {
                    _hasOrder = false;
                    _ui.HideOrder();
                }
            }
        }

        public void TapIngredient(int typeIndex)
        {
            if (!_isActive || _isBrewing) return;
            if (typeIndex < 0 || typeIndex >= _ingredientStock.Length) return;
            var type = (IngredientType)typeIndex;
            if (_ingredientStock[typeIndex] <= 0) return;
            if (_cauldronSlots.Count >= 5) return;

            _ingredientStock[typeIndex]--;
            _cauldronSlots.Add(type);

            _ui.UpdateIngredientStock(typeIndex, _ingredientStock[typeIndex]);
            _ui.UpdateCauldronSlots(_cauldronSlots);
            _ui.UpdateBrewButton(_cauldronSlots.Count >= 2);
            _ui.PulseIngredientButton(typeIndex);
        }

        public void ClearCauldron()
        {
            if (!_isActive || _isBrewing) return;
            // Return ingredients to stock
            foreach (var ing in _cauldronSlots)
            {
                int idx = (int)ing;
                if (_ingredientStock[idx] < 20) _ingredientStock[idx]++;
            }
            _cauldronSlots.Clear();
            _ui.UpdateAllIngredients(_ingredientStock, _currentStage);
            _ui.UpdateCauldronSlots(_cauldronSlots);
            _ui.UpdateBrewButton(false);
        }

        public void Brew()
        {
            if (!_isActive || _isBrewing || _cauldronSlots.Count < 2) return;
            StartCoroutine(BrewCoroutine());
        }

        IEnumerator BrewCoroutine()
        {
            _isBrewing = true;
            _ui.UpdateBrewButton(false);
            _ui.ShowBrewing(true);

            // Compute bitmask
            int mask = 0;
            foreach (var ing in _cauldronSlots)
                mask |= (1 << (int)ing);

            yield return new WaitForSeconds(2f);

            _cauldronSlots.Clear();
            _ui.UpdateCauldronSlots(_cauldronSlots);

            if (Recipes.TryGetValue(mask, out PotionType potionType))
            {
                // Check legendary gate
                if (potionType == PotionType.LegendaryPotion && !_legendaryEnabled)
                {
                    // Not yet unlocked - treat as no recipe
                    BrewFail();
                }
                else
                {
                    _potionStock[(int)potionType]++;
                    _combo++;
                    _comboTimer = ComboDecay;
                    _ui.UpdateCombo(_combo);
                    _ui.UpdatePotionInventory(_potionStock, PotionPrices);
                    StartCoroutine(CauldronSuccessAnimation());
                    _ui.ShowFloatingText($"✨ {PotionNames[(int)potionType]} 完成！", Vector3.up * 1.5f);
                }
            }
            else
            {
                BrewFail();
            }

            _ui.ShowBrewing(false);
            _isBrewing = false;
            _ui.UpdateBrewButton(false);
        }

        void BrewFail()
        {
            if (_failureEnabled)
            {
                // Ingredients already consumed - show failure
                _combo = 0;
                _comboTimer = 0f;
                _ui.UpdateCombo(0);
                StartCoroutine(CauldronFailAnimation());
                _ui.ShowFloatingText("💥 失敗！材料がロスト！", Vector3.zero);
            }
            else
            {
                // No penalty in early stages, just fail silently
                StartCoroutine(CauldronFailAnimation());
                _ui.ShowFloatingText("？ レシピが見つからない", Vector3.zero);
            }
        }

        public void SellPotion(int potionIndex)
        {
            if (!_isActive) return;
            if (_potionStock[potionIndex] <= 0) return;
            _potionStock[potionIndex]--;
            long price = PotionPrices[potionIndex];
            float comboMult = GetComboMult();
            bool isOrderMatch = _hasOrder && (int)_currentOrder == potionIndex;
            if (isOrderMatch)
            {
                price = (long)(price * 2f * comboMult);
                _hasOrder = false;
                _ui.HideOrder();
                _ui.ShowFloatingText($"+{price}G 注文ボーナス！", Vector3.up * 0.5f);
            }
            else
            {
                price = (long)(price * comboMult);
                _ui.ShowFloatingText($"+{price}G", Vector3.up * 0.5f);
            }
            _gold += price;
            _ui.UpdateGold(_gold, _goldTarget);
            _ui.UpdatePotionInventory(_potionStock, PotionPrices);
            CheckGoal();
        }

        public void SellAll()
        {
            if (!_isActive) return;
            long total = 0;
            for (int i = 0; i < _potionStock.Length; i++)
            {
                if (_potionStock[i] <= 0) continue;
                long price = PotionPrices[i];
                float comboMult = GetComboMult();
                bool isOrderMatch = _hasOrder && (int)_currentOrder == i;
                if (isOrderMatch)
                {
                    price = (long)(price * 2f * comboMult);
                    _hasOrder = false;
                    _ui.HideOrder();
                }
                else
                {
                    price = (long)(price * comboMult);
                }
                total += price * _potionStock[i];
                _potionStock[i] = 0;
            }
            if (total > 0)
            {
                _gold += total;
                _ui.ShowFloatingText($"+{total}G", Vector3.up * 0.5f);
                _ui.UpdateGold(_gold, _goldTarget);
                _ui.UpdatePotionInventory(_potionStock, PotionPrices);
                CheckGoal();
            }
        }

        void CheckGoal()
        {
            if (_gold >= _goldTarget && _isActive)
            {
                _isActive = false;
                _gameManager.OnStageClear();
            }
        }

        float GetComboMult()
        {
            if (_combo >= 8) return 5f;
            if (_combo >= 5) return 3f;
            if (_combo >= 3) return 2f;
            if (_combo >= 2) return 1.5f;
            return 1f;
        }

        IEnumerator AutoCollectLoop()
        {
            while (true)
            {
                float interval = Mathf.Max(0.2f, 1f / Mathf.Max(_speedMultiplier, 0.1f));
                yield return new WaitForSeconds(interval);
                if (!_isActive || _gameManager.State != SpellBreweryGameManager.GameState.Playing) continue;

                // Collect random ingredient
                int maxType = Mathf.Min(4, _currentStage + 1);
                int idx = Random.Range(0, maxType + 1);
                int count = Mathf.Max(1, Mathf.RoundToInt(_countMultiplier));
                for (int c = 0; c < count; c++)
                {
                    if (_ingredientStock[idx] < 20)
                        _ingredientStock[idx]++;
                }
                _ui.UpdateIngredientStock(idx, _ingredientStock[idx]);
            }
        }

        IEnumerator OrderLoop()
        {
            yield return new WaitForSeconds(5f); // Initial delay
            while (true)
            {
                if (!_isActive || _gameManager.State != SpellBreweryGameManager.GameState.Playing)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }
                if (!_hasOrder)
                {
                    // Generate order (Legendary excluded from orders intentionally)
                    int maxPotion = _legendaryEnabled ? 7 : 6; // 0..6 range, Legendary(7) not ordered
                    _currentOrder = (PotionType)Random.Range(0, maxPotion + 1);
                    _hasOrder = true;
                    _orderTimer = OrderDuration;
                    _ui.ShowOrder(_currentOrder, PotionNames[(int)_currentOrder], (long)(PotionPrices[(int)_currentOrder] * 2f));
                }
                yield return new WaitForSeconds(1f);
            }
        }

        IEnumerator CauldronSuccessAnimation()
        {
            if (_cauldronObj == null) yield break;
            var t = _cauldronObj.transform;
            var origScale = t.localScale;
            var sr = _cauldronSR;

            float dur = 0.15f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(1f, 1.4f, elapsed / dur);
                t.localScale = origScale * s;
                yield return null;
            }
            if (sr != null) sr.color = new Color(1f, 0.9f, 0.2f);
            elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(1.4f, 1f, elapsed / dur);
                t.localScale = origScale * s;
                yield return null;
            }
            t.localScale = origScale;
            yield return new WaitForSeconds(0.2f);
            if (sr != null) sr.color = Color.white;
        }

        IEnumerator CauldronFailAnimation()
        {
            if (_cauldronSR != null) _cauldronSR.color = new Color(1f, 0.2f, 0.2f);
            yield return StartCoroutine(CameraShake());
            yield return new WaitForSeconds(0.1f);
            if (_cauldronSR != null) _cauldronSR.color = Color.white;
        }

        IEnumerator CameraShake()
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            var orig = cam.transform.localPosition;
            float dur = 0.4f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float str = 0.12f * (1f - t / dur);
                cam.transform.localPosition = orig + (Vector3)Random.insideUnitCircle * str;
                yield return null;
            }
            cam.transform.localPosition = orig;
        }

        void OnDestroy()
        {
            StopAllCoroutines(); // Includes CameraShake so camera position is restored
        }
    }
}
