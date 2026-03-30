using UnityEngine;
using UnityEngine.UI;

namespace Game006_ShadowMatch
{
    public class ShadowMatchUI : MonoBehaviour
    {
        [SerializeField] private Text _levelText;
        [SerializeField] private Image _matchFill;
        [SerializeField] private Text _matchText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private ShadowMatchGameManager _gameManager;

        private void Start()
        {
            _gameManager.OnLevelCleared.AddListener(ShowClearPanel);
            if (_clearPanel) _clearPanel.SetActive(false);
        }

        public void SetLevelText(string text)
        {
            if (_levelText) _levelText.text = text;
        }

        public void UpdateMatch(float match)
        {
            if (_matchFill) _matchFill.fillAmount = match;
            if (_matchText) _matchText.text = $"{Mathf.RoundToInt(match * 100)}%";
        }

        public void ShowClearPanel(int level)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
        }

        public void HideClearPanel()
        {
            if (_clearPanel) _clearPanel.SetActive(false);
        }

        public void OnNextLevelClicked() => _gameManager?.LoadNextLevel();
        public void OnMenuClicked() => _gameManager?.LoadMenu();
    }
}
