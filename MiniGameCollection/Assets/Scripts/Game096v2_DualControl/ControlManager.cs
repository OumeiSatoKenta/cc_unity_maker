using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game096v2_DualControl
{
    public class ControlManager : MonoBehaviour
    {
        [SerializeField] DualControlGameManager _gameManager;
        [SerializeField] Sprite _charLeftSprite;
        [SerializeField] Sprite _charRightSprite;
        [SerializeField] Sprite _obstacleSprite;
        [SerializeField] Sprite _movingObstacleSprite;
        [SerializeField] Sprite _goalSprite;
        [SerializeField] Sprite _switchOnSprite;
        [SerializeField] Sprite _switchOffSprite;
        [SerializeField] Sprite _doorSprite;

        // Characters
        GameObject _leftChar;
        GameObject _rightChar;
        SpriteRenderer _leftCharSR;
        SpriteRenderer _rightCharSR;

        // Goal state
        bool _leftGoalReached;
        bool _rightGoalReached;
        float _leftGoalTime = -1f;
        float _rightGoalTime = -1f;

        // Stage setup
        StageManager.StageConfig _currentConfig;
        int _currentStageIndex;
        bool _isActive;
        bool _hasSwitch;

        // Switch/Door
        GameObject _leftSwitch;
        GameObject _rightSwitch;
        GameObject _leftDoor;
        GameObject _rightDoor;
        bool _leftSwitchActivated;
        bool _rightSwitchActivated;

        // Moving obstacles
        List<GameObject> _stageObjects = new List<GameObject>();
        List<(GameObject go, float speed, float minX, float maxX)> _movingObstacles
            = new List<(GameObject, float, float, float)>();

        // Camera layout
        float _camSize;
        float _camWidth;
        float _halfW;
        float _topY;
        float _bottomY;
        const float TOP_MARGIN = 1.2f;
        const float BOTTOM_MARGIN = 2.8f;
        const float CHAR_SIZE = 0.45f;

        // Drag input state
        int _leftTouchId = -1;
        int _rightTouchId = -1;
        bool _leftMouseActive;
        bool _rightMouseActive;

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            ClearStage();
            _currentConfig = config;
            _currentStageIndex = stageIndex;
            _isActive = true;
            _leftGoalReached = false;
            _rightGoalReached = false;
            _leftGoalTime = -1f;
            _rightGoalTime = -1f;
            _leftSwitchActivated = false;
            _rightSwitchActivated = false;
            _hasSwitch = stageIndex >= 3; // Stage 4 and 5

            _leftTouchId = -1;
            _rightTouchId = -1;
            _leftMouseActive = false;
            _rightMouseActive = false;

            // Camera layout (cache Camera.main to avoid repeated FindFirstObjectByType calls)
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[ControlManager] Camera.main is null."); return; }
            _camSize = cam.orthographicSize;
            _camWidth = _camSize * cam.aspect;
            _halfW = _camWidth * 0.5f;
            _topY = _camSize - TOP_MARGIN - 0.5f;
            _bottomY = -(_camSize - BOTTOM_MARGIN) + 0.5f;

            SpawnCharacters();
            SpawnObstacles();
            SpawnGoals();
            if (_hasSwitch) SpawnSwitchesAndDoors();
        }

        void SpawnCharacters()
        {
            if (_leftChar == null)
            {
                _leftChar = new GameObject("LeftChar");
                _leftCharSR = _leftChar.AddComponent<SpriteRenderer>();
                _leftCharSR.sortingOrder = 5;
            }
            if (_rightChar == null)
            {
                _rightChar = new GameObject("RightChar");
                _rightCharSR = _rightChar.AddComponent<SpriteRenderer>();
                _rightCharSR.sortingOrder = 5;
            }

            _leftCharSR.sprite = _charLeftSprite;
            _rightCharSR.sprite = _charRightSprite;

            float scale = CHAR_SIZE / 1.28f; // 128px sprite → world units
            _leftChar.transform.localScale = Vector3.one * scale;
            _rightChar.transform.localScale = Vector3.one * scale;

            _leftChar.transform.position = new Vector3(-_halfW * 0.5f, _topY, 0f);
            _rightChar.transform.position = new Vector3(_halfW * 0.5f, _topY, 0f);
        }

        void SpawnObstacles()
        {
            float availH = (_camSize * 2f) - TOP_MARGIN - BOTTOM_MARGIN;
            int rowCount = 2 + _currentConfig.countMultiplier; // 3〜5 rows
            float rowSpacing = availH / (rowCount + 1);
            float obstW = 0.7f;
            float obstH = 0.35f;

            for (int row = 1; row <= rowCount; row++)
            {
                float yPos = _topY - rowSpacing * row;

                // Left side obstacle(s)
                SpawnStaticObstacle(-_halfW * 0.5f, yPos, obstW, obstH, row);

                // Right side: same layout on Stage1, different on Stage2+
                float rightXOffset = _currentStageIndex >= 1 ? (row % 2 == 0 ? 0.2f : -0.2f) : 0f;
                SpawnStaticObstacle(_halfW * 0.5f + rightXOffset * _halfW, yPos, obstW, obstH, row);

                // Add moving obstacles on Stage3+
                if (_currentStageIndex >= 2 && row % 2 == 1)
                {
                    float speed = 1.2f * _currentConfig.speedMultiplier;
                    SpawnMovingObstacle(-_halfW * 0.5f, yPos - rowSpacing * 0.5f, speed, true);
                    SpawnMovingObstacle(_halfW * 0.5f, yPos - rowSpacing * 0.5f, -speed, false);
                }
            }
        }

        void SpawnStaticObstacle(float cx, float cy, float w, float h, int row)
        {
            var go = new GameObject($"Obstacle_r{row}_x{cx:F1}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _obstacleSprite;
            sr.sortingOrder = 3;
            go.transform.position = new Vector3(cx, cy, 0f);
            go.transform.localScale = new Vector3(w, h, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            _stageObjects.Add(go);
        }

        void SpawnMovingObstacle(float cx, float cy, float speed, bool isLeft)
        {
            float margin = 0.4f;
            float minX = isLeft ? -_camWidth + margin : margin;
            float maxX = isLeft ? -margin : _camWidth - margin;

            var go = new GameObject($"MovingObstacle_{cx:F1}_{cy:F1}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _movingObstacleSprite;
            sr.sortingOrder = 3;
            go.transform.position = new Vector3(cx, cy, 0f);
            go.transform.localScale = new Vector3(1.2f, 0.3f, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            _stageObjects.Add(go);
            _movingObstacles.Add((go, speed, minX, maxX));
        }

        void SpawnGoals()
        {
            float goalSize = 0.6f;

            var leftGoal = new GameObject("GoalLeft");
            var lgSR = leftGoal.AddComponent<SpriteRenderer>();
            lgSR.sprite = _goalSprite;
            lgSR.sortingOrder = 2;
            leftGoal.transform.position = new Vector3(-_halfW * 0.5f, _bottomY, 0f);
            leftGoal.transform.localScale = Vector3.one * goalSize;
            var lgCol = leftGoal.AddComponent<CircleCollider2D>();
            lgCol.isTrigger = true;
            lgCol.radius = 0.45f;
            _stageObjects.Add(leftGoal);

            var rightGoal = new GameObject("GoalRight");
            var rgSR = rightGoal.AddComponent<SpriteRenderer>();
            rgSR.sprite = _goalSprite;
            rgSR.sortingOrder = 2;
            rightGoal.transform.position = new Vector3(_halfW * 0.5f, _bottomY, 0f);
            rightGoal.transform.localScale = Vector3.one * goalSize;
            var rgCol = rightGoal.AddComponent<CircleCollider2D>();
            rgCol.isTrigger = true;
            rgCol.radius = 0.45f;
            _stageObjects.Add(rightGoal);
        }

        void SpawnSwitchesAndDoors()
        {
            float availH = (_camSize * 2f) - TOP_MARGIN - BOTTOM_MARGIN;
            float switchY = _topY - availH * 0.4f;
            float doorY = _topY - availH * 0.6f;

            // Left switch activates Right door
            _leftSwitch = CreateSwitch("LeftSwitch", -_halfW * 0.5f, switchY);
            _rightDoor = CreateDoor("RightDoor", _halfW * 0.5f, doorY);

            // Right switch activates Left door
            _rightSwitch = CreateSwitch("RightSwitch", _halfW * 0.5f, switchY);
            _leftDoor = CreateDoor("LeftDoor", -_halfW * 0.5f, doorY);
        }

        GameObject CreateSwitch(string name, float x, float y)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _switchOffSprite;
            sr.sortingOrder = 4;
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * 0.5f;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            _stageObjects.Add(go);
            return go;
        }

        GameObject CreateDoor(string name, float x, float y)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _doorSprite;
            sr.sortingOrder = 3;
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(0.4f, 0.7f, 1f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            _stageObjects.Add(go);
            return go;
        }

        void Update()
        {
            if (!_isActive || _gameManager == null || !_gameManager.IsPlaying) return;

            UpdateMovingObstacles();
            HandleInput();
            CheckCollisions();
        }

        void UpdateMovingObstacles()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _movingObstacles.Count; i++)
            {
                var (go, speed, minX, maxX) = _movingObstacles[i];
                if (go == null) continue;
                float newX = go.transform.position.x + speed * dt;
                if (newX < minX || newX > maxX)
                {
                    speed = -speed;
                    _movingObstacles[i] = (go, speed, minX, maxX);
                    newX = go.transform.position.x + speed * dt;
                }
                go.transform.position = new Vector3(newX, go.transform.position.y, 0f);
            }
        }

        void HandleInput()
        {
            var cam = Camera.main;
            if (cam == null) return;

            Vector2? leftTarget = null;
            Vector2? rightTarget = null;

            // Touch input (mobile)
            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.press.ReadValue() > 0)
                    {
                        Vector2 tp = touch.position.ReadValue();
                        Vector2 worldPos = cam.ScreenToWorldPoint(new Vector3(tp.x, tp.y, 10f));
                        if (tp.x < Screen.width * 0.5f)
                            leftTarget = worldPos;
                        else
                            rightTarget = worldPos;
                    }
                }
            }

            // Mouse input (PC/Editor)
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Vector2 mp = Mouse.current.position.ReadValue();
                Vector2 worldPos = cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, 10f));
                if (mp.x < Screen.width * 0.5f)
                    leftTarget = worldPos;
                else
                    rightTarget = worldPos;
            }

            float moveSpeed = 5f * _currentConfig.speedMultiplier;
            float clampedMoveSpeed = Mathf.Min(moveSpeed, 8f);

            if (leftTarget.HasValue && _leftChar != null)
            {
                // Clamp to left half of world
                Vector2 clamped = leftTarget.Value;
                clamped.x = Mathf.Clamp(clamped.x, -_camWidth + 0.3f, -0.1f);
                clamped.y = Mathf.Clamp(clamped.y, _bottomY, _topY);
                _leftChar.transform.position = Vector2.MoveTowards(
                    _leftChar.transform.position, clamped,
                    clampedMoveSpeed * Time.deltaTime);
            }

            if (rightTarget.HasValue && _rightChar != null)
            {
                Vector2 clamped = rightTarget.Value;
                clamped.x = Mathf.Clamp(clamped.x, 0.1f, _camWidth - 0.3f);
                clamped.y = Mathf.Clamp(clamped.y, _bottomY, _topY);
                _rightChar.transform.position = Vector2.MoveTowards(
                    _rightChar.transform.position, clamped,
                    clampedMoveSpeed * Time.deltaTime);
            }
        }

        void CheckCollisions()
        {
            if (_leftChar != null) CheckCharCollisions(_leftChar, isLeft: true);
            if (_rightChar != null) CheckCharCollisions(_rightChar, isLeft: false);
        }

        void CheckCharCollisions(GameObject character, bool isLeft)
        {
            float radius = CHAR_SIZE * 0.4f;
            var hits = Physics2D.OverlapCircleAll(character.transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit == null || hit.gameObject == null) continue;
                string goName = hit.gameObject.name;

                // Goal check
                if (isLeft && goName == "GoalLeft" && !_leftGoalReached)
                {
                    _leftGoalReached = true;
                    _leftGoalTime = Time.time;
                    StartCoroutine(GoalPopAnimation(character));
                    CheckBothGoals();
                    return;
                }
                if (!isLeft && goName == "GoalRight" && !_rightGoalReached)
                {
                    _rightGoalReached = true;
                    _rightGoalTime = Time.time;
                    StartCoroutine(GoalPopAnimation(character));
                    CheckBothGoals();
                    return;
                }

                // Switch check
                if (isLeft && _hasSwitch && _leftSwitch != null && goName == "LeftSwitch" && !_leftSwitchActivated)
                {
                    _leftSwitchActivated = true;
                    ActivateSwitch(_leftSwitch, _rightDoor);
                }
                if (!isLeft && _hasSwitch && _rightSwitch != null && goName == "RightSwitch" && !_rightSwitchActivated)
                {
                    _rightSwitchActivated = true;
                    ActivateSwitch(_rightSwitch, _leftDoor);
                }

                // Obstacle check (not trigger)
                if (!hit.isTrigger)
                {
                    if (goName.StartsWith("Obstacle") || goName.StartsWith("MovingObstacle") ||
                        (goName.EndsWith("Door") && IsActiveDoor(goName)))
                    {
                        TriggerTrapHit(character, isLeft);
                        return;
                    }
                }
            }
        }

        bool IsActiveDoor(string name)
        {
            if (name == "LeftDoor" && _leftDoor != null && _leftDoor.activeSelf) return true;
            if (name == "RightDoor" && _rightDoor != null && _rightDoor.activeSelf) return true;
            return false;
        }

        void ActivateSwitch(GameObject sw, GameObject door)
        {
            if (sw != null)
            {
                var sr = sw.GetComponent<SpriteRenderer>();
                if (sr != null && _switchOnSprite != null) sr.sprite = _switchOnSprite;
            }
            if (door != null)
            {
                door.SetActive(false);
            }
        }

        void CheckBothGoals()
        {
            if (!_leftGoalReached || !_rightGoalReached) return;
            bool isSynchro = Mathf.Abs(_leftGoalTime - _rightGoalTime) <= 1.0f;
            _isActive = false;
            if (isSynchro) StartCoroutine(SynchroFlash());
            _gameManager.OnStageClear(isSynchro);
        }

        void TriggerTrapHit(GameObject character, bool isLeft)
        {
            if (!_isActive) return;
            _isActive = false;
            StartCoroutine(TrapHitEffect(character));
            _gameManager.OnTrapHit();
        }

        IEnumerator GoalPopAnimation(GameObject character)
        {
            if (character == null) yield break;
            Vector3 original = character.transform.localScale;
            float elapsed = 0f;
            float duration = 0.2f;
            while (elapsed < duration)
            {
                if (character == null) yield break; // guard against mid-animation Destroy
                float t = elapsed / duration;
                float scale = t < 0.5f ? Mathf.Lerp(1f, 1.3f, t * 2f) : Mathf.Lerp(1.3f, 1f, (t - 0.5f) * 2f);
                character.transform.localScale = original * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (character != null) character.transform.localScale = original;
        }

        IEnumerator TrapHitEffect(GameObject character)
        {
            if (character == null) yield break;
            var sr = character.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
            Color original = sr.color;
            float elapsed = 0f;
            float duration = 0.15f;
            while (elapsed < duration)
            {
                if (character == null || sr == null) yield break; // guard against mid-animation Destroy
                float t = elapsed / duration;
                sr.color = Color.Lerp(Color.red, original, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (sr != null) sr.color = original;

            // Camera shake
            if (Camera.main != null)
                StartCoroutine(CameraShake(0.3f, 0.15f));
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 originalPos = cam.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (cam == null) yield break; // guard against camera destruction
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                cam.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (cam != null) cam.transform.position = originalPos;
        }

        IEnumerator SynchroFlash()
        {
            Color golden = new Color(1f, 0.85f, 0.2f);
            for (int i = 0; i < 3; i++)
            {
                if (_leftCharSR != null) _leftCharSR.color = golden;
                if (_rightCharSR != null) _rightCharSR.color = golden;
                yield return new WaitForSeconds(0.1f);
                if (_leftCharSR != null) _leftCharSR.color = Color.white;
                if (_rightCharSR != null) _rightCharSR.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
        }

        void ClearStage()
        {
            foreach (var go in _stageObjects)
            {
                if (go != null) Destroy(go);
            }
            _stageObjects.Clear();
            _movingObstacles.Clear();
            _leftSwitch = null;
            _rightSwitch = null;
            _leftDoor = null;
            _rightDoor = null;
        }

        void OnDestroy()
        {
            ClearStage();
            if (_leftChar != null) Destroy(_leftChar);
            if (_rightChar != null) Destroy(_rightChar);
        }
    }
}
