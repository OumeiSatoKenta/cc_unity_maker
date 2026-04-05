using UnityEngine;
using TMPro;

namespace Game040v2_DashDungeon
{
    public class DashDungeonUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _hpText;
        [SerializeField] TextMeshProUGUI _movesText;
        [SerializeField] TextMeshProUGUI _scoreText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        public void UpdateStage(int stage)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {stage} / 5";
        }

        public void UpdateHp(int hp, int maxHp)
        {
            if (_hpText == null) return;
            string hearts = "";
            for (int i = 0; i < maxHp; i++)
                hearts += i < hp ? "♥ " : "♡ ";
            _hpText.text = $"HP: {hearts.TrimEnd()}";
            _hpText.color = hp <= 1 ? new Color(1f, 0.3f, 0.3f) : Color.white;
        }

        public void UpdateMoves(int moves, int minMoves)
        {
            if (_movesText != null)
                _movesText.text = $"手数: {moves} (最短: {minMoves})";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void ShowStageClearPanel(int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null)
                    _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void ShowFinalClearPanel(int score)
        {
            if (_finalClearPanel != null)
            {
                _finalClearPanel.SetActive(true);
                if (_finalScoreText != null)
                    _finalScoreText.text = $"全ステージクリア！\nScore: {score}";
            }
        }

        public void ShowGameOverPanel(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"ゲームオーバー\nScore: {score}";
            }
        }
    }
}
