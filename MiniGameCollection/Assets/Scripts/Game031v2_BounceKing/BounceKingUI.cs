using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Game031v2_BounceKing
{
    public class BounceKingUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _lifeText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _blocksText;

        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _finalClearPanel;
        [SerializeField] TextMeshProUGUI _finalScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        [SerializeField] BounceKingGameManager _gameManager;

        Coroutine _comboPopCo;

        public void Initialize(BounceKingGameManager gm)
        {
            _gameManager = gm;
            HideAllPanels();
            if (_comboText != null) _comboText.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateLife(int life)
        {
            if (_lifeText != null)
            {
                string hearts = "";
                for (int i = 0; i < life; i++) hearts += "♥";
                _lifeText.text = hearts;
            }
        }

        public void UpdateBlocks(int remaining)
        {
            if (_blocksText != null) _blocksText.text = $"Blocks: {remaining}";
        }

        public void ShowCombo(int count, float multiplier)
        {
            if (_comboText == null) return;
            if (count <= 1)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"Combo x{count}\n×{multiplier:F1}";
            if (_comboPopCo != null) StopCoroutine(_comboPopCo);
            _comboPopCo = StartCoroutine(ComboPop());
        }

        IEnumerator ComboPop()
        {
            if (_comboText == null) yield break;
            _comboText.transform.localScale = Vector3.one;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.3f;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.5f, ratio * 2f)
                    : Mathf.Lerp(1.5f, 1f, (ratio - 0.5f) * 2f);
                _comboText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            _comboText.transform.localScale = Vector3.one;
        }

        public void ShowStageClear(int stage, int bonus)
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(true);
            if (_stageClearText != null)
                _stageClearText.text = $"ステージ {stage} クリア！\nボーナス: {bonus}pt";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowFinalClear(int score)
        {
            HideAllPanels();
            if (_finalClearPanel != null) _finalClearPanel.SetActive(true);
            if (_finalScoreText != null) _finalScoreText.text = $"Final Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            HideAllPanels();
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }

        void HideAllPanels()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_finalClearPanel != null) _finalClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        public void OnNextStagePressed()
        {
            _gameManager?.OnNextStagePressed();
        }

        public void OnRestartPressed()
        {
            _gameManager?.RestartGame();
        }

        public void OnMenuPressed()
        {
            _gameManager?.ReturnToMenu();
        }
    }
}
