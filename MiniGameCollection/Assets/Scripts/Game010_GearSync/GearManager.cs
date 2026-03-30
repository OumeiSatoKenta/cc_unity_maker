using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game010_GearSync
{
    /// <summary>
    /// 歯車の配置・入力処理・クリア判定を一元管理するコアメカニクスManager。
    /// 入力処理はここだけで行う。
    /// </summary>
    public class GearManager : MonoBehaviour
    {
        [SerializeField] private GearSyncGameManager _gameManager;
        [SerializeField] private Sprite _gearSprite;
        [SerializeField] private Sprite _arrowSprite;

        private readonly List<GearController> _gears = new List<GearController>();
        private float _cellSize = 1.4f;

        // ── レベル定義 ────────────────────────────────────────────────────────
        // 各レベル: (gearCount, layout, targetDirections)
        // 方向: 0=右,1=上,2=左,3=下
        private static readonly int[][,] LevelLayouts = new int[][,]
        {
            // Level 1: 3x1 横並び (3歯車)
            new int[,] { {0,0}, {1,0}, {2,0} },
            // Level 2: 2x2 グリッド (4歯車)
            new int[,] { {0,0}, {1,0}, {0,1}, {1,1} },
            // Level 3: L字 (5歯車)
            new int[,] { {0,0}, {1,0}, {2,0}, {0,1}, {0,2} },
            // Level 4: T字 (5歯車)
            new int[,] { {0,1}, {1,1}, {2,1}, {1,0}, {1,2} },
            // Level 5: 3x3 (9歯車)
            new int[,] { {0,0},{1,0},{2,0}, {0,1},{1,1},{2,1}, {0,2},{1,2},{2,2} },
        };

        private static readonly int[][] LevelTargetDirs = new int[][]
        {
            new int[] { 0, 1, 2 },             // Level 1
            new int[] { 0, 1, 2, 3 },          // Level 2
            new int[] { 0, 2, 1, 3, 0 },       // Level 3
            new int[] { 1, 0, 3, 2, 1 },       // Level 4
            new int[] { 0,1,2, 3,0,1, 2,3,0 }, // Level 5
        };

        public void SetupLevel(int level)
        {
            ClearGears();

            int idx = Mathf.Clamp(level - 1, 0, LevelLayouts.Length - 1);
            var layout = LevelLayouts[idx];
            var targets = LevelTargetDirs[idx];

            // レイアウト中心を計算してオフセット
            float maxX = 0, maxY = 0;
            for (int i = 0; i < layout.GetLength(0); i++)
            {
                if (layout[i, 0] > maxX) maxX = layout[i, 0];
                if (layout[i, 1] > maxY) maxY = layout[i, 1];
            }
            float offsetX = maxX * _cellSize * 0.5f;
            float offsetY = maxY * _cellSize * 0.5f;

            for (int i = 0; i < layout.GetLength(0); i++)
            {
                float wx = layout[i, 0] * _cellSize - offsetX;
                float wy = layout[i, 1] * _cellSize - offsetY;

                var go = new GameObject($"Gear_{i}");
                go.transform.SetParent(transform, false);
                go.transform.position = new Vector3(wx, wy, 0f);

                var gear = go.AddComponent<GearController>();
                bool isDriver = (i == 0); // 最初の歯車が駆動歯車
                gear.Initialize(i, targets[i], isDriver, _gearSprite, _arrowSprite);
                gear.SetupCollider();
                _gears.Add(gear);
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (!mouse.leftButton.wasPressedThisFrame) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var gear = hit.GetComponent<GearController>();
            if (gear == null || !_gears.Contains(gear)) return;
            if (gear.IsDriver) return; // 駆動歯車は回転不可

            gear.RotateClockwise();
            if (_gameManager != null) _gameManager.OnGearRotated();
        }

        /// <summary>全ての非駆動歯車の向きが目標方向と一致しているか確認</summary>
        public bool IsAllGearsSynced()
        {
            foreach (var gear in _gears)
            {
                if (!gear.IsSynced) return false;
            }
            return true;
        }

        private void ClearGears()
        {
            foreach (var gear in _gears)
            {
                if (gear != null) Destroy(gear.gameObject);
            }
            _gears.Clear();
        }
    }
}
