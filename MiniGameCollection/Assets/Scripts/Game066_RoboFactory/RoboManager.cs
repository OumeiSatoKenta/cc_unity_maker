using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game066_RoboFactory
{
    public class RoboManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private RoboFactoryGameManager _gameManager;
        [SerializeField, Tooltip("ロボットスプライト")] private Sprite _robotSprite;
        [SerializeField, Tooltip("資源スプライト")] private Sprite _resourceSprite;

        private bool _isActive;
        private List<GameObject> _robots = new List<GameObject>();
        private float _gatherTimer;

        private void Awake() { }

        public void StartGame()
        {
            _isActive = true;
            _gatherTimer = 0f;
            // Start with 1 robot
            SpawnRobot(new Vector3(-2f, -2f, 0f));
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Animate robots
            for (int i = 0; i < _robots.Count; i++)
            {
                if (_robots[i] == null) continue;
                float t = Time.time + i * 1.5f;
                var pos = _robots[i].transform.position;
                pos.x += Mathf.Sin(t * 1.2f) * 0.01f;
                pos.y += Mathf.Cos(t * 0.8f) * 0.005f;
                _robots[i].transform.position = pos;
            }

            // Auto gather
            if (_robots.Count > 0)
                _gatherTimer += Time.deltaTime;
        }

        private void SpawnRobot(Vector3 pos)
        {
            var obj = new GameObject($"Robot_{_robots.Count}");
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _robotSprite; sr.sortingOrder = 5;
            obj.transform.localScale = Vector3.one * 0.8f;
            sr.color = Color.HSVToRGB(Random.Range(0f, 1f), 0.2f, 1f);
            _robots.Add(obj);
        }

        public void AddRobot()
        {
            int cost = NextRobotCost;
            if (_gameManager.TrySpendForRobot(cost))
            {
                float x = Random.Range(-3f, 3f);
                float y = Random.Range(-3f, 0f);
                SpawnRobot(new Vector3(x, y, 0f));
            }
        }

        public int AutoGather
        {
            get
            {
                if (_robots.Count <= 0) return 0;
                if (_gatherTimer >= 1.5f)
                {
                    _gatherTimer -= 1.5f;
                    return _robots.Count;
                }
                return 0;
            }
        }

        public int RobotCount => _robots.Count;
        public int NextRobotCost => 8 + _robots.Count * 5;
    }
}
