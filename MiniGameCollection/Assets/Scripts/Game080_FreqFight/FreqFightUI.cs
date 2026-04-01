using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game080_FreqFight
{
    public class FreqFightUI : MonoBehaviour
    {
        [SerializeField, Tooltip("敵数")] private TextMeshProUGUI _enemyText;
        [SerializeField, Tooltip("タイマー")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("HP")] private TextMeshProUGUI _hpText;
        [SerializeField, Tooltip("周波数スライダー")] private Slider _freqSlider;
        [SerializeField, Tooltip("FreqManager")] private FreqManager _freqManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOスコア")] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        private void Update()
        {
            if (_freqSlider != null && _freqManager != null)
                _freqSlider.value = _freqManager.PlayerFreq;
        }

        public void UpdateEnemies(int defeated, int total) { if (_enemyText != null) _enemyText.text = $"撃破: {defeated}/{total}"; }
        public void UpdateTimer(float t) { if (_timerText != null) _timerText.text = $"{t:F1}s"; }
        public void UpdateHP(int hp) { if (_hpText != null) _hpText.text = $"HP: {hp}"; }
        public void ShowClear(int defeated, int hp) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{defeated}体撃破！ HP{hp}残り"; }
        public void ShowGameOver(int defeated) { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); if (_gameOverScoreText != null) _gameOverScoreText.text = $"{defeated}体撃破"; }
    }
}
