using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game066v2_RoboFactory
{
    public class RoboFactoryUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text _stageText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _cityLevelText;
        [SerializeField] TMP_Text _oreText;
        [SerializeField] TMP_Text _energyText;
        [SerializeField] TMP_Text _partsText;
        [SerializeField] TMP_Text _comboText;
        [SerializeField] TMP_Text _collectRateText;

        [Header("Robot Buttons")]
        [SerializeField] Button _buyWorkerBtn;
        [SerializeField] Button _buyMinerBtn;
        [SerializeField] Button _buyBuilderBtn;
        [SerializeField] Button _buyRepairBtn;
        [SerializeField] Button _buyPowerBtn;
        [SerializeField] Button _buyAIBtn;

        [Header("Building Buttons")]
        [SerializeField] Button _buildHouseBtn;
        [SerializeField] Button _buildFactoryBtn;
        [SerializeField] Button _buildPowerPlantBtn;
        [SerializeField] Button _buildLabBtn;
        [SerializeField] Button _buildMiningDrillBtn;
        [SerializeField] Button _buildAICoreBtn;

        [Header("Research")]
        [SerializeField] GameObject _researchPanel;
        [SerializeField] Button _researchEfficiencyBtn;
        [SerializeField] Button _researchRobotBtn;
        [SerializeField] Button _researchBuildingBtn;
        [SerializeField] Slider _researchProgressSlider;

        [Header("Broken Warning")]
        [SerializeField] GameObject _brokenPanel;
        [SerializeField] TMP_Text _brokenText;
        [SerializeField] Button _repairBtn;

        [Header("Energy Warning")]
        [SerializeField] GameObject _energyWarningPanel;

        [Header("Stage Clear")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TMP_Text _stageClearText;
        [SerializeField] Button _nextStageBtn;

        [Header("All Clear")]
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TMP_Text _finalScoreText;

        [Header("Menu")]
        [SerializeField] Button _menuBtn;

        [SerializeField] FactoryManager _factoryManager;
        [SerializeField] RoboFactoryGameManager _gameManager;

        RobotType _currentBrokenType;
        int _score;

        void Start()
        {
            _buyWorkerBtn?.onClick.AddListener(() => _factoryManager.TryBuyRobot(RobotType.Worker));
            _buyMinerBtn?.onClick.AddListener(() => _factoryManager.TryBuyRobot(RobotType.Miner));
            _buyBuilderBtn?.onClick.AddListener(() => _factoryManager.TryBuyRobot(RobotType.Builder));
            _buyRepairBtn?.onClick.AddListener(() => _factoryManager.TryBuyRobot(RobotType.Repair));
            _buyPowerBtn?.onClick.AddListener(() => _factoryManager.TryBuyRobot(RobotType.Power));
            _buyAIBtn?.onClick.AddListener(() => _factoryManager.TryBuyRobot(RobotType.AI));

            _buildHouseBtn?.onClick.AddListener(() => _factoryManager.TryBuildBuilding(BuildingType.House));
            _buildFactoryBtn?.onClick.AddListener(() => _factoryManager.TryBuildBuilding(BuildingType.Factory));
            _buildPowerPlantBtn?.onClick.AddListener(() => _factoryManager.TryBuildBuilding(BuildingType.PowerPlant));
            _buildLabBtn?.onClick.AddListener(() => _factoryManager.TryBuildBuilding(BuildingType.Lab));
            _buildMiningDrillBtn?.onClick.AddListener(() => _factoryManager.TryBuildBuilding(BuildingType.MiningDrill));
            _buildAICoreBtn?.onClick.AddListener(() => _factoryManager.TryBuildBuilding(BuildingType.AICore));

            _researchEfficiencyBtn?.onClick.AddListener(() => _factoryManager.StartResearch(0));
            _researchRobotBtn?.onClick.AddListener(() => _factoryManager.StartResearch(1));
            _researchBuildingBtn?.onClick.AddListener(() => _factoryManager.StartResearch(2));

            _repairBtn?.onClick.AddListener(() => _factoryManager.RepairRobot(_currentBrokenType));
            _nextStageBtn?.onClick.AddListener(() => { _stageClearPanel?.SetActive(false); _gameManager.NextStage(); });
            _menuBtn?.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));

            _stageClearPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            _brokenPanel?.SetActive(false);
            _energyWarningPanel?.SetActive(false);
            if (_comboText) _comboText.gameObject.SetActive(false);
            if (_researchPanel) _researchPanel.SetActive(false);
        }

        public void SetupForStage(int stageIndex, bool[] unlocked, bool showTech, bool showEnergy, bool showAI)
        {
            _stageClearPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            _brokenPanel?.SetActive(false);
            _energyWarningPanel?.SetActive(false);

            _buyMinerBtn?.gameObject.SetActive(unlocked[(int)RobotType.Miner]);
            _buyBuilderBtn?.gameObject.SetActive(unlocked[(int)RobotType.Builder]);
            _buyRepairBtn?.gameObject.SetActive(unlocked[(int)RobotType.Repair]);
            _buyPowerBtn?.gameObject.SetActive(unlocked[(int)RobotType.Power]);
            _buyAIBtn?.gameObject.SetActive(unlocked[(int)RobotType.AI]);

            _buildAICoreBtn?.gameObject.SetActive(showAI);
            _researchPanel?.SetActive(showTech);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            _score = score;
            if (_scoreText) _scoreText.text = $"Score: {score:N0}";
        }

        public void UpdateCityLevel(int level, int target)
        {
            if (_cityLevelText) _cityLevelText.text = $"City Lv.{level} / {target}";
        }

        public void UpdateResources(float ore, float energy, float parts)
        {
            if (_oreText)    _oreText.text    = $"Ore: {ore:F0}";
            if (_energyText) _energyText.text = $"Energy: {energy:F0}";
            if (_partsText)  _partsText.text  = $"Parts: {parts:F0}";
        }

        public void UpdateRobotCounts(Dictionary<RobotType, RobotData> robots)
        {
            // Could add count labels per button, simplified here
        }

        public void UpdateCombo(int count, float multiplier)
        {
            if (_comboText == null) return;
            if (count <= 1)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"COMBO x{count}  ({multiplier:F1}x)";
            StartCoroutine(ComboPopAnim(_comboText.transform));
        }

        IEnumerator ComboPopAnim(Transform t)
        {
            Vector3 orig = t.localScale;
            t.localScale = orig * 1.3f;
            yield return new WaitForSeconds(0.15f);
            t.localScale = orig;
        }

        public void UpdateResearchProgress(float normalized)
        {
            if (_researchProgressSlider)
                _researchProgressSlider.value = normalized;
        }

        public void MarkResearchDone(int index)
        {
            var btn = index switch { 0 => _researchEfficiencyBtn, 1 => _researchRobotBtn, _ => _researchBuildingBtn };
            if (btn) btn.interactable = false;
            if (_researchProgressSlider) _researchProgressSlider.value = 0f;
        }

        public void ShowBrokenWarning(RobotType type)
        {
            _currentBrokenType = type;
            if (_brokenPanel) _brokenPanel.SetActive(true);
            if (_brokenText)  _brokenText.text = $"{type} が故障！\nパーツ10で修理";
            StartCoroutine(ShakePanel(_brokenPanel?.GetComponent<RectTransform>()));
        }

        public void HideBrokenWarning(RobotType type)
        {
            if (_currentBrokenType == type)
                _brokenPanel?.SetActive(false);
        }

        IEnumerator ShakePanel(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector3 orig = rt.anchoredPosition3D;
            for (int i = 0; i < 6; i++)
            {
                rt.anchoredPosition3D = orig + new Vector3(Random.Range(-8f, 8f), Random.Range(-4f, 4f), 0);
                yield return new WaitForSeconds(0.05f);
            }
            rt.anchoredPosition3D = orig;
        }

        public void ShowEnergyShortage(bool shortage)
        {
            _energyWarningPanel?.SetActive(shortage);
        }

        public void ShowStageClear(int nextStage)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearText)  _stageClearText.text = $"Stage {nextStage - 1} クリア！";
        }

        public void ShowAllClear(int finalScore)
        {
            if (_allClearPanel) _allClearPanel.SetActive(true);
            if (_finalScoreText) _finalScoreText.text = $"最終スコア: {finalScore:N0}";
        }

        public void ShowFloatingText(string msg, Vector3 worldPos)
        {
            StartCoroutine(FloatText(msg, worldPos));
        }

        IEnumerator FloatText(string msg, Vector3 worldPos)
        {
            var go = new GameObject("FloatText");
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = msg;
            tmp.fontSize = 3f;
            tmp.color = Color.yellow;
            tmp.alignment = TextAlignmentOptions.Center;
            go.transform.position = worldPos;

            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                go.transform.position = worldPos + Vector3.up * elapsed * 1.5f;
                var c = tmp.color;
                c.a = 1f - elapsed;
                tmp.color = c;
                yield return null;
            }
            Destroy(go);
        }
    }
}
