using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game004v2_WordCrystal
{
    public class WordManager : MonoBehaviour
    {
        [SerializeField] private GameObject _crystalNormalPrefab;
        [SerializeField] private GameObject _crystalHiddenPrefab;
        [SerializeField] private GameObject _crystalBonusPrefab;
        [SerializeField] private GameObject _crystalPoisonPrefab;
        [SerializeField] private GameObject _letterTilePrefab;

        public event System.Action<string, int, bool> OnWordResult; // word, score, isCorrect
        public event System.Action OnPoisonHit; // -5秒
        public event System.Action<char[]> OnSlotChanged;

        private WordCrystalGameManager _gameManager;
        private List<CrystalObject> _crystals = new List<CrystalObject>();
        private List<LetterTile> _activeTiles = new List<LetterTile>();
        private char[] _slots = new char[8];
        private bool[] _slotBonus = new bool[8];
        private int _slotCount;
        private bool _isActive;

        private StageConfig _currentConfig;

        // ステージ別単語リスト
        private static readonly Dictionary<int, string[]> _wordLists = new Dictionary<int, string[]>
        {
            { 1, new[] { "cat", "dog", "hat", "run", "sun", "map", "cup", "bat", "car", "fun", "red", "big", "hot", "wet", "old" } },
            { 2, new[] { "play", "blue", "fish", "jump", "rock", "cake", "help", "time", "warm", "gold", "fast", "dark", "rain" } },
            { 3, new[] { "house", "table", "music", "dance", "light", "fresh", "brain", "chair", "plant", "stone", "bread" } },
            { 4, new[] { "breath", "finger", "garden", "mirror", "pencil", "bridge", "castle", "flower", "window", "school" } },
            { 5, new[] { "shark", "eagle", "horse", "tiger", "whale", "snake", "zebra", "panda", "koala", "rabbit", "parrot" } },
        };

        private HashSet<string> _validWords = new HashSet<string>();
        private float _cellSize;
        private Vector3 _gridOrigin;
        private Camera _camera;
        private LetterTile[] _slotTiles = new LetterTile[8];

        private void Awake()
        {
            _gameManager = GetComponentInParent<WordCrystalGameManager>();
            _camera = Camera.main;
        }

        public class StageConfig
        {
            public int stageIndex;
            public int timeLimit;
            public int targetScore;
            public int crystalCount;
            public bool hasHidden;
            public bool hasBonus;
            public bool hasPoison;
            public string theme;
        }

        public void SetupStage(StageConfig config)
        {
            _currentConfig = config;
            _isActive = false;
            ClearAll();

            // 単語リスト構築（全ステージ分）
            _validWords.Clear();
            foreach (var kv in _wordLists)
                foreach (var w in kv.Value)
                    _validWords.Add(w.ToLower());

            // レスポンシブ配置
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) { Debug.LogError("[WordManager] Camera.main is null"); return; }
            var cam = _camera;
            float camSize = cam.orthographicSize;
            float camWidth = camSize * cam.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 3.5f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            int cols = 3;
            int rows = Mathf.CeilToInt(config.crystalCount / (float)cols);
            _cellSize = Mathf.Min(availableHeight / rows, camWidth * 2f / cols, 1.8f);

            float gridW = cols * _cellSize;
            float gridH = rows * _cellSize;
            _gridOrigin = new Vector3(
                -gridW * 0.5f + _cellSize * 0.5f,
                camSize - topMargin - _cellSize * 0.5f,
                0f);

            SpawnCrystals(config);
            _slotCount = 0;
            _slots = new char[8];
            _slotBonus = new bool[8];
            _isActive = true;
        }

        private void SpawnCrystals(StageConfig config)
        {
            int total = config.crystalCount;
            int bonusCount = config.hasBonus ? Mathf.Max(1, total / 5) : 0;
            int poisonCount = config.hasPoison ? Mathf.Max(1, total / 6) : 0;
            int hiddenCount = config.hasHidden ? Mathf.Max(1, total / 4) : 0;
            int normalCount = Mathf.Max(0, total - bonusCount - poisonCount - hiddenCount);

            var types = new List<CrystalType>();
            for (int i = 0; i < normalCount; i++) types.Add(CrystalType.Normal);
            for (int i = 0; i < hiddenCount; i++) types.Add(CrystalType.Hidden);
            for (int i = 0; i < bonusCount; i++) types.Add(CrystalType.Bonus);
            for (int i = 0; i < poisonCount; i++) types.Add(CrystalType.Poison);

            // シャッフル
            for (int i = types.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (types[i], types[j]) = (types[j], types[i]);
            }

            int cols = 3;
            for (int i = 0; i < types.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                Vector3 pos = _gridOrigin + new Vector3(col * _cellSize, -row * _cellSize, 0f);
                SpawnCrystal(types[i], pos);
            }
        }

        private void SpawnCrystal(CrystalType type, Vector3 pos)
        {
            GameObject prefab = type switch
            {
                CrystalType.Hidden => _crystalHiddenPrefab,
                CrystalType.Bonus => _crystalBonusPrefab,
                CrystalType.Poison => _crystalPoisonPrefab,
                _ => _crystalNormalPrefab
            };
            if (prefab == null) prefab = _crystalNormalPrefab;

            var obj = Instantiate(prefab, pos, Quaternion.identity, transform);
            obj.transform.localScale = Vector3.one * _cellSize * 0.85f;
            var crystal = obj.GetComponent<CrystalObject>();
            if (crystal != null)
            {
                crystal.Initialize(type, this);
                _crystals.Add(crystal);
            }
        }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (_camera == null) return;
            Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            worldPos.z = 0f;

            // クリスタルチェック
            var col = Physics2D.OverlapPoint(worldPos);
            if (col == null) return;

            var crystal = col.GetComponent<CrystalObject>();
            if (crystal != null && !crystal.IsDestroyed)
            {
                HandleCrystalTap(crystal);
                return;
            }

            var tile = col.GetComponent<LetterTile>();
            if (tile != null && !tile.IsUsed)
            {
                HandleTileTap(tile);
                return;
            }
        }

        private void HandleCrystalTap(CrystalObject crystal)
        {
            if (crystal.CrystalType == CrystalType.Poison)
            {
                crystal.PlayDestroyAnimation();
                _crystals.Remove(crystal);
                OnPoisonHit?.Invoke();
                return;
            }

            // 文字を決定
            char letter = GetRandomLetter(crystal.CrystalType == CrystalType.Bonus);
            bool isBonus = crystal.CrystalType == CrystalType.Bonus;

            crystal.PlayDestroyAnimation();
            _crystals.Remove(crystal);

            // LetterTile生成
            if (_letterTilePrefab != null)
            {
                var tileObj = Instantiate(_letterTilePrefab, crystal.transform.position, Quaternion.identity, transform);
                tileObj.transform.localScale = Vector3.one * _cellSize * 0.75f;
                var tile = tileObj.GetComponent<LetterTile>();
                if (tile != null)
                {
                    tile.Initialize(letter, isBonus);
                    _activeTiles.Add(tile);
                }
            }
        }

        private void HandleTileTap(LetterTile tile)
        {
            if (_slotCount >= 8) return;
            tile.SetUsed(true);
            _slots[_slotCount] = tile.Letter;
            _slotBonus[_slotCount] = tile.IsBonus;
            _slotTiles[_slotCount] = tile;
            _slotCount++;
            OnSlotChanged?.Invoke(GetCurrentSlots());
            StartCoroutine(SlotBounce());
        }

        public void RemoveSlotAt(int index)
        {
            if (index < 0 || index >= _slotCount) return;

            var tileToRestore = _slotTiles[index];
            if (tileToRestore != null) tileToRestore.SetUsed(false);

            for (int i = index; i < _slotCount - 1; i++)
            {
                _slots[i] = _slots[i + 1];
                _slotBonus[i] = _slotBonus[i + 1];
                _slotTiles[i] = _slotTiles[i + 1];
            }
            _slotTiles[_slotCount - 1] = null;
            _slotCount--;
            OnSlotChanged?.Invoke(GetCurrentSlots());
        }

        public void ClearSlots()
        {
            for (int i = 0; i < _slotCount; i++)
                if (_slotTiles[i] != null) _slotTiles[i].SetUsed(false);
            _slotCount = 0;
            _slots = new char[8];
            _slotBonus = new bool[8];
            _slotTiles = new LetterTile[8];
            OnSlotChanged?.Invoke(GetCurrentSlots());
        }

        public (int score, bool isCorrect) SubmitWord()
        {
            if (_slotCount < 3) return (0, false);

            string word = new string(_slots, 0, _slotCount).ToLower();
            bool isValid = _validWords.Contains(word);

            // テーマ縛りチェック(Stage5)
            if (isValid && _currentConfig?.theme == "animal")
            {
                if (_wordLists.TryGetValue(5, out var animalWords))
                    isValid = System.Array.Exists(animalWords, w => w == word);
            }

            int score = 0;
            if (isValid)
            {
                score = _slotCount switch
                {
                    3 => 100,
                    4 => 250,
                    5 => 500,
                    _ => 800
                };
                // ボーナス文字チェック
                bool hasBonus = false;
                for (int i = 0; i < _slotCount; i++)
                    if (_slotBonus[i]) { hasBonus = true; break; }
                if (hasBonus) score *= 2;

                // 正解時はタイルを削除
                RemoveUsedTiles();
            }

            ClearSlots();
            OnWordResult?.Invoke(word, score, isValid);
            return (score, isValid);
        }

        private void RemoveUsedTiles()
        {
            var toRemove = new List<LetterTile>();
            foreach (var t in _activeTiles)
                if (t != null && t.IsUsed) toRemove.Add(t);
            foreach (var t in toRemove)
            {
                _activeTiles.Remove(t);
                if (t != null) Destroy(t.gameObject);
            }
        }

        private char GetRandomLetter(bool bonus)
        {
            // 英語で使われやすい文字に重み付け
            string common = "AAABBBCCDDEEEEFFFGGHHIIIJJKKLLLMMNNNOOOOPPPRRRSSSTTTUUUVWXY";
            return common[Random.Range(0, common.Length)];
        }

        private char[] GetCurrentSlots()
        {
            var result = new char[_slotCount];
            for (int i = 0; i < _slotCount; i++) result[i] = _slots[i];
            return result;
        }

        private IEnumerator SlotBounce()
        {
            // UI側でアニメーション
            yield return null;
        }

        private void ClearAll()
        {
            foreach (var c in _crystals)
                if (c != null) Destroy(c.gameObject);
            _crystals.Clear();

            foreach (var t in _activeTiles)
                if (t != null) Destroy(t.gameObject);
            _activeTiles.Clear();

            _slotCount = 0;
            _slots = new char[8];
            _slotBonus = new bool[8];
            _slotTiles = new LetterTile[8];
        }

        public void SetActive(bool active) => _isActive = active;

        private void OnDestroy() => ClearAll();
    }
}
