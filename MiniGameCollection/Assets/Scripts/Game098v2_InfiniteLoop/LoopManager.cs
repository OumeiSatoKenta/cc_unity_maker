using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game098v2_InfiniteLoop
{
    public class LoopManager : MonoBehaviour
    {
        [SerializeField] InfiniteLoopGameManager _gameManager;
        [SerializeField] InfiniteLoopUI _ui;
        [SerializeField] SpriteRenderer[] _roomObjects; // 5 objects: door, window, book, clock, picture
        [SerializeField] Image _flashImage;

        // Stage config
        int _loopLimit;
        int _realChangeCount;
        int _fakeChangeCount;
        bool _randomOrderEnabled;
        bool _reverseLoopEnabled;

        // Runtime state
        int _currentLoop;
        bool _isActive;
        List<ChangeElement> _changeElements = new List<ChangeElement>();
        HashSet<string> _discoveredElements = new HashSet<string>();
        int _correctDiscoveries;

        // Object names must match room object names in scene
        static readonly string[] ObjectIds = { "door", "window", "book", "clock", "picture" };
        // Colors for normal vs changed state
        static readonly Color NormalColor = new Color(0.7f, 0.7f, 0.9f, 1f);
        static readonly Color ChangedColor = new Color(0.3f, 1f, 0.9f, 1f);
        static readonly Color FakeChangedColor = new Color(1f, 0.5f, 0.3f, 1f);

        class ChangeElement
        {
            public string elementId;
            public bool isReal;
            public int appearsOnLoop;
            public bool isReverse;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _realChangeCount = config.countMultiplier;
            _fakeChangeCount = Mathf.RoundToInt(config.complexityFactor * 2f);
            _randomOrderEnabled = stageIndex >= 2;
            _reverseLoopEnabled = stageIndex >= 4;
            _loopLimit = Mathf.RoundToInt(10f / config.speedMultiplier);
            _loopLimit = Mathf.Max(5, _loopLimit);

            GenerateChangeElements();
            _currentLoop = 0;
            _discoveredElements.Clear();
            _correctDiscoveries = 0;
            _isActive = true;

            ResetRoomObjects();
            StartLoop();
        }

        void GenerateChangeElements()
        {
            _changeElements.Clear();
            var usedIds = new List<string>();

            // Assign real elements
            for (int i = 0; i < _realChangeCount && i < ObjectIds.Length; i++)
            {
                string id = ObjectIds[i];
                usedIds.Add(id);
                int loop = _randomOrderEnabled ? Random.Range(1, _loopLimit) : (i + 1);
                bool rev = _reverseLoopEnabled && Random.value < 0.4f;
                _changeElements.Add(new ChangeElement
                {
                    elementId = id,
                    isReal = true,
                    appearsOnLoop = loop,
                    isReverse = rev
                });
            }

            // Assign fake elements
            for (int i = 0; i < _fakeChangeCount; i++)
            {
                string id = null;
                for (int j = _realChangeCount + i; j < ObjectIds.Length; j++)
                {
                    if (!usedIds.Contains(ObjectIds[j]))
                    {
                        id = ObjectIds[j];
                        break;
                    }
                }
                if (id == null) break;
                usedIds.Add(id);
                int loop = _randomOrderEnabled ? Random.Range(1, _loopLimit) : (_realChangeCount + i + 1);
                _changeElements.Add(new ChangeElement
                {
                    elementId = id,
                    isReal = false,
                    appearsOnLoop = loop,
                    isReverse = false
                });
            }
        }

        void StartLoop()
        {
            if (!_isActive) return;

            bool isReverse = _reverseLoopEnabled && (_currentLoop % 3 == 2);
            _ui.UpdateLoopCount(_loopLimit - _currentLoop);
            _ui.SetReverseIndicator(isReverse);

            // Reveal objects that appear in this loop
            ResetRoomObjects();
            foreach (var elem in _changeElements)
            {
                bool shouldAppear;
                if (isReverse)
                    shouldAppear = (_loopLimit - elem.appearsOnLoop) == _currentLoop;
                else
                    shouldAppear = elem.appearsOnLoop <= _currentLoop;

                if (shouldAppear)
                {
                    ShowObjectChange(elem);
                }
            }

            StartCoroutine(LoopFlash());
        }

        void ShowObjectChange(ChangeElement elem)
        {
            for (int i = 0; i < ObjectIds.Length; i++)
            {
                if (ObjectIds[i] == elem.elementId && i < _roomObjects.Length && _roomObjects[i] != null)
                {
                    _roomObjects[i].color = elem.isReal ? ChangedColor : FakeChangedColor;
                    break;
                }
            }
        }

        void ResetRoomObjects()
        {
            foreach (var obj in _roomObjects)
            {
                if (obj != null)
                    obj.color = NormalColor;
            }
        }


        IEnumerator LoopFlash()
        {
            if (_flashImage == null) yield break;
            _flashImage.color = new Color(1f, 1f, 1f, 0.7f);
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                _flashImage.color = new Color(1f, 1f, 1f, 0.7f * (1f - t / 0.3f));
                yield return null;
            }
            _flashImage.color = new Color(1f, 1f, 1f, 0f);
        }

        void Update()
        {
            if (!_isActive || _gameManager == null || !_gameManager.IsPlaying) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                var cam = Camera.main;
                if (cam == null) return;
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    OnObjectTapped(hit.gameObject.name);
                }
            }
        }

        void OnObjectTapped(string objName)
        {
            string elemId = objName.ToLower().Replace("roomobject_", "");
            if (!_discoveredElements.Contains(elemId))
            {
                _discoveredElements.Add(elemId);
                ChangeElement elem = _changeElements.Find(e => e.elementId == elemId);
                bool isReal = elem != null && elem.isReal;

                if (isReal) _correctDiscoveries++;

                // Visual feedback: scale pulse
                StartCoroutine(PulseObject(elemId));
                _ui.AddMemoEntry(elemId, isReal);
                _gameManager.OnChangeDiscovered(isReal);
            }
        }

        IEnumerator PulseObject(string elemId)
        {
            int idx = System.Array.IndexOf(ObjectIds, elemId);
            if (idx < 0 || idx >= _roomObjects.Length || _roomObjects[idx] == null) yield break;

            Transform t = _roomObjects[idx].transform;
            Vector3 orig = t.localScale;
            float elapsed = 0f;
            while (elapsed < 0.25f)
            {
                elapsed += Time.deltaTime;
                float s = 1f + 0.3f * Mathf.Sin(elapsed / 0.25f * Mathf.PI);
                t.localScale = orig * s;
                yield return null;
            }
            t.localScale = orig;
        }

        /// <summary>Called by UI's Escape button.</summary>
        public void TryEscape()
        {
            if (!_isActive || !_gameManager.IsPlaying) return;

            // Check if enough real elements discovered
            bool allRealDiscovered = _correctDiscoveries >= _realChangeCount;
            if (allRealDiscovered)
            {
                _gameManager.OnEscapeSuccess(_currentLoop, _loopLimit);
                StartCoroutine(EscapeFlash(true));
            }
            else
            {
                // Wrong attempt: consume 2 loops as penalty
                _currentLoop += 2;
                _gameManager.OnEscapeFailed();
                if (_currentLoop >= _loopLimit)
                {
                    _gameManager.OnLoopLimitReached();
                    return;
                }
                StartCoroutine(EscapeFlash(false));
                StartLoop();
            }
        }

        IEnumerator EscapeFlash(bool success)
        {
            if (_flashImage == null) yield break;
            Color col = success ? new Color(0.3f, 1f, 0.4f, 0.8f) : new Color(1f, 0.2f, 0.2f, 0.8f);
            _flashImage.color = col;
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                _flashImage.color = new Color(col.r, col.g, col.b, col.a * (1f - t / 0.5f));
                yield return null;
            }
            _flashImage.color = new Color(0, 0, 0, 0);
        }

        /// <summary>Called by UI's Next Loop button (advance one loop).</summary>
        public void AdvanceLoop()
        {
            if (!_isActive || !_gameManager.IsPlaying) return;
            _currentLoop++;
            if (_currentLoop >= _loopLimit)
            {
                _isActive = false;
                _gameManager.OnLoopLimitReached();
                return;
            }
            StartLoop();
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                StopAllCoroutines();
            }
        }

        public void OpenMemo()
        {
            _gameManager.OnMemoUsed();
        }
    }
}
