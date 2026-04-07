using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game084v2_GardenZen
{
    public class GardenZenUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _matchRateText;
        [SerializeField] TextMeshProUGUI _messageText;

        // Palette buttons
        [SerializeField] Button _stoneBtn;
        [SerializeField] Button _plantBtn;
        [SerializeField] Button _decoBtn;
        [SerializeField] Button _sandBtn;
        [SerializeField] Button _eraserBtn;

        [SerializeField] Button _submitBtn;
        [SerializeField] Button _resetBtn;

        // Panels
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] TextMeshProUGUI _stageClearStarsText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GardenManager _gardenManager;

        void Start()
        {
            _stoneBtn?.onClick.AddListener(() => SelectType(PlacementType.Stone1));
            _plantBtn?.onClick.AddListener(() => SelectType(PlacementType.Plant1));
            _decoBtn?.onClick.AddListener(() => SelectType(PlacementType.Decoration));
            _sandBtn?.onClick.AddListener(() => SelectType(PlacementType.Sand));
            _eraserBtn?.onClick.AddListener(() => SelectType(PlacementType.None));
            _submitBtn?.onClick.AddListener(() => _gardenManager?.SubmitGarden());
            _resetBtn?.onClick.AddListener(() => _gardenManager?.ResetCurrentStage());

            if (_messageText != null) _messageText.gameObject.SetActive(false);
        }

        void SelectType(PlacementType type)
        {
            if (_gardenManager != null)
                _gardenManager.SelectedType = type;
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMatchRate(float rate)
        {
            if (_matchRateText != null)
                _matchRateText.text = $"一致度: {Mathf.RoundToInt(rate * 100f)}%";
        }

        public void ShowRetryMessage(float rate)
        {
            if (_messageText == null) return;
            _messageText.gameObject.SetActive(true);
            int pct = Mathf.RoundToInt(rate * 100f);
            _messageText.text = $"一致度 {pct}% - もう少し！もう一度試してみよう";
        }

        public void HideMessage()
        {
            if (_messageText != null) _messageText.gameObject.SetActive(false);
        }

        public void ShowStageClear(int stage, int stars)
        {
            stars = Mathf.Clamp(stars, 0, 3);
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearText != null) _stageClearText.text = $"ステージ {stage} クリア！";
            if (_stageClearStarsText != null)
                _stageClearStarsText.text = new string('★', stars) + new string('☆', 3 - stars);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            HideMessage();
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"Final Score: {score}";
        }
    }
}
