using UnityEngine;

namespace Game054_FruitSlash
{
    public class FruitSlashGameManager : MonoBehaviour
    {
        [SerializeField] private FruitManager _fruitManager;
        [SerializeField] private FruitSlashUI _ui;

        private int _score;
        private int _combo;
        private int _life;
        private bool _isGameOver;
        private const int MaxLife = 3;

        public bool IsGameOver => _isGameOver;

        private void Start() { StartGame(); }

        public void StartGame()
        {
            _score = 0; _combo = 0; _life = MaxLife; _isGameOver = false;
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateCombo(_combo); _ui.UpdateLife(_life); _ui.HideGameOverPanel(); }
            if (_fruitManager != null) _fruitManager.Init();
        }

        public void OnFruitSlashed()
        {
            if (_isGameOver) return;
            _combo++;
            _score += _combo;
            if (_ui != null) { _ui.UpdateScore(_score); _ui.UpdateCombo(_combo); }
        }

        public void OnFruitMissed()
        {
            if (_isGameOver) return;
            _combo = 0;
            if (_ui != null) _ui.UpdateCombo(_combo);
        }

        public void OnBombHit()
        {
            if (_isGameOver) return;
            _life--; _combo = 0;
            if (_ui != null) { _ui.UpdateLife(_life); _ui.UpdateCombo(_combo); }
            if (_life <= 0) { _isGameOver = true; if (_fruitManager != null) _fruitManager.StopSpawning(); if (_ui != null) _ui.ShowGameOverPanel(_score); }
        }
    }
}
