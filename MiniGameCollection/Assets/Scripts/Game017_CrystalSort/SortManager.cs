using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game017_CrystalSort
{
    public class SortManager : MonoBehaviour
    {
        [SerializeField] private GameObject _crystalPrefab;
        [SerializeField] private GameObject _bottlePrefab;

        private readonly List<CrystalItem> _crystals = new List<CrystalItem>();
        private readonly List<Bottle> _bottles = new List<Bottle>();
        private readonly List<GameObject> _stageObjects = new List<GameObject>();

        private CrystalSortGameManager _gameManager;
        private Camera _mainCamera;
        private CrystalItem _selectedCrystal;

        private static readonly string[] ColorNames = { "red", "green", "blue", "yellow" };

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<CrystalSortGameManager>();
            _mainCamera = Camera.main;
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
                if (hit == null) return;

                // If crystal clicked, select it
                var crystal = hit.GetComponent<CrystalItem>();
                if (crystal != null && !crystal.IsSorted)
                {
                    _selectedCrystal = crystal;
                    // Highlight
                    crystal.transform.localScale = Vector3.one * 1.3f;
                    return;
                }

                // If bottle clicked and crystal selected, try to sort
                var bottle = hit.GetComponent<Bottle>();
                if (bottle != null && _selectedCrystal != null)
                {
                    if (_selectedCrystal.ColorIndex == bottle.AcceptColorIndex && bottle.TryAdd())
                    {
                        _selectedCrystal.MarkSorted();
                        _selectedCrystal = null;

                        if (_gameManager != null)
                        {
                            _gameManager.OnCrystalSorted();
                            if (CheckAllSorted())
                                _gameManager.OnPuzzleSolved();
                        }
                    }
                    else
                    {
                        // Wrong bottle - deselect
                        if (_selectedCrystal != null)
                            _selectedCrystal.transform.localScale = Vector3.one;
                        _selectedCrystal = null;
                        if (_gameManager != null) _gameManager.OnMiss();
                    }
                }
            }
        }

        public bool CheckAllSorted()
        {
            foreach (var c in _crystals)
                if (!c.IsSorted) return false;
            return true;
        }

        public void SetupStage(int stageIndex)
        {
            ClearStage();
            _selectedCrystal = null;
            var data = GetStageData(stageIndex);
            BuildStage(data);
        }

        private void ClearStage()
        {
            foreach (var obj in _stageObjects)
                if (obj != null) Destroy(obj);
            _stageObjects.Clear();
            _crystals.Clear();
            _bottles.Clear();
        }

        private void BuildStage(StageData data)
        {
            // Create bottles at bottom
            float bottleSpacing = 2.0f;
            float bottleStartX = -(data.colorCount - 1) * bottleSpacing * 0.5f;
            for (int i = 0; i < data.colorCount; i++)
            {
                if (_bottlePrefab == null) continue;
                var obj = Instantiate(_bottlePrefab, transform);
                obj.transform.position = new Vector3(bottleStartX + i * bottleSpacing, -2f, 0f);
                obj.name = $"Bottle_{ColorNames[i]}";

                // Color tint on bottle
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    var sprite = Resources.Load<Sprite>($"Sprites/Game017_CrystalSort/crystal_{ColorNames[i]}");
                    // Keep bottle sprite, just tint slightly
                }

                var bottle = obj.GetComponent<Bottle>();
                if (bottle != null)
                    bottle.Initialize(i, data.crystalsPerColor);
                _bottles.Add(bottle);
                _stageObjects.Add(obj);
            }

            // Create crystals scattered above
            var rand = new System.Random(data.seed);
            var positions = new List<Vector2>();
            float crystalAreaWidth = (data.colorCount + 1) * 1.5f;
            for (int c = 0; c < data.colorCount; c++)
            {
                for (int j = 0; j < data.crystalsPerColor; j++)
                {
                    float x = (float)(rand.NextDouble() * crystalAreaWidth - crystalAreaWidth * 0.5f);
                    float y = (float)(rand.NextDouble() * 3f + 0.5f);
                    positions.Add(new Vector2(x, y));
                }
            }

            // Shuffle positions
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (positions[i], positions[j]) = (positions[j], positions[i]);
            }

            int idx = 0;
            for (int c = 0; c < data.colorCount; c++)
            {
                for (int j = 0; j < data.crystalsPerColor; j++)
                {
                    if (_crystalPrefab == null) continue;
                    var obj = Instantiate(_crystalPrefab, transform);
                    obj.transform.position = new Vector3(positions[idx].x, positions[idx].y, 0f);
                    obj.name = $"Crystal_{ColorNames[c]}_{j}";

                    var sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        var sprite = Resources.Load<Sprite>($"Sprites/Game017_CrystalSort/crystal_{ColorNames[c]}");
                        if (sprite != null) sr.sprite = sprite;
                    }

                    var crystal = obj.GetComponent<CrystalItem>();
                    if (crystal != null) crystal.Initialize(c);
                    _crystals.Add(crystal);
                    _stageObjects.Add(obj);
                    idx++;
                }
            }
        }

        #region Stage Data

        private struct StageData
        {
            public int colorCount;
            public int crystalsPerColor;
            public int seed;
        }

        private StageData GetStageData(int index)
        {
            switch (index % StageCount)
            {
                case 0: return new StageData { colorCount = 2, crystalsPerColor = 3, seed = 42 };
                case 1: return new StageData { colorCount = 3, crystalsPerColor = 3, seed = 123 };
                case 2: return new StageData { colorCount = 4, crystalsPerColor = 4, seed = 456 };
                default: return new StageData { colorCount = 2, crystalsPerColor = 3, seed = 42 };
            }
        }

        #endregion
    }
}
