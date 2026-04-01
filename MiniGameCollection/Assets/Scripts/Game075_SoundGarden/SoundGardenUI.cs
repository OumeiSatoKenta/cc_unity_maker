using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game075_SoundGarden
{
    public class SoundGardenUI : MonoBehaviour
    {
        [SerializeField, Tooltip("コレクション")] private TextMeshProUGUI _collectionText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateCollection(int types, int total) { if (_collectionText != null) _collectionText.text = $"植物: {types}/{total}種"; }
        public void ShowClear(int types) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{types}種の庭が完成！"; }
    }
}
