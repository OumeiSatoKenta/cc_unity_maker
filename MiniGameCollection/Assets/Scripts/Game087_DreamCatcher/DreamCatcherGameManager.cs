using UnityEngine;

namespace Game087_DreamCatcher
{
    public class DreamCatcherGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private DreamManager _dreamManager;
        [SerializeField, Tooltip("UI管理")] private DreamCatcherUI _ui;
        [SerializeField, Tooltip("全破片種類数")] private int _totalFragmentTypes = 6;

        private bool _isPlaying;

        private void Start()
        {
            _isPlaying = true;
            _dreamManager.StartGame();
            _ui.UpdateCollection(_dreamManager.CollectedTypes, _totalFragmentTypes);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _ui.UpdateCollection(_dreamManager.CollectedTypes, _totalFragmentTypes);
            _ui.UpdateFragments(_dreamManager.TotalFragments);

            if (_dreamManager.CollectedTypes >= _totalFragmentTypes)
            {
                _isPlaying = false;
                _dreamManager.StopGame();
                _ui.ShowClear(_dreamManager.CollectedTypes, _dreamManager.TotalFragments);
            }
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
