using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

namespace Game050_BubbleSort
{
    public class BubbleSortManager : MonoBehaviour
    {
        [SerializeField] private BubbleSortGameManager _gameManager;

        private static readonly Color[] BubbleColors = {
            new Color(1f, 0.4f, 0.4f, 0.8f),
            new Color(0.4f, 0.7f, 1f, 0.8f),
            new Color(0.4f, 1f, 0.5f, 0.8f),
            new Color(1f, 0.85f, 0.3f, 0.8f),
            new Color(0.8f, 0.4f, 1f, 0.8f),
        };

        private const float BubbleSize = 0.6f;
        private const float ZoneWidth = 1.2f;
        private const int BubblesPerColor = 3;

        private List<List<int>> _zones;
        private List<List<GameObject>> _bubbleObjects;
        private List<GameObject> _zoneObjects = new List<GameObject>();
        private Sprite _bubbleSprite, _zoneSprite;
        private Camera _mainCamera;
        private int _selectedZone = -1;
        private int _colorCount;

        public void GenerateStage(int stage)
        {
            _mainCamera = Camera.main;
            _bubbleSprite = Resources.Load<Sprite>("Sprites/Game050_BubbleSort/bubble");
            _zoneSprite = Resources.Load<Sprite>("Sprites/Game050_BubbleSort/zone");

            CleanUp();

            _colorCount = Mathf.Min(3 + (stage - 1) / 2, BubbleColors.Length);
            int zoneCount = _colorCount + 1;

            var allBubbles = new List<int>();
            for (int c = 0; c < _colorCount; c++)
                for (int i = 0; i < BubblesPerColor; i++)
                    allBubbles.Add(c);
            Shuffle(allBubbles);

            _zones = new List<List<int>>();
            _bubbleObjects = new List<List<GameObject>>();

            float totalW = (zoneCount - 1) * ZoneWidth;
            float startX = -totalW / 2f;

            int idx = 0;
            for (int z = 0; z < zoneCount; z++)
            {
                float x = startX + z * ZoneWidth;
                var zoneObj = new GameObject("Zone_" + z);
                zoneObj.transform.position = new Vector3(x, -1f, 0f);
                zoneObj.transform.localScale = new Vector3(0.8f, 1.5f, 1f);
                var sr = zoneObj.AddComponent<SpriteRenderer>();
                sr.sprite = _zoneSprite;
                sr.sortingOrder = 0;
                sr.color = new Color(1f, 1f, 1f, 0.3f);
                _zoneObjects.Add(zoneObj);

                var zone = new List<int>();
                var bubbles = new List<GameObject>();

                if (z < _colorCount)
                {
                    for (int i = 0; i < BubblesPerColor && idx < allBubbles.Count; i++, idx++)
                    {
                        zone.Add(allBubbles[idx]);
                        var bObj = CreateBubble(allBubbles[idx], x, -2f + i * BubbleSize);
                        bubbles.Add(bObj);
                    }
                }

                _zones.Add(zone);
                _bubbleObjects.Add(bubbles);
            }
        }

        private GameObject CreateBubble(int colorIdx, float x, float y)
        {
            var go = new GameObject("Bubble");
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * 0.9f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _bubbleSprite;
            sr.color = BubbleColors[colorIdx];
            sr.sortingOrder = 5;
            return go;
        }

        private void CleanUp()
        {
            foreach (var zo in _zoneObjects) if (zo != null) Destroy(zo);
            _zoneObjects.Clear();
            if (_bubbleObjects != null)
                foreach (var bl in _bubbleObjects) foreach (var b in bl) if (b != null) Destroy(b);
            _bubbleObjects = null;
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                int tapped = GetZoneAt(wp.x);
                if (tapped < 0) return;

                if (_selectedZone < 0)
                {
                    if (_zones[tapped].Count > 0)
                    {
                        _selectedZone = tapped;
                        HighlightZone(tapped, true);
                    }
                }
                else
                {
                    if (tapped == _selectedZone)
                    {
                        HighlightZone(_selectedZone, false);
                        _selectedZone = -1;
                    }
                    else if (_zones[tapped].Count < BubblesPerColor)
                    {
                        int topColor = _zones[_selectedZone][_zones[_selectedZone].Count - 1];
                        _zones[_selectedZone].RemoveAt(_zones[_selectedZone].Count - 1);
                        var topBubble = _bubbleObjects[_selectedZone][_bubbleObjects[_selectedZone].Count - 1];
                        _bubbleObjects[_selectedZone].RemoveAt(_bubbleObjects[_selectedZone].Count - 1);

                        _zones[tapped].Add(topColor);
                        float x = _zoneObjects[tapped].transform.position.x;
                        float y = -2f + (_zones[tapped].Count - 1) * BubbleSize;
                        topBubble.transform.position = new Vector3(x, y, 0f);
                        _bubbleObjects[tapped].Add(topBubble);

                        HighlightZone(_selectedZone, false);
                        _selectedZone = -1;

                        if (_gameManager != null) _gameManager.OnBubbleMoved();
                    }
                    else
                    {
                        HighlightZone(_selectedZone, false);
                        _selectedZone = -1;
                    }
                }
            }
        }

        private void HighlightZone(int idx, bool on)
        {
            if (idx < 0 || idx >= _zoneObjects.Count || _zoneObjects[idx] == null) return;
            var sr = _zoneObjects[idx].GetComponent<SpriteRenderer>();
            sr.color = on ? new Color(1f, 1f, 0.6f, 0.5f) : new Color(1f, 1f, 1f, 0.3f);
        }

        private int GetZoneAt(float x)
        {
            for (int i = 0; i < _zoneObjects.Count; i++)
            {
                if (Mathf.Abs(x - _zoneObjects[i].transform.position.x) < ZoneWidth * 0.5f)
                    return i;
            }
            return -1;
        }

        public bool CheckSorted()
        {
            foreach (var zone in _zones)
            {
                if (zone.Count == 0) continue;
                if (zone.Count != BubblesPerColor) return false;
                int first = zone[0];
                if (zone.Any(b => b != first)) return false;
            }
            return true;
        }

        private void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
