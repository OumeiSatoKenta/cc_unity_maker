using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game085v2_MechPet
{
    public class MechPetManager : MonoBehaviour
    {
        public enum PartType { Normal, Speed, Shield, Heavy, Legendary }
        public enum SlotIndex { Head = 0, Body = 1, Arm = 2, Leg = 3 }

        [System.Serializable]
        public class Part
        {
            public string partName;
            public PartType partType;
            public int attack;
            public int defense;
            public int speed;
            public Sprite sprite;
        }

        [SerializeField] MechPetGameManager _gameManager;
        [SerializeField] MechPetUI _ui;

        [SerializeField] SpriteRenderer _headRenderer;
        [SerializeField] SpriteRenderer _bodyRenderer;
        [SerializeField] SpriteRenderer _leftArmRenderer;
        [SerializeField] SpriteRenderer _rightArmRenderer;
        [SerializeField] SpriteRenderer _legRenderer;

        // Part sprites by type and slot
        [SerializeField] Sprite _headNormal;
        [SerializeField] Sprite _headSpeed;
        [SerializeField] Sprite _headShield;
        [SerializeField] Sprite _headHeavy;
        [SerializeField] Sprite _headLegendary;

        [SerializeField] Sprite _bodyNormal;
        [SerializeField] Sprite _bodySpeed;
        [SerializeField] Sprite _bodyShield;
        [SerializeField] Sprite _bodyHeavy;
        [SerializeField] Sprite _bodyLegendary;

        [SerializeField] Sprite _armNormal;
        [SerializeField] Sprite _armSpeed;
        [SerializeField] Sprite _armShield;
        [SerializeField] Sprite _armHeavy;
        [SerializeField] Sprite _armLegendary;

        [SerializeField] Sprite _legNormal;
        [SerializeField] Sprite _legSpeed;
        [SerializeField] Sprite _legShield;
        [SerializeField] Sprite _legHeavy;
        [SerializeField] Sprite _legLegendary;

        readonly List<Part>[] _availableParts = new List<Part>[4];
        readonly int[] _selectedIndex = new int[4];

        float _energy = 100f;
        const float MaxEnergy = 100f;
        float _chargeAmount = 20f;
        float _missionCost = 20f;

        bool _synergyEnabled;
        bool _legendaryEnabled;
        bool _isActive;
        int _stageIndex;
        public int StageTargetScore { get; private set; } = int.MaxValue;

        static readonly int[] TargetScores = { 100, 200, 350, 500, 700 };

        void Awake()
        {
            for (int i = 0; i < 4; i++)
                _availableParts[i] = new List<Part>();
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _isActive = true;
            _energy = MaxEnergy;
            _synergyEnabled = stageIndex >= 1;
            _legendaryEnabled = stageIndex >= 4;

            // Charge cost increases from stage 3
            _chargeAmount = 20f;
            _missionCost = stageIndex >= 2 ? 25f : 20f;

            StageTargetScore = TargetScores[Mathf.Clamp(stageIndex, 0, 4)];

            BuildPartLists(stageIndex);

            for (int i = 0; i < 4; i++)
                _selectedIndex[i] = 0;

            RefreshRobotDisplay();
            UpdateUIState();
            _gameManager.UpdateEnergyDisplay(_energy / MaxEnergy);

            // Position robot using responsive camera coords
            PositionRobotParts();
        }

        void BuildPartLists(int stageIndex)
        {
            // Number of part types available per stage (grows by stage)
            // Stage 0: 1 type, Stage 1: 2 types, Stage 2: 3 types, Stage 3: 4 types, Stage 4: 5 types
            int maxTypes = Mathf.Clamp(stageIndex + 1, 1, 5);

            PartType[] types = { PartType.Normal, PartType.Speed, PartType.Shield, PartType.Heavy, PartType.Legendary };

            for (int slot = 0; slot < 4; slot++)
            {
                _availableParts[slot].Clear();
                for (int t = 0; t < maxTypes; t++)
                {
                    if (types[t] == PartType.Legendary && !_legendaryEnabled) continue;
                    _availableParts[slot].Add(CreatePart((SlotIndex)slot, types[t]));
                }
            }
        }

        Part CreatePart(SlotIndex slot, PartType type)
        {
            var p = new Part { partType = type };
            switch (type)
            {
                case PartType.Normal:
                    p.partName = "ノーマル";
                    p.attack = 5; p.defense = 5; p.speed = 5;
                    p.sprite = slot == SlotIndex.Head ? _headNormal
                             : slot == SlotIndex.Body ? _bodyNormal
                             : slot == SlotIndex.Arm  ? _armNormal
                             : _legNormal;
                    break;
                case PartType.Speed:
                    p.partName = "スピード";
                    p.attack = 4; p.defense = 3; p.speed = 10;
                    p.sprite = slot == SlotIndex.Head ? _headSpeed
                             : slot == SlotIndex.Body ? _bodySpeed
                             : slot == SlotIndex.Arm  ? _armSpeed
                             : _legSpeed;
                    break;
                case PartType.Shield:
                    p.partName = "シールド";
                    p.attack = 3; p.defense = 12; p.speed = 3;
                    p.sprite = slot == SlotIndex.Head ? _headShield
                             : slot == SlotIndex.Body ? _bodyShield
                             : slot == SlotIndex.Arm  ? _armShield
                             : _legShield;
                    break;
                case PartType.Heavy:
                    p.partName = "ヘビー";
                    p.attack = 12; p.defense = 6; p.speed = 2;
                    p.sprite = slot == SlotIndex.Head ? _headHeavy
                             : slot == SlotIndex.Body ? _bodyHeavy
                             : slot == SlotIndex.Arm  ? _armHeavy
                             : _legHeavy;
                    break;
                case PartType.Legendary:
                    p.partName = "レジェンド";
                    p.attack = 15; p.defense = 15; p.speed = 15;
                    p.sprite = slot == SlotIndex.Head ? _headLegendary
                             : slot == SlotIndex.Body ? _bodyLegendary
                             : slot == SlotIndex.Arm  ? _armLegendary
                             : _legLegendary;
                    break;
            }
            return p;
        }

        void PositionRobotParts()
        {
            if (Camera.main == null) return;
            float camSize = Camera.main.orthographicSize;
            float topY = camSize - 1.5f;

            if (_headRenderer != null)
                _headRenderer.transform.localPosition = new Vector3(0f, topY - 0.8f, 0f);
            if (_bodyRenderer != null)
                _bodyRenderer.transform.localPosition = new Vector3(0f, topY - 2.2f, 0f);
            if (_leftArmRenderer != null)
                _leftArmRenderer.transform.localPosition = new Vector3(-1.2f, topY - 2.2f, 0f);
            if (_rightArmRenderer != null)
                _rightArmRenderer.transform.localPosition = new Vector3(1.2f, topY - 2.2f, 0f);
            if (_legRenderer != null)
                _legRenderer.transform.localPosition = new Vector3(0f, topY - 3.5f, 0f);
        }

        void RefreshRobotDisplay()
        {
            SpriteRenderer[] renderers = { _headRenderer, _bodyRenderer, _leftArmRenderer, _legRenderer };
            for (int i = 0; i < 4; i++)
            {
                if (renderers[i] == null) continue;
                var list = _availableParts[i];
                if (list.Count == 0) continue;
                int idx = Mathf.Clamp(_selectedIndex[i], 0, list.Count - 1);
                renderers[i].sprite = list[idx].sprite;
                renderers[i].color = Color.white;
            }
            // Right arm mirrors left arm
            if (_rightArmRenderer != null && _leftArmRenderer != null)
            {
                _rightArmRenderer.sprite = _leftArmRenderer.sprite;
                _rightArmRenderer.flipX = true;
                _rightArmRenderer.color = Color.white;
            }
        }

        void UpdateUIState()
        {
            // Update slot button labels
            string[] slotNames = { "頭", "胴体", "腕", "脚" };
            for (int i = 0; i < 4; i++)
            {
                var list = _availableParts[i];
                string partName = list.Count > 0 ? list[Mathf.Clamp(_selectedIndex[i], 0, list.Count - 1)].partName : "-";
                _ui.UpdateSlotLabel(i, slotNames[i], partName);
            }
            UpdateSynergyDisplay();
        }

        void UpdateSynergyDisplay()
        {
            if (!_synergyEnabled)
            {
                _gameManager.UpdateSynergyDisplay("");
                return;
            }
            float bonus = CalculateSynergyBonus();
            Part[] current = GetCurrentParts();
            // Count types
            var typeCounts = new Dictionary<PartType, int>();
            foreach (var p in current)
            {
                if (!typeCounts.ContainsKey(p.partType)) typeCounts[p.partType] = 0;
                typeCounts[p.partType]++;
            }
            string synergyText = "";
            foreach (var kv in typeCounts)
            {
                if (kv.Value >= 2)
                {
                    string typeName = kv.Key == PartType.Normal ? "ノーマル"
                                    : kv.Key == PartType.Speed ? "スピード"
                                    : kv.Key == PartType.Shield ? "シールド"
                                    : kv.Key == PartType.Heavy ? "ヘビー"
                                    : "レジェンド";
                    synergyText += $"{typeName}×{kv.Value} ";
                }
            }
            if (bonus > 1.0f)
                _gameManager.UpdateSynergyDisplay($"シナジー！ ×{bonus:F1}  {synergyText.Trim()}");
            else
                _gameManager.UpdateSynergyDisplay("シナジーなし");
        }

        Part[] GetCurrentParts()
        {
            var parts = new Part[4];
            for (int i = 0; i < 4; i++)
            {
                var list = _availableParts[i];
                parts[i] = list.Count > 0 ? list[Mathf.Clamp(_selectedIndex[i], 0, list.Count - 1)] : CreatePart((SlotIndex)i, PartType.Normal);
            }
            return parts;
        }

        float CalculateSynergyBonus()
        {
            if (!_synergyEnabled) return 1.0f;
            var typeCounts = new Dictionary<PartType, int>();
            foreach (var p in GetCurrentParts())
            {
                if (!typeCounts.ContainsKey(p.partType)) typeCounts[p.partType] = 0;
                typeCounts[p.partType]++;
            }
            float best = 1.0f;
            foreach (var kv in typeCounts)
            {
                float multiplier = kv.Value >= 4 ? 1.8f
                                 : kv.Value == 3 ? 1.5f
                                 : kv.Value == 2 ? 1.3f
                                 : 1.0f;
                if (multiplier > best) best = multiplier;
            }
            return best;
        }

        public void CycleSlot(int slotIndex)
        {
            if (!_isActive) return;
            var list = _availableParts[slotIndex];
            if (list.Count == 0) return;
            _selectedIndex[slotIndex] = (_selectedIndex[slotIndex] + 1) % list.Count;
            RefreshRobotDisplay();
            UpdateUIState();
            StartCoroutine(PulseRendererAt(slotIndex));
        }

        public void ChargeEnergy()
        {
            if (!_isActive) return;
            _energy = Mathf.Min(_energy + _chargeAmount, MaxEnergy);
            _gameManager.UpdateEnergyDisplay(_energy / MaxEnergy);
            StartCoroutine(PulseAllRenderers(new Color(0.5f, 1f, 0.5f)));
        }

        public void StartMission()
        {
            if (!_isActive) return;
            if (_energy < _missionCost) { _ui.ShowMessage("エネルギー不足！"); return; }
            _isActive = false;
            _energy = Mathf.Max(0f, _energy - _missionCost);
            _gameManager.UpdateEnergyDisplay(_energy / MaxEnergy);
            StartCoroutine(RunMission());
        }

        IEnumerator RunMission()
        {
            // Brief animation before result
            yield return StartCoroutine(PulseAllRenderers(new Color(1f, 1f, 0.5f)));
            if (!gameObject.activeInHierarchy) yield break;
            yield return new WaitForSeconds(0.3f);
            if (!gameObject.activeInHierarchy) yield break;

            Part[] parts = GetCurrentParts();
            int totalPower = 0;
            foreach (var p in parts)
                totalPower += p.attack + p.defense + p.speed;
            float synergy = CalculateSynergyBonus();
            float energyFactor = Mathf.Lerp(0.5f, 1.0f, _energy / MaxEnergy);
            float effectivePower = totalPower * synergy * energyFactor;

            // Stage-based difficulty threshold
            float[] thresholds = { 20f, 30f, 45f, 60f, 80f };
            float threshold = thresholds[Mathf.Clamp(_stageIndex, 0, 4)];
            float complexityBoost = 1f + _stageIndex * 0.1f;
            threshold *= complexityBoost;

            bool success = effectivePower >= threshold;
            int baseScore = success ? Mathf.RoundToInt(effectivePower * 2f) : 0;

            if (success)
                yield return StartCoroutine(PulseAllRenderers(new Color(0.5f, 1f, 0.5f)));
            else
                yield return StartCoroutine(FlashAllRenderers(new Color(1f, 0.3f, 0.3f)));
            if (!gameObject.activeInHierarchy) yield break;

            // Call OnMissionResult first; it may call SetActive(false) for stage clear.
            // Only restore _isActive if we're still in an active state after the result.
            _gameManager.OnMissionResult(success, baseScore);
            if (gameObject.activeInHierarchy)
                _isActive = true;
        }

        IEnumerator PulseRendererAt(int slot)
        {
            SpriteRenderer[] renderers = { _headRenderer, _bodyRenderer, _leftArmRenderer, _legRenderer };
            if (slot >= renderers.Length || renderers[slot] == null) yield break;
            var t = renderers[slot].transform;
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.2f;
                float scale = ratio < 0.5f ? Mathf.Lerp(1f, 1.3f, ratio * 2f) : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                t.localScale = orig * scale;
                yield return null;
            }
            t.localScale = orig;
        }

        IEnumerator PulseAllRenderers(Color flashColor)
        {
            SpriteRenderer[] renderers = { _headRenderer, _bodyRenderer, _leftArmRenderer, _rightArmRenderer, _legRenderer };
            float dur = 0.15f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / dur;
                float scale = ratio < 0.5f ? Mathf.Lerp(1f, 1.25f, ratio * 2f) : Mathf.Lerp(1.25f, 1f, (ratio - 0.5f) * 2f);
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    r.transform.localScale = Vector3.one * scale;
                    r.color = Color.Lerp(Color.white, flashColor, Mathf.Sin(ratio * Mathf.PI));
                }
                yield return null;
            }
            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.transform.localScale = Vector3.one;
                r.color = Color.white;
            }
        }

        IEnumerator FlashAllRenderers(Color flashColor)
        {
            SpriteRenderer[] renderers = { _headRenderer, _bodyRenderer, _leftArmRenderer, _rightArmRenderer, _legRenderer };
            float dur = 0.3f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / dur;
                Color c = Color.Lerp(flashColor, Color.white, ratio);
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    r.color = c;
                }
                yield return null;
            }
            foreach (var r in renderers)
            {
                if (r != null) r.color = Color.white;
            }
        }

        public void SetActive(bool active) => _isActive = active;

        void OnDestroy()
        {
            // No dynamic textures to clean up
        }
    }
}
