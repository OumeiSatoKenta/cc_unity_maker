using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game061_CookieFactory
{
    public class CookieFactoryUI : MonoBehaviour
    {
        [SerializeField, Tooltip("クッキー数")] private TextMeshProUGUI _cookieText;
        [SerializeField, Tooltip("売上")] private TextMeshProUGUI _salesText;
        [SerializeField, Tooltip("オーブンボタン")] private Button _ovenButton;
        [SerializeField, Tooltip("オーブンテキスト")] private TextMeshProUGUI _ovenButtonText;
        [SerializeField, Tooltip("コンベアボタン")] private Button _conveyorButton;
        [SerializeField, Tooltip("コンベアテキスト")] private TextMeshProUGUI _conveyorButtonText;
        [SerializeField, Tooltip("FactoryManager")] private FactoryManager _factoryManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        private void Update()
        {
            if (_factoryManager == null) return;
            if (_ovenButtonText != null)
                _ovenButtonText.text = $"オーブンLv{_factoryManager.OvenLv}\n{_factoryManager.NextOvenCost}枚";
            if (_conveyorButtonText != null)
                _conveyorButtonText.text = $"コンベアLv{_factoryManager.ConveyorLv}\n{_factoryManager.NextConveyorCost}枚";
        }

        public void UpdateCookies(int c) { if (_cookieText != null) _cookieText.text = $"クッキー: {c}"; }
        public void UpdateSales(int s, int target) { if (_salesText != null) _salesText.text = $"売上: {s}/{target}"; }
        public void ShowClear(int sales) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"売上{sales}達成！"; }
    }
}
