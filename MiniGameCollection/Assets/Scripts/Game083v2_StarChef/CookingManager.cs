using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game083v2_StarChef
{
    public class CookingManager : MonoBehaviour
    {
        // --- State ---
        public enum CookingState { Idle, SelectingIngredients, Heating, ShowResult }

        [Header("UI References")]
        [SerializeField] Slider _heatGauge;
        [SerializeField] Image _optimalZone;
        [SerializeField] Button _tapButton;
        [SerializeField] TextMeshProUGUI _resultText;
        [SerializeField] GameObject _ingredientPanel;
        [SerializeField] Button[] _ingredientButtons;
        [SerializeField] TextMeshProUGUI[] _ingredientNameTexts;
        [SerializeField] Image[] _ingredientIcons;
        [SerializeField] Button _cookButton;
        [SerializeField] TextMeshProUGUI _recipeHintText;
        [SerializeField] GameObject _heatingPanel;

        StarChefGameManager _gm;

        // --- Stage Parameters ---
        float _gaugeSpeed = 1.5f;
        float _optimalZoneWidth = 0.3f;
        float _optimalZoneStart = 0.35f;
        int _availableIngredientCount = 2;
        float _cookTimeLimit = 30f;

        // --- Game State ---
        CookingState _state = CookingState.Idle;
        bool _isActive;
        float _gaugeValue;
        bool _gaugeGoingRight = true;
        float _timeRemaining;

        // Score & Combo
        int _totalScore;
        int _combo;
        int _fails;
        int _stageDishCount;
        const int MaxFails = 3;
        const int DishesPerStage = 5;

        // Selected ingredients (indices into _ingredientNames)
        List<int> _selectedIngredients = new List<int>();
        bool[] _buttonSelected;

        // --- Recipe Data ---
        static readonly string[] _ingredientNames = {
            "星の粉", "月光ジュース", "銀河ハーブ", "宇宙塩", "ネビュラソース", "彗星クリーム"
        };

        // Each recipe: name, required ingredient indices (sorted)
        static readonly (string name, int[] ingredients)[] _recipes = {
            ("星のスープ",     new[]{0,1}),
            ("銀河サラダ",     new[]{2,3}),
            ("宇宙プリン",     new[]{1,3}),
            ("星雲ピザ",       new[]{0,2,4}),
            ("月面パスタ",     new[]{1,4,3}),
            ("彗星デザート",   new[]{3,5,0}),
            ("銀河カレー",     new[]{0,2,3,4}),
            ("星屑ラーメン",   new[]{1,2,0}),
            ("宇宙アイス",     new[]{5,1,0}),
            ("ネビュラ鍋",     new[]{4,2,3,0}),
        };

        // Recipes available per stage (cumulative)
        static readonly int[] _recipesPerStage = { 3, 6, 7, 9, 10 };

        int _currentStage;
        int _availableRecipes;

        public int TotalScore => _totalScore;

        void Awake()
        {
            _gm = GetComponentInParent<StarChefGameManager>();
            if (_gm == null) Debug.LogError("[CookingManager] StarChefGameManager not found in parent.");
            _buttonSelected = new bool[_ingredientButtons != null ? _ingredientButtons.Length : 0];
        }

        public void ResetGame()
        {
            _totalScore = 0;
            _combo = 0;
            _fails = 0;
            _stageDishCount = 0;
            _isActive = false;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active) StopAllCoroutines();
            if (_heatingPanel != null) _heatingPanel.SetActive(false);
            if (_ingredientPanel != null) _ingredientPanel.SetActive(active);
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _currentStage = stageIndex;
            _stageDishCount = 0;
            _availableRecipes = _recipesPerStage[Mathf.Clamp(stageIndex, 0, _recipesPerStage.Length - 1)];

            // Stage parameters
            _gaugeSpeed = 1.0f + config.speedMultiplier * 0.5f;
            _optimalZoneWidth = Mathf.Lerp(0.30f, 0.12f, config.complexityFactor);
            _cookTimeLimit = Mathf.Lerp(30f, 12f, config.complexityFactor);
            _availableIngredientCount = Mathf.Min(2 + config.countMultiplier, _ingredientNames.Length);

            _isActive = true;
            _state = CookingState.Idle;

            SetupIngredientButtons();
            ShowIngredientSelection();
        }

        void SetupIngredientButtons()
        {
            // Show only available ingredients
            for (int i = 0; i < _ingredientButtons.Length; i++)
            {
                bool visible = i < _availableIngredientCount;
                _ingredientButtons[i].gameObject.SetActive(visible);
                if (visible && _ingredientNameTexts[i] != null)
                    _ingredientNameTexts[i].text = _ingredientNames[i];
            }
        }

        void ShowIngredientSelection()
        {
            _state = CookingState.SelectingIngredients;
            _selectedIngredients.Clear();
            for (int i = 0; i < _buttonSelected.Length; i++) _buttonSelected[i] = false;
            RefreshIngredientButtonColors();

            if (_ingredientPanel != null) _ingredientPanel.SetActive(true);
            if (_heatingPanel != null) _heatingPanel.SetActive(false);

            // Show a random recipe hint
            if (_recipeHintText != null)
            {
                int idx = Random.Range(0, _availableRecipes);
                var r = _recipes[idx];
                string hint = "";
                foreach (int ingIdx in r.ingredients)
                    if (ingIdx < _availableIngredientCount)
                        hint += _ingredientNames[ingIdx] + " + ";
                hint = hint.TrimEnd('+', ' ');
                _recipeHintText.text = $"ヒント: {r.name}\n{hint}";
            }

            if (_cookButton != null) _cookButton.interactable = false;
        }

        void RefreshIngredientButtonColors()
        {
            for (int i = 0; i < _ingredientButtons.Length; i++)
            {
                if (!_ingredientButtons[i].gameObject.activeSelf) continue;
                var img = _ingredientButtons[i].GetComponent<Image>();
                if (img != null)
                    img.color = _buttonSelected[i] ? new Color(1f, 0.9f, 0.3f) : Color.white;
            }
        }

        public void OnIngredientButtonTapped(int index)
        {
            if (_state != CookingState.SelectingIngredients) return;
            if (index < 0 || index >= _availableIngredientCount || index >= _buttonSelected.Length) return;

            _buttonSelected[index] = !_buttonSelected[index];
            if (_buttonSelected[index])
                _selectedIngredients.Add(index);
            else
                _selectedIngredients.Remove(index);

            RefreshIngredientButtonColors();

            // Enable cook button if 2+ ingredients selected
            if (_cookButton != null)
                _cookButton.interactable = _selectedIngredients.Count >= 2;
        }

        public void OnCookButtonTapped()
        {
            if (_state != CookingState.SelectingIngredients) return;
            if (_selectedIngredients.Count < 2) return;
            StartHeating();
        }

        void StartHeating()
        {
            _state = CookingState.Heating;
            if (_ingredientPanel != null) _ingredientPanel.SetActive(false);
            if (_heatingPanel != null) _heatingPanel.SetActive(true);

            // Position optimal zone
            _optimalZoneStart = Random.Range(0.2f, 0.6f);
            if (_optimalZone != null)
            {
                var rect = _optimalZone.rectTransform;
                float fillWidth = _heatGauge != null ? _heatGauge.GetComponent<RectTransform>().rect.width : 400f;
                rect.anchorMin = new Vector2(_optimalZoneStart, 0f);
                rect.anchorMax = new Vector2(_optimalZoneStart + _optimalZoneWidth, 1f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            _gaugeValue = 0f;
            _gaugeGoingRight = true;
            _timeRemaining = _cookTimeLimit;

            if (_tapButton != null)
            {
                _tapButton.interactable = true;
                _tapButton.onClick.RemoveAllListeners();
                _tapButton.onClick.AddListener(OnTapButtonPressed);
            }
        }

        void Update()
        {
            if (!_isActive) return;
            if (_state != CookingState.Heating) return;

            // Animate gauge
            float delta = _gaugeSpeed * Time.deltaTime;
            if (_gaugeGoingRight) _gaugeValue += delta;
            else _gaugeValue -= delta;

            if (_gaugeValue >= 1f) { _gaugeValue = 1f; _gaugeGoingRight = false; }
            if (_gaugeValue <= 0f) { _gaugeValue = 0f; _gaugeGoingRight = true; }

            if (_heatGauge != null) _heatGauge.value = _gaugeValue;

            // Highlight optimal zone when gauge is in it
            bool inZone = _gaugeValue >= _optimalZoneStart && _gaugeValue <= _optimalZoneStart + _optimalZoneWidth;
            if (_optimalZone != null)
                _optimalZone.color = inZone ? new Color(1f, 1f, 0f, 0.8f) : new Color(0.3f, 1f, 0.5f, 0.4f);

            // Time limit
            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                // Timeout = auto tap at current position
                OnTapButtonPressed();
            }
        }

        void OnTapButtonPressed()
        {
            if (_state != CookingState.Heating) return;
            _state = CookingState.ShowResult;
            if (_tapButton != null) _tapButton.interactable = false;

            // Calculate score
            bool inOptimal = _gaugeValue >= _optimalZoneStart && _gaugeValue <= _optimalZoneStart + _optimalZoneWidth;
            float distFromCenter = Mathf.Abs(_gaugeValue - (_optimalZoneStart + _optimalZoneWidth * 0.5f));
            float halfZone = _optimalZoneWidth * 0.5f;
            float accuracy = inOptimal ? 1f - (distFromCenter / halfZone) * 0.4f : Mathf.Max(0f, 1f - distFromCenter * 2f);

            // Check if selected ingredients match a recipe
            bool recipeMatch = CheckRecipeMatch();
            int baseScore = recipeMatch
                ? Mathf.RoundToInt(accuracy * 100f)
                : Mathf.RoundToInt(accuracy * 30f);  // penalty for wrong recipe

            // Combo
            if (baseScore >= 60)
            {
                _combo++;
            }
            else
            {
                _combo = 0;
                if (!recipeMatch)
                {
                    _fails++;
                    if (_gm != null) _gm.UpdateFailDisplay(_fails);
                }
            }

            float comboMult = GetComboMultiplier();
            int finalScore = Mathf.RoundToInt(baseScore * comboMult);
            _totalScore += finalScore;

            if (_gm != null) _gm.UpdateScoreDisplay(_totalScore);
            if (_gm != null) _gm.UpdateComboDisplay(_combo);

            // Show result feedback
            string resultMsg;
            if (recipeMatch)
            {
                if (accuracy >= 0.8f) resultMsg = "★★★ 完璧！";
                else if (accuracy >= 0.5f) resultMsg = "★★ 良い！";
                else resultMsg = "★ OK";
            }
            else
            {
                resultMsg = "レシピが違う…";
            }

            if (_resultText != null) _resultText.text = $"{resultMsg}\n+{finalScore}pt";

            // Count successful dishes for stage clear
            if (baseScore >= 60 && recipeMatch) _stageDishCount++;

            // Visual feedback
            StartCoroutine(ShowResultFeedback(recipeMatch && accuracy >= 0.5f));

            // Check game over
            if (_fails >= MaxFails)
            {
                StartCoroutine(DelayedGameOver());
                return;
            }

            // Check stage clear (5 successful dishes per stage)
            if (_stageDishCount >= DishesPerStage)
            {
                StartCoroutine(DelayedStageClear());
            }
            else
            {
                StartCoroutine(DelayedNextRound());
            }
        }

        bool CheckRecipeMatch()
        {
            var selected = new List<int>(_selectedIngredients);
            selected.Sort();
            for (int i = 0; i < _availableRecipes; i++)
            {
                var req = new List<int>(_recipes[i].ingredients);
                req.Sort();
                if (req.Count != selected.Count) continue;
                bool match = true;
                for (int j = 0; j < req.Count; j++)
                    if (req[j] != selected[j]) { match = false; break; }
                if (match) return true;
            }
            return false;
        }

        float GetComboMultiplier()
        {
            if (_combo >= 8) return 2.0f;
            if (_combo >= 5) return 1.5f;
            if (_combo >= 3) return 1.2f;
            return 1.0f;
        }

        IEnumerator ShowResultFeedback(bool success)
        {
            // Scale pop on result text
            if (_resultText != null)
            {
                _resultText.gameObject.SetActive(true);
                _resultText.color = success ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f);
                float t = 0f;
                while (t < 0.2f)
                {
                    t += Time.deltaTime;
                    float s = Mathf.Lerp(1f, 1.4f, t / 0.2f);
                    _resultText.transform.localScale = Vector3.one * s;
                    yield return null;
                }
                t = 0f;
                while (t < 0.2f)
                {
                    t += Time.deltaTime;
                    float s = Mathf.Lerp(1.4f, 1f, t / 0.2f);
                    _resultText.transform.localScale = Vector3.one * s;
                    yield return null;
                }
            }

            // Flash heating panel background on failure
            if (!success && _heatingPanel != null)
            {
                var img = _heatingPanel.GetComponent<Image>();
                if (img != null)
                {
                    Color orig = img.color;
                    img.color = new Color(1f, 0.2f, 0.2f, orig.a);
                    yield return new WaitForSeconds(0.15f);
                    img.color = orig;
                    yield return new WaitForSeconds(0.15f);
                    img.color = new Color(1f, 0.2f, 0.2f, orig.a);
                    yield return new WaitForSeconds(0.15f);
                    img.color = orig;
                }
            }
        }

        IEnumerator DelayedNextRound()
        {
            yield return new WaitForSeconds(1.5f);
            if (_resultText != null) _resultText.gameObject.SetActive(false);
            if (_isActive) ShowIngredientSelection();
        }

        IEnumerator DelayedStageClear()
        {
            _totalScore += (_currentStage + 1) * 200;
            if (_gm != null) _gm.UpdateScoreDisplay(_totalScore);
            yield return new WaitForSeconds(1.5f);
            if (_resultText != null) _resultText.gameObject.SetActive(false);
            if (_heatingPanel != null) _heatingPanel.SetActive(false);
            _isActive = false;
            if (_gm != null) _gm.OnStageClear();
        }

        IEnumerator DelayedGameOver()
        {
            yield return new WaitForSeconds(1.5f);
            if (_resultText != null) _resultText.gameObject.SetActive(false);
            _isActive = false;
            if (_gm != null) _gm.OnGameOver();
        }
    }
}
