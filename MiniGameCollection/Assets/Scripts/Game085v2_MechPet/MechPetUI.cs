using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game085v2_MechPet
{
    public class MechPetUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _synergyText;
        [SerializeField] TextMeshProUGUI _messageText;
        [SerializeField] Slider _energySlider;

        [SerializeField] Button[] _slotButtons;
        [SerializeField] TextMeshProUGUI[] _slotTexts;

        [SerializeField] Button _missionButton;
        [SerializeField] Button _chargeButton;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;

        [SerializeField] MechPetManager _mechPetManager;

        Coroutine _hideCoroutine;

        void Awake()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_messageText != null) _messageText.gameObject.SetActive(false);
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo >= 2)
                _comboText.text = $"Combo x{combo}  ×{multiplier:F1}";
            else
                _comboText.text = combo == 1 ? "Combo x1" : "";
        }

        public void UpdateSynergy(string text)
        {
            if (_synergyText != null) _synergyText.text = text;
        }

        public void UpdateEnergy(float normalized)
        {
            if (_energySlider != null) _energySlider.value = normalized;
        }

        public void UpdateSlotLabel(int slotIndex, string slotName, string partName)
        {
            if (_slotTexts == null || slotIndex >= _slotTexts.Length) return;
            if (_slotTexts[slotIndex] != null)
                _slotTexts[slotIndex].text = $"{slotName}\n{partName}";
        }

        public void ShowMessage(string msg)
        {
            if (_messageText == null) return;
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            _messageText.text = msg;
            _messageText.gameObject.SetActive(true);
            _hideCoroutine = StartCoroutine(HideMessageAfter(1.5f));
        }

        public void ShowMissionFailed()
        {
            ShowMessage("ミッション失敗！");
        }

        IEnumerator HideMessageAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_messageText != null) _messageText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int stage)
        {
            if (_stageClearPanel == null) return;
            if (_stageClearScoreText != null)
                _stageClearScoreText.text = $"ステージ {stage} クリア！";
            _stageClearPanel.SetActive(true);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int totalScore)
        {
            if (_allClearPanel == null) return;
            if (_allClearScoreText != null)
                _allClearScoreText.text = $"Final Score: {totalScore}";
            _allClearPanel.SetActive(true);
        }

        public void ShowGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }
    }
}
