using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Game061v2_CookieFactory
{
    public class CookieFactoryUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _cookieText;
        [SerializeField] TextMeshProUGUI _autoRateText;
        [SerializeField] TextMeshProUGUI _comboText;
        [SerializeField] Slider _progressSlider;

        // Shop buttons
        [SerializeField] Button _ovenBtn;
        [SerializeField] TextMeshProUGUI _ovenBtnText;
        [SerializeField] Button _conveyorBtn;
        [SerializeField] TextMeshProUGUI _conveyorBtnText;
        [SerializeField] Button _packagingBtn;
        [SerializeField] TextMeshProUGUI _packagingBtnText;

        // Special order
        [SerializeField] Button _specialOrderBtn;
        [SerializeField] Slider _specialOrderSlider;
        [SerializeField] GameObject _specialOrderPanel;

        // Breakdown
        [SerializeField] GameObject _breakdownPanel;
        [SerializeField] TextMeshProUGUI _repairText;
        [SerializeField] Button _repairBtn;

        // VIP order
        [SerializeField] GameObject _vipPanel;
        [SerializeField] TextMeshProUGUI _vipTimerText;
        [SerializeField] TextMeshProUGUI _vipProgressText;
        [SerializeField] Slider _vipSlider;
        [SerializeField] Image _vipSliderFill;

        // Panels
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearScoreText;
        [SerializeField] GameObject _gameClearPanel;
        [SerializeField] TextMeshProUGUI _gameClearScoreText;

        // Floating text parent
        [SerializeField] RectTransform _floatingTextParent;
        [SerializeField] TextMeshProUGUI _floatingTextPrefab;

        static string FormatCookies(long n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000.0:F1}M";
            if (n >= 1_000) return $"{n / 1000.0:F1}K";
            return n.ToString();
        }

        public void UpdateStageDisplay(int stage)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / 5";
        }

        public void UpdateCookies(long cookies, long goal)
        {
            if (_cookieText) _cookieText.text = $"🍪 {FormatCookies(cookies)} / {FormatCookies(goal)}";
            if (_progressSlider) _progressSlider.value = goal > 0 ? Mathf.Clamp01((float)cookies / goal) : 0f;
        }

        public void UpdateAutoRate(float rate)
        {
            if (_autoRateText) _autoRateText.text = $"自動: {rate:F1}/秒";
        }

        public void UpdateCombo(int combo)
        {
            if (_comboText == null) return;
            if (combo <= 1)
            {
                _comboText.gameObject.SetActive(false);
                return;
            }
            _comboText.gameObject.SetActive(true);
            float mult = combo >= 10 ? 2f : combo >= 5 ? 1.5f : 1f;
            _comboText.text = $"COMBO x{combo} ({mult:F1}x)";
            _comboText.color = combo >= 10 ? Color.red : combo >= 5 ? new Color(1f, 0.6f, 0f) : Color.yellow;
        }

        public void SetupShop(int stageIndex, long[] ovenCosts, long[] convCosts, long[] packCosts, bool convVisible, bool packVisible)
        {
            if (_ovenBtnText) _ovenBtnText.text = $"オーブン\n強化 {FormatCookies(ovenCosts[0])}🍪";
            if (_conveyorBtnText) _conveyorBtnText.text = $"ベルト\n購入 {FormatCookies(convCosts[0])}🍪";
            if (_packagingBtnText) _packagingBtnText.text = $"包装機\n購入 {FormatCookies(packCosts[0])}🍪";
            if (_conveyorBtn) _conveyorBtn.gameObject.SetActive(convVisible);
            if (_packagingBtn) _packagingBtn.gameObject.SetActive(packVisible);
        }

        public void UpdateShopButtons(int ovenLv, int convLv, int packLv, long cookies,
            long[] ovenCosts, long[] convCosts, long[] packCosts, int stageIndex)
        {
            if (_ovenBtn)
            {
                bool maxed = ovenLv >= 3;
                _ovenBtn.interactable = !maxed && cookies >= ovenCosts[Mathf.Min(ovenLv, 2)];
                if (_ovenBtnText)
                    _ovenBtnText.text = maxed ? "オーブン\nMAX" : $"オーブン Lv{ovenLv + 1}\n{FormatCookies(ovenCosts[ovenLv])}🍪";
            }
            if (_conveyorBtn)
            {
                bool maxed = convLv >= 3;
                _conveyorBtn.interactable = !maxed && stageIndex >= 1 && cookies >= convCosts[Mathf.Min(convLv, 2)];
                if (_conveyorBtnText)
                    _conveyorBtnText.text = maxed ? "ベルト\nMAX" : $"ベルト Lv{convLv + 1}\n{FormatCookies(convCosts[convLv])}🍪";
            }
            if (_packagingBtn)
            {
                bool maxed = packLv >= 3;
                _packagingBtn.interactable = !maxed && stageIndex >= 2 && cookies >= packCosts[Mathf.Min(packLv, 2)];
                if (_packagingBtnText)
                    _packagingBtnText.text = maxed ? "包装機\nMAX" : $"包装機 Lv{packLv + 1}\n{FormatCookies(packCosts[packLv])}🍪";
            }
        }

        public void SetSpecialOrderVisible(bool visible)
        {
            if (_specialOrderBtn) _specialOrderBtn.gameObject.SetActive(visible);
        }

        public void SetSpecialOrderActive(bool active)
        {
            if (_specialOrderBtn) _specialOrderBtn.interactable = !active;
            if (_specialOrderPanel) _specialOrderPanel.SetActive(active);
        }

        public void UpdateSpecialOrderTimer(float ratio)
        {
            if (_specialOrderSlider) _specialOrderSlider.value = ratio;
        }

        public void SetBreakdownActive(bool active, int tapsLeft)
        {
            if (_breakdownPanel) _breakdownPanel.SetActive(active);
            UpdateRepairProgress(tapsLeft);
        }

        public void UpdateRepairProgress(int tapsLeft)
        {
            if (_repairText) _repairText.text = $"故障！修理タップ: {tapsLeft}回";
        }

        public void SetVIPOrderActive(bool active)
        {
            if (_vipPanel) _vipPanel.SetActive(active);
        }

        public void UpdateVIPOrder(float timerRatio, long progress, long goal, float secondsLeft = 30f)
        {
            if (_vipTimerText) _vipTimerText.text = $"VIP注文 {Mathf.CeilToInt(secondsLeft)}秒";
            if (_vipProgressText) _vipProgressText.text = $"{FormatCookies(progress)}/{FormatCookies(goal)}";
            if (_vipSlider) _vipSlider.value = timerRatio;
            if (_vipSliderFill)
            {
                _vipSliderFill.color = timerRatio > 0.5f ? Color.green :
                                       timerRatio > 0.25f ? Color.yellow : Color.red;
            }
        }

        public void ShowStageClear(long cookies)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(true);
            if (_stageClearScoreText) _stageClearScoreText.text = $"生産量: {FormatCookies(cookies)}🍪";
        }

        public void HideStageClear()
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
        }

        public void ShowGameClear(long cookies)
        {
            if (_gameClearPanel) _gameClearPanel.SetActive(true);
            if (_gameClearScoreText) _gameClearScoreText.text = $"総生産量: {FormatCookies(cookies)}🍪";
        }

        public void HideGameClear()
        {
            if (_gameClearPanel) _gameClearPanel.SetActive(false);
        }

        public void ShowFloatingText(string text)
        {
            if (_floatingTextPrefab == null || _floatingTextParent == null) return;
            StartCoroutine(FloatText(text));
        }

        IEnumerator FloatText(string text)
        {
            var go = Instantiate(_floatingTextPrefab, _floatingTextParent);
            go.text = text;
            go.gameObject.SetActive(true);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(Random.Range(-80f, 80f), 0f);
            float dur = 1.2f;
            for (float t = 0; t < dur; t += Time.deltaTime)
            {
                float ratio = t / dur;
                rt.anchoredPosition += Vector2.up * Time.deltaTime * 120f;
                go.color = new Color(1f, 1f, 0.3f, 1f - ratio);
                go.transform.localScale = Vector3.one * (1f + ratio * 0.5f);
                yield return null;
            }
            Destroy(go.gameObject);
        }
    }
}
