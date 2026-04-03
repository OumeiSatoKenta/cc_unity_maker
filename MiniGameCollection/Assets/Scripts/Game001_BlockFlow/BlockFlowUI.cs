using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game001_BlockFlow
{
    public class BlockFlowUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _scoreText;
        [SerializeField, Tooltip("ステージ表示")] private TextMeshProUGUI _stageText;
        [SerializeField, Tooltip("手数表示")] private TextMeshProUGUI _movesText;
        [SerializeField, Tooltip("ステージクリアパネル")] private GameObject _stageClearPanel;
        [SerializeField, Tooltip("ステージクリアテキスト")] private TextMeshProUGUI _stageClearText;
        [SerializeField, Tooltip("次ステージボタン")] private Button _nextStageButton;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアテキスト")] private TextMeshProUGUI _clearText;
        [SerializeField, Tooltip("ゲームオーバーパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("ゲームオーバーテキスト")] private TextMeshProUGUI _gameOverText;

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"SCORE: {score}";
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateMoves(int moves, int limit)
        {
            if (_movesText)
            {
                if (limit > 0)
                {
                    _movesText.gameObject.SetActive(true);
                    int remaining = limit - moves;
                    _movesText.text = $"残り: {remaining}手";
                    _movesText.color = remaining <= 3 ? new Color(1f, 0.3f, 0.3f) : Color.white;
                }
                else
                {
                    _movesText.gameObject.SetActive(true);
                    _movesText.text = $"手数: {moves}";
                    _movesText.color = Color.white;
                }
            }
        }

        public void ShowStageClearPanel(int stage, int score, string bonus)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearText)
            {
                string text = $"Stage {stage} クリア！\nスコア: {score}";
                if (!string.IsNullOrEmpty(bonus)) text += $"\n{bonus}";
                _stageClearText.text = text;
            }
        }

        public void ShowClearPanel(int score)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearText) _clearText.text = $"全ステージクリア！\n\n最終スコア: {score}";
        }

        public void ShowGameOverPanel(int score, int stage)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverText) _gameOverText.text = $"ゲームオーバー\n\nStage {stage}\nスコア: {score}";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_clearPanel) _clearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }
    }
}
