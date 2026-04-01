using UnityEngine;
using UnityEngine.InputSystem;

namespace Game061_CookieFactory
{
    public class FactoryManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private CookieFactoryGameManager _gameManager;
        [SerializeField, Tooltip("クッキースプライト")] private Sprite _cookieSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _ovenLevel;
        private int _conveyorLevel;
        private float _autoTimer;
        private float _autoInterval = 1f;

        // Upgrade costs
        private int OvenCost => 10 + _ovenLevel * 15;
        private int ConveyorCost => 25 + _conveyorLevel * 20;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _ovenLevel = 0;
            _conveyorLevel = 0;
            _autoTimer = 0f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Handle tap to bake
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                // Tap anywhere in game area (not UI) to bake
                if (wp.y < 2f && wp.y > -4f)
                {
                    _gameManager.OnCookieBaked();
                    SpawnCookieEffect(wp);
                }
            }

            // Auto production timer
            if (_ovenLevel > 0)
            {
                _autoTimer += Time.deltaTime;
            }
        }

        private void SpawnCookieEffect(Vector2 pos)
        {
            var obj = new GameObject("CookieFX");
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _cookieSprite; sr.sortingOrder = 10;
            obj.transform.localScale = Vector3.one * 0.4f;
            Destroy(obj, 0.3f);
        }

        public void UpgradeOven()
        {
            if (_gameManager.TrySpend(OvenCost))
            {
                _ovenLevel++;
            }
        }

        public void UpgradeConveyor()
        {
            if (_gameManager.TrySpend(ConveyorCost))
            {
                _conveyorLevel++;
            }
        }

        public int AutoProduction
        {
            get
            {
                if (_ovenLevel <= 0) return 0;
                if (_autoTimer >= _autoInterval)
                {
                    _autoTimer -= _autoInterval;
                    return _ovenLevel + _conveyorLevel * 2;
                }
                return 0;
            }
        }

        public int OvenLv => _ovenLevel;
        public int ConveyorLv => _conveyorLevel;
        public int NextOvenCost => OvenCost;
        public int NextConveyorCost => ConveyorCost;
    }
}
