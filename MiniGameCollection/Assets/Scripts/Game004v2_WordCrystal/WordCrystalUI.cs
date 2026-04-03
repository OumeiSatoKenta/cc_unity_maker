using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game004v2_WordCrystal
{
    public class WordCrystalUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _targetScoreText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private TextMeshProUGUI _themeLabel;

        [Header("Word Slots")]
        [SerializeField] private GameObject _slotContainer;
        [SerializeField] private GameObject[] _slotObjects;
        [SerializeField] private TextMeshProUGUI[] _slotTexts;
        [SerializeField] private Image[] _slotImages;

        [Header("Score Popup")]
        [SerializeField] private TextMeshProUGUI _scorePopupText;

        [Header("Panels")]
        [SerializeField] private GameObject _stageClearPanel;
        [SerializeField] private TextMeshProUGUI _stageClearStageText;
        [SerializeField] private TextMeshProUGUI _stageClearScoreText;
        [SerializeField] private TextMeshProUGUI _stageClearStarsText;
        [SerializeField] private GameObject _gameClearPanel;
        [SerializeField] private TextMeshProUGUI _gameClearScoreText;
        [SerializeField] private GameObject _gameOverPanel;

        [SerializeField] private WordManager _wordManager;
        private WordCrystalGameManager _gameManager;
        private Image _timerImage;
        private Coroutine _flashCoroutine;
        private Coroutine _popupCoroutine;
        private Coroutine _timerFlashCoroutine;

        private void Awake()
        {
            _gameManager = GetComponentInParent<WordCrystalGameManager>();
            if (_timerText != null)
                _timerImage = _timerText.GetComponent<Image>();
        }

        private void Start()
        {
            if (_wordManager != null)
                _wordManager.OnSlotChanged += OnSlotChanged;
            HideAllPanels();
            if (_scorePopupText != null) _scorePopupText.gameObject.SetActive(false);
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString("N0");
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateTargetScore(int target)
        {
            if (_targetScoreText != null) _targetScoreText.text = $"目標: {target:N0}";
        }

        public void UpdateTimer(float seconds)
        {
            if (_timerText != null) _timerText.text = Mathf.CeilToInt(seconds).ToString();
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo >= 2)
            {
                _comboText.gameObject.SetActive(true);
                _comboText.text = $"COMBO x{combo}";
                _comboText.color = combo >= 4 ? new Color(1f, 0.3f, 0.3f) :
                                   combo == 3 ? new Color(1f, 0.7f, 0f) : new Color(0.3f, 1f, 0.3f);
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void SetThemeLabel(string theme)
        {
            if (_themeLabel == null) return;
            if (string.IsNullOrEmpty(theme))
            {
                _themeLabel.gameObject.SetActive(false);
            }
            else
            {
                _themeLabel.gameObject.SetActive(true);
                _themeLabel.text = $"テーマ: {theme}";
            }
        }

        private void OnSlotChanged(char[] slots)
        {
            if (_slotTexts == null) return;
            for (int i = 0; i < _slotTexts.Length; i++)
            {
                if (_slotTexts[i] == null) continue;
                if (i < slots.Length)
                {
                    _slotTexts[i].text = slots[i].ToString().ToUpper();
                    if (_slotObjects != null && i < _slotObjects.Length && _slotObjects[i] != null)
                        _slotObjects[i].SetActive(true);
                }
                else
                {
                    _slotTexts[i].text = "";
                    if (_slotObjects != null && i < _slotObjects.Length && _slotObjects[i] != null)
                        _slotObjects[i].SetActive(false);
                }
            }
        }

        public void FlashSlots(bool correct)
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashSlotsCoroutine(correct));
        }

        private IEnumerator FlashSlotsCoroutine(bool correct)
        {
            Color flashColor = correct ? new Color(0.3f, 1f, 0.3f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);
            Color normalColor = Color.white;
            if (_slotImages == null) yield break;
            foreach (var img in _slotImages)
                if (img != null) img.color = flashColor;
            yield return new WaitForSeconds(0.15f);
            foreach (var img in _slotImages)
                if (img != null) img.color = normalColor;
        }

        public void FlashTimer()
        {
            if (_timerFlashCoroutine != null) StopCoroutine(_timerFlashCoroutine);
            _timerFlashCoroutine = StartCoroutine(FlashTimerCoroutine());
        }

        private IEnumerator FlashTimerCoroutine()
        {
            if (_timerText == null) yield break;
            Color orig = _timerText.color;
            _timerText.color = new Color(1f, 0.2f, 0.2f);
            yield return new WaitForSeconds(0.3f);
            _timerText.color = orig;
        }

        public void ShowScorePopup(int score, float multiplier)
        {
            if (_scorePopupText == null) return;
            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(ScorePopupCoroutine(score, multiplier));
        }

        private IEnumerator ScorePopupCoroutine(int score, float multiplier)
        {
            _scorePopupText.gameObject.SetActive(true);
            string text = multiplier > 1f ? $"+{score}\nx{multiplier:0.0}" : $"+{score}";
            _scorePopupText.text = text;
            _scorePopupText.color = multiplier >= 3f ? new Color(1f, 0.3f, 0.3f) :
                                    multiplier >= 2f ? new Color(1f, 0.7f, 0f) : Color.white;

            float t = 0f;
            Vector3 startPos = _scorePopupText.transform.localPosition;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.8f;
                _scorePopupText.transform.localPosition = startPos + Vector3.up * (50f * ratio);
                _scorePopupText.color = new Color(_scorePopupText.color.r, _scorePopupText.color.g, _scorePopupText.color.b, 1f - ratio);
                yield return null;
            }
            _scorePopupText.transform.localPosition = startPos;
            _scorePopupText.gameObject.SetActive(false);
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void ShowStageClearPanel(int stage, int score, int stars)
        {
            HideAllPanels();
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearStageText != null) _stageClearStageText.text = $"Stage {stage} クリア！";
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"スコア: {score:N0}";
            if (_stageClearStarsText != null) _stageClearStarsText.text = new string('★', stars) + new string('☆', 3 - stars);
        }

        public void ShowClearPanel(int score)
        {
            HideAllPanels();
            if (_gameClearPanel == null) return;
            _gameClearPanel.SetActive(true);
            if (_gameClearScoreText != null) _gameClearScoreText.text = $"最終スコア: {score:N0}";
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        private void OnDestroy()
        {
            if (_wordManager != null)
                _wordManager.OnSlotChanged -= OnSlotChanged;
        }
    }
}
