using UnityEngine;

namespace Game081_PetBonsai
{
    public class PetBonsaiGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private BonsaiManager _bonsaiManager;
        [SerializeField, Tooltip("UI管理")] private PetBonsaiUI _ui;
        [SerializeField, Tooltip("目標美しさ")] private int _targetBeauty = 100;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _bonsaiManager.StartGame();
            UpdateUI();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            UpdateUI();

            if (_bonsaiManager.Beauty >= _targetBeauty)
            {
                _isPlaying = false;
                _bonsaiManager.StopGame();
                _ui.ShowClear(_bonsaiManager.Beauty);
            }
        }

        private void UpdateUI()
        {
            _ui.UpdateBeauty(_bonsaiManager.Beauty, _targetBeauty);
            _ui.UpdateGrowth(_bonsaiManager.GrowthLevel);
            _ui.UpdateWater(_bonsaiManager.WaterLevel);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
