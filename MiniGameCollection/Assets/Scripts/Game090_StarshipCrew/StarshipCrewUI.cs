using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game090_StarshipCrew
{
    public class StarshipCrewUI : MonoBehaviour
    {
        [SerializeField, Tooltip("コイン")] private TextMeshProUGUI _coinText;
        [SerializeField, Tooltip("ミッション")] private TextMeshProUGUI _missionText;
        [SerializeField, Tooltip("クルー数")] private TextMeshProUGUI _crewText;
        [SerializeField, Tooltip("募集ボタン")] private Button _recruitButton;
        [SerializeField, Tooltip("募集テキスト")] private TextMeshProUGUI _recruitButtonText;
        [SerializeField, Tooltip("出撃ボタン")] private Button _missionButton;
        [SerializeField, Tooltip("出撃テキスト")] private TextMeshProUGUI _missionButtonText;
        [SerializeField, Tooltip("CrewManager")] private CrewManager _crewManager;
        [SerializeField, Tooltip("クリアパネル")] private GameObject _clearPanel;
        [SerializeField, Tooltip("クリアスコア")] private TextMeshProUGUI _clearScoreText;
        [SerializeField, Tooltip("クリアリトライ")] private Button _clearRetryButton;
        [SerializeField, Tooltip("メニュー")] private Button _menuButton;

        private void Update()
        {
            if (_crewManager == null) return;
            if (_recruitButtonText != null) _recruitButtonText.text = $"募集\n{_crewManager.NextRecruitCost}";
            if (_missionButtonText != null) _missionButtonText.text = $"出撃\n{_crewManager.NextMissionCost}";
        }

        public void UpdateCoins(int c) { if (_coinText != null) _coinText.text = $"コイン: {c}"; }
        public void UpdateMissions(int m, int total) { if (_missionText != null) _missionText.text = $"任務: {m}/{total}"; }
        public void UpdateCrew(int c) { if (_crewText != null) _crewText.text = $"隊員: {c}"; }
        public void ShowClear(int missions, int crew) { if (_clearPanel != null) _clearPanel.SetActive(true); if (_clearScoreText != null) _clearScoreText.text = $"全{missions}任務完了！ {crew}名"; }
    }
}
