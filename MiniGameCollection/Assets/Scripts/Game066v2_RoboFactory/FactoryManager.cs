using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game066v2_RoboFactory
{
    public enum RobotType { Worker, Miner, Builder, Repair, Power, AI }
    public enum BuildingType { House, Factory, PowerPlant, Lab, MiningDrill, AICore }

    [System.Serializable]
    public class RobotData
    {
        public RobotType type;
        public int count;
        public float collectRate;  // resources/sec
        public int oreCost;
        public int energyCost;
        public int partsCost;
        public bool isBroken;
    }

    [System.Serializable]
    public class BuildingData
    {
        public BuildingType type;
        public int oreCost;
        public int energyCost;
        public int partsCost;
        public int cityLevelValue;
        public SpriteRenderer spriteRenderer;
        public GameObject gameObject;
        public bool isBuilt;
    }

    public class FactoryManager : MonoBehaviour
    {
        [SerializeField] RoboFactoryGameManager _gameManager;
        [SerializeField] RoboFactoryUI _ui;

        [SerializeField] Sprite _workerBotSprite;
        [SerializeField] Sprite _minerBotSprite;
        [SerializeField] Sprite _builderBotSprite;
        [SerializeField] Sprite _repairBotSprite;
        [SerializeField] Sprite _powerBotSprite;
        [SerializeField] Sprite _aiBotSprite;
        [SerializeField] Sprite _brokenWarningSprite;

        [SerializeField] Sprite _buildingHouseSprite;
        [SerializeField] Sprite _buildingFactorySprite;
        [SerializeField] Sprite _buildingPowerPlantSprite;
        [SerializeField] Sprite _buildingLabSprite;
        [SerializeField] Sprite _buildingMiningDrillSprite;
        [SerializeField] Sprite _buildingAICoreSprite;
        [SerializeField] Sprite _emptyCellSprite;

        // Resources
        public float Ore { get; private set; }
        public float Energy { get; private set; }
        public float Parts { get; private set; }
        public int CityLevel { get; private set; } = 1;
        public int TotalScore { get; private set; }

        // Stage config
        int _targetCityLevel;
        float _speedMultiplier = 1f;
        int _stageIndex;
        bool _autoMiningEnabled;
        bool _autoBuilderEnabled;
        bool _breakdownEnabled;
        bool _energyManagementEnabled;
        bool _aiEnabled;
        bool _techTreeEnabled;
        bool _isActive;
        bool _stageClearPending;

        // Robots
        Dictionary<RobotType, RobotData> _robots = new();
        bool[] _robotUnlocked = new bool[6];

        // Buildings - 3x3 grid
        const int GridSize = 3;
        BuildingData[,] _grid = new BuildingData[GridSize, GridSize];
        SpriteRenderer[,] _cellRenderers = new SpriteRenderer[GridSize, GridSize];
        GameObject[,] _cellObjects = new GameObject[GridSize, GridSize];

        // Combo
        int _comboCount;
        float _comboTimer;
        float _comboMultiplier = 1f;
        const float ComboResetTime = 30f;

        // Research
        bool[] _researchDone = new bool[3]; // 0=efficiency, 1=new_robot, 2=new_building
        float _researchProgress;
        bool _isResearching;
        int _activeResearchIndex = -1;

        // Energy management
        float _energyCapacity = 100f;
        bool _energyShortage;

        // Broken robot tracking
        List<RobotType> _brokenRobots = new();

        // Cell world positions
        Vector3[,] _cellPositions = new Vector3[GridSize, GridSize];
        float _cellSize;

        // Coroutines
        Coroutine _autoCollectCoroutine;
        Coroutine _breakdownCoroutine;
        Coroutine _energyDrainCoroutine;

        // Score tracking
        int _baseScore;

        void Awake()
        {
            InitRobots();
        }

        void InitRobots()
        {
            _robots[RobotType.Worker]  = new RobotData { type = RobotType.Worker,  count = 1, collectRate = 0.5f, oreCost = 0,   energyCost = 0,  partsCost = 0   };
            _robots[RobotType.Miner]   = new RobotData { type = RobotType.Miner,   count = 0, collectRate = 1.5f, oreCost = 30,  energyCost = 20, partsCost = 10  };
            _robots[RobotType.Builder] = new RobotData { type = RobotType.Builder, count = 0, collectRate = 0,    oreCost = 50,  energyCost = 30, partsCost = 20  };
            _robots[RobotType.Repair]  = new RobotData { type = RobotType.Repair,  count = 0, collectRate = 0,    oreCost = 40,  energyCost = 25, partsCost = 15  };
            _robots[RobotType.Power]   = new RobotData { type = RobotType.Power,   count = 0, collectRate = 0,    oreCost = 60,  energyCost = 40, partsCost = 30  };
            _robots[RobotType.AI]      = new RobotData { type = RobotType.AI,      count = 0, collectRate = 5f,   oreCost = 200, energyCost = 150, partsCost = 100 };

            _robotUnlocked[(int)RobotType.Worker] = true;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _stageClearPending = false;
            _stageIndex = stageIndex;
            _speedMultiplier = config.speedMultiplier;

            // Reset for new stage
            Ore    = 50f;
            Energy = 30f;
            Parts  = 20f;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _comboTimer = 0f;
            _brokenRobots.Clear();
            _isResearching = false;
            _researchProgress = 0f;
            _energyShortage = false;

            // Stage unlocks
            _autoMiningEnabled        = stageIndex >= 1;
            _techTreeEnabled          = stageIndex >= 1;
            _autoBuilderEnabled       = stageIndex >= 2;
            _breakdownEnabled         = stageIndex >= 2;
            _energyManagementEnabled  = stageIndex >= 3;
            _aiEnabled                = stageIndex >= 4;

            // Unlock robots per stage
            _robotUnlocked[(int)RobotType.Worker]  = true;
            _robotUnlocked[(int)RobotType.Miner]   = stageIndex >= 1;
            _robotUnlocked[(int)RobotType.Builder]  = stageIndex >= 2;
            _robotUnlocked[(int)RobotType.Repair]  = stageIndex >= 2;
            _robotUnlocked[(int)RobotType.Power]   = stageIndex >= 3;
            _robotUnlocked[(int)RobotType.AI]      = stageIndex >= 4;

            // Reset robot counts (keep 1 worker)
            _robots[RobotType.Worker].count  = 1;
            _robots[RobotType.Miner].count   = 0;
            _robots[RobotType.Builder].count  = 0;
            _robots[RobotType.Repair].count  = 0;
            _robots[RobotType.Power].count   = 0;
            _robots[RobotType.AI].count      = 0;

            // Target city level per stage
            int[] targets = { 5, 15, 30, 50, 100 };
            _targetCityLevel = targets[Mathf.Clamp(stageIndex, 0, targets.Length - 1)];
            CityLevel = 1;

            // Setup grid
            SetupGrid();

            // Energy
            _energyCapacity = _energyManagementEnabled ? 100f : float.MaxValue;

            _isActive = true;

            // Start coroutines
            _autoCollectCoroutine = StartCoroutine(AutoCollectLoop());
            if (_breakdownEnabled)
                _breakdownCoroutine = StartCoroutine(BreakdownLoop());
            if (_energyManagementEnabled)
                _energyDrainCoroutine = StartCoroutine(EnergyDrainLoop());
            if (_autoBuilderEnabled)
                StartCoroutine(AutoBuilderLoop());

            _ui.SetupForStage(stageIndex, _robotUnlocked, _techTreeEnabled, _energyManagementEnabled, _aiEnabled);
            _ui.UpdateResources(Ore, Energy, Parts);
            _ui.UpdateCityLevel(CityLevel, _targetCityLevel);
            _ui.UpdateRobotCounts(_robots);
        }

        void SetupGrid()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            _cellSize = Mathf.Min(availableHeight / GridSize, camWidth * 2f / GridSize, 1.8f);

            float gridWidth = _cellSize * GridSize;
            float gridHeight = _cellSize * GridSize;
            float startX = -gridWidth / 2f + _cellSize / 2f;
            float startY = (camSize - topMargin) - _cellSize / 2f;

            // Clear old cells
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    if (_cellObjects[r, c] != null)
                        Destroy(_cellObjects[r, c]);

            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    float px = startX + c * _cellSize;
                    float py = startY - r * _cellSize;
                    _cellPositions[r, c] = new Vector3(px, py, 0);

                    var cellGO = new GameObject($"Cell_{r}_{c}");
                    cellGO.transform.position = _cellPositions[r, c];
                    cellGO.transform.localScale = Vector3.one * _cellSize * 0.9f;

                    var sr = cellGO.AddComponent<SpriteRenderer>();
                    sr.sprite = _emptyCellSprite;
                    sr.sortingOrder = 1;

                    _cellObjects[r, c] = cellGO;
                    _cellRenderers[r, c] = sr;
                    _grid[r, c] = null;
                }
            }
        }

        void Update()
        {
            if (!_isActive) return;

            // Combo timer
            if (_comboCount > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    _comboCount = 0;
                    _comboMultiplier = 1f;
                    _ui.UpdateCombo(0, 1f);
                }
            }

            // Research progress
            if (_isResearching && _activeResearchIndex >= 0)
            {
                float labBonus = GetBuildingCount(BuildingType.Lab) * 0.2f + 1f;
                _researchProgress += Time.deltaTime * _speedMultiplier * labBonus;
                _ui.UpdateResearchProgress(_researchProgress / 30f);
                if (_researchProgress >= 30f)
                {
                    CompleteResearch(_activeResearchIndex);
                }
            }
        }

        IEnumerator AutoCollectLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(1f);
                if (!_isActive) yield break;

                if (_energyShortage) continue;

                float oreGain = 0f;
                float energyGain = 0f;
                float partsGain = 0f;
                float aiMult = _robots[RobotType.AI].count > 0 && _aiEnabled ? 3f : 1f;

                // Worker collects all types
                int workerCount = GetActiveRobotCount(RobotType.Worker);
                oreGain    += workerCount * 0.5f * _speedMultiplier * aiMult;
                energyGain += workerCount * 0.3f * _speedMultiplier * aiMult;
                partsGain  += workerCount * 0.2f * _speedMultiplier * aiMult;

                // Miner specializes in ore
                if (_autoMiningEnabled)
                {
                    int minerCount = GetActiveRobotCount(RobotType.Miner);
                    float drillBonus = GetBuildingCount(BuildingType.MiningDrill) * 0.3f + 1f;
                    oreGain += minerCount * 1.5f * _speedMultiplier * aiMult * drillBonus;
                }

                // PowerBot generates energy
                if (_robots[RobotType.Power].count > 0)
                {
                    int powerCount = GetActiveRobotCount(RobotType.Power);
                    float plantBonus = GetBuildingCount(BuildingType.PowerPlant) * 0.4f + 1f;
                    energyGain += powerCount * 2f * _speedMultiplier * aiMult * plantBonus;
                }

                // Factory bonus for parts
                float factBonus = GetBuildingCount(BuildingType.Factory) * 0.3f + 1f;
                partsGain += factBonus * 0.5f * _speedMultiplier;

                Ore    = Mathf.Min(Ore + oreGain, 9999f);
                Parts  = Mathf.Min(Parts + partsGain, 9999f);

                if (_energyManagementEnabled)
                    Energy = Mathf.Min(Energy + energyGain, _energyCapacity);
                else
                    Energy = Mathf.Min(Energy + energyGain, 9999f);

                _ui.UpdateResources(Ore, Energy, Parts);
            }
        }

        IEnumerator BreakdownLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(20f);
                if (!_isActive) yield break;

                // 15% chance of a breakdown if robots > 0
                int totalBots = GetTotalActiveRobots();
                if (totalBots > 0 && Random.value < 0.15f)
                {
                    var types = new List<RobotType>();
                    foreach (var kv in _robots)
                        if (kv.Value.count > 0 && !kv.Value.isBroken && kv.Key != RobotType.AI && kv.Key != RobotType.Worker)
                            types.Add(kv.Key);

                    if (types.Count > 0)
                    {
                        var broken = types[Random.Range(0, types.Count)];
                        _robots[broken].isBroken = true;
                        _brokenRobots.Add(broken);
                        _ui.ShowBrokenWarning(broken);
                    }
                }
            }
        }

        IEnumerator EnergyDrainLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(2f);
                if (!_isActive) yield break;

                // Skip drain when already in shortage to prevent unrecoverable deadlock
                int totalBots = _energyShortage ? 0 : GetTotalActiveRobots();
                float drain = totalBots * 0.5f;
                Energy = Mathf.Max(0f, Energy - drain);

                bool wasShortage = _energyShortage;
                _energyShortage = Energy <= 0f;
                if (_energyShortage != wasShortage)
                    _ui.ShowEnergyShortage(_energyShortage);

                _ui.UpdateResources(Ore, Energy, Parts);
            }
        }

        IEnumerator AutoBuilderLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(8f);
                if (!_isActive) yield break;

                int builderCount = GetActiveRobotCount(RobotType.Builder);
                if (builderCount <= 0) continue;

                // Auto-build cheapest available building
                TryAutoBuild();
            }
        }

        void TryAutoBuild()
        {
            // Find an empty cell
            int emptyR = -1, emptyC = -1;
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    if (_grid[r, c] == null) { emptyR = r; emptyC = c; break; }

            if (emptyR < 0) return;

            // Try to build House (cheapest)
            if (Ore >= 20 && Parts >= 10)
            {
                Ore   -= 20;
                Parts -= 10;
                PlaceBuilding(BuildingType.House, emptyR, emptyC);
            }
        }

        // Public API called by UI buttons
        public bool TryBuyRobot(RobotType type)
        {
            if (!_robotUnlocked[(int)type]) return false;
            var robot = _robots[type];
            if (Ore < robot.oreCost || Energy < robot.energyCost || Parts < robot.partsCost) return false;

            Ore    -= robot.oreCost;
            Energy -= robot.energyCost;
            Parts  -= robot.partsCost;
            robot.count++;

            _ui.UpdateResources(Ore, Energy, Parts);
            _ui.UpdateRobotCounts(_robots);
            return true;
        }

        public bool TryBuildBuilding(BuildingType type)
        {
            // Find empty cell
            int emptyR = -1, emptyC = -1;
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    if (_grid[r, c] == null) { emptyR = r; emptyC = c; break; }

            if (emptyR < 0) return false;

            var (oc, ec, pc, _) = GetBuildingCost(type);
            if (Ore < oc || Energy < ec || Parts < pc) return false;

            Ore    -= oc;
            Energy -= ec;
            Parts  -= pc;

            PlaceBuilding(type, emptyR, emptyC);
            return true;
        }

        void PlaceBuilding(BuildingType type, int r, int c)
        {
            var (_, _, _, lvVal) = GetBuildingCost(type);

            var sr = _cellRenderers[r, c];
            sr.sprite = GetBuildingSprite(type);

            var data = new BuildingData
            {
                type = type,
                isBuilt = true,
                cityLevelValue = lvVal,
                spriteRenderer = sr,
                gameObject = _cellObjects[r, c]
            };
            _grid[r, c] = data;

            CityLevel += lvVal;

            // Combo
            _comboCount++;
            _comboMultiplier = _comboCount switch
            {
                >= 5 => 3f,
                >= 3 => 2f,
                >= 2 => 1.5f,
                _ => 1f,
            };
            _comboTimer = ComboResetTime;
            _ui.UpdateCombo(_comboCount, _comboMultiplier);

            // Score
            int scoreGain = Mathf.RoundToInt(lvVal * 10 * _comboMultiplier);
            if (_robots[RobotType.AI].count > 0 && _aiEnabled) scoreGain *= 2;
            TotalScore += scoreGain;
            _gameManager.AddScore(scoreGain);

            // Visual: scale pulse
            StartCoroutine(ScalePulse(_cellObjects[r, c].transform));

            // Flash yellow
            StartCoroutine(ColorFlash(sr, Color.yellow));

            _ui.UpdateCityLevel(CityLevel, _targetCityLevel);
            _ui.UpdateResources(Ore, Energy, Parts);

            // Check clear
            if (CityLevel >= _targetCityLevel && !_stageClearPending)
            {
                _stageClearPending = true;
                _gameManager.OnStageClear();
            }
        }

        public void RepairRobot(RobotType type)
        {
            if (!_robots[type].isBroken) return;
            if (Parts < 10) return;
            Parts -= 10;
            _robots[type].isBroken = false;
            _brokenRobots.Remove(type);
            _ui.HideBrokenWarning(type);
            _ui.UpdateResources(Ore, Energy, Parts);
        }

        public void StartResearch(int researchIndex)
        {
            if (_isResearching || _researchDone[researchIndex]) return;
            if (Parts < 20) return;
            Parts -= 20;
            _isResearching = true;
            _activeResearchIndex = researchIndex;
            _researchProgress = 0f;
            _ui.UpdateResources(Ore, Energy, Parts);
        }

        void CompleteResearch(int index)
        {
            _researchDone[index] = true;
            _isResearching = false;
            _activeResearchIndex = -1;
            _researchProgress = 0f;

            switch (index)
            {
                case 0: _speedMultiplier *= 1.3f; break;   // efficiency
                case 1: /* unlock higher tier already done via stage */ break;
                case 2: /* new building slot (extend grid conceptually) */ break;
            }
            _ui.MarkResearchDone(index);
        }

        // Helpers
        int GetActiveRobotCount(RobotType type)
        {
            var r = _robots[type];
            if (_energyShortage && type != RobotType.Power) return 0;
            return r.isBroken ? 0 : r.count;
        }

        int GetTotalActiveRobots()
        {
            int total = 0;
            foreach (var kv in _robots)
                if (!kv.Value.isBroken) total += kv.Value.count;
            return total;
        }

        int GetBuildingCount(BuildingType type)
        {
            int count = 0;
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    if (_grid[r, c] != null && _grid[r, c].type == type)
                        count++;
            return count;
        }

        (int ore, int energy, int parts, int lvVal) GetBuildingCost(BuildingType type) => type switch
        {
            BuildingType.House        => (20,  10,  10, 1),
            BuildingType.Factory      => (40,  20,  30, 2),
            BuildingType.PowerPlant   => (50,  10,  40, 2),
            BuildingType.Lab          => (60,  40,  50, 3),
            BuildingType.MiningDrill  => (35,  25,  20, 2),
            BuildingType.AICore       => (150, 100, 120, 5),
            _ => (0, 0, 0, 0)
        };

        Sprite GetBuildingSprite(BuildingType type) => type switch
        {
            BuildingType.House       => _buildingHouseSprite,
            BuildingType.Factory     => _buildingFactorySprite,
            BuildingType.PowerPlant  => _buildingPowerPlantSprite,
            BuildingType.Lab         => _buildingLabSprite,
            BuildingType.MiningDrill => _buildingMiningDrillSprite,
            BuildingType.AICore      => _buildingAICoreSprite,
            _ => null
        };

        IEnumerator ScalePulse(Transform t)
        {
            if (t == null) yield break;
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.1f;
                float scale = ratio <= 1f
                    ? Mathf.Lerp(1f, 1.3f, ratio)
                    : Mathf.Lerp(1.3f, 1f, ratio - 1f);
                if (t != null) t.localScale = orig * scale;
                yield return null;
            }
            if (t != null) t.localScale = orig;
        }

        IEnumerator ColorFlash(SpriteRenderer sr, Color flashColor)
        {
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = flashColor;
            yield return new WaitForSeconds(0.15f);
            if (sr != null) sr.color = orig;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }
    }
}
