using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game032v2_SpinCutter
{
    public class SpinCutterUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _launchText;
        [SerializeField] TextMeshProUGUI _enemyText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        [SerializeField] LineRenderer _orbitPreview;

        SpinCutterGameManager _gm;

        public void Initialize(SpinCutterGameManager gm)
        {
            _gm = gm;
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateLaunches(int remaining)
        {
            if (_launchText != null) _launchText.text = $"発射: {remaining}";
        }

        public void UpdateEnemies(int remaining)
        {
            if (_enemyText != null) _enemyText.text = $"敵: {remaining}";
        }

        public void UpdateOrbitPreview(Vector3 center, float radius)
        {
            if (_orbitPreview == null) return;
            int segments = 64;
            _orbitPreview.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * (360f / segments) * Mathf.Deg2Rad;
                _orbitPreview.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
        }

        public void ShowStageClear(int stage, int bonus)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearText != null)
                _stageClearText.text = $"ステージ {stage} クリア！\nボーナス: +{bonus}pt";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowFinalClear(int score)
        {
            if (_finalClearPanel != null)
            {
                _finalClearPanel.SetActive(true);
                if (_finalScoreText != null)
                    _finalScoreText.text = $"全ステージクリア！\nスコア: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"GAME OVER\nスコア: {score}";
            }
        }

        public void OnLaunchButton()
        {
            if (_gm != null) _gm.OnLaunchPressed();
        }

        public void OnNextStageButton()
        {
            if (_gm != null) _gm.OnNextStagePressed();
        }

        public void OnRestartButton()
        {
            if (_gm != null) _gm.RestartGame();
        }

        public void OnMenuButton()
        {
            if (_gm != null) _gm.ReturnToMenu();
        }
    }
}
