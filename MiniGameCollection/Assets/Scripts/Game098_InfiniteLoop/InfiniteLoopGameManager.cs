using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game098_InfiniteLoop
{
    public class InfiniteLoopGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ループマネージャー")] private LoopManager _loopManager;
        [SerializeField, Tooltip("UI管理")] private InfiniteLoopUI _ui;

        private int _loopCount;
        private int _foundCount;
        private bool _isPlaying;

        public const int TotalChanges = 5;

        private void Start()
        {
            if (_loopManager == null) { Debug.LogError("[InfiniteLoopGM] _loopManager が未アサイン"); return; }
            if (_ui == null) { Debug.LogError("[InfiniteLoopGM] _ui が未アサイン"); return; }
            StartNewGame();
        }

        private void StartNewGame()
        {
            _loopCount = 1;
            _foundCount = 0;
            _isPlaying = true;
            _ui.HideClearPanel();
            UpdateDisplay();
            _loopManager.SetupRoom(_foundCount);
        }

        public void OnCorrectTap()
        {
            if (!_isPlaying) return;
            _foundCount++;
            _loopCount++;
            UpdateDisplay();

            if (_foundCount >= TotalChanges)
            {
                _isPlaying = false;
                _ui.ShowClearPanel(_loopCount);
                return;
            }

            _loopManager.PlayLoopTransition(_foundCount);
        }

        public void OnWrongTap()
        {
            if (!_isPlaying) return;
            _loopCount++;
            UpdateDisplay();
            _ui.ShowHint("違う…何かが変わったはず…");
            _loopManager.ShakeCamera();
        }

        private void UpdateDisplay()
        {
            _ui.UpdateLoopCount(_loopCount);
            _ui.UpdateStage(_foundCount, TotalChanges);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
