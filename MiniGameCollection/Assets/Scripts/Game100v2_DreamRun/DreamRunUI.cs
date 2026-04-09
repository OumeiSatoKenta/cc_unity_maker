using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game100v2_DreamRun
{
    public class DreamRunUI : MonoBehaviour
    {
        [SerializeField] DreamRunGameManager _gameManager;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _livesText;
        [SerializeField] TextMeshProUGUI _fragmentText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] Button _backToMenuButton;

        // Stage clear panel
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        // Game over panel
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverText;
        [SerializeField] Button _restartButton;

        // All clear panel
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearText;

        Coroutine _comboCoroutine;

        void Start()
        {
            if (_backToMenuButton != null)
                _backToMenuButton.onClick.AddListener(OnBackToMenu);
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(OnNextStage);
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestart);

            if (_comboText != null) _comboText.gameObject.SetActive(false);
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateLives(int lives, int maxLives)
        {
            if (_livesText == null) return;
            string hearts = "";
            for (int i = 0; i < maxLives; i++)
                hearts += i < lives ? "♥" : "♡";
            _livesText.text = hearts;
        }

        public void UpdateFragments(int collected, int total)
        {
            if (_fragmentText != null) _fragmentText.text = $"断片 {collected}/{total}";
        }

        public void ShowComboIfNeeded(int combo)
        {
            if (_comboText == null || combo < 2) return;
            if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
            _comboCoroutine = StartCoroutine(ShowComboAnimation(combo));
        }

        IEnumerator ShowComboAnimation(int combo)
        {
            _comboText.text = $"コンボ x{combo}！";
            _comboText.gameObject.SetActive(true);
            _comboText.transform.localScale = Vector3.one * 0.5f;

            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                _comboText.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, t / 0.15f);
                yield return null;
            }
            t = 0f;
            while (t < 0.05f)
            {
                t += Time.deltaTime;
                _comboText.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1.0f, t / 0.05f);
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(1.2f);
            _comboText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int stage, int score)
        {
            if (_stageClearPanel == null) return;
            if (_stageClearText != null) _stageClearText.text = $"ステージ{stage} クリア！\nScore: {score}";
            _stageClearPanel.SetActive(true);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel == null) return;
            if (_gameOverText != null) _gameOverText.text = $"ゲームオーバー\nScore: {score}";
            _gameOverPanel.SetActive(true);
        }

        public void HideGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel == null) return;
            if (_allClearText != null) _allClearText.text = $"全ステージクリア！\n夢の断片を集めた！\nFinal Score: {score}";
            _allClearPanel.SetActive(true);
        }

        void OnBackToMenu()
        {
            SceneLoader.BackToMenu();
        }

        void OnNextStage()
        {
            if (_gameManager != null) _gameManager.NextStage();
        }

        void OnRestart()
        {
            if (_gameManager != null) _gameManager.RestartGame();
        }
    }
}
