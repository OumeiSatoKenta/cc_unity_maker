using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game094_GravityPainter
{
    public class GravityPainterGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private PaintManager _paintManager;
        [SerializeField, Tooltip("UI管理")] private GravityPainterUI _ui;
        [SerializeField, Tooltip("最大操作回数")] private int _maxMoves = 8;
        [SerializeField, Tooltip("クリア一致率閾値")] private float _clearThreshold = 0.6f;

        private int _movesUsed;
        private bool _isPlaying;

        private void Start()
        {
            if (_paintManager == null) { Debug.LogError("[GravityPainterGameManager] _paintManager が未アサイン"); return; }
            if (_ui == null) { Debug.LogError("[GravityPainterGameManager] _ui が未アサイン"); return; }
            _movesUsed = 0;
            _isPlaying = true;
            _paintManager.StartGame();
            _ui.UpdateMatch(0f);
            _ui.UpdateMoves(_maxMoves - _movesUsed);
        }

        public void OnPaintDropped()
        {
            if (!_isPlaying) return;
            _movesUsed++;
            float rate = _paintManager.CalculateMatchRate();
            _ui.UpdateMatch(rate);
            _ui.UpdateMoves(_maxMoves - _movesUsed);
            if (_movesUsed >= _maxMoves)
                CheckResult(rate);
        }

        private void CheckResult(float rate)
        {
            _isPlaying = false;
            _paintManager.StopGame();
            if (rate >= _clearThreshold)
            {
                int stars = rate >= 0.9f ? 3 : rate >= 0.7f ? 2 : 1;
                _ui.ShowClear(rate, stars);
            }
            else
            {
                _ui.ShowGameOver(rate);
            }
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
