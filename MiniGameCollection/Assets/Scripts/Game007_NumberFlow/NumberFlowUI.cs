using UnityEngine;
using UnityEngine.UI;

namespace Game007_NumberFlow
{
    /// <summary>
    /// NumberFlow の UI を管理する。
    /// レベル・ステップ表示、クリアパネル表示を担当する。
    /// ボタン連携はSceneSetupのUnityEventで直接配線する。
    /// </summary>
    public class NumberFlowUI : MonoBehaviour
    {
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _stepText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private NumberFlowGameManager _gameManager;

        public void SetLevelText(string text)
        {
            if (_levelText != null) _levelText.text = text;
        }

        public void UpdateStep(int step, int max)
        {
            if (_stepText != null) _stepText.text = $"{step} / {max}";
        }

        public void ShowClearPanel(int level)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
        }

        public void ShowClearPanel()
        {
            ShowClearPanel(0);
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }
    }
}
