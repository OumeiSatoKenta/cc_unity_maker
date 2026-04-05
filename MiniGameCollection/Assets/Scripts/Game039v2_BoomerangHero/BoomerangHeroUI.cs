using UnityEngine;
using TMPro;
using System.Collections;

namespace Game039v2_BoomerangHero
{
    public class BoomerangHeroUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _ammoText;
        [SerializeField] TextMeshProUGUI _enemyCountText;
        [SerializeField] TextMeshProUGUI _scorePopupText;

        Coroutine _popupCoroutine;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        public void UpdateStage(int stage)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / 5";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateAmmo(int remaining, int max)
        {
            if (_ammoText) _ammoText.text = $"弾: {remaining} / {max}";
        }

        public void UpdateEnemyCount(int remaining)
        {
            if (_enemyCountText) _enemyCountText.text = $"敵: {remaining}";
        }

        public void ShowScorePopup(int points)
        {
            if (_scorePopupText == null) return;
            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(PopupAnimation(points));
        }

        IEnumerator PopupAnimation(int points)
        {
            _scorePopupText.text = $"+{points}";
            _scorePopupText.gameObject.SetActive(true);
            _scorePopupText.color = new Color(1f, 0.9f, 0.2f, 1f);
            Vector3 startPos = _scorePopupText.rectTransform.anchoredPosition;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                _scorePopupText.rectTransform.anchoredPosition = startPos + Vector3.up * (t * 60f);
                _scorePopupText.color = new Color(1f, 0.9f, 0.2f, 1f - t);
                yield return null;
            }
            _scorePopupText.rectTransform.anchoredPosition = startPos;
            _scorePopupText.gameObject.SetActive(false);
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_finalClearPanel) _finalClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }

        public void ShowStageClearPanel(int score)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText) _stageClearScoreText.text = $"Score: {score}";
        }

        public void ShowFinalClearPanel(int score)
        {
            if (_finalClearPanel) _finalClearPanel.SetActive(true);
            if (_finalScoreText) _finalScoreText.text = $"全ステージクリア！\nScore: {score}";
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"ゲームオーバー\nScore: {score}";
        }
    }
}
