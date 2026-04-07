using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game086v2_CityBonsai
{
    public class CityBonsaiUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _populationText;
        [SerializeField] TextMeshProUGUI _beautyText;
        [SerializeField] TextMeshProUGUI _satisfactionText;
        [SerializeField] TextMeshProUGUI _turnText;
        [SerializeField] TextMeshProUGUI _seasonText;
        [SerializeField] TextMeshProUGUI _demandText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _messageText;

        [SerializeField] Button[] _buildingButtons;
        [SerializeField] TextMeshProUGUI[] _buildingButtonTexts;
        [SerializeField] Image[] _buildingButtonImages;
        [SerializeField] Button _pruneButton;
        [SerializeField] Image _pruneButtonImage;
        [SerializeField] Button _turnButton;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;

        Color _pruneNormalColor = new Color(0.6f, 0.3f, 0.15f);
        Color _pruneActiveColor = new Color(0.9f, 0.2f, 0.1f);

        Coroutine _hideCoroutine;

        void Awake()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_messageText != null) _messageText.gameObject.SetActive(false);
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo >= 2)
                _comboText.text = $"剪定コンボ x{combo}  ×{multiplier:F1}";
            else
                _comboText.text = "";
        }

        public void UpdatePopulation(int pop, int target)
        {
            if (_populationText != null)
            {
                _populationText.text = $"人口: {pop}/{target}";
                _populationText.color = pop >= target ? new Color(0.3f, 1f, 0.5f) : Color.white;
            }
        }

        public void UpdateBeauty(float beauty, float target)
        {
            if (_beautyText != null)
            {
                _beautyText.text = $"美しさ: {beauty:F0}/{target:F0}";
                _beautyText.color = beauty >= target ? new Color(0.3f, 1f, 0.5f) : Color.white;
            }
        }

        public void UpdateSatisfaction(float sat)
        {
            if (_satisfactionText != null)
            {
                _satisfactionText.text = $"満足度: {sat:F0}%";
                _satisfactionText.color = sat < 20 ? new Color(1f, 0.3f, 0.2f) : Color.white;
            }
        }

        public void UpdateTurn(int turn)
        {
            if (_turnText != null) _turnText.text = $"ターン: {turn}";
        }

        public void UpdateSeason(string season)
        {
            if (_seasonText != null) _seasonText.text = $"季節: {season}";
        }

        public void UpdateDemand(string demandText)
        {
            if (_demandText != null)
            {
                _demandText.text = demandText;
                _demandText.gameObject.SetActive(!string.IsNullOrEmpty(demandText));
            }
        }

        public void SetBuildingButton(int index, bool unlocked, string label)
        {
            if (_buildingButtons == null || index >= _buildingButtons.Length) return;
            if (_buildingButtons[index] != null)
                _buildingButtons[index].interactable = unlocked;
            if (_buildingButtonTexts != null && index < _buildingButtonTexts.Length && _buildingButtonTexts[index] != null)
                _buildingButtonTexts[index].text = label;
            if (_buildingButtonImages != null && index < _buildingButtonImages.Length && _buildingButtonImages[index] != null)
                _buildingButtonImages[index].color = unlocked ? GetBuildingColor(index) : new Color(0.3f, 0.3f, 0.3f);
        }

        public void HighlightBuildingButton(int selectedIndex)
        {
            if (_buildingButtonImages == null) return;
            for (int i = 0; i < _buildingButtonImages.Length; i++)
            {
                if (_buildingButtonImages[i] == null) continue;
                if (i == selectedIndex)
                    _buildingButtonImages[i].color = GetBuildingColor(i) * 1.3f;
                else if (_buildingButtons[i] != null && _buildingButtons[i].interactable)
                    _buildingButtonImages[i].color = GetBuildingColor(i);
            }
        }

        static Color GetBuildingColor(int index)
        {
            switch (index)
            {
                case 0: return new Color(0.26f, 0.65f, 0.96f); // House blue
                case 1: return new Color(1f, 0.65f, 0.15f);    // Shop orange
                case 2: return new Color(0.4f, 0.73f, 0.42f);  // Public green
                case 3: return new Color(0.94f, 0.33f, 0.31f);  // Shrine red
                case 4: return new Color(0.16f, 0.71f, 0.96f); // Park cyan
                default: return Color.gray;
            }
        }

        public void SetPruneMode(bool active)
        {
            if (_pruneButtonImage != null)
                _pruneButtonImage.color = active ? _pruneActiveColor : _pruneNormalColor;
        }

        public void ShowMessage(string msg)
        {
            if (_messageText == null) return;
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            _messageText.text = msg;
            _messageText.gameObject.SetActive(true);
            _hideCoroutine = StartCoroutine(HideMessageAfter(1.5f));
        }

        IEnumerator HideMessageAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_messageText != null) _messageText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int stage)
        {
            if (_stageClearPanel == null) return;
            if (_stageClearScoreText != null)
                _stageClearScoreText.text = $"ステージ {stage} クリア！";
            _stageClearPanel.SetActive(true);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int totalScore)
        {
            if (_allClearPanel == null) return;
            if (_allClearScoreText != null)
                _allClearScoreText.text = $"Final Score: {totalScore}";
            _allClearPanel.SetActive(true);
        }

        public void ShowGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }
    }
}
