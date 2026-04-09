using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game099v2_TouchMemory
{
    public class TouchMemoryUI : MonoBehaviour
    {
        [SerializeField] TouchMemoryGameManager _gameManager;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _roundText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _reverseText;

        [Header("Action Buttons")]
        [SerializeField] Button _backToMenuButton;

        [Header("Stage Clear Panel")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [Header("Game Over Panel")]
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverText;
        [SerializeField] Button _restartButton;

        [Header("All Clear Panel")]
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearText;

        Coroutine _comboCoroutine;

        void Start()
        {
            _backToMenuButton?.onClick.AddListener(OnBackToMenuClicked);
            _nextStageButton?.onClick.AddListener(OnNextStageClicked);
            _restartButton?.onClick.AddListener(OnRestartClicked);

            _stageClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);

            if (_comboText != null) _comboText.gameObject.SetActive(false);
            if (_reverseText != null) _reverseText.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateRound(int round)
        {
            if (_roundText != null) _roundText.text = $"Round {round}";
        }

        public void SetReverseIndicator(bool isReverse)
        {
            if (_reverseText != null)
            {
                _reverseText.gameObject.SetActive(isReverse);
                if (isReverse) _reverseText.text = "【逆順】";
            }
        }

        public void ShowComboIfNeeded(int comboCount)
        {
            if (comboCount < 2 || _comboText == null) return;
            _comboText.text = $"コンボ x{comboCount}！";
            _comboText.gameObject.SetActive(true);
            if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
            _comboCoroutine = StartCoroutine(HideAfter(_comboText.gameObject, 1.5f));
        }

        IEnumerator HideAfter(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) go.SetActive(false);
        }

        public void ShowStageClear(int stage, int score)
        {
            _stageClearPanel?.SetActive(true);
            if (_stageClearText != null)
                _stageClearText.text = $"ステージ {stage} クリア！\nスコア: {score}";
        }

        public void HideStageClear()
        {
            _stageClearPanel?.SetActive(false);
        }

        public void ShowGameOver(int score)
        {
            _gameOverPanel?.SetActive(true);
            if (_gameOverText != null)
                _gameOverText.text = $"ミス...\nスコア: {score}";
        }

        public void HideGameOver()
        {
            _gameOverPanel?.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            _allClearPanel?.SetActive(true);
            if (_allClearText != null)
                _allClearText.text = $"全ステージクリア！\n最終スコア: {score}";
        }

        void OnBackToMenuClicked() => SceneLoader.BackToMenu();
        void OnNextStageClicked() => _gameManager?.NextStage();
        void OnRestartClicked() => _gameManager?.RestartGame();
    }
}
