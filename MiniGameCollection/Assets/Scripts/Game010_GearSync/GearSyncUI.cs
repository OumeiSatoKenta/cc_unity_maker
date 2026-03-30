using UnityEngine;
using UnityEngine.UI;

namespace Game010_GearSync
{
    /// <summary>
    /// GearSync のUI表示を担当する。
    /// テキスト更新・クリアパネルの表示/非表示を管理する。
    /// </summary>
    public class GearSyncUI : MonoBehaviour
    {
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _rotationText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private Text _clearRotationText;
        [SerializeField] private GearSyncGameManager _gameManager;

        public void UpdateLevelText(int current, int total)
        {
            if (_levelText != null)
                _levelText.text = $"Level {current} / {total}";
        }

        public void UpdateRotationText(int count)
        {
            if (_rotationText != null)
                _rotationText.text = $"Rotations: {count}";
        }

        public void ShowClearPanel(int rotationCount)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearRotationText != null)
                _clearRotationText.text = $"Rotations: {rotationCount}";
        }

        public void HideClearPanel()
        {
            if (_clearPanel != null) _clearPanel.SetActive(false);
        }

        public void OnRestartClicked()
        {
            if (_gameManager != null) _gameManager.OnRestart();
        }

        public void OnNextClicked()
        {
            if (_gameManager != null) _gameManager.OnNextLevel();
        }

        public void OnMenuClicked()
        {
            if (_gameManager != null) _gameManager.LoadMenu();
        }
    }
}
