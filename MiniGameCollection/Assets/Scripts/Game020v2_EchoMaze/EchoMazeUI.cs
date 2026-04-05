using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game020v2_EchoMaze
{
    public class EchoMazeUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _moveLimitText;
        [SerializeField] TextMeshProUGUI _floorIndicatorText;

        // Echo indicator bars (fill images for 4 directions)
        [SerializeField] Image _echoNorthBar;
        [SerializeField] Image _echoSouthBar;
        [SerializeField] Image _echoWestBar;
        [SerializeField] Image _echoEastBar;
        [SerializeField] TextMeshProUGUI _echoDisturbText;

        [SerializeField] GameObject _mapPanel;
        [SerializeField] TextMeshProUGUI _mapText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] TextMeshProUGUI _stageClearBonusText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _gameOverMenuButton;

        [SerializeField] GameObject _clearPanel;
        [SerializeField] TextMeshProUGUI _clearScoreText;
        [SerializeField] Button _clearMenuButton;

        int _score;
        bool _mapShown;

        void Start()
        {
            _stageClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _clearPanel?.SetActive(false);
            _mapPanel?.SetActive(false);
            _mapShown = false;
        }

        public void UpdateStageDisplay(int stage, int totalStages)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {totalStages}";
            _stageClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _clearPanel?.SetActive(false);
            _mapPanel?.SetActive(false);
            _mapShown = false;
        }

        public void UpdateScore(int score)
        {
            _score = score;
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void SetMoveLimit(int limit, int remaining)
        {
            if (_moveLimitText != null)
            {
                _moveLimitText.text = $"残り {remaining}/{limit}";
                _moveLimitText.color = remaining <= limit / 4 ? new Color(1f, 0.4f, 0.4f) : new Color(0.7f, 0.9f, 1f);
            }
        }

        public void UpdateEchoIndicator(float north, float south, float west, float east, FloorType floorType, bool isDisturbed)
        {
            Color barColor = GetFloorBarColor(floorType);
            if (_echoNorthBar != null) { _echoNorthBar.fillAmount = north; _echoNorthBar.color = barColor; }
            if (_echoSouthBar != null) { _echoSouthBar.fillAmount = south; _echoSouthBar.color = barColor; }
            if (_echoWestBar != null)  { _echoWestBar.fillAmount = west;  _echoWestBar.color = barColor; }
            if (_echoEastBar != null)  { _echoEastBar.fillAmount = east;  _echoEastBar.color = barColor; }
            if (_echoDisturbText != null) _echoDisturbText.gameObject.SetActive(isDisturbed);
        }

        Color GetFloorBarColor(FloorType floorType)
        {
            switch (floorType)
            {
                case FloorType.Stone: return new Color(0.4f, 0.6f, 1f);   // blue
                case FloorType.Wood:  return new Color(0.4f, 0.9f, 0.5f); // green
                case FloorType.Water: return new Color(1f, 0.4f, 0.4f);   // red
                default: return new Color(0.6f, 0.8f, 1f);
            }
        }

        public void ShowFloorIndicator(bool isSecondFloor)
        {
            if (_floorIndicatorText != null)
                _floorIndicatorText.text = isSecondFloor ? "2階" : "1階";
        }

        public void ToggleMapPanel(bool[,] visited, int gridSize, Vector2Int playerPos, Vector2Int goalPos)
        {
            if (_mapPanel == null) return;
            _mapShown = !_mapShown;
            _mapPanel.SetActive(_mapShown);

            if (_mapShown && _mapText != null)
            {
                var sb = new System.Text.StringBuilder();
                for (int y = 0; y < gridSize; y++)
                {
                    for (int x = 0; x < gridSize; x++)
                    {
                        if (x == playerPos.x && y == playerPos.y) sb.Append("P");
                        else if (x == goalPos.x && y == goalPos.y) sb.Append("G");
                        else if (visited[x, y]) sb.Append(".");
                        else sb.Append(" ");
                    }
                    sb.AppendLine();
                }
                _mapText.text = sb.ToString();
            }
        }

        public void ShowStageClearPanel(int stageScore, int combo, bool echoBonus, bool mapBonus)
        {
            _stageClearPanel?.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"+{stageScore}pt";
            if (_stageClearBonusText != null)
            {
                string bonuses = "";
                if (echoBonus) bonuses += "エコー未使用 ×1.5　";
                if (mapBonus)  bonuses += "マップ未確認 ×2.0";
                if (combo >= 3) bonuses += $"コンボ×{combo} ×1.5";
                _stageClearBonusText.text = bonuses;
            }
        }

        public void ShowGameOverPanel()
        {
            _gameOverPanel.SetActive(true);
        }

        public void ShowClearPanel(int totalScore)
        {
            _clearPanel.SetActive(true);
            if (_clearScoreText != null) _clearScoreText.text = $"Total: {totalScore}";
        }
    }
}
