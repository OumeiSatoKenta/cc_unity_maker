using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game095v2_SilentWorld
{
    public class SilentWorldUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _stageText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _lifeText;
        [SerializeField] TMP_Text _hintText;
        [SerializeField] TMP_Text _timeText;
        [SerializeField] TMP_Text _comboText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TMP_Text _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TMP_Text _gameOverScoreText;
        [SerializeField] Button _retryButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TMP_Text _allClearScoreText;

        [SerializeField] Button _menuButton;

        [SerializeField] SilentWorldGameManager _gameManager;
        [SerializeField] WorldManager _worldManager;

        bool _showingTime;
        Coroutine _comboCoroutine;

        void Start()
        {
            _stageClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            if (_timeText != null) _timeText.gameObject.SetActive(false);
            if (_comboText != null) _comboText.text = "";

            _nextStageButton?.onClick.AddListener(() => _gameManager?.NextStage());
            _retryButton?.onClick.AddListener(() => _gameManager?.RestartGame());
        }

        void Update()
        {
            if (_worldManager != null && _worldManager.IsActive)
            {
                float t = _worldManager.GetTimeRemaining();
                if (_timeText != null && _timeText.gameObject.activeSelf)
                {
                    _timeText.text = $"残り {Mathf.CeilToInt(t)}秒";
                    _timeText.color = t <= 10f ? Color.red : Color.white;
                }
            }
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateLives(int lives, int maxLives)
        {
            if (_lifeText == null) return;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < maxLives; i++)
                sb.Append(i < lives ? "♥" : "♡");
            _lifeText.text = sb.ToString();
        }

        public void UpdateHints(int used, int max)
        {
            if (_hintText != null)
            {
                int remaining = max - used;
                _hintText.text = $"ヒント: {remaining}回";
                _hintText.color = remaining <= 0 ? Color.gray : new Color(0.5f, 0.8f, 1f);
            }
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo >= 2)
            {
                _comboText.text = $"COMBO x{combo}  {multiplier:F1}x";
                if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
                _comboCoroutine = StartCoroutine(ComboPopEffect());
            }
            else
            {
                _comboText.text = "";
            }
        }

        IEnumerator ComboPopEffect()
        {
            if (_comboText == null) yield break;
            float t = 0f;
            while (t < 0.25f)
            {
                float s = 1f + 0.3f * Mathf.Sin(Mathf.PI * t / 0.25f);
                _comboText.transform.localScale = Vector3.one * s;
                t += Time.deltaTime;
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }

        public void ShowStageClear(int stageNum, int score)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearScoreText != null)
                    _stageClearScoreText.text = $"Stage {stageNum} クリア！\nScore: {score}";
            }
        }

        public void HideStageClear()
        {
            _stageClearPanel?.SetActive(false);
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null)
                    _gameOverScoreText.text = $"GAME OVER\nScore: {score}";
            }
        }

        public void HideGameOver()
        {
            _gameOverPanel?.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText != null)
                    _allClearScoreText.text = $"SILENT WORLD\nクリア！\nFinal Score: {score}";
            }
        }

        public void ShowTimeLimit(bool show)
        {
            if (_timeText != null) _timeText.gameObject.SetActive(show);
        }
    }
}
