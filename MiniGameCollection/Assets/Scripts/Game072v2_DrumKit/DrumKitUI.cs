using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game072v2_DrumKit
{
    public class DrumKitUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _missText;
        [SerializeField] TextMeshProUGUI _judgementText;
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;
        [SerializeField] DrumKitGameManager _gameManager;

        Coroutine _judgementCoroutine;

        public void UpdateStage(int stage, int total) =>
            _stageText.text = $"Stage {stage} / {total}";

        public void UpdateScore(int score) =>
            _scoreText.text = score.ToString();

        public void UpdateCombo(int combo)
        {
            if (combo <= 0)
            {
                _comboText.text = "";
            }
            else
            {
                _comboText.text = $"{combo} COMBO";
                StartCoroutine(ComboPopAnim());
            }
        }

        public void UpdateMiss(int miss, int maxMiss) =>
            _missText.text = $"Miss: {miss}/{maxMiss}";

        public void ShowJudgement(string text, Color color)
        {
            if (_judgementCoroutine != null) StopCoroutine(_judgementCoroutine);
            _judgementCoroutine = StartCoroutine(ShowJudgementAnim(text, color));
        }

        IEnumerator ShowJudgementAnim(string text, Color color)
        {
            _judgementText.text = text;
            _judgementText.color = color;
            _judgementText.gameObject.SetActive(true);

            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01((t - 0.3f) / 0.3f);
                _judgementText.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
            _judgementText.gameObject.SetActive(false);
        }

        IEnumerator ComboPopAnim()
        {
            _comboText.transform.localScale = Vector3.one * 1.3f;
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1.3f, 1f, t / 0.15f);
                _comboText.transform.localScale = Vector3.one * s;
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }

        public void ShowStageClear(int stageNum)
        {
            _stageClearPanel.SetActive(true);
            _stageClearText.text = $"Stage {stageNum} Clear!";
        }

        public void ShowAllClear(int score)
        {
            _stageClearPanel.SetActive(false);
            _allClearPanel.SetActive(true);
            _allClearScoreText.text = $"Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            _gameOverPanel.SetActive(true);
            _gameOverScoreText.text = $"Score: {score}";
        }

        public void OnNextStageButton()
        {
            _stageClearPanel.SetActive(false);
            _gameManager.NextStage();
        }
    }
}
