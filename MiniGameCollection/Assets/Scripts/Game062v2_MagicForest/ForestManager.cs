using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game062v2_MagicForest
{
    public class ForestManager : MonoBehaviour
    {
        [SerializeField] MagicForestGameManager _gameManager;
        [SerializeField] MagicForestUI _ui;

        // Sprites loaded from Resources
        Sprite _sprOak, _sprBirch, _sprPine, _sprWorldTree, _sprSapling, _sprWithered, _sprBonusSapling, _sprAnimal, _sprStorm;

        const int Cols = 7;
        const int Rows = 8;

        enum CellState { Empty, Sapling, BonusSapling, Oak, Birch, Pine, WorldTree, Withered }

        CellState[] _grid;
        GameObject[] _cellObjects;
        float _cellSize;
        Vector2 _gridOrigin;

        // Game state
        int _mana;
        int _totalTrees;
        int _targetTrees;
        float _autoGrowRate; // trees per second
        float _autoGrowTimer;
        float _stormTimer;
        float _stormDuration = 10f;
        float _stormInterval = 30f;
        float _animalTimer;
        float _animalInterval = 20f;
        bool _stormActive;
        bool _animalsEnabled;
        bool _stormEnabled;
        bool _worldTreeEnabled;
        bool _worldTreeGrowing;
        float _worldTreeTimer;
        float _worldTreeGrowDuration = 30f;
        int _worldTreeCell = -1;
        int _currentStage;
        bool _isActive;

        // Combo
        int _combo;
        float _comboTimer;
        const float ComboWindow = 2f;

        // Unlocked tree types
        bool _birchUnlocked;
        bool _pineUnlocked;

        // Auto grow upgrade cost
        int _autoGrowCost = 30;
        bool _autoGrowPurchased;

        public int TotalTrees => _totalTrees;
        public int Mana => _mana;
        public bool AutoGrowPurchased => _autoGrowPurchased;
        public int AutoGrowCost => _autoGrowCost;

        void Awake()
        {
            LoadSprites();
            _grid = new CellState[Cols * Rows];
            _cellObjects = new GameObject[Cols * Rows];
        }

        void LoadSprites()
        {
            _sprOak = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/TreeOak");
            _sprBirch = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/TreeBirch");
            _sprPine = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/TreePine");
            _sprWorldTree = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/WorldTree");
            _sprSapling = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/Sapling");
            _sprWithered = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/Withered");
            _sprBonusSapling = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/BonusSapling");
            _sprAnimal = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/Animal");
            _sprStorm = Resources.Load<Sprite>("Sprites/Game062v2_MagicForest/Storm");
        }

        public void SetupStage(int stageIndex, StageManager.StageConfig config)
        {
            _currentStage = stageIndex;
            _targetTrees = config.countMultiplier;
            _autoGrowRate = config.speedMultiplier;
            _isActive = true;

            // Unlock features per stage
            _birchUnlocked = stageIndex >= 1;
            _pineUnlocked = stageIndex >= 2;
            _animalsEnabled = stageIndex >= 2;
            _stormEnabled = stageIndex >= 3;
            _worldTreeEnabled = stageIndex >= 4;

            // Reset storm / animal timers
            _stormTimer = _stormInterval;
            _animalTimer = _animalInterval;
            _stormActive = false;
            _worldTreeGrowing = false;
            _worldTreeCell = -1;

            _autoGrowPurchased = false;

            RebuildGrid();
            _ui.UpdateMana(_mana);
            _ui.UpdateArea(_totalTrees, _targetTrees);
            _ui.SetAutoGrowButtonVisible(stageIndex >= 1 && !_autoGrowPurchased);
            _ui.UpdateAutoGrowCost(_autoGrowCost);
        }

        void RebuildGrid()
        {
            // Destroy existing cell objects
            foreach (var obj in _cellObjects)
                if (obj != null) Destroy(obj);

            // Calculate responsive layout
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float availableH = camSize * 2f - topMargin - bottomMargin;
            float availableW = camWidth * 2f - 0.4f;
            _cellSize = Mathf.Min(availableH / Rows, availableW / Cols, 1.1f);

            float gridW = _cellSize * Cols;
            float gridH = _cellSize * Rows;
            float startY = camSize - topMargin - _cellSize * 0.5f;
            float startX = -gridW * 0.5f + _cellSize * 0.5f;
            _gridOrigin = new Vector2(startX, startY);

            // Reset grid data
            for (int i = 0; i < _grid.Length; i++) _grid[i] = CellState.Empty;
            _totalTrees = 0;

            // Place initial saplings in center region
            int centerCol = Cols / 2;
            int centerRow = Rows / 2;
            PlantSapling(centerRow * Cols + centerCol);

            RefreshAllCells();
        }

        void RefreshAllCells()
        {
            for (int i = 0; i < _grid.Length; i++)
                RefreshCell(i);
        }

        void RefreshCell(int idx)
        {
            if (_cellObjects[idx] != null)
            {
                Destroy(_cellObjects[idx]);
                _cellObjects[idx] = null;
            }

            int col = idx % Cols;
            int row = idx / Cols;
            Vector2 pos = new Vector2(
                _gridOrigin.x + col * _cellSize,
                _gridOrigin.y - row * _cellSize
            );

            Sprite spr = GetSprite(_grid[idx]);
            if (spr == null) return;

            var obj = new GameObject($"Cell_{idx}");
            obj.transform.position = new Vector3(pos.x, pos.y, 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            float scale = _cellSize * 0.9f / Mathf.Max(spr.bounds.size.x, spr.bounds.size.y);
            obj.transform.localScale = Vector3.one * scale;
            sr.sortingOrder = 1;
            _cellObjects[idx] = obj;
        }

        Sprite GetSprite(CellState state)
        {
            return state switch
            {
                CellState.Sapling => _sprSapling,
                CellState.BonusSapling => _sprBonusSapling,
                CellState.Oak => _sprOak,
                CellState.Birch => _sprBirch,
                CellState.Pine => _sprPine,
                CellState.WorldTree => _sprWorldTree,
                CellState.Withered => _sprWithered,
                _ => null
            };
        }

        void Update()
        {
            if (!_isActive || !_gameManager.IsPlaying) return;

            HandleInput();
            HandleAutoGrow();
            HandleAnimal();
            HandleStorm();
            HandleWorldTree();
            HandleComboTimeout();
        }

        void HandleInput()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            Vector2 wp = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            int idx = WorldToCell(wp);
            if (idx < 0) return;
            OnCellTapped(idx);
        }

        int WorldToCell(Vector2 wp)
        {
            float relX = wp.x - (_gridOrigin.x - _cellSize * 0.5f);
            float relY = _gridOrigin.y + _cellSize * 0.5f - wp.y;
            int col = Mathf.FloorToInt(relX / _cellSize);
            int row = Mathf.FloorToInt(relY / _cellSize);
            if (col < 0 || col >= Cols || row < 0 || row >= Rows) return -1;
            return row * Cols + col;
        }

        void OnCellTapped(int idx)
        {
            switch (_grid[idx])
            {
                case CellState.Empty:
                    PlantSapling(idx);
                    RefreshCell(idx);
                    break;

                case CellState.Sapling:
                case CellState.BonusSapling:
                    bool isBonus = _grid[idx] == CellState.BonusSapling;
                    int cost = isBonus ? 0 : 10;
                    if (_mana >= cost)
                    {
                        _mana -= cost;
                        GrowToTree(idx);
                    }
                    break;

                case CellState.Oak:
                case CellState.Birch:
                case CellState.Pine:
                    TapTree(idx);
                    break;

                case CellState.Withered:
                    // Replant withered tree (costs mana)
                    if (_mana >= 5)
                    {
                        _mana -= 5;
                        _grid[idx] = CellState.Sapling;
                        RefreshCell(idx);
                        _ui.UpdateMana(_mana);
                    }
                    break;
            }
        }

        void PlantSapling(int idx)
        {
            if (_grid[idx] != CellState.Empty) return;
            _grid[idx] = CellState.Sapling;
        }

        void GrowToTree(int idx)
        {
            CellState newState = ChooseTreeType(idx);
            _grid[idx] = newState;
            _totalTrees++;
            RefreshCell(idx);
            StartCoroutine(GrowEffect(idx));
            SpawnNeighborSapling(idx);
            _ui.UpdateMana(_mana);
            _ui.UpdateArea(_totalTrees, _targetTrees);
            CheckStageClear();
            CheckWorldTreeCondition();
        }

        CellState ChooseTreeType(int idx)
        {
            if (!_birchUnlocked) return CellState.Oak;
            if (!_pineUnlocked)
            {
                // 30% birch if unlocked
                return Random.value < 0.3f ? CellState.Birch : CellState.Oak;
            }
            float r = Random.value;
            if (r < 0.5f) return CellState.Oak;
            if (r < 0.75f) return CellState.Birch;
            return CellState.Pine;
        }

        void TapTree(int idx)
        {
            _combo++;
            _comboTimer = ComboWindow;
            float mult = GetComboMultiplier();
            int gained = Mathf.RoundToInt(5 * mult);
            _mana += gained;
            StartCoroutine(TapEffect(idx));
            _ui.UpdateMana(_mana);
            _ui.UpdateCombo(_combo, mult);
        }

        float GetComboMultiplier()
        {
            if (_combo <= 2) return 1f;
            if (_combo <= 5) return 1.5f;
            if (_combo <= 9) return 2f;
            return 3f;
        }

        void SpawnNeighborSapling(int idx)
        {
            int col = idx % Cols;
            int row = idx / Cols;
            int[] dcs = { -1, 1, 0, 0 };
            int[] drs = { 0, 0, -1, 1 };
            List<int> empties = new List<int>();
            for (int d = 0; d < 4; d++)
            {
                int nc = col + dcs[d];
                int nr = row + drs[d];
                if (nc < 0 || nc >= Cols || nr < 0 || nr >= Rows) continue;
                int ni = nr * Cols + nc;
                if (_grid[ni] == CellState.Empty) empties.Add(ni);
            }
            if (empties.Count > 0 && Random.value < 0.6f)
            {
                int target = empties[Random.Range(0, empties.Count)];
                _grid[target] = CellState.Sapling;
                RefreshCell(target);
            }
        }

        void HandleAutoGrow()
        {
            if (!_autoGrowPurchased || _autoGrowRate <= 0f) return;
            _autoGrowTimer += Time.deltaTime;
            float interval = 1f / _autoGrowRate;
            if (_autoGrowTimer >= interval)
            {
                _autoGrowTimer = 0f;
                AutoGrowOneSapling();
            }
        }

        void AutoGrowOneSapling()
        {
            List<int> saplings = new List<int>();
            for (int i = 0; i < _grid.Length; i++)
                if (_grid[i] == CellState.Sapling || _grid[i] == CellState.BonusSapling)
                    saplings.Add(i);
            if (saplings.Count == 0) return;
            int idx = saplings[Random.Range(0, saplings.Count)];
            _grid[idx] = CellState.Sapling; // ensure type before growing
            GrowToTree(idx);
        }

        void HandleAnimal()
        {
            if (!_animalsEnabled) return;
            _animalTimer -= Time.deltaTime;
            if (_animalTimer <= 0f)
            {
                _animalTimer = _animalInterval;
                StartCoroutine(AnimalVisit());
            }
        }

        IEnumerator AnimalVisit()
        {
            // Find a sapling to convert to bonus sapling
            List<int> saplings = new List<int>();
            for (int i = 0; i < _grid.Length; i++)
                if (_grid[i] == CellState.Sapling) saplings.Add(i);
            if (saplings.Count == 0) yield break;

            int idx = saplings[Random.Range(0, saplings.Count)];
            // Show animal briefly
            if (_sprAnimal == null) yield break;
            var animalObj = new GameObject("AnimalVisitor");
            int col = idx % Cols; int row = idx / Cols;
            animalObj.transform.position = new Vector3(
                _gridOrigin.x + col * _cellSize + _cellSize,
                _gridOrigin.y - row * _cellSize, 0f);
            var sr = animalObj.AddComponent<SpriteRenderer>();
            sr.sprite = _sprAnimal;
            float sc = _cellSize * 0.7f / Mathf.Max(_sprAnimal.bounds.size.x, _sprAnimal.bounds.size.y);
            animalObj.transform.localScale = Vector3.one * sc;
            sr.sortingOrder = 5;

            yield return new WaitForSeconds(0.8f);
            Destroy(animalObj);

            _grid[idx] = CellState.BonusSapling;
            RefreshCell(idx);
        }

        void HandleStorm()
        {
            if (!_stormEnabled) return;
            if (_stormActive)
            {
                _stormDuration -= Time.deltaTime;
                if (_stormDuration <= 0f)
                {
                    _stormActive = false;
                    _stormDuration = 10f;
                    _ui.HideStorm();
                }
                else
                {
                    // Random tree may wither
                    if (Random.value < 0.2f * Time.deltaTime)
                        WitherRandomTree();
                }
            }
            else
            {
                _stormTimer -= Time.deltaTime;
                if (_stormTimer <= 0f)
                {
                    _stormTimer = _stormInterval;
                    _stormActive = true;
                    _stormDuration = 10f;
                    _ui.ShowStorm();
                }
            }
        }

        void WitherRandomTree()
        {
            List<int> trees = new List<int>();
            for (int i = 0; i < _grid.Length; i++)
                if (_grid[i] == CellState.Oak || _grid[i] == CellState.Birch || _grid[i] == CellState.Pine)
                    trees.Add(i);
            if (trees.Count == 0) return;
            int idx = trees[Random.Range(0, trees.Count)];
            _grid[idx] = CellState.Withered;
            _totalTrees = Mathf.Max(0, _totalTrees - 1);
            RefreshCell(idx);
            _ui.UpdateArea(_totalTrees, _targetTrees);
        }

        void HandleWorldTree()
        {
            if (!_worldTreeEnabled || !_worldTreeGrowing) return;
            _worldTreeTimer -= Time.deltaTime;
            _ui.UpdateWorldTreeProgress(1f - _worldTreeTimer / _worldTreeGrowDuration);
            if (_worldTreeTimer <= 0f)
            {
                _worldTreeGrowing = false;
                _gameManager.OnWorldTreeCompleted();
            }
        }

        void CheckWorldTreeCondition()
        {
            if (!_worldTreeEnabled || _worldTreeGrowing || _worldTreeCell >= 0) return;
            bool hasOak = false, hasBirch = false, hasPine = false;
            for (int i = 0; i < _grid.Length; i++)
            {
                if (_grid[i] == CellState.Oak) hasOak = true;
                if (_grid[i] == CellState.Birch) hasBirch = true;
                if (_grid[i] == CellState.Pine) hasPine = true;
            }
            if (hasOak && hasBirch && hasPine)
            {
                // Find closest empty cell
                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Cols; c++)
                    {
                        int i = r * Cols + c;
                        if (_grid[i] == CellState.Empty)
                        {
                            _worldTreeCell = i;
                            _grid[i] = CellState.WorldTree;
                            RefreshCell(i);
                            _worldTreeGrowing = true;
                            _worldTreeTimer = _worldTreeGrowDuration;
                            _ui.ShowWorldTreeGrowing();
                            return;
                        }
                    }
                }
            }
        }

        void CheckStageClear()
        {
            if (_worldTreeEnabled) return; // Stage 5 uses world tree completion
            if (_totalTrees >= _targetTrees)
                _gameManager.OnStageClear();
        }

        void HandleComboTimeout()
        {
            if (_combo <= 0) return;
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0f)
            {
                _combo = 0;
                _ui.UpdateCombo(0, 1f);
            }
        }

        public void PurchaseAutoGrow()
        {
            if (_autoGrowPurchased || _mana < _autoGrowCost) return;
            _mana -= _autoGrowCost;
            _autoGrowPurchased = true;
            _ui.UpdateMana(_mana);
            _ui.SetAutoGrowButtonVisible(false);
        }

        public void ResetAll()
        {
            _mana = 0;
            _totalTrees = 0;
            _combo = 0;
            _autoGrowPurchased = false;
            _stormActive = false;
            _worldTreeGrowing = false;
        }

        IEnumerator TapEffect(int idx)
        {
            if (_cellObjects[idx] == null) yield break;
            var t = _cellObjects[idx].transform;
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                if (_cellObjects[idx] == null) yield break;
                elapsed += Time.deltaTime;
                float r = elapsed / 0.3f;
                float s = r < 0.5f ? Mathf.Lerp(1f, 1.4f, r * 2f) : Mathf.Lerp(1.4f, 1f, (r - 0.5f) * 2f);
                t.localScale = orig * s;
                yield return null;
            }
            if (_cellObjects[idx] != null) t.localScale = orig;
        }

        IEnumerator GrowEffect(int idx)
        {
            if (_cellObjects[idx] == null) yield break;
            var sr = _cellObjects[idx].GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = new Color(0.5f, 1f, 0.5f, 1f);
            yield return new WaitForSeconds(0.2f);
            if (sr != null) sr.color = orig;
        }
    }
}
