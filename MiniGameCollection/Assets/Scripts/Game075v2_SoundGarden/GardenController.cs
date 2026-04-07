using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game075v2_SoundGarden
{
    public class GardenController : MonoBehaviour
    {
        [SerializeField] Sprite _spriteLv0;
        [SerializeField] Sprite _spriteLv1;
        [SerializeField] Sprite _spriteLv2;
        [SerializeField] Sprite _spritePest;
        [SerializeField] SoundGardenGameManager _gameManager;

        // Stage params
        int _bpm = 80;
        int _plantCount = 2;
        float _timeLimit = 60f;
        bool _enableSimultaneous;
        bool _enablePest;
        bool _enableBpmChange;
        bool _enableResonance;

        float _camSize;
        float _camWidth;
        bool _isActive;
        float _beatInterval;
        float _beatTimer;
        float _timeRemaining;
        int _currentBeatPlant; // for alternating beat
        int _stageIndex;

        int _totalScore;
        int _combo;

        // Beat timing
        float _beatPerfectWindow = 0.12f;
        float _beatGreatWindow = 0.25f;
        float _beatGoodWindow = 0.45f;
        float _lastBeatTime;

        List<Plant> _plants = new List<Plant>();
        List<GameObject> _pests = new List<GameObject>();
        Dictionary<GameObject, Coroutine> _pestRoutines = new Dictionary<GameObject, Coroutine>();
        List<int> _litPlantIndices = new List<int>();

        // BPM change state (stage 4+)
        float _bpmChangeTimer;
        float _bpmChangeDuration = 4f;
        float _bpmChangeInterval = 8f;
        bool _inBpmChange;
        int _originalBpm;

        public int TotalScore => _totalScore;
        public float TimeRemaining => _timeRemaining;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            ClearPests();
            ClearPlants();

            _isActive = false;
            _stageIndex = stageIndex;
            _combo = 0;

            ApplyStageParams(stageIndex, config);

            _camSize = Camera.main.orthographicSize;
            _camWidth = _camSize * Camera.main.aspect;

            SpawnPlants();

            _beatInterval = 60f / _bpm;
            _beatTimer = 0f;
            _timeRemaining = _timeLimit;
            _currentBeatPlant = 0;
            _lastBeatTime = Time.time;
            _bpmChangeTimer = 0f;
            _inBpmChange = false;
            _originalBpm = _bpm;

            _gameManager.UpdateScoreDisplay(_totalScore);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateTimerDisplay(_timeRemaining);

            _isActive = true;
        }

        void ApplyStageParams(int idx, StageManager.StageConfig config)
        {
            // BPM and plant counts per stage
            int[] bpms = { 80, 100, 120, 140, 160 };
            int[] counts = { 2, 3, 4, 5, 6 };
            float[] limits = { 60f, 55f, 50f, 45f, 40f };

            _bpm = bpms[Mathf.Clamp(idx, 0, 4)];
            _plantCount = counts[Mathf.Clamp(idx, 0, 4)];
            _timeLimit = limits[Mathf.Clamp(idx, 0, 4)];

            _enableSimultaneous = idx >= 1;
            _enablePest = idx >= 2;
            _enableBpmChange = idx >= 3;
            _enableResonance = idx >= 4;

            // Apply speedMultiplier if set (overrides default BPM scaling)
            _bpm = Mathf.RoundToInt(_bpm * config.speedMultiplier);
        }

        void SpawnPlants()
        {
            float topMargin = 1.5f;
            float bottomMargin = 3.0f;
            float availableHeight = (_camSize * 2f) - topMargin - bottomMargin;
            float availableWidth = _camWidth * 2f - 1.0f;

            int rows, cols;
            GetGridLayout(_plantCount, out rows, out cols);

            float cellH = availableHeight / rows;
            float cellW = availableWidth / cols;
            float cellSize = Mathf.Min(cellH, cellW, 1.8f);

            float startY = _camSize - topMargin - cellH * 0.5f;
            float startX = -_camWidth + 0.5f + cellW * 0.5f;

            int plantIdx = 0;
            for (int r = 0; r < rows && plantIdx < _plantCount; r++)
            {
                int colsThisRow = GetColsForRow(r, rows, _plantCount, cols);
                float rowStartX = -(colsThisRow - 1) * cellW * 0.5f;
                for (int c = 0; c < colsThisRow && plantIdx < _plantCount; c++)
                {
                    float px = rowStartX + c * cellW;
                    float py = startY - r * cellH;

                    var go = new GameObject($"Plant_{plantIdx}");
                    go.transform.SetParent(transform);
                    go.transform.position = new Vector3(px, py, 0f);
                    go.transform.localScale = Vector3.one * cellSize;

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = _spriteLv0;
                    sr.sortingOrder = 2;

                    var col = go.AddComponent<CircleCollider2D>();
                    col.radius = 0.45f;

                    var plant = go.AddComponent<Plant>();
                    plant.Initialize(plantIdx, _spriteLv0, _spriteLv1, _spriteLv2);

                    _plants.Add(plant);
                    plantIdx++;
                }
            }
        }

        void GetGridLayout(int count, out int rows, out int cols)
        {
            if (count <= 3) { rows = 1; cols = count; }
            else if (count == 4) { rows = 2; cols = 2; }
            else if (count == 5) { rows = 2; cols = 3; }
            else { rows = 2; cols = 3; }
        }

        int GetColsForRow(int row, int totalRows, int totalPlants, int maxCols)
        {
            if (totalRows == 1) return totalPlants;
            if (totalPlants == 5)
                return row == 0 ? 2 : 3;
            return maxCols;
        }

        void ClearPlants()
        {
            foreach (var p in _plants)
            {
                if (p != null) Destroy(p.gameObject);
            }
            _plants.Clear();
            _litPlantIndices.Clear();
        }

        void ClearPests()
        {
            foreach (var kvp in _pestRoutines)
                StopCoroutine(kvp.Value);
            _pestRoutines.Clear();
            foreach (var p in _pests)
            {
                if (p != null) Destroy(p);
            }
            _pests.Clear();
        }

        void Update()
        {
            if (!_isActive) return;

            // Timer
            _timeRemaining -= Time.deltaTime;
            _gameManager.UpdateTimerDisplay(Mathf.Max(0f, _timeRemaining));

            if (_timeRemaining <= 0f)
            {
                _isActive = false;
                CheckTimeUpResult();
                return;
            }

            // BPM change (stage 4+)
            if (_enableBpmChange)
                HandleBpmChange();

            // Beat tick
            _beatTimer += Time.deltaTime;
            if (_beatTimer >= _beatInterval)
            {
                _beatTimer -= _beatInterval;
                _lastBeatTime = Time.time;
                TriggerBeat();
            }

            // Pest spawn
            if (_enablePest && _pests.Count < 1)
            {
                _beatTimer += 0f; // pest spawn controlled by coroutine
            }

            HandleTap();
        }

        void HandleBpmChange()
        {
            _bpmChangeTimer += Time.deltaTime;
            if (!_inBpmChange && _bpmChangeTimer >= _bpmChangeInterval)
            {
                _inBpmChange = true;
                _bpmChangeTimer = 0f;
                // Speed up BPM temporarily
                _bpm = Mathf.RoundToInt(_originalBpm * 1.3f);
                _beatInterval = 60f / _bpm;
                _gameManager.ShowJudgement("季節の嵐！", new Color(1f, 0.5f, 0f));
                StartCoroutine(ResetBpmAfter(_bpmChangeDuration));
            }
        }

        IEnumerator ResetBpmAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            _bpm = _originalBpm;
            _beatInterval = 60f / _bpm;
            _inBpmChange = false;
            _bpmChangeTimer = 0f;
        }

        void TriggerBeat()
        {
            // Unlit all previous
            foreach (var p in _plants)
                p.SetLit(false);
            _litPlantIndices.Clear();

            if (_plants.Count == 0) return;

            // Lit current beat plant
            List<int> toLit = new List<int>();

            if (_enableSimultaneous && Random.value < 0.3f && _plants.Count >= 2)
            {
                // Simultaneous: 2 plants
                int a = Random.Range(0, _plants.Count);
                int b;
                do { b = Random.Range(0, _plants.Count); } while (b == a);
                toLit.Add(a);
                toLit.Add(b);
            }
            else
            {
                // Alternate through plants
                toLit.Add(_currentBeatPlant % _plants.Count);
                _currentBeatPlant++;
            }

            foreach (int idx in toLit)
            {
                if (idx < _plants.Count && !_plants[idx].IsCompleted)
                {
                    _plants[idx].SetLit(true);
                    _litPlantIndices.Add(idx);
                }
            }

            // Pest spawn (stage 3+)
            if (_enablePest && _pests.Count == 0 && Random.value < 0.15f)
                SpawnPest();
        }

        void SpawnPest()
        {
            if (_plants.Count == 0) return;
            int targetPlant = Random.Range(0, _plants.Count);
            var plant = _plants[targetPlant];
            if (plant.IsCompleted) return;

            var pestObj = new GameObject("Pest");
            pestObj.transform.SetParent(transform);
            pestObj.transform.position = plant.transform.position + new Vector3(0.4f, 0.4f, 0f);
            pestObj.transform.localScale = Vector3.one * 0.5f;

            var sr = pestObj.AddComponent<SpriteRenderer>();
            sr.sprite = _spritePest;
            sr.sortingOrder = 5;

            var col = pestObj.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            _pests.Add(pestObj);
            var routine = StartCoroutine(PestDamageRoutine(pestObj, plant));
            _pestRoutines[pestObj] = routine;
        }

        IEnumerator PestDamageRoutine(GameObject pestObj, Plant plant)
        {
            float timer = 0f;
            float pestLife = 4f;
            while (timer < pestLife && pestObj != null && _pests.Contains(pestObj))
            {
                timer += Time.deltaTime;
                // Pulsing effect
                if (pestObj != null)
                {
                    float s = 0.5f + 0.08f * Mathf.Sin(timer * 6f);
                    pestObj.transform.localScale = Vector3.one * s;
                }
                yield return null;
            }
            // If pest wasn't removed, damage plant
            if (pestObj != null && _pests.Contains(pestObj))
            {
                plant.AddGrowth(-30); // Growth penalty
                _pests.Remove(pestObj);
                Destroy(pestObj);
            }
        }

        void HandleTap()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            // Check pest tap
            foreach (var pestObj in new List<GameObject>(_pests))
            {
                if (pestObj == null) continue;
                var col = Physics2D.OverlapPoint(worldPos);
                if (col != null && col.gameObject == pestObj)
                {
                    RemovePest(pestObj);
                    _gameManager.ShowJudgement("害虫除去！", Color.green);
                    return;
                }
            }

            // Check plant tap
            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            Plant tappedPlant = hit.GetComponent<Plant>();
            if (tappedPlant == null) return;
            if (tappedPlant.IsCompleted) return;

            // Timing judgement
            float timeSinceBeat = Time.time - _lastBeatTime;
            // Wrap: also check closeness to next beat
            float timeToNextBeat = _beatInterval - _beatTimer;
            float minTime = Mathf.Min(timeSinceBeat, timeToNextBeat);

            bool isLit = _litPlantIndices.Contains(tappedPlant.PlotIndex);

            if (isLit)
            {
                string judgement;
                int score;
                int growthAmount;
                Color judgementColor;

                if (minTime <= _beatPerfectWindow)
                {
                    judgement = "Perfect!";
                    score = 100;
                    growthAmount = 3;
                    judgementColor = new Color(1f, 0.9f, 0f);
                    _combo++;
                }
                else if (minTime <= _beatGreatWindow)
                {
                    judgement = "Great";
                    score = 60;
                    growthAmount = 2;
                    judgementColor = new Color(0f, 0.9f, 1f);
                    _combo++;
                }
                else if (minTime <= _beatGoodWindow)
                {
                    judgement = "Good";
                    score = 20;
                    growthAmount = 1;
                    judgementColor = Color.green;
                    // Good doesn't increase combo but doesn't reset
                }
                else
                {
                    judgement = "Miss";
                    score = 0;
                    growthAmount = 0;
                    judgementColor = Color.red;
                    _combo = 0;
                    tappedPlant.FlashMiss();
                    _gameManager.ShowJudgement(judgement, judgementColor);
                    _gameManager.UpdateComboDisplay(_combo);
                    return;
                }

                // Combo multiplier
                float multiplier = 1f;
                if (judgement == "Perfect!")
                    multiplier = Mathf.Min(1f + _combo * 0.1f, 3f);
                else if (judgement == "Great")
                    multiplier = Mathf.Min(1f + _combo * 0.05f, 2f);

                // Resonance bonus (stage 5: adjacent simultaneous)
                if (_enableResonance && _litPlantIndices.Count >= 2 && _litPlantIndices.Contains(tappedPlant.PlotIndex))
                    multiplier *= 1.5f;

                int finalScore = Mathf.RoundToInt(score * multiplier);
                _totalScore += finalScore;
                // growthAmount: Perfect=3, Great=2, Good=1 maps to 34/22/11 (ceil division so 3 Perfects = 102 > 100)
                int growthDelta = growthAmount == 3 ? 34 : growthAmount == 2 ? 22 : 11;
                tappedPlant.AddGrowth(growthDelta);
                tappedPlant.PulseSuccess();

                // Unlit tapped plant
                _litPlantIndices.Remove(tappedPlant.PlotIndex);
                tappedPlant.SetLit(false);

                _gameManager.ShowJudgement(judgement, judgementColor);
                _gameManager.UpdateScoreDisplay(_totalScore);
                _gameManager.UpdateComboDisplay(_combo);

                CheckAllCompleted();
            }
            else
            {
                // Tapped plant not lit = Miss
                _combo = 0;
                tappedPlant.FlashMiss();
                _gameManager.ShowJudgement("Miss", Color.red);
                _gameManager.UpdateComboDisplay(_combo);
            }
        }

        void RemovePest(GameObject pestObj)
        {
            if (_pestRoutines.TryGetValue(pestObj, out var routine))
            {
                StopCoroutine(routine);
                _pestRoutines.Remove(pestObj);
            }
            _pests.Remove(pestObj);
            Destroy(pestObj);
        }

        void CheckAllCompleted()
        {
            bool allDone = true;
            foreach (var p in _plants)
            {
                if (!p.IsCompleted) { allDone = false; break; }
            }

            if (allDone)
            {
                _isActive = false;
                // Check full harmony bonus (all completed nearly simultaneously - simplified: all completed)
                // Full harmony bonus is already visual; just trigger clear
                _gameManager.OnStageClear();
            }
        }

        void CheckTimeUpResult()
        {
            int completedCount = 0;
            foreach (var p in _plants)
                if (p.IsCompleted) completedCount++;

            bool halfDone = completedCount >= _plants.Count / 2;
            if (!halfDone || completedCount == 0)
                _gameManager.OnGameOver();
            else
                _gameManager.OnStageClear(); // Partial clear still progresses
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            ClearPlants();
            ClearPests();
        }
    }
}
