using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game046_SqueezePop
{
    public class SqueezePopUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _squeezeText;
        [SerializeField] private TextMeshProUGUI _fillText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearText;
        [SerializeField] private SqueezePopGameManager _gameManager;

        public void UpdateSqueezes(int count)
        {
            if (_squeezeText != null) _squeezeText.text = "タップ: " + count;
        }

        public void UpdateFillPercent(float percent)
        {
            if (_fillText != null) _fillText.text = Mathf.RoundToInt(percent * 100f) + "%";
        }

        public void ShowClearPanel(int squeezes)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearText != null) _clearText.text = squeezes + " タップでクリア!";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        public void OnRetryButton()
        {
            if (_gameManager != null) _gameManager.StartGame();
        }
    }
}
