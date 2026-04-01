using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game038_FlyBird
{
    public class FlyManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム状態管理")]
        private FlyBirdGameManager _gameManager;

        [SerializeField, Tooltip("鳥Transform")]
        private Transform _birdTransform;

        [SerializeField, Tooltip("パイプスプライト")]
        private Sprite _pipeSprite;

        private float _birdVelocityY;
        private float _pipeTimer;
        private List<Transform> _pipes = new List<Transform>();

        private const float Gravity = -12f;
        private const float FlapForce = 5.5f;
        private const float PipeSpeed = 3f;
        private const float PipeInterval = 2f;
        private const float GapSize = 2.8f;
        private const float BirdX = -2f;
        private const float HitRadius = 0.35f;

        public void StartGame()
        {
            _birdVelocityY = 0f;
            _pipeTimer = 1.5f; // 最初のパイプまで少し猶予
            if (_birdTransform != null)
                _birdTransform.position = new Vector3(BirdX, 0f, 0f);
        }

        private void Update()
        {
            if (!_gameManager.IsPlaying) return;

            HandleInput();
            UpdateBird();
            UpdatePipes();
            CheckCollision();
        }

        private void HandleInput()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _birdVelocityY = FlapForce;
            }
        }

        private void UpdateBird()
        {
            if (_birdTransform == null) return;

            _birdVelocityY += Gravity * Time.deltaTime;
            var pos = _birdTransform.position;
            pos.y += _birdVelocityY * Time.deltaTime;
            _birdTransform.position = pos;

            // 傾き
            float angle = Mathf.Clamp(_birdVelocityY * 5f, -60f, 30f);
            _birdTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            // 画面外
            if (pos.y < -6f || pos.y > 6f)
            {
                _gameManager.OnCrash();
            }
        }

        private void UpdatePipes()
        {
            _pipeTimer -= Time.deltaTime;
            if (_pipeTimer <= 0f)
            {
                _pipeTimer = PipeInterval;
                SpawnPipePair();
            }

            for (int i = _pipes.Count - 1; i >= 0; i--)
            {
                if (_pipes[i] == null) { _pipes.RemoveAt(i); continue; }
                _pipes[i].position += new Vector3(-PipeSpeed * Time.deltaTime, 0f, 0f);
                if (_pipes[i].position.x < -7f) { Destroy(_pipes[i].gameObject); _pipes.RemoveAt(i); }
            }
        }

        private void SpawnPipePair()
        {
            float gapCenter = Random.Range(-2f, 2.5f);

            // 上パイプ
            var topObj = new GameObject("PipeTop");
            topObj.transform.position = new Vector3(6f, gapCenter + GapSize / 2f + 2.5f, 0f);
            topObj.transform.localScale = new Vector3(1f, 5f, 1f);
            var topSr = topObj.AddComponent<SpriteRenderer>();
            topSr.sprite = _pipeSprite; topSr.sortingOrder = 1;
            topSr.flipY = true;
            var topCol = topObj.AddComponent<BoxCollider2D>();
            topCol.size = new Vector2(1f, 1f);
            _pipes.Add(topObj.transform);

            // 下パイプ
            var botObj = new GameObject("PipeBot");
            botObj.transform.position = new Vector3(6f, gapCenter - GapSize / 2f - 2.5f, 0f);
            botObj.transform.localScale = new Vector3(1f, 5f, 1f);
            var botSr = botObj.AddComponent<SpriteRenderer>();
            botSr.sprite = _pipeSprite; botSr.sortingOrder = 1;
            var botCol = botObj.AddComponent<BoxCollider2D>();
            botCol.size = new Vector2(1f, 1f);
            _pipes.Add(botObj.transform);

            // スコアトリガー（見えない）
            var scoreObj = new GameObject("ScoreTrigger");
            scoreObj.transform.position = new Vector3(6f, gapCenter, 0f);
            scoreObj.transform.localScale = new Vector3(0.3f, GapSize, 1f);
            var scoreCol = scoreObj.AddComponent<BoxCollider2D>();
            scoreCol.isTrigger = true;
            scoreCol.size = new Vector2(1f, 1f);
            var scoreTrigger = scoreObj.AddComponent<ScoreTrigger>();
            scoreTrigger.Initialize(_gameManager);
            _pipes.Add(scoreObj.transform);
        }

        private void CheckCollision()
        {
            if (_birdTransform == null) return;
            Vector2 birdPos = _birdTransform.position;

            foreach (var pipe in _pipes)
            {
                if (pipe == null) continue;
                var col = pipe.GetComponent<BoxCollider2D>();
                if (col == null || col.isTrigger) continue;

                // パイプのBoundsでざっくり判定
                if (col.bounds.Contains(new Vector3(birdPos.x, birdPos.y, 0f)))
                {
                    _gameManager.OnCrash();
                    return;
                }
            }
        }
    }
}
