using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game048_GlassBall
{
    public class SlopeManager : MonoBehaviour
    {
        [SerializeField] private GlassBallGameManager _gameManager;

        private struct SlopeData
        {
            public Vector2 Pos;
            public float Angle;
            public float Width;
            public bool Adjustable;
        }

        private List<GameObject> _slopes = new List<GameObject>();
        private List<SlopeData> _slopeData = new List<SlopeData>();
        private GameObject _ball;
        private Rigidbody2D _ballRb;
        private Vector2 _startPos;
        private Vector2 _goalPos;
        private GameObject _goalObj;
        private Sprite _ballSprite, _slopeSprite, _goalSprite;
        private Camera _mainCamera;
        private int _selectedSlope = -1;

        private static readonly List<SlopeData[]> Stages = new List<SlopeData[]>
        {
            new SlopeData[] {
                new SlopeData { Pos = new Vector2(-1f, 2f), Angle = -15f, Width = 3f, Adjustable = true },
                new SlopeData { Pos = new Vector2(1.5f, 0f), Angle = 10f, Width = 3f, Adjustable = true },
                new SlopeData { Pos = new Vector2(-0.5f, -2f), Angle = -20f, Width = 3f, Adjustable = true },
            },
            new SlopeData[] {
                new SlopeData { Pos = new Vector2(-2f, 2.5f), Angle = -25f, Width = 2.5f, Adjustable = true },
                new SlopeData { Pos = new Vector2(2f, 1f), Angle = 15f, Width = 3f, Adjustable = false },
                new SlopeData { Pos = new Vector2(0f, -0.5f), Angle = -10f, Width = 3f, Adjustable = true },
                new SlopeData { Pos = new Vector2(-1f, -2.5f), Angle = -30f, Width = 2.5f, Adjustable = true },
            },
            new SlopeData[] {
                new SlopeData { Pos = new Vector2(-2f, 3f), Angle = -20f, Width = 2f, Adjustable = true },
                new SlopeData { Pos = new Vector2(1f, 1.5f), Angle = 25f, Width = 2.5f, Adjustable = true },
                new SlopeData { Pos = new Vector2(-1.5f, 0f), Angle = -15f, Width = 3f, Adjustable = false },
                new SlopeData { Pos = new Vector2(2f, -1.5f), Angle = 10f, Width = 2f, Adjustable = true },
                new SlopeData { Pos = new Vector2(-0.5f, -3f), Angle = -25f, Width = 3f, Adjustable = true },
            },
        };

        public void GenerateStage(int stage)
        {
            _mainCamera = Camera.main;
            _ballSprite = Resources.Load<Sprite>("Sprites/Game048_GlassBall/glass_ball");
            _slopeSprite = Resources.Load<Sprite>("Sprites/Game048_GlassBall/slope");
            _goalSprite = Resources.Load<Sprite>("Sprites/Game048_GlassBall/goal");

            CleanUp();

            int idx = (stage - 1) % Stages.Count;
            var stageSlopes = Stages[idx];
            _slopeData.Clear();

            foreach (var sd in stageSlopes)
            {
                _slopeData.Add(sd);
                var go = new GameObject("Slope");
                go.transform.position = new Vector3(sd.Pos.x, sd.Pos.y, 0f);
                go.transform.rotation = Quaternion.Euler(0, 0, sd.Angle);
                go.transform.localScale = new Vector3(sd.Width, 0.3f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _slopeSprite;
                sr.color = sd.Adjustable ? new Color(0.7f, 0.8f, 0.95f, 0.9f) : new Color(0.5f, 0.55f, 0.65f, 0.9f);
                sr.sortingOrder = 2;
                var bc = go.AddComponent<BoxCollider2D>();
                bc.size = new Vector2(1f, 1f);
                _slopes.Add(go);
            }

            _startPos = new Vector2(-2f, 4f);
            _goalPos = stageSlopes[stageSlopes.Length - 1].Pos + Vector2.right * stageSlopes[stageSlopes.Length - 1].Width * 0.4f + Vector2.down * 0.5f;

            _goalObj = new GameObject("Goal");
            _goalObj.transform.position = new Vector3(_goalPos.x, _goalPos.y, 0f);
            _goalObj.transform.localScale = Vector3.one * 1.2f;
            var gsr = _goalObj.AddComponent<SpriteRenderer>();
            gsr.sprite = _goalSprite;
            gsr.sortingOrder = 3;

            SpawnBall();
            _selectedSlope = -1;
        }

        private void SpawnBall()
        {
            if (_ball != null) Destroy(_ball);
            _ball = new GameObject("GlassBall");
            _ball.transform.position = new Vector3(_startPos.x, _startPos.y, 0f);
            _ball.transform.localScale = Vector3.one * 0.8f;
            var sr = _ball.AddComponent<SpriteRenderer>();
            sr.sprite = _ballSprite;
            sr.sortingOrder = 10;
            _ballRb = _ball.AddComponent<Rigidbody2D>();
            _ballRb.gravityScale = 1.5f;
            _ballRb.linearDamping = 0.3f;
            _ballRb.angularDamping = 0.5f;
            var cc = _ball.AddComponent<CircleCollider2D>();
            cc.radius = 0.4f;
            var pm = new PhysicsMaterial2D("GlassBallMat");
            pm.bounciness = 0.3f;
            pm.friction = 0.3f;
            cc.sharedMaterial = pm;
        }

        public void ResetBall()
        {
            SpawnBall();
        }

        private void CleanUp()
        {
            foreach (var s in _slopes) if (s != null) Destroy(s);
            _slopes.Clear();
            _slopeData.Clear();
            if (_ball != null) { Destroy(_ball); _ball = null; }
            if (_goalObj != null) { Destroy(_goalObj); _goalObj = null; }
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsPlaying) return;

            HandleInput();
            CheckGoalAndFall();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.isPressed && _selectedSlope >= 0)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));

                Vector2 slopePos = _slopes[_selectedSlope].transform.position;
                float angle = Mathf.Atan2(wp.y - slopePos.y, wp.x - slopePos.x) * Mathf.Rad2Deg;
                angle = Mathf.Clamp(angle, -45f, 45f);
                _slopes[_selectedSlope].transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var screenPos = Mouse.current.position.ReadValue();
                Vector3 wp = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));

                _selectedSlope = -1;
                for (int i = 0; i < _slopes.Count; i++)
                {
                    if (!_slopeData[i].Adjustable) continue;
                    if (Vector2.Distance(wp, _slopes[i].transform.position) < 1.5f)
                    {
                        _selectedSlope = i;
                        break;
                    }
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _selectedSlope = -1;
            }
        }

        private void CheckGoalAndFall()
        {
            if (_ball == null) return;
            Vector2 ballPos = _ball.transform.position;

            if (Vector2.Distance(ballPos, _goalPos) < 0.6f)
            {
                if (_gameManager != null) _gameManager.OnReachGoal();
                return;
            }

            if (ballPos.y < -6f || ballPos.x < -7f || ballPos.x > 7f)
            {
                if (_gameManager != null) _gameManager.OnFallOff();
            }
        }
    }
}
