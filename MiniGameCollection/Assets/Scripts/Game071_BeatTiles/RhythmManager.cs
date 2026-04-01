using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game071_BeatTiles
{
    public class RhythmManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private BeatTilesGameManager _gameManager;
        [SerializeField, Tooltip("タイルスプライト")] private Sprite _tileSprite;
        [SerializeField, Tooltip("レーン数")] private int _laneCount = 4;
        [SerializeField, Tooltip("タイル落下速度")] private float _fallSpeed = 4f;
        [SerializeField, Tooltip("スポーン間隔")] private float _spawnInterval = 0.6f;

        private Camera _mainCamera;
        private bool _isActive;
        private float _spawnTimer;
        private float _hitLineY = -3.5f;
        private float _laneWidth = 1.5f;
        private List<TileData> _tiles = new List<TileData>();

        private static readonly Color[] LaneColors = {
            new Color(1f, 0.3f, 0.3f), new Color(0.3f, 0.7f, 1f),
            new Color(0.3f, 1f, 0.4f), new Color(1f, 0.8f, 0.2f)
        };

        private class TileData
        {
            public GameObject Obj;
            public int Lane;
            public bool Hit;
        }

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _spawnTimer = 0.3f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Spawn tiles
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnTile();
                _spawnTimer = _spawnInterval;
            }

            // Move tiles down and check misses
            for (int i = _tiles.Count - 1; i >= 0; i--)
            {
                var t = _tiles[i];
                if (t.Obj == null) { _tiles.RemoveAt(i); continue; }

                t.Obj.transform.position += Vector3.down * _fallSpeed * Time.deltaTime;

                if (t.Obj.transform.position.y < _hitLineY - 1.5f && !t.Hit)
                {
                    t.Hit = true;
                    _gameManager.OnMiss();
                    Destroy(t.Obj);
                    _tiles.RemoveAt(i);
                }
            }

            // Handle input
            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                CheckTap(wp);
            }
        }

        private void SpawnTile()
        {
            int lane = Random.Range(0, _laneCount);
            float x = GetLaneX(lane);

            var obj = new GameObject($"Tile_{lane}");
            obj.transform.position = new Vector3(x, 6f, 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _tileSprite; sr.sortingOrder = 3;
            sr.color = LaneColors[lane];

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 0.5f);

            _tiles.Add(new TileData { Obj = obj, Lane = lane, Hit = false });
        }

        private void CheckTap(Vector2 worldPos)
        {
            TileData closest = null;
            float closestDist = float.MaxValue;

            foreach (var t in _tiles)
            {
                if (t.Hit || t.Obj == null) continue;
                float dist = Vector2.Distance(worldPos, t.Obj.transform.position);
                float yDiff = Mathf.Abs(t.Obj.transform.position.y - _hitLineY);

                if (dist < 1.5f && yDiff < 1.5f && dist < closestDist)
                {
                    closest = t;
                    closestDist = dist;
                }
            }

            if (closest != null)
            {
                float yDiff = Mathf.Abs(closest.Obj.transform.position.y - _hitLineY);
                closest.Hit = true;

                if (yDiff < 0.3f)
                    _gameManager.OnPerfect();
                else if (yDiff < 0.8f)
                    _gameManager.OnGreat();
                else
                    _gameManager.OnMiss();

                Destroy(closest.Obj);
                _tiles.Remove(closest);
            }
        }

        private float GetLaneX(int lane)
        {
            float totalWidth = (_laneCount - 1) * _laneWidth;
            return -totalWidth / 2f + lane * _laneWidth;
        }
    }
}
