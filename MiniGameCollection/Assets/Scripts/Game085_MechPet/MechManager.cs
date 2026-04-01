using UnityEngine;
using UnityEngine.InputSystem;

namespace Game085_MechPet
{
    public class MechManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private MechPetGameManager _gameManager;
        [SerializeField, Tooltip("ボディスプライト")] private Sprite _bodySprite;
        [SerializeField, Tooltip("ヘッドスプライト")] private Sprite _headSprite;
        [SerializeField, Tooltip("アームスプライト")] private Sprite _armSprite;
        [SerializeField, Tooltip("レッグスプライト")] private Sprite _legSprite;

        private bool _isActive;
        private int _headLevel, _bodyLevel, _armLevel, _legLevel;
        private float _incomeTimer;
        private GameObject _mechObj;

        public void StartGame()
        {
            _isActive = true;
            _headLevel = 1; _bodyLevel = 1; _armLevel = 1; _legLevel = 1;
            _incomeTimer = 0f;

            _mechObj = new GameObject("Mech");
            _mechObj.transform.position = Vector3.zero;

            var body = CreatePart("Body", _bodySprite, Vector3.zero, 0.8f, 3);
            var head = CreatePart("Head", _headSprite, new Vector3(0f, 0.8f, 0f), 0.6f, 4);
            var armL = CreatePart("ArmL", _armSprite, new Vector3(-0.6f, 0.1f, 0f), 0.5f, 2);
            var armR = CreatePart("ArmR", _armSprite, new Vector3(0.6f, 0.1f, 0f), 0.5f, 2);
            var legL = CreatePart("LegL", _legSprite, new Vector3(-0.25f, -0.7f, 0f), 0.5f, 2);
            var legR = CreatePart("LegR", _legSprite, new Vector3(0.25f, -0.7f, 0f), 0.5f, 2);
        }

        private GameObject CreatePart(string name, Sprite sprite, Vector3 localPos, float scale, int order)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(_mechObj.transform);
            obj.transform.localPosition = localPos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.sortingOrder = order;
            obj.transform.localScale = Vector3.one * scale;
            return obj;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            _incomeTimer += Time.deltaTime;

            // Idle animation
            if (_mechObj != null)
            {
                float bob = Mathf.Sin(Time.time * 2f) * 0.05f;
                _mechObj.transform.position = new Vector3(0f, bob, 0f);
            }

            // Tap for coins
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _gameManager.TrySpend(-1); // Negative = gain 1 coin via tap
            }
        }

        public void Upgrade()
        {
            if (_gameManager.TrySpend(NextUpgradeCost))
            {
                // Upgrade weakest part
                int min = Mathf.Min(_headLevel, _bodyLevel, _armLevel, _legLevel);
                if (_headLevel == min) _headLevel++;
                else if (_bodyLevel == min) _bodyLevel++;
                else if (_armLevel == min) _armLevel++;
                else _legLevel++;
            }
        }

        public int AutoIncome
        {
            get
            {
                if (_incomeTimer >= 2f)
                {
                    _incomeTimer -= 2f;
                    return TotalPower / 5;
                }
                return 0;
            }
        }

        public int TotalPower => _headLevel * 3 + _bodyLevel * 4 + _armLevel * 2 + _legLevel * 2;
        public int NextUpgradeCost => 8 + (TotalPower / 3);
    }
}
