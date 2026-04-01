using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game040_DashDungeon
{
    public class DungeonManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private DashDungeonGameManager _gameManager;

        [SerializeField, Tooltip("ヒーロースプライト")]
        private Sprite _heroSprite;

        [SerializeField, Tooltip("敵スプライト")]
        private Sprite _enemySprite;

        [SerializeField, Tooltip("壁スプライト")]
        private Sprite _wallSprite;

        [SerializeField, Tooltip("出口スプライト")]
        private Sprite _exitSprite;

        private Transform _hero;
        private Vector2Int _heroGridPos;
        private Vector2Int _dashDir;
        private bool _isDashing;
        private float _dashTimer;
        private List<Vector2Int> _enemyPositions = new List<Vector2Int>();
        private HashSet<Vector2Int> _wallPositions = new HashSet<Vector2Int>();
        private Vector2Int _exitPos;
        private Dictionary<Vector2Int, GameObject> _enemyObjects = new Dictionary<Vector2Int, GameObject>();

        private const float CellSize = 1.0f;
        private const float DashSpeed = 12f;
        private const int GridW = 7;
        private const int GridH = 9;
        private static readonly Vector2 GridOffset = new Vector2(-3f, -4f);

        // 簡易マップ: 1=壁, 2=敵, 3=出口, 0=空
        private static readonly int[,] Map = {
            {1,1,1,1,1,1,1},
            {1,0,0,0,0,0,1},
            {1,0,1,0,1,2,1},
            {1,2,0,0,0,0,1},
            {1,0,1,1,0,1,1},
            {1,0,2,0,0,2,1},
            {1,0,0,1,0,0,1},
            {1,0,0,0,2,0,1},
            {1,1,1,1,3,1,1},
        };

        private Camera _mainCamera;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            BuildMap();
            SpawnHero();
        }

        private void BuildMap()
        {
            for (int y = 0; y < GridH; y++)
            {
                for (int x = 0; x < GridW; x++)
                {
                    int cell = Map[y, x];
                    Vector3 worldPos = GridToWorld(x, y);

                    if (cell == 1)
                    {
                        var obj = CreateTile("Wall", worldPos, _wallSprite, 1);
                        _wallPositions.Add(new Vector2Int(x, y));
                    }
                    else if (cell == 2)
                    {
                        var obj = CreateTile($"Enemy_{x}_{y}", worldPos, _enemySprite, 2);
                        _enemyPositions.Add(new Vector2Int(x, y));
                        _enemyObjects[new Vector2Int(x, y)] = obj;
                    }
                    else if (cell == 3)
                    {
                        CreateTile("Exit", worldPos, _exitSprite, 1);
                        _exitPos = new Vector2Int(x, y);
                    }
                }
            }
        }

        private GameObject CreateTile(string name, Vector3 pos, Sprite sprite, int order)
        {
            var obj = new GameObject(name);
            obj.transform.position = pos;
            obj.transform.localScale = new Vector3(CellSize, CellSize, 1f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.sortingOrder = order;
            obj.transform.SetParent(transform);
            return obj;
        }

        private void SpawnHero()
        {
            _heroGridPos = new Vector2Int(1, 1);
            var obj = new GameObject("Hero");
            obj.transform.position = GridToWorld(_heroGridPos.x, _heroGridPos.y);
            obj.transform.localScale = new Vector3(CellSize * 0.8f, CellSize * 0.8f, 1f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _heroSprite; sr.sortingOrder = 5;
            _hero = obj.transform;
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying) return;

            if (_isDashing)
            {
                ContinueDash();
            }
            else
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            Vector3 wp = _mainCamera.ScreenToWorldPoint(mp);

            Vector2 diff = (Vector2)wp - (Vector2)_hero.position;
            // 4方向のうち最も大きい成分の方向にダッシュ
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                _dashDir = diff.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                _dashDir = diff.y > 0 ? Vector2Int.up : Vector2Int.down;

            _isDashing = true;
        }

        private void ContinueDash()
        {
            Vector2Int nextPos = _heroGridPos + _dashDir;

            // 壁チェック
            if (_wallPositions.Contains(nextPos))
            {
                _isDashing = false;
                return;
            }

            // 移動
            _heroGridPos = nextPos;
            Vector3 targetWorld = GridToWorld(_heroGridPos.x, _heroGridPos.y);
            _hero.position = Vector3.MoveTowards(_hero.position, targetWorld, DashSpeed * Time.deltaTime);

            if (Vector3.Distance(_hero.position, targetWorld) < 0.01f)
            {
                _hero.position = targetWorld;

                // 敵チェック
                if (_enemyObjects.ContainsKey(_heroGridPos))
                {
                    Destroy(_enemyObjects[_heroGridPos]);
                    _enemyObjects.Remove(_heroGridPos);
                    _gameManager.AddScore(100);
                    _gameManager.TakeDamage(1);
                }

                // 出口チェック
                if (_heroGridPos == _exitPos)
                {
                    _isDashing = false;
                    _gameManager.OnReachExit();
                    return;
                }

                // 次のマスが壁なら停止
                Vector2Int ahead = _heroGridPos + _dashDir;
                if (_wallPositions.Contains(ahead) || ahead.x < 0 || ahead.x >= GridW || ahead.y < 0 || ahead.y >= GridH)
                {
                    _isDashing = false;
                }
            }
        }

        private Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(GridOffset.x + x * CellSize, GridOffset.y + (GridH - 1 - y) * CellSize, 0f);
        }
    }
}
