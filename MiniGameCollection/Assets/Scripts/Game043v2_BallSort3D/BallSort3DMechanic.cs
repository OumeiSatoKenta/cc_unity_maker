using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game043v2_BallSort3D
{
    public class BallSort3DMechanic : MonoBehaviour
    {
        [SerializeField] BallSort3DGameManager _gameManager;
        [SerializeField] BallSort3DUI _ui;
        [SerializeField] Sprite _spriteTube;
        [SerializeField] Sprite _spriteBallR;
        [SerializeField] Sprite _spriteBallG;
        [SerializeField] Sprite _spriteBallB;
        [SerializeField] Sprite _spriteBallY;
        [SerializeField] Sprite _spriteBallM;
        [SerializeField] Sprite _spriteLid;
        [SerializeField] Sprite _spriteLockIcon;
        [SerializeField] Sprite _spriteRotateIcon;

        // Tube capacity (balls per tube)
        const int TUBE_CAPACITY = 4;

        class BallData
        {
            public int colorId; // 0=R,1=G,2=B,3=Y,4=M
            public bool isLocked;
            public int lockTapsRemaining; // 2 taps to unlock
        }

        class TubeData
        {
            public List<BallData> balls = new List<BallData>();
            public bool hasCover;
            public bool isRotated;
            public bool isSelected;
            public GameObject tubeObj;
            public GameObject coverObj;
            public GameObject rotateIconObj;
            public List<GameObject> ballObjs = new List<GameObject>();
        }

        List<TubeData> _tubes = new List<TubeData>();
        int _selectedTubeIndex = -1;
        bool _isActive;
        int _comboCount;
        int _lastMovedColorId = -1;

        // Undo
        struct TubeSnapshot
        {
            public List<BallData> balls;
            public bool hasCover;
            public bool isRotated;
        }
        List<TubeSnapshot[]> _undoStack = new List<TubeSnapshot[]>();

        // Stage config
        int _stageIndex;
        bool _hasLock;
        bool _hasCover;
        bool _hasRotation;
        bool _hasTimer;
        float _timeRemaining;
        bool _timerActive;
        int _minMoves;

        // Color palette
        Color[] _ballColors = {
            new Color(0.94f, 0.24f, 0.24f), // R
            new Color(0.24f, 0.78f, 0.32f), // G
            new Color(0.24f, 0.47f, 0.94f), // B
            new Color(0.94f, 0.86f, 0.20f), // Y
            new Color(0.78f, 0.24f, 0.78f), // M
        };

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active) _timerActive = false;
        }

        public void SetupStage(int stageIndex)
        {
            _stageIndex = stageIndex;
            _hasLock = stageIndex == 1 || stageIndex == 4;
            _hasCover = stageIndex == 2 || stageIndex == 4;
            _hasRotation = stageIndex == 3 || stageIndex == 4;
            _hasTimer = stageIndex == 4;
            _timerActive = _hasTimer;
            _timeRemaining = 120f;
            _comboCount = 0;
            _lastMovedColorId = -1;
            _undoStack.Clear();
            _selectedTubeIndex = -1;

            ClearTubes();

            int tubeCount = stageIndex == 0 ? 4 :
                            stageIndex == 1 ? 5 :
                            stageIndex == 2 ? 6 :
                            stageIndex == 3 ? 7 : 8;
            int colorCount = stageIndex < 3 ? stageIndex + 2 : (stageIndex == 3 ? 4 : 5);
            // empty tubes = 1 always
            int filledTubes = tubeCount - 1;

            _minMoves = filledTubes * TUBE_CAPACITY / 2; // rough estimate

            // Generate puzzle
            var allBalls = new List<BallData>();
            for (int c = 0; c < colorCount; c++)
            {
                for (int j = 0; j < TUBE_CAPACITY; j++)
                {
                    var bd = new BallData { colorId = c, isLocked = false, lockTapsRemaining = 0 };
                    allBalls.Add(bd);
                }
            }
            // Shuffle
            for (int i = allBalls.Count - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                var tmp = allBalls[i]; allBalls[i] = allBalls[rnd]; allBalls[rnd] = tmp;
            }

            // Distribute into filled tubes
            _tubes = new List<TubeData>();
            int ballIdx = 0;
            for (int t = 0; t < filledTubes; t++)
            {
                var tube = new TubeData();
                for (int b = 0; b < TUBE_CAPACITY && ballIdx < allBalls.Count; b++, ballIdx++)
                    tube.balls.Add(allBalls[ballIdx]);
                // Apply lock to one ball in stage 1/4
                if (_hasLock && tube.balls.Count > 0)
                {
                    int lockIdx = Random.Range(0, tube.balls.Count);
                    if (Random.value < 0.3f)
                    {
                        tube.balls[lockIdx].isLocked = true;
                        tube.balls[lockIdx].lockTapsRemaining = 2;
                    }
                }
                // Apply cover to some tubes in stage 2/4
                if (_hasCover && Random.value < 0.4f)
                    tube.hasCover = true;
                // Apply rotation to some tubes in stage 3/4
                if (_hasRotation && Random.value < 0.35f)
                {
                    tube.isRotated = true;
                    tube.balls.Reverse();
                }
                _tubes.Add(tube);
            }
            // Empty tube
            _tubes.Add(new TubeData());

            PlaceTubes();
            _isActive = true;
        }

        void ClearTubes()
        {
            foreach (var tube in _tubes)
            {
                if (tube.tubeObj != null) Destroy(tube.tubeObj);
                if (tube.coverObj != null) Destroy(tube.coverObj);
                if (tube.rotateIconObj != null) Destroy(tube.rotateIconObj);
                foreach (var b in tube.ballObjs)
                    if (b != null) Destroy(b);
            }
            _tubes.Clear();
        }

        void PlaceTubes()
        {
            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float topMargin = 1.5f;
            float bottomMargin = 2.8f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            float tubeVisualHeight = Mathf.Min(availableHeight, 3.8f);
            float tubeWidth = 0.7f;
            float tubeSpacing = (camWidth * 2f - tubeWidth) / Mathf.Max(_tubes.Count, 1);

            float startX = -camWidth + tubeWidth * 0.5f + (camWidth * 2f - tubeSpacing * _tubes.Count) * 0.5f + tubeSpacing * 0.5f;
            float centerY = camSize - topMargin - tubeVisualHeight * 0.5f;

            for (int i = 0; i < _tubes.Count; i++)
            {
                var tube = _tubes[i];
                float x = startX + i * tubeSpacing;
                float y = centerY;

                // Tube object
                var tubeObj = new GameObject($"Tube_{i}");
                tubeObj.transform.SetParent(transform);
                tubeObj.transform.position = new Vector3(x, y, 0f);

                var sr = tubeObj.AddComponent<SpriteRenderer>();
                sr.sprite = _spriteTube;
                sr.sortingOrder = 0;
                sr.color = new Color(1f, 1f, 1f, 0.85f);
                tubeObj.transform.localScale = new Vector3(tubeWidth / (sr.sprite != null ? sr.sprite.bounds.size.x : 1f),
                                                           tubeVisualHeight / (sr.sprite != null ? sr.sprite.bounds.size.y : 1f), 1f);

                // Collider for tap
                var col = tubeObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(tubeWidth, tubeVisualHeight);

                tube.tubeObj = tubeObj;

                // Cover
                if (tube.hasCover)
                {
                    var coverObj = new GameObject($"Cover_{i}");
                    coverObj.transform.SetParent(tubeObj.transform);
                    coverObj.transform.localPosition = new Vector3(0f, 0.5f, -0.1f);
                    var csr = coverObj.AddComponent<SpriteRenderer>();
                    csr.sprite = _spriteLid;
                    csr.sortingOrder = 2;
                    coverObj.transform.localScale = new Vector3(1f / tubeWidth * tubeWidth, 0.15f, 1f);
                    tube.coverObj = coverObj;
                }

                // Rotate icon
                if (tube.isRotated)
                {
                    var rotObj = new GameObject($"RotateIcon_{i}");
                    rotObj.transform.SetParent(tubeObj.transform);
                    rotObj.transform.localPosition = new Vector3(0f, -0.5f, -0.1f);
                    var rsr = rotObj.AddComponent<SpriteRenderer>();
                    rsr.sprite = _spriteRotateIcon;
                    rsr.sortingOrder = 2;
                    rotObj.transform.localScale = Vector3.one * 0.3f;
                    tube.rotateIconObj = rotObj;
                }

                RefreshBallVisuals(i, tubeVisualHeight);
            }
        }

        void RefreshBallVisuals(int tubeIdx, float tubeVisualHeight = 3.8f)
        {
            var tube = _tubes[tubeIdx];
            // Remove existing ball visuals
            foreach (var b in tube.ballObjs)
                if (b != null) Destroy(b);
            tube.ballObjs.Clear();

            if (tube.tubeObj == null) return;

            float ballSize = 0.48f;
            float bottomY = -tubeVisualHeight * 0.5f + ballSize * 0.5f + 0.1f;

            for (int b = 0; b < tube.balls.Count; b++)
            {
                var ball = tube.balls[b];
                var ballObj = new GameObject($"Ball_{tubeIdx}_{b}");
                ballObj.transform.SetParent(tube.tubeObj.transform);
                ballObj.transform.localPosition = new Vector3(0f, bottomY + b * (ballSize + 0.04f), -0.05f);
                ballObj.transform.localScale = Vector3.one * ballSize;

                var sr = ballObj.AddComponent<SpriteRenderer>();
                sr.sprite = GetBallSprite(ball.colorId);
                sr.color = _ballColors[ball.colorId];
                sr.sortingOrder = 1;

                // Lock icon
                if (ball.isLocked)
                {
                    var lockObj = new GameObject("LockIcon");
                    lockObj.transform.SetParent(ballObj.transform);
                    lockObj.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                    lockObj.transform.localScale = Vector3.one * 0.5f;
                    var lsr = lockObj.AddComponent<SpriteRenderer>();
                    lsr.sprite = _spriteLockIcon;
                    lsr.sortingOrder = 3;
                }

                tube.ballObjs.Add(ballObj);
            }

            // Highlight if selected
            if (tube.isSelected && tube.balls.Count > 0)
            {
                var topBallObj = tube.ballObjs[tube.balls.Count - 1];
                if (topBallObj != null)
                    topBallObj.transform.localPosition += new Vector3(0f, 0.15f, 0f);
            }
        }

        Sprite GetBallSprite(int colorId)
        {
            return colorId switch {
                0 => _spriteBallR,
                1 => _spriteBallG,
                2 => _spriteBallB,
                3 => _spriteBallY,
                4 => _spriteBallM,
                _ => _spriteBallR
            };
        }

        float GetTubeVisualHeight()
        {
            float camSize = Camera.main.orthographicSize;
            float topMargin = 1.5f;
            float bottomMargin = 2.8f;
            float availableHeight = camSize * 2f - topMargin - bottomMargin;
            return Mathf.Min(availableHeight, 3.8f);
        }

        void Update()
        {
            if (!_isActive) return;

            if (_hasTimer && _timerActive)
            {
                _timeRemaining -= Time.deltaTime;
                _ui?.UpdateTimer(_timeRemaining);
                if (_timeRemaining <= 0f)
                {
                    _timerActive = false;
                    _gameManager.OnGameOver();
                    return;
                }
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                HandleTap(worldPos);
            }
        }

        void HandleTap(Vector2 worldPos)
        {
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            // Find which tube was tapped
            int tubeIdx = -1;
            for (int i = 0; i < _tubes.Count; i++)
            {
                if (_tubes[i].tubeObj != null && _tubes[i].tubeObj == hit.gameObject)
                {
                    tubeIdx = i;
                    break;
                }
            }
            if (tubeIdx < 0) return;

            var tube = _tubes[tubeIdx];

            // Handle cover first
            if (tube.hasCover)
            {
                RemoveCover(tubeIdx);
                return;
            }

            // Handle rotation
            if (tube.isRotated)
            {
                RotateTube(tubeIdx);
                return;
            }

            if (_selectedTubeIndex < 0)
            {
                // Select tube
                if (tube.balls.Count == 0) return;
                var topBall = tube.balls[tube.balls.Count - 1];
                if (topBall.isLocked)
                {
                    topBall.lockTapsRemaining--;
                    if (topBall.lockTapsRemaining <= 0)
                        topBall.isLocked = false;
                    RefreshBallVisuals(tubeIdx, GetTubeVisualHeight());
                    return;
                }
                _selectedTubeIndex = tubeIdx;
                tube.isSelected = true;
                RefreshBallVisuals(tubeIdx, GetTubeVisualHeight());
                StartCoroutine(SelectPulse(tube.ballObjs.Count > 0 ? tube.ballObjs[tube.balls.Count - 1] : null));
            }
            else if (_selectedTubeIndex == tubeIdx)
            {
                // Deselect
                _tubes[_selectedTubeIndex].isSelected = false;
                RefreshBallVisuals(_selectedTubeIndex, GetTubeVisualHeight());
                _selectedTubeIndex = -1;
            }
            else
            {
                // Try to move
                TryMove(_selectedTubeIndex, tubeIdx);
            }
        }

        void RemoveCover(int tubeIdx)
        {
            var tube = _tubes[tubeIdx];
            tube.hasCover = false;
            if (tube.coverObj != null)
            {
                Destroy(tube.coverObj);
                tube.coverObj = null;
            }
        }

        void RotateTube(int tubeIdx)
        {
            var tube = _tubes[tubeIdx];
            tube.isRotated = false;
            tube.balls.Reverse();
            if (tube.rotateIconObj != null)
            {
                Destroy(tube.rotateIconObj);
                tube.rotateIconObj = null;
            }
            RefreshBallVisuals(tubeIdx, GetTubeVisualHeight());
        }

        void TryMove(int fromIdx, int toIdx)
        {
            var from = _tubes[fromIdx];
            var to = _tubes[toIdx];

            from.isSelected = false;

            if (from.balls.Count == 0) { _selectedTubeIndex = -1; return; }
            if (to.balls.Count >= TUBE_CAPACITY) { ShowError(fromIdx); return; }

            var movingBall = from.balls[from.balls.Count - 1];
            // Must be same color as top of destination, or destination empty
            if (to.balls.Count > 0 && to.balls[to.balls.Count - 1].colorId != movingBall.colorId)
            {
                ShowError(fromIdx);
                RefreshBallVisuals(fromIdx, GetTubeVisualHeight());
                _selectedTubeIndex = -1;
                return;
            }

            // Save undo
            SaveUndoSnapshot();

            // Move ball
            to.balls.Add(movingBall);
            from.balls.RemoveAt(from.balls.Count - 1);

            _gameManager.OnMoveMade();

            // Combo
            if (movingBall.colorId == _lastMovedColorId)
            {
                _comboCount++;
                int bonus = _comboCount >= 3 ? Mathf.RoundToInt(50 * 2.0f) :
                            _comboCount >= 2 ? Mathf.RoundToInt(50 * 1.5f) : 50;
                _gameManager.OnScoreAdded(bonus);
                _gameManager.OnComboChanged(_comboCount);
            }
            else
            {
                _comboCount = 1;
                _gameManager.OnScoreAdded(20);
                _gameManager.OnComboChanged(_comboCount);
            }
            _lastMovedColorId = movingBall.colorId;

            float th = GetTubeVisualHeight();
            RefreshBallVisuals(fromIdx, th);
            RefreshBallVisuals(toIdx, th);

            var topBallObj = to.ballObjs.Count > 0 && to.balls.Count - 1 < to.ballObjs.Count
                ? to.ballObjs[to.balls.Count - 1] : null;
            StartCoroutine(PlacePulse(topBallObj));

            _selectedTubeIndex = -1;

            // Check clear
            if (IsStageClear())
            {
                StartCoroutine(ClearEffect());
            }
            // Check deadlock
            else if (IsDeadlocked())
            {
                _gameManager.OnGameOver();
            }
        }

        bool IsStageClear()
        {
            foreach (var tube in _tubes)
            {
                if (tube.balls.Count == 0) continue;
                if (tube.balls.Count != TUBE_CAPACITY) return false;
                int firstColor = tube.balls[0].colorId;
                foreach (var b in tube.balls)
                    if (b.colorId != firstColor) return false;
            }
            return true;
        }

        bool IsDeadlocked()
        {
            // Cover-tap and rotate-tap are still available actions, so not a deadlock
            foreach (var t in _tubes)
                if (t.hasCover || t.isRotated) return false;

            for (int i = 0; i < _tubes.Count; i++)
            {
                if (_tubes[i].balls.Count == 0) continue;
                for (int j = 0; j < _tubes.Count; j++)
                {
                    if (i == j) continue;
                    if (CanMove(i, j)) return false;
                }
            }
            return true;
        }

        bool CanMove(int fromIdx, int toIdx)
        {
            var from = _tubes[fromIdx];
            var to = _tubes[toIdx];
            if (from.balls.Count == 0) return false;
            if (to.balls.Count >= TUBE_CAPACITY) return false;
            if (to.balls.Count == 0) return true;
            var topFrom = from.balls[from.balls.Count - 1];
            if (topFrom.isLocked) return false;
            return to.balls[to.balls.Count - 1].colorId == topFrom.colorId;
        }

        void SaveUndoSnapshot()
        {
            var snapshot = new TubeSnapshot[_tubes.Count];
            for (int i = 0; i < _tubes.Count; i++)
            {
                var copy = new List<BallData>();
                foreach (var b in _tubes[i].balls)
                    copy.Add(new BallData { colorId = b.colorId, isLocked = b.isLocked, lockTapsRemaining = b.lockTapsRemaining });
                snapshot[i] = new TubeSnapshot { balls = copy, hasCover = _tubes[i].hasCover, isRotated = _tubes[i].isRotated };
            }
            _undoStack.Add(snapshot);
            if (_undoStack.Count > 30) _undoStack.RemoveAt(0);
        }

        public void UndoLastMove()
        {
            if (!_isActive || _undoStack.Count == 0) return;
            var snapshot = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);

            float th = GetTubeVisualHeight();
            for (int i = 0; i < _tubes.Count && i < snapshot.Length; i++)
            {
                _tubes[i].balls = snapshot[i].balls;
                _tubes[i].hasCover = snapshot[i].hasCover;
                _tubes[i].isRotated = snapshot[i].isRotated;
                _tubes[i].isSelected = false;
                // Rebuild cover/rotate icons
                if (_tubes[i].coverObj != null) Destroy(_tubes[i].coverObj);
                _tubes[i].coverObj = null;
                if (_tubes[i].rotateIconObj != null) { Destroy(_tubes[i].rotateIconObj); _tubes[i].rotateIconObj = null; }
                if (_tubes[i].isRotated)
                    CreateRotateIcon(i);
                if (_tubes[i].hasCover && _tubes[i].coverObj == null)
                    CreateCoverIcon(i, th);
                RefreshBallVisuals(i, th);
            }

            _selectedTubeIndex = -1;
            _comboCount = 0;
            _lastMovedColorId = -1;
            _gameManager.OnUndoUsed();
            _gameManager.OnComboChanged(0);
        }

        void CreateRotateIcon(int idx)
        {
            var tube = _tubes[idx];
            if (tube.tubeObj == null) return;
            var rotObj = new GameObject($"RotateIcon_{idx}");
            rotObj.transform.SetParent(tube.tubeObj.transform);
            rotObj.transform.localPosition = new Vector3(0f, -0.5f, -0.1f);
            var rsr = rotObj.AddComponent<SpriteRenderer>();
            rsr.sprite = _spriteRotateIcon;
            rsr.sortingOrder = 2;
            rotObj.transform.localScale = Vector3.one * 0.3f;
            tube.rotateIconObj = rotObj;
        }

        void CreateCoverIcon(int idx, float tubeVisualHeight)
        {
            var tube = _tubes[idx];
            if (tube.tubeObj == null) return;
            var coverObj = new GameObject($"Cover_{idx}");
            coverObj.transform.SetParent(tube.tubeObj.transform);
            coverObj.transform.localPosition = new Vector3(0f, 0.5f, -0.1f);
            var csr = coverObj.AddComponent<SpriteRenderer>();
            csr.sprite = _spriteLid;
            csr.sortingOrder = 2;
            coverObj.transform.localScale = new Vector3(1f, 0.15f, 1f);
            tube.coverObj = coverObj;
        }

        void ShowError(int tubeIdx)
        {
            StartCoroutine(ErrorFlash(tubeIdx));
            StartCoroutine(CameraShake(0.12f, 0.08f));
        }

        IEnumerator SelectPulse(GameObject obj)
        {
            if (obj == null) yield break;
            float t = 0f;
            Vector3 origScale = obj.transform.localScale;
            while (t < 0.15f)
            {
                if (obj == null) yield break;
                t += Time.deltaTime;
                float ratio = t / 0.15f;
                float s = ratio < 0.5f ? Mathf.Lerp(1f, 1.3f, ratio * 2f) : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                obj.transform.localScale = origScale * s;
                yield return null;
            }
            if (obj != null) obj.transform.localScale = origScale;
        }

        IEnumerator PlacePulse(GameObject obj)
        {
            if (obj == null) yield break;
            var sr = obj.GetComponent<SpriteRenderer>();
            Color origColor = sr != null ? sr.color : Color.white;
            Vector3 origScale = obj.transform.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                if (obj == null) yield break;
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float s = ratio < 0.5f ? Mathf.Lerp(1f, 1.4f, ratio * 2f) : Mathf.Lerp(1.4f, 1f, (ratio - 0.5f) * 2f);
                obj.transform.localScale = origScale * s;
                if (sr != null)
                    sr.color = Color.Lerp(origColor, Color.green, Mathf.Sin(ratio * Mathf.PI));
                yield return null;
            }
            if (obj != null) obj.transform.localScale = origScale;
            if (sr != null) sr.color = origColor;
        }

        IEnumerator ErrorFlash(int tubeIdx)
        {
            if (tubeIdx >= _tubes.Count) yield break;
            var tube = _tubes[tubeIdx];
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                Color flash = Color.Lerp(Color.white, new Color(1f, 0.2f, 0.2f), Mathf.Sin(ratio * Mathf.PI));
                foreach (var bObj in tube.ballObjs)
                {
                    if (bObj == null) continue;
                    var sr = bObj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = flash;
                }
                yield return null;
            }
            RefreshBallVisuals(tubeIdx, GetTubeVisualHeight());
        }

        IEnumerator CameraShake(float duration, float magnitude)
        {
            var cam = Camera.main;
            Vector3 origPos = cam.transform.position;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                cam.transform.position = origPos + new Vector3(
                    Random.Range(-magnitude, magnitude),
                    Random.Range(-magnitude, magnitude),
                    0f);
                yield return null;
            }
            cam.transform.position = origPos;
        }

        IEnumerator ClearEffect()
        {
            SetActive(false);
            for (int i = 0; i < _tubes.Count; i++)
            {
                var tube = _tubes[i];
                if (tube.balls.Count == 0) continue;
                foreach (var bObj in tube.ballObjs)
                {
                    if (bObj == null) continue;
                    StartCoroutine(SelectPulse(bObj));
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield return new WaitForSeconds(0.3f);
            _gameManager.OnStageClear(_minMoves);
        }

        void OnDestroy()
        {
            ClearTubes();
        }
    }
}
