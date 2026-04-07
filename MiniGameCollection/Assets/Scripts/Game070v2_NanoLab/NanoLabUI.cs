using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game070v2_NanoLab
{
    public class NanoLabUI : MonoBehaviour
    {
        [SerializeField] NanoMachineManager _nanoManager;
        [SerializeField] TextMeshProUGUI _stageText;
        [SerializeField] TextMeshProUGUI _nanoCountText;
        [SerializeField] TextMeshProUGUI _eraText;
        [SerializeField] TextMeshProUGUI _autoRateText;
        [SerializeField] TextMeshProUGUI _prestigeMultText;
        [SerializeField] TextMeshProUGUI _scoreText;
        [SerializeField] Transform _techNodeContainer;
        [SerializeField] GameObject _techNodeButtonPrefab;
        [SerializeField] Button _prestigeButton;
        [SerializeField] TextMeshProUGUI _prestigeButtonText;
        [SerializeField] GameObject _stageClearPanel;
        [SerializeField] TextMeshProUGUI _stageClearText;
        [SerializeField] GameObject _allClearPanel;
        [SerializeField] TextMeshProUGUI _allClearScoreText;
        [SerializeField] Image _flashOverlay;
        [SerializeField] GameObject _mutationNotification;
        [SerializeField] TextMeshProUGUI _mutationText;

        readonly Dictionary<string, GameObject> _techButtons = new();
        Coroutine _eraFlashCoroutine;
        Coroutine _hideMutCoroutine;

        public void UpdateStage(int stage, int total)
        {
            if (_stageText) _stageText.text = $"Stage {stage} / {total}";
        }

        public void UpdateNanoCount(long count)
        {
            if (_nanoCountText) _nanoCountText.text = $"ナノマシン: {FormatNumber(count)}";
        }

        public void UpdateEra(int era, string eraName)
        {
            if (_eraText) _eraText.text = $"現在: {eraName}";
            if (_eraFlashCoroutine != null) StopCoroutine(_eraFlashCoroutine);
            _eraFlashCoroutine = StartCoroutine(EraFlash());
        }

        public void UpdateAutoRate(float rate)
        {
            if (_autoRateText)
            {
                if (rate > 0)
                    _autoRateText.text = $"自動: {rate:F1}/秒";
                else
                    _autoRateText.text = "自動: なし";
            }
        }

        public void UpdatePrestigeMultiplier(float mult)
        {
            if (_prestigeMultText) _prestigeMultText.text = $"倍率: ×{mult:F1}";
        }

        public void UpdateScore(long score)
        {
            if (_scoreText) _scoreText.text = $"スコア: {FormatNumber(score)}";
        }

        public void ClearTechNodes()
        {
            foreach (var kv in _techButtons)
                if (kv.Value != null) Destroy(kv.Value);
            _techButtons.Clear();
        }

        public void UpdateTechNodes(TechNodeData[] nodes, long nanoCount)
        {
            if (_techNodeContainer == null) return;

            foreach (var node in nodes)
            {
                if (!_techButtons.TryGetValue(node.id, out var btnObj))
                {
                    if (_techNodeButtonPrefab == null) continue;
                    btnObj = Instantiate(_techNodeButtonPrefab, _techNodeContainer);
                    btnObj.SetActive(true);
                    _techButtons[node.id] = btnObj;
                }

                var texts = btnObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = node.unlocked ? $"✓ {node.nameJP}" : node.nameJP;
                    texts[1].text = node.unlocked ? node.description : $"{node.description} ({FormatNumber(node.cost)})";
                }

                var btn = btnObj.GetComponent<Button>();
                if (btn)
                {
                    btn.interactable = !node.unlocked && node.available;
                    var nodeId = node.id;
                    btn.onClick.RemoveAllListeners();
                    if (!node.unlocked)
                    {
                        btn.onClick.AddListener(() => {
                            if (_nanoManager) _nanoManager.UnlockTech(nodeId);
                            if (btnObj != null) StartCoroutine(ButtonPulse(btnObj.transform));
                        });
                    }
                }

                var img = btnObj.GetComponent<Image>();
                if (img)
                {
                    if (node.unlocked) img.color = new Color(0.3f, 0.7f, 0.3f, 1f);
                    else if (node.available) img.color = new Color(0.6f, 0.3f, 0.8f, 1f);
                    else img.color = new Color(0.3f, 0.2f, 0.4f, 0.7f);
                }
            }
        }

        public void UpdatePrestigeButton(bool available, long cost)
        {
            if (_prestigeButton)
            {
                _prestigeButton.interactable = available;
                if (_prestigeButtonText)
                    _prestigeButtonText.text = available ? "時代進化！" : "時代進化（未解放）";
            }
        }

        public void ShowStageClear(int nextStage)
        {
            if (_stageClearPanel)
            {
                _stageClearPanel.SetActive(true);
                if (_stageClearText) _stageClearText.text = $"Stage {nextStage - 1} クリア！";
            }
        }

        public void ShowAllClear(long score)
        {
            if (_stageClearPanel) _stageClearPanel.SetActive(false);
            if (_allClearPanel)
            {
                _allClearPanel.SetActive(true);
                if (_allClearScoreText) _allClearScoreText.text = $"最終スコア: {FormatNumber(score)}";
            }
        }

        public void ShowMutationEvent(bool isPositive, string description)
        {
            if (_mutationNotification)
            {
                _mutationNotification.SetActive(true);
                if (_mutationText)
                {
                    _mutationText.text = description;
                    _mutationText.color = isPositive ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
                }
                if (_hideMutCoroutine != null) StopCoroutine(_hideMutCoroutine);
                _hideMutCoroutine = StartCoroutine(HideMutationAfterDelay(3f));
            }
        }

        IEnumerator HideMutationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_mutationNotification) _mutationNotification.SetActive(false);
        }

        IEnumerator EraFlash()
        {
            if (_flashOverlay == null) yield break;
            _flashOverlay.gameObject.SetActive(true);
            _flashOverlay.color = new Color(0.8f, 0.9f, 1f, 0.8f);
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(0.8f, 0f, t / 0.5f);
                _flashOverlay.color = new Color(0.8f, 0.9f, 1f, a);
                yield return null;
            }
            _flashOverlay.gameObject.SetActive(false);
        }

        IEnumerator ButtonPulse(Transform t)
        {
            if (t == null) yield break;
            Vector3 orig = t.localScale;
            float half = 0.1f;
            float elapsed = 0;
            while (elapsed < half)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(orig, orig * 1.3f, elapsed / half);
                yield return null;
            }
            elapsed = 0;
            while (elapsed < half)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(orig * 1.3f, orig, elapsed / half);
                yield return null;
            }
            if (t != null) t.localScale = orig;
        }

        static string FormatNumber(long n)
        {
            if (n >= 1_000_000_000) return $"{n / 1_000_000_000f:F1}B";
            if (n >= 1_000_000) return $"{n / 1_000_000f:F1}M";
            if (n >= 1_000) return $"{n / 1_000f:F1}K";
            return n.ToString();
        }
    }
}
