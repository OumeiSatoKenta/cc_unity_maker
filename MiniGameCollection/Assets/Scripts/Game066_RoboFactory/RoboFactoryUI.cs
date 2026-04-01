using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game066_RoboFactory
{
    public class RoboFactoryUI : MonoBehaviour
    {
        [SerializeField, Tooltip("資源テキスト")] private TextMeshProUGUI _resourceText;
        [SerializeField, Tooltip("ロボット数")] private TextMeshProUGUI _robotText;
        [SerializeField, Tooltip("都市レベル")] private TextMeshProUGUI _cityLevelText;
        [SerializeField, Tooltip("ロボットボタン")] private Button _robotButton;
        [SerializeField, Tooltip("ロボットボタンテキスト")] private TextMeshProUGUI _robotButtonText;
        [SerializeField, Tooltip("建設ボタン")] private Button _buildButton;
        [SerializeField, Tooltip("建設ボタンテキスト")] private TextMeshProUGUI _buildButtonText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateResources(int r) { if (_resourceText != null) _resourceText.text = $"資源: {r}"; }
        public void UpdateRobots(int r) { if (_robotText != null) _robotText.text = $"ロボ: {r}体"; }
        public void UpdateCityLevel(int lv, int target) { if (_cityLevelText != null) _cityLevelText.text = $"都市Lv{lv}/{target}"; }
        public void UpdateCosts(int robotCost, int buildCost)
        {
            if (_robotButtonText != null) _robotButtonText.text = $"ロボ追加\n{robotCost}";
            if (_buildButtonText != null) _buildButtonText.text = $"建設\n{buildCost}";
        }
        public void ShowClear(int lv, int robots) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"都市Lv{lv} / {robots}体！"; }
    }
}
