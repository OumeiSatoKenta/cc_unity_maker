using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game016_LightSwitch
{
    public class LightManager : MonoBehaviour
    {
        [SerializeField] private int _gridSize = 5;
        [SerializeField] private float _cellSize = 1.2f;
        [SerializeField] private GameObject _bulbPrefab;

        private LightBulb[,] _grid;
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private LightSwitchGameManager _gameManager;
        private Camera _mainCamera;

        private Sprite _onSprite;
        private Sprite _offSprite;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<LightSwitchGameManager>();
            _mainCamera = Camera.main;
            _onSprite = Resources.Load<Sprite>("Sprites/Game016_LightSwitch/bulb_on");
            _offSprite = Resources.Load<Sprite>("Sprites/Game016_LightSwitch/bulb_off");
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _mainCamera == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector3 sp = mouse.position.ReadValue();
                sp.z = -_mainCamera.transform.position.z;
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(sp);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var bulb = hit.GetComponent<LightBulb>();
                    if (bulb != null)
                    {
                        ToggleCross(bulb.GridPosition);
                        if (_gameManager != null)
                        {
                            _gameManager.OnSwitchToggled();
                            if (CheckAllOff())
                                _gameManager.OnPuzzleSolved();
                        }
                    }
                }
            }
        }

        private void ToggleCross(Vector2Int center)
        {
            Vector2Int[] positions = {
                center,
                center + Vector2Int.up,
                center + Vector2Int.down,
                center + Vector2Int.left,
                center + Vector2Int.right
            };

            foreach (var pos in positions)
            {
                if (pos.x >= 0 && pos.x < _gridSize && pos.y >= 0 && pos.y < _gridSize)
                    _grid[pos.x, pos.y].Toggle();
            }
        }

        public bool CheckAllOff()
        {
            for (int x = 0; x < _gridSize; x++)
                for (int y = 0; y < _gridSize; y++)
                    if (_grid[x, y].IsOn) return false;
            return true;
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            var data = GetStageData(stageIndex);
            _gridSize = data.size;
            _grid = new LightBulb[_gridSize, _gridSize];
            BuildGrid(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
        }

        private void BuildGrid(StageData data)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_bulbPrefab == null) continue;
                    var obj = Instantiate(_bulbPrefab, transform);
                    var gp = new Vector2Int(x, y);
                    obj.transform.position = GridToWorld(gp);
                    obj.name = $"Bulb_{x}_{y}";

                    var bulb = obj.GetComponent<LightBulb>();
                    if (bulb != null)
                        bulb.Initialize(gp, data.initialOn.Contains(gp), _onSprite, _offSprite);

                    _grid[x, y] = bulb;
                    _stageObjects.Add(obj);
                }
            }
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offset = (_gridSize - 1) * _cellSize * 0.5f;
            return new Vector3(gridPos.x * _cellSize - offset, gridPos.y * _cellSize - offset, 0f);
        }

        #region Stage Data

        private struct StageData
        {
            public int size;
            public HashSet<Vector2Int> initialOn;
        }

        private StageData GetStageData(int index)
        {
            switch (index % StageCount)
            {
                case 0: return GetStage1();
                case 1: return GetStage2();
                case 2: return GetStage3();
                default: return GetStage1();
            }
        }

        private StageData GetStage1()
        {
            return new StageData
            {
                size = 3,
                initialOn = new HashSet<Vector2Int> {
                    new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2),
                }
            };
        }

        private StageData GetStage2()
        {
            return new StageData
            {
                size = 4,
                initialOn = new HashSet<Vector2Int> {
                    new Vector2Int(0, 1), new Vector2Int(1, 0),
                    new Vector2Int(2, 3), new Vector2Int(3, 2),
                    new Vector2Int(1, 2), new Vector2Int(2, 1),
                }
            };
        }

        private StageData GetStage3()
        {
            return new StageData
            {
                size = 5,
                initialOn = new HashSet<Vector2Int> {
                    new Vector2Int(0, 0), new Vector2Int(0, 4),
                    new Vector2Int(4, 0), new Vector2Int(4, 4),
                    new Vector2Int(2, 2),
                    new Vector2Int(1, 2), new Vector2Int(3, 2),
                    new Vector2Int(2, 1), new Vector2Int(2, 3),
                }
            };
        }

        #endregion
    }
}
