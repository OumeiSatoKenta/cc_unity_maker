using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game067v2_TapDojo
{
    public class TapDojoUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _mpText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] TextMeshProUGUI _autoRateText;
        [SerializeField] TextMeshProUGUI _rankText;

        // Tech buttons
        [SerializeField] Button _seikenBtn;
        [SerializeField] Button _mawashiBtn;
        [SerializeField] Button _tohouBtn;
        [SerializeField] Button _shihanTestBtn;

        // Feature buttons
        [SerializeField] Button _tournamentBtn;
        [SerializeField] Button _trainingBtn;

        // Panels
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] GameObject _trainingTimerPanel;
        [SerializeField] TextMeshProUGUI _trainingTimerText;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateMP(long mp, long target)
        {
            if (_mpText) _mpText.text = $"修行PT: {FormatNumber(mp)} / {FormatNumber(target)}";
        }

        public void UpdateCombo(int combo, float multiplier)
        {
            if (_comboText == null) return;
            if (combo <= 0)
            {
                _comboText.text = "";
                return;
            }
            _comboText.text = $"COMBO x{combo}  ({multiplier}x)";
            // Color by multiplier
            if (multiplier >= 5f) _comboText.color = Color.red;
            else if (multiplier >= 3f) _comboText.color = new Color(1f, 0.5f, 0f);
            else if (multiplier >= 2f) _comboText.color = Color.yellow;
            else _comboText.color = Color.white;
            // Scale effect
            _comboText.transform.localScale = Vector3.one * 1.2f;
            _comboText.transform.localScale = Vector3.one * 1.2f;
            StartCoroutine(ScaleBackToOne(_comboText.transform, 0.15f));
        }

        public void UpdateAutoRate(float rate)
        {
            if (_autoRateText) _autoRateText.text = $"自動: {rate:F1}/秒";
        }

        public void UpdateRank(string rankName)
        {
            if (_rankText) _rankText.text = rankName;
        }

        public void UpdateTechButtons(bool[] unlocked, bool[] affordable, bool autoUnlocked, bool tournamentUnlocked, bool trainingUnlocked, bool shihanTestUnlocked)
        {
            SetTechBtn(_seikenBtn, unlocked[0], affordable[0], unlocked[0] ? "正拳突き 習得済" : "正拳突き (50MP)");
            if (_mawashiBtn) _mawashiBtn.gameObject.SetActive(autoUnlocked); // mawashi available from stage 1 (when auto is unlocked)
            SetTechBtn(_mawashiBtn, unlocked[1], affordable[1], unlocked[1] ? "回し蹴り 習得済" : "回し蹴り (200MP)");
            if (_tohouBtn) _tohouBtn.gameObject.SetActive(trainingUnlocked);
            SetTechBtn(_tohouBtn, unlocked[2], false, unlocked[2] ? "虎砲 習得済" : "虎砲 (特訓で習得)");

            // ShihanTest is handled via StartShihanTest(), not BuyTech
            if (_shihanTestBtn) _shihanTestBtn.gameObject.SetActive(shihanTestUnlocked);
            if (_shihanTestBtn != null)
            {
                bool owned = unlocked[3];
                bool canTest = shihanTestUnlocked && !owned && affordable[3];
                _shihanTestBtn.interactable = canTest;
                var img = _shihanTestBtn.GetComponent<UnityEngine.UI.Image>();
                if (img) img.color = owned ? new Color(0.6f, 0.8f, 0.6f) : canTest ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
                var txt = _shihanTestBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txt) txt.text = owned ? "師範位 取得済" : "師範試験 (5000MP)";
            }

            if (_tournamentBtn) _tournamentBtn.gameObject.SetActive(tournamentUnlocked);
            if (_trainingBtn)
            {
                _trainingBtn.gameObject.SetActive(trainingUnlocked);
                // Disable training button if tohou already unlocked
                _trainingBtn.interactable = trainingUnlocked && !unlocked[2];
            }
        }

        void SetTechBtn(Button btn, bool isOwned, bool isAffordable, string label)
        {
            if (btn == null) return;
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text) text.text = label;
            btn.interactable = !isOwned && isAffordable;
            var img = btn.GetComponent<Image>();
            if (img)
            {
                if (isOwned) img.color = new Color(0.6f, 0.8f, 0.6f);
                else if (isAffordable) img.color = Color.white;
                else img.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }
        }

        public void UpdateTrainingTimer(bool active, float remaining, int taps, int goal)
        {
            if (_trainingTimerPanel) _trainingTimerPanel.SetActive(active);
            if (_trainingTimerText && active)
                _trainingTimerText.text = $"特訓！ {remaining:F1}秒  {taps}/{goal}タップ";
        }

        public void ShowStageClear(int stage)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearText) _stageClearText.text = $"Stage {stage} クリア！";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowAllClear(long score)
        {
            if (_allClearPanel) _allClearPanel.SetActive(true);
            if (_allClearScoreText) _allClearScoreText.text = $"総修行PT: {FormatNumber(score)}\n師範への道、完成！";
        }

        string FormatNumber(long n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000.0:F1}M";
            if (n >= 1_000) return $"{n / 1_000.0:F1}K";
            return n.ToString();
        }

        System.Collections.IEnumerator ScaleBackToOne(Transform t, float duration)
        {
            Vector3 start = t.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(start, Vector3.one, elapsed / duration);
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }
}
