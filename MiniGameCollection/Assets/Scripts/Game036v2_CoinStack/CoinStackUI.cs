using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game036v2_CoinStack
{
    public class CoinStackUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _coinCountText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;
        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;
        [SerializeField] Button _finalRetryButton;
        [SerializeField] Button _finalMenuButton;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] Button _retryButton;
        [SerializeField] Button _returnMenuButton;

        CoinStackGameManager _manager;

        public void Initialize(CoinStackGameManager manager)
        {
            _manager = manager;

            if (_nextStageButton != null)
            {
                _nextStageButton.onClick.RemoveAllListeners();
                _nextStageButton.onClick.AddListener(() => _manager.AdvanceToNextStage());
            }
            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveAllListeners();
                _retryButton.onClick.AddListener(() => _manager.RetryGame());
            }
            if (_returnMenuButton != null)
            {
                _returnMenuButton.onClick.RemoveAllListeners();
                _returnMenuButton.onClick.AddListener(() => _manager.ReturnToMenu());
            }
            if (_finalRetryButton != null)
            {
                _finalRetryButton.onClick.RemoveAllListeners();
                _finalRetryButton.onClick.AddListener(() => _manager.RetryGame());
            }
            if (_finalMenuButton != null)
            {
                _finalMenuButton.onClick.RemoveAllListeners();
                _finalMenuButton.onClick.AddListener(() => _manager.ReturnToMenu());
            }

            HideStageClear();
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void UpdateStage(int current, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {current} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCoinCount(int remaining)
        {
            if (_coinCountText != null) _coinCountText.text = $"残り {remaining} 枚";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 0)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            string comboLabel = combo >= 5 ? $"COMBO x3 ({combo})" : combo >= 3 ? $"COMBO x2 ({combo})" : $"COMBO {combo}";
            _comboText.text = comboLabel;
            StartCoroutine(ComboPopAnimation());
        }

        IEnumerator ComboPopAnimation()
        {
            if (_comboText == null) yield break;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, 1.5f, Mathf.Sin(t / 0.3f * Mathf.PI));
                _comboText.transform.localScale = Vector3.one * s;
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }

        public void ShowStageClear(int stageNum, int totalStages)
        {
            if (_stageClearPanel != null)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText != null)
                    _stageClearText.text = stageNum < totalStages ? "ステージクリア！" : "全ステージクリア！";
            }
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
                if (_finalScoreText != null) _finalScoreText.text = $"最終スコア: {score}";
            }
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverScoreText != null) _gameOverScoreText.text = $"スコア: {score}";
            }
        }
    }
}
