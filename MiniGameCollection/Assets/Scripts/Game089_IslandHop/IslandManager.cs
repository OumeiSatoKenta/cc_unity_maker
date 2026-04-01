using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game089_IslandHop
{
    public class IslandManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private IslandHopGameManager _gameManager;
        [SerializeField, Tooltip("島スプライト")] private Sprite _islandSprite;
        [SerializeField, Tooltip("ヤシの木スプライト")] private Sprite _palmSprite;
        [SerializeField, Tooltip("リゾートスプライト")] private Sprite _resortSprite;
        [SerializeField, Tooltip("開拓コスト")] private int _expandCost = 15;

        private bool _isActive;
        private List<GameObject> _islands = new List<GameObject>();
        private float _incomeTimer;

        public void StartGame()
        {
            _isActive = true;
            _incomeTimer = 0f;
            // Starting island
            CreateIsland(Vector2.zero);
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            if (_islands.Count > 0) _incomeTimer += Time.deltaTime;
        }

        public void ExpandIsland()
        {
            int cost = _expandCost + _islands.Count * 8;
            if (_gameManager.TrySpend(cost))
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist = 2f + _islands.Count * 0.8f;
                Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                CreateIsland(pos);
            }
        }

        private void CreateIsland(Vector2 pos)
        {
            var obj = new GameObject($"Island_{_islands.Count}");
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _islandSprite; sr.sortingOrder = 1;
            float scale = Random.Range(0.5f, 0.8f);
            obj.transform.localScale = Vector3.one * scale;

            // Add palm tree
            var palm = new GameObject("Palm");
            palm.transform.SetParent(obj.transform);
            palm.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), 0.2f, -0.01f);
            var psr = palm.AddComponent<SpriteRenderer>();
            psr.sprite = _palmSprite; psr.sortingOrder = 2;
            palm.transform.localScale = Vector3.one * 0.4f;

            // Add resort if not first island
            if (_islands.Count > 0)
            {
                var resort = new GameObject("Resort");
                resort.transform.SetParent(obj.transform);
                resort.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), -0.1f, -0.01f);
                var rsr = resort.AddComponent<SpriteRenderer>();
                rsr.sprite = _resortSprite; rsr.sortingOrder = 3;
                resort.transform.localScale = Vector3.one * 0.3f;
            }

            _islands.Add(obj);
        }

        public int AutoGather
        {
            get
            {
                if (_islands.Count <= 0) return 0;
                if (_incomeTimer >= 2f) { _incomeTimer -= 2f; return _islands.Count * 2; }
                return 0;
            }
        }

        public int IslandCount => _islands.Count;
        public int NextExpandCost => _expandCost + _islands.Count * 8;
    }
}
