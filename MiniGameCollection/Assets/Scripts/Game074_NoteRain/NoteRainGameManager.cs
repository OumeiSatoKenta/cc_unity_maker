using UnityEngine;

namespace Game074_NoteRain
{
    public class NoteRainGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("コアメカニクス")] private RainManager _rainManager;
        [SerializeField, Tooltip("UI管理")] private NoteRainUI _ui;
        [SerializeField, Tooltip("全音符数")] private int _totalNotes = 20;
        [SerializeField, Tooltip("連続ミス上限")] private int _maxConsecutiveMisses = 3;

        private int _caught;
        private int _missed;
        private int _consecutiveMisses;
        private int _combo;
        private bool _isPlaying;

        private void Start()
        {
            _caught = 0; _missed = 0; _consecutiveMisses = 0; _combo = 0;
            _isPlaying = true;
            _rainManager.StartGame(_totalNotes);
            _ui.UpdateScore(_caught, _totalNotes);
            _ui.UpdateCombo(_combo);
        }

        public void OnNoteCaught()
        {
            if (!_isPlaying) return;
            _caught++;
            _combo++;
            _consecutiveMisses = 0;
            _ui.UpdateScore(_caught, _totalNotes);
            _ui.UpdateCombo(_combo);

            if (_caught >= _totalNotes)
            {
                _isPlaying = false;
                _rainManager.StopGame();
                _ui.ShowClear(_caught, _combo);
            }
        }

        public void OnNoteMissed()
        {
            if (!_isPlaying) return;
            _missed++;
            _combo = 0;
            _consecutiveMisses++;
            _ui.UpdateCombo(_combo);

            if (_consecutiveMisses >= _maxConsecutiveMisses)
            {
                _isPlaying = false;
                _rainManager.StopGame();
                _ui.ShowGameOver(_caught, _totalNotes);
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
