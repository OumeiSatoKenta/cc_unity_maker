using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game054v2_FruitSlash
{
    public class FruitSlashUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _targetScoreText;
        [SerializeField] Slider _progressSlider;
        [SerializeField] GameObject[] _heartObjects;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] Button _allClearRetryButton;

        [SerializeField] Button _menuButton;

        Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = score.ToString("N0");
            if (_progressSlider) _progressSlider.value = Mathf.Clamp01((float)score / Mathf.Max(1, _progressSlider.maxValue));
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (!_comboText) return;
            if (combo <= 1)
            {
                _comboText.text = "";
                return;
            }
            _comboText.text = $"×{multiplier:F1} COMBO {combo}";
        }

        public void UpdateTimer(float time, float maxTime)
        {
            if (_timerText) _timerText.text = Mathf.CeilToInt(time).ToString() + "s";
            if (_timerText) _timerText.color = time <= 10f ? Color.red : Color.white;
        }

        public void UpdateStage(int stage, int total, int targetScore)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
            if (_targetScoreText) _targetScoreText.text = $"目標: {targetScore}";
            if (_progressSlider) _progressSlider.maxValue = targetScore;
        }

        public void UpdateLife(int life, int maxLife)
        {
            if (_heartObjects == null) return;
            for (int i = 0; i < _heartObjects.Length; i++)
            {
                if (_heartObjects[i] == null) continue;
                bool active = i < life;
                _heartObjects[i].SetActive(active);
                var sr = _heartObjects[i].GetComponent<Image>();
                if (sr) sr.color = active ? Color.red : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }

        public void PlayComboEffect(int combo)
        {
            if (_comboText == null) return;
            StopAllCoroutines();
            StartCoroutine(PulseText(_comboText.transform, combo >= 10 ? 1.6f : 1.3f));
            if (combo >= 5)
                _comboText.color = combo >= 20 ? Color.cyan : combo >= 10 ? Color.yellow : Color.white;
        }

        public void PlayBombEffect()
        {
            if (_cam) StartCoroutine(CameraShake(0.3f, 0.15f));
        }

        public void ShowIceFreezeEffect()
        {
            if (_comboText)
            {
                _comboText.text = "FREEZE!";
                _comboText.color = new Color(0.5f, 0.8f, 1f);
                StartCoroutine(PulseText(_comboText.transform, 1.5f));
            }
        }

        IEnumerator PulseText(Transform t, float maxScale)
        {
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float ratio = elapsed / 0.1f;
                float scale = ratio < 1f ? Mathf.Lerp(1f, maxScale, ratio) : Mathf.Lerp(maxScale, 1f, ratio - 1f);
                t.localScale = Vector3.one * scale;
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            Vector3 orig = _cam.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                _cam.transform.localPosition = orig + new Vector3(x, y, 0);
                yield return null;
            }
            _cam.transform.localPosition = orig;
        }

        public void ShowStageClear(int stage, int score)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearText) _stageClearText.text = $"Stage {stage} クリア！\nスコア: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"スコア: {score}";
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel) _allClearPanel.SetActive(true);
            if (_allClearScoreText) _allClearScoreText.text = $"最終スコア: {score}";
        }
    }
}
