using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game070v2_NanoLab
{
    public class NanoMachineManager : MonoBehaviour
    {
        [SerializeField] NanoLabGameManager _gameManager;

        // Nano state
        long _nanoCount;
        long _totalNanoEarned;
        float _autoRate;
        float _clickPower;
        float _prestigeMultiplier = 1f;
        int _prestigeCount;
        int _currentEra;
        int _targetEra;
        long _score;
        float _stageStartTime;
        int _stageIndex;

        // Features per stage
        bool _autoEnabled;
        bool _prestigeEnabled;
        bool _mutationEnabled;
        bool _fusionEnabled;

        // Tech tree
        TechNodeData[] _allNodes;
        float _clickPowerBonus; // additive from tech
        float _autoRateBonus;   // additive from tech
        float _prestigeBonusAdd;
        bool _mutationControlOwned;

        // Mutation
        float _mutationTimer;
        float _mutationInterval = 30f;
        bool _currentMutationPositive;

        // Active state
        bool _isActive;

        static readonly string[] EraNames = { "原子時代", "分子時代", "細胞時代", "生物時代", "機械時代", "宇宙時代" };
        static readonly long[] EraThresholds = { 0, 100, 500, 2000, 10000, 50000 };

        public long TotalScore => _score;

        public void SetActive(bool active) => _isActive = active;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _isActive = true;
            _stageStartTime = Time.time;

            // Reset per-stage state
            _nanoCount = 0;
            _totalNanoEarned = 0;
            _currentEra = 0;
            _prestigeMultiplier = 1f;
            _prestigeCount = 0;
            _clickPowerBonus = 0;
            _autoRateBonus = 0;
            _prestigeBonusAdd = 0;
            _mutationControlOwned = false;
            _mutationTimer = 0;

            // Map config to features
            _clickPower = 1f * config.countMultiplier;
            _autoRate = 0f;

            _autoEnabled = config.complexityFactor >= 0.3f;
            _prestigeEnabled = config.complexityFactor >= 0.6f;
            _mutationEnabled = config.complexityFactor >= 0.8f;
            _fusionEnabled = config.complexityFactor >= 1.0f;

            if (_autoEnabled)
                _autoRate = 0.5f * config.speedMultiplier;

            _targetEra = stageIndex + 1; // stage0->era1, stage4->era5

            BuildTechTree(stageIndex);
            RefreshUI();
        }

        void BuildTechTree(int stageIndex)
        {
            var nodes = new List<TechNodeData>();

            // Always available
            nodes.Add(new TechNodeData { id = "atomic_research", nameJP = "原子研究", description = "タップ倍率+1", cost = 10, effect = TechEffect.ClickPower, value = 1f, path = TechPath.Efficiency });
            nodes.Add(new TechNodeData { id = "molecular_bond", nameJP = "分子結合", description = "自動増殖+10%", cost = 30, effect = TechEffect.AutoRate, value = 0.1f, path = TechPath.Efficiency, prerequisiteId = "atomic_research" });
            nodes.Add(new TechNodeData { id = "nano_optimizer", nameJP = "ナノ最適化", description = "タップ倍率+2", cost = 80, effect = TechEffect.ClickPower, value = 2f, path = TechPath.Efficiency, prerequisiteId = "molecular_bond" });

            if (stageIndex >= 1)
            {
                nodes.Add(new TechNodeData { id = "auto_replication", nameJP = "自動複製", description = "自動増殖+0.5/秒", cost = 50, effect = TechEffect.AutoRate, value = 0.5f, path = TechPath.Growth });
                nodes.Add(new TechNodeData { id = "efficiency_boost", nameJP = "効率ブースト", description = "全効率+20%", cost = 100, effect = TechEffect.ClickPower, value = 2f, path = TechPath.Efficiency, prerequisiteId = "nano_optimizer" });
            }

            if (stageIndex >= 2)
            {
                nodes.Add(new TechNodeData { id = "prestige_amplifier", nameJP = "プレステージ増幅", description = "プレステージ倍率+0.5", cost = 200, effect = TechEffect.PrestigeBonus, value = 0.5f, path = TechPath.Prestige });
            }

            if (stageIndex >= 3)
            {
                nodes.Add(new TechNodeData { id = "mutation_control", nameJP = "変異制御", description = "変異を制御し良い効果を引き出す", cost = 500, effect = TechEffect.MutationControl, value = 1f, path = TechPath.Efficiency, prerequisiteId = "efficiency_boost" });
            }

            if (stageIndex >= 4)
            {
                nodes.Add(new TechNodeData { id = "cosmic_fusion", nameJP = "宇宙融合", description = "超技術を解放（全パス技術が必要）", cost = 1000, effect = TechEffect.FusionUnlock, value = 1f, path = TechPath.Fusion });
            }

            _allNodes = nodes.ToArray();
            UpdateNodeAvailability();
        }

        void UpdateNodeAvailability()
        {
            foreach (var node in _allNodes)
            {
                if (node.unlocked) continue;
                bool prereqMet = string.IsNullOrEmpty(node.prerequisiteId) || IsUnlocked(node.prerequisiteId);
                // Fusion requires all paths
                if (node.effect == TechEffect.FusionUnlock)
                    prereqMet = IsUnlocked("auto_replication") && IsUnlocked("prestige_amplifier") && IsUnlocked("mutation_control");
                node.available = prereqMet && _nanoCount >= node.cost;
            }
        }

        bool IsUnlocked(string id)
        {
            foreach (var n in _allNodes)
                if (n.id == id && n.unlocked) return true;
            return false;
        }

        public void UnlockTech(string nodeId)
        {
            if (!_isActive) return;
            foreach (var node in _allNodes)
            {
                if (node.id != nodeId || node.unlocked || _nanoCount < node.cost) continue;
                _nanoCount -= node.cost;
                node.unlocked = true;
                ApplyTechEffect(node);
                UpdateNodeAvailability();
                StartCoroutine(TechUnlockFeedback());
                RefreshUI();
                break;
            }
        }

        void ApplyTechEffect(TechNodeData node)
        {
            switch (node.effect)
            {
                case TechEffect.ClickPower:
                    _clickPowerBonus += node.value;
                    break;
                case TechEffect.AutoRate:
                    _autoRateBonus += node.value;
                    break;
                case TechEffect.PrestigeBonus:
                    _prestigeBonusAdd += node.value;
                    break;
                case TechEffect.MutationControl:
                    _mutationControlOwned = true;
                    break;
                case TechEffect.FusionUnlock:
                    _fusionEnabled = true;
                    // Cosmic fusion: massive bonus
                    _clickPower += 10f;
                    _autoRate += 10f;
                    break;
            }
        }

        public void OnTap()
        {
            if (!_isActive) return;
            long gained = (long)Mathf.Max(1f, (_clickPower + _clickPowerBonus) * _prestigeMultiplier);
            AddNano(gained);
            _gameManager.UpdateNanoCount(_nanoCount);
        }

        public void DoPrestige()
        {
            if (!_isActive || !_prestigeEnabled) return;
            if (_currentEra < 1) return;

            _prestigeCount++;
            float bonus = 0.5f + _prestigeBonusAdd + (_prestigeCount * 0.1f);
            _prestigeMultiplier += bonus;

            // Reset nano but keep multiplier and unlocked tech
            _nanoCount = 0;
            _currentEra = 0;

            StartCoroutine(PrestigeFeedback());
            RefreshUI();
        }

        void AddNano(long amount)
        {
            _nanoCount += amount;
            _totalNanoEarned += amount;
            CheckEraProgress();
            UpdateNodeAvailability();
        }

        void CheckEraProgress()
        {
            while (_currentEra < EraThresholds.Length - 1 && _totalNanoEarned >= EraThresholds[_currentEra + 1])
            {
                _currentEra++;
                int eraIdx = Mathf.Clamp(_currentEra, 0, EraNames.Length - 1);
                _gameManager.UpdateEra(_currentEra, EraNames[eraIdx]);

                if (_currentEra >= _targetEra)
                {
                    CalculateScore();
                    _isActive = false;
                    _gameManager.OnStageClear();
                    return;
                }
            }
        }

        void CalculateScore()
        {
            int unlockedCount = 0;
            foreach (var n in _allNodes)
                if (n.unlocked) unlockedCount++;

            _score += unlockedCount * 100;
            _score += _currentEra * 500;
            _score += _prestigeCount * 200;

            float elapsed = Time.time - _stageStartTime;
            if (elapsed < 60f) _score = (long)(_score * 2f);
            else if (elapsed < 120f) _score = (long)(_score * 1.5f);
        }

        void Update()
        {
            if (!_isActive) return;

            // Auto rate
            if (_autoEnabled)
            {
                float rate = (_autoRate + _autoRateBonus) * _prestigeMultiplier;
                if (rate > 0)
                {
                    AddNano((long)(rate * Time.deltaTime));
                    _gameManager.UpdateNanoCount(_nanoCount);
                    _gameManager.UpdateAutoRate(rate);
                }
            }

            // Mutation events
            if (_mutationEnabled)
            {
                _mutationTimer += Time.deltaTime;
                if (_mutationTimer >= _mutationInterval)
                {
                    _mutationTimer = 0;
                    TriggerMutation();
                }
            }

            UpdateNodeAvailability();
            _gameManager.UpdateTechNodes(_allNodes, _nanoCount);

            bool prestigeAvail = _prestigeEnabled && _currentEra >= 1;
            _gameManager.UpdatePrestigeButton(prestigeAvail, 0);
        }

        void TriggerMutation()
        {
            bool positive = _mutationControlOwned || (Random.value > 0.4f);
            _currentMutationPositive = positive;

            if (positive)
            {
                _clickPower *= 1.3f;
                _gameManager.ShowMutationEvent(true, "有益な変異！タップ効率が上昇");
            }
            else
            {
                _autoRate = Mathf.Max(0, _autoRate * 0.8f);
                _gameManager.ShowMutationEvent(false, "有害な変異…自動増殖が低下");
            }
        }

        IEnumerator TechUnlockFeedback()
        {
            // Brief visual feedback via scale pulse - handled in UI
            yield return null;
        }

        IEnumerator PrestigeFeedback()
        {
            // Flash overlay and era text - handled in UI
            yield return null;
        }

        void RefreshUI()
        {
            _gameManager.UpdateNanoCount(_nanoCount);
            _gameManager.UpdateEra(_currentEra, EraNames[_currentEra]);
            _gameManager.UpdateAutoRate((_autoRate + _autoRateBonus) * _prestigeMultiplier);
            _gameManager.UpdatePrestigeMultiplier(_prestigeMultiplier);
            _gameManager.UpdateScore(_score);
            _gameManager.UpdateTechNodes(_allNodes, _nanoCount);
            _gameManager.UpdatePrestigeButton(_prestigeEnabled && _currentEra >= 1, 0);
        }
    }
}
