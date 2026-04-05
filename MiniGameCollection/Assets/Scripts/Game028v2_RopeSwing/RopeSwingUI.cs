using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game028v2_RopeSwing
{
    public class RopeSwingUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _landingFeedbackText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _restartButton;
        [SerializeField] Button _menuButton;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;
        [SerializeField] Button _finalRestartButton;
        [SerializeField] Button _finalMenuButton;

        RopeSwingGameManager _gm;
        Coroutine _feedbackCo;
        Coroutine _comboCo;

        public void Initialize(RopeSwingGameManager gm)
        {
            _gm = gm;

            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
            if (_finalClearPanel) _finalClearPanel.SetActive(false);
            if (_landingFeedbackText) _landingFeedbackText.gameObject.SetActive(false);

            if (_nextStageButton) _nextStageButton.onClick.RemoveAllListeners();
            if (_restartButton) _restartButton.onClick.RemoveAllListeners();
            if (_menuButton) _menuButton.onClick.RemoveAllListeners();
            if (_finalRestartButton) _finalRestartButton.onClick.RemoveAllListeners();
            if (_finalMenuButton) _finalMenuButton.onClick.RemoveAllListeners();

            if (_nextStageButton) _nextStageButton.onClick.AddListener(() => _stageClearPanel.SetActive(false));
            if (_restartButton) _restartButton.onClick.AddListener(() => _gm.RestartGame());
            if (_menuButton) _menuButton.onClick.AddListener(() => _gm.ReturnToMenu());
            if (_finalRestartButton) _finalRestartButton.onClick.AddListener(() => _gm.RestartGame());
            if (_finalMenuButton) _finalMenuButton.onClick.AddListener(() => _gm.ReturnToMenu());
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }


        public void ShowLandingFeedback(string grade, int combo)
        {
            if (_landingFeedbackText == null) return;
            if (_feedbackCo != null) StopCoroutine(_feedbackCo);
            _feedbackCo = StartCoroutine(FeedbackSequence(grade, combo));
        }

        IEnumerator FeedbackSequence(string grade, int combo)
        {
            _landingFeedbackText.gameObject.SetActive(true);
            string comboStr = combo >= 2 ? $"\nCombo x{combo}!" : "";
            _landingFeedbackText.text = grade + comboStr;

            Color c = grade == "Perfect!" ? new Color(1f, 0.9f, 0.2f) :
                      grade == "Good!" ? new Color(0.3f, 1f, 0.5f) :
                      new Color(0.8f, 0.8f, 0.8f);
            _landingFeedbackText.color = c;
            _landingFeedbackText.transform.localScale = Vector3.one * 1.3f;

            float elapsed = 0f;
            float dur = 1.2f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                _landingFeedbackText.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one * 0.8f, t);
                float alpha = t < 0.7f ? 1f : 1f - (t - 0.7f) / 0.3f;
                _landingFeedbackText.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
            _landingFeedbackText.gameObject.SetActive(false);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowStageClear(int stage, int bonus)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearText) _stageClearText.text = $"ステージ {stage} クリア！\nボーナス: +{bonus}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel == null) return;
            _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"スコア: {score}";
        }

        public void ShowFinalClear(int score)
        {
            if (_finalClearPanel == null) return;
            _finalClearPanel.SetActive(true);
            if (_finalScoreText) _finalScoreText.text = $"全ステージクリア！\n最終スコア: {score}";
        }
    }
}
