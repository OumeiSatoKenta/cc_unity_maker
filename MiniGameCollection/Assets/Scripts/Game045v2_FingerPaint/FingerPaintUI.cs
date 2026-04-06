using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game045v2_FingerPaint
{
    public class FingerPaintUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _matchRateText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _targetText;
        [SerializeField] Slider _inkSlider;
        [SerializeField] TextMeshProUGUI _comboText;

        [Header("Palette")]
        [SerializeField] Button[] _colorButtons;
        [SerializeField] Button _thinBrushButton;
        [SerializeField] Button _eraserButton;

        [Header("Panels")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearStarsText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverMatchText;

        [Header("References")]
        [SerializeField] FingerPaintGameManager _gameManager;
        [SerializeField] FingerPaintCanvas _canvas;

        private int _currentStage;
        private bool _isThinBrush;
        private bool _isEraserMode;
        private float _targetMatch;
        private Coroutine _comboRoutine;

        private void Start()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_finalClearPanel) _finalClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
            if (_comboText) _comboText.gameObject.SetActive(false);
        }

        public void SetupStage(int stageNumber, int totalStages, float targetMatch, float timeLimit)
        {
            _currentStage = stageNumber;
            _targetMatch = targetMatch;

            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_finalClearPanel) _finalClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);

            if (_stageText) _stageText.text = $"Stage {stageNumber} / {totalStages}";
            if (_targetText) _targetText.text = $"目標: {Mathf.RoundToInt(targetMatch * 100)}%";
            if (_timerText) _timerText.text = $"{Mathf.CeilToInt(timeLimit)}";
            if (_inkSlider) _inkSlider.value = 1f;
            if (_matchRateText) _matchRateText.text = "0%";

            // Setup palette buttons for stage
            Color[] colors = FingerPaintCanvas.GetPaletteColors(stageNumber - 1);
            for (int i = 0; i < _colorButtons.Length; i++)
            {
                if (_colorButtons[i] == null) continue;
                bool visible = i < colors.Length;
                _colorButtons[i].gameObject.SetActive(visible);
                if (visible)
                {
                    var img = _colorButtons[i].GetComponent<Image>();
                    if (img) img.color = colors[i];
                }
            }

            // Thin brush: Stage3+
            if (_thinBrushButton) _thinBrushButton.gameObject.SetActive(stageNumber >= 3);
            // Eraser: Stage4+
            if (_eraserButton) _eraserButton.gameObject.SetActive(stageNumber >= 4);

            // Reset brush state
            _isThinBrush = false;
            _isEraserMode = false;
            if (_canvas)
            {
                _canvas.SetThinBrush(false);
                _canvas.SetEraserMode(false);
            }

            // Set initial color
            if (colors.Length > 0 && _canvas)
                _canvas.SetColor(colors[0]);
        }

        public void UpdateHUD(float matchRate, float inkAmount, float timeLeft)
        {
            if (_matchRateText) _matchRateText.text = $"{Mathf.RoundToInt(matchRate * 100)}%";
            if (_inkSlider) _inkSlider.value = inkAmount;
            if (_timerText) _timerText.text = $"{Mathf.CeilToInt(Mathf.Max(0f, timeLeft))}";

            // Flash timer red when low
            if (_timerText) _timerText.color = timeLeft < 10f ? Color.red : Color.white;
        }

        public void ShowCombo(int count)
        {
            if (_comboText == null) return;
            if (count < 3)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"COMBO x{count}!";

            if (_comboRoutine != null) StopCoroutine(_comboRoutine);
            _comboRoutine = StartCoroutine(PulseCombo());
        }

        private IEnumerator PulseCombo()
        {
            _comboText.transform.localScale = Vector3.one * 1.3f;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                _comboText.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, t / 0.2f);
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }

        public void ShowStageClear(int score, int stars)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText) _stageClearScoreText.text = $"スコア: {score}";
            if (_stageClearStarsText) _stageClearStarsText.text = new string('★', stars) + new string('☆', 3 - stars);
        }

        public void ShowFinalClear(int score)
        {
            if (_finalClearPanel) _finalClearPanel.SetActive(true);
            if (_finalScoreText) _finalScoreText.text = $"最終スコア: {score}";
        }

        public void ShowGameOver(float matchRate)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverMatchText) _gameOverMatchText.text = $"一致率: {Mathf.RoundToInt(matchRate * 100)}%\n目標: {Mathf.RoundToInt(_targetMatch * 100)}%";
        }

        // --- Button callbacks (wired in SceneSetup) ---
        public void OnColorButtonPressed(int index)
        {
            Color[] colors = FingerPaintCanvas.GetPaletteColors(_currentStage - 1);
            if (index < colors.Length && _canvas)
            {
                _canvas.SetColor(colors[index]);
                _isEraserMode = false;
                _canvas.SetEraserMode(false);

                // Pulse the button
                if (index < _colorButtons.Length && _colorButtons[index] != null)
                    StartCoroutine(PulseButton(_colorButtons[index].transform));
            }
        }

        public void OnThinBrushToggle()
        {
            _isThinBrush = !_isThinBrush;
            if (_canvas) _canvas.SetThinBrush(_isThinBrush);

            if (_thinBrushButton)
            {
                var img = _thinBrushButton.GetComponent<Image>();
                if (img) img.color = _isThinBrush ? new Color(0.8f, 1f, 0.8f) : Color.white;
            }
        }

        public void OnEraserToggle()
        {
            _isEraserMode = !_isEraserMode;
            if (_canvas) _canvas.SetEraserMode(_isEraserMode);

            if (_eraserButton)
            {
                var img = _eraserButton.GetComponent<Image>();
                if (img) img.color = _isEraserMode ? new Color(1f, 0.7f, 0.7f) : Color.white;
            }
        }

        public void OnNextStageButtonPressed()
        {
            if (_gameManager) _gameManager.OnStageClearButtonPressed();
        }

        private IEnumerator PulseButton(Transform t)
        {
            t.localScale = Vector3.one * 1.3f;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, elapsed / 0.15f);
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }
}
