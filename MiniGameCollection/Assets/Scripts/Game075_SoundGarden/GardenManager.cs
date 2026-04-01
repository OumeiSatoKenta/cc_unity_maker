using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game075_SoundGarden
{
    public class GardenManager : MonoBehaviour
    {
        [SerializeField, Tooltip("花スプライト")] private Sprite _flowerSprite;
        [SerializeField, Tooltip("キノコスプライト")] private Sprite _mushroomSprite;
        [SerializeField, Tooltip("クリスタルスプライト")] private Sprite _crystalSprite;
        [SerializeField, Tooltip("ツルスプライト")] private Sprite _vineSprite;
        [SerializeField, Tooltip("種スプライト")] private Sprite _seedSprite;
        [SerializeField, Tooltip("土スプライト")] private Sprite _soilSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private PlotData[] _plots;
        private int _gridCols = 4;
        private int _gridRows = 3;
        private float _cellSize = 1.4f;
        private HashSet<int> _grownTypes = new HashSet<int>();

        private static readonly Color[] PlantColors = {
            new Color(1f, 0.5f, 0.7f), new Color(0.8f, 0.3f, 0.3f),
            new Color(0.5f, 0.7f, 1f), new Color(0.3f, 0.8f, 0.4f)
        };

        private class PlotData
        {
            public GameObject Obj;
            public GameObject PlantObj;
            public SpriteRenderer PlantSr;
            public int State; // 0=empty, 1=seed, 2=growing, 3=grown
            public int PlantType; // 0-3
            public float GrowTimer;
        }

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            int total = _gridCols * _gridRows;
            _plots = new PlotData[total];

            float startX = -(_gridCols - 1) * _cellSize / 2f;
            float startY = (_gridRows - 1) * _cellSize / 2f - 0.5f;

            for (int i = 0; i < total; i++)
            {
                int r = i / _gridCols, c = i % _gridCols;
                float x = startX + c * _cellSize;
                float y = startY - r * _cellSize;

                var obj = new GameObject($"Plot_{i}");
                obj.transform.position = new Vector3(x, y, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _soilSprite; sr.sortingOrder = 1;
                var col = obj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.45f, 0.45f);

                _plots[i] = new PlotData { Obj = obj, State = 0, GrowTimer = 0f };
            }
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Grow plants
            for (int i = 0; i < _plots.Length; i++)
            {
                if (_plots[i].State == 2)
                {
                    _plots[i].GrowTimer += Time.deltaTime;
                    if (_plots[i].GrowTimer >= 4f)
                    {
                        _plots[i].State = 3;
                        Sprite[] sprites = { _flowerSprite, _mushroomSprite, _crystalSprite, _vineSprite };
                        if (_plots[i].PlantSr != null)
                        {
                            _plots[i].PlantSr.sprite = sprites[_plots[i].PlantType];
                            _plots[i].PlantSr.color = PlantColors[_plots[i].PlantType];
                            _plots[i].PlantObj.transform.localScale = Vector3.one * 0.6f;
                        }
                        _grownTypes.Add(_plots[i].PlantType);
                    }
                    else
                    {
                        // Growing animation
                        float scale = Mathf.Lerp(0.2f, 0.5f, _plots[i].GrowTimer / 4f);
                        if (_plots[i].PlantObj != null)
                            _plots[i].PlantObj.transform.localScale = Vector3.one * scale;
                    }
                }
            }

            // Handle tap
            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null)
                {
                    for (int i = 0; i < _plots.Length; i++)
                    {
                        if (_plots[i].Obj == hit.gameObject)
                        {
                            HandlePlotTap(i);
                            break;
                        }
                    }
                }
            }
        }

        private void HandlePlotTap(int index)
        {
            var plot = _plots[index];
            if (plot.State == 0)
            {
                // Plant seed
                plot.State = 1;
                plot.PlantType = Random.Range(0, 4);

                var plantObj = new GameObject("Plant");
                plantObj.transform.SetParent(plot.Obj.transform);
                plantObj.transform.localPosition = new Vector3(0f, 0.1f, -0.01f);
                var psr = plantObj.AddComponent<SpriteRenderer>();
                psr.sprite = _seedSprite; psr.sortingOrder = 2;
                plantObj.transform.localScale = Vector3.one * 0.2f;
                psr.color = PlantColors[plot.PlantType] * 0.6f;

                plot.PlantObj = plantObj;
                plot.PlantSr = psr;

                // Start growing immediately
                plot.State = 2;
                plot.GrowTimer = 0f;
            }
            else if (plot.State == 3)
            {
                // Tap grown plant - visual feedback (pulse)
                if (plot.PlantObj != null)
                {
                    plot.PlantObj.transform.localScale = Vector3.one * 0.7f;
                    // Will shrink back naturally
                }
            }
        }

        public int GrownTypes => _grownTypes.Count;
    }
}
