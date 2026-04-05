using UnityEngine;
using System.Collections.Generic;

namespace Game031v2_BounceKing
{
    public class BlockManager : MonoBehaviour
    {
        [SerializeField] BounceKingGameManager _gameManager;

        Sprite _spriteNormal;
        Sprite _spriteHard;
        Sprite _spriteBoss;
        Sprite _spriteItemExpand;
        Sprite _spriteItemMultiBall;
        Sprite _spriteItemShrink;

        List<Block> _blocks = new List<Block>();
        int _remainingBlocks;
        int _currentStage;
        bool _allBlocksNotified;
        float _hardBlockRatio;
        bool _hasBossBlock;
        bool _hasItemDrop;
        bool _hasMultiBall;
        bool _hasShrinkTrap;

        const string SpritePath = "Sprites/Game031v2_BounceKing/";

        public void LoadSprites()
        {
            _spriteNormal = Resources.Load<Sprite>(SpritePath + "BlockNormal");
            _spriteHard = Resources.Load<Sprite>(SpritePath + "BlockHard");
            _spriteBoss = Resources.Load<Sprite>(SpritePath + "BlockBoss");
            _spriteItemExpand = Resources.Load<Sprite>(SpritePath + "ItemExpand");
            _spriteItemMultiBall = Resources.Load<Sprite>(SpritePath + "ItemMultiBall");
            _spriteItemShrink = Resources.Load<Sprite>(SpritePath + "ItemShrink");
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            ClearBlocks();
            _allBlocksNotified = false;
            _currentStage = stageIndex + 1;
            _hardBlockRatio = config.complexityFactor;
            _hasItemDrop = _currentStage >= 3;
            _hasMultiBall = _currentStage >= 4;
            _hasShrinkTrap = _currentStage >= 5;
            _hasBossBlock = _currentStage >= 5;

            int rows = config.countMultiplier;
            int cols = _currentStage >= 4 ? 9 : 8;

            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float gameAreaTop = camSize - topMargin;
            float gameAreaBottom = -camSize + bottomMargin;
            float blockAreaHeight = gameAreaTop - gameAreaBottom - 1.0f;

            float blockHeight = blockAreaHeight / rows;
            float blockWidth = (camWidth * 2f) / cols;

            float padding = 0.05f;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    float x = -camWidth + blockWidth * (col + 0.5f);
                    float y = gameAreaTop - blockHeight * (row + 0.5f);

                    BlockType type = DetermineBlockType(row, col, rows);
                    Sprite sp = GetSprite(type);

                    var go = new GameObject($"Block_{row}_{col}");
                    go.layer = LayerMask.NameToLayer("Block");
                    go.transform.position = new Vector3(x, y, 0f);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 2;

                    var col2d = go.AddComponent<BoxCollider2D>();

                    float w = blockWidth - padding * 2f;
                    float h = blockHeight - padding * 2f;
                    col2d.size = new Vector2(w, h);

                    if (sp != null)
                    {
                        sr.sprite = sp;
                        float scaleX = w / (sp.rect.width / sp.pixelsPerUnit);
                        float scaleY = h / (sp.rect.height / sp.pixelsPerUnit);
                        go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                        col2d.size = Vector2.one;
                    }

                    var block = go.AddComponent<Block>();
                    block.Initialize(type, sp);
                    block.OnDestroyed += OnBlockDestroyed;

                    _blocks.Add(block);
                }
            }

            _remainingBlocks = _blocks.Count;
        }

        BlockType DetermineBlockType(int row, int col, int totalRows)
        {
            if (_hasBossBlock && row == 0 && col % 3 == 1)
                return BlockType.Boss;
            if (_hardBlockRatio > 0f && row < totalRows * _hardBlockRatio)
                return BlockType.Hard;
            return BlockType.Normal;
        }

        Sprite GetSprite(BlockType type) => type switch
        {
            BlockType.Hard => _spriteHard,
            BlockType.Boss => _spriteBoss,
            _ => _spriteNormal
        };

        void OnBlockDestroyed(Block block)
        {
            _blocks.Remove(block);
            _remainingBlocks--;

            // Drop item
            if (_hasItemDrop)
            {
                float dropRate = block.GetDropRate();
                if (Random.value < dropRate)
                    SpawnItem(block.transform.position);
            }

            if (_remainingBlocks <= 0 && !_allBlocksNotified)
            {
                _allBlocksNotified = true;
                _gameManager.OnAllBlocksDestroyed();
            }
        }

        void SpawnItem(Vector3 pos)
        {
            ItemType type = PickItemType();
            Sprite sp = GetItemSprite(type);

            var go = new GameObject("Item");
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Default");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = 5;
            if (sp != null)
            {
                float s = 0.4f / (sp.rect.width / sp.pixelsPerUnit);
                go.transform.localScale = Vector3.one * s;
            }

            var col2d = go.AddComponent<CircleCollider2D>();
            col2d.isTrigger = true;
            col2d.radius = 0.25f;

            var item = go.AddComponent<ItemController>();
            item.Initialize(type, _gameManager);
        }

        ItemType PickItemType()
        {
            float r = Random.value;
            if (_hasShrinkTrap && r < 0.2f) return ItemType.PaddleShrink;
            if (_hasMultiBall && r < 0.5f) return ItemType.MultiBall;
            return ItemType.PaddleExpand;
        }

        Sprite GetItemSprite(ItemType type) => type switch
        {
            ItemType.MultiBall => _spriteItemMultiBall,
            ItemType.PaddleShrink => _spriteItemShrink,
            _ => _spriteItemExpand
        };

        public void ClearBlocks()
        {
            foreach (var b in _blocks)
                if (b != null) Destroy(b.gameObject);
            _blocks.Clear();
            _remainingBlocks = 0;
        }

        public int RemainingBlocks => _remainingBlocks;
    }
}
