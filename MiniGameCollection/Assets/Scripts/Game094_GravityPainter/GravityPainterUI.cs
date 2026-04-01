using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game094_GravityPainter
{
    public class GravityPainterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _matchText;
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TextMeshProUGUI _clearScoreText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverScoreText;

        public void UpdateMatch(float rate)
        { if (_matchText) _matchText.text = $"一致率: {(int)(rate * 100)}%"; }

        public void UpdateMoves(int remaining)
        { if (_movesText) _movesText.text = $"残り {remaining} 回"; }

        public void ShowClear(float rate, int stars)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            string starStr = stars == 3 ? "★★★" : stars == 2 ? "★★☆" : "★☆☆";
            if (_clearScoreText) _clearScoreText.text = $"{starStr}  {(int)(rate * 100)}%";
        }

        public void ShowGameOver(float rate)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"一致率: {(int)(rate * 100)}%";
        }
    }
}
