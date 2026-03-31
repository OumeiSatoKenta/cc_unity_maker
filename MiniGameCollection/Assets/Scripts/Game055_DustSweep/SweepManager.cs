using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game055_DustSweep
{
    public class SweepManager : MonoBehaviour
    {
        [SerializeField] private DustSweepGameManager _gameManager;

        private const int GridW = 16;
        private const int GridH = 10;
        private const float CellSize = 0.55f;
        private const float SweepRadius = 1.2f;
        private const int StarCount = 3;

        private bool[,] _dustGrid;
        private GameObject[,] _dustObjects;
        private List<GameObject> _starObjects = new List<GameObject>();
        private List<Vector2Int> _starPositions = new List<Vector2Int>();
        private List<bool> _starRevealed = new List<bool>();
        private Sprite _dustSprite, _cleanSprite, _starSprite;
        private Camera _mainCamera;
        private float _offsetX, _offsetY;
        private int _totalDust;

        public void Init()
        {
            _mainCamera = Camera.main;
            _dustSprite = Resources.Load<Sprite>("Sprites/Game055_DustSweep/dust");
            _cleanSprite = Resources.Load<Sprite>("Sprites/Game055_DustSweep/clean");
            _starSprite = Resources.Load<Sprite>("Sprites/Game055_DustSweep/star");

            CleanUp();

            _dustGrid = new bool[GridH, GridW];
            _dustObjects = new GameObject[GridH, GridW];
            _offsetX = -(GridW - 1) * CellSize / 2f;
            _offsetY = (GridH - 1) * CellSize / 2f;
            _totalDust = GridW * GridH;

            // Place stars
            _starPositions.Clear(); _starRevealed.Clear(); _starObjects.Clear();
            for (int i = 0; i < StarCount; i++)
            {
                int r = Random.Range(1, GridH - 1);
                int c = Random.Range(1, GridW - 1);
                _starPositions.Add(new Vector2Int(c, r));
                _starRevealed.Add(false);
                float x = _offsetX + c * CellSize;
                float y = _offsetY - r * CellSize;
                var sGo = new GameObject("Star_" + i);
                sGo.transform.position = new Vector3(x, y, 0f);
                sGo.transform.localScale = Vector3.one * 0.5f;
                var ssr = sGo.AddComponent<SpriteRenderer>();
                ssr.sprite = _starSprite; ssr.sortingOrder = 1;
                _starObjects.Add(sGo);
            }

            // Place clean tiles underneath
            for (int r = 0; r < GridH; r++)
            {
                for (int c = 0; c < GridW; c++)
                {
                    float x = _offsetX + c * CellSize;
                    float y = _offsetY - r * CellSize;
                    var cGo = new GameObject("Clean");
                    cGo.transform.position = new Vector3(x, y, 0f);
                    cGo.transform.localScale = Vector3.one * (CellSize * 0.95f);
                    var csr = cGo.AddComponent<SpriteRenderer>();
                    csr.sprite = _cleanSprite; csr.sortingOrder = 0;
                    float hue = ((float)(r + c) / (GridW + GridH)) * 0.3f + 0.5f;
                    csr.color = Color.HSVToRGB(hue, 0.15f, 1f);
                }
            }

            // Place dust on top
            for (int r = 0; r < GridH; r++)
            {
                for (int c = 0; c < GridW; c++)
                {
                    _dustGrid[r, c] = true;
                    float x = _offsetX + c * CellSize;
                    float y = _offsetY - r * CellSize;
                    var go = new GameObject("Dust");
                    go.transform.position = new Vector3(x, y, 0f);
                    go.transform.localScale = Vector3.one * (CellSize * 0.95f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = _dustSprite; sr.sortingOrder = 5;
                    float shade = Random.Range(0.8f, 1f);
                    sr.color = new Color(shade * 0.7f, shade * 0.63f, shade * 0.5f, 0.9f);
                    _dustObjects[r, c] = go;
                }
            }
        }

        private void CleanUp()
        {
            if (_dustObjects != null)
                foreach (var d in _dustObjects) if (d != null) Destroy(d);
            foreach (var s in _starObjects) if (s != null) Destroy(s);
            _starObjects.Clear();
            var all = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            foreach (var sr in all)
                if (sr.gameObject.name == "Clean") Destroy(sr.gameObject);
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.isPressed)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                SweepAt(wp);
            }
        }

        private void SweepAt(Vector3 worldPos)
        {
            bool changed = false;
            for (int r = 0; r < GridH; r++)
            {
                for (int c = 0; c < GridW; c++)
                {
                    if (!_dustGrid[r, c]) continue;
                    float x = _offsetX + c * CellSize;
                    float y = _offsetY - r * CellSize;
                    float dist = Vector2.Distance(new Vector2(worldPos.x, worldPos.y), new Vector2(x, y));
                    if (dist < SweepRadius)
                    {
                        _dustGrid[r, c] = false;
                        if (_dustObjects[r, c] != null) { Destroy(_dustObjects[r, c]); _dustObjects[r, c] = null; }
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                int cleaned = 0;
                foreach (var d in _dustGrid) if (!d) cleaned++;
                float percent = (float)cleaned / _totalDust;
                if (_gameManager != null) _gameManager.OnCleanProgress(percent);

                for (int i = 0; i < _starPositions.Count; i++)
                {
                    if (_starRevealed[i]) continue;
                    var sp = _starPositions[i];
                    if (!_dustGrid[sp.y, sp.x])
                    {
                        _starRevealed[i] = true;
                        if (_gameManager != null) _gameManager.OnStarFound();
                    }
                }
            }
        }
    }
}
