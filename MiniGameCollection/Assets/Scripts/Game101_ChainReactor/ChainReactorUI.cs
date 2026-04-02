using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game101_ChainReactor
{
    public class ChainReactorUI : MonoBehaviour
    {
        [SerializeField, Tooltip("残りタップ数")] private TextMeshProUGUI _tapsText;
        [SerializeField, Tooltip("スコア")] private TextMeshProUGUI _scoreText;
        [SerializeField, Tooltip("ステージ表示")] private TextMeshProUGUI _stageText;
        [SerializeField, Tooltip("連鎖数表示")] private TextMeshProUGUI _chainText;
        [SerializeField, Tooltip("タイマー")] private TextMeshProUGUI _timerText;
        [SerializeField, Tooltip("オーブ残数")] private TextMeshProUGUI _orbCountText;
        [SerializeField, Tooltip("ステージクリアパネル")] private GameObject _stageClearPanel;
        [SerializeField, Tooltip("ステージクリアテキスト")] private TextMeshProUGUI _stageClearText;
        [SerializeField, Tooltip("次ステージボタン")] private Button _nextStageButton;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアテキスト")] private TextMeshProUGUI _clearText;
        [SerializeField, Tooltip("ゲームオーバーパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("ゲームオーバーテキスト")] private TextMeshProUGUI _gameOverText;

        private Coroutine _chainPulse;

        public void UpdateTaps(int taps)
        {
            if (_tapsText) _tapsText.text = $"TAP: {taps}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"SCORE: {score}";
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateChain(int chain)
        {
            if (_chainText)
            {
                _chainText.text = chain > 0 ? $"x{chain} CHAIN!" : "";
                if (chain > 0)
                {
                    if (_chainPulse != null) StopCoroutine(_chainPulse);
                    _chainPulse = StartCoroutine(ChainPulseEffect());
                }
            }
        }

        public void UpdateTimer(float time)
        {
            if (_timerText)
            {
                if (time < 0)
                {
                    _timerText.gameObject.SetActive(false);
                }
                else
                {
                    _timerText.gameObject.SetActive(true);
                    _timerText.text = $"TIME: {time:F1}";
                    _timerText.color = time < 5f ? new Color(1f, 0.3f, 0.3f) : Color.white;
                }
            }
        }

        public void UpdateOrbCount(int remaining, int total)
        {
            if (_orbCountText) _orbCountText.text = $"ORB: {remaining}/{total}";
        }

        public void ShowMultiplier(int multiplier)
        {
            if (_chainText)
            {
                _chainText.text = $"x{multiplier} BONUS!";
                _chainText.color = new Color(1f, 0.85f, 0.2f);
                if (_chainPulse != null) StopCoroutine(_chainPulse);
                _chainPulse = StartCoroutine(ChainPulseEffect());
            }
        }

        private IEnumerator ChainPulseEffect()
        {
            if (_chainText == null) yield break;
            var rt = _chainText.GetComponent<RectTransform>();
            if (rt == null) yield break;

            Vector3 orig = rt.localScale;
            rt.localScale = orig * 1.5f;
            _chainText.color = new Color(1f, 0.9f, 0.3f);

            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                rt.localScale = Vector3.Lerp(orig * 1.5f, orig, t);
                _chainText.color = Color.Lerp(new Color(1f, 0.9f, 0.3f), Color.white, t);
                yield return null;
            }
            rt.localScale = orig;
        }

        public void ShowStageClearPanel(int stage, int score)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearText) _stageClearText.text = $"Stage {stage} クリア！\nスコア: {score}";
        }

        public void ShowClearPanel(int score)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearText) _clearText.text = $"全ステージクリア！\n\n最終スコア: {score}";
        }

        public void ShowGameOverPanel(int score, int stage)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverText) _gameOverText.text = $"ゲームオーバー\n\nStage {stage}\nスコア: {score}";
        }

        public void HideAllPanels()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_clearPanel) _clearPanel.SetActive(false);
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }
    }
}
