using UnityEngine;

namespace Game067_TapDojo
{
    public class TapDojoGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private DojoManager _dojoManager;
        [SerializeField, Tooltip("UI管理")] private TapDojoUI _ui;
        [SerializeField, Tooltip("最高段位")] private int _maxRank = 5;

        private int _trainingPoints;
        private int _currentRank; // 0=白帯 ~ 5=黒帯
        private bool _isPlaying;

        private static readonly string[] RankNames = { "白帯", "黄帯", "緑帯", "青帯", "茶帯", "黒帯" };

        private void Start()
        {
            _trainingPoints = 0;
            _currentRank = 0;
            _isPlaying = true;
            _dojoManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            int autoPoints = _dojoManager.AutoTrain;
            if (autoPoints > 0)
            {
                _trainingPoints += autoPoints;
                CheckRankUp();
                UpdateUI();
            }
        }

        public void OnTap()
        {
            if (!_isPlaying) return;
            int gain = 1 + _currentRank;
            _trainingPoints += gain;
            CheckRankUp();
            UpdateUI();
        }

        private void CheckRankUp()
        {
            int nextRankCost = NextRankCost;
            while (_currentRank < _maxRank && _trainingPoints >= nextRankCost)
            {
                _trainingPoints -= nextRankCost;
                _currentRank++;
                nextRankCost = NextRankCost;

                if (_currentRank >= _maxRank)
                {
                    _isPlaying = false;
                    _dojoManager.StopGame();
                    _ui.ShowClear(RankNames[_currentRank]);
                    return;
                }
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateRank(RankNames[_currentRank]);
            _ui.UpdatePoints(_trainingPoints, NextRankCost);
            _ui.UpdateTechnique(_dojoManager.TechniqueLevel, _dojoManager.NextTechniqueCost);
        }

        public bool TrySpend(int cost)
        {
            if (_trainingPoints >= cost) { _trainingPoints -= cost; UpdateUI(); return true; }
            return false;
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int TrainingPoints => _trainingPoints;
        private int NextRankCost => 30 + _currentRank * 40;
    }
}
