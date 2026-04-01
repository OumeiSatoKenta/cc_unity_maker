using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game068_CloudFarm
{
    public class FarmManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private CloudFarmGameManager _gameManager;
        [SerializeField, Tooltip("土スプライト")] private Sprite _soilSprite;
        [SerializeField, Tooltip("苗スプライト")] private Sprite _seedlingSprite;
        [SerializeField, Tooltip("収穫スプライト")] private Sprite _cropSprite;
        [SerializeField, Tooltip("種コスト")] private int _seedCost = 5;

        private Camera _mainCamera;
        private bool _isActive;
        private PlotData[] _plots;
        private int _gridCols = 4;
        private int _gridRows = 3;
        private float _cellSize = 1.3f;
        private HashSet<int> _collectedTypes = new HashSet<int>();

        private static readonly string[] CropNames = { "ニンジン", "トマト", "キャベツ", "カボチャ" };
        private static readonly Color[] CropColors = {
            new Color(1f, 0.55f, 0.2f), new Color(1f, 0.25f, 0.2f),
            new Color(0.4f, 0.8f, 0.3f), new Color(1f, 0.7f, 0.1f)
        };

        private class PlotData
        {
            public GameObject Obj;
            public SpriteRenderer Sr;
            public GameObject CropObj;
            public SpriteRenderer CropSr;
            public int State; // 0=empty, 1=planted, 2=grown
            public int CropType;
            public float GrowTimer;
        }

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            int total = _gridCols * _gridRows;
            _plots = new PlotData[total];

            float startX = -(_gridCols - 1) * _cellSize / 2f;
            float startY = (_gridRows - 1) * _cellSize / 2f - 1f;

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

                _plots[i] = new PlotData { Obj = obj, Sr = sr, State = 0, GrowTimer = 0f };
            }
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Grow crops
            for (int i = 0; i < _plots.Length; i++)
            {
                if (_plots[i].State == 1)
                {
                    _plots[i].GrowTimer += Time.deltaTime;
                    if (_plots[i].GrowTimer >= 5f)
                    {
                        _plots[i].State = 2;
                        if (_plots[i].CropSr != null)
                        {
                            _plots[i].CropSr.sprite = _cropSprite;
                            _plots[i].CropSr.color = CropColors[_plots[i].CropType];
                            _plots[i].CropObj.transform.localScale = Vector3.one * 0.5f;
                        }
                    }
                }
            }

            // Handle input
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
                if (_gameManager.TrySpend(_seedCost))
                {
                    plot.State = 1;
                    plot.CropType = Random.Range(0, CropNames.Length);
                    plot.GrowTimer = 0f;

                    var cropObj = new GameObject("Crop");
                    cropObj.transform.SetParent(plot.Obj.transform);
                    cropObj.transform.localPosition = new Vector3(0f, 0.1f, -0.01f);
                    var csr = cropObj.AddComponent<SpriteRenderer>();
                    csr.sprite = _seedlingSprite; csr.sortingOrder = 2;
                    cropObj.transform.localScale = Vector3.one * 0.3f;
                    csr.color = CropColors[plot.CropType] * 0.7f;

                    plot.CropObj = cropObj;
                    plot.CropSr = csr;
                }
            }
            else if (plot.State == 2)
            {
                // Harvest
                int value = 8 + plot.CropType * 3;
                _gameManager.AddCoins(value);
                _collectedTypes.Add(plot.CropType);

                if (plot.CropObj != null) Destroy(plot.CropObj);
                plot.CropObj = null;
                plot.CropSr = null;
                plot.State = 0;
            }
        }

        public int CollectedCropTypes => _collectedTypes.Count;
    }
}
