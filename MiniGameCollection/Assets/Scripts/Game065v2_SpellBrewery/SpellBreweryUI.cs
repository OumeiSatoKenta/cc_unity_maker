using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game065v2_SpellBrewery
{
    public class SpellBreweryUI : MonoBehaviour
    {
        [SerializeField] SpellBreweryGameManager _gameManager;
        [SerializeField] BreweryManager _breweryManager;

        // HUD
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _goldText;
        [SerializeField] TextMeshProUGUI _targetText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _newElementText;

        // Ingredient buttons (5)
        [SerializeField] Button[] _ingredientButtons;
        [SerializeField] TextMeshProUGUI[] _ingredientCountTexts;
        [SerializeField] TextMeshProUGUI[] _ingredientNameTexts;

        // Cauldron slots display
        [SerializeField] TextMeshProUGUI _cauldronSlotsText;
        [SerializeField] Button _brewButton;
        [SerializeField] Button _clearCauldronButton;

        // Potion inventory
        [SerializeField] GameObject[] _potionSlots;
        [SerializeField] TextMeshProUGUI[] _potionCountTexts;
        [SerializeField] Button[] _potionSellButtons;
        [SerializeField] Button _sellAllButton;

        // Brewing indicator
        [SerializeField] GameObject _brewingIndicator;

        // Order panel
        [SerializeField] GameObject _orderPanel;
        [SerializeField] TextMeshProUGUI _orderText;
        [SerializeField] TextMeshProUGUI _orderBonusText;
        [SerializeField] Image _orderTimerBar;

        // Stage clear / all clear panels
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;
        [SerializeField] GameObject _allClearPanel;

        // Menu button
        [SerializeField] Button _menuButton;

        // Floating text
        readonly List<GameObject> _floatingPool = new();
        Transform _canvasTransform;
        Coroutine _newElementCoroutine;

        static readonly string[] IngredientNames = { "🔥Fire", "💧Water", "🌿Earth", "💨Air", "✨Light" };
        static readonly string[] PotionNames = { "Fire", "Water", "Earth", "Air", "Light", "Storm", "Nature", "Legend" };

        void Awake()
        {
            _canvasTransform = GetComponentInParent<Canvas>()?.transform;
        }

        void Start()
        {
            // Ingredient buttons
            for (int i = 0; i < _ingredientButtons.Length && i < 5; i++)
            {
                int idx = i;
                _ingredientButtons[i]?.onClick.AddListener(() => _breweryManager.TapIngredient(idx));
                if (_ingredientNameTexts != null && i < _ingredientNameTexts.Length && _ingredientNameTexts[i] != null)
                    _ingredientNameTexts[i].text = IngredientNames[i];
            }

            _brewButton?.onClick.AddListener(() => _breweryManager.Brew());
            _clearCauldronButton?.onClick.AddListener(() => _breweryManager.ClearCauldron());
            _sellAllButton?.onClick.AddListener(() => _breweryManager.SellAll());

            // Potion sell buttons
            for (int i = 0; i < _potionSellButtons.Length && i < 8; i++)
            {
                int idx = i;
                _potionSellButtons[i]?.onClick.AddListener(() => _breweryManager.SellPotion(idx));
            }

            _nextStageButton?.onClick.AddListener(() =>
            {
                _stageClearPanel?.SetActive(false);
                _gameManager.NextStage();
            });

            _menuButton?.onClick.AddListener(() =>
                UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));

            _stageClearPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            _orderPanel?.SetActive(false);
            _brewingIndicator?.SetActive(false);
            _newElementText?.gameObject.SetActive(false);
            _comboText?.gameObject.SetActive(false);

            _brewButton?.gameObject.SetActive(false);

            // Hide Air/Light buttons initially (unlocked at stage 2)
            if (_ingredientButtons != null && _ingredientButtons.Length >= 5)
            {
                if (_ingredientButtons[3] != null) _ingredientButtons[3].gameObject.SetActive(false);
                if (_ingredientButtons[4] != null) _ingredientButtons[4].gameObject.SetActive(false);
            }
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateGold(long gold, long target)
        {
            if (_goldText != null) _goldText.text = $"💰 {gold:N0}G";
            if (_targetText != null) _targetText.text = $"目標: {target:N0}G";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 0)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            string colorHex = combo >= 8 ? "#FF4444" : combo >= 5 ? "#FF8800" : combo >= 3 ? "#FFCC00" : "#FFFFFF";
            _comboText.text = $"<color={colorHex}>🔥 COMBO x{combo}!</color>";
        }

        public void UpdateAllIngredients(int[] stocks, int stage)
        {
            if (_ingredientButtons == null) return;
            int unlocked = stage >= 2 ? 5 : 3;
            for (int i = 0; i < _ingredientButtons.Length && i < 5; i++)
            {
                bool active = i < unlocked;
                if (_ingredientButtons[i] != null) _ingredientButtons[i].gameObject.SetActive(active);
                if (active) UpdateIngredientStock(i, stocks[i]);
            }
        }

        public void UpdateIngredientStock(int idx, int stock)
        {
            if (_ingredientCountTexts != null && idx < _ingredientCountTexts.Length && _ingredientCountTexts[idx] != null)
                _ingredientCountTexts[idx].text = stock.ToString();
            if (_ingredientButtons != null && idx < _ingredientButtons.Length && _ingredientButtons[idx] != null)
                _ingredientButtons[idx].interactable = stock > 0;
        }

        public void UpdateCauldronSlots(List<BreweryManager.IngredientType> slots)
        {
            if (_cauldronSlotsText == null) return;
            if (slots.Count == 0) { _cauldronSlotsText.text = "（空）"; return; }
            var sb = new System.Text.StringBuilder();
            foreach (var s in slots)
            {
                sb.Append(IngredientNames[(int)s]);
                sb.Append(" ");
            }
            _cauldronSlotsText.text = sb.ToString().Trim();
        }

        public void UpdateBrewButton(bool interactable)
        {
            if (_brewButton == null) return;
            _brewButton.gameObject.SetActive(true);
            _brewButton.interactable = interactable;
        }

        public void UpdatePotionInventory(int[] stocks, long[] prices)
        {
            if (_potionSlots == null) return;
            for (int i = 0; i < _potionSlots.Length && i < 8; i++)
            {
                if (_potionSlots[i] == null) continue;
                bool hasStock = stocks[i] > 0;
                _potionSlots[i].SetActive(hasStock);
                if (_potionCountTexts != null && i < _potionCountTexts.Length && _potionCountTexts[i] != null)
                    _potionCountTexts[i].text = $"{PotionNames[i]}\n×{stocks[i]} ({prices[i]}G)";
                if (_potionSellButtons != null && i < _potionSellButtons.Length && _potionSellButtons[i] != null)
                    _potionSellButtons[i].interactable = hasStock;
            }
        }

        public void ShowBrewing(bool active)
        {
            _brewingIndicator?.SetActive(active);
        }

        public void ShowOrder(BreweryManager.PotionType order, string name, long bonus)
        {
            if (_orderPanel == null) return;
            _orderPanel.SetActive(true);
            if (_orderText != null) _orderText.text = $"📜 注文: {name}";
            if (_orderBonusText != null) _orderBonusText.text = $"報酬: {bonus}G (2倍！)";
        }

        public void HideOrder()
        {
            _orderPanel?.SetActive(false);
        }

        public void UpdateOrderTimer(float ratio)
        {
            if (_orderTimerBar != null) _orderTimerBar.fillAmount = Mathf.Clamp01(ratio);
        }

        public void ShowNewElement(string message)
        {
            if (_newElementText == null) return;
            if (_newElementCoroutine != null) StopCoroutine(_newElementCoroutine);
            _newElementCoroutine = StartCoroutine(ShowNewElementCoroutine(message));
        }

        IEnumerator ShowNewElementCoroutine(string msg)
        {
            _newElementText.gameObject.SetActive(true);
            _newElementText.text = msg;
            _newElementText.color = new Color(1f, 0.9f, 0.2f, 1f);
            yield return new WaitForSeconds(2f);
            float fade = 1f;
            while (fade > 0f)
            {
                fade -= Time.deltaTime;
                var c = _newElementText.color;
                _newElementText.color = new Color(c.r, c.g, c.b, fade);
                yield return null;
            }
            _newElementText.gameObject.SetActive(false);
        }

        public void PulseIngredientButton(int idx)
        {
            if (_ingredientButtons == null || idx >= _ingredientButtons.Length || _ingredientButtons[idx] == null) return;
            StartCoroutine(PulseButton(_ingredientButtons[idx].transform));
        }

        IEnumerator PulseButton(Transform t)
        {
            var orig = t.localScale;
            float dur = 0.05f;
            float e = 0f;
            while (e < dur) { e += Time.deltaTime; t.localScale = orig * Mathf.Lerp(1f, 1.2f, e / dur); yield return null; }
            e = 0f;
            while (e < dur) { e += Time.deltaTime; t.localScale = orig * Mathf.Lerp(1.2f, 1f, e / dur); yield return null; }
            t.localScale = orig;
        }

        public void ShowStageClear(int stage, int total)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearText != null) _stageClearText.text = $"Stage {stage} クリア！";
        }

        public void ShowAllClear()
        {
            _allClearPanel?.SetActive(true);
            _stageClearPanel?.SetActive(false);
        }

        public void ShowFloatingText(string text, Vector3 worldPos)
        {
            if (_canvasTransform == null) return;
            var go = new GameObject("FT");
            go.transform.SetParent(_canvasTransform, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 36;
            tmp.color = new Color(1f, 0.9f, 0.2f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 60);
            // Convert world to canvas
            var cam = Camera.main;
            if (cam != null)
            {
                var screenPos = cam.WorldToScreenPoint(worldPos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasTransform as RectTransform, screenPos, null, out var lp);
                rt.localPosition = lp;
            }
            StartCoroutine(FloatAndFade(go, tmp));
        }

        IEnumerator FloatAndFade(GameObject go, TextMeshProUGUI tmp)
        {
            float dur = 1.2f;
            float t = 0f;
            var rt = go.GetComponent<RectTransform>();
            var startPos = rt.localPosition;
            while (t < dur)
            {
                t += Time.deltaTime;
                float ratio = t / dur;
                rt.localPosition = startPos + Vector3.up * (80f * ratio);
                tmp.color = new Color(1f, 0.9f, 0.2f, 1f - ratio);
                yield return null;
            }
            Destroy(go);
        }
    }
}
