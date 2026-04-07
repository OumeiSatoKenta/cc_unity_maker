using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game080v2_FreqFight
{
    public class FreqFightUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] TextMeshProUGUI _phaseText;

        [Header("Beat Guide")]
        [SerializeField] Image _beatGuideImage;

        [Header("HP Bars")]
        [SerializeField] Slider _playerHpSlider;
        [SerializeField] Slider _enemyHpSlider1;
        [SerializeField] Slider _enemyHpSlider2;

        [Header("Enemy Freq Markers")]
        [SerializeField] RectTransform _enemyFreqMarker1;
        [SerializeField] RectTransform _enemyFreqMarker2;

        [Header("Enemy Sprites")]
        [SerializeField] Image _enemyImage1;
        [SerializeField] Image _enemyImage2;

        [Header("Panels")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        // Slider track rect for marker positioning
        [SerializeField] RectTransform _sliderTrackRect;
        [SerializeField] RectTransform _sliderTrackRect2;

        Coroutine _judgementCoroutine;
        Coroutine _comboCoroutine;
        Coroutine _beatCoroutine;
        Coroutine _shakeCoroutine1;
        Coroutine _shakeCoroutine2;

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            _comboText.text = combo >= 2 ? $"x{combo} Combo!" : "";
            _comboText.color = combo >= 10 ? new Color(1f, 0.85f, 0f) : Color.white;

            if (combo >= 2)
            {
                if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
                _comboCoroutine = StartCoroutine(PulseScale(_comboText.transform, 1.3f, 0.15f));
            }
        }

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementText == null) return;
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);

            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementCoroutine = StartCoroutine(JudgementRoutine());
        }

        IEnumerator JudgementRoutine()
        {
            _judgementText.transform.localScale = Vector3.one;
            yield return StartCoroutine(PulseScale(_judgementText.transform, 1.5f, 0.2f));
            yield return new WaitForSeconds(0.3f);
            _judgementText.gameObject.SetActive(false);
        }

        public void UpdatePhase(string phase)
        {
            if (_phaseText != null)
            {
                _phaseText.text = phase;
                _phaseText.color = phase.Contains("攻撃") ? new Color(1f, 0.5f, 0.3f) : new Color(0.3f, 0.8f, 1f);
            }
        }

        public void UpdatePlayerHp(float ratio)
        {
            if (_playerHpSlider != null)
                _playerHpSlider.value = Mathf.Clamp01(ratio);
        }

        public void UpdateEnemyHp(float ratio, int enemyIndex = 0)
        {
            var slider = enemyIndex == 0 ? _enemyHpSlider1 : _enemyHpSlider2;
            if (slider != null)
                slider.value = Mathf.Clamp01(ratio);
        }

        public void UpdateEnemyFreqMarker(float normalizedFreq, int enemyIndex = 0)
        {
            var marker = enemyIndex == 0 ? _enemyFreqMarker1 : _enemyFreqMarker2;
            var trackRect = enemyIndex == 0 ? _sliderTrackRect : _sliderTrackRect2;
            if (marker == null || trackRect == null) return;

            float trackWidth = trackRect.rect.width;
            float xPos = trackRect.anchoredPosition.x - trackWidth * 0.5f + trackWidth * normalizedFreq;
            marker.anchoredPosition = new Vector2(xPos, marker.anchoredPosition.y);
        }

        public void PulseBeat()
        {
            if (_beatGuideImage == null) return;
            if (_beatCoroutine != null) StopCoroutine(_beatCoroutine);
            _beatCoroutine = StartCoroutine(PulseScale(_beatGuideImage.transform, 1.15f, 0.12f));
        }

        public void ShakeEnemy(int enemyIndex = 0)
        {
            var img = enemyIndex == 0 ? _enemyImage1 : _enemyImage2;
            if (img == null) return;

            ref Coroutine routine = ref (enemyIndex == 0 ? ref _shakeCoroutine1 : ref _shakeCoroutine2);
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(ShakeRoutine(img.transform));
        }

        IEnumerator ShakeRoutine(Transform target)
        {
            Vector3 originalPos = target.localPosition;
            float elapsed = 0f;
            float duration = 0.15f;
            while (elapsed < duration)
            {
                float x = Mathf.Sin(elapsed * 80f) * 6f * (1f - elapsed / duration);
                target.localPosition = originalPos + new Vector3(x, 0f, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            target.localPosition = originalPos;
        }

        IEnumerator PulseScale(Transform target, float maxScale, float duration)
        {
            float half = duration * 0.5f;
            float elapsed = 0f;
            while (elapsed < half)
            {
                float t = elapsed / half;
                target.localScale = Vector3.one * Mathf.Lerp(1f, maxScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < half)
            {
                float t = elapsed / half;
                target.localScale = Vector3.one * Mathf.Lerp(maxScale, 1f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            target.localScale = Vector3.one;
        }

        public void ShowStageClear(int stageNumber)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText != null)
                    _stageClearText.text = $"Stage {stageNumber} Clear!";
            }
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null)
                _stageClearPanel.SetActive(false);
        }

        public void ShowGameOver(int finalScore)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"Score: {finalScore}";
            }
        }

        public void ShowAllClear(int finalScore)
        {
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null)
                    _allClearScoreText.text = $"Final Score: {finalScore}";
            }
        }
    }
}
