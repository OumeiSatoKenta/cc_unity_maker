using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game010v2_GearSync
{
    public class GearSyncUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _stageText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _testCountText;
        [SerializeField] TMP_Text _comboText;
        [SerializeField] TMP_Text _partsSmallText;
        [SerializeField] TMP_Text _partsLargeText;
        [SerializeField] TMP_Text _partsBeltText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TMP_Text _stageClearScoreText;
        [SerializeField] TMP_Text _stageClearStarsText;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TMP_Text _gameClearScoreText;

        [SerializeField] Button _smallGearButton;
        [SerializeField] Button _largeGearButton;
        [SerializeField] Button _beltButton;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateTestCount(int count)
        {
            if (_testCountText) _testCountText.text = $"テスト: {count}回";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText)
            {
                _comboText.text = combo >= 2 ? $"Combo x{combo}" : "";
            }
        }

        public void UpdateParts(int small, int large, int belt)
        {
            if (_partsSmallText) _partsSmallText.text = $"×{small}";
            if (_partsLargeText) _partsLargeText.text = $"×{large}";
            if (_partsBeltText) _partsBeltText.text = $"×{belt}";

            if (_smallGearButton) _smallGearButton.interactable = small > 0;
            if (_largeGearButton) _largeGearButton.interactable = large > 0;
            if (_beltButton) _beltButton.interactable = belt > 0;
        }

        public void UpdateSelection(GearType selected)
        {
            if (_smallGearButton)
            {
                var colors = _smallGearButton.colors;
                colors.normalColor = selected == GearType.SmallGear ? new Color(0.3f, 0.8f, 1f) : Color.white;
                _smallGearButton.colors = colors;
            }
            if (_largeGearButton)
            {
                var colors = _largeGearButton.colors;
                colors.normalColor = selected == GearType.LargeGear ? new Color(0.3f, 0.8f, 1f) : Color.white;
                _largeGearButton.colors = colors;
            }
            if (_beltButton)
            {
                var colors = _beltButton.colors;
                colors.normalColor = selected == GearType.Belt ? new Color(0.3f, 0.8f, 1f) : Color.white;
                _beltButton.colors = colors;
            }
        }

        public void ShowStageClearPanel(bool show, int score = 0, int stars = 0)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(show);
            if (show)
            {
                if (_stageClearScoreText) _stageClearScoreText.text = $"+{score}";
                if (_stageClearStarsText) _stageClearStarsText.text = new string('★', stars) + new string('☆', 3 - stars);
            }
        }

        public void ShowGameClearPanel(int totalScore)
        {
            if (_gameClearPanel) _gameClearPanel.SetActive(true);
            if (_gameClearScoreText) _gameClearScoreText.text = $"Total: {totalScore}";
        }
    }
}
