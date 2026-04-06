using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game064v2_AquaCity
{
    public class AquaCityUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _populationText;
        [SerializeField] TextMeshProUGUI _coinsText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _autoRateText;
        [SerializeField] TextMeshProUGUI _sharkWarningText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;

        [SerializeField] Button _buyHouseButton;
        [SerializeField] Button _buyPlazaButton;
        [SerializeField] Button _buyCoralButton;
        [SerializeField] Button _buyDecoButton;
        [SerializeField] Button _buyAquariumButton;
        [SerializeField] Button _buyDeepBaseButton;

        [SerializeField] Button _menuButton;

        [SerializeField] CityManager _cityManager;
        [SerializeField] AquaCityGameManager _gameManager;

        // Floating texts pool
        readonly List<GameObject> _floatingPool = new();
        Transform _canvasTransform;


        void Awake()
        {
            _canvasTransform = GetComponentInParent<Canvas>()?.transform;
        }

        void Start()
        {
            _buyHouseButton?.onClick.AddListener(() => _cityManager.TryBuyBuilding(CityManager.BuildingType.House));
            _buyPlazaButton?.onClick.AddListener(() => _cityManager.TryBuyBuilding(CityManager.BuildingType.Plaza));
            _buyCoralButton?.onClick.AddListener(() => _cityManager.TryBuyBuilding(CityManager.BuildingType.Coral));
            _buyDecoButton?.onClick.AddListener(() => _cityManager.TryBuyBuilding(CityManager.BuildingType.Deco));
            _buyAquariumButton?.onClick.AddListener(() => _cityManager.TryBuyBuilding(CityManager.BuildingType.Aquarium));
            _buyDeepBaseButton?.onClick.AddListener(() => _cityManager.TryBuyBuilding(CityManager.BuildingType.DeepBase));

            _nextStageButton?.onClick.AddListener(() =>
            {
                _stageClearPanel?.SetActive(false);
                _gameManager.NextStage();
            });

            _menuButton?.onClick.AddListener(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu");
            });

            _stageClearPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            _sharkWarningText?.gameObject.SetActive(false);
            _comboText?.gameObject.SetActive(false);

            // Hide unlocked buildings initially
            _buyDecoButton?.gameObject.SetActive(false);
            _buyAquariumButton?.gameObject.SetActive(false);
            _buyDeepBaseButton?.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdatePopulation(long pop, long target)
        {
            if (_populationText != null) _populationText.text = $"人口: {pop:N0} / {target:N0}";
        }

        public void UpdateCoins(long coins)
        {
            if (_coinsText != null) _coinsText.text = $"コイン: {coins}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 1)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"x{combo} コンボ!";
        }

        public void UpdateAutoRate(long income, float speed)
        {
            if (_autoRateText != null)
                _autoRateText.text = $"自動: {(long)(income * speed)}/秒";
        }

        public void ShowSharkWarning(bool show)
        {
            _sharkWarningText?.gameObject.SetActive(show);
        }

        public void ShowDecoUnlocked()
        {
            _buyDecoButton?.gameObject.SetActive(true);
            ShowFloatingText("デコ解放!", new Vector3(0, 0, 0));
        }

        public void ShowAdjacencyUnlocked()
        {
            _buyAquariumButton?.gameObject.SetActive(true);
            ShowFloatingText("隣接ボーナス解放!", new Vector3(0, 0, 0));
        }

        public void ShowDeepSeaUnlocked()
        {
            _buyDeepBaseButton?.gameObject.SetActive(true);
            ShowFloatingText("深海エリア解放!", new Vector3(0, 0, 0));
        }

        public void UpdateShopAvailability(int stage, long coins)
        {
            SetBtnInteractable(_buyHouseButton, coins >= CityManager.BuildingCost[0]);
            SetBtnInteractable(_buyPlazaButton, coins >= CityManager.BuildingCost[1]);
            SetBtnInteractable(_buyCoralButton, coins >= CityManager.BuildingCost[2]);
            if (stage >= 2) SetBtnInteractable(_buyDecoButton, coins >= CityManager.BuildingCost[3]);
            if (stage >= 3) SetBtnInteractable(_buyAquariumButton, coins >= CityManager.BuildingCost[4]);
            if (stage >= 5) SetBtnInteractable(_buyDeepBaseButton, coins >= CityManager.BuildingCost[5]);
        }

        void SetBtnInteractable(Button btn, bool val)
        {
            if (btn != null) btn.interactable = val;
        }

        public void ShowStageClear(int stage, int total)
        {
            _stageClearPanel?.SetActive(true);
            if (_stageClearText != null)
                _stageClearText.text = $"ステージ {stage} クリア!";
        }

        public void ShowAllClear()
        {
            _stageClearPanel?.SetActive(false);
            _allClearPanel?.SetActive(true);
        }

        public void ShowFloatingText(string text, Vector3 worldPos)
        {
            StartCoroutine(FloatText(text, worldPos));
        }

        IEnumerator FloatText(string text, Vector3 worldPos)
        {
            var go = new GameObject("FloatingText");
            if (_canvasTransform != null) go.transform.SetParent(_canvasTransform, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.95f, 0.2f, 1f);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 60);

            // Convert world to screen to canvas
            if (Camera.main != null)
            {
                var screenPos = Camera.main.WorldToScreenPoint(worldPos);
                rt.position = screenPos + new Vector3(0, 50, 0);
            }

            float dur = 0.8f;
            float t = 0f;
            var startPos = rt.position;
            while (t < dur)
            {
                t += Time.deltaTime;
                float ratio = t / dur;
                rt.position = startPos + new Vector3(0, 80f * ratio, 0);
                tmp.color = new Color(1f, 0.95f, 0.2f, 1f - ratio);
                yield return null;
            }
            Destroy(go);
        }
    }
}
