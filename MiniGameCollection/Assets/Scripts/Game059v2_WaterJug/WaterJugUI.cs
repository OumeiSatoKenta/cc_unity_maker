using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game059v2_WaterJug
{
    public class WaterJugUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _movesText;
        [SerializeField] TextMeshProUGUI _targetInfoText;
        [SerializeField] TextMeshProUGUI _modeText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;

        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        WaterJugGameManager _gm;

        public void Init(WaterJugGameManager gm)
        {
            _gm = gm;
            HideAllPanels();
        }

        public void OnStageChanged(int stageNum, int maxMoves, int[][] targetAmounts, int[][] targetIndices)
        {
            // targetAmounts/targetIndices are stage-based; simpler: just show target info
            if (_stageText != null)
                _stageText.text = $"Stage {stageNum} / 5";
            if (_movesText != null)
                _movesText.text = $"手数: 0 / {maxMoves}";
            if (_scoreText != null)
                _scoreText.text = "Score: 0";
            if (_modeText != null)
                _modeText.text = "";
            HideAllPanels();
        }

        // Overload for simpler call
        public void OnStageChanged(int stageNum, int maxMoves, int[] targetAmts, int[] targetJugIndices)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {stageNum} / 5";
            if (_movesText != null)
                _movesText.text = $"手数: 0 / {maxMoves}";
            if (_scoreText != null)
                _scoreText.text = "Score: 0";

            // Build target info string
            if (_targetInfoText != null)
            {
                if (targetAmts.Length == 1)
                    _targetInfoText.text = $"目標: {targetAmts[0]}L";
                else if (targetAmts.Length == 2)
                    _targetInfoText.text = $"目標: {targetAmts[0]}L と {targetAmts[1]}L";
            }

            if (_modeText != null)
                _modeText.text = "";
            HideAllPanels();
        }

        public void UpdateMoves(int current, int max)
        {
            if (_movesText != null)
                _movesText.text = $"手数: {current} / {max}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void SetInputMode(string mode)
        {
            if (_modeText == null) return;
            switch (mode)
            {
                case "faucet": _modeText.text = "▼ 蛇口モード：注ぐジャグをタップ"; _modeText.color = new Color(0.3f, 0.7f, 1f); break;
                case "drain": _modeText.text = "▲ 排水モード：空にするジャグをタップ"; _modeText.color = new Color(1f, 0.5f, 0.2f); break;
                default: _modeText.text = ""; break;
            }
        }

        public void ShowStageClear(int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
            }
        }

        public void ShowGameClear(int totalScore)
        {
            if (_gameClearPanel != null)
            {
                _gameClearPanel.SetActive(true);
                if (_gameClearScoreText != null) _gameClearScoreText.text = $"Total Score: {totalScore}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
            }
        }

        void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }
    }
}
