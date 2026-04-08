using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game091v2_TimeBlender
{
    public class TimeBlenderUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _moveText;
        [SerializeField] TextMeshProUGUI _paradoxText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _eraText;

        [SerializeField] Button _pastButton;
        [SerializeField] Button _futureButton;
        [SerializeField] Button _presentButton;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;

        [SerializeField] TimeBlenderGameManager _gameManager;
        [SerializeField] PuzzleManager _puzzleManager;

        void Start()
        {
            _stageClearPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _presentButton?.gameObject.SetActive(false);

            _pastButton?.onClick.AddListener(() => _puzzleManager.SwitchEra(Era.Past));
            _futureButton?.onClick.AddListener(() => _puzzleManager.SwitchEra(Era.Future));
            _presentButton?.onClick.AddListener(() => _puzzleManager.SwitchEra(Era.Present));
            _nextStageButton?.onClick.AddListener(() => _gameManager.NextStage());
            _retryButton?.onClick.AddListener(() => _gameManager.RestartGame());
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateMoves(int used, int limit)
        {
            if (_moveText == null) return;
            if (limit <= 0)
                _moveText.text = $"Moves: {used}";
            else
                _moveText.text = $"Moves: {used} / {limit}";
        }

        public void UpdateParadox(int count, int limit)
        {
            if (_paradoxText == null) return;
            int remaining = limit - count;
            _paradoxText.text = $"Paradox: {remaining} 残り";
            _paradoxText.color = remaining <= 1 ? Color.red : new Color(1f, 0.4f, 1f);
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo >= 3)
            {
                _comboText.text = $"Combo {combo}! x{multiplier:F1}";
                StartCoroutine(ComboPopAnimation());
            }
            else if (combo > 0)
            {
                _comboText.text = $"Combo {combo}";
            }
            else
            {
                _comboText.text = "";
            }
        }

        public void UpdateEra(Era era, bool hasThreeEras)
        {
            if (_eraText != null)
            {
                _eraText.text = era switch
                {
                    Era.Past => "過去",
                    Era.Present => "現在",
                    Era.Future => "未来",
                    _ => "過去",
                };
                _eraText.color = era switch
                {
                    Era.Past => new Color(1f, 0.6f, 0.2f),
                    Era.Present => new Color(0.3f, 0.9f, 0.5f),
                    Era.Future => new Color(0.3f, 0.7f, 1f),
                    _ => Color.white,
                };
            }

            // Update button highlights
            SetButtonHighlight(_pastButton, era == Era.Past);
            SetButtonHighlight(_futureButton, era == Era.Future);
            if (_presentButton != null)
            {
                _presentButton.gameObject.SetActive(hasThreeEras);
                SetButtonHighlight(_presentButton, era == Era.Present);
            }
        }

        void SetButtonHighlight(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = active ? new Color(1f, 0.9f, 0.3f) : new Color(0.3f, 0.3f, 0.5f);
        }

        public void ShowParadox(int remaining)
        {
            StartCoroutine(ParadoxFlash());
        }

        public void ShowStageClear(int stage, int score)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel != null) _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"Total Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }

        public void HideGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
        }

        IEnumerator ComboPopAnimation()
        {
            if (_comboText == null) yield break;
            float dur = 0.25f;
            float elapsed = 0f;
            Vector3 origScale = _comboText.transform.localScale;
            while (elapsed < dur)
            {
                float ratio = elapsed / dur;
                float s = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.4f, ratio * 2f)
                    : Mathf.Lerp(1.4f, 1f, (ratio - 0.5f) * 2f);
                _comboText.transform.localScale = origScale * s;
                elapsed += Time.deltaTime;
                yield return null;
            }
            _comboText.transform.localScale = origScale;
        }

        IEnumerator ParadoxFlash()
        {
            if (_paradoxText == null) yield break;
            Color orig = _paradoxText.color;
            for (int i = 0; i < 3; i++)
            {
                _paradoxText.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                _paradoxText.color = Color.red;
                yield return new WaitForSeconds(0.1f);
            }
            _paradoxText.color = orig;
        }
    }
}
