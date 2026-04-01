using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game093_ColorPerception
{
    public class ColorManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private ColorPerceptionGameManager _gameManager;
        [SerializeField, Tooltip("赤タイル")] private Sprite _tileRedSprite;
        [SerializeField, Tooltip("緑タイル")] private Sprite _tileGreenSprite;
        [SerializeField, Tooltip("青タイル")] private Sprite _tileBlueSprite;
        [SerializeField, Tooltip("黄タイル")] private Sprite _tileYellowSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _targetColorIndex;
        private bool _isAlternateView;
        private List<TileData> _tiles = new List<TileData>();

        private static readonly Color[] NormalColors = {
            new Color(0.86f,0.24f,0.24f), new Color(0.24f,0.71f,0.24f),
            new Color(0.24f,0.39f,0.86f), new Color(0.86f,0.78f,0.16f)
        };
        private static readonly Color[] AltColors = {
            new Color(0.7f,0.5f,0.2f), new Color(0.6f,0.6f,0.3f),
            new Color(0.3f,0.3f,0.8f), new Color(0.8f,0.7f,0.3f)
        };

        private class TileData { public GameObject Obj; public SpriteRenderer Sr; public int ColorId; }

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame() { _isActive = true; SetupPuzzle(); }
        public void StopGame() { _isActive = false; }
        public void NextPuzzle() { ClearTiles(); SetupPuzzle(); }

        private void SetupPuzzle()
        {
            _targetColorIndex = Random.Range(0, 4);
            _isAlternateView = false;

            Sprite[] sprites = { _tileRedSprite, _tileGreenSprite, _tileBlueSprite, _tileYellowSprite };
            float startX = -2.5f, startY = 1.5f;

            for (int i = 0; i < 16; i++)
            {
                int r = i / 4, c = i % 4;
                int colorId = Random.Range(0, 4);
                // Ensure at least one target
                if (i == Random.Range(0, 16)) colorId = _targetColorIndex;

                var obj = new GameObject($"Tile_{i}");
                obj.transform.position = new Vector3(startX + c * 1.4f, startY - r * 1.4f, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = sprites[colorId]; sr.sortingOrder = 2;
                sr.color = NormalColors[colorId];
                var col = obj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.45f, 0.45f);
                _tiles.Add(new TileData { Obj = obj, Sr = sr, ColorId = colorId });
            }
        }

        private void ClearTiles()
        {
            foreach (var t in _tiles) if (t.Obj != null) Destroy(t.Obj);
            _tiles.Clear();
        }

        public void ToggleView()
        {
            _isAlternateView = !_isAlternateView;
            foreach (var t in _tiles)
            {
                if (t.Sr != null)
                    t.Sr.color = _isAlternateView ? AltColors[t.ColorId] : NormalColors[t.ColorId];
            }
        }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

            var hit = Physics2D.OverlapPoint(wp);
            if (hit != null)
            {
                foreach (var t in _tiles)
                {
                    if (t.Obj == hit.gameObject)
                    {
                        _gameManager.OnMoveUsed();
                        if (t.ColorId == _targetColorIndex)
                            _gameManager.OnCorrectMatch();
                        break;
                    }
                }
            }
        }
    }
}
