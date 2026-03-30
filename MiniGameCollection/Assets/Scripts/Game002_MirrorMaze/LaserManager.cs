using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game002_MirrorMaze
{
    public enum LaserDir { Right, Up, Left, Down }

    public class LaserManager : MonoBehaviour
    {
        [SerializeField] private MirrorMazeGameManager _gameManager;
        [SerializeField] private Sprite _mirrorSlashSprite;
        [SerializeField] private Sprite _mirrorBackslashSprite;
        [SerializeField] private SpriteRenderer[] _receiverRenderers;

        private const int GRID_W = 7;
        private const int GRID_H = 7;
        private const float CELL_SIZE = 1.0f;
        private static readonly Vector2 GRID_ORIGIN = new Vector2(-3f, -3f);

        private readonly Color _receiverInactive = new Color(1f, 0.3f, 0.3f);
        private readonly Color _receiverHitColor  = new Color(0.2f, 1f, 0.4f);

        private Vector2Int[] _emitterPositions;
        private LaserDir[]   _emitterDirections;
        private Color[]      _emitterColors;
        private Vector2Int[] _receiverPositions;
        private LaserDir[]   _receiverExpectedDirs;
        private bool[]       _receiverHit;

        private MirrorType?[,]      _mirrors;
        private MirrorController[,] _mirrorObjects;
        private LineRenderer[]      _laserLines;
        private bool _ready;

        public void InitializeLevel()
        {
            CleanupDynamic();
            LoadLevel();
            _ready = true;
            RecalculateLaser();
        }

        private void CleanupDynamic()
        {
            _ready = false;
            if (_mirrorObjects != null)
                for (int x = 0; x < GRID_W; x++)
                    for (int y = 0; y < GRID_H; y++)
                        if (_mirrorObjects[x, y] != null)
                            Destroy(_mirrorObjects[x, y].gameObject);

            if (_laserLines != null)
                foreach (var lr in _laserLines)
                    if (lr != null) Destroy(lr.gameObject);

            _mirrors       = new MirrorType?[GRID_W, GRID_H];
            _mirrorObjects = new MirrorController[GRID_W, GRID_H];
        }

        private void LoadLevel()
        {
            // Level: Emitter at grid(-1,3) shoots RIGHT.
            // Receiver at grid(3,7) expects laser going UP.
            // Minimal solution: place a '/' mirror at (3,3).
            _emitterPositions    = new[] { new Vector2Int(-1, 3) };
            _emitterDirections   = new[] { LaserDir.Right };
            _emitterColors       = new[] { new Color(1f, 0.35f, 0.1f) };
            _receiverPositions    = new[] { new Vector2Int(3, 7) };
            _receiverExpectedDirs = new[] { LaserDir.Up };
            _receiverHit          = new bool[1];

            _laserLines = new LineRenderer[_emitterPositions.Length];
            for (int i = 0; i < _emitterPositions.Length; i++)
            {
                var laserGo = new GameObject("LaserLine");
                laserGo.transform.SetParent(transform);
                var lr       = laserGo.AddComponent<LineRenderer>();
                lr.material    = new Material(Shader.Find("Sprites/Default"));
                lr.startColor  = _emitterColors[i];
                lr.endColor    = _emitterColors[i];
                lr.startWidth  = 0.07f;
                lr.endWidth    = 0.07f;
                lr.useWorldSpace  = true;
                lr.sortingOrder   = 5;
                _laserLines[i]  = lr;
            }
            UpdateReceiverVisuals();
        }

        private void Update()
        {
            if (!_ready) return;
            if (_gameManager != null && _gameManager.State != GameState.Playing) return;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                HandleClick();
        }

        private void HandleClick()
        {
            var screenPos = Mouse.current.position.ReadValue();
            var worldPos  = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 0f));
            int gx = Mathf.RoundToInt((worldPos.x - GRID_ORIGIN.x) / CELL_SIZE);
            int gy = Mathf.RoundToInt((worldPos.y - GRID_ORIGIN.y) / CELL_SIZE);
            if (gx < 0 || gx >= GRID_W || gy < 0 || gy >= GRID_H) return;

            if (_mirrors[gx, gy] == null)
                PlaceMirror(gx, gy, MirrorType.Slash);
            else if (_mirrors[gx, gy] == MirrorType.Slash)
                PlaceMirror(gx, gy, MirrorType.Backslash);
            else
                EraseMirror(gx, gy);

            RecalculateLaser();
        }

        private void PlaceMirror(int gx, int gy, MirrorType type)
        {
            EraseMirror(gx, gy);
            _mirrors[gx, gy] = type;

            var mirrorGo = new GameObject("Mirror");
            mirrorGo.transform.position = (Vector3)GridToWorld(gx, gy);
            var sr = mirrorGo.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.6f, 0.88f, 1f);
            sr.sortingOrder = 2;

            var slash = _mirrorSlashSprite    != null ? _mirrorSlashSprite    : MakeFallbackSprite(true);
            var back  = _mirrorBackslashSprite != null ? _mirrorBackslashSprite : MakeFallbackSprite(false);
            var mc = mirrorGo.AddComponent<MirrorController>();
            mc.Init(new Vector2Int(gx, gy), type, slash, back);
            _mirrorObjects[gx, gy] = mc;
        }

        private void EraseMirror(int gx, int gy)
        {
            _mirrors[gx, gy] = null;
            if (_mirrorObjects[gx, gy] != null)
            {
                Destroy(_mirrorObjects[gx, gy].gameObject);
                _mirrorObjects[gx, gy] = null;
            }
        }

        private void RecalculateLaser()
        {
            for (int i = 0; i < _receiverHit.Length; i++) _receiverHit[i] = false;
            for (int i = 0; i < _emitterPositions.Length; i++) TraceLaser(i);
            UpdateReceiverVisuals();
            CheckWin();
        }

        private void TraceLaser(int idx)
        {
            var lr  = _laserLines[idx];
            var pts = new List<Vector3>();
            var cur = _emitterPositions[idx];
            var dir = _emitterDirections[idx];
            pts.Add(ToVec3(GridToWorld(cur.x, cur.y)));

            for (int step = 0; step < 64; step++)
            {
                cur += DirToVec(dir);
                pts.Add(ToVec3(GridToWorld(cur.x, cur.y)));

                bool stopped = false;
                for (int r = 0; r < _receiverPositions.Length; r++)
                {
                    if (_receiverPositions[r] == cur)
                    {
                        if (_receiverExpectedDirs[r] == dir) _receiverHit[r] = true;
                        stopped = true;
                        break;
                    }
                }
                if (stopped) break;

                if (cur.x < 0 || cur.x >= GRID_W || cur.y < 0 || cur.y >= GRID_H) break;

                if (_mirrors[cur.x, cur.y].HasValue)
                    dir = ReflectDir(dir, _mirrors[cur.x, cur.y].Value);
            }

            lr.positionCount = pts.Count;
            lr.SetPositions(pts.ToArray());
        }

        private void UpdateReceiverVisuals()
        {
            if (_receiverRenderers == null || _receiverHit == null) return;
            for (int i = 0; i < _receiverRenderers.Length && i < _receiverHit.Length; i++)
                if (_receiverRenderers[i] != null)
                    _receiverRenderers[i].color = _receiverHit[i] ? _receiverHitColor : _receiverInactive;
        }

        private void CheckWin()
        {
            if (_receiverHit == null || _receiverHit.Length == 0) return;
            foreach (var h in _receiverHit) if (!h) return;
            _gameManager?.OnAllReceiversHit();
        }

        private static LaserDir ReflectDir(LaserDir dir, MirrorType m)
        {
            if (m == MirrorType.Slash)
            {
                if (dir == LaserDir.Right) return LaserDir.Up;
                if (dir == LaserDir.Up)    return LaserDir.Right;
                if (dir == LaserDir.Left)  return LaserDir.Down;
                return LaserDir.Left;
            }
            if (dir == LaserDir.Right) return LaserDir.Down;
            if (dir == LaserDir.Down)  return LaserDir.Right;
            if (dir == LaserDir.Left)  return LaserDir.Up;
            return LaserDir.Left;
        }

        private static Vector2Int DirToVec(LaserDir d)
        {
            if (d == LaserDir.Right) return Vector2Int.right;
            if (d == LaserDir.Up)    return Vector2Int.up;
            if (d == LaserDir.Left)  return Vector2Int.left;
            return Vector2Int.down;
        }

        private static Vector2 GridToWorld(int gx, int gy)
            => GRID_ORIGIN + new Vector2(gx * CELL_SIZE, gy * CELL_SIZE);

        private static Vector3 ToVec3(Vector2 v, float z = -0.1f)
            => new Vector3(v.x, v.y, z);

        private static Sprite MakeFallbackSprite(bool isSlash)
        {
            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    float dist = isSlash ? Mathf.Abs(x - y) : Mathf.Abs(x + y - (size - 1));
                    tex.SetPixel(x, y, dist <= 2 ? Color.white : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
