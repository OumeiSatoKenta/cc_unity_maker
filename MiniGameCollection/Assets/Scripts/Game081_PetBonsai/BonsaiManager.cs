using UnityEngine;
using UnityEngine.InputSystem;

namespace Game081_PetBonsai
{
    public class BonsaiManager : MonoBehaviour
    {
        [SerializeField, Tooltip("盆栽スプライト")] private Sprite _bonsaiSprite;
        [SerializeField, Tooltip("鉢スプライト")] private Sprite _potSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _beauty;
        private int _growthLevel;
        private float _waterLevel;
        private float _growTimer;
        private float _waterDecayTimer;
        private GameObject _bonsaiObj;
        private SpriteRenderer _bonsaiSr;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _beauty = 0; _growthLevel = 1; _waterLevel = 0.5f;
            _growTimer = 0f; _waterDecayTimer = 0f;

            // Pot
            var potObj = new GameObject("Pot");
            potObj.transform.position = new Vector3(0f, -2.5f, 0f);
            var psr = potObj.AddComponent<SpriteRenderer>();
            psr.sprite = _potSprite; psr.sortingOrder = 2;

            // Bonsai
            _bonsaiObj = new GameObject("Bonsai");
            _bonsaiObj.transform.position = new Vector3(0f, -0.5f, 0f);
            _bonsaiSr = _bonsaiObj.AddComponent<SpriteRenderer>();
            _bonsaiSr.sprite = _bonsaiSprite; _bonsaiSr.sortingOrder = 3;
            UpdateBonsaiVisual();
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Water decay
            _waterDecayTimer += Time.deltaTime;
            if (_waterDecayTimer >= 2f)
            {
                _waterDecayTimer = 0f;
                _waterLevel = Mathf.Max(0f, _waterLevel - 0.05f);
            }

            // Growth (needs water)
            if (_waterLevel > 0.2f)
            {
                _growTimer += Time.deltaTime;
                if (_growTimer >= 3f)
                {
                    _growTimer = 0f;
                    _growthLevel++;
                    _beauty += 5 + _growthLevel;
                    UpdateBonsaiVisual();
                }
            }
        }

        public void Water()
        {
            _waterLevel = Mathf.Min(1f, _waterLevel + 0.3f);
            _beauty += 2;
            // Visual splash
            if (_bonsaiSr != null) _bonsaiSr.color = new Color(0.7f, 1f, 0.7f);
            Invoke(nameof(ResetColor), 0.2f);
        }

        public void Prune()
        {
            _beauty += 10;
            _growthLevel = Mathf.Max(1, _growthLevel - 1);
            UpdateBonsaiVisual();
            // Visual prune effect
            if (_bonsaiSr != null) _bonsaiSr.color = new Color(1f, 1f, 0.7f);
            Invoke(nameof(ResetColor), 0.2f);
        }

        private void ResetColor()
        {
            if (_bonsaiSr != null) _bonsaiSr.color = Color.white;
        }

        private void UpdateBonsaiVisual()
        {
            if (_bonsaiObj == null) return;
            float scale = 0.8f + _growthLevel * 0.1f;
            _bonsaiObj.transform.localScale = Vector3.one * Mathf.Min(scale, 2f);
        }

        public int Beauty => _beauty;
        public int GrowthLevel => _growthLevel;
        public float WaterLevel => _waterLevel;
    }
}
