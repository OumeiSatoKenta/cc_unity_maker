using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game090v2_StarshipCrew
{
    public enum SkillType { Combat, Engineering, Medical }
    public enum Difficulty { Easy, Medium, Hard, VeryHard, Boss }
    public enum EquipType { None, Combat, Engine, Medical }

    [System.Serializable]
    public class CrewData
    {
        public string crewName;
        public int combatSkill;
        public int engSkill;
        public int medSkill;
        public int[] goodCompat;   // indices of good-compatible crew
        public int[] badCompat;    // indices of bad-compatible crew
        public Sprite portrait;
        public bool isAvailable = true;
    }

    [System.Serializable]
    public class MissionData
    {
        public string missionName;
        public SkillType requiredSkillType;
        public int requiredSkillValue;
        public SkillType secondarySkillType;
        public int secondarySkillValue;     // 0 = no secondary requirement
        public Difficulty difficulty;
        public int baseScore;
        public bool isCompleted;

        public float GetDifficultyMultiplier()
        {
            return difficulty switch
            {
                Difficulty.Easy     => 1.0f,
                Difficulty.Medium   => 1.2f,
                Difficulty.Hard     => 1.5f,
                Difficulty.VeryHard => 2.0f,
                Difficulty.Boss     => 3.0f,
                _ => 1.0f
            };
        }
    }

    public class CrewManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] StarshipCrewGameManager _gameManager;
        [SerializeField] StarshipCrewUI _ui;

        [Header("Crew Portraits (0-9)")]
        [SerializeField] Sprite _sprCrew0;
        [SerializeField] Sprite _sprCrew1;
        [SerializeField] Sprite _sprCrew2;
        [SerializeField] Sprite _sprCrew3;
        [SerializeField] Sprite _sprCrew4;
        [SerializeField] Sprite _sprCrew5;
        [SerializeField] Sprite _sprCrew6;
        [SerializeField] Sprite _sprCrew7;
        [SerializeField] Sprite _sprCrew8;
        [SerializeField] Sprite _sprCrew9;

        [Header("Mission Sprites")]
        [SerializeField] Sprite _sprMissionEasy;
        [SerializeField] Sprite _sprMissionMedium;
        [SerializeField] Sprite _sprMissionHard;
        [SerializeField] Sprite _sprMissionVeryHard;
        [SerializeField] Sprite _sprMissionBoss;

        [Header("Equipment Sprites")]
        [SerializeField] Sprite _sprEquipCombat;
        [SerializeField] Sprite _sprEquipEngine;
        [SerializeField] Sprite _sprEquipMedical;

        bool _isActive;
        int _stageIndex;
        bool _hasEquipmentSystem;
        bool _hasCompatSystem;
        bool _hasEmergencyEvents;

        // All 10 crew definitions
        CrewData[] _allCrew;
        // Active crew for current stage
        List<CrewData> _stageCrew = new List<CrewData>();
        // Selected crew indices (for current dispatch)
        List<int> _selectedCrewIndices = new List<int>();

        // Current stage missions
        MissionData[] _currentMissions;
        int _selectedMissionIndex = -1;

        // Equipment in use
        EquipType _selectedEquip = EquipType.None;

        // Experience tracking per crew (skill bonus accumulation)
        int[] _crewExpBonus; // index matches _allCrew

        // Stage mission definitions
        static readonly int[][] StageMissionIndices = {
            new[] { 0, 1, 2 },
            new[] { 3, 4, 5, 6 },
            new[] { 7, 8, 9, 6, 10 },
            new[] { 10, 11, 8, 9, 12, 3 },
            new[] { 13, 14, 11, 12, 9, 10, 4 },
        };
        static readonly int[] StageCrewCounts = { 3, 5, 6, 8, 10 };

        void Awake()
        {
            _allCrew = BuildAllCrew();
            _crewExpBonus = new int[_allCrew.Length];
        }

        CrewData[] BuildAllCrew()
        {
            var portraits = new Sprite[] {
                _sprCrew0, _sprCrew1, _sprCrew2, _sprCrew3, _sprCrew4,
                _sprCrew5, _sprCrew6, _sprCrew7, _sprCrew8, _sprCrew9
            };
            return new CrewData[]
            {
                new CrewData { crewName="艦長オリン",    combatSkill=80, engSkill=30, medSkill=20, goodCompat=new[]{1,3},  badCompat=new[]{5},  portrait=portraits[0] },
                new CrewData { crewName="エンジニアKai", combatSkill=20, engSkill=90, medSkill=30, goodCompat=new[]{0,2},  badCompat=new int[0], portrait=portraits[1] },
                new CrewData { crewName="医療士Nora",    combatSkill=10, engSkill=30, medSkill=85, goodCompat=new[]{1,4},  badCompat=new int[0], portrait=portraits[2] },
                new CrewData { crewName="パイロットRex", combatSkill=60, engSkill=70, medSkill=20, goodCompat=new[]{0,4},  badCompat=new int[0], portrait=portraits[3] },
                new CrewData { crewName="科学者Lyra",    combatSkill=20, engSkill=50, medSkill=60, goodCompat=new[]{2,3},  badCompat=new int[0], portrait=portraits[4] },
                new CrewData { crewName="傭兵Zack",      combatSkill=95, engSkill=20, medSkill=10, goodCompat=new[]{7},    badCompat=new[]{0},  portrait=portraits[5] },
                new CrewData { crewName="整備士Mia",     combatSkill=30, engSkill=80, medSkill=40, goodCompat=new[]{1,8},  badCompat=new int[0], portrait=portraits[6] },
                new CrewData { crewName="交渉人Sora",    combatSkill=40, engSkill=40, medSkill=50, goodCompat=new[]{5,9},  badCompat=new int[0], portrait=portraits[7] },
                new CrewData { crewName="狙撃手Ares",    combatSkill=85, engSkill=25, medSkill=15, goodCompat=new[]{6},    badCompat=new int[0], portrait=portraits[8] },
                new CrewData { crewName="副官Luna",      combatSkill=50, engSkill=60, medSkill=70, goodCompat=new[]{7,2},  badCompat=new int[0], portrait=portraits[9] },
            };
        }

        MissionData[] BuildAllMissions()
        {
            return new MissionData[]
            {
                new MissionData { missionName="偵察任務",      requiredSkillType=SkillType.Combat,      requiredSkillValue=30, secondarySkillValue=0, difficulty=Difficulty.Easy,     baseScore=30 },
                new MissionData { missionName="エンジン修理",  requiredSkillType=SkillType.Engineering, requiredSkillValue=50, secondarySkillValue=0, difficulty=Difficulty.Medium,   baseScore=30 },
                new MissionData { missionName="負傷者救助",    requiredSkillType=SkillType.Medical,     requiredSkillValue=40, secondarySkillValue=0, difficulty=Difficulty.Medium,   baseScore=30 },
                new MissionData { missionName="海賊撃退",      requiredSkillType=SkillType.Combat,      requiredSkillValue=60, secondarySkillValue=0, difficulty=Difficulty.Hard,     baseScore=30 },
                new MissionData { missionName="ワープ航法",    requiredSkillType=SkillType.Engineering, requiredSkillValue=70, secondarySkillValue=0, difficulty=Difficulty.Hard,     baseScore=30 },
                new MissionData { missionName="疫病対応",      requiredSkillType=SkillType.Medical,     requiredSkillValue=60, secondarySkillValue=0, difficulty=Difficulty.Hard,     baseScore=30 },
                new MissionData { missionName="惑星探索",      requiredSkillType=SkillType.Combat,      requiredSkillValue=40, secondarySkillType=SkillType.Engineering, secondarySkillValue=40, difficulty=Difficulty.Medium, baseScore=30 },
                new MissionData { missionName="外交交渉",      requiredSkillType=SkillType.Medical,     requiredSkillValue=30, secondarySkillType=SkillType.Engineering, secondarySkillValue=40, difficulty=Difficulty.Medium, baseScore=30 },
                new MissionData { missionName="小惑星帯突破",  requiredSkillType=SkillType.Engineering, requiredSkillValue=80, secondarySkillValue=0, difficulty=Difficulty.VeryHard, baseScore=30 },
                new MissionData { missionName="高度医療",      requiredSkillType=SkillType.Medical,     requiredSkillValue=80, secondarySkillValue=0, difficulty=Difficulty.VeryHard, baseScore=30 },
                new MissionData { missionName="基地強襲",      requiredSkillType=SkillType.Combat,      requiredSkillValue=80, secondarySkillValue=0, difficulty=Difficulty.VeryHard, baseScore=30 },
                new MissionData { missionName="緊急脱出",      requiredSkillType=SkillType.Combat,      requiredSkillValue=50, secondarySkillType=SkillType.Engineering, secondarySkillValue=50, difficulty=Difficulty.Hard,     baseScore=30 },
                new MissionData { missionName="文明接触",      requiredSkillType=SkillType.Medical,     requiredSkillValue=50, secondarySkillType=SkillType.Engineering, secondarySkillValue=50, difficulty=Difficulty.VeryHard, baseScore=30 },
                new MissionData { missionName="艦隊決戦",      requiredSkillType=SkillType.Combat,      requiredSkillValue=90, secondarySkillValue=0, difficulty=Difficulty.Boss,     baseScore=30 },
                new MissionData { missionName="最終ミッション", requiredSkillType=SkillType.Combat,     requiredSkillValue=70, secondarySkillType=SkillType.Engineering, secondarySkillValue=70, difficulty=Difficulty.Boss, baseScore=30 },
            };
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _isActive = true;
            _stageIndex = stageIndex;
            _selectedCrewIndices.Clear();
            _selectedMissionIndex = -1;
            _selectedEquip = EquipType.None;

            _hasCompatSystem      = stageIndex >= 1;
            _hasEquipmentSystem   = stageIndex >= 2;
            _hasEmergencyEvents   = stageIndex >= 3;

            // Reset all crew availability
            foreach (var c in _allCrew) c.isAvailable = true;

            int crewCount = StageCrewCounts[Mathf.Clamp(stageIndex, 0, StageCrewCounts.Length - 1)];
            _stageCrew.Clear();
            for (int i = 0; i < Mathf.Min(crewCount, _allCrew.Length); i++)
                _stageCrew.Add(_allCrew[i]);

            // Build missions for this stage
            var allMissions = BuildAllMissions();
            var missionIndices = StageMissionIndices[Mathf.Clamp(stageIndex, 0, StageMissionIndices.Length - 1)];
            _currentMissions = new MissionData[missionIndices.Length];
            for (int i = 0; i < missionIndices.Length; i++)
                _currentMissions[i] = allMissions[missionIndices[i]];

            // Apply complexityFactor to increase required skills
            float complexity = config.complexityFactor;
            foreach (var m in _currentMissions)
                m.requiredSkillValue = Mathf.RoundToInt(m.requiredSkillValue * (1f + complexity * 0.5f));

            _ui.SetupCrewCards(_stageCrew, _allCrew);
            _ui.SetupMissionButtons(_currentMissions, GetMissionSprite);
            _ui.SetupEquipmentPanel(_hasEquipmentSystem);
            _ui.UpdateDispatchButton(false, 0f);
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active && _resultCoroutine != null)
            {
                StopCoroutine(_resultCoroutine);
                _resultCoroutine = null;
            }
        }

        // Called by UI when a crew card is tapped
        public void OnCrewCardClicked(int stageCrewIndex)
        {
            if (!_isActive) return;

            // Find actual crew index in _allCrew
            if (stageCrewIndex < 0 || stageCrewIndex >= _stageCrew.Count) return;

            if (_selectedCrewIndices.Contains(stageCrewIndex))
            {
                _selectedCrewIndices.Remove(stageCrewIndex);
            }
            else
            {
                if (_selectedCrewIndices.Count >= 3) return; // max 3
                _selectedCrewIndices.Add(stageCrewIndex);
            }

            UpdateSelectionUI();
        }

        // Called by UI when a mission is tapped
        public void OnMissionSelected(int missionIndex)
        {
            if (!_isActive) return;
            if (missionIndex < 0 || missionIndex >= _currentMissions.Length) return;
            if (_currentMissions[missionIndex].isCompleted) return;

            _selectedMissionIndex = missionIndex;
            UpdateSelectionUI();
        }

        public void OnCancelClicked()
        {
            if (!_isActive) return;
            _selectedCrewIndices.Clear();
            _selectedMissionIndex = -1;
            _selectedEquip = EquipType.None;
            UpdateSelectionUI();
        }

        public void OnEquipSelected(EquipType equip)
        {
            if (!_isActive || !_hasEquipmentSystem) return;
            _selectedEquip = (_selectedEquip == equip) ? EquipType.None : equip;
            UpdateSelectionUI();
        }

        void UpdateSelectionUI()
        {
            float successRate = 0f;
            bool canDispatch = _selectedCrewIndices.Count > 0 && _selectedMissionIndex >= 0;
            if (canDispatch)
                successRate = CalculateSuccessRate(_selectedCrewIndices, _selectedMissionIndex, _selectedEquip);

            _ui.UpdateCrewSelection(_selectedCrewIndices);
            _ui.UpdateMissionSelection(_selectedMissionIndex, successRate);
            _ui.UpdateDispatchButton(canDispatch, successRate);
            if (_hasCompatSystem)
                _ui.ShowCompatibility(_selectedCrewIndices, _stageCrew);
        }

        float CalculateSuccessRate(List<int> crewIndices, int missionIndex, EquipType equip)
        {
            if (missionIndex < 0 || missionIndex >= _currentMissions.Length) return 0f;
            var mission = _currentMissions[missionIndex];

            // Sum relevant skills from selected crew
            int totalPrimary = 0, totalSecondary = 0;
            foreach (int ci in crewIndices)
            {
                var crew = _stageCrew[ci];
                int allIdx = GetAllCrewIndex(crew);
                int expBonus = allIdx >= 0 ? _crewExpBonus[allIdx] : 0;
                totalPrimary   += GetSkill(crew, mission.requiredSkillType)   + expBonus;
                if (mission.secondarySkillValue > 0)
                    totalSecondary += GetSkill(crew, mission.secondarySkillType) + expBonus;
            }

            float primaryRatio = mission.requiredSkillValue > 0
                ? Mathf.Clamp01((float)totalPrimary / mission.requiredSkillValue)
                : 1f;
            float secondaryRatio = mission.secondarySkillValue > 0
                ? Mathf.Clamp01((float)totalSecondary / mission.secondarySkillValue)
                : 1f;

            float baseRate = (primaryRatio * 0.7f + secondaryRatio * 0.3f) * 100f;

            // Compatibility bonus/penalty
            if (_hasCompatSystem)
            {
                float compatBonus = CalculateCompatBonus(crewIndices);
                baseRate += compatBonus;
            }

            // Equipment bonus
            if (_hasEquipmentSystem && equip != EquipType.None)
            {
                float equipBonus = GetEquipBonus(equip, mission.requiredSkillType);
                baseRate += equipBonus;
            }

            // Emergency event penalty (stage 4+)
            if (_hasEmergencyEvents)
                baseRate -= 10f;

            return Mathf.Clamp(baseRate, 5f, 99f);
        }

        float CalculateCompatBonus(List<int> crewIndices)
        {
            float bonus = 0f;
            for (int a = 0; a < crewIndices.Count; a++)
            {
                var crewA = _stageCrew[crewIndices[a]];
                int allIdxA = GetAllCrewIndex(crewA);
                for (int b = a + 1; b < crewIndices.Count; b++)
                {
                    int allIdxB = GetAllCrewIndex(_stageCrew[crewIndices[b]]);
                    // Compare using _allCrew indices (which goodCompat/badCompat reference)
                    if (allIdxB >= 0 && System.Array.IndexOf(crewA.goodCompat, allIdxB) >= 0)
                        bonus += 20f;
                    if (allIdxB >= 0 && System.Array.IndexOf(crewA.badCompat, allIdxB) >= 0)
                        bonus -= 15f;
                }
            }
            return bonus;
        }

        float GetEquipBonus(EquipType equip, SkillType missionSkill)
        {
            // Equipment gives +15% if it matches mission type, +8% otherwise
            bool matches = (equip == EquipType.Combat  && missionSkill == SkillType.Combat)
                        || (equip == EquipType.Engine   && missionSkill == SkillType.Engineering)
                        || (equip == EquipType.Medical  && missionSkill == SkillType.Medical);
            return matches ? 20f : 8f;
        }

        int GetSkill(CrewData crew, SkillType skill)
        {
            return skill switch
            {
                SkillType.Combat       => crew.combatSkill,
                SkillType.Engineering  => crew.engSkill,
                SkillType.Medical      => crew.medSkill,
                _ => 0
            };
        }

        int GetAllCrewIndex(CrewData crew)
        {
            for (int i = 0; i < _allCrew.Length; i++)
                if (_allCrew[i] == crew) return i;
            return -1;
        }

        Coroutine _resultCoroutine;

        // Called by UI dispatch button
        public void OnDispatchClicked()
        {
            if (!_isActive) return;
            if (_selectedCrewIndices.Count == 0 || _selectedMissionIndex < 0) return;
            if (_resultCoroutine != null) return; // prevent double-dispatch

            float successRate = CalculateSuccessRate(_selectedCrewIndices, _selectedMissionIndex, _selectedEquip);
            float roll = Random.Range(0f, 100f);
            bool success = roll < successRate;
            bool isPerfect = success && (successRate >= 90f);

            // Capture mission info BEFORE clearing selection
            float diffMult = _currentMissions[_selectedMissionIndex].GetDifficultyMultiplier();
            int baseScore  = _currentMissions[_selectedMissionIndex].baseScore;

            // Count synergy pairs
            int synergyCount = CountSynergyPairs(_selectedCrewIndices);

            // Grant experience bonus to used crew on success
            if (success)
            {
                foreach (int ci in _selectedCrewIndices)
                {
                    int allIdx = GetAllCrewIndex(_stageCrew[ci]);
                    if (allIdx >= 0)
                        _crewExpBonus[allIdx] = Mathf.Min(_crewExpBonus[allIdx] + 5, 30);
                }
            }

            if (success)
                _currentMissions[_selectedMissionIndex].isCompleted = true;

            // Cleanup selection
            _selectedCrewIndices.Clear();
            _selectedMissionIndex = -1;
            _selectedEquip = EquipType.None;

            _ui.SetupMissionButtons(_currentMissions, GetMissionSprite);
            _ui.UpdateCrewSelection(_selectedCrewIndices);
            _ui.UpdateDispatchButton(false, 0f);

            _resultCoroutine = StartCoroutine(ShowResultWithDelay(success, isPerfect, synergyCount, diffMult, baseScore));
        }

        IEnumerator ShowResultWithDelay(bool success, bool isPerfect, int synergyCount, float diffMult, int baseScore)
        {
            yield return new WaitForSeconds(0.1f);

            if (success)
                _gameManager.OnMissionSuccess(baseScore, isPerfect, synergyCount, diffMult);
            else
                _gameManager.OnMissionFailed();

            _resultCoroutine = null;
        }

        int CountSynergyPairs(List<int> crewIndices)
        {
            if (!_hasCompatSystem) return 0;
            int count = 0;
            for (int a = 0; a < crewIndices.Count; a++)
            {
                int allIdxB_outer = GetAllCrewIndex(_stageCrew[crewIndices[a]]);
                for (int b = a + 1; b < crewIndices.Count; b++)
                {
                    int allIdxB = GetAllCrewIndex(_stageCrew[crewIndices[b]]);
                    if (allIdxB >= 0 && System.Array.IndexOf(_stageCrew[crewIndices[a]].goodCompat, allIdxB) >= 0)
                        count++;
                }
            }
            return count;
        }

        Sprite GetMissionSprite(Difficulty diff)
        {
            return diff switch
            {
                Difficulty.Easy     => _sprMissionEasy,
                Difficulty.Medium   => _sprMissionMedium,
                Difficulty.Hard     => _sprMissionHard,
                Difficulty.VeryHard => _sprMissionVeryHard,
                Difficulty.Boss     => _sprMissionBoss,
                _ => _sprMissionEasy
            };
        }

        public Sprite GetEquipSprite(EquipType equip)
        {
            return equip switch
            {
                EquipType.Combat  => _sprEquipCombat,
                EquipType.Engine  => _sprEquipEngine,
                EquipType.Medical => _sprEquipMedical,
                _ => null
            };
        }
    }
}
