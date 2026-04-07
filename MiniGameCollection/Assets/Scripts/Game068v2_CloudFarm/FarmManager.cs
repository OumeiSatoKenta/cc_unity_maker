using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game068v2_CloudFarm
{
    public class FarmManager : MonoBehaviour
    {
        [SerializeField] CloudFarmGameManager _gameManager;
        [SerializeField] Camera _camera;

        // Plot cells (set via SceneSetup)
        [SerializeField] SpriteRenderer[] _plotRenderers;
        [SerializeField] Collider2D[] _plotColliders;

        // Sprites
        [SerializeField] Sprite _plotEmptySprite;
        [SerializeField] Sprite _plotGrowingSprite;
        [SerializeField] Sprite _plotReadySprite;
        [SerializeField] Sprite _cropCarrotSprite;
        [SerializeField] Sprite _cropCabbageSprite;
        [SerializeField] Sprite _cropTomatoSprite;
        [SerializeField] Sprite _cropStarMelonSprite;
        [SerializeField] SpriteRenderer _weatherIconRenderer;
        [SerializeField] Sprite _weatherSunnySprite;
        [SerializeField] Sprite _weatherRainySprite;
        [SerializeField] Sprite _weatherStormySprite;

        // Crop definitions: growthTime, basePrice
        static readonly float[] GrowthTimes = { 30f, 50f, 80f, 120f };
        static readonly long[] BasePrices = { 50, 80, 120, 1000 };
        static readonly string[] CropNames = { "にんじん", "キャベツ", "トマト", "スターメロン" };

        // Stage targets
        static readonly long[] StageTargets = { 500, 5000, 50000, 200000, 1000000 };

        // State
        bool _isActive;
        int _stageIndex;
        float _speedMul = 1f;
        float _countMul = 1f;
        float _complexityFactor = 0.2f;
        int _activePlots = 6;

        long _coins;
        long _inventory; // total value of harvested crops waiting for shipment
        long _totalEarned;
        long _stageTarget;

        // Crop per plot: -1=empty, 0-3=crop type
        int[] _cropType;
        float[] _growthProgress; // 0 to 1
        bool[] _hasPest;

        // Weather (0=sunny, 1=rainy, 2=stormy)
        int _weather;
        float _weatherTimer;
        float _weatherDuration;
        bool _weatherEnabled;
        float _weatherGrowthMul => _weather == 1 ? 2f : _weather == 2 ? 0.5f : 1f;

        // Market
        float _marketMultiplier = 1f;
        float _marketTimer;
        bool _marketEnabled;

        // Pest
        float _pestTimer;
        float _pestInterval = 20f;
        bool _pestEnabled;

        // Auto harvest
        bool _autoHarvestUnlocked;
        float _autoHarvestTimer;
        float _autoHarvestInterval = 3f;

        // Combo
        int _harvestCombo;
        float _comboTimer;
        const float ComboTimeout = 1f;

        // Selected crop
        int _selectedCrop = 0;
        bool _premiumUnlocked;
        bool _companionEnabled;

        // Upgrades
        int _autoUpgradeLevel; // 0-3, each level halves interval
        int _growthUpgradeLevel; // 0-2, each level -20% growth time

        long AutoUpgradeCost => 200 * (long)Mathf.Pow(5, _autoUpgradeLevel);
        long GrowthUpgradeCost => 300 * (long)Mathf.Pow(5, _growthUpgradeLevel);

        public long TotalEarned => _totalEarned;

        const int MaxPlots = 12;

        public void SetActive(bool active) => _isActive = active;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            ResetAllPlotScales();
            _stageIndex = stageIndex;
            _speedMul = config.speedMultiplier > 0 ? config.speedMultiplier : 1f;
            _countMul = config.countMultiplier > 0 ? config.countMultiplier : 1f;
            _complexityFactor = config.complexityFactor;

            _activePlots = stageIndex switch
            {
                0 => 6,
                1 => 8,
                2 => 10,
                _ => 12
            };
            _activePlots = Mathf.Min(_activePlots, _plotRenderers != null ? _plotRenderers.Length : MaxPlots);

            _weatherEnabled = stageIndex >= 1;
            _marketEnabled = stageIndex >= 2;
            _pestEnabled = stageIndex >= 3;
            _premiumUnlocked = stageIndex >= 4;
            _companionEnabled = stageIndex >= 2;
            _autoHarvestUnlocked = stageIndex >= 1;

            _cropType = new int[MaxPlots];
            _growthProgress = new float[MaxPlots];
            _hasPest = new bool[MaxPlots];
            for (int i = 0; i < MaxPlots; i++) { _cropType[i] = -1; _growthProgress[i] = 0; _hasPest[i] = false; }

            _weather = 0;
            _weatherTimer = 0f;
            _weatherDuration = Random.Range(10f, 30f);
            _marketMultiplier = 1f;
            _marketTimer = 0f;
            _pestTimer = 0f;
            _harvestCombo = 0;
            _comboTimer = 0f;
            _autoHarvestTimer = 0f;
            _autoHarvestInterval = 3f / _speedMul;

            _stageTarget = StageTargets[Mathf.Clamp(stageIndex, 0, StageTargets.Length - 1)];
            _isActive = true;

            if (_selectedCrop == 3 && !_premiumUnlocked) _selectedCrop = 0;

            RefreshPlotVisuals();
            UpdateWeatherIcon();
            NotifyUI();
        }

        void Update()
        {
            if (!_isActive) return;

            float dt = Time.deltaTime;

            // Combo timeout
            if (_harvestCombo > 0)
            {
                _comboTimer -= dt;
                if (_comboTimer <= 0) { _harvestCombo = 0; }
            }

            // Crop growth
            float growthMul = _weatherGrowthMul * _speedMul * (1f + _growthUpgradeLevel * 0.2f);
            bool anyChanged = false;
            for (int i = 0; i < _activePlots; i++)
            {
                if (_cropType[i] < 0 || _growthProgress[i] >= 1f) continue;
                float baseTime = GrowthTimes[_cropType[i]];
                _growthProgress[i] += dt * growthMul / baseTime;
                if (_growthProgress[i] >= 1f) { _growthProgress[i] = 1f; anyChanged = true; }
            }
            if (anyChanged) RefreshPlotVisuals();

            // Auto harvest
            if (_autoHarvestUnlocked)
            {
                _autoHarvestTimer += dt;
                if (_autoHarvestTimer >= _autoHarvestInterval)
                {
                    _autoHarvestTimer = 0f;
                    AutoHarvest();
                }
            }

            // Weather
            if (_weatherEnabled)
            {
                _weatherTimer += dt;
                if (_weatherTimer >= _weatherDuration)
                {
                    _weatherTimer = 0f;
                    _weatherDuration = Random.Range(10f, 30f);
                    _weather = Random.Range(0, 3);
                    UpdateWeatherIcon();
                    _gameManager.UpdateWeatherDisplay(_weather);
                }
            }

            // Market price
            if (_marketEnabled)
            {
                _marketTimer += dt;
                float period = 10f;
                _marketMultiplier = 1f + 0.5f * Mathf.Sin(_marketTimer * 2f * Mathf.PI / period) * _complexityFactor / 0.6f;
                _marketMultiplier = Mathf.Clamp(_marketMultiplier, 0.5f, 2f);
                _gameManager.UpdateMarketDisplay(_marketMultiplier);
            }

            // Pest
            if (_pestEnabled)
            {
                _pestTimer += dt;
                if (_pestTimer >= _pestInterval)
                {
                    _pestTimer = 0f;
                    SpawnPest();
                }
                // Damage crops with pests
                for (int i = 0; i < _activePlots; i++)
                {
                    if (_hasPest[i] && _cropType[i] >= 0 && _growthProgress[i] > 0f)
                    {
                        _growthProgress[i] = Mathf.Max(0f, _growthProgress[i] - dt * 0.01f);
                    }
                }
            }

            // Input
            if (_camera != null && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 worldPos = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                for (int i = 0; i < _activePlots; i++)
                {
                    if (_plotColliders != null && i < _plotColliders.Length && _plotColliders[i] != null)
                    {
                        if (_plotColliders[i].OverlapPoint(worldPos))
                        {
                            OnPlotTapped(i);
                            break;
                        }
                    }
                }
            }

            // Progress & auto rate
            float autoRate = _autoHarvestUnlocked ? (_activePlots * 0.5f * _speedMul) : 0f;
            _gameManager.UpdateAutoRateDisplay(autoRate);
        }

        void OnPlotTapped(int i)
        {
            if (!_isActive) return;
            if (_hasPest[i])
            {
                // Exterminate pest
                _hasPest[i] = false;
                StartCoroutine(PestEliminateFlash(i));
                return;
            }
            if (_cropType[i] < 0)
            {
                // Plant seed
                if (_selectedCrop == 3 && !_premiumUnlocked) return;
                _cropType[i] = _selectedCrop;
                _growthProgress[i] = 0f;
                RefreshPlotVisual(i);
            }
            else if (_growthProgress[i] >= 1f)
            {
                // Harvest
                Harvest(i);
            }
        }

        void Harvest(int i)
        {
            int type = _cropType[i];
            long baseVal = BasePrices[type];
            float companion = _companionEnabled ? GetCompanionBonus(i, type) : 1f;
            long val = (long)(baseVal * companion * _countMul);

            _inventory += val;
            _harvestCombo++;
            _comboTimer = ComboTimeout;
            long comboBonus = _harvestCombo * 5;
            _coins += comboBonus;

            _cropType[i] = -1;
            _growthProgress[i] = 0f;

            StartCoroutine(HarvestPulse(i));
            _gameManager.UpdateComboDisplay(_harvestCombo);
            _gameManager.UpdateInventoryDisplay(_inventory);
            _gameManager.UpdateCoinsDisplay(_coins);
        }

        void AutoHarvest()
        {
            bool anyHarvested = false;
            for (int i = 0; i < _activePlots; i++)
            {
                if (_cropType[i] >= 0 && _growthProgress[i] >= 1f)
                {
                    Harvest(i);
                    anyHarvested = true;
                }
            }
            if (anyHarvested) RefreshPlotVisuals();
        }

        float GetCompanionBonus(int i, int type)
        {
            int rows = Mathf.CeilToInt(_activePlots / 2f);
            int col = i % 2;
            int row = i / 2;
            int sameNeighbors = 0;
            int totalNeighbors = 0;
            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };
            for (int d = 0; d < 4; d++)
            {
                int nr = row + dr[d], nc = col + dc[d];
                if (nr < 0 || nc < 0 || nc >= 2 || nr >= rows) continue;
                int ni = nr * 2 + nc;
                if (ni >= _activePlots) continue;
                totalNeighbors++;
                if (_cropType[ni] == type) sameNeighbors++;
            }
            return 1f + sameNeighbors * 0.3f;
        }

        public void Sell()
        {
            if (!_isActive || _inventory <= 0) return;
            float bonus = (_marketEnabled && _marketMultiplier >= 1.5f) ? 1.5f : 1f;
            long earned = (long)(_inventory * _marketMultiplier * bonus);
            _coins += earned;
            _totalEarned += earned;
            _inventory = 0;

            _gameManager.UpdateCoinsDisplay(_coins);
            _gameManager.UpdateInventoryDisplay(_inventory);
            _gameManager.UpdateProgressDisplay(_totalEarned, _stageTarget);

            if (_totalEarned >= _stageTarget)
            {
                _isActive = false;
                _gameManager.OnStageClear();
            }
        }

        public void SelectCrop(int type)
        {
            if (type == 3 && !_premiumUnlocked) return;
            _selectedCrop = type;
            _gameManager.UpdateSelectedCrop(CropNames[type]);
        }

        public void UpgradeAutoHarvest()
        {
            if (_autoUpgradeLevel >= 3 || _coins < AutoUpgradeCost) return;
            _coins -= AutoUpgradeCost;
            _autoUpgradeLevel++;
            _autoHarvestInterval = 3f / (_speedMul * (1f + _autoUpgradeLevel * 0.5f));
            _gameManager.UpdateCoinsDisplay(_coins);
            NotifyUI();
        }

        public void UpgradeGrowth()
        {
            if (_growthUpgradeLevel >= 2 || _coins < GrowthUpgradeCost) return;
            _coins -= GrowthUpgradeCost;
            _growthUpgradeLevel++;
            _gameManager.UpdateCoinsDisplay(_coins);
            NotifyUI();
        }

        void SpawnPest()
        {
            List<int> candidates = new List<int>();
            for (int i = 0; i < _activePlots; i++)
            {
                if (_cropType[i] >= 0 && !_hasPest[i]) candidates.Add(i);
            }
            if (candidates.Count == 0) return;
            int idx = candidates[Random.Range(0, candidates.Count)];
            _hasPest[idx] = true;
            StartCoroutine(PestFlash(idx));
        }

        void RefreshPlotVisuals()
        {
            if (_plotRenderers == null) return;
            for (int i = 0; i < _plotRenderers.Length; i++)
            {
                if (_plotRenderers[i] == null) continue;
                _plotRenderers[i].gameObject.SetActive(i < _activePlots);
                if (i < _activePlots) RefreshPlotVisual(i);
            }
        }

        void RefreshPlotVisual(int i)
        {
            if (_plotRenderers == null || i >= _plotRenderers.Length || _plotRenderers[i] == null) return;
            if (_cropType[i] < 0)
            {
                _plotRenderers[i].sprite = _plotEmptySprite;
                _plotRenderers[i].color = Color.white;
            }
            else if (_growthProgress[i] < 1f)
            {
                _plotRenderers[i].sprite = _plotGrowingSprite;
                _plotRenderers[i].color = Color.white;
            }
            else
            {
                _plotRenderers[i].sprite = CropSpriteOf(_cropType[i]);
                _plotRenderers[i].color = Color.white;
            }
            if (_hasPest[i]) _plotRenderers[i].color = new Color(1f, 0.5f, 0.5f);
        }

        Sprite CropSpriteOf(int type) => type switch
        {
            0 => _cropCarrotSprite,
            1 => _cropCabbageSprite,
            2 => _cropTomatoSprite,
            3 => _cropStarMelonSprite,
            _ => _plotReadySprite
        };

        void UpdateWeatherIcon()
        {
            if (_weatherIconRenderer == null) return;
            if (!_weatherEnabled) { _weatherIconRenderer.gameObject.SetActive(false); return; }
            _weatherIconRenderer.gameObject.SetActive(true);
            _weatherIconRenderer.sprite = _weather switch
            {
                1 => _weatherRainySprite,
                2 => _weatherStormySprite,
                _ => _weatherSunnySprite
            };
        }

        void NotifyUI()
        {
            _gameManager.UpdateCoinsDisplay(_coins);
            _gameManager.UpdateInventoryDisplay(_inventory);
            _gameManager.UpdateProgressDisplay(_totalEarned, _stageTarget);
            _gameManager.UpdateWeatherDisplay(_weather);
            _gameManager.UpdateMarketDisplay(_marketMultiplier);
            _gameManager.UpdateShopButtons(
                _autoHarvestUnlocked, _companionEnabled, _pestEnabled, _premiumUnlocked,
                _coins, AutoUpgradeCost, GrowthUpgradeCost);
            _gameManager.UpdateSelectedCrop(CropNames[_selectedCrop]);
        }

        IEnumerator HarvestPulse(int i)
        {
            if (_plotRenderers == null || i >= _plotRenderers.Length || _plotRenderers[i] == null) yield break;
            var t = _plotRenderers[i].transform;
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.2f;
                float s = ratio < 0.5f ? Mathf.Lerp(1f, 1.4f, ratio * 2f) : Mathf.Lerp(1.4f, 1f, (ratio - 0.5f) * 2f);
                t.localScale = orig * s;
                yield return null;
            }
            t.localScale = orig;
            RefreshPlotVisual(i);
        }

        IEnumerator PestFlash(int i)
        {
            if (_plotRenderers == null || i >= _plotRenderers.Length || _plotRenderers[i] == null) yield break;
            var sr = _plotRenderers[i];
            for (int f = 0; f < 4; f++)
            {
                if (!_hasPest[i]) yield break;
                sr.color = new Color(1f, 0.2f, 0.2f);
                yield return new WaitForSeconds(0.1f);
                sr.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
            if (_hasPest[i]) sr.color = new Color(1f, 0.5f, 0.5f);
        }

        IEnumerator PestEliminateFlash(int i)
        {
            if (_plotRenderers == null || i >= _plotRenderers.Length || _plotRenderers[i] == null) yield break;
            var sr = _plotRenderers[i];
            sr.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            RefreshPlotVisual(i);
        }

        void ResetAllPlotScales()
        {
            if (_plotRenderers == null) return;
            foreach (var sr in _plotRenderers)
            {
                if (sr != null) sr.transform.localScale = Vector3.one * (sr.transform.localScale.x > 0 ? sr.transform.localScale.x : 1f);
            }
        }

        void OnDestroy()
        {
            // Clean up dynamically loaded sprites if needed (none here, loaded via Resources)
        }
    }
}
