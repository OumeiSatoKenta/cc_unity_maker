using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game095_SilentWorld
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private SilentWorldGameManager _gameManager;
        [SerializeField, Tooltip("マススプライト")] private Sprite _cellSprite;
        [SerializeField, Tooltip("プレイヤー")] private Sprite _playerSprite;
        [SerializeField, Tooltip("出口")] private Sprite _exitSprite;
        [SerializeField, Tooltip("ヒント使用可能回数")] private int _maxHints = 3;

        private const int GridSize = 5;
        private const float CellSize = 0.9f;

        private static readonly Color ColorNormal  = new Color(0.18f, 0.18f, 0.22f);
        private static readonly Color ColorExit    = new Color(0.2f,  0.6f,  0.3f);
        private static readonly Color ColorPlayer  = new Color(0.9f,  0.9f,  1.0f);
        private static readonly Color ColorItemHint= new Color(0.95f, 0.85f, 0.1f);
        private static readonly Color ColorTrapHint= new Color(0.85f, 0.2f,  0.2f);
        private static readonly Color ColorCollected= new Color(0.3f, 0.4f,  0.35f);

        private enum CellType { Empty, Item, Trap, Exit }

        private CellType[] _grid;
        private bool[] _itemCollected;
        private SpriteRenderer[] _cellRenderers;
        private GameObject _playerObj;
        private int _playerRow, _playerCol;
        private int _hintsRemaining;
        private bool _isActive;
        private Camera _mainCamera;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            if (_cellSprite == null) { Debug.LogError("[WorldManager] _cellSprite が未アサイン"); return; }
            _isActive = true;
            _hintsRemaining = _maxHints;
            _playerRow = 0; _playerCol = 0;
            _grid = new CellType[GridSize * GridSize];
            _itemCollected = new bool[GridSize * GridSize];

            SetupGrid();
            CreateVisuals();
            if (_gameManager != null)
                _gameManager?.UpdateHintDisplay(_hintsRemaining);
        }

        public void StopGame() { _isActive = false; }

        public void UseHint()
        {
            if (!_isActive || _hintsRemaining <= 0) return;
            _hintsRemaining--;
            if (_gameManager != null)
                _gameManager?.UpdateHintDisplay(_hintsRemaining);
            StartCoroutine(ShowHint());
        }

        private void SetupGrid()
        {
            // 出口を右下に設定
            _grid[4 * GridSize + 4] = CellType.Exit;

            // アイテム3個をランダム配置
            PlaceRandom(CellType.Item, 3);
            // トラップ3個をランダム配置
            PlaceRandom(CellType.Trap, 3);
        }

        private void PlaceRandom(CellType type, int count)
        {
            int placed = 0;
            int attempts = 0;
            while (placed < count && attempts < 100)
            {
                attempts++;
                int r = Random.Range(0, GridSize);
                int c = Random.Range(0, GridSize);
                int idx = r * GridSize + c;
                // プレイヤー初期位置と出口は除く
                if ((r == 0 && c == 0) || (r == 4 && c == 4)) continue;
                if (_grid[idx] != CellType.Empty) continue;
                _grid[idx] = type;
                placed++;
            }
        }

        private void CreateVisuals()
        {
            _cellRenderers = new SpriteRenderer[GridSize * GridSize];
            float startX = -(GridSize * CellSize) / 2f + CellSize / 2f;
            float startY = (GridSize * CellSize) / 2f - CellSize / 2f + 0.5f;

            for (int i = 0; i < GridSize * GridSize; i++)
            {
                int r = i / GridSize, c = i % GridSize;
                var obj = new GameObject($"Cell_{r}_{c}");
                obj.transform.SetParent(transform);
                obj.transform.position = new Vector3(startX + c * CellSize, startY - r * CellSize, 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 1;
                obj.transform.localScale = Vector3.one * (CellSize / 0.8f);
                obj.AddComponent<BoxCollider2D>().size = Vector2.one * 0.8f;

                // 出口は常時表示
                if (_grid[i] == CellType.Exit)
                { sr.sprite = _exitSprite; sr.color = ColorExit; }
                else
                { sr.sprite = _cellSprite; sr.color = ColorNormal; }

                _cellRenderers[i] = sr;
            }

            // プレイヤー
            _playerObj = new GameObject("Player");
            _playerObj.transform.SetParent(transform);
            var psr = _playerObj.AddComponent<SpriteRenderer>();
            psr.sprite = _playerSprite; psr.sortingOrder = 3;
            _playerObj.transform.localScale = Vector3.one * (CellSize / 0.8f);
            UpdatePlayerVisual();
        }

        private void UpdatePlayerVisual()
        {
            if (_playerObj == null) return;
            float startX = -(GridSize * CellSize) / 2f + CellSize / 2f;
            float startY = (GridSize * CellSize) / 2f - CellSize / 2f + 0.5f;
            _playerObj.transform.position = new Vector3(
                startX + _playerCol * CellSize,
                startY - _playerRow * CellSize, -0.1f);
        }

        private void Update()
        {
            if (!_isActive || _cellRenderers == null) return;
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

            var hit = Physics2D.OverlapPoint(wp);
            if (hit == null) return;

            // クリックされたセルを特定
            for (int i = 0; i < GridSize * GridSize; i++)
            {
                if (_cellRenderers[i] == null) continue;
                if (Vector2.Distance(_cellRenderers[i].transform.position, wp) < CellSize * 0.5f)
                {
                    int r = i / GridSize, c = i % GridSize;
                    TryMove(r, c);
                    break;
                }
            }
        }

        private void TryMove(int r, int c)
        {
            // 隣接マスのみ移動可能（上下左右）
            int dr = Mathf.Abs(r - _playerRow);
            int dc = Mathf.Abs(c - _playerCol);
            if (!((dr == 1 && dc == 0) || (dr == 0 && dc == 1))) return;

            _playerRow = r; _playerCol = c;
            UpdatePlayerVisual();

            int idx = r * GridSize + c;
            CellType cell = _grid[idx];

            if (cell == CellType.Item && !_itemCollected[idx])
            {
                _itemCollected[idx] = true;
                _cellRenderers[idx].color = ColorCollected;
                _gameManager?.OnItemCollected();
            }
            else if (cell == CellType.Trap)
            {
                _gameManager?.OnTrapHit();
            }
            else if (cell == CellType.Exit)
            {
                _gameManager?.OnExitReached();
            }
        }

        private IEnumerator ShowHint()
        {
            // アイテム/トラップ/出口を光らせる
            for (int i = 0; i < GridSize * GridSize; i++)
            {
                if (_grid[i] == CellType.Item && !_itemCollected[i])
                    _cellRenderers[i].color = ColorItemHint;
                else if (_grid[i] == CellType.Trap)
                    _cellRenderers[i].color = ColorTrapHint;
            }
            yield return new WaitForSeconds(1f);
            if (!_isActive || _cellRenderers == null) yield break;
            // 元に戻す
            for (int i = 0; i < GridSize * GridSize; i++)
            {
                if (_grid[i] == CellType.Item && !_itemCollected[i])
                    _cellRenderers[i].color = ColorNormal;
                else if (_grid[i] == CellType.Trap)
                    _cellRenderers[i].color = ColorNormal;
            }
        }
    }
}
