using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game012v2_BridgeBuilder
{
    public class BridgeBuilderUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _budgetText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _feedbackText;

        [SerializeField] Transform _partButtonContainer;
        [SerializeField] Button _testButton;
        [SerializeField] Button _retryButton;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearStarsText;
        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        List<Button> _partButtons = new List<Button>();

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateBudget(int remaining, int total)
        {
            if (_budgetText) _budgetText.text = $"予算: ${remaining}";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText)
            {
                _comboText.text = combo >= 2 ? $"Combo x{combo}!" : "";
            }
        }

        public void SetupPartButtons(List<BridgeManager.PartType> available, System.Action<BridgeManager.PartType> onSelect)
        {
            if (_partButtonContainer == null) return;

            // Clear existing
            foreach (var b in _partButtons) if (b) Destroy(b.gameObject);
            _partButtons.Clear();

            // The buttons are pre-created children; just activate/wire them
            var children = new List<Transform>();
            for (int i = 0; i < _partButtonContainer.childCount; i++)
                children.Add(_partButtonContainer.GetChild(i));

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                bool active = i < available.Count;
                child.gameObject.SetActive(active);
                if (!active) continue;

                var partType = available[i];
                var btn = child.GetComponent<Button>();
                if (btn == null) continue;

                // Clear and re-add listeners
                btn.onClick.RemoveAllListeners();
                var capturedType = partType;
                btn.onClick.AddListener(() => {
                    onSelect(capturedType);
                    HighlightPartButton(btn);
                });
                _partButtons.Add(btn);

                var label = child.GetComponentInChildren<TextMeshProUGUI>();
                if (label)
                {
                    string[] names = { "木材\n$50", "鉄骨\n$150", "ロープ\n$80" };
                    label.text = names[i];
                }
            }

            // Select first by default
            if (_partButtons.Count > 0) HighlightPartButton(_partButtons[0]);
        }

        void HighlightPartButton(Button selected)
        {
            foreach (var b in _partButtons)
            {
                if (b == null) continue;
                var img = b.GetComponent<Image>();
                if (img) img.color = b == selected ? new Color(0.3f, 0.7f, 1f) : new Color(0.2f, 0.2f, 0.3f);
            }
        }

        public void ShowStageClearPanel(bool show, int score = 0, int stars = 1)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(show);
            if (show)
            {
                if (_stageClearScoreText) _stageClearScoreText.text = $"+{score}";
                if (_stageClearStarsText)
                {
                    _stageClearStarsText.text = stars == 3 ? "★★★" : stars == 2 ? "★★☆" : "★☆☆";
                    _stageClearStarsText.color = stars == 3 ? new Color(1f, 0.85f, 0.1f) : stars == 2 ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.7f, 0.6f, 0.4f);
                }
            }
        }

        public void ShowGameClearPanel(int totalScore)
        {
            if (_gameClearPanel) _gameClearPanel.SetActive(true);
            if (_gameClearScoreText) _gameClearScoreText.text = $"Total: {totalScore}";
        }

        public void ShowTestFailedFeedback()
        {
            if (_feedbackText) StartCoroutine(ShowTemporaryText("橋が崩壊！設計を見直して", 2f));
        }

        public void ShowBudgetWarning()
        {
            if (_feedbackText) StartCoroutine(ShowTemporaryText("予算が足りません！", 1.5f));
        }

        IEnumerator ShowTemporaryText(string msg, float duration)
        {
            if (_feedbackText == null) yield break;
            _feedbackText.text = msg;
            _feedbackText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            _feedbackText.gameObject.SetActive(false);
        }
    }
}
