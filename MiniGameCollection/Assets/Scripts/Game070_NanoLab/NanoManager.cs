using UnityEngine;
using UnityEngine.InputSystem;

namespace Game070_NanoLab
{
    public class NanoManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private NanoLabGameManager _gameManager;
        [SerializeField, Tooltip("ナノボットスプライト")] private Sprite _nanobotSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _multiplierLevel;
        private int _unlockedTech;
        private float _autoTimer;

        private static readonly string[] TechNames = {
            "原子操作", "分子合成", "セル工学",
            "量子制御", "星間通信", "宇宙工学"
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _multiplierLevel = 0;
            _unlockedTech = 0;
            _autoTimer = 0f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _gameManager.OnTap();
            }

            if (_multiplierLevel > 0)
                _autoTimer += Time.deltaTime;
        }

        public void UpgradeMultiplier()
        {
            if (_gameManager.TrySpend(NextMultiplierCost))
            {
                _multiplierLevel++;
            }
        }

        public void Research()
        {
            if (_gameManager.TrySpend(NextResearchCost))
            {
                _unlockedTech++;
            }
        }

        public long AutoGenerate
        {
            get
            {
                if (_multiplierLevel <= 0) return 0;
                if (_autoTimer >= 1f)
                {
                    _autoTimer -= 1f;
                    return (long)_multiplierLevel * (_unlockedTech + 1);
                }
                return 0;
            }
        }

        public int TapBonus => _multiplierLevel + _unlockedTech * 2;
        public int MultiplierLevel => _multiplierLevel;
        public int UnlockedTech => _unlockedTech;
        public long NextMultiplierCost => 20L + _multiplierLevel * 15L;
        public long NextResearchCost => 50L + _unlockedTech * 80L;
    }
}
