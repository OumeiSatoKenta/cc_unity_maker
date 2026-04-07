using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Game059v2_WaterJug
{
    public class WaterJugGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] WaterJugUI _ui;
        [SerializeField] Transform _jugContainer;

        [SerializeField] Sprite _jugSprite;
        [SerializeField] Sprite _waterFillSprite;

        public enum GameState { Idle, Playing, StageClear, GameClear, GameOver }
        enum InputMode { Normal, FaucetSelected, DrainSelected }

        GameState _state = GameState.Idle;
        InputMode _inputMode = InputMode.Normal;

        JugController[] _jugs;
        JugController _selectedJug;

        // Stage data per stage index (0-4)
        static readonly int[] JugCounts = { 2, 2, 3, 3, 4 };
        static readonly int[][] Capacities = {
            new int[]{3,5}, new int[]{3,7}, new int[]{3,5,8}, new int[]{4,7,10}, new int[]{3,5,7,11}
        };
        static readonly float[][] InitialAmounts = {
            new float[]{0,0}, new float[]{0,0}, new float[]{0,0,0}, new float[]{2,0,0}, new float[]{0,0,0,0}
        };
        static readonly int[][] TargetJugIndices = {
            new int[]{1}, new int[]{1}, new int[]{2}, new int[]{0}, new int[]{0,1}
        };
        static readonly int[][] TargetAmounts = {
            new int[]{4}, new int[]{5}, new int[]{6}, new int[]{3}, new int[]{4,6}
        };
        static readonly int[] MaxMoves = { 10, 8, 10, 12, 15 };
        static readonly int[] MinMoves = { 6, 6, 7, 8, 10 };

        int _currentStageIndex;
        int _moveCount;
        int _undoCount;
        int _score;
        int _totalScore;
        int _comboMultiplier;

        // Undo stack: snapshot of each jug's amount
        struct UndoState { public float[] amounts; }
        Stack<UndoState> _undoStack = new Stack<UndoState>();

        bool _isActive;

        public GameState State => _state;

        void Start()
        {
            _ui.Init(this);
            _instructionPanel.Show(
                "059",
                "WaterJug",
                "ジャグを傾けて指定量の水を正確に計ろう！",
                "ジャグをタップして注ぎ元→注ぎ先を選択。蛇口で満杯、排水口で空にできる",
                "目標ジャグにぴったりの量の水を入れてステージクリア！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }

        void StartGame()
        {
            _comboMultiplier = 1;
            _totalScore = 0;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _state = GameState.Playing;
            _isActive = true;
            _currentStageIndex = stage;
            _moveCount = 0;
            _undoCount = 0;
            _score = 0;
            _selectedJug = null;
            _inputMode = InputMode.Normal;
            _undoStack.Clear();

            BuildJugs(stage);
            _ui.OnStageChanged(stage + 1, MaxMoves[stage], TargetAmounts[_currentStageIndex], TargetJugIndices[_currentStageIndex]);
            _ui.UpdateMoves(_moveCount, MaxMoves[stage]);
            _ui.UpdateScore(_score);
            _ui.SetInputMode("normal");
        }

        void OnAllStagesCleared()
        {
            _state = GameState.GameClear;
            _isActive = false;
            _ui.ShowGameClear(_totalScore);
        }

        void BuildJugs(int stageIndex)
        {
            // Destroy existing jugs
            if (_jugs != null)
                foreach (var j in _jugs) if (j != null) Destroy(j.gameObject);

            int count = JugCounts[stageIndex];
            _jugs = new JugController[count];

            float camSize = Camera.main.orthographicSize;
            float camWidth = camSize * Camera.main.aspect;
            float bottomMargin = 3.2f;
            float jugY = -camSize + bottomMargin + 0.8f;
            float totalWidth = camWidth * 1.6f;
            float jugSpacing = count > 1 ? totalWidth / (count - 1) : 0f;
            float startX = count > 1 ? -totalWidth / 2f : 0f;

            var targetSet = new HashSet<int>();
            foreach (var ti in TargetJugIndices[stageIndex]) targetSet.Add(ti);

            for (int i = 0; i < count; i++)
            {
                var jugObj = new GameObject($"Jug{i}");
                jugObj.transform.SetParent(_jugContainer);
                float x = count > 1 ? startX + i * jugSpacing : 0f;
                jugObj.transform.position = new Vector3(x, jugY, 0);

                // Sprite renderer for jug body
                var jugSR = new GameObject("JugBody").AddComponent<SpriteRenderer>();
                jugSR.transform.SetParent(jugObj.transform);
                jugSR.transform.localPosition = Vector3.zero;
                jugSR.sortingOrder = 5;
                float jugScale = Mathf.Clamp(totalWidth / count / 2.2f, 0.35f, 0.8f);
                jugSR.transform.localScale = new Vector3(jugScale, jugScale * 1.5f, 1f);

                // Water fill renderer (child, masked by jug shape)
                var waterSR = new GameObject("WaterFill").AddComponent<SpriteRenderer>();
                waterSR.transform.SetParent(jugObj.transform);
                waterSR.transform.localPosition = new Vector3(0, 0, -0.01f);
                waterSR.sortingOrder = 6;
                waterSR.transform.localScale = new Vector3(jugScale * 0.72f, jugScale * 1.5f, 1f);

                // Amount text (above jug)
                var amtObj = new GameObject("AmountText");
                amtObj.transform.SetParent(jugObj.transform);
                amtObj.transform.localPosition = new Vector3(0, jugScale * 1.0f, -0.1f);
                var amtTMP = amtObj.AddComponent<TextMeshPro>();
                amtTMP.fontSize = 2.8f;
                amtTMP.color = Color.white;
                amtTMP.alignment = TMPro.TextAlignmentOptions.Center;

                // Capacity text (below amount)
                var capObj = new GameObject("CapacityText");
                capObj.transform.SetParent(jugObj.transform);
                capObj.transform.localPosition = new Vector3(0, jugScale * 0.7f, -0.1f);
                var capTMP = capObj.AddComponent<TextMeshPro>();
                capTMP.fontSize = 2.2f;
                capTMP.color = new Color(0.8f, 0.9f, 0.8f, 1f);
                capTMP.alignment = TMPro.TextAlignmentOptions.Center;

                // Target text (above jug, if target)
                var tgtObj = new GameObject("TargetText");
                tgtObj.transform.SetParent(jugObj.transform);
                tgtObj.transform.localPosition = new Vector3(0, jugScale * 1.5f, -0.1f);
                var tgtTMP = tgtObj.AddComponent<TextMeshPro>();
                tgtTMP.fontSize = 2.4f;
                tgtTMP.alignment = TMPro.TextAlignmentOptions.Center;

                // Collider
                var col = jugObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(jugScale * 1.6f, jugScale * 2.0f);

                var jc = jugObj.AddComponent<JugController>();
                // Wire serialized fields via reflection-like approach — use SetupJug
                var so = new UnityEditor.SerializedObject(jc);
                // We can't use SerializedObject at runtime; use public setters instead
                // JugController uses serialized fields set in SceneSetup; at runtime we assign via component refs
                // Since we're building at runtime, assign via the component's children
                AssignJugControllerFields(jc, jugSR, waterSR, amtTMP, capTMP, tgtTMP, col);

                bool isTarget = targetSet.Contains(i);
                int targetAmt = 0;
                if (isTarget)
                {
                    for (int ti = 0; ti < TargetJugIndices[stageIndex].Length; ti++)
                        if (TargetJugIndices[stageIndex][ti] == i) { targetAmt = TargetAmounts[stageIndex][ti]; break; }
                }

                jc.SetupJug(Capacities[stageIndex][i], InitialAmounts[stageIndex][i], isTarget, targetAmt, _jugSprite, _waterFillSprite);
                _jugs[i] = jc;
            }
        }

        void AssignJugControllerFields(JugController jc, SpriteRenderer jugSR, SpriteRenderer waterSR,
            TextMeshPro amtTMP, TextMeshPro capTMP, TextMeshPro tgtTMP, BoxCollider2D col)
        {
            // Use reflection to assign serialized private fields at runtime
            var t = typeof(JugController);
            t.GetField("_jugRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(jc, jugSR);
            t.GetField("_waterRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(jc, waterSR);
            t.GetField("_amountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(jc, amtTMP);
            t.GetField("_capacityText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(jc, capTMP);
            t.GetField("_targetText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(jc, tgtTMP);
            t.GetField("_col", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(jc, col);
        }

        void Update()
        {
            if (!_isActive) return;
            if (_state != GameState.Playing) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                HandleTap(worldPos);
            }
        }

        void HandleTap(Vector2 worldPos)
        {
            var hit = Physics2D.OverlapPoint(worldPos);
            JugController tappedJug = hit != null ? hit.GetComponent<JugController>() : null;

            if (_inputMode == InputMode.FaucetSelected)
            {
                if (tappedJug != null)
                {
                    PushUndoState();
                    tappedJug.AddWater(tappedJug.Capacity - tappedJug.CurrentAmount);
                    _moveCount++;
                    _ui.UpdateMoves(_moveCount, MaxMoves[_currentStageIndex]);
                    CheckAllTargets();
                    CheckMovesExceeded();
                }
                _inputMode = InputMode.Normal;
                _ui.SetInputMode("normal");
                return;
            }

            if (_inputMode == InputMode.DrainSelected)
            {
                if (tappedJug != null)
                {
                    PushUndoState();
                    tappedJug.RemoveWater(tappedJug.CurrentAmount);
                    _moveCount++;
                    _ui.UpdateMoves(_moveCount, MaxMoves[_currentStageIndex]);
                }
                _inputMode = InputMode.Normal;
                _ui.SetInputMode("normal");
                DeselectJug();
                return;
            }

            // Normal mode
            if (tappedJug != null)
            {
                if (_selectedJug == null)
                {
                    // Select source
                    if (tappedJug.IsEmpty) { tappedJug.PlayErrorFlash(); return; }
                    _selectedJug = tappedJug;
                    tappedJug.SetHighlight(true);
                }
                else if (tappedJug == _selectedJug)
                {
                    // Deselect
                    DeselectJug();
                }
                else
                {
                    // Pour from selected to tapped
                    TryPour(_selectedJug, tappedJug);
                    DeselectJug();
                }
            }
            else
            {
                DeselectJug();
            }
        }

        void TryPour(JugController from, JugController to)
        {
            if (to.IsFull) { to.PlayErrorFlash(); return; }
            float available = from.CurrentAmount;
            float canReceive = to.Capacity - to.CurrentAmount;
            float amount = Mathf.Min(available, canReceive);
            if (amount <= 0f) return;

            PushUndoState();
            from.RemoveWater(amount);
            to.AddWater(amount);
            _moveCount++;
            _ui.UpdateMoves(_moveCount, MaxMoves[_currentStageIndex]);

            CheckAllTargets();
            CheckMovesExceeded();
        }

        void CheckAllTargets()
        {
            bool allAchieved = true;
            foreach (var j in _jugs)
            {
                if (j.IsTarget)
                {
                    if (j.CheckTargetAchieved())
                        j.PlayAchieveAnimation();
                    else
                        allAchieved = false;
                }
            }

            // For stages with multiple targets, check all
            bool hasTarget = false;
            foreach (var j in _jugs) if (j.IsTarget) { hasTarget = true; break; }

            if (hasTarget && allAchieved)
                StartCoroutine(StageClearCoroutine());
        }

        IEnumerator StageClearCoroutine()
        {
            _state = GameState.StageClear;
            _isActive = false;
            yield return new WaitForSeconds(0.5f);

            int baseScore = (MaxMoves[_currentStageIndex] - _moveCount) * 100;
            baseScore = Mathf.Max(baseScore, 50);

            float optimalBonus = (_moveCount <= MinMoves[_currentStageIndex]) ? 3.0f : 1.0f;
            float noUndoBonus = (_undoCount == 0) ? 1.5f : 1.0f;
            float combo = 1.0f + (_comboMultiplier - 1) * 0.1f;
            combo = Mathf.Clamp(combo, 1.0f, 1.5f);

            _score = Mathf.RoundToInt(baseScore * optimalBonus * noUndoBonus * combo);
            _totalScore += _score;
            _comboMultiplier++;
            _ui.UpdateScore(_score);
            _ui.ShowStageClear(_score);
        }

        void CheckMovesExceeded()
        {
            if (_moveCount >= MaxMoves[_currentStageIndex])
            {
                // If we reach max moves without clearing, game over
                if (_state == GameState.Playing)
                {
                    _state = GameState.GameOver;
                    _isActive = false;
                    StartCoroutine(CameraShake());
                    _ui.ShowGameOver(_score);
                }
            }
        }

        void PushUndoState()
        {
            if (_jugs == null) return;
            float[] snapshot = new float[_jugs.Length];
            for (int i = 0; i < _jugs.Length; i++)
                snapshot[i] = _jugs[i].CurrentAmount;
            _undoStack.Push(new UndoState { amounts = snapshot });
        }

        void DeselectJug()
        {
            if (_selectedJug != null)
            {
                _selectedJug.SetHighlight(false);
                _selectedJug = null;
            }
        }

        public void OnFaucetSelected()
        {
            if (_state != GameState.Playing) return;
            DeselectJug();
            _inputMode = InputMode.FaucetSelected;
            _ui.SetInputMode("faucet");
        }

        public void OnDrainSelected()
        {
            if (_state != GameState.Playing) return;
            DeselectJug();
            _inputMode = InputMode.DrainSelected;
            _ui.SetInputMode("drain");
        }

        public void OnUndo()
        {
            if (_state != GameState.Playing) return;
            if (_undoStack.Count == 0) return;
            var state = _undoStack.Pop();
            for (int i = 0; i < _jugs.Length; i++)
                _jugs[i].SetAmount(state.amounts[i]);
            _undoCount++;
            if (_moveCount > 0) _moveCount--;
            _ui.UpdateMoves(_moveCount, MaxMoves[_currentStageIndex]);
            DeselectJug();
            _inputMode = InputMode.Normal;
            _ui.SetInputMode("normal");
        }

        public void OnNextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void OnBackToMenu()
        {
            SceneLoader.BackToMenu();
        }

        IEnumerator CameraShake()
        {
            if (Camera.main == null) yield break;
            var cam = Camera.main.transform;
            Vector3 origin = cam.position;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                cam.position = origin + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f), 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.position = origin;
        }
    }
}
