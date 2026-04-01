using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game087_DreamCatcher
{
    public class DreamManager : MonoBehaviour
    {
        [SerializeField, Tooltip("破片スプライト")] private Sprite _fragmentSprite;
        [SerializeField, Tooltip("キャッチャースプライト")] private Sprite _catcherSprite;
        [SerializeField, Tooltip("スポーン間隔")] private float _spawnInterval = 2f;

        private Camera _mainCamera;
        private bool _isActive;
        private int _totalFragments;
        private HashSet<int> _collectedTypes = new HashSet<int>();
        private List<GameObject> _fragments = new List<GameObject>();
        private float _spawnTimer;

        private static readonly Color[] FragmentColors = {
            new Color(1f, 0.7f, 0.7f), new Color(0.7f, 0.7f, 1f),
            new Color(0.7f, 1f, 0.7f), new Color(1f, 1f, 0.7f),
            new Color(1f, 0.7f, 1f), new Color(0.7f, 1f, 1f),
        };

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _totalFragments = 0;
            _spawnTimer = 0.5f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Spawn fragments
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnFragment();
                _spawnTimer = _spawnInterval;
            }

            // Float fragments upward slowly
            for (int i = _fragments.Count - 1; i >= 0; i--)
            {
                if (_fragments[i] == null) { _fragments.RemoveAt(i); continue; }
                var pos = _fragments[i].transform.position;
                pos.y += 0.3f * Time.deltaTime;
                pos.x += Mathf.Sin(Time.time * 2f + i) * 0.01f;
                _fragments[i].transform.position = pos;

                if (pos.y > 7f)
                {
                    Destroy(_fragments[i]);
                    _fragments.RemoveAt(i);
                }
            }

            // Tap to catch
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                for (int i = _fragments.Count - 1; i >= 0; i--)
                {
                    if (_fragments[i] == null) continue;
                    if (Vector2.Distance(wp, _fragments[i].transform.position) < 0.8f)
                    {
                        int typeId = i % FragmentColors.Length;
                        _collectedTypes.Add(typeId);
                        _totalFragments++;
                        Destroy(_fragments[i]);
                        _fragments.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void SpawnFragment()
        {
            var obj = new GameObject($"Fragment_{_totalFragments + _fragments.Count}");
            obj.transform.position = new Vector3(Random.Range(-3.5f, 3.5f), -5f, 0f);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _fragmentSprite; sr.sortingOrder = 3;
            int typeId = Random.Range(0, FragmentColors.Length);
            sr.color = FragmentColors[typeId];
            obj.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f);
            _fragments.Add(obj);
        }

        public int CollectedTypes => _collectedTypes.Count;
        public int TotalFragments => _totalFragments;
    }
}
