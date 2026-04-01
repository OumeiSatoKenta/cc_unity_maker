using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game084_GardenZen
{
    public class ZenManager : MonoBehaviour
    {
        [SerializeField, Tooltip("石スプライト")] private Sprite _stoneSprite;
        [SerializeField, Tooltip("苔スプライト")] private Sprite _mossSprite;
        [SerializeField, Tooltip("砂スプライト")] private Sprite _sandSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _placementCount;
        private int _currentTool; // 0=stone, 1=moss, 2=rake(sand pattern)
        private List<GameObject> _placed = new List<GameObject>();

        private static readonly Color[] StoneColors = {
            new Color(0.47f, 0.45f, 0.41f), new Color(0.55f, 0.52f, 0.48f),
            new Color(0.4f, 0.38f, 0.35f),
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _placementCount = 0;
            _currentTool = 0;

            // Sand base
            for (int r = -3; r <= 3; r++)
                for (int c = -4; c <= 4; c++)
                {
                    var obj = new GameObject($"Sand_{r}_{c}");
                    obj.transform.position = new Vector3(c * 1f, r * 1f - 0.5f, 0f);
                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = _sandSprite; sr.sortingOrder = 0;
                    obj.transform.localScale = Vector3.one * 0.021f;
                }
        }

        public void StopGame() { _isActive = false; }

        public void SetTool(int tool) { _currentTool = tool; }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                if (wp.y < 3f && wp.y > -4f)
                {
                    PlaceItem(wp);
                }
            }
        }

        private void PlaceItem(Vector2 pos)
        {
            var obj = new GameObject($"Item_{_placementCount}");
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;

            switch (_currentTool)
            {
                case 0: // Stone
                    sr.sprite = _stoneSprite;
                    sr.color = StoneColors[Random.Range(0, StoneColors.Length)];
                    float stoneScale = Random.Range(0.4f, 0.8f);
                    obj.transform.localScale = Vector3.one * stoneScale;
                    break;
                case 1: // Moss
                    sr.sprite = _mossSprite;
                    sr.color = new Color(Random.Range(0.6f, 0.9f), 1f, Random.Range(0.5f, 0.8f));
                    obj.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f);
                    break;
                case 2: // Sand rake pattern (visual line)
                    sr.sprite = _sandSprite;
                    sr.color = new Color(0.82f, 0.78f, 0.65f, 0.5f);
                    obj.transform.localScale = new Vector3(0.005f, 0.001f, 1f);
                    obj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
                    break;
            }

            _placed.Add(obj);
            _placementCount++;
        }

        public int PlacementCount => _placementCount;
    }
}
