using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Common;

namespace Game069v2_DungeonDigger
{
    public enum BlockType { Soil, Rock, Lava, Monster, Boss, Empty }
    public enum ItemType { Gold, Copper, Iron, Gem, RareGem }

    public class BlockData
    {
        public BlockType type;
        public int hp;
        public int maxHp;
        public GameObject obj;
        public SpriteRenderer sr;
    }

    public class DigManager : MonoBehaviour
    {
        [SerializeField] Sprite _blockSoilSprite;
        [SerializeField] Sprite _blockRockSprite;
        [SerializeField] Sprite _blockLavaSprite;
        [SerializeField] Sprite _blockMonsterSprite;
        [SerializeField] Sprite _blockBossSprite;
        [SerializeField] Sprite _itemGoldSprite;
        [SerializeField] Sprite _itemCopperSprite;
        [SerializeField] Sprite _itemIronSprite;
        [SerializeField] Sprite _itemGemSprite;
        [SerializeField] Sprite _itemRareGemSprite;
        [SerializeField] Sprite _monsterSprite;
        [SerializeField] Sprite _bossSprite;

        DungeonDiggerGameManager _gameManager;

        // Stage config
        int _stageIndex;
        int _tapPower;
        int _blockHpBase;
        float _autoRate; // blocks per second
        int _depthTarget;
        bool _hasMonster;
        bool _hasLava;
        bool _hasBoss;

        // Upgrades
        int _drillLevel = 1;
        bool _heatShieldOwned;
        bool _lanternOwned;
        int[] _drillUpgradeCosts = { 20, 50, 120, 300, 700 };

        // State
        bool _isActive;
        int _currentDepth;
        long _gold;
        int _inventoryCount;
        int _combo;
        float _lastTapTime;
        float _autoTimer;

        // Grid
        const int Cols = 5;
        const int VisibleRows = 6;
        BlockData[,] _grid; // [col, row]
        float _cellSize;
        float _startX;
        float _startY;

        public long TotalGold => _gold;

        void Awake()
        {
            _gameManager = GetComponentInParent<DungeonDiggerGameManager>();
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _isActive = true;
            _currentDepth = 0;
            _combo = 0;
            _lastTapTime = 0f;
            _autoTimer = 0f;
            _monsterActive = false;

            // Stage parameters
            int[] depthTargets = { 50, 200, 500, 1000, 2000 };
            int[] tapPowers = { 1, 1, 2, 3, 5 };
            int[] blockHps = { 1, 3, 5, 8, 12 };
            float[] autoRates = { 0f, 0.5f, 1.0f, 1.5f, 2.0f };

            _depthTarget = depthTargets[stageIndex];
            _tapPower = tapPowers[stageIndex] * _drillLevel;
            _blockHpBase = blockHps[stageIndex];
            _autoRate = autoRates[stageIndex];
            _hasMonster = stageIndex >= 2;
            _hasLava = stageIndex >= 3;
            _hasBoss = stageIndex == 4;

            // Recalculate layout
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.8f;
            float bottomMargin = 3.5f;
            float availableH = camSize * 2f - topMargin - bottomMargin;
            _cellSize = Mathf.Min(availableH / VisibleRows, camWidth * 2f / Cols, 1.0f);
            _startX = -_cellSize * (Cols - 1) / 2f;
            _startY = camSize - topMargin - _cellSize / 2f;

            BuildGrid();
            RefreshUI();
        }

        void BuildGrid()
        {
            // Clear existing
            if (_grid != null)
            {
                for (int c = 0; c < Cols; c++)
                    for (int r = 0; r < VisibleRows; r++)
                        if (_grid[c, r]?.obj != null)
                            Destroy(_grid[c, r].obj);
            }

            _grid = new BlockData[Cols, VisibleRows];

            for (int c = 0; c < Cols; c++)
            {
                for (int r = 0; r < VisibleRows; r++)
                {
                    _grid[c, r] = CreateBlock(c, r);
                }
            }
        }

        BlockData CreateBlock(int col, int row)
        {
            BlockType type = ChooseBlockType(row);
            int hp = Mathf.Max(1, _blockHpBase + Random.Range(-1, 2));

            var obj = new GameObject($"Block_{col}_{row}");
            obj.transform.SetParent(transform);
            float x = _startX + col * _cellSize;
            float y = _startY - row * _cellSize;
            obj.transform.position = new Vector3(x, y, 0f);
            float scale = _cellSize / 1.28f;
            obj.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetBlockSprite(type);
            sr.sortingOrder = 1;

            var col2d = obj.AddComponent<BoxCollider2D>();
            col2d.size = new Vector2(1f, 1f);

            return new BlockData { type = type, hp = hp, maxHp = hp, obj = obj, sr = sr };
        }

        BlockType ChooseBlockType(int row)
        {
            // Deep rows have tougher blocks
            if (_hasBoss && row == VisibleRows - 1 && Random.value < 0.1f) return BlockType.Boss;
            if (_hasLava && row >= 4 && Random.value < 0.3f) return BlockType.Lava;
            if (_hasMonster && row >= 3 && Random.value < 0.15f) return BlockType.Monster;
            if (_stageIndex >= 1 && row >= 2 && Random.value < 0.4f) return BlockType.Rock;
            return BlockType.Soil;
        }

        Sprite GetBlockSprite(BlockType type)
        {
            return type switch
            {
                BlockType.Rock => _blockRockSprite,
                BlockType.Lava => _blockLavaSprite,
                BlockType.Monster => _blockMonsterSprite,
                BlockType.Boss => _blockBossSprite,
                _ => _blockSoilSprite
            };
        }

        void Update()
        {
            if (!_isActive) return;

            // Tap input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var hit = Physics2D.OverlapPoint(mouseWorld);
                if (hit != null)
                {
                    HandleBlockTap(hit.gameObject);
                }
            }

            // Auto drill
            if (_autoRate > 0f)
            {
                _autoTimer += Time.deltaTime;
                float interval = 1f / _autoRate;
                if (_autoTimer >= interval)
                {
                    _autoTimer = 0f;
                    AutoDig();
                }
            }

            // Combo timeout (3 seconds)
            if (_combo > 0 && Time.time - _lastTapTime > 3f)
            {
                _combo = 0;
                _gameManager.UpdateComboDisplay(0);
            }
        }

        void HandleBlockTap(GameObject obj)
        {
            for (int c = 0; c < Cols; c++)
            {
                for (int r = 0; r < VisibleRows; r++)
                {
                    var block = _grid[c, r];
                    if (block == null || block.obj != obj) continue;

                    // Lava penalty
                    if (block.type == BlockType.Lava && !_heatShieldOwned)
                    {
                        ApplyLavaPenalty();
                        return;
                    }

                    // Update combo
                    _combo++;
                    _lastTapTime = Time.time;

                    DigBlock(c, r, _tapPower);
                    return;
                }
            }
        }

        void DigBlock(int col, int row, int power)
        {
            var block = _grid[col, row];
            if (block == null || block.type == BlockType.Empty) return;

            block.hp -= power;

            // Visual feedback - scale pulse
            StartCoroutine(ScalePulse(block.obj.transform));

            if (block.hp <= 0)
            {
                // Block destroyed
                DropItems(block.type, col, row);
                Destroy(block.obj);
                _grid[col, row] = new BlockData { type = BlockType.Empty, obj = null };

                _currentDepth++;
                if (_currentDepth >= _depthTarget)
                {
                    _isActive = false;
                    _gameManager.OnStageClear();
                    return;
                }

                // Shift grid down and add new block at top
                ShiftColumn(col);
                RefreshUI();
            }
            else
            {
                // Show HP damage via color
                float ratio = (float)block.hp / block.maxHp;
                block.sr.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), Color.white, ratio);
            }

            _gameManager.UpdateDepthDisplay(_currentDepth, _depthTarget);
            _gameManager.UpdateComboDisplay(_combo);
        }

        void AutoDig()
        {
            // Pick random column, dig top block
            int col = Random.Range(0, Cols);
            DigBlock(col, 0, 1);
        }

        void ShiftColumn(int col)
        {
            // Move blocks down one row
            for (int r = VisibleRows - 1; r > 0; r--)
            {
                _grid[col, r] = _grid[col, r - 1];
                if (_grid[col, r] != null && _grid[col, r].obj != null)
                {
                    float x = _startX + col * _cellSize;
                    float y = _startY - r * _cellSize;
                    _grid[col, r].obj.transform.position = new Vector3(x, y, 0f);
                    _grid[col, r].obj.name = $"Block_{col}_{r}";
                }
            }

            // Create new block at top
            _grid[col, 0] = CreateBlock(col, 0);
        }

        void DropItems(BlockType blockType, int col, int row)
        {
            float dropChance = _lanternOwned ? 0.6f : 0.4f;
            if (Random.value > dropChance) return;

            ItemType item;
            float roll = Random.value;
            if (blockType == BlockType.Boss)
                item = ItemType.RareGem;
            else if (blockType == BlockType.Lava && roll < 0.3f)
                item = ItemType.Gem;
            else if (roll < 0.05f)
                item = ItemType.RareGem;
            else if (roll < 0.2f)
                item = ItemType.Gem;
            else if (roll < 0.5f)
                item = ItemType.Iron;
            else if (roll < 0.7f)
                item = ItemType.Copper;
            else
                item = ItemType.Gold;

            long value = ItemValue(item);
            float multiplier = ComboMultiplier();
            long earned = (long)(value * multiplier);
            _gold += earned;
            _inventoryCount++;

            // Visual flash on item drop
            StartCoroutine(GoldFlash());

            RefreshUI();
        }

        long ItemValue(ItemType item) => item switch
        {
            ItemType.RareGem => 200,
            ItemType.Gem => 50,
            ItemType.Iron => 10,
            ItemType.Copper => 5,
            ItemType.Gold => 3,
            _ => 1
        };

        float ComboMultiplier()
        {
            if (_combo >= 20) return 3.0f;
            if (_combo >= 10) return 2.0f;
            if (_combo >= 5) return 1.5f;
            return 1.0f;
        }

        void ApplyLavaPenalty()
        {
            _currentDepth = Mathf.Max(0, _currentDepth - 10);
            _gameManager.UpdateDepthDisplay(_currentDepth, _depthTarget);
            StartCoroutine(CameraShake());
        }

        public void BuyDrillUpgrade()
        {
            if (_drillLevel > _drillUpgradeCosts.Length) return;
            long cost = _drillUpgradeCosts[_drillLevel - 1];
            if (_gold < cost) return;
            _gold -= cost;
            _drillLevel++;
            _tapPower++;
            _gameManager.UpdateGoldDisplay(_gold);
            _gameManager.UpdateDrillLevelDisplay(_drillLevel);
            RefreshUpgradeUI();
        }

        public void BuyHeatShield()
        {
            long cost = 100;
            if (_gold < cost || _heatShieldOwned) return;
            _gold -= cost;
            _heatShieldOwned = true;
            _gameManager.UpdateGoldDisplay(_gold);
            RefreshUpgradeUI();
        }

        public void BuyLantern()
        {
            long cost = 80;
            if (_gold < cost || _lanternOwned) return;
            _gold -= cost;
            _lanternOwned = true;
            _gameManager.UpdateGoldDisplay(_gold);
            RefreshUpgradeUI();
        }

        // アイテムはドロップ時に即ゴールド加算される設計。
        // SellAll はインベントリ収集カウンターのリセット（UI表示クリア）専用。
        public void SellAll()
        {
            _inventoryCount = 0;
            _gameManager.UpdateInventoryDisplay(0);
        }

        public void SetActive(bool active) => _isActive = active;

        void RefreshUI()
        {
            _gameManager.UpdateGoldDisplay(_gold);
            _gameManager.UpdateInventoryDisplay(_inventoryCount);
            _gameManager.UpdateDrillLevelDisplay(_drillLevel);
            _gameManager.UpdateAutoRateDisplay(_autoRate);
            RefreshUpgradeUI();
        }

        void RefreshUpgradeUI()
        {
            long drillCost = _drillLevel <= _drillUpgradeCosts.Length
                ? _drillUpgradeCosts[_drillLevel - 1] : 9999;
            _gameManager.UpdateUpgradeButtons(_gold, drillCost,
                _heatShieldOwned, _lanternOwned, 100, 80);
        }

        IEnumerator ScalePulse(Transform t)
        {
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.1f)
            {
                t.localScale = orig * Mathf.Lerp(1f, 1.3f, elapsed / 0.1f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < 0.1f)
            {
                t.localScale = orig * Mathf.Lerp(1.3f, 1f, elapsed / 0.1f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            t.localScale = orig;
        }

        IEnumerator GoldFlash()
        {
            // Flash combo text scale
            yield return new WaitForSeconds(0.05f);
            _gameManager.UpdateComboDisplay(_combo);
        }

        IEnumerator CameraShake()
        {
            var cam = Camera.main.transform;
            Vector3 orig = cam.localPosition;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                cam.localPosition = orig + (Vector3)Random.insideUnitCircle * 0.15f;
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.localPosition = orig;
        }

        void OnDestroy()
        {
            // Clean up grid
            if (_grid != null)
            {
                for (int c = 0; c < Cols; c++)
                    for (int r = 0; r < VisibleRows; r++)
                        if (_grid[c, r]?.obj != null)
                            Destroy(_grid[c, r].obj);
            }
        }
    }
}
