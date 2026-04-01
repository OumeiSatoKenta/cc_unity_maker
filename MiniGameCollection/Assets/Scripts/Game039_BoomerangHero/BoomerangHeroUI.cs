using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game039_BoomerangHero
{
    public class BoomerangHeroUI : MonoBehaviour
    {
        [SerializeField, Tooltip("撃破数テキスト")] private TextMeshProUGUI _killsText;
        [SerializeField, Tooltip("残り投擲テキスト")] private TextMeshProUGUI _throwsText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリア★テキスト")] private TextMeshProUGUI _clearStarText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("GOパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("GOリトライ")] private Button _gameOverRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateKills(int k, int t) { if (_killsText != null) _killsText.text = $"撃破: {k}/{t}"; }
        public void UpdateThrows(int r) { if (_throwsText != null) _throwsText.text = $"残り: {r}"; }
        public void ShowClear(int stars) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearStarText != null) _clearStarText.text = new string('\u2605', stars) + new string('\u2606', 3 - stars); }
        public void ShowGameOver() { if (_gameOverPanel != null) _gameOverPanel.SetActive(true); }
    }
}
