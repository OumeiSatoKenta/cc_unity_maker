using UnityEngine;
using TMPro;

namespace Game006v2_ShadowMatch
{
    public class ShadowMatchUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _matchText;
        [SerializeField] private TextMeshProUGUI _judgeCountText;
        [SerializeField] private TextMeshProUGUI _hintCountText;

        [SerializeField] private GameObject _stageClearPanel;
        [SerializeField] private TextMeshProUGUI _stageClearStageText;
        [SerializeField] private TextMeshProUGUI _stageClearScoreText;
        [SerializeField] private TextMeshProUGUI _stageClearStarsText;

        [SerializeField] private GameObject _gameClearPanel;
        [SerializeField] private TextMeshProUGUI _gameClearScoreText;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString();
        }

        public void UpdateMatchRate(float rate)
        {
            if (_matchText != null) _matchText.text = $"一致度: {Mathf.RoundToInt(rate * 100)}%";
        }

        public void UpdateJudgeCount(int count)
        {
            if (_judgeCountText != null) _judgeCountText.text = $"判定: {count}回";
        }

        public void UpdateHintCount(int count)
        {
            if (_hintCountText != null) _hintCountText.text = $"ヒント残り: {count}回";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
        }

        public void ShowStageClearPanel(int stage, int score, int stars)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearStageText != null)
                _stageClearStageText.text = stage < 5 ? $"Stage {stage} クリア！" : "全ステージクリア！";
            if (_stageClearScoreText != null)
                _stageClearScoreText.text = $"スコア: {score}";
            if (_stageClearStarsText != null)
                _stageClearStarsText.text = stars == 3 ? "★★★" : stars == 2 ? "★★☆" : "★☆☆";
        }

        public void ShowClearPanel(int score)
        {
            if (_gameClearPanel == null) return;
            _gameClearPanel.SetActive(true);
            if (_gameClearScoreText != null) _gameClearScoreText.text = $"トータルスコア: {score}";
        }
    }
}
