using UnityEngine;

namespace Game090_StarshipCrew
{
    public class StarshipCrewGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private CrewManager _crewManager;
        [SerializeField, Tooltip("UI管理")] private StarshipCrewUI _ui;
        [SerializeField, Tooltip("全ミッション数")] private int _totalMissions = 5;

        private int _coins;
        private bool _isPlaying;

        private void Start()
        {
            _coins = 10;
            _isPlaying = true;
            _crewManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            int autoCoins = _crewManager.AutoIncome;
            if (autoCoins > 0) { _coins += autoCoins; UpdateUI(); }

            if (_crewManager.CompletedMissions >= _totalMissions)
            {
                _isPlaying = false;
                _crewManager.StopGame();
                _ui.ShowClear(_crewManager.CompletedMissions, _crewManager.CrewCount);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateCoins(_coins);
            _ui.UpdateMissions(_crewManager.CompletedMissions, _totalMissions);
            _ui.UpdateCrew(_crewManager.CrewCount);
        }

        public bool TrySpend(int cost)
        {
            if (_coins >= cost) { _coins -= cost; UpdateUI(); return true; }
            return false;
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
