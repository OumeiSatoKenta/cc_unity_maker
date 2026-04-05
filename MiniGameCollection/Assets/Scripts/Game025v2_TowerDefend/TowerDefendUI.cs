using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game025v2_TowerDefend
{
    public class TowerDefendUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _waveText;
        [SerializeField] TextMeshProUGUI _breachText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] Slider _inkSlider;
        [SerializeField] TextMeshProUGUI _inkPercentText;
        [SerializeField] Button _waveStartButton;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearBonusText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _menuButton;

        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearScoreText;
        [SerializeField] Button _clearMenuButton;

        [SerializeField] TextMeshProUGUI _bonusPopText;

        [SerializeField] Image _screenFlash;

        TowerDefendGameManager _gameManager;
        WallManager _wallManager;
        Camera _mainCam;
        Coroutine _bonusCoroutine;
        float _camShakeTimer;
        float _camShakeIntensity;
        Vector3 _camBasePos;

        public void Initialize(TowerDefendGameManager gm, WallManager wm)
        {
            _gameManager = gm;
            _wallManager = wm;
            _mainCam = Camera.main;
            if (_mainCam != null) _camBasePos = _mainCam.transform.position;

            _stageClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _clearPanel?.SetActive(false);
            if (_bonusPopText != null) _bonusPopText.gameObject.SetActive(false);
            if (_screenFlash != null) _screenFlash.gameObject.SetActive(false);
        }

        void Update()
        {
            // Ink display
            if (_wallManager != null)
            {
                float ratio = _wallManager.InkRatio;
                if (_inkSlider != null) _inkSlider.value = ratio;
                if (_inkPercentText != null) _inkPercentText.text = $"{Mathf.RoundToInt(ratio * 100)}%";
                if (_inkSlider != null)
                {
                    var fill = _inkSlider.fillRect?.GetComponent<Image>();
                    if (fill != null)
                        fill.color = ratio > 0.5f ? new Color(0f, 0.74f, 0.83f) :
                                     ratio > 0.25f ? new Color(1f, 0.6f, 0f) :
                                                     new Color(0.9f, 0.2f, 0.2f);
                }
            }

            // Camera shake
            if (_camShakeTimer > 0f && _mainCam != null)
            {
                _camShakeTimer -= Time.deltaTime;
                if (_camShakeTimer <= 0f)
                    _mainCam.transform.position = _camBasePos;
                else
                    _mainCam.transform.position = _camBasePos + (Vector3)Random.insideUnitCircle * _camShakeIntensity;
            }
        }

        public void UpdateStageDisplay(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateWave(int wave, int total)
        {
            if (_waveText != null) _waveText.text = $"Wave {wave} / {total}";
        }

        public void UpdateBreach(int count, int max)
        {
            if (_breachText != null)
            {
                _breachText.text = $"突破 {count} / {max}";
                _breachText.color = count >= max - 1 ? new Color(1f, 0.3f, 0.3f) : Color.white;
            }
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString();
        }

        public void UpdateInk(float ratio)
        {
            // Handled in Update
        }

        public void ShowWaveStartButton(bool show)
        {
            if (_waveStartButton != null) _waveStartButton.gameObject.SetActive(show);
        }

        public void ShowWaveBonus(int bonus)
        {
            ShowBonusText($"Wave完封！ +{bonus}");
        }

        public void ShowBreachEffect()
        {
            StartCoroutine(ScreenFlash(new Color(0.9f, 0.1f, 0.1f, 0.5f)));
            ShakeCamera(0.3f, 0.12f);
        }

        void ShakeCamera(float duration, float intensity)
        {
            if (_mainCam == null) return;
            _camShakeTimer = duration;
            _camShakeIntensity = intensity;
            _camBasePos = _mainCam.transform.position;
        }

        IEnumerator ScreenFlash(Color c)
        {
            if (_screenFlash == null) yield break;
            _screenFlash.gameObject.SetActive(true);
            _screenFlash.color = c;
            yield return new WaitForSeconds(0.1f);
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                _screenFlash.color = Color.Lerp(c, Color.clear, t / 0.3f);
                yield return null;
            }
            _screenFlash.gameObject.SetActive(false);
        }

        void ShowBonusText(string msg)
        {
            if (_bonusPopText == null) return;
            if (_bonusCoroutine != null) StopCoroutine(_bonusCoroutine);
            _bonusCoroutine = StartCoroutine(BonusPopCoroutine(msg));
        }

        IEnumerator BonusPopCoroutine(string msg)
        {
            _bonusPopText.gameObject.SetActive(true);
            _bonusPopText.text = msg;
            _bonusPopText.transform.localScale = Vector3.one;
            float t = 0f;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                float scale = t < 0.2f ? Mathf.Lerp(0.5f, 1.2f, t / 0.2f) : Mathf.Lerp(1.2f, 1f, (t - 0.2f) / 0.6f);
                _bonusPopText.transform.localScale = Vector3.one * scale;
                float alpha = t > 0.5f ? Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.3f) : 1f;
                _bonusPopText.color = new Color(1f, 0.9f, 0.2f, alpha);
                yield return null;
            }
            _bonusPopText.gameObject.SetActive(false);
        }

        public void ShowStageClearPanel(int score, int bonus, float multiplier)
        {
            _stageClearPanel?.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"スコア: {score}";
            if (_stageClearBonusText != null)
            {
                if (multiplier > 1f)
                    _stageClearBonusText.text = $"インクボーナス x{multiplier:F1}: +{bonus}";
                else
                    _stageClearBonusText.text = "インクを節約するとボーナスあり！";
            }
        }

        public void HideStageClearPanel()
        {
            _stageClearPanel?.SetActive(false);
        }

        public void ShowGameOverPanel(int score)
        {
            _gameOverPanel?.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"スコア: {score}";
        }

        public void ShowClearPanel(int score)
        {
            _clearPanel?.SetActive(true);
            if (_clearScoreText != null) _clearScoreText.text = $"最終スコア: {score}";
        }
    }
}
