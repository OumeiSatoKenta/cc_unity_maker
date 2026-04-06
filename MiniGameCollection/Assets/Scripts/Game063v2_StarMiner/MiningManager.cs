using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game063v2_StarMiner
{
    public class MiningManager : MonoBehaviour
    {
        [SerializeField] StarMinerGameManager _gameManager;
        [SerializeField] StarMinerUI _ui;
        [SerializeField] Transform _starTransform;
        [SerializeField] SpriteRenderer _starRenderer;

        // Game state
        int _drillLevel = 1;
        int _droneCount = 0;
        long _oreCount = 0;
        long _funds = 0;
        long _targetOre = 100;

        // Combo
        int _combo = 0;
        float _comboTimer = 0f;
        const float ComboWindow = 1f;

        // Stage config
        float _speedMultiplier = 1f;
        float _countMultiplier = 1f;
        float _rareMineralChance = 0f;
        bool _droneUnlocked = false;
        bool _stormEnabled = false;
        bool _legendaryUnlocked = false;
        bool _isActive = false;
        int _currentStage = 1;

        // Storm
        bool _stormActive = false;
        float _stormTimer = 0f;
        float _stormInterval = 20f;
        float _stormDuration = 5f;

        // Drone coroutine
        Coroutine _droneCoroutine;

        // Upgrade costs
        long DrillUpgradeCost => 10L * _drillLevel * _drillLevel;
        long DroneUnlockCost => 50;
        long DroneCost => 30L * (_droneCount + 1);
        long LegendaryMiningCost => 100;

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

            // Storm logic
            if (_stormEnabled)
            {
                _stormTimer += Time.deltaTime;
                if (!_stormActive && _stormTimer >= _stormInterval)
                {
                    StartStorm();
                }
                else if (_stormActive && _stormTimer >= _stormDuration)
                {
                    EndStorm();
                }
            }

            // Tap input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                var worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                worldPos.z = 0f;
                var dist = Vector2.Distance(worldPos, _starTransform.position);
                var starSize = _starTransform.localScale.x * 1.5f;
                if (dist <= starSize)
                {
                    TapMine();
                }
            }
        }

        void TapMine()
        {
            if (_gameManager.State != StarMinerGameManager.GameState.Playing) return;

            // Combo
            _combo++;
            _comboTimer = ComboWindow;
            _ui.UpdateCombo(_combo);

            float comboMult = GetComboMultiplier();
            long tapAmount = Mathf.RoundToInt(_drillLevel * comboMult * _countMultiplier);

            bool isRare = _rareMineralChance > 0f && Random.value < _rareMineralChance;
            if (isRare) tapAmount *= 10;

            AddOre(tapAmount);

            // Visual feedback
            StartCoroutine(StarPulse(isRare));
            if (isRare) StartCoroutine(RareFlash());
        }

        float GetComboMultiplier()
        {
            if (_combo >= 5) return 5f;
            if (_combo >= 4) return 3f;
            if (_combo >= 3) return 2f;
            if (_combo >= 2) return 1.5f;
            return 1f;
        }

        void AddOre(long amount)
        {
            _oreCount += amount;
            _ui.UpdateOre(_oreCount, _targetOre);
            if (_oreCount >= _targetOre)
            {
                _isActive = false;
                _ui.SetUpgradeButtonsInteractable(false);
                _gameManager.OnStageClear();
            }
        }

        void StartStorm()
        {
            _stormActive = true;
            _stormTimer = 0f;
            _ui.ShowStormWarning(true);
            StartCoroutine(CameraShake());
        }

        void EndStorm()
        {
            _stormActive = false;
            _stormTimer = 0f;
            _ui.ShowStormWarning(false);
        }

        IEnumerator DroneLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f / _speedMultiplier);
                if (!_isActive || _gameManager.State != StarMinerGameManager.GameState.Playing) continue;
                long droneOre = _droneCount;
                if (_stormActive) droneOre = (long)Mathf.Max(1, droneOre / 2f);
                AddOre(droneOre);
                _ui.UpdateAutoRate(_droneCount, _speedMultiplier);
            }
        }

        public void UpgradeDrill()
        {
            if (_funds < DrillUpgradeCost) return;
            _funds -= DrillUpgradeCost;
            _drillLevel++;
            _ui.UpdateFunds(_funds);
            _ui.UpdateDrillLevel(_drillLevel);
            _ui.UpdateUpgradeCosts(DrillUpgradeCost, _droneUnlocked ? DroneCost : DroneUnlockCost, LegendaryMiningCost, _droneUnlocked, _legendaryUnlocked);
        }

        public void BuyDrone()
        {
            if (!_droneUnlocked)
            {
                if (_funds < DroneUnlockCost) return;
                _funds -= DroneUnlockCost;
                _droneUnlocked = true;
                _droneCount = 1;
                _droneCoroutine = StartCoroutine(DroneLoop());
                _ui.ShowDroneUnlocked();
            }
            else
            {
                if (_funds < DroneCost) return;
                _funds -= DroneCost;
                _droneCount++;
            }
            _funds = System.Math.Max(0, _funds);
            _ui.UpdateFunds(_funds);
            _ui.UpdateDroneCount(_droneCount);
            _ui.UpdateUpgradeCosts(DrillUpgradeCost, DroneCost, LegendaryMiningCost, _droneUnlocked, _legendaryUnlocked);
        }

        public void DoLegendaryMining()
        {
            if (!_legendaryUnlocked || _funds < LegendaryMiningCost) return;
            _funds -= LegendaryMiningCost;
            long legendaryOre = 500L * _drillLevel;
            AddOre(legendaryOre);
            _ui.UpdateFunds(_funds);
            StartCoroutine(StarPulse(true));
        }

        public void SellOre()
        {
            if (_oreCount <= 0) return;
            long earned = _oreCount; // 1G per ore (simplified)
            _funds += earned;
            _oreCount = 0;
            _ui.UpdateFunds(_funds);
            _ui.UpdateOre(_oreCount, _targetOre);
        }

        public void SetupStage(StageManager.StageConfig config, int stage)
        {
            _currentStage = stage;
            _speedMultiplier = config.speedMultiplier;
            _countMultiplier = config.countMultiplier;

            // Stage-specific settings
            switch (stage)
            {
                case 1:
                    _targetOre = 100;
                    _rareMineralChance = 0f;
                    _droneUnlocked = false;
                    _stormEnabled = false;
                    _legendaryUnlocked = false;
                    break;
                case 2:
                    _targetOre = 500;
                    _rareMineralChance = 0f;
                    _droneUnlocked = false;
                    _stormEnabled = false;
                    _legendaryUnlocked = false;
                    _ui.ShowDroneButton(true);
                    break;
                case 3:
                    _targetOre = 2000;
                    _rareMineralChance = config.complexityFactor > 0 ? config.complexityFactor : 0.15f;
                    _stormEnabled = false;
                    _legendaryUnlocked = false;
                    break;
                case 4:
                    _targetOre = 8000;
                    _rareMineralChance = 0.2f;
                    _stormEnabled = true;
                    _stormInterval = 20f;
                    _legendaryUnlocked = false;
                    break;
                case 5:
                    _targetOre = 30000;
                    _rareMineralChance = 0.25f;
                    _stormEnabled = true;
                    _stormInterval = 15f;
                    _legendaryUnlocked = true;
                    _ui.ShowLegendaryButton(true);
                    break;
            }

            _oreCount = 0;
            _stormActive = false;
            _stormTimer = 0f;
            _combo = 0;
            _comboTimer = 0f;

            // Restart drone coroutine so new speedMultiplier is applied
            if (_droneCoroutine != null) StopCoroutine(_droneCoroutine);
            if (_droneCount > 0)
                _droneCoroutine = StartCoroutine(DroneLoop());

            _isActive = true;

            _ui.UpdateOre(_oreCount, _targetOre);
            _ui.UpdateDrillLevel(_drillLevel);
            _ui.UpdateDroneCount(_droneCount);
            _ui.UpdateFunds(_funds);
            _ui.UpdateAutoRate(_droneCount, _speedMultiplier);
            _ui.UpdateCombo(0);
            _ui.ShowStormWarning(false);
            _ui.SetUpgradeButtonsInteractable(true);
            _ui.UpdateUpgradeCosts(DrillUpgradeCost, _droneUnlocked ? DroneCost : DroneUnlockCost, LegendaryMiningCost, _droneUnlocked, _legendaryUnlocked);
            _ui.UpdateStarForStage(stage);
        }

        IEnumerator StarPulse(bool big)
        {
            float scale = big ? 1.4f : 1.2f;
            float dur = 0.15f;
            var orig = _starTransform.localScale;
            float t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, scale, t / dur);
                _starTransform.localScale = orig * s;
                yield return null;
            }
            t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(scale, 1f, t / dur);
                _starTransform.localScale = orig * s;
                yield return null;
            }
            _starTransform.localScale = orig;
        }

        IEnumerator RareFlash()
        {
            if (_starRenderer == null) yield break;
            var orig = _starRenderer.color;
            _starRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            _starRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            _starRenderer.color = orig;
        }

        IEnumerator CameraShake()
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            var origPos = cam.transform.localPosition;
            float dur = 0.4f;
            float t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                float strength = 0.15f * (1 - t / dur);
                cam.transform.localPosition = origPos + (Vector3)Random.insideUnitCircle * strength;
                yield return null;
            }
            cam.transform.localPosition = origPos;
        }

        void OnDestroy()
        {
            if (_droneCoroutine != null) StopCoroutine(_droneCoroutine);
        }
    }
}
