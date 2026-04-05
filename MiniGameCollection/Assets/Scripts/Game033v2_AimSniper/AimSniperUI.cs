using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game033v2_AimSniper
{
    public class AimSniperUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _bulletText;
        [SerializeField] TextMeshProUGUI _targetText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] Image _windArrow;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearTitle;
        [SerializeField] TextMeshProUGUI _stageClearBonus;
        [SerializeField] Button _nextStageButton;
        [SerializeField] Button _stageClearMenuButton;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;
        [SerializeField] Button _restartButton;
        [SerializeField] Button _menuButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _menuButton2;

        AimSniperGameManager _gameManager;
        Coroutine _comboFade;

        public void Initialize(AimSniperGameManager gm)
        {
            _gameManager = gm;
            HideStageClear();
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);

            // RemoveAllListeners to prevent duplicate registration on re-init
            if (_nextStageButton != null) { _nextStageButton.onClick.RemoveAllListeners(); _nextStageButton.onClick.AddListener(() => _gameManager.OnNextStagePressed()); }
            if (_stageClearMenuButton != null) { _stageClearMenuButton.onClick.RemoveAllListeners(); _stageClearMenuButton.onClick.AddListener(() => _gameManager.ReturnToMenu()); }
            if (_restartButton != null) { _restartButton.onClick.RemoveAllListeners(); _restartButton.onClick.AddListener(() => _gameManager.RestartGame()); }
            if (_menuButton != null) { _menuButton.onClick.RemoveAllListeners(); _menuButton.onClick.AddListener(() => _gameManager.ReturnToMenu()); }
            if (_retryButton != null) { _retryButton.onClick.RemoveAllListeners(); _retryButton.onClick.AddListener(() => _gameManager.RestartGame()); }
            if (_menuButton2 != null) { _menuButton2.onClick.RemoveAllListeners(); _menuButton2.onClick.AddListener(() => _gameManager.ReturnToMenu()); }
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void UpdateBullets(int bullets)
        {
            if (_bulletText != null)
            {
                _bulletText.text = $"弾: {bullets}";
                _bulletText.color = bullets <= 2 ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.9f, 0.3f);
            }
        }

        public void UpdateTargets(int targets)
        {
            if (_targetText != null)
                _targetText.text = $"残: {targets}";
        }

        public void UpdateWindIndicator(Vector2 wind)
        {
            if (_windArrow == null) return;
            bool hasWind = wind.magnitude > 0.05f;
            _windArrow.gameObject.SetActive(hasWind);
            if (hasWind)
            {
                float angle = Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg;
                _windArrow.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
                float intensity = Mathf.Clamp01(wind.magnitude / 0.4f);
                _windArrow.color = new Color(0.4f + intensity * 0.6f, 0.8f, 1f, 0.8f);
            }
        }

        public void ShowCombo(float multiplier)
        {
            if (_comboText == null) return;
            _comboText.text = $"x{multiplier:F1} COMBO!";
            _comboText.gameObject.SetActive(true);
            if (_comboFade != null) StopCoroutine(_comboFade);
            _comboFade = StartCoroutine(FadeCombo());
        }

        System.Collections.IEnumerator FadeCombo()
        {
            yield return new WaitForSeconds(0.8f);
            float elapsed = 0f;
            Color c = _comboText.color;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                c.a = 1f - elapsed / 0.4f;
                _comboText.color = c;
                yield return null;
            }
            _comboText.gameObject.SetActive(false);
            c.a = 1f;
            _comboText.color = c;
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowStageClear(int stage, int bonus)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearTitle != null)
                _stageClearTitle.text = $"Stage {stage} クリア！";
            if (_stageClearBonus != null)
                _stageClearBonus.text = bonus > 0 ? $"残弾ボーナス +{bonus}" : "";
        }

        public void ShowFinalClear(int score)
        {
            HideStageClear();
            if (_finalClearPanel == null) return;
            _finalClearPanel.SetActive(true);
            if (_finalScoreText != null)
                _finalScoreText.text = $"全ステージクリア！\nScore: {score}";
        }

        public void ShowGameOver(int score)
        {
            HideStageClear();
            if (_gameOverPanel == null) return;
            _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null)
                _gameOverScoreText.text = $"ゲームオーバー\nScore: {score}";
        }
    }
}
