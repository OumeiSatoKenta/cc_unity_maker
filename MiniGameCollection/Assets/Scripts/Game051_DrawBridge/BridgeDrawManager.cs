using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game051_DrawBridge
{
    public class BridgeDrawManager : MonoBehaviour
    {
        [SerializeField] private DrawBridgeGameManager _gameManager;

        private const float MaxInk = 8f;
        private const float MinSegDist = 0.2f;

        private float _inkLeft;
        private bool _drawing;
        private Vector3 _lastDrawPos;
        private List<GameObject> _segments = new List<GameObject>();
        private List<GameObject> _stageObjects = new List<GameObject>();
        private GameObject _ball;
        private Rigidbody2D _ballRb;
        private Vector2 _goalPos;
        private Sprite _lineSprite, _ballSprite, _cliffSprite, _flagSprite;
        private Camera _mainCamera;
        private bool _ballReleased;

        private struct StageInfo { public Vector2 LeftCliff, RightCliff, StartPos, GoalPos; public float GapWidth; }
        private static readonly StageInfo[] Stages = {
            new StageInfo { LeftCliff=new Vector2(-3f,-1f), RightCliff=new Vector2(2f,-1f), StartPos=new Vector2(-3.5f,0.5f), GoalPos=new Vector2(3f,0f), GapWidth=5f },
            new StageInfo { LeftCliff=new Vector2(-3.5f,-0.5f), RightCliff=new Vector2(1.5f,-2f), StartPos=new Vector2(-4f,1f), GoalPos=new Vector2(3f,-1f), GapWidth=5f },
            new StageInfo { LeftCliff=new Vector2(-3f,0f), RightCliff=new Vector2(3f,-1.5f), StartPos=new Vector2(-3.5f,1.5f), GoalPos=new Vector2(4f,-0.5f), GapWidth=6f },
        };

        public void Init(int stage)
        {
            _mainCamera = Camera.main;
            _lineSprite = Resources.Load<Sprite>("Sprites/Game051_DrawBridge/line");
            _ballSprite = Resources.Load<Sprite>("Sprites/Game051_DrawBridge/ball");
            _cliffSprite = Resources.Load<Sprite>("Sprites/Game051_DrawBridge/cliff");
            _flagSprite = Resources.Load<Sprite>("Sprites/Game051_DrawBridge/flag");

            CleanUp();

            int idx = (stage - 1) % Stages.Length;
            var s = Stages[idx];
            _goalPos = s.GoalPos;
            _inkLeft = MaxInk;
            _drawing = false;
            _ballReleased = false;

            CreateCliff(s.LeftCliff, 3f); CreateCliff(s.RightCliff, 3f);
            CreateFlag(s.GoalPos);
            SpawnBall(s.StartPos);
        }

        private void CreateCliff(Vector2 pos, float width)
        {
            var go = new GameObject("Cliff");
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(width, 2f, 1f);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _cliffSprite; sr.sortingOrder = 1;
            var bc = go.AddComponent<BoxCollider2D>(); bc.size = new Vector2(1f, 1f);
            _stageObjects.Add(go);
        }

        private void CreateFlag(Vector2 pos)
        {
            var go = new GameObject("Flag");
            go.transform.position = new Vector3(pos.x, pos.y + 0.5f, 0f);
            go.transform.localScale = Vector3.one * 1.5f;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _flagSprite; sr.sortingOrder = 3;
            _stageObjects.Add(go);
        }

        private void SpawnBall(Vector2 pos)
        {
            if (_ball != null) Destroy(_ball);
            _ball = new GameObject("Ball");
            _ball.transform.position = new Vector3(pos.x, pos.y, 0f);
            _ball.transform.localScale = Vector3.one * 0.7f;
            var sr = _ball.AddComponent<SpriteRenderer>(); sr.sprite = _ballSprite; sr.sortingOrder = 10;
            _ballRb = _ball.AddComponent<Rigidbody2D>();
            _ballRb.gravityScale = 0f;
            _ballRb.freezeRotation = true;
            var cc = _ball.AddComponent<CircleCollider2D>(); cc.radius = 0.4f;
        }

        private void CleanUp()
        {
            foreach (var s in _segments) if (s != null) Destroy(s);
            _segments.Clear();
            foreach (var o in _stageObjects) if (o != null) Destroy(o);
            _stageObjects.Clear();
            if (_ball != null) { Destroy(_ball); _ball = null; }
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsActive) return;
            if (Mouse.current == null) return;

            if (!_ballReleased)
            {
                HandleDrawing();
            }
            else
            {
                CheckGoalAndFall();
            }
        }

        private void HandleDrawing()
        {
            var screenPos = Mouse.current.position.ReadValue();
            Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            wp.z = 0f;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _drawing = true;
                _lastDrawPos = wp;
            }

            if (_drawing && Mouse.current.leftButton.isPressed && _inkLeft > 0f)
            {
                float dist = Vector3.Distance(wp, _lastDrawPos);
                if (dist >= MinSegDist)
                {
                    CreateSegment(_lastDrawPos, wp);
                    _inkLeft -= dist;
                    _lastDrawPos = wp;
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _drawing = false;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                _ballReleased = true;
                _ballRb.gravityScale = 2f;
            }
        }

        private void CreateSegment(Vector3 from, Vector3 to)
        {
            var go = new GameObject("Seg");
            Vector3 mid = (from + to) / 2f;
            go.transform.position = mid;
            float dist = Vector3.Distance(from, to);
            float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
            go.transform.localScale = new Vector3(dist / 0.32f, 0.4f, 1f);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _lineSprite; sr.sortingOrder = 4;
            sr.color = new Color(0.4f, 0.25f, 0.15f);
            var bc = go.AddComponent<BoxCollider2D>(); bc.size = new Vector2(1f, 0.5f);
            _segments.Add(go);
        }

        private void CheckGoalAndFall()
        {
            if (_ball == null) return;
            Vector2 pos = _ball.transform.position;
            if (Vector2.Distance(pos, _goalPos) < 1f)
            {
                if (_gameManager != null) _gameManager.OnReachGoal();
            }
            else if (pos.y < -6f)
            {
                if (_gameManager != null) _gameManager.OnBallFell();
            }
        }
    }
}
