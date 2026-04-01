using UnityEngine;
using UnityEngine.InputSystem;

namespace Game069_DungeonDigger
{
    public class DigManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private DungeonDiggerGameManager _gameManager;
        [SerializeField, Tooltip("ドリルスプライト")] private Sprite _drillSprite;
        [SerializeField, Tooltip("土スプライト")] private Sprite _dirtSprite;
        [SerializeField, Tooltip("宝石スプライト")] private Sprite _gemSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _drillLevel;
        private float _autoTimer;
        private GameObject _drillObj;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _drillLevel = 0;
            _autoTimer = 0f;

            // Create dirt blocks visual
            for (int i = 0; i < 8; i++)
            {
                float x = Random.Range(-3.5f, 3.5f);
                float y = Random.Range(-4f, 1f);
                var obj = new GameObject($"Dirt_{i}");
                obj.transform.position = new Vector3(x, y, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _dirtSprite; sr.sortingOrder = 1;
                float scale = Random.Range(0.4f, 0.8f);
                obj.transform.localScale = Vector3.one * scale;
                sr.color = new Color(
                    Random.Range(0.8f, 1f),
                    Random.Range(0.7f, 0.9f),
                    Random.Range(0.6f, 0.8f));
            }

            // Drill visual
            _drillObj = new GameObject("Drill");
            _drillObj.transform.position = new Vector3(0f, 2f, 0f);
            var dsr = _drillObj.AddComponent<SpriteRenderer>();
            dsr.sprite = _drillSprite; dsr.sortingOrder = 5;
            _drillObj.transform.localScale = Vector3.one * 1.2f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _gameManager.OnDig();
                // Drill animation
                if (_drillObj != null)
                {
                    _drillObj.transform.position = new Vector3(0f, 1.5f, 0f);
                    Invoke(nameof(ResetDrill), 0.1f);
                }
            }

            if (_drillLevel > 0) _autoTimer += Time.deltaTime;
        }

        private void ResetDrill()
        {
            if (_drillObj != null) _drillObj.transform.position = new Vector3(0f, 2f, 0f);
        }

        public void UpgradeDrill()
        {
            if (_gameManager.TrySpend(NextDrillCost))
            {
                _drillLevel++;
            }
        }

        public int AutoDig
        {
            get
            {
                if (_drillLevel <= 0) return 0;
                if (_autoTimer >= 1.5f)
                {
                    _autoTimer -= 1.5f;
                    return _drillLevel;
                }
                return 0;
            }
        }

        public int DrillLevel => _drillLevel;
        public int NextDrillCost => 10 + _drillLevel * 8;
    }
}
