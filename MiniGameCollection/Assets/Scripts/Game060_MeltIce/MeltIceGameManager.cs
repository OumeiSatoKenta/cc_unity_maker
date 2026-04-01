using UnityEngine;

namespace Game060_MeltIce
{
    public class MeltIceGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private MirrorManager _mirrorManager;
        [SerializeField, Tooltip("UI管理")] private MeltIceUI _ui;
        [SerializeField, Tooltip("鏡配置上限")] private int _maxMirrors = 5;

        private int _mirrorsPlaced;
        private bool _isPlaying;

        private void Start()
        {
            _mirrorsPlaced = 0;
            _isPlaying = true;
            _mirrorManager.StartGame();
            _ui.UpdateMirrors(_maxMirrors - _mirrorsPlaced);
            _ui.UpdateIce(_mirrorManager.RemainingIce, _mirrorManager.TotalTargetIce);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _ui.UpdateIce(_mirrorManager.RemainingIce, _mirrorManager.TotalTargetIce);

            if (_mirrorManager.RemainingIce <= 0)
            {
                _isPlaying = false;
                _mirrorManager.StopGame();
                _ui.ShowClear(_mirrorsPlaced);
            }
        }

        public void OnMirrorPlaced()
        {
            if (!_isPlaying) return;
            _mirrorsPlaced++;
            _ui.UpdateMirrors(_maxMirrors - _mirrorsPlaced);

            if (_mirrorsPlaced >= _maxMirrors && _mirrorManager.RemainingIce > 0)
            {
                _isPlaying = false;
                _mirrorManager.StopGame();
                _ui.ShowGameOver();
            }
        }

        public void OnWrongIceMelted()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _mirrorManager.StopGame();
            _ui.ShowGameOver();
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int MirrorsRemaining => _maxMirrors - _mirrorsPlaced;
    }
}
