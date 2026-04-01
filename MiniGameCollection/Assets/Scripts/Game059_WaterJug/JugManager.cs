using UnityEngine;
using UnityEngine.InputSystem;

namespace Game059_WaterJug
{
    public class JugManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private WaterJugGameManager _gameManager;
        [SerializeField, Tooltip("ジャグスプライト")] private Sprite _jugSprite;
        [SerializeField, Tooltip("水スプライト")] private Sprite _waterSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int[] _capacity;
        private int[] _current;
        private int _targetAmount = 4;
        private int _selectedJug = -1;
        private GameObject[] _jugObjects;
        private GameObject[] _waterObjects;
        private SpriteRenderer[] _jugRenderers;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _capacity = new int[] { 3, 5, 8 };
            _current = new int[] { 0, 0, 8 };
            _targetAmount = 4;

            _jugObjects = new GameObject[3];
            _waterObjects = new GameObject[3];
            _jugRenderers = new SpriteRenderer[3];

            for (int i = 0; i < 3; i++)
            {
                float x = -2.5f + i * 2.5f;
                var obj = new GameObject($"Jug_{i}");
                obj.transform.position = new Vector3(x, -1f, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _jugSprite; sr.sortingOrder = 2;
                float scale = 0.5f + _capacity[i] * 0.08f;
                obj.transform.localScale = new Vector3(scale, scale, 1f);
                var col = obj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.6f, 0.9f);
                _jugObjects[i] = obj;
                _jugRenderers[i] = sr;

                // Water fill visual
                var wObj = new GameObject($"Water_{i}");
                wObj.transform.SetParent(obj.transform);
                wObj.transform.localPosition = new Vector3(0f, -0.2f, -0.01f);
                var wsr = wObj.AddComponent<SpriteRenderer>();
                wsr.sprite = _waterSprite; wsr.sortingOrder = 1;
                wsr.color = new Color(0.3f, 0.5f, 0.9f, 0.7f);
                _waterObjects[i] = wObj;

                // Label
                var tObj = new GameObject("Label");
                tObj.transform.SetParent(obj.transform);
                tObj.transform.localPosition = new Vector3(0f, -0.7f, -0.1f);
                var tm = tObj.AddComponent<TextMesh>();
                tm.fontSize = 36; tm.characterSize = 0.12f;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = Color.white;
            }

            UpdateVisuals();
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (_jugObjects[i] == hit.gameObject)
                        {
                            HandleJugClick(i);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleJugClick(int index)
        {
            if (_selectedJug < 0)
            {
                if (_current[index] > 0)
                {
                    _selectedJug = index;
                    _jugRenderers[index].color = Color.yellow;
                }
            }
            else if (_selectedJug == index)
            {
                _jugRenderers[index].color = Color.white;
                _selectedJug = -1;
            }
            else
            {
                // Pour from selected to index
                _jugRenderers[_selectedJug].color = Color.white;
                Pour(_selectedJug, index);
                _selectedJug = -1;
                UpdateVisuals();
                _gameManager.OnMovePerformed();
            }
        }

        private void Pour(int from, int to)
        {
            int space = _capacity[to] - _current[to];
            int amount = Mathf.Min(_current[from], space);
            _current[from] -= amount;
            _current[to] += amount;
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < 3; i++)
            {
                float ratio = _capacity[i] > 0 ? (float)_current[i] / _capacity[i] : 0f;
                _waterObjects[i].transform.localScale = new Vector3(0.7f, ratio * 0.6f, 1f);

                var tm = _jugObjects[i].GetComponentInChildren<TextMesh>();
                if (tm != null) tm.text = $"{_current[i]}/{_capacity[i]}";
            }
        }

        public bool IsSolved => _current[0] == _targetAmount || _current[1] == _targetAmount || _current[2] == _targetAmount;

        public string GetJugStates()
        {
            return $"{_current[0]}/{_capacity[0]}  {_current[1]}/{_capacity[1]}  {_current[2]}/{_capacity[2]}  目標: {_targetAmount}";
        }
    }
}
