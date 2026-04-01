using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game087_DreamCatcher
{
    public class DreamCatcherUI : MonoBehaviour
    {
        [SerializeField, Tooltip("コレクション")] private TextMeshProUGUI _collectionText;
        [SerializeField, Tooltip("破片数")] private TextMeshProUGUI _fragmentText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateCollection(int types, int total) { if (_collectionText != null) _collectionText.text = $"夢: {types}/{total}種"; }
        public void UpdateFragments(int f) { if (_fragmentText != null) _fragmentText.text = $"破片: {f}"; }
        public void ShowClear(int types, int fragments) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{types}種 / {fragments}個収集！"; }
    }
}
