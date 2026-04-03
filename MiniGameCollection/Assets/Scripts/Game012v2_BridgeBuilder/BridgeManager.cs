using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game012v2_BridgeBuilder
{
    public class BridgeManager : MonoBehaviour
    {
        // ---- Part type definitions ----
        public enum PartType { None, Wood, Steel, Rope }

        [System.Serializable]
        public class PartDef
        {
            public PartType type;
            public string label;
            public int cost;
            public float strength;
            public Color color;
        }

        // ---- Span state ----
        class BridgeSpan
        {
            public int fromIndex;
            public int toIndex;
            public PartType partType;
            public float strength;
            public float currentLoad;
            public bool isDestroyed;
            public GameObject visualObj;
        }

        // ---- Serialized References ----
        [SerializeField] BridgeBuilderGameManager _gameManager;
        [SerializeField] BridgeBuilderUI _ui;
        [SerializeField] Sprite _anchorSprite;
        [SerializeField] Sprite _woodSprite;
        [SerializeField] Sprite _steelSprite;
        [SerializeField] Sprite _ropeSprite;
        [SerializeField] Sprite _carSprite;
        [SerializeField] Sprite _goalSprite;

        // ---- Stage params ----
        int _spanCount = 3;
        int _budget = 500;
        int _budgetRemaining;
        bool _heavyVehicle = false;
        float _windStrength = 0f;
        List<PartType> _availableParts = new List<PartType> { PartType.Wood };

        // ---- Building state ----
        PartType _selectedPart = PartType.Wood;
        int _firstAnchorIndex = -1;
        List<GameObject> _anchorObjects = new List<GameObject>();
        List<BridgeSpan> _spans = new List<BridgeSpan>();
        bool _isActive = false;

        // ---- Test state ----
        bool _isTesting = false;
        GameObject _carObject;
        float _carX;
        float _carSpeed = 2.0f;
        float _windTimer = 0f;
        float _anchorY;
        float _anchorSpacing;
        float _leftEdgeX;

        // ---- Part definitions ----
        static readonly PartDef[] PartDefs = {
            new PartDef { type = PartType.Wood,  label = "木材",  cost = 50,  strength = 100f, color = new Color(0.63f, 0.39f, 0.20f) },
            new PartDef { type = PartType.Steel, label = "鉄骨",  cost = 150, strength = 250f, color = new Color(0.31f, 0.39f, 0.55f) },
            new PartDef { type = PartType.Rope,  label = "ロープ", cost = 80,  strength = 180f, color = new Color(0.78f, 0.63f, 0.31f) },
        };

        static PartDef GetDef(PartType t) => System.Array.Find(PartDefs, d => d.type == t);

        // ------------------------------------------------
        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            ClearAll();

            // stageIndex is 1-based from StageManager
            int si = stageIndex;
            _spanCount = si <= 1 ? 3 : si <= 3 ? 4 + (si - 2) : 6;
            _budget = si == 1 ? 500 : si == 2 ? 400 : si == 3 ? 350 : si == 4 ? 300 : 280;
            _heavyVehicle = si >= 4;
            _windStrength = si >= 5 ? 1.5f : 0f;

            _availableParts.Clear();
            _availableParts.Add(PartType.Wood);
            if (si >= 2) _availableParts.Add(PartType.Steel);
            if (si >= 3) _availableParts.Add(PartType.Rope);

            _budgetRemaining = _budget;
            _selectedPart = PartType.Wood;
            _isActive = true;
            _isTesting = false;
            _spanTypes = new Sprite[] { _woodSprite, _steelSprite, _ropeSprite };

            PlaceAnchors();
            _ui.SetupPartButtons(_availableParts, OnPartSelected);
            _ui.UpdateBudget(_budgetRemaining, _budget);
        }

        void PlaceAnchors()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float totalAnchors = _spanCount + 1;
            _anchorY = 0.3f;
            float totalWidth = camWidth * 1.6f;
            _anchorSpacing = totalWidth / _spanCount;
            _leftEdgeX = -totalWidth / 2f;

            for (int i = 0; i <= _spanCount; i++)
            {
                float x = _leftEdgeX + i * _anchorSpacing;
                var go = new GameObject($"Anchor_{i}");
                go.transform.position = new Vector3(x, _anchorY, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _anchorSprite;
                sr.sortingOrder = 2;
                sr.transform.localScale = Vector3.one * 0.4f;
                _anchorObjects.Add(go);
            }

            // Goal flag at rightmost anchor
            if (_goalSprite != null)
            {
                var goalGo = new GameObject("Goal");
                goalGo.transform.position = new Vector3(
                    _leftEdgeX + _spanCount * _anchorSpacing + 0.3f,
                    _anchorY + 0.3f, 0f);
                var gsr = goalGo.AddComponent<SpriteRenderer>();
                gsr.sprite = _goalSprite;
                gsr.sortingOrder = 3;
                gsr.transform.localScale = Vector3.one * 0.5f;
                _anchorObjects.Add(goalGo);
            }
        }

        // ------------------------------------------------
        // Input
        // ------------------------------------------------
        void Update()
        {
            if (!_isActive) return;
            if (_isTesting)
            {
                UpdateTest();
                return;
            }
            HandleBuildInput();
        }

        void HandleBuildInput()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            worldPos.z = 0;

            // Find nearest anchor
            int nearest = FindNearestAnchor(worldPos, 0.8f);
            if (nearest < 0)
            {
                // Check if clicking on existing span
                TryDeleteSpan(worldPos);
                return;
            }

            if (_firstAnchorIndex < 0)
            {
                _firstAnchorIndex = nearest;
                HighlightAnchor(nearest, true);
            }
            else
            {
                if (_firstAnchorIndex == nearest)
                {
                    HighlightAnchor(nearest, false);
                    _firstAnchorIndex = -1;
                    return;
                }
                // Ensure adjacent
                int from = Mathf.Min(_firstAnchorIndex, nearest);
                int to = Mathf.Max(_firstAnchorIndex, nearest);
                if (to - from == 1)
                {
                    TryPlaceSpan(from, to);
                }
                HighlightAnchor(_firstAnchorIndex, false);
                _firstAnchorIndex = -1;
            }
        }

        int FindNearestAnchor(Vector3 pos, float maxDist)
        {
            int best = -1;
            float bestD = maxDist;
            for (int i = 0; i < _anchorObjects.Count; i++)
            {
                if (_anchorObjects[i].name.StartsWith("Goal")) continue;
                float d = Vector3.Distance(_anchorObjects[i].transform.position, pos);
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        void HighlightAnchor(int index, bool on)
        {
            if (index < 0 || index >= _anchorObjects.Count) return;
            var sr = _anchorObjects[index].GetComponent<SpriteRenderer>();
            if (sr) sr.color = on ? new Color(1f, 0.9f, 0.3f) : Color.white;
        }

        void TryPlaceSpan(int from, int to)
        {
            // Check if span already exists
            var existing = _spans.Find(s => s.fromIndex == from && s.toIndex == to);
            if (existing != null) return;

            var def = GetDef(_selectedPart);
            if (_budgetRemaining < def.cost)
            {
                _ui.ShowBudgetWarning();
                return;
            }

            _budgetRemaining -= def.cost;
            _ui.UpdateBudget(_budgetRemaining, _budget);

            var span = new BridgeSpan
            {
                fromIndex = from,
                toIndex = to,
                partType = _selectedPart,
                strength = def.strength,
                currentLoad = 0,
                isDestroyed = false,
            };
            span.visualObj = CreateSpanVisual(from, to, def.color);
            _spans.Add(span);

            // Pop animation
            StartCoroutine(PopScale(span.visualObj.transform));
        }

        void TryDeleteSpan(Vector3 worldPos)
        {
            for (int i = _spans.Count - 1; i >= 0; i--)
            {
                var s = _spans[i];
                if (s.visualObj == null) continue;
                float dist = DistanceToSegment(worldPos, s.visualObj.transform.position,
                    GetAnchorPos(s.fromIndex), GetAnchorPos(s.toIndex));
                if (dist < 0.25f)
                {
                    var def = GetDef(s.partType);
                    _budgetRemaining += def.cost;
                    _ui.UpdateBudget(_budgetRemaining, _budget);
                    Destroy(s.visualObj);
                    _spans.RemoveAt(i);
                    return;
                }
            }
        }

        float DistanceToSegment(Vector3 p, Vector3 center, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 0.0001f) return Vector3.Distance(p, a);
            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / len2);
            Vector3 proj = a + t * ab;
            return Vector3.Distance(p, proj);
        }

        GameObject CreateSpanVisual(int from, int to, Color color)
        {
            Vector3 posA = GetAnchorPos(from);
            Vector3 posB = GetAnchorPos(to);
            Vector3 mid = (posA + posB) * 0.5f;
            float dist = Vector3.Distance(posA, posB);
            float angle = Mathf.Atan2(posB.y - posA.y, posB.x - posA.x) * Mathf.Rad2Deg;

            var go = new GameObject($"Span_{from}_{to}");
            go.transform.position = mid;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);

            var sr = go.AddComponent<SpriteRenderer>();
            // Use sprite based on type
            sr.sprite = _spanTypes[GetSpanTypeSpriteIndex(_selectedPart)];
            sr.color = color;
            sr.sortingOrder = 1;

            float spriteWidth = sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
            float spriteHeight = sr.sprite != null ? sr.sprite.bounds.size.y : 0.25f;
            go.transform.localScale = new Vector3(dist / spriteWidth, 0.25f / spriteHeight * (sr.sprite != null ? 1f : 1f), 1f);

            return go;
        }

        Sprite[] _spanTypes;

        int GetSpanTypeSpriteIndex(PartType t)
        {
            return t == PartType.Wood ? 0 : t == PartType.Steel ? 1 : 2;
        }

        Vector3 GetAnchorPos(int index)
        {
            if (index < 0 || index >= _anchorObjects.Count) return Vector3.zero;
            return _anchorObjects[index].transform.position;
        }

        IEnumerator PopScale(Transform t)
        {
            float elapsed = 0f;
            Vector3 original = t.localScale;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / 0.15f;
                float s = ratio < 0.5f ? Mathf.Lerp(1f, 1.3f, ratio * 2f) : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                t.localScale = original * s;
                yield return null;
            }
            t.localScale = original;
        }

        // ------------------------------------------------
        // Test phase
        // ------------------------------------------------
        public void StartTest()
        {
            _isActive = true;
            _isTesting = true;
            _firstAnchorIndex = -1;

            // Check all spans present
            bool allSpanned = true;
            for (int i = 0; i < _spanCount; i++)
            {
                if (_spans.Find(s => s.fromIndex == i && s.toIndex == i + 1) == null)
                {
                    allSpanned = false;
                    break;
                }
            }
            if (!allSpanned)
            {
                // Missing spans - immediate fail
                _isTesting = false;
                _isActive = false;
                _gameManager.OnTestResult(false, (float)_budgetRemaining / _budget);
                return;
            }

            // Create car
            Vector3 startPos = GetAnchorPos(0) + new Vector3(-0.5f, 0.3f, 0);
            _carObject = new GameObject("Car");
            _carObject.transform.position = startPos;
            var sr = _carObject.AddComponent<SpriteRenderer>();
            sr.sprite = _carSprite;
            sr.sortingOrder = 5;
            _carObject.transform.localScale = Vector3.one * 0.7f;

            _carX = startPos.x;
            _carSpeed = 2.0f;
            _windTimer = 0f;

            // Reset span loads
            foreach (var s in _spans) { s.currentLoad = 0; s.isDestroyed = false; }
        }

        void UpdateTest()
        {
            if (_carObject == null) return;

            _carX += _carSpeed * Time.deltaTime;
            _windTimer += Time.deltaTime;

            // Wind effect (Stage 5)
            float windOffset = _windStrength > 0 ? Mathf.Sin(_windTimer * 2f) * _windStrength * 0.05f : 0f;
            _carObject.transform.position = new Vector3(_carX, _anchorY + 0.3f + windOffset, 0);

            // Determine which span the car is on
            int spanIndex = Mathf.FloorToInt((_carX - _leftEdgeX) / _anchorSpacing);
            spanIndex = Mathf.Clamp(spanIndex, 0, _spanCount - 1);

            // Apply load
            float vehicleLoad = _heavyVehicle ? 160f : 80f;
            ApplyLoad(spanIndex, vehicleLoad);

            // Check collapse
            bool anyDestroyed = false;
            foreach (var s in _spans)
            {
                if (s.isDestroyed)
                {
                    anyDestroyed = true;
                    break;
                }
            }

            if (anyDestroyed)
            {
                _isTesting = false;
                StartCoroutine(CollapseAndFail());
                return;
            }

            // Check goal reached
            float goalX = _leftEdgeX + _spanCount * _anchorSpacing;
            if (_carX >= goalX)
            {
                _isTesting = false;
                _isActive = false;
                if (_carObject != null) Destroy(_carObject);
                _carObject = null;
                ShowSuccessEffect();
                _gameManager.OnTestResult(true, (float)_budgetRemaining / _budget);
            }
        }

        void ApplyLoad(int spanIndex, float load)
        {
            var span = _spans.Find(s => s.fromIndex == spanIndex && s.toIndex == spanIndex + 1);
            if (span == null || span.isDestroyed) return;

            span.currentLoad = load;

            // Rope bonus: adjacent rope spans reduce load by 20%
            if (span.partType == PartType.Rope)
                span.currentLoad *= 0.8f;

            // Update visual color (stress indicator)
            if (span.visualObj != null)
            {
                var sr = span.visualObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float ratio = span.currentLoad / span.strength;
                    sr.color = Color.Lerp(new Color(0.3f, 0.9f, 0.3f), new Color(1f, 0.2f, 0.2f), ratio);
                }
            }

            if (span.currentLoad > span.strength)
            {
                span.isDestroyed = true;
                if (span.visualObj != null)
                    StartCoroutine(FlashRed(span.visualObj.GetComponent<SpriteRenderer>()));
            }
        }

        IEnumerator FlashRed(SpriteRenderer sr)
        {
            if (sr == null) yield break;
            for (int i = 0; i < 3; i++)
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.08f);
                sr.color = new Color(0.6f, 0.1f, 0.1f);
                yield return new WaitForSeconds(0.08f);
            }
        }

        IEnumerator CollapseAndFail()
        {
            _isActive = false;

            // Car falls
            if (_carObject != null)
            {
                float t = 0;
                Vector3 startPos = _carObject.transform.position;
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    _carObject.transform.position = startPos + new Vector3(0, -4f * t * t, 0);
                    yield return null;
                }
                Destroy(_carObject);
                _carObject = null;
            }

            yield return new WaitForSeconds(0.3f);
            _gameManager.OnTestResult(false, (float)_budgetRemaining / _budget);
        }

        void ShowSuccessEffect()
        {
            // Flash all spans green briefly
            StartCoroutine(SuccessFlash());
        }

        IEnumerator SuccessFlash()
        {
            foreach (var s in _spans)
            {
                if (s.visualObj != null)
                {
                    var sr = s.visualObj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(0.3f, 1f, 0.4f);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }

        public void ResetToBuilding()
        {
            _isTesting = false;
            if (_carObject != null) { Destroy(_carObject); _carObject = null; }

            // Restore span colors
            foreach (var s in _spans)
            {
                s.currentLoad = 0;
                s.isDestroyed = false;
                if (s.visualObj != null)
                {
                    var sr = s.visualObj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = GetDef(s.partType).color;
                }
            }
            _isActive = true;
        }

        public void OnPartSelected(PartType part)
        {
            _selectedPart = part;
        }

        void ClearAll()
        {
            _isActive = false;
            _isTesting = false;
            _firstAnchorIndex = -1;

            if (_carObject != null) { Destroy(_carObject); _carObject = null; }

            foreach (var go in _anchorObjects) if (go != null) Destroy(go);
            _anchorObjects.Clear();

            foreach (var s in _spans) if (s.visualObj != null) Destroy(s.visualObj);
            _spans.Clear();
        }

        void OnDestroy()
        {
            ClearAll();
        }
    }
}
