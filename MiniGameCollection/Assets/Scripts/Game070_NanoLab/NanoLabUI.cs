using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game070_NanoLab
{
    public class NanoLabUI : MonoBehaviour
    {
        [SerializeField, Tooltip("ナノボット数")] private TextMeshProUGUI _nanobotText;
        [SerializeField, Tooltip("技術テキスト")] private TextMeshProUGUI _techText;
        [SerializeField, Tooltip("増殖ボタン")] private Button _multiplierButton;
        [SerializeField, Tooltip("増殖ボタンテキスト")] private TextMeshProUGUI _multiplierButtonText;
        [SerializeField, Tooltip("研究ボタン")] private Button _researchButton;
        [SerializeField, Tooltip("研究ボタンテキスト")] private TextMeshProUGUI _researchButtonText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        public void UpdateNanobots(long n) { if (_nanobotText != null) _nanobotText.text = $"ナノ: {n}"; }
        public void UpdateTech(int unlocked, int total) { if (_techText != null) _techText.text = $"技術: {unlocked}/{total}"; }
        public void UpdateMultiplier(int lv, long cost) { if (_multiplierButtonText != null) _multiplierButtonText.text = $"増殖Lv{lv}\n{cost}"; }
        public void UpdateResearch(long cost) { if (_researchButtonText != null) _researchButtonText.text = $"研究\n{cost}"; }
        public void ShowClear(int tech) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{tech}技術解放！"; }
    }
}
