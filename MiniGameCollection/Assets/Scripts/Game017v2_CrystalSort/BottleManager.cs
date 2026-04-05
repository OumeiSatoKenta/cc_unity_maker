using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game017v2_CrystalSort
{
    public enum CrystalColor { Red, Blue, Green, Yellow, Purple, Orange, Rainbow, None }
    public enum CrystalType { Normal, Rainbow, Frozen }
    public enum BottleType { Normal, Capped, Timer }

    [System.Serializable]
    public class CrystalData
    {
        public CrystalColor color;
        public CrystalType type;
        public bool isFrozen;
    }

    public class BottleView
    {
        public int index;
        public BottleType bottleType;
        public bool capOpen;
        public int timerCountdown;
        public List<CrystalData> crystals = new List<CrystalData>();
        public int capacity;

        public bool IsEmpty => crystals.Count == 0;
        public bool IsFull => crystals.Count >= capacity;
        public CrystalData TopCrystal => crystals.Count > 0 ? crystals[crystals.Count - 1] : null;
        public bool IsComplete
        {
            get
            {
                if (IsEmpty) return false;
                if (crystals.Count != capacity) return false;
                var col = crystals[0].color;
                foreach (var c in crystals)
                    if (c.color != col) return false;
                return true;
            }
        }
    }

    public class BottleManager : MonoBehaviour
    {
        [SerializeField] Sprite _spBottle;
        [SerializeField] Sprite _spBottleCapped;
        [SerializeField] Sprite _spBottleTimer;
        [SerializeField] Sprite _spBottleSelected;
        [SerializeField] Sprite _spBottleComplete;
        [SerializeField] Sprite _spCrystalRed;
        [SerializeField] Sprite _spCrystalBlue;
        [SerializeField] Sprite _spCrystalGreen;
        [SerializeField] Sprite _spCrystalYellow;
        [SerializeField] Sprite _spCrystalPurple;
        [SerializeField] Sprite _spCrystalOrange;
        [SerializeField] Sprite _spCrystalRainbow;
        [SerializeField] Sprite _spCrystalFrozen;

        [SerializeField] CrystalSortUI _ui;
        CrystalSortGameManager _gm;

        List<BottleView> _bottles = new List<BottleView>();
        List<GameObject> _bottleObjects = new List<GameObject>();
        List<List<GameObject>> _crystalObjects = new List<List<GameObject>>();

        int _selectedBottleIndex = -1;
        int _moves;
        int _maxMoves;
        int _stageIndex;
        int _consecutiveSameColorMoves;
        CrystalColor _lastMovedColor = CrystalColor.None;
        bool _isActive;
        bool _hasRainbow;
        bool _hasFrozen;
        bool _hasCapped;
        bool _hasTimer;

        void Awake()
        {
            _gm = GetComponentInParent<CrystalSortGameManager>();
            if (_gm == null) Debug.LogError("[BottleManager] CrystalSortGameManager not found in parent hierarchy.");
        }

        public void SetupStage(StageManager.StageConfig config, int stageNumber)
        {
            ClearAll();
            _stageIndex = stageNumber;
            _selectedBottleIndex = -1;
            _consecutiveSameColorMoves = 0;
            _lastMovedColor = CrystalColor.None;

            // Determine parameters by stage
            int colorCount, bottleCount, emptyBottles, capacity, maxMoves;
            _hasRainbow = false;
            _hasFrozen = false;
            _hasCapped = false;
            _hasTimer = false;

            switch (stageNumber)
            {
                case 1: colorCount=3; bottleCount=4; emptyBottles=1; capacity=4; maxMoves=20; break;
                case 2: colorCount=4; bottleCount=5; emptyBottles=1; capacity=4; maxMoves=25; _hasCapped=true; break;
                case 3: colorCount=5; bottleCount=6; emptyBottles=1; capacity=4; maxMoves=35; _hasRainbow=true; break;
                case 4: colorCount=5; bottleCount=7; emptyBottles=2; capacity=5; maxMoves=45; _hasFrozen=true; break;
                case 5: colorCount=6; bottleCount=8; emptyBottles=1; capacity=4; maxMoves=55; _hasRainbow=true; _hasFrozen=true; _hasCapped=true; _hasTimer=true; break;
                default: colorCount=3; bottleCount=4; emptyBottles=1; capacity=4; maxMoves=20; break;
            }

            _moves = 0;
            _maxMoves = maxMoves;

            // Generate bottle data
            GenerateBottleData(colorCount, bottleCount, emptyBottles, capacity, stageNumber);

            // Create GameObjects
            CreateBottleObjects();

            _isActive = true;
            _ui.UpdateMoves(_moves, _maxMoves);
            _ui.UpdateBottleCount(0, bottleCount - emptyBottles);
        }

        void GenerateBottleData(int colorCount, int bottleCount, int emptyBottles, int capacity, int stage)
        {
            // Create crystal pool
            var pool = new List<CrystalData>();
            int filledBottles = bottleCount - emptyBottles;

            // Add rainbow crystal if applicable
            int rainbowCount = _hasRainbow ? 1 : 0;
            int frozenCount = _hasFrozen ? 2 : 0;

            // Regular crystals
            for (int c = 0; c < colorCount; c++)
            {
                int crystalsForColor = capacity;
                if (c == 0 && rainbowCount > 0) crystalsForColor -= rainbowCount;
                for (int i = 0; i < crystalsForColor; i++)
                    pool.Add(new CrystalData { color = (CrystalColor)c, type = CrystalType.Normal });
            }

            // Rainbow crystals
            for (int i = 0; i < rainbowCount; i++)
                pool.Add(new CrystalData { color = CrystalColor.Rainbow, type = CrystalType.Rainbow });

            // Frozen crystals (replace some regular ones)
            if (frozenCount > 0)
            {
                int replaced = 0;
                for (int i = 0; i < pool.Count && replaced < frozenCount; i++)
                {
                    if (pool[i].type == CrystalType.Normal && pool[i].color != CrystalColor.Rainbow)
                    {
                        pool[i] = new CrystalData { color = pool[i].color, type = CrystalType.Frozen, isFrozen = true };
                        replaced++;
                    }
                }
            }

            // Shuffle
            var rng = new System.Random(42 + stage * 13);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
            }

            // Distribute to bottles
            _bottles.Clear();
            int poolIdx = 0;
            for (int b = 0; b < bottleCount; b++)
            {
                var bv = new BottleView { index = b, capacity = capacity };

                if (b < filledBottles && poolIdx < pool.Count)
                {
                    int take = Mathf.Min(capacity, pool.Count - poolIdx);
                    for (int i = 0; i < take; i++)
                        bv.crystals.Add(pool[poolIdx++]);
                }

                // Assign special bottle types
                if (_hasCapped && b == 1)
                    bv.bottleType = BottleType.Capped;
                else if (_hasTimer && b == 2)
                {
                    bv.bottleType = BottleType.Timer;
                    bv.timerCountdown = 8;
                }
                else
                    bv.bottleType = BottleType.Normal;

                _bottles.Add(bv);
            }
        }

        void CreateBottleObjects()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 2.8f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float availableWidth = camWidth * 2f - 0.5f;

            int count = _bottles.Count;
            int cols = count <= 4 ? count : (count <= 6 ? 3 : 4);
            int rows = Mathf.CeilToInt((float)count / cols);

            float bottleWidth = availableWidth / cols;
            float bottleHeight = availableHeight / rows;
            // bottleScale based on fitting a 0.5w x 1.0h sprite unit
            float scaleX = bottleWidth / 1.0f;
            float scaleY = bottleHeight / 2.0f;
            float bottleScale = Mathf.Min(scaleX, scaleY, 0.7f);

            float startX = -(cols - 1) * bottleWidth * 0.5f;
            float startY = camSize - topMargin - bottleHeight * 0.5f;

            _bottleObjects.Clear();
            _crystalObjects.Clear();

            for (int i = 0; i < count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                float x = startX + col * bottleWidth;
                float y = startY - row * bottleHeight;

                var bv = _bottles[i];
                var go = new GameObject($"Bottle_{i}");
                go.transform.position = new Vector3(x, y, 0);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = GetBottleSprite(bv);
                sr.sortingOrder = 1;
                go.transform.localScale = Vector3.one * bottleScale;

                var col2d = go.AddComponent<BoxCollider2D>();
                col2d.size = new Vector2(0.9f / bottleScale, 1.8f / bottleScale);

                _bottleObjects.Add(go);

                // Create crystals
                var crystalList = new List<GameObject>();
                float crystalScale = bottleScale * 0.5f;
                float crystalSpacing = 0.45f * bottleScale;
                float crystalStartY = y - 0.5f * bottleScale * (bv.capacity - 1);

                for (int ci = 0; ci < bv.crystals.Count; ci++)
                {
                    var cd = bv.crystals[ci];
                    var cgo = new GameObject($"Crystal_{i}_{ci}");
                    cgo.transform.position = new Vector3(x, crystalStartY + ci * crystalSpacing, -0.1f);
                    var csr = cgo.AddComponent<SpriteRenderer>();
                    csr.sprite = GetCrystalSprite(cd);
                    csr.sortingOrder = 2;
                    cgo.transform.localScale = Vector3.one * crystalScale;
                    if (cd.isFrozen)
                        csr.color = new Color(0.6f, 0.8f, 1f, 0.85f);
                    crystalList.Add(cgo);
                }
                _crystalObjects.Add(crystalList);
            }
        }

        Sprite GetBottleSprite(BottleView bv)
        {
            if (bv.IsComplete) return _spBottleComplete ?? _spBottle;
            if (bv.bottleType == BottleType.Capped && !bv.capOpen) return _spBottleCapped ?? _spBottle;
            if (bv.bottleType == BottleType.Timer) return _spBottleTimer ?? _spBottle;
            return _spBottle;
        }

        Sprite GetCrystalSprite(CrystalData cd)
        {
            if (cd.isFrozen) return _spCrystalFrozen ?? _spCrystalBlue;
            if (cd.type == CrystalType.Rainbow) return _spCrystalRainbow;
            return cd.color switch
            {
                CrystalColor.Red => _spCrystalRed,
                CrystalColor.Blue => _spCrystalBlue,
                CrystalColor.Green => _spCrystalGreen,
                CrystalColor.Yellow => _spCrystalYellow,
                CrystalColor.Purple => _spCrystalPurple,
                CrystalColor.Orange => _spCrystalOrange,
                _ => _spCrystalBlue
            };
        }

        void ClearAll()
        {
            _isActive = false;
            foreach (var go in _bottleObjects)
                if (go != null) Destroy(go);
            foreach (var list in _crystalObjects)
                foreach (var go in list)
                    if (go != null) Destroy(go);
            _bottleObjects.Clear();
            _crystalObjects.Clear();
            _bottles.Clear();
        }

        void Update()
        {
            if (!_isActive) return;
            if (_gm.State != CrystalSortGameManager.GameState.Playing) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            var screenPos = Mouse.current.position.ReadValue();
            var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            var hit = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));
            if (hit == null) { DeselectBottle(); return; }

            // Find which bottle was hit
            int hitIndex = -1;
            for (int i = 0; i < _bottleObjects.Count; i++)
            {
                if (_bottleObjects[i] == hit.gameObject) { hitIndex = i; break; }
            }
            if (hitIndex < 0) { DeselectBottle(); return; }

            if (_selectedBottleIndex < 0)
            {
                TrySelectBottle(hitIndex);
            }
            else if (hitIndex == _selectedBottleIndex)
            {
                DeselectBottle();
            }
            else
            {
                TryMove(_selectedBottleIndex, hitIndex);
            }
        }

        void TrySelectBottle(int index)
        {
            var bv = _bottles[index];
            if (bv.IsEmpty) return;
            if (bv.bottleType == BottleType.Capped && !bv.capOpen)
            {
                // Open cap
                bv.capOpen = true;
                UpdateBottleSprite(index);
                _moves++;
                _ui.UpdateMoves(_moves, _maxMoves);
                UpdateTimerBottles();
                CheckGameOver();
                return;
            }
            var top = bv.TopCrystal;
            if (top == null) return;
            if (top.isFrozen) return; // Cannot move frozen crystal

            _selectedBottleIndex = index;
            // Visual: scale up top crystal
            if (_crystalObjects[index].Count > 0)
            {
                var cgo = _crystalObjects[index][_crystalObjects[index].Count - 1];
                StartCoroutine(ScalePop(cgo.transform, 1.0f, 1.3f, 0.1f));
                // Lift up
                var pos = cgo.transform.position;
                cgo.transform.position = new Vector3(pos.x, pos.y + 0.15f, pos.z - 0.2f);
            }
            // Update bottle sprite to selected
            var sr = _bottleObjects[index].GetComponent<SpriteRenderer>();
            if (sr != null && _spBottleSelected != null) sr.sprite = _spBottleSelected;
        }

        void DeselectBottle()
        {
            if (_selectedBottleIndex < 0) return;
            int idx = _selectedBottleIndex;
            _selectedBottleIndex = -1;

            // Restore crystal position
            if (idx < _crystalObjects.Count && idx < _bottles.Count && _crystalObjects[idx].Count > 0)
            {
                var cgo = _crystalObjects[idx][_crystalObjects[idx].Count - 1];
                var bottlePos = _bottleObjects[idx].transform.position;
                float bottleScale = _bottleObjects[idx].transform.localScale.x;
                float crystalSpacing = 0.45f * bottleScale;
                float crystalStartY = bottlePos.y - 0.5f * bottleScale * (_bottles[idx].capacity - 1);
                int ci = _bottles[idx].crystals.Count - 1;
                cgo.transform.position = new Vector3(bottlePos.x, crystalStartY + ci * crystalSpacing, -0.1f);
                StartCoroutine(ScalePop(cgo.transform, 1.3f, 1.0f, 0.1f));
            }
            UpdateBottleSprite(idx);
        }

        void TryMove(int fromIndex, int toIndex)
        {
            var from = _bottles[fromIndex];
            var to = _bottles[toIndex];
            var crystal = from.TopCrystal;
            if (crystal == null) { DeselectBottle(); return; }

            // Check capped
            if (to.bottleType == BottleType.Capped && !to.capOpen) { FlashFail(fromIndex); DeselectBottle(); return; }

            // Check capacity
            if (to.IsFull) { FlashFail(fromIndex); DeselectBottle(); return; }

            // Check color rule
            bool canPlace = false;
            if (to.IsEmpty) canPlace = true;
            else
            {
                var toTop = to.TopCrystal;
                if (crystal.type == CrystalType.Rainbow) canPlace = true;
                else if (toTop.type == CrystalType.Rainbow) canPlace = true;
                else if (crystal.color == toTop.color && !toTop.isFrozen) canPlace = true;
            }

            if (!canPlace) { FlashFail(fromIndex); DeselectBottle(); return; }

            // Restore selected state of from bottle
            _selectedBottleIndex = -1;

            // Execute move
            from.crystals.RemoveAt(from.crystals.Count - 1);
            to.crystals.Add(crystal);

            // Moves counter
            _moves++;
            _ui.UpdateMoves(_moves, _maxMoves);

            // Thaw check: if two adjacent same-color in same bottle
            CheckThaw(to);

            // Rebuild visuals
            RebuildBottleVisuals(fromIndex);
            RebuildBottleVisuals(toIndex);

            // Combo tracking (skip Rainbow crystals — they shouldn't reset color chain)
            if (crystal.type == CrystalType.Normal)
            {
                if (crystal.color == _lastMovedColor)
                {
                    _consecutiveSameColorMoves++;
                    if (_consecutiveSameColorMoves >= 2)
                        _gm.OnComboMove(_consecutiveSameColorMoves);
                }
                else
                {
                    _consecutiveSameColorMoves = 1;
                    _lastMovedColor = crystal.color;
                }
            }

            // Check bottle complete
            if (to.IsComplete)
            {
                StartCoroutine(FlashComplete(toIndex));
                _gm.OnBottleCompleted();
                _ui.UpdateBottleCount(CountCompleted(), CountNonEmpty());
            }

            // Update timer bottles
            UpdateTimerBottles();

            // Check win/lose
            if (CheckWin())
            {
                _isActive = false;
                _gm.OnStageClear(_maxMoves - _moves, _maxMoves);
                return;
            }
            CheckGameOver();
        }

        void CheckThaw(BottleView bv)
        {
            for (int i = 1; i < bv.crystals.Count; i++)
            {
                var c1 = bv.crystals[i - 1];
                var c2 = bv.crystals[i];
                if (c2.isFrozen && c1.color == c2.color)
                {
                    c2.isFrozen = false;
                }
            }
        }

        void UpdateTimerBottles()
        {
            for (int i = 0; i < _bottles.Count; i++)
            {
                var bv = _bottles[i];
                if (bv.bottleType != BottleType.Timer || bv.capOpen) continue;
                bv.timerCountdown--;
                if (bv.timerCountdown <= 0)
                {
                    bv.capOpen = false;
                    UpdateBottleSprite(i);
                }
            }
        }

        bool CheckWin()
        {
            foreach (var bv in _bottles)
            {
                if (bv.IsEmpty) continue;
                if (!bv.IsComplete) return false;
            }
            return true;
        }

        void CheckGameOver()
        {
            if (_moves >= _maxMoves)
            {
                _isActive = false;
                _gm.OnGameOver();
                return;
            }
            // Deadlock check: can any move be made?
            bool anyMove = false;
            for (int from = 0; from < _bottles.Count && !anyMove; from++)
            {
                var bf = _bottles[from];
                if (bf.IsEmpty) continue;
                var top = bf.TopCrystal;
                if (top == null || top.isFrozen) continue;
                if (bf.bottleType == BottleType.Capped && !bf.capOpen) continue;
                for (int to = 0; to < _bottles.Count && !anyMove; to++)
                {
                    if (from == to) continue;
                    var bt = _bottles[to];
                    if (bt.IsFull) continue;
                    if (bt.bottleType == BottleType.Capped && !bt.capOpen) continue;
                    if (bt.IsEmpty) { anyMove = true; break; }
                    var toTop = bt.TopCrystal;
                    if (top.type == CrystalType.Rainbow || toTop.type == CrystalType.Rainbow || top.color == toTop.color)
                        anyMove = true;
                }
            }
            if (!anyMove)
            {
                _isActive = false;
                _gm.OnGameOver();
            }
        }

        int CountCompleted()
        {
            int n = 0;
            foreach (var bv in _bottles) if (bv.IsComplete) n++;
            return n;
        }

        int CountNonEmpty()
        {
            int n = 0;
            foreach (var bv in _bottles) if (!bv.IsEmpty) n++;
            return n;
        }

        void RebuildBottleVisuals(int index)
        {
            var bv = _bottles[index];
            var bottleGO = _bottleObjects[index];
            float bottleScale = bottleGO.transform.localScale.x;
            float crystalSpacing = 0.45f * bottleScale;
            float crystalStartY = bottleGO.transform.position.y - 0.5f * bottleScale * (bv.capacity - 1);

            // Remove old crystal objects
            foreach (var cgo in _crystalObjects[index])
                if (cgo != null) Destroy(cgo);
            _crystalObjects[index].Clear();

            // Rebuild
            for (int ci = 0; ci < bv.crystals.Count; ci++)
            {
                var cd = bv.crystals[ci];
                var cgo = new GameObject($"Crystal_{index}_{ci}");
                cgo.transform.position = new Vector3(bottleGO.transform.position.x, crystalStartY + ci * crystalSpacing, -0.1f);
                var csr = cgo.AddComponent<SpriteRenderer>();
                csr.sprite = GetCrystalSprite(cd);
                csr.sortingOrder = 2;
                cgo.transform.localScale = Vector3.one * bottleScale * 0.5f;
                if (cd.isFrozen) csr.color = new Color(0.6f, 0.8f, 1f, 0.85f);
                _crystalObjects[index].Add(cgo);
            }

            UpdateBottleSprite(index);
        }

        void UpdateBottleSprite(int index)
        {
            var bv = _bottles[index];
            var sr = _bottleObjects[index].GetComponent<SpriteRenderer>();
            if (sr == null) return;
            sr.sprite = GetBottleSprite(bv);
        }

        void FlashFail(int index)
        {
            if (index >= _crystalObjects.Count || _crystalObjects[index].Count == 0) return;
            var cgo = _crystalObjects[index][_crystalObjects[index].Count - 1];
            var csr = cgo.GetComponent<SpriteRenderer>();
            if (csr != null) StartCoroutine(FlashColor(csr, Color.red, 0.15f));
        }

        IEnumerator FlashComplete(int index)
        {
            if (!_isActive) yield break;
            if (index >= _bottleObjects.Count || _bottleObjects[index] == null) yield break;
            var sr = _bottleObjects[index].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var orig = sr.color;
                float t = 0;
                while (t < 0.3f)
                {
                    if (_bottleObjects[index] == null) yield break;
                    t += Time.deltaTime;
                    sr.color = Color.Lerp(Color.yellow, orig, t / 0.3f);
                    yield return null;
                }
                if (sr != null) sr.color = orig;
            }
            // Bounce all crystals in bottle
            if (index < _crystalObjects.Count)
            {
                foreach (var cgo in _crystalObjects[index])
                {
                    if (cgo != null) StartCoroutine(ScalePop(cgo.transform, 1.0f, 1.1f, 0.15f));
                }
            }
        }

        IEnumerator ScalePop(Transform t, float from, float to, float duration)
        {
            if (t == null) yield break;
            float elapsed = 0;
            while (elapsed < duration)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float ratio = elapsed / duration;
                t.localScale = Vector3.one * Mathf.Lerp(from, to, ratio);
                yield return null;
            }
            if (t != null) t.localScale = Vector3.one * to;
        }

        IEnumerator FlashColor(SpriteRenderer sr, Color flashColor, float duration)
        {
            if (sr == null) yield break;
            var orig = sr.color;
            sr.color = flashColor;
            yield return new WaitForSeconds(duration);
            if (sr != null) sr.color = orig;
        }
    }
}
