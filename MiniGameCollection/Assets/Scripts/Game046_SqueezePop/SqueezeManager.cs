using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game046_SqueezePop
{
    public class SqueezeManager : MonoBehaviour
    {
        [SerializeField] private SqueezePopGameManager _gameManager;

        private const int GridW = 12;
        private const int GridH = 8;
        private const float CellSize = 0.65f;
        private const int BlobsPerSqueeze = 8;
        private const float BlobSpeed = 6f;

        private bool[,] _filled;
        private GameObject[,] _cellObjects;
        private Sprite _blobSprite;
        private Sprite _cellSprite;
        private Camera _mainCamera;
        private List<GameObject> _blobs = new List<GameObject>();
        private List<Vector2> _blobVelocities = new List<Vector2>();
        private float _currentHue;
        private Color _currentColor;

        public void Init()
        {
            _mainCamera = Camera.main;
            _blobSprite = Resources.Load<Sprite>("Sprites/Game046_SqueezePop/blob");
            _cellSprite = Resources.Load<Sprite>("Sprites/Game046_SqueezePop/cell");

            CleanUp();

            _filled = new bool[GridH, GridW];
            _cellObjects = new GameObject[GridH, GridW];
            _currentHue = Random.Range(0f, 1f);
            _currentColor = Color.HSVToRGB(_currentHue, 0.7f, 1f);

            float offsetX = -(GridW - 1) * CellSize / 2f;
            float offsetY = (GridH - 1) * CellSize / 2f - 0.5f;

            for (int r = 0; r < GridH; r++)
            {
                for (int c = 0; c < GridW; c++)
                {
                    float x = offsetX + c * CellSize;
                    float y = offsetY - r * CellSize;
                    var go = new GameObject("Cell");
                    go.transform.position = new Vector3(x, y, 0f);
                    go.transform.localScale = Vector3.one * (CellSize * 0.95f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = _cellSprite;
                    sr.color = new Color(0.9f, 0.88f, 0.84f);
                    sr.sortingOrder = 0;
                    _cellObjects[r, c] = go;
                }
            }
        }

        private void CleanUp()
        {
            if (_cellObjects != null)
                foreach (var c in _cellObjects) if (c != null) Destroy(c);
            foreach (var b in _blobs) if (b != null) Destroy(b);
            _blobs.Clear();
            _blobVelocities.Clear();
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                wp.z = 0f;

                _currentHue = (_currentHue + 0.15f) % 1f;
                _currentColor = Color.HSVToRGB(_currentHue, 0.7f, 1f);

                for (int i = 0; i < BlobsPerSqueeze; i++)
                {
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(BlobSpeed * 0.5f, BlobSpeed);
                    var vel = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    var blob = new GameObject("Blob");
                    blob.transform.position = wp;
                    blob.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f);
                    var sr = blob.AddComponent<SpriteRenderer>();
                    sr.sprite = _blobSprite;
                    sr.color = _currentColor;
                    sr.sortingOrder = 10;
                    _blobs.Add(blob);
                    _blobVelocities.Add(vel);
                }
            }

            UpdateBlobs();
        }

        private void UpdateBlobs()
        {
            float offsetX = -(GridW - 1) * CellSize / 2f;
            float offsetY = (GridH - 1) * CellSize / 2f - 0.5f;
            bool anyFilled = false;

            for (int i = _blobs.Count - 1; i >= 0; i--)
            {
                if (_blobs[i] == null) { _blobs.RemoveAt(i); _blobVelocities.RemoveAt(i); continue; }

                _blobVelocities[i] *= 0.95f;
                _blobs[i].transform.position += (Vector3)_blobVelocities[i] * Time.deltaTime;

                var pos = _blobs[i].transform.position;
                int col = Mathf.RoundToInt((pos.x - offsetX) / CellSize);
                int row = Mathf.RoundToInt((offsetY - pos.y) / CellSize);

                if (row >= 0 && row < GridH && col >= 0 && col < GridW && !_filled[row, col])
                {
                    _filled[row, col] = true;
                    var sr = _cellObjects[row, col].GetComponent<SpriteRenderer>();
                    sr.color = _blobs[i].GetComponent<SpriteRenderer>().color;
                    anyFilled = true;
                }

                if (_blobVelocities[i].sqrMagnitude < 0.1f || pos.x < -5f || pos.x > 5f || pos.y < -5f || pos.y > 5f)
                {
                    Destroy(_blobs[i]);
                    _blobs.RemoveAt(i);
                    _blobVelocities.RemoveAt(i);
                }
            }

            if (anyFilled)
            {
                float percent = CalculateFillPercent();
                if (_gameManager != null) _gameManager.OnSqueeze(percent);
            }
        }

        private float CalculateFillPercent()
        {
            int count = 0;
            foreach (var f in _filled) if (f) count++;
            return (float)count / (GridW * GridH);
        }
    }
}
