using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game097_PixelEvolution
{
    public class PixelEvolutionUI : MonoBehaviour
    {
        [SerializeField, Tooltip("世代テキスト")] private TextMeshProUGUI _generationText;
        [SerializeField, Tooltip("生命体名テキスト")] private TextMeshProUGUI _creatureNameText;
        [SerializeField, Tooltip("世代交代ボタン")] private Button _evolveButton;
        [SerializeField, Tooltip("分岐パネル")] private GameObject _branchPanel;
        [SerializeField, Tooltip("選択肢Aボタン")] private Button _branchOptionAButton;
        [SerializeField, Tooltip("選択肢Bボタン")] private Button _branchOptionBButton;
        [SerializeField, Tooltip("選択肢Aテキスト")] private TextMeshProUGUI _branchOptionAText;
        [SerializeField, Tooltip("選択肢Bテキスト")] private TextMeshProUGUI _branchOptionBText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアテキスト")] private TextMeshProUGUI _clearText;
        [SerializeField, Tooltip("クリアリトライボタン")] private Button _clearRetryButton;

        public void UpdateGeneration(int gen, int max)
        {
            if (_generationText) _generationText.text = $"世代 {gen} / {max}";
        }

        public void UpdateCreatureName(string name)
        {
            if (_creatureNameText) _creatureNameText.text = name;
        }

        public void ShowEvolveButton(bool show)
        {
            if (_evolveButton) _evolveButton.gameObject.SetActive(show);
        }

        public void ShowBranchPanel(string optionA, string optionB)
        {
            if (_branchPanel) _branchPanel.SetActive(true);
            if (_branchOptionAText) _branchOptionAText.text = optionA;
            if (_branchOptionBText) _branchOptionBText.text = optionB;
        }

        public void HideBranchPanel()
        {
            if (_branchPanel) _branchPanel.SetActive(false);
        }

        public void ShowClearPanel(string finalName, int generation)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearText) _clearText.text = $"進化完了！\n\n最終形態:\n{finalName}\n\n{generation}世代で到達";
        }

        public void HideClearPanel()
        {
            if (_clearPanel) _clearPanel.SetActive(false);
        }
    }
}
