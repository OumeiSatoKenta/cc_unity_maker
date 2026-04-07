using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game071v2_BeatTiles
{
    public enum NoteType { Normal, Hold, Rapid }

    public class NoteManager : MonoBehaviour
    {
        [SerializeField] BeatTilesGameManager _gameManager;
        [SerializeField] Sprite _noteNormalSprite;
        [SerializeField] Sprite _noteHoldSprite;
        [SerializeField] Sprite _noteRapidSprite;
        [SerializeField] Sprite _holdBodySprite;
        [SerializeField] Sprite _laneBgSprite;

        // Stage params
        float _bpm = 90f;
        int _laneCount = 2;
        bool _enableSimultaneous = false;
        bool _enableHold = false;
        bool _enableRapid = false;
        bool _enableOffbeat = false;
        float _noteDensity = 1.0f;

        // Game state
        bool _isActive = false;
        int _score = 0;
        int _combo = 0;
        float _life = 100f;
        const float MaxLife = 100f;
        int _stageIndex = 0;

        // Layout
        float _judgeLineY;
        float _spawnY;
        float[] _laneXPositions;
        float _laneWidth;
        float _fallSpeed; // units per second

        // Notes
        List<NoteObject> _activeNotes = new List<NoteObject>();
        List<GameObject> _noteObjects = new List<GameObject>();

        // Beat timer
        float _beatInterval;
        float _beatTimer;
        int _beatCount;
        int _totalBeats = 32;
        int _beatsProcessed;

        // Judgment windows (seconds)
        const float PerfectWindow = 0.030f;
        const float GreatWindow = 0.080f;
        const float GoodWindow = 0.150f;

        // Camera shake
        Camera _mainCam;
        Vector3 _camOrigin;
        bool _isShaking;

        public int TotalScore => _score;

        class NoteObject
        {
            public GameObject go;
            public SpriteRenderer sr;
            public int lane;
            public NoteType type;
            public float spawnTime;
            public float targetTime; // when it should be at judge line
            public bool judged;
            public bool isHolding;
            public float holdDuration; // for Hold notes
        }

        void Awake()
        {
            _mainCam = Camera.main;
            if (_mainCam == null) Debug.LogError("[NoteManager] Main Camera not found");
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _isActive = false;

            // Clear old notes
            foreach (var n in _noteObjects)
                if (n != null) Destroy(n);
            _noteObjects.Clear();
            _activeNotes.Clear();

            // BPM based on stage
            float[] bpms = { 90f, 110f, 130f, 150f, 170f };
            _bpm = bpms[Mathf.Clamp(stageIndex, 0, 4)] * config.speedMultiplier;
            _beatInterval = 60f / _bpm;

            // Lane count
            _laneCount = stageIndex == 0 ? 2 : 4;

            // Feature unlocks
            _enableSimultaneous = stageIndex >= 1;
            _enableHold = stageIndex >= 2;
            _enableRapid = stageIndex >= 3;
            _enableOffbeat = stageIndex >= 4;

            _noteDensity = 0.5f + stageIndex * 0.15f * config.countMultiplier;

            // Layout
            float camSize = _mainCam.orthographicSize;
            float camWidth = camSize * _mainCam.aspect;
            float bottomMargin = 2.8f;
            float topMargin = 1.2f;

            _judgeLineY = -camSize + bottomMargin;
            _spawnY = camSize - topMargin;
            _fallSpeed = (_spawnY - _judgeLineY) / (_beatInterval * 3.5f); // notes take ~3.5 beats to fall

            // Lane positions (center-aligned)
            float totalLaneWidth = camWidth * 1.7f;
            _laneWidth = totalLaneWidth / 4f; // always divide by 4 for consistent width
            _laneXPositions = new float[4];
            float startX = -totalLaneWidth / 2f + _laneWidth / 2f;
            for (int i = 0; i < 4; i++)
                _laneXPositions[i] = startX + i * _laneWidth;

            // Reset state
            _score = 0;
            _combo = 0;
            _life = MaxLife;
            _beatTimer = 0f;
            _beatCount = 0;
            _beatsProcessed = 0;
            _totalBeats = 24 + stageIndex * 8;

            _gameManager.UpdateScoreDisplay(_score);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateLifeDisplay(_life / MaxLife);

            _isActive = true;
        }

        public void SetActive(bool active) => _isActive = active;

        void Update()
        {
            if (!_isActive) return;

            _beatTimer += Time.deltaTime;

            // Beat: spawn notes
            if (_beatTimer >= _beatInterval)
            {
                _beatTimer -= _beatInterval;
                SpawnNotesOnBeat(_beatCount);
                _beatCount++;
                _beatsProcessed++;

                // Check stage complete
                if (_beatsProcessed >= _totalBeats && _activeNotes.Count == 0)
                    OnStageComplete();
            }

            // Move notes
            MoveNotes();

            // Handle input
            HandleInput();

            // Auto-miss notes past judge line
            AutoMissNotes();

            // Camera shake
            if (_isShaking) return;
        }

        void SpawnNotesOnBeat(int beat)
        {
            if (_beatsProcessed >= _totalBeats) return;

            // Decide which lanes to spawn in
            bool shouldSpawn = Random.value < _noteDensity;
            if (!shouldSpawn && beat % 2 != 0) return;

            int maxLane = _laneCount;
            int lane = Random.Range(0, maxLane);

            NoteType type = NoteType.Normal;
            float rand = Random.value;
            if (_enableRapid && rand < 0.15f * Mathf.Min(1f, _stageIndex - 2f))
                type = NoteType.Rapid;
            else if (_enableHold && rand < 0.25f)
                type = NoteType.Hold;

            SpawnNote(lane, type);

            // Simultaneous note
            if (_enableSimultaneous && Random.value < 0.3f + _stageIndex * 0.05f)
            {
                int lane2 = (lane + 2) % maxLane;
                if (lane2 != lane) SpawnNote(lane2, NoteType.Normal);
            }

            // Offbeat (backbeat)
            if (_enableOffbeat && beat % 4 == 2 && Random.value < 0.4f)
            {
                int offLane = Random.Range(0, maxLane);
                if (offLane != lane) SpawnNote(offLane, NoteType.Normal);
            }
        }

        void SpawnNote(int lane, NoteType type)
        {
            if (lane >= _laneCount) lane = _laneCount - 1;

            var go = new GameObject($"Note_{lane}_{type}_{_beatCount}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = type == NoteType.Hold ? _noteHoldSprite
                      : type == NoteType.Rapid ? _noteRapidSprite
                      : _noteNormalSprite;
            sr.sortingOrder = 5;

            float noteHeight = 0.35f;
            float noteWidth = _laneWidth * 0.88f;
            go.transform.localScale = new Vector3(noteWidth / 2.56f, noteHeight / 0.64f, 1f);
            go.transform.position = new Vector3(_laneXPositions[lane], _spawnY, 0f);

            float holdDur = type == NoteType.Hold ? _beatInterval * 1.5f : 0f;

            var note = new NoteObject
            {
                go = go,
                sr = sr,
                lane = lane,
                type = type,
                spawnTime = Time.time,
                targetTime = Time.time + (_spawnY - _judgeLineY) / _fallSpeed,
                judged = false,
                holdDuration = holdDur
            };
            _activeNotes.Add(note);
            _noteObjects.Add(go);
        }

        void MoveNotes()
        {
            foreach (var note in _activeNotes)
            {
                if (note.judged) continue;
                var pos = note.go.transform.position;
                pos.y -= _fallSpeed * Time.deltaTime;
                note.go.transform.position = pos;
            }
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            bool pressed = mouse.leftButton.wasPressedThisFrame;
            bool held = mouse.leftButton.isPressed;

            if (!pressed && !held) return;

            Vector2 screenPos = mouse.position.ReadValue();
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));

            // Find which lane was tapped
            int tappedLane = GetLaneAtWorldX(worldPos.x);
            if (tappedLane < 0 || tappedLane >= _laneCount) return;

            if (pressed)
                TryJudgeNote(tappedLane);
        }

        int GetLaneAtWorldX(float worldX)
        {
            float camWidth = _mainCam.orthographicSize * _mainCam.aspect;
            float totalLaneWidth = camWidth * 1.7f;
            float laneW = totalLaneWidth / 4f;
            float startX = -totalLaneWidth / 2f;

            for (int i = 0; i < 4; i++)
            {
                float lx = startX + i * laneW;
                float rx = lx + laneW;
                if (worldX >= lx && worldX < rx) return i;
            }
            return -1;
        }

        void TryJudgeNote(int lane)
        {
            float now = Time.time;
            NoteObject bestNote = null;
            float bestDelta = float.MaxValue;

            foreach (var note in _activeNotes)
            {
                if (note.judged || note.lane != lane) continue;
                float delta = Mathf.Abs(now - note.targetTime);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    bestNote = note;
                }
            }

            if (bestNote == null) return;

            string judgement;
            Color color;
            int baseScore;

            if (bestDelta <= PerfectWindow)
            {
                judgement = "PERFECT";
                color = new Color(0f, 1f, 1f);
                baseScore = 100;
                _life = Mathf.Min(MaxLife, _life + 1f);
            }
            else if (bestDelta <= GreatWindow)
            {
                judgement = "GREAT";
                color = new Color(0.5f, 1f, 0.5f);
                baseScore = 70;
                _life = Mathf.Min(MaxLife, _life + 0.5f);
            }
            else if (bestDelta <= GoodWindow)
            {
                judgement = "GOOD";
                color = new Color(1f, 1f, 0.3f);
                baseScore = 30;
            }
            else
            {
                judgement = "MISS";
                color = new Color(1f, 0.3f, 0.3f);
                baseScore = 0;
                RegisterMiss(bestNote);
                return;
            }

            _combo++;
            float multiplier = Mathf.Min(3.0f, 1.0f + _combo * 0.1f);
            _score += Mathf.RoundToInt(baseScore * multiplier);

            bestNote.judged = true;
            StartCoroutine(PopNote(bestNote));

            _gameManager.UpdateScoreDisplay(_score);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateLifeDisplay(_life / MaxLife);
            _gameManager.ShowJudgement(judgement, color);
        }

        void RegisterMiss(NoteObject note)
        {
            note.judged = true;
            _combo = 0;
            _life -= 8f;

            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateLifeDisplay(_life / MaxLife);
            _gameManager.ShowJudgement("MISS", new Color(1f, 0.3f, 0.3f));

            StartCoroutine(ShakeCamera());
            note.sr.color = new Color(1f, 0.3f, 0.3f);

            if (_life <= 0f)
            {
                _isActive = false;
                _gameManager.OnGameOver();
            }
        }

        void AutoMissNotes()
        {
            float now = Time.time;
            // Collect to-miss separately to avoid modifying list during iteration
            var toMiss = new System.Collections.Generic.List<NoteObject>();
            foreach (var note in _activeNotes)
            {
                if (note.judged) continue;
                if (now - note.targetTime > GoodWindow + 0.05f)
                    toMiss.Add(note);
            }
            foreach (var note in toMiss)
                RegisterMiss(note);

            // Remove judged notes below screen
            _activeNotes.RemoveAll(n =>
            {
                if (n.judged && n.go.transform.position.y < _judgeLineY - 1f)
                {
                    Destroy(n.go);
                    return true;
                }
                return false;
            });

            // Check complete
            if (_beatsProcessed >= _totalBeats && _activeNotes.Count == 0 && _isActive)
                OnStageComplete();
        }

        void OnStageComplete()
        {
            if (!_isActive) return;
            _isActive = false;
            _gameManager.OnStageClear();
        }

        IEnumerator PopNote(NoteObject note)
        {
            if (note.go == null) yield break;
            Vector3 origScale = note.go.transform.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float scale = ratio < 0.5f ? 1f + 0.3f * (ratio / 0.5f) : 1.3f - 0.3f * ((ratio - 0.5f) / 0.5f);
                if (note.go != null)
                    note.go.transform.localScale = origScale * scale;
                yield return null;
            }
            if (note.go != null)
            {
                note.go.transform.localScale = origScale;
                note.sr.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        IEnumerator ShakeCamera()
        {
            if (_isShaking) yield break;
            _isShaking = true;
            _camOrigin = _mainCam.transform.position;
            float duration = 0.15f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float strength = 0.1f * (1f - t / duration);
                _mainCam.transform.position = _camOrigin + new Vector3(
                    Random.Range(-strength, strength),
                    Random.Range(-strength, strength), 0f);
                yield return null;
            }
            _mainCam.transform.position = _camOrigin;
            _isShaking = false;
        }

        void OnDestroy()
        {
            foreach (var go in _noteObjects)
                if (go != null) Destroy(go);
        }
    }
}
