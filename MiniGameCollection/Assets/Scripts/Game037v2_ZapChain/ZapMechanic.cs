using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game037v2_ZapChain
{
    public class ZapMechanic : MonoBehaviour
    {
        [SerializeField] ZapChainGameManager _gameManager;
        [SerializeField] LineRenderer _connectionRenderer;
        [SerializeField] Transform _nodeContainer;

        [SerializeField] Sprite _normalSprite;
        [SerializeField] Sprite _connectedSprite;
        [SerializeField] Sprite _activeSprite;
        [SerializeField] Sprite _obstacleSprite;
        [SerializeField] Sprite _movingSprite;
        [SerializeField] Sprite _timedSprite;

        // Stage config
        int _nodeCount = 5;
        bool _hasDistanceLimit;
        float _maxDistance = 999f;
        int _obstacleCount;
        int _movingCount;
        int _timedCount;
        float _energyMax = 100f;
        float _currentEnergy;
        float _energyPerConnection = 10f;

        readonly List<NodeObject> _allNodes = new();
        readonly List<NodeObject> _chainNodes = new();
        bool _isDragging;
        bool _isActive;

        static readonly int[] NodeCounts = { 5, 8, 10, 12, 15 };
        static readonly bool[] HasDistanceLimits = { false, true, true, true, true };
        static readonly float[] MaxDistances = { 999f, 2.5f, 2.5f, 2.5f, 2.5f };
        static readonly int[] ObstacleCounts = { 0, 0, 2, 1, 1 };
        static readonly int[] MovingCounts = { 0, 0, 0, 2, 2 };
        static readonly int[] TimedCounts = { 0, 0, 0, 0, 3 };
        static readonly float[] EnergyMaxValues = { 100f, 100f, 120f, 120f, 150f };

        public float CurrentEnergy => _currentEnergy;
        public float EnergyMax => _energyMax;
        public int TotalNodes => _allNodes.Count;
        public int ConnectedNodes
        {
            get
            {
                int count = 0;
                foreach (var n in _allNodes)
                    if (n != null && n.NodeType != NodeType.Obstacle && n.IsConnected) count++;
                return count;
            }
        }

        public void SetupStage(int stageIndex)
        {
            int idx = Mathf.Clamp(stageIndex, 0, 4);
            _nodeCount = NodeCounts[idx];
            _hasDistanceLimit = HasDistanceLimits[idx];
            _maxDistance = MaxDistances[idx];
            _obstacleCount = ObstacleCounts[idx];
            _movingCount = MovingCounts[idx];
            _timedCount = TimedCounts[idx];
            _energyMax = EnergyMaxValues[idx];
            _currentEnergy = _energyMax;

            ClearNodes();
            GenerateNodes();
            _isActive = true;
        }

        void ClearNodes()
        {
            foreach (var node in _allNodes)
            {
                if (node != null)
                {
                    node.OnTimedExpired -= OnTimedNodeExpired;
                    Destroy(node.gameObject);
                }
            }
            _allNodes.Clear();
            _chainNodes.Clear();
            _isDragging = false;
            if (_connectionRenderer != null)
                _connectionRenderer.positionCount = 0;
        }

        void GenerateNodes()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float areaTop = camSize - 1.2f;
            float areaBottom = -camSize + 2.8f;
            float areaLeft = -camWidth + 0.8f;
            float areaRight = camWidth - 0.8f;

            int normalCount = _nodeCount - _obstacleCount - _movingCount - _timedCount;

            var positions = new List<Vector2>();
            int maxAttempts = 200;

            for (int i = 0; i < _nodeCount; i++)
            {
                Vector2 pos = Vector2.zero;
                bool placed = false;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    float x = Random.Range(areaLeft, areaRight);
                    float y = Random.Range(areaBottom, areaTop);
                    pos = new Vector2(x, y);

                    bool tooClose = false;
                    foreach (var existingPos in positions)
                    {
                        if (Vector2.Distance(pos, existingPos) < 0.8f)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (!tooClose) { placed = true; break; }
                }
                if (!placed)
                {
                    pos = new Vector2(
                        Random.Range(areaLeft, areaRight),
                        Random.Range(areaBottom, areaTop));
                }
                positions.Add(pos);
            }

            // Shuffle
            for (int i = 0; i < positions.Count; i++)
            {
                int j = Random.Range(i, positions.Count);
                (positions[i], positions[j]) = (positions[j], positions[i]);
            }

            int idx = 0;

            for (int i = 0; i < normalCount; i++)
                SpawnNode(positions[idx++], NodeType.Normal, idx);

            for (int i = 0; i < _movingCount; i++)
            {
                var node = SpawnNode(positions[idx], NodeType.Moving, idx);
                float radius = Random.Range(0.6f, 1.2f);
                float speed = Random.Range(0.8f, 1.5f) * (Random.value > 0.5f ? 1f : -1f);
                node.SetupMoving(positions[idx], radius, Random.Range(0f, Mathf.PI * 2f), speed);
                idx++;
            }

            for (int i = 0; i < _timedCount; i++)
            {
                var node = SpawnNode(positions[idx], NodeType.Timed, idx);
                node.StartTimed(5f);
                node.OnTimedExpired += OnTimedNodeExpired;
                idx++;
            }

            for (int i = 0; i < _obstacleCount; i++)
            {
                Vector2 pos = Vector2.zero;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    float x = Random.Range(areaLeft, areaRight);
                    float y = Random.Range(areaBottom, areaTop);
                    pos = new Vector2(x, y);
                    bool tooClose = false;
                    foreach (var existingPos in positions)
                        if (Vector2.Distance(pos, existingPos) < 1.0f)
                        { tooClose = true; break; }
                    if (!tooClose) break;
                }
                SpawnNode(pos, NodeType.Obstacle, idx++);
            }
        }

        NodeObject SpawnNode(Vector2 pos, NodeType type, int idx)
        {
            var go = new GameObject($"Node_{idx}_{type}");
            go.transform.SetParent(_nodeContainer);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = Vector3.one * 0.4f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 1.2f;

            var node = go.AddComponent<NodeObject>();

            Sprite normalSprite = _normalSprite;
            switch (type)
            {
                case NodeType.Obstacle: normalSprite = _obstacleSprite; break;
                case NodeType.Moving: normalSprite = _movingSprite; break;
                case NodeType.Timed: normalSprite = _timedSprite; break;
            }
            node.Initialize(type, normalSprite, _connectedSprite, _activeSprite);
            _allNodes.Add(node);
            return node;
        }

        void OnTimedNodeExpired()
        {
            if (!_isActive) return;
            // タイマー切れ時点でまだ未接続のTimedノードが存在する場合ゲームオーバー
            bool hasUnconnectedTimed = false;
            foreach (var n in _allNodes)
                if (n != null && n.NodeType == NodeType.Timed && !n.IsConnected)
                { hasUnconnectedTimed = true; break; }

            if (hasUnconnectedTimed)
                _gameManager.OnEnergyEmpty();
        }

        void Update()
        {
            if (!_isActive) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
                HandlePress();
            else if (mouse.leftButton.isPressed && _isDragging)
                HandleDrag();
            else if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
                HandleRelease();
        }

        void HandlePress()
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var node = hit.GetComponent<NodeObject>();
            if (node == null || node.NodeType == NodeType.Obstacle || node.IsConnected) return;

            _isDragging = true;
            _chainNodes.Clear();
            node.SetActive(true);
            _chainNodes.Add(node);
            UpdateConnectionRenderer();
        }

        void HandleDrag()
        {
            if (_chainNodes.Count == 0) return;
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var node = hit.GetComponent<NodeObject>();
            if (node == null || node.NodeType == NodeType.Obstacle || node.IsConnected) return;
            if (_chainNodes.Contains(node)) return;

            var lastNode = _chainNodes[_chainNodes.Count - 1];
            float dist = Vector2.Distance(lastNode.transform.position, node.transform.position);
            if (_hasDistanceLimit && dist > _maxDistance) return;

            // Energy check - trigger game over without calling HandleRelease to avoid double processing
            if (_currentEnergy < _energyPerConnection)
            {
                _isDragging = false;
                foreach (var cn in _chainNodes) cn.SetConnected();
                _chainNodes.Clear();
                if (_connectionRenderer != null) _connectionRenderer.positionCount = 0;
                _gameManager.OnEnergyEmpty();
                return;
            }

            _currentEnergy -= _energyPerConnection;
            node.SetActive(true);
            _chainNodes.Add(node);
            UpdateConnectionRenderer();
            _gameManager.OnNodeConnected(_chainNodes.Count);
        }

        void HandleRelease()
        {
            _isDragging = false;
            if (_chainNodes.Count == 0) return;

            foreach (var node in _chainNodes)
                node.SetConnected();

            int chainLen = _chainNodes.Count;
            _chainNodes.Clear();
            if (_connectionRenderer != null) _connectionRenderer.positionCount = 0;

            bool allConnected = CheckAllConnected();
            _gameManager.OnChainCompleted(chainLen, allConnected);
        }

        bool CheckAllConnected()
        {
            foreach (var node in _allNodes)
                if (node != null && node.NodeType != NodeType.Obstacle && !node.IsConnected)
                    return false;
            return true;
        }

        void UpdateConnectionRenderer()
        {
            if (_connectionRenderer == null) return;
            _connectionRenderer.positionCount = _chainNodes.Count;
            for (int i = 0; i < _chainNodes.Count; i++)
                _connectionRenderer.SetPosition(i, _chainNodes[i].transform.position);
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                _isDragging = false;
                if (_connectionRenderer != null)
                    _connectionRenderer.positionCount = 0;
            }
        }

        public int GetNonObstacleCount()
        {
            int count = 0;
            foreach (var n in _allNodes)
                if (n != null && n.NodeType != NodeType.Obstacle) count++;
            return count;
        }
    }
}
