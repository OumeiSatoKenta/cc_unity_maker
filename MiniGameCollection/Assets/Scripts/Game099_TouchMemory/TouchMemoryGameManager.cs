using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Game099_TouchMemory
{
    public enum GameState { Showing, Input, Clear, GameOver }

    public class TouchMemoryGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("メモリマネージャー")] private MemoryManager _memoryManager;
        [SerializeField, Tooltip("UI管理")] private TouchMemoryUI _ui;

        public const int MaxRound = 10;

        private int _round;
        private readonly List<int> _pattern = new List<int>();
        private int _inputIndex;
        private GameState _state;

        private void Start()
        {
            if (_memoryManager == null) { Debug.LogError("[TouchMemoryGM] _memoryManager が未アサイン"); return; }
            if (_ui == null) { Debug.LogError("[TouchMemoryGM] _ui が未アサイン"); return; }
            StartNewGame();
        }

        private void StartNewGame()
        {
            _round = 0;
            _pattern.Clear();
            _ui.HideClearPanel();
            _ui.HideGameOverPanel();
            NextRound();
        }

        private void NextRound()
        {
            _round++;
            _inputIndex = 0;
            _pattern.Add(Random.Range(0, 4));
            _state = GameState.Showing;
            _ui.UpdateRound(_round, MaxRound);
            _ui.UpdateStatus("見て覚えて！");
            _memoryManager.ShowPattern(_pattern);
        }

        public void OnPatternShowComplete()
        {
            _state = GameState.Input;
            _inputIndex = 0;
            _ui.UpdateStatus("タップして！");
        }

        public void OnPanelTapped(int index)
        {
            if (_state != GameState.Input) return;

            if (index == _pattern[_inputIndex])
            {
                _memoryManager.FlashPanel(index);
                _inputIndex++;
                if (_inputIndex >= _pattern.Count)
                {
                    if (_round >= MaxRound)
                    {
                        _state = GameState.Clear;
                        _ui.UpdateStatus("クリア！");
                        _ui.ShowClearPanel(_round);
                    }
                    else
                    {
                        _state = GameState.Showing;
                        _ui.UpdateStatus("正解！次へ…");
                        StartCoroutine(DelayNextRound());
                    }
                }
            }
            else
            {
                _state = GameState.GameOver;
                _memoryManager.FlashPanelError(index);
                _ui.UpdateStatus("残念…");
                _ui.ShowGameOverPanel(_round - 1);
            }
        }

        private System.Collections.IEnumerator DelayNextRound()
        {
            yield return new WaitForSeconds(0.8f);
            NextRound();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public GameState State => _state;
    }
}
