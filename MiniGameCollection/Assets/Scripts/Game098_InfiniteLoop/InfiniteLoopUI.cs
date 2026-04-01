using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game098_InfiniteLoop
{
    public class InfiniteLoopUI : MonoBehaviour
    {
        [SerializeField, Tooltip("ループカウントテキスト")] private TextMeshProUGUI _loopCountText;
        [SerializeField, Tooltip("ステージテキスト")] private TextMeshProUGUI _stageText;
        [SerializeField, Tooltip("ヒントテキスト")] private TextMeshProUGUI _hintText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアテキスト")] private TextMeshProUGUI _clearText;

        private float _hintTimer;

        public void UpdateLoopCount(int count)
        {
            if (_loopCountText) _loopCountText.text = $"Loop #{count}";
        }

        public void UpdateStage(int found, int total)
        {
            if (_stageText) _stageText.text = $"変化 {found}/{total} 発見";
        }

        public void ShowHint(string text)
        {
            if (_hintText)
            {
                _hintText.text = text;
                _hintText.gameObject.SetActive(true);
                _hintTimer = 2f;
            }
        }

        public void ShowClearPanel(int loopCount)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearText) _clearText.text = $"ループ脱出！\n\n{loopCount} ループで脱出";
        }

        public void HideClearPanel()
        {
            if (_clearPanel) _clearPanel.SetActive(false);
            if (_hintText) _hintText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_hintText != null && _hintText.gameObject.activeSelf && _hintTimer > 0f)
            {
                _hintTimer -= Time.deltaTime;
                if (_hintTimer <= 0f)
                    _hintText.gameObject.SetActive(false);
            }
        }
    }
}
