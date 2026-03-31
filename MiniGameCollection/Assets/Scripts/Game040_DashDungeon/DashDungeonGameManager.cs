using UnityEngine;
namespace Game040_DashDungeon
{
    public class DashDungeonGameManager : MonoBehaviour
    {
        [SerializeField] private DungeonManager _dungeonManager;
        [SerializeField] private DashDungeonUI _ui;
        private int _coins; private float _elapsed; private bool _isComplete;
        private void Start() { StartGame(); }
        public void StartGame() { _coins = 0; _elapsed = 0; _isComplete = false; if (_dungeonManager != null) _dungeonManager.StartGame(); if (_ui != null) { _ui.UpdateCoins(0); _ui.UpdateTime(0); _ui.HideResultPanel(); } }
        private void Update() { if (!_isComplete) { _elapsed += Time.deltaTime; if (_ui != null) _ui.UpdateTime(_elapsed); } }
        public void OnCoinCollected() { _coins++; if (_ui != null) _ui.UpdateCoins(_coins); }
        public void OnLevelComplete() { _isComplete = true; if (_dungeonManager != null) _dungeonManager.StopGame(); if (_ui != null) _ui.ShowResultPanel(_coins, _elapsed); }
        public void RestartGame() { StartGame(); }
    }
}
