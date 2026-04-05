using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game018v2_TimeRewind
{
    public class TimeRewindUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _stageText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _rewindCountText;
        [SerializeField] TMP_Text _moveCountText;
        [SerializeField] TMP_Text _bombCountdownText;
        [SerializeField] GameObject _timelinePanel;
        [SerializeField] Transform _timelineContent;
        [SerializeField] TMP_Text _stageClearScoreText;
        [SerializeField] TMP_Text _stageClearComboText;
        [SerializeField] TMP_Text _stageClearStarsText;
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] GameObject _clearPanel;
        [SerializeField] TMP_Text _clearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Image _flashOverlay;

        BoardManager _boardManager;

        void Awake()
        {
            _boardManager = FindFirstObjectByType<BoardManager>();
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateRewindCount(int remaining, int total)
        {
            if (_rewindCountText != null)
            {
                _rewindCountText.text = $"⏪ {remaining}/{total}";
                _rewindCountText.color = remaining <= 1 ? new Color(1f, 0.3f, 0.3f) : new Color(0.6f, 0.8f, 1f);
            }
        }

        public void UpdateMoveCount(int moves)
        {
            if (_moveCountText != null) _moveCountText.text = $"手数: {moves}";
        }

        public void UpdateBombCountdown(int count)
        {
            if (_bombCountdownText != null)
            {
                _bombCountdownText.gameObject.SetActive(true);
                _bombCountdownText.text = $"💣 {count}手";
                _bombCountdownText.color = count <= 2 ? Color.red : new Color(1f, 0.6f, 0f);
            }
        }

        public void HideBombCountdown()
        {
            if (_bombCountdownText != null) _bombCountdownText.gameObject.SetActive(false);
        }

        public void ShowTimelinePanel(int historyCount)
        {
            if (_timelinePanel == null) return;
            _timelinePanel.SetActive(true);

            // Clear existing entries
            if (_timelineContent != null)
            {
                foreach (Transform child in _timelineContent)
                    Destroy(child.gameObject);

                // Create buttons for each history entry
                for (int i = 0; i < historyCount; i++)
                {
                    int idx = i;
                    var entryObj = new GameObject($"Entry_{i}");
                    entryObj.transform.SetParent(_timelineContent, false);

                    var btn = entryObj.AddComponent<Button>();
                    var img = entryObj.AddComponent<Image>();
                    img.color = new Color(0.3f, 0.5f, 0.8f, 0.9f);

                    var rt = entryObj.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(80, 50);

                    var txtObj = new GameObject("Label");
                    txtObj.transform.SetParent(entryObj.transform, false);
                    var txt = txtObj.AddComponent<TextMeshProUGUI>();
                    txt.text = $"手番{i}";
                    txt.fontSize = 18;
                    txt.alignment = TextAlignmentOptions.Center;
                    var txtRt = txtObj.GetComponent<RectTransform>();
                    txtRt.anchorMin = Vector2.zero;
                    txtRt.anchorMax = Vector2.one;
                    txtRt.sizeDelta = Vector2.zero;

                    btn.onClick.AddListener(() => {
                        _boardManager?.RewindTo(idx);
                    });
                }
            }
        }

        public void HideTimelinePanel()
        {
            if (_timelinePanel != null) _timelinePanel.SetActive(false);
        }

        public void ShowRewindEffect()
        {
            StartCoroutine(DoRewindFlash());
        }

        IEnumerator DoRewindFlash()
        {
            if (_flashOverlay == null) yield break;
            _flashOverlay.gameObject.SetActive(true);
            _flashOverlay.color = new Color(0.5f, 0.8f, 1f, 0.7f);
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0.7f, 0f, t / 0.5f);
                _flashOverlay.color = new Color(0.5f, 0.8f, 1f, alpha);
                yield return null;
            }
            _flashOverlay.gameObject.SetActive(false);
        }

        public void ShowStageClearPanel(int score, int combo, int stars)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"+{score}pt";
            if (_stageClearComboText != null)
            {
                _stageClearComboText.gameObject.SetActive(combo >= 2);
                _stageClearComboText.text = $"COMBO ×{combo}!";
            }
            if (_stageClearStarsText != null)
            {
                string starStr = stars >= 3 ? "★★★" : stars >= 2 ? "★★☆" : "★☆☆";
                _stageClearStarsText.text = starStr;
            }
        }

        public void ShowClearPanel(int totalScore)
        {
            if (_clearPanel != null) _clearPanel.SetActive(true);
            if (_clearScoreText != null) _clearScoreText.text = $"Total Score: {totalScore}";
        }

        public void ShowGameOverPanel()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_clearPanel != null) _clearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_timelinePanel != null) _timelinePanel.SetActive(false);
            if (_flashOverlay != null) _flashOverlay.gameObject.SetActive(false);
            HideBombCountdown();
        }
    }
}
