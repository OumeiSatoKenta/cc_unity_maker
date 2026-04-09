using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game098v2_InfiniteLoop
{
    public class InfiniteLoopUI : MonoBehaviour
    {
        [SerializeField] InfiniteLoopGameManager _gameManager;
        [SerializeField] LoopManager _loopManager;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _loopCountText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _reverseText;

        [Header("Action Buttons")]
        [SerializeField] Button _escapeButton;
        [SerializeField] Button _nextLoopButton;
        [SerializeField] Button _memoButton;
        [SerializeField] Button _backToMenuButton;

        [Header("Memo Panel")]
        [SerializeField] GameObject _memoPanel;
        [SerializeField] TextMeshProUGUI _memoContent;
        [SerializeField] Button _memoCloseButton;

        [Header("Stage Clear Panel")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [Header("Game Over Panel")]
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverText;
        [SerializeField] Button _restartButton;

        [Header("All Clear Panel")]
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearText;

        [Header("Escape Failed")]
        [SerializeField] GameObject _escapeFailedPanel;
        [SerializeField] TextMeshProUGUI _escapeFailedText;

        List<string> _memoEntries = new List<string>();
        Coroutine _comboCoroutine;

        void Start()
        {
            _escapeButton?.onClick.AddListener(OnEscapeClicked);
            _nextLoopButton?.onClick.AddListener(OnNextLoopClicked);
            _memoButton?.onClick.AddListener(OnMemoClicked);
            _memoCloseButton?.onClick.AddListener(OnMemoCloseClicked);
            _backToMenuButton?.onClick.AddListener(OnBackToMenuClicked);
            _nextStageButton?.onClick.AddListener(OnNextStageClicked);
            _restartButton?.onClick.AddListener(OnRestartClicked);

            _memoPanel?.SetActive(false);
            _stageClearPanel?.SetActive(false);
            _gameOverPanel?.SetActive(false);
            _allClearPanel?.SetActive(false);
            _escapeFailedPanel?.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
            if (_reverseText != null) _reverseText.gameObject.SetActive(false);
        }

        public void UpdateStage(int stage, int total)
        {
            if (_stageText != null) _stageText.text = $"Stage {stage} / {total}";
            _memoEntries.Clear();
            UpdateMemoContent();
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        public void UpdateLoopCount(int remaining)
        {
            if (_loopCountText != null)
            {
                _loopCountText.text = $"残り {remaining} ループ";
                _loopCountText.color = remaining <= 2 ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.9f, 0.3f);
            }
        }

        public void SetReverseIndicator(bool isReverse)
        {
            if (_reverseText != null)
            {
                _reverseText.gameObject.SetActive(isReverse);
                if (isReverse) _reverseText.text = "【注意】逆行ループ！";
            }
        }

        public void AddMemoEntry(string elemId, bool isReal)
        {
            string label = elemId switch
            {
                "door" => "ドア",
                "window" => "窓",
                "book" => "本",
                "clock" => "時計",
                "picture" => "絵画",
                "plant" => "植物",
                _ => elemId
            };
            _memoEntries.Add($"・{label} の変化を発見");
            UpdateMemoContent();
        }

        void UpdateMemoContent()
        {
            if (_memoContent == null) return;
            if (_memoEntries.Count == 0)
                _memoContent.text = "（まだ何も発見していない）";
            else
                _memoContent.text = string.Join("\n", _memoEntries);
        }

        public void ShowComboIfNeeded(int comboCount)
        {
            if (comboCount < 2 || _comboText == null) return;
            _comboText.text = $"コンボ x{comboCount}！";
            _comboText.gameObject.SetActive(true);
            if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
            _comboCoroutine = StartCoroutine(HideAfter(_comboText.gameObject, 1.5f));
        }

        IEnumerator HideAfter(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) go.SetActive(false);
        }

        public void ShowEscapeFailed()
        {
            if (_escapeFailedPanel == null) return;
            _escapeFailedPanel.SetActive(true);
            if (_escapeFailedText != null) _escapeFailedText.text = "だまされた！ループ-2消費";
            StartCoroutine(HideAfter(_escapeFailedPanel, 1.5f));
        }

        public void ShowStageClear(int stage, int score)
        {
            _stageClearPanel?.SetActive(true);
            if (_stageClearText != null)
                _stageClearText.text = $"ステージ {stage} クリア！\nスコア: {score}";
        }

        public void HideStageClear()
        {
            _stageClearPanel?.SetActive(false);
        }

        public void ShowGameOver(int score)
        {
            _gameOverPanel?.SetActive(true);
            if (_gameOverText != null)
                _gameOverText.text = $"ループ制限超過...\nスコア: {score}";
        }

        public void HideGameOver()
        {
            _gameOverPanel?.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            _allClearPanel?.SetActive(true);
            if (_allClearText != null)
                _allClearText.text = $"全ステージ脱出！\n最終スコア: {score}";
        }

        void OnEscapeClicked() => _loopManager?.TryEscape();
        void OnNextLoopClicked() => _loopManager?.AdvanceLoop();
        void OnMemoClicked()
        {
            _loopManager?.OpenMemo();
            _memoPanel?.SetActive(true);
        }
        void OnMemoCloseClicked() => _memoPanel?.SetActive(false);

        void OnBackToMenuClicked()
        {
            SceneLoader.BackToMenu();
        }

        void OnNextStageClicked() => _gameManager?.NextStage();
        void OnRestartClicked() => _gameManager?.RestartGame();
    }
}
