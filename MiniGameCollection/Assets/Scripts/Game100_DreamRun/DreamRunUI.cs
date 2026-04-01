using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game100_DreamRun
{
    public class DreamRunUI : MonoBehaviour
    {
        [SerializeField, Tooltip("距離テキスト")] private TextMeshProUGUI _distanceText;
        [SerializeField, Tooltip("断片数テキスト")] private TextMeshProUGUI _fragmentText;
        [SerializeField, Tooltip("ライフテキスト")] private TextMeshProUGUI _lifeText;
        [SerializeField, Tooltip("ストーリーテキスト")] private TextMeshProUGUI _storyText;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアテキスト")] private TextMeshProUGUI _clearText;
        [SerializeField, Tooltip("ゲームオーバーパネル")] private GameObject _gameOverPanel;
        [SerializeField, Tooltip("ゲームオーバーテキスト")] private TextMeshProUGUI _gameOverText;

        private Coroutine _storyCoroutine;

        public void UpdateDistance(float dist)
        {
            if (_distanceText) _distanceText.text = $"{dist:F0}m";
        }

        public void UpdateFragments(int count, int total)
        {
            if (_fragmentText) _fragmentText.text = $"断片 {count}/{total}";
        }

        public void UpdateLife(int life, int max)
        {
            if (_lifeText)
            {
                string hearts = "";
                for (int i = 0; i < max; i++)
                    hearts += i < life ? "♥ " : "♡ ";
                _lifeText.text = hearts.TrimEnd();
            }
        }

        public void ShowStoryText(string text)
        {
            if (_storyText == null) return;
            if (_storyCoroutine != null) StopCoroutine(_storyCoroutine);
            _storyCoroutine = StartCoroutine(StoryTextCoroutine(text));
        }

        private IEnumerator StoryTextCoroutine(string text)
        {
            _storyText.text = text;
            _storyText.gameObject.SetActive(true);

            // フェードイン
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                _storyText.color = new Color(1f, 1f, 0.8f, elapsed / 0.5f);
                yield return null;
            }
            _storyText.color = new Color(1f, 1f, 0.8f, 1f);

            yield return new WaitForSeconds(2.5f);

            // フェードアウト
            elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                if (_storyText != null)
                    _storyText.color = new Color(1f, 1f, 0.8f, 1f - elapsed / 0.5f);
                yield return null;
            }
            if (_storyText != null) _storyText.gameObject.SetActive(false);
        }

        public void HideStoryText()
        {
            if (_storyText) _storyText.gameObject.SetActive(false);
        }

        public void ShowClearPanel(float distance, int fragments)
        {
            if (_clearPanel) _clearPanel.SetActive(true);
            if (_clearText) _clearText.text = $"夢から覚めた…\n\n走行距離: {distance:F0}m\n断片: {fragments}個収集";
        }

        public void HideClearPanel()
        {
            if (_clearPanel) _clearPanel.SetActive(false);
        }

        public void ShowGameOverPanel(float distance, int fragments)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverText) _gameOverText.text = $"夢が途切れた…\n\n走行距離: {distance:F0}m\n断片: {fragments}個";
        }

        public void HideGameOverPanel()
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(false);
        }
    }
}
