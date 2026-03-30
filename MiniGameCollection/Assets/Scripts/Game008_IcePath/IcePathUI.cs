using UnityEngine;
using UnityEngine.UI;

namespace Game008_IcePath
{
    /// <summary>
    /// IcePath の UI 表示管理。
    /// レベル表示、進捗表示、クリアパネル制御を担当。
    /// </summary>
    public class IcePathUI : MonoBehaviour
    {
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _progressText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private IcePathGameManager _gameManager;

        public void SetLevelText(string text)
        {
            if (_levelText != null) _levelText.text = text;
        }

        public void UpdateProgress(int visited, int total)
        {
            if (_progressText != null)
                _progressText.text = $"{visited} / {total}";
        }

        public void ShowClearPanel(int level)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }
    }
}
