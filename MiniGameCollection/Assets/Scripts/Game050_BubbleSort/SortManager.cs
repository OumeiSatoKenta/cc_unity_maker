using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game050_BubbleSort
{
    public class SortManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private BubbleSortGameManager _gameManager;
        [SerializeField, Tooltip("バブルスプライト")] private Sprite _bubbleSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private List<SortBubble> _bubbles = new List<SortBubble>();
        private int _selectedIndex = -1;

        private static readonly Color[] BubbleColors = {
            new Color(1f, 0.3f, 0.3f),   // Red
            new Color(1f, 0.6f, 0.2f),   // Orange
            new Color(1f, 1f, 0.3f),     // Yellow
            new Color(0.3f, 0.9f, 0.3f), // Green
            new Color(0.3f, 0.7f, 1f),   // Blue
            new Color(0.6f, 0.3f, 1f),   // Purple
            new Color(1f, 0.5f, 0.8f),   // Pink
            new Color(0.5f, 0.9f, 0.9f), // Cyan
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame(int count)
        {
            _isActive = true;
            _selectedIndex = -1;

            // Create sorted color indices then shuffle
            int[] colorIndices = new int[count];
            for (int i = 0; i < count; i++) colorIndices[i] = i % BubbleColors.Length;

            // Fisher-Yates shuffle (ensure not already sorted)
            do {
                for (int i = count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (colorIndices[i], colorIndices[j]) = (colorIndices[j], colorIndices[i]);
                }
            } while (IsSortedArray(colorIndices));

            // Spawn bubbles in a horizontal row
            float startX = -(count - 1) * 0.7f;
            for (int i = 0; i < count; i++)
            {
                var obj = new GameObject($"Bubble_{i}");
                float x = startX + i * 1.4f;
                obj.transform.position = new Vector3(x, 0f, 0f);

                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _bubbleSprite;
                sr.sortingOrder = 2;
                sr.color = BubbleColors[colorIndices[i]];

                var col = obj.AddComponent<CircleCollider2D>();
                col.radius = 0.45f;

                var bubble = obj.AddComponent<SortBubble>();
                bubble.Initialize(colorIndices[i]);
                _bubbles.Add(bubble);
            }
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null)
                {
                    var bubble = hit.GetComponent<SortBubble>();
                    if (bubble != null)
                    {
                        int index = _bubbles.IndexOf(bubble);
                        if (index >= 0) HandleBubbleClick(index);
                    }
                }
            }
        }

        private void HandleBubbleClick(int index)
        {
            if (_selectedIndex < 0)
            {
                // First selection
                _selectedIndex = index;
                _bubbles[index].SetSelected(true);
            }
            else if (_selectedIndex == index)
            {
                // Deselect
                _bubbles[index].SetSelected(false);
                _selectedIndex = -1;
            }
            else if (Mathf.Abs(_selectedIndex - index) == 1)
            {
                // Adjacent: swap
                _bubbles[_selectedIndex].SetSelected(false);
                SwapBubbles(_selectedIndex, index);
                _selectedIndex = -1;
                _gameManager.OnSwapPerformed();
            }
            else
            {
                // Non-adjacent: change selection
                _bubbles[_selectedIndex].SetSelected(false);
                _selectedIndex = index;
                _bubbles[index].SetSelected(true);
            }
        }

        private void SwapBubbles(int a, int b)
        {
            // Swap in list
            (_bubbles[a], _bubbles[b]) = (_bubbles[b], _bubbles[a]);

            // Swap positions
            Vector3 posA = _bubbles[a].transform.position;
            Vector3 posB = _bubbles[b].transform.position;
            _bubbles[a].transform.position = posB;
            _bubbles[b].transform.position = posA;
        }

        public bool IsSorted
        {
            get
            {
                for (int i = 1; i < _bubbles.Count; i++)
                    if (_bubbles[i].ColorIndex < _bubbles[i - 1].ColorIndex) return false;
                return true;
            }
        }

        public int CorrectCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _bubbles.Count; i++)
                    if (_bubbles[i].ColorIndex == i % BubbleColors.Length) count++;
                return count;
            }
        }

        private bool IsSortedArray(int[] arr)
        {
            for (int i = 1; i < arr.Length; i++)
                if (arr[i] < arr[i - 1]) return false;
            return true;
        }
    }
}
