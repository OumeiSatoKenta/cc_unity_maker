using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game047v2_SpinBalance
{
    public class SpinBalanceUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _timerText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _coinCountText;
        [SerializeField] TextMeshProUGUI _multiplierText;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] Image _brakeIcon;
        [SerializeField] TextMeshProUGUI _brakeCooldownText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        [SerializeField] SpinBalanceGameManager _gameManager;

        void Start()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_allClearPanel) _allClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
            if (_multiplierText) _multiplierText.gameObject.SetActive(false);
        }

        public void UpdateTimer(float remaining)
        {
            if (_timerText == null) return;
            _timerText.text = $"{Mathf.CeilToInt(remaining)}";
            _timerText.color = remaining < 5f ? Color.red : Color.white;
        }

        public void UpdateScore(int score, float multiplier)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMultiplier(float mult)
        {
            if (_multiplierText == null) return;
            bool show = mult > 1f;
            _multiplierText.gameObject.SetActive(show);
            if (show)
            {
                _multiplierText.text = $"x{mult:F0}";
                StartCoroutine(PulseText(_multiplierText.transform));
            }
        }

        System.Collections.IEnumerator PulseText(Transform t)
        {
            t.localScale = new Vector3(1.3f, 1.3f, 1f);
            float d = 0.2f, elapsed = 0f;
            while (elapsed < d)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.Lerp(1.3f, 1f, elapsed / d);
                t.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        public void UpdateCoinCount(int current, int max)
        {
            if (_coinCountText) _coinCountText.text = $"コマ: {current}/{max}";
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage}/{total}";
        }

        public void UpdateBrake(bool available, float cooldownRatio)
        {
            if (_brakeIcon)
                _brakeIcon.color = available ? Color.yellow : Color.gray;
            if (_brakeCooldownText)
            {
                _brakeCooldownText.gameObject.SetActive(!available);
                if (!available)
                    _brakeCooldownText.text = $"{Mathf.CeilToInt(cooldownRatio * 5f)}s";
            }
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText) _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText) _allClearScoreText.text = $"Score: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText) _gameOverScoreText.text = $"Score: {score}";
            }
        }

        public void OnNextStageClicked()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_gameManager) _gameManager.GoNextStage();
        }

        public void OnRestartClicked()
        {
            if (_gameManager) _gameManager.RestartGame();
        }

        public void OnMenuClicked()
        {
            if (_gameManager) _gameManager.GoToMenu();
        }
    }
}
