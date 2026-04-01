using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game052_HammerNail
{
    public class HammerNailUI : MonoBehaviour
    {
        [SerializeField, Tooltip("進捗テキスト")] private TextMeshProUGUI _progressText;
        [SerializeField, Tooltip("ミステキスト")] private TextMeshProUGUI _missText;
        [SerializeField, Tooltip("タイミングゲージ")] private Slider _gaugeSlider;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        [SerializeField, Tooltip("NailManager")] private NailManager _nailManager;

        private void Update()
        {
            if (_gaugeSlider != null && _nailManager != null)
                _gaugeSlider.value = _nailManager.GaugeValue;
        }

        public void UpdateProgress(int done, int total) { if (_progressText != null) _progressText.text = $"{done}/{total}"; }
        public void UpdateMisses(int misses, int max) { if (_missText != null) _missText.text = $"ミス: {misses}/{max}"; }
        public void ShowClear(int score) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"{score}本成功！"; }
        public void ShowGameOver(int score) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"{score}本成功"; }
    }
}
