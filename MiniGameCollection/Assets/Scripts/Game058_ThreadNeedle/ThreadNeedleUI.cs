using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game058_ThreadNeedle
{
    public class ThreadNeedleUI : MonoBehaviour
    {
        [SerializeField, Tooltip("ステージ")] private TextMeshProUGUI _stageText;
        [SerializeField, Tooltip("ミス")] private TextMeshProUGUI _missText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateStage(int s, int total) { if (_stageText != null) _stageText.text = $"Stage {s}/{total}"; }
        public void UpdateMisses(int m, int max) { if (_missText != null) _missText.text = $"ミス: {m}/{max}"; }
        public void ShowClear(int stages) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{stages}ステージクリア！"; }
        public void ShowGameOver(int stages) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"{stages}ステージまで"; }
    }
}
