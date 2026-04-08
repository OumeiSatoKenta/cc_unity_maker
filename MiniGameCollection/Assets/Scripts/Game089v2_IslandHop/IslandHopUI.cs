using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game089v2_IslandHop
{
    public class IslandHopUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _targetText;

        [Header("Resources")]
        [SerializeField] TextMeshProUGUI _woodText;
        [SerializeField] TextMeshProUGUI _stoneText;
        [SerializeField] TextMeshProUGUI _foodText;
        [SerializeField] TextMeshProUGUI _goldText;

        [Header("Panels")]
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] Button _nextStageButton;

        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;

        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] TextMeshProUGUI _gameOverScoreText;

        [Header("Build Panel")]
        [SerializeField] GameObject _buildPanel;
        [SerializeField] Transform _buildButtonContainer;
        [SerializeField] GameObject _buildButtonPrefab;
        [SerializeField] TextMeshProUGUI _buildPanelTitleText;

        [Header("Notifications")]
        [SerializeField] TextMeshProUGUI _feedbackText;
        [SerializeField] TextMeshProUGUI _guestRequestText;
        [SerializeField] TextMeshProUGUI _weatherWarningText;
        [SerializeField] TextMeshProUGUI _noResourceText;

        static readonly int[] StageTargets = { 50, 120, 200, 300, 420 };

        Action<FacilityType> _buildCallback;
        Coroutine _feedbackCoroutine;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
            int target = (stage - 1) < StageTargets.Length ? StageTargets[stage - 1] : 0;
            if (_targetText) _targetText.text = $"目標: {target}pt";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText) _scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo, float mult)
        {
            if (_comboText)
            {
                if (combo >= 2)
                    _comboText.text = $"Combo x{combo} (×{mult:F1})";
                else
                    _comboText.text = "";
            }
        }

        public void UpdateResources(int wood, int stone, int food, int gold)
        {
            if (_woodText)  _woodText.text  = $"[木]{wood}";
            if (_stoneText) _stoneText.text = $"[石]{stone}";
            if (_foodText)  _foodText.text  = $"[食]{food}";
            if (_goldText)  _goldText.text  = $"[金]{gold}";
        }

        public void ShowBuildFeedback(int earned, bool hasSynergy)
        {
            string msg = hasSynergy ? $"+{earned}pt シナジー！" : $"+{earned}pt";
            ShowFeedback(msg, hasSynergy ? new Color(1f, 0.85f, 0.1f) : Color.white);
        }

        public void ShowNotEnoughResources()
        {
            if (_noResourceText)
            {
                _noResourceText.text = "資源が足りません！";
                if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
                _feedbackCoroutine = StartCoroutine(FadeText(_noResourceText, 2f));
            }
        }

        public void ShowGuestRequest(FacilityType facility, string facilityName)
        {
            if (_guestRequestText) _guestRequestText.text = $"[客] お客様のリクエスト: {facilityName}";
        }

        public void HideGuestRequest()
        {
            if (_guestRequestText) _guestRequestText.text = "";
        }

        public void ShowWeatherWarning(float seconds)
        {
            if (_weatherWarningText)
                StartCoroutine(WeatherWarningCR(seconds));
        }

        public void ShowWeatherBlocked()
        {
            ShowFeedback("灯台が嵐を防いだ！", new Color(0.5f, 1f, 0.5f));
        }

        IEnumerator WeatherWarningCR(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float remaining = duration - elapsed;
                if (_weatherWarningText) _weatherWarningText.text = $"[嵐] 接近！ {remaining:F0}秒後";
                yield return null;
            }
            if (_weatherWarningText) _weatherWarningText.text = "";
        }

        public void ShowBuildPanel(List<(FacilityType type, string name, int wood, int stone, bool canAfford)> facilities, Action<FacilityType> callback)
        {
            _buildCallback = callback;
            if (_buildPanelTitleText) _buildPanelTitleText.text = "施設を選択";

            // Clear existing buttons
            if (_buildButtonContainer != null)
            {
                foreach (Transform child in _buildButtonContainer)
                    Destroy(child.gameObject);
            }

            // Create buttons for each facility
            foreach (var (ftype, fname, wood, stone, canAfford) in facilities)
            {
                if (_buildButtonContainer == null || _buildButtonPrefab == null) break;
                var btnObj = Instantiate(_buildButtonPrefab, _buildButtonContainer);
                var btn = btnObj.GetComponent<Button>();
                var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp) tmp.text = $"{fname}\n[木]{wood} [石]{stone}";
                btn.interactable = canAfford;
                var ft = ftype; // capture
                btn.onClick.AddListener(() => _buildCallback?.Invoke(ft));
            }

            // Cancel button
            if (_buildButtonContainer != null && _buildButtonPrefab != null)
            {
                var cancelObj = Instantiate(_buildButtonPrefab, _buildButtonContainer);
                var cancelBtn = cancelObj.GetComponent<Button>();
                var cancelTmp = cancelObj.GetComponentInChildren<TextMeshProUGUI>();
                if (cancelTmp) cancelTmp.text = "キャンセル";
                cancelBtn.onClick.AddListener(() => _buildCallback?.Invoke(FacilityType.None));
            }

            if (_buildPanel) _buildPanel.SetActive(true);
        }

        public void HideBuildPanel()
        {
            if (_buildPanel) _buildPanel.SetActive(false);
            _buildCallback = null;
        }

        public void ShowStageClear(int stageNum, int score)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearText) _stageClearText.text = $"ステージ {stageNum} クリア！\nスコア: {score}";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(int score)
        {
            if (_allClearPanel) _allClearPanel.SetActive(true);
            if (_allClearScoreText) _allClearScoreText.text = $"全クリア！\nFinalScore: {score}";
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel) _gameOverPanel.SetActive(true);
            if (_gameOverScoreText) _gameOverScoreText.text = $"ゲームオーバー\nScore: {score}";
        }

        void ShowFeedback(string msg, Color color)
        {
            if (_feedbackText == null) return;
            _feedbackText.text = msg;
            _feedbackText.color = color;
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(FadeText(_feedbackText, 1.5f));
        }

        IEnumerator FadeText(TextMeshProUGUI tmp, float duration)
        {
            float elapsed = 0f;
            Color original = tmp.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                tmp.color = new Color(original.r, original.g, original.b, Mathf.Lerp(1f, 0f, elapsed / duration));
                yield return null;
            }
            tmp.text = "";
            tmp.color = Color.white;
        }
    }
}
