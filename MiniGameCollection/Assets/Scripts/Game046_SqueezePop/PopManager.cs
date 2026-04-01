using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game046_SqueezePop
{
    public class PopManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private SqueezePopGameManager _gameManager;

        [SerializeField, Tooltip("バブルスプライト")]
        private Sprite _bubbleSprite;

        [SerializeField, Tooltip("ポップスプライト")]
        private Sprite _popSprite;

        private Camera _mainCamera;
        private List<Bubble> _bubbles = new List<Bubble>();

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            SpawnBubbles();
        }

        private void SpawnBubbles()
        {
            int total = _gameManager.TotalBubbles;
            for (int i = 0; i < total; i++)
            {
                float x = Random.Range(-3.5f, 3.5f);
                float y = Random.Range(-3f, 4f);
                float scale = Random.Range(0.5f, 1.2f);

                var obj = new GameObject($"Bubble_{i}");
                obj.transform.position = new Vector3(x, y, 0f);
                obj.transform.localScale = new Vector3(scale, scale, 1f);
                obj.transform.SetParent(transform);

                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _bubbleSprite;
                sr.sortingOrder = 2;
                // ランダム色味
                sr.color = Color.HSVToRGB(Random.Range(0f, 1f), 0.3f, 1f);

                var col = obj.AddComponent<CircleCollider2D>();
                col.radius = 0.5f;

                var bubble = obj.AddComponent<Bubble>();
                bubble.Initialize(_popSprite, OnBubblePopped);
                _bubbles.Add(bubble);
            }
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying) return;
            if (Mouse.current == null) return;

            // 長押しでスクイーズ→バブルを膨張→ポップ
            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector3 wp = _mainCamera.ScreenToWorldPoint(mp);

                foreach (var b in _bubbles)
                {
                    if (b == null || b.IsPopped) continue;
                    float dist = Vector2.Distance(wp, b.transform.position);
                    if (dist < b.transform.localScale.x * 0.6f)
                    {
                        b.Squeeze(Time.deltaTime);
                        break; // 1つずつ
                    }
                }
            }
        }

        private void OnBubblePopped(Bubble b)
        {
            _bubbles.Remove(b);
            _gameManager.OnBubblePopped();
        }
    }
}
