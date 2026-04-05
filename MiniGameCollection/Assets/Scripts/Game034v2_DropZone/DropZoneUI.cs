using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game034v2_DropZone
{
    public class DropZoneUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _missText;
        [SerializeField] TextMeshProUGUI _comboText;

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

        DropZoneGameManager _gm;
        Coroutine _comboHideCo;

        public void Initialize(DropZoneGameManager gm)
        {
            _gm = gm;
            if (_nextStageButton) { _nextStageButton.onClick.RemoveAllListeners(); _nextStageButton.onClick.AddListener(() => _gm.OnNextStagePressed()); }
            if (_stageClearMenuButton) { _stageClearMenuButton.onClick.RemoveAllListeners(); _stageClearMenuButton.onClick.AddListener(() => _gm.ReturnToMenu()); }
            if (_restartButton) { _restartButton.onClick.RemoveAllListeners(); _restartButton.onClick.AddListener(() => _gm.RestartGame()); }
            if (_menuButton) { _menuButton.onClick.RemoveAllListeners(); _menuButton.onClick.AddListener(() => _gm.ReturnToMenu()); }
            if (_retryButton) { _retryButton.onClick.RemoveAllListeners(); _retryButton.onClick.AddListener(() => _gm.RestartGame()); }
            if (_menuButton2) { _menuButton2.onClick.RemoveAllListeners(); _menuButton2.onClick.AddListener(() => _gm.ReturnToMenu()); }

            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_finalClearPanel) _finalClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
            if (_comboText) _comboText.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMiss(int miss, int maxMiss)
        {
            if (_missText) _missText.text = $"ミス: {miss}/{maxMiss}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 1)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"×{combo} COMBO!";

            if (_comboHideCo != null) StopCoroutine(_comboHideCo);
            _comboHideCo = StartCoroutine(HideComboAfterDelay());
        }

        IEnumerator HideComboAfterDelay()
        {
            yield return new WaitForSeconds(1.2f);
            if (_comboText) _comboText.gameObject.SetActive(false);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowStageClear(int stage, int bonus)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearTitle) _stageClearTitle.text = $"Stage {stage} クリア！";
            if (_stageClearBonus) _stageClearBonus.text = bonus > 0 ? $"ボーナス: +{bonus}pt" : "";
        }

        public void ShowFinalClear(int score)
        {
            if (_finalClearPanel) _finalClearPanel.SetActive(true);
            if (_finalScoreText) _finalScoreText.text = $"全ステージクリア！\nScore: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"ゲームオーバー\nScore: {score}";
        }
    }
}
