using UnityEngine;
using UnityEngine.InputSystem;

namespace Game067_TapDojo
{
    public class DojoManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private TapDojoGameManager _gameManager;
        [SerializeField, Tooltip("格闘家スプライト")] private Sprite _fighterSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _techniqueLevel;
        private float _autoTimer;
        private GameObject _fighter;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _techniqueLevel = 0;
            _autoTimer = 0f;

            _fighter = new GameObject("Fighter");
            _fighter.transform.position = new Vector3(0f, -1f, 0f);
            var sr = _fighter.AddComponent<SpriteRenderer>();
            sr.sprite = _fighterSprite; sr.sortingOrder = 5;
            _fighter.transform.localScale = Vector3.one * 1.5f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Tap to train
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                if (wp.y < 3f && wp.y > -5f)
                {
                    _gameManager.OnTap();
                    // Punch animation
                    if (_fighter != null)
                    {
                        _fighter.transform.localScale = Vector3.one * 1.6f;
                        Invoke(nameof(ResetScale), 0.1f);
                    }
                }
            }

            // Auto training from techniques
            if (_techniqueLevel > 0)
            {
                _autoTimer += Time.deltaTime;
            }
        }

        private void ResetScale()
        {
            if (_fighter != null) _fighter.transform.localScale = Vector3.one * 1.5f;
        }

        public void LearnTechnique()
        {
            if (_gameManager.TrySpend(NextTechniqueCost))
            {
                _techniqueLevel++;
            }
        }

        public int AutoTrain
        {
            get
            {
                if (_techniqueLevel <= 0) return 0;
                if (_autoTimer >= 1f)
                {
                    _autoTimer -= 1f;
                    return _techniqueLevel * 2;
                }
                return 0;
            }
        }

        public int TechniqueLevel => _techniqueLevel;
        public int NextTechniqueCost => 15 + _techniqueLevel * 12;
    }
}
