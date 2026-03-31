using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game033_AimSniper
{
    public class AimSniperUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _hitsText;
        [SerializeField] private TextMeshProUGUI _missesText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private AimSniperGameManager _gameManager;

        private void Awake() { if (_retryButton != null) _retryButton.onClick.AddListener(() => { if (_gameManager != null) _gameManager.RestartGame(); }); }

        public void UpdateHits(int h) { if (_hitsText != null) _hitsText.text = $"命中: {h}"; }
        public void UpdateMisses(int m) { if (_missesText != null) _missesText.text = $"ミス: {m}"; }
        public void UpdateTime(float t) { if (_timeText != null) _timeText.text = $"残り: {Mathf.CeilToInt(Mathf.Max(t,0))}秒"; }

        public void ShowResultPanel(int hits, int misses)
        {
            if (_resultPanel != null) _resultPanel.SetActive(true);
            float accuracy = (hits + misses) > 0 ? (float)hits / (hits + misses) * 100f : 0f;
            if (_resultText != null) _resultText.text = $"タイムアップ!\n命中: {hits} / ミス: {misses}\n精度: {accuracy:F0}%";
        }

        public void HideResultPanel() { if (_resultPanel != null) _resultPanel.SetActive(false); }
    }
}
