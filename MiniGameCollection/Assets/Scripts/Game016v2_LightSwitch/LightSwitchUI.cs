using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game016v2_LightSwitch
{
    public class LightSwitchUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _moveText;
        [SerializeField] TextMeshProUGUI _undoText;
        [SerializeField] TextMeshProUGUI _comboText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearComboText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearTotalScoreText;

        [SerializeField] LightSwitchGameManager _gameManager;
        [SerializeField] BulbManager _bulbManager;

        // Target pattern display (small icons in corner)
        [SerializeField] Transform _targetPatternRoot;
        [SerializeField] Sprite _spTargetOn;
        [SerializeField] Sprite _spTargetOff;

        Image[] _targetIcons;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMoves(int remaining, int max)
        {
            if (_moveText) _moveText.text = $"手数: {remaining}/{max}";
            if (_moveText)
                _moveText.color = remaining <= 3 ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.85f, 0.3f);
        }

        public void UpdateUndo(int remaining)
        {
            if (_undoText) _undoText.text = $"Undo: {remaining}";
            if (_undoText)
                _undoText.color = remaining <= 0 ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.4f, 0.8f, 1f);
        }

        public void UpdateTargetPattern(bool[] pattern, int gridSize)
        {
            if (_targetPatternRoot == null) return;

            // Destroy old icons
            foreach (Transform child in _targetPatternRoot)
                Destroy(child.gameObject);

            _targetIcons = new Image[pattern.Length];
            float iconSize = Mathf.Min(60f / gridSize, 18f);
            float spacing = iconSize + 2f;
            float originX = -(gridSize - 1) * spacing * 0.5f;
            float originY = (gridSize - 1) * spacing * 0.5f;

            for (int i = 0; i < pattern.Length; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;

                var go = new GameObject($"TIcon_{row}_{col}");
                go.transform.SetParent(_targetPatternRoot, false);
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(iconSize, iconSize);
                rt.anchoredPosition = new Vector2(originX + col * spacing, originY - row * spacing);

                var img = go.AddComponent<Image>();
                img.sprite = pattern[i] ? _spTargetOn : _spTargetOff;
                _targetIcons[i] = img;
            }
        }

        public void ShowStageClearPanel(int stageScore, int combo)
        {
            HideAllPanels();
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText) _stageClearScoreText.text = $"+{stageScore}";
            if (_stageClearComboText)
            {
                _stageClearComboText.text = combo >= 2 ? $"コンボ x{combo}！" : "";
                _stageClearComboText.color = combo >= 5 ? new Color(1f, 0.4f, 0.1f) : new Color(1f, 0.6f, 0.2f);
            }
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
        }

        public void ShowClearPanel(int totalScore)
        {
            HideAllPanels();
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearTotalScoreText) _clearTotalScoreText.text = $"Total: {totalScore}";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
            if (_clearPanel) _clearPanel.SetActive(false);
            if (_comboText) _comboText.text = "";
        }

        // Button callbacks
        public void OnUndoPressed()
        {
            if (_bulbManager) _bulbManager.UndoMove();
        }
    }
}
