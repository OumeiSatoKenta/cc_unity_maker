using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game035v2_WaveRider
{
    public class WaveRiderUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _distanceText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] Image _shieldIcon;

        [Header("Panels")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;

        [Header("Menu")]
        [SerializeField] Button _menuButton;

        WaveRiderGameManager _manager;
        Coroutine _comboAnimCo;

        public void Initialize(WaveRiderGameManager manager)
        {
            _manager = manager;
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
            if (_shieldIcon != null) _shieldIcon.gameObject.SetActive(false);

            if (_nextStageButton != null)
            {
                _nextStageButton.onClick.RemoveAllListeners();
                _nextStageButton.onClick.AddListener(() => manager.AdvanceToNextStage());
            }
            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveAllListeners();
                _retryButton.onClick.AddListener(() => manager.RetryGame());
            }
            if (_menuButton != null)
            {
                _menuButton.onClick.RemoveAllListeners();
                _menuButton.onClick.AddListener(() => SceneLoader.BackToMenu());
            }
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null)
                _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        public void UpdateDistance(float traveled, float goal)
        {
            if (_distanceText != null)
            {
                float remaining = Mathf.Max(0f, goal - traveled);
                _distanceText.text = $"残り {Mathf.CeilToInt(remaining)}m";
            }
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 1)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"x{combo} COMBO!";

            if (_comboAnimCo != null) StopCoroutine(_comboAnimCo);
            _comboAnimCo = StartCoroutine(ComboBounce());
        }

        public void ShowShield(bool active)
        {
            if (_shieldIcon != null)
                _shieldIcon.gameObject.SetActive(active);
        }

        public void ShowStageClear(int currentStage, int totalStages)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearText != null)
                _stageClearText.text = $"ステージ {currentStage} クリア！";
            if (_nextStageButton != null)
                _nextStageButton.gameObject.SetActive(currentStage < totalStages);
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowFinalClear(int score)
        {
            if (_finalClearPanel != null) _finalClearPanel.SetActive(true);
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_finalScoreText != null)
                _finalScoreText.text = $"全ステージクリア！\nスコア: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null)
                _gameOverScoreText.text = $"スコア: {score}";
        }

        IEnumerator ComboBounce()
        {
            if (_comboText == null) yield break;
            float dur = 0.2f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float s = t < 0.5f ? Mathf.Lerp(1f, 1.3f, t * 2f) : Mathf.Lerp(1.3f, 1f, (t - 0.5f) * 2f);
                _comboText.transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }
    }
}
