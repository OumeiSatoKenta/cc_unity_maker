using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game097v2_PixelEvolution
{
    public class PixelEvolutionUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _generationText;
        [SerializeField] TextMeshProUGUI _evolutionLevelText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _mutationText;

        [Header("Environment UI")]
        [SerializeField] TextMeshProUGUI _tempValueText;
        [SerializeField] TextMeshProUGUI _humidityValueText;
        [SerializeField] TextMeshProUGUI _lightValueText;

        [Header("Branch Choice Panel")]
        [SerializeField] GameObject _branchPanel;
        [SerializeField] Button _branchBtn0;
        [SerializeField] Button _branchBtn1;
        [SerializeField] Button _branchBtn2;
        [SerializeField] TextMeshProUGUI _branchTitle;

        [Header("Stage Clear Panel")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;

        [Header("All Clear Panel")]
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [Header("Game Over Panel")]
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        void Start()
        {
            if (_branchPanel != null) _branchPanel.SetActive(false);
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
            if (_allClearPanel != null) _allClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
            if (_mutationText != null) _mutationText.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateGeneration(int gen, int limit)
        {
            if (_generationText != null) _generationText.text = $"世代: {gen} / {limit}";
        }

        public void UpdateEvolutionLevel(int level, int maxLevel)
        {
            if (_evolutionLevelText != null) _evolutionLevelText.text = $"進化Lv: {level} / {maxLevel}";
        }

        public void UpdateEnvironment(EnvLevel temp, EnvLevel humidity, EnvLevel light)
        {
            string[] labels = { "低", "中", "高" };
            if (_tempValueText != null) _tempValueText.text = labels[(int)temp];
            if (_humidityValueText != null) _humidityValueText.text = labels[(int)humidity];
            if (_lightValueText != null) _lightValueText.text = labels[(int)light];
        }

        public void ShowComboIfNeeded(int comboCount)
        {
            if (_comboText == null) return;
            if (comboCount >= 3)
            {
                _comboText.gameObject.SetActive(true);
                _comboText.text = $"COMBO x{comboCount}!";
                StartCoroutine(HideAfterDelay(_comboText.gameObject, 1.5f));
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        public void ShowMutationEffect()
        {
            if (_mutationText == null) return;
            _mutationText.gameObject.SetActive(true);
            _mutationText.text = "突然変異！";
            StartCoroutine(HideAfterDelay(_mutationText.gameObject, 1.5f));
        }

        public void ShowBranchChoice(int count, bool hiddenAvailable)
        {
            if (_branchPanel == null) return;
            _branchPanel.SetActive(true);

            if (_branchTitle != null)
                _branchTitle.text = "進化の分岐を選択してください";

            // Setup buttons
            if (_branchBtn0 != null)
            {
                _branchBtn0.gameObject.SetActive(true);
                var t = _branchBtn0.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) t.text = "安全な進化";
            }
            if (_branchBtn1 != null)
            {
                _branchBtn1.gameObject.SetActive(count >= 2);
                var t = _branchBtn1.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) t.text = "複合進化";
            }
            if (_branchBtn2 != null)
            {
                _branchBtn2.gameObject.SetActive(hiddenAvailable);
                var t = _branchBtn2.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) t.text = "★隠し進化 (+200pt)";
            }
        }

        public void HideBranchChoice()
        {
            if (_branchPanel != null) _branchPanel.SetActive(false);
        }

        public void ShowStageClear(int stage, int score)
        {
            if (_stageClearPanel == null) return;
            _stageClearPanel.SetActive(true);
            if (_stageClearText != null) _stageClearText.text = $"Stage {stage} クリア！";
            if (_stageClearScoreText != null) _stageClearScoreText.text = $"Score: {score}";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel != null) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel == null) return;
            _allClearPanel.SetActive(true);
            if (_allClearScoreText != null) _allClearScoreText.text = $"Total Score: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel == null) return;
            _gameOverPanel.SetActive(true);
            if (_gameOverScoreText != null) _gameOverScoreText.text = $"Score: {score}";
        }

        public void HideGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        IEnumerator HideAfterDelay(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) go.SetActive(false);
        }
    }
}
