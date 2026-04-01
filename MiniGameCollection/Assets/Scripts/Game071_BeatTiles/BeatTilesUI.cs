using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game071_BeatTiles
{
    public class BeatTilesUI : MonoBehaviour
    {
        [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _scoreText;
        [SerializeField, Tooltip("コンボ")] private TextMeshProUGUI _comboText;
        [SerializeField, Tooltip("ミス")] private TextMeshProUGUI _missText;
        [SerializeField, Tooltip("判定テキスト")] private TextMeshProUGUI _judgeText;
        [SerializeField, Tooltip("進捗スライダー")] private Slider _progressSlider;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        private float _judgeTimer;

        private void Update()
        {
            if (_judgeTimer > 0f)
            {
                _judgeTimer -= Time.deltaTime;
                if (_judgeTimer <= 0f && _judgeText != null)
                    _judgeText.text = "";
            }
        }

        public void UpdateScore(int s) { if (_scoreText != null) _scoreText.text = $"{s}"; }
        public void UpdateCombo(int c) { if (_comboText != null) _comboText.text = c > 0 ? $"x{c}" : ""; }
        public void UpdateMisses(int m, int max) { if (_missText != null) _missText.text = $"Miss: {m}/{max}"; }
        public void UpdateProgress(float ratio) { if (_progressSlider != null) _progressSlider.value = ratio; }
        public void ShowJudge(string text) { if (_judgeText != null) { _judgeText.text = text; _judgeTimer = 0.5f; } }
        public void ShowClear(int score, int maxCombo) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"スコア: {score}\n最大コンボ: {maxCombo}"; }
        public void ShowGameOver(int score) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"スコア: {score}"; }
    }
}
