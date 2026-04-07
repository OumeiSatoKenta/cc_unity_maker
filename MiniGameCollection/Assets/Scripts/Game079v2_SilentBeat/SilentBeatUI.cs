using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game079v2_SilentBeat
{
    public class SilentBeatUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] TextMeshProUGUI _progressText;
        [SerializeField] TextMeshProUGUI _bpmText;
        [SerializeField] TextMeshProUGUI _guidePhaseText;
        [SerializeField] Image _tapAreaImage;
        [SerializeField] Sprite _tapAreaIdleSprite;
        [SerializeField] Sprite _tapAreaActiveSprite;
        [SerializeField] RectTransform _accuracyIndicator;
        [SerializeField] Image _accuracyDot;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        Coroutine _judgementCoroutine;
        Coroutine _comboCoroutine;
        Coroutine _tapFlashCoroutine;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score:N0}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            _comboText.text = combo > 1 ? $"×{combo} Combo" : "";

            if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
            if (combo > 1) _comboCoroutine = StartCoroutine(ComboAnim(combo));
        }

        IEnumerator ComboAnim(int combo)
        {
            float scale = combo >= 10 ? 1.4f : 1.2f;
            Color color = combo >= 10
                ? new Color(1f, 0.85f, 0f)
                : new Color(0.3f, 1f, 0.5f);
            _comboText.color = color;
            _comboText.transform.localScale = Vector3.one * scale;
            yield return new WaitForSeconds(0.15f);
            _comboText.transform.localScale = Vector3.one;
            _comboText.color = Color.white;
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);
            _judgementCoroutine = StartCoroutine(FadeJudgement());
        }

        IEnumerator FadeJudgement()
        {
            _judgementText.transform.localScale = Vector3.one * 1.3f;
            yield return new WaitForSeconds(0.1f);
            _judgementText.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.6f);
            _judgementText.gameObject.SetActive(false);
        }

        public void UpdateProgress(int current, int total)
        {
            if (_progressText) _progressText.text = $"{current} / {total}";
        }

        public void UpdateBpm(float bpm)
        {
            if (_bpmText) _bpmText.text = $"BPM: {bpm:F0}";
        }

        public void ShowGuidePhase()
        {
            if (_guidePhaseText)
            {
                _guidePhaseText.gameObject.SetActive(true);
                _guidePhaseText.text = "リズムを覚えよう！";
            }
            if (_bpmText) _bpmText.gameObject.SetActive(true);
        }

        public void HideGuidePhase()
        {
            if (_guidePhaseText) _guidePhaseText.gameObject.SetActive(false);
            if (_bpmText) _bpmText.gameObject.SetActive(false);
        }

        public void FlashTapArea(bool isActive)
        {
            if (_tapAreaImage == null) return;
            if (_tapFlashCoroutine != null) StopCoroutine(_tapFlashCoroutine);

            if (isActive)
            {
                if (_tapAreaActiveSprite) _tapAreaImage.sprite = _tapAreaActiveSprite;
                _tapFlashCoroutine = StartCoroutine(TapPulseAnim());
            }
            else
            {
                if (_tapAreaIdleSprite) _tapAreaImage.sprite = _tapAreaIdleSprite;
                _tapAreaImage.transform.localScale = Vector3.one;
            }
        }

        IEnumerator TapPulseAnim()
        {
            _tapAreaImage.transform.localScale = Vector3.one * 1.15f;
            yield return new WaitForSeconds(0.08f);
            _tapAreaImage.transform.localScale = Vector3.one;
        }

        public void UpdateAccuracyIndicator(float normalizedDeviation)
        {
            if (_accuracyDot == null || _accuracyIndicator == null) return;
            // normalizedDeviation: -1 (too early) to +1 (too late)
            float clampedDev = Mathf.Clamp(normalizedDeviation, -1f, 1f);
            float barWidth = _accuracyIndicator.rect.width * 0.4f;
            _accuracyDot.rectTransform.anchoredPosition = new Vector2(clampedDev * barWidth, 0f);

            // Color: perfect = gold, otherwise red gradient
            float absNorm = Mathf.Abs(clampedDev);
            _accuracyDot.color = Color.Lerp(new Color(1f, 0.85f, 0f), new Color(1f, 0.2f, 0.2f), absNorm);
        }

        public void ShowStageClear(int stage)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearText) _stageClearText.text = $"Stage {stage} クリア！";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel) _allClearPanel.SetActive(true);
            if (_allClearScoreText) _allClearScoreText.text = $"最終スコア: {score:N0}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"スコア: {score:N0}";
        }
    }
}
