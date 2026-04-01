using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game037_ZapChain
{
    public class ChainManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private ZapChainGameManager _gameManager;

        [SerializeField, Tooltip("ノードスプライト")]
        private Sprite _nodeSprite;

        [SerializeField, Tooltip("ザップ済みスプライト")]
        private Sprite _zappedSprite;

        private Camera _mainCamera;
        private List<ZapNode> _nodes = new List<ZapNode>();
        private bool _isChaining;

        private const float ChainRadius = 2.0f;
        private const float ChainDelay = 0.15f;

        // ノード配置（12個、画面上に散らす）
        private static readonly Vector2[] NodePositions = {
            new Vector2(-3f, 3.5f), new Vector2(-1f, 4f), new Vector2(1.5f, 3.5f), new Vector2(3f, 3f),
            new Vector2(-2.5f, 1.5f), new Vector2(0f, 2f), new Vector2(2f, 1.5f),
            new Vector2(-3f, -0.5f), new Vector2(-0.5f, 0f), new Vector2(2.5f, -0.5f),
            new Vector2(-1.5f, -2f), new Vector2(1f, -2.5f),
        };

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void StartStage()
        {
            SpawnNodes();
        }

        private void SpawnNodes()
        {
            foreach (var n in _nodes)
                if (n != null) Destroy(n.gameObject);
            _nodes.Clear();

            for (int i = 0; i < NodePositions.Length; i++)
            {
                var obj = new GameObject($"Node_{i}");
                obj.transform.position = new Vector3(NodePositions[i].x, NodePositions[i].y, 0f);
                obj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                obj.transform.SetParent(transform);

                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 2;
                obj.AddComponent<CircleCollider2D>().radius = 0.5f;

                var node = obj.AddComponent<ZapNode>();
                node.Initialize(_nodeSprite, _zappedSprite);
                _nodes.Add(node);
            }
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying || _isChaining) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                mousePos.z = -_mainCamera.transform.position.z;
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

                // タップしたノードを検出
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    var node = hit.GetComponent<ZapNode>();
                    if (node != null && !node.IsZapped)
                    {
                        _gameManager.OnZapUsed();
                        StartCoroutine(ChainZap(node));
                    }
                }
            }
        }

        private IEnumerator ChainZap(ZapNode startNode)
        {
            _isChaining = true;
            var queue = new Queue<ZapNode>();
            queue.Enqueue(startNode);
            startNode.Zap();
            _gameManager.OnNodeZapped();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return new WaitForSeconds(ChainDelay);

                // 近くの未ザップノードに連鎖
                foreach (var node in _nodes)
                {
                    if (node == null || node.IsZapped) continue;
                    float dist = Vector2.Distance(current.transform.position, node.transform.position);
                    if (dist <= ChainRadius)
                    {
                        node.Zap();
                        _gameManager.OnNodeZapped();
                        queue.Enqueue(node);
                    }
                }
            }

            _isChaining = false;
            _gameManager.OnChainEnded();
        }
    }
}
