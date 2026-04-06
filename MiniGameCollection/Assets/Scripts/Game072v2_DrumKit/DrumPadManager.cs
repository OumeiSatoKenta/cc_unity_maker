using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Common;

namespace Game072v2_DrumKit
{
    public class DrumPadManager : MonoBehaviour
    {
        [SerializeField] DrumKitGameManager _gameManager;
        [SerializeField] Sprite _spriteBass;
        [SerializeField] Sprite _spriteSnare;
        [SerializeField] Sprite _spriteHihat;
        [SerializeField] Sprite _spriteCymbal;
        [SerializeField] Sprite _spriteTomHigh;
        [SerializeField] Sprite _spriteTomLow;
        [SerializeField] Sprite _spriteRingGuide;
        [SerializeField] Sprite _spriteRingJudge;

        // Stage params
        float _bpm = 80f;
        int _padCount = 2;
        bool _enableSimultaneous = false;
        bool _enableFill = false;
        bool _enableSyncopation = false;
        int _totalBeats = 32;

        // Game state
        bool _isActive = false;
        int _score = 0;
        int _combo = 0;
        int _missCount = 0;
        const int MaxMiss = 10;
        int _stageIndex = 0;

        // Beat timer
        float _beatInterval;
        float _beatTimer;
        int _beatCount;

        // Judgment windows (seconds) — ring shrink time × ratio
        const float PerfectWindow = 0.040f;
        const float GreatWindow   = 0.090f;
        const float GoodWindow    = 0.160f;

        // Pad layout
        PadObject[] _pads;
        List<NoteEvent> _activeNotes = new List<NoteEvent>();

        // Camera shake
        Camera _mainCam;
        Vector3 _camOrigin;
        bool _isShaking;

        int _accumulatedScore = 0;
        public int TotalScore => _accumulatedScore + _score;

        struct PadPosition
        {
            public Vector3 worldPos;
            public float radius; // half-size for hit detection
        }

        class PadObject
        {
            public GameObject go;
            public SpriteRenderer padSr;
            public SpriteRenderer ringSr;
            public CircleCollider2D col;
            public int padIndex;
            public Color originalColor;
        }

        class NoteEvent
        {
            public int padIndex;
            public float targetTime;   // when ring should be at judge size
            public float spawnTime;
            public float ringScaleStart; // start scale (large)
            public float ringScaleEnd;   // end scale (small, ~1.0)
            public bool judged;
            public PadObject pad;
        }

        // Pad sprites indexed by padIndex
        Sprite[] _padSprites;
        // Pad names
        static readonly string[] PadNames = { "BassDrum", "Snare", "HiHat", "Cymbal", "TomHigh", "TomLow" };

        void Awake()
        {
            _mainCam = Camera.main;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _isActive = false;

            // Cleanup old notes
            foreach (var n in _activeNotes)
                if (n.pad != null && n.pad.ringSr != null)
                    n.pad.ringSr.enabled = false;
            _activeNotes.Clear();

            // BPM per stage
            float[] bpms = { 80f, 100f, 120f, 140f, 160f };
            _bpm = bpms[Mathf.Clamp(stageIndex, 0, 4)] * config.speedMultiplier;
            _beatInterval = 60f / _bpm;

            // Pad count per stage
            int[] padCounts = { 2, 3, 5, 6, 6 };
            _padCount = padCounts[Mathf.Clamp(stageIndex, 0, 4)];

            _enableSimultaneous = stageIndex >= 3;
            _enableFill        = stageIndex >= 1;
            _enableSyncopation = stageIndex >= 4;

            _totalBeats = 24 + stageIndex * 8;

            // Reset state
            _accumulatedScore += _score;
            _score = 0;
            _combo = 0;
            _missCount = 0;
            _beatTimer = 0f;
            _beatCount = 0;

            _gameManager.UpdateScoreDisplay(_score);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateMissDisplay(_missCount, MaxMiss);

            // Build pad sprites array
            _padSprites = new Sprite[]
            {
                _spriteBass, _spriteSnare, _spriteHihat,
                _spriteCymbal, _spriteTomHigh, _spriteTomLow
            };

            BuildPads();
            _isActive = true;
        }

        void BuildPads()
        {
            // Destroy old pads
            if (_pads != null)
                foreach (var p in _pads)
                    if (p != null && p.go != null) Destroy(p.go);

            _pads = new PadObject[6];

            float camSize = _mainCam.orthographicSize;
            float camWidth = camSize * _mainCam.aspect;
            float bottomMargin = 3.0f;

            float rowLow  = -camSize + bottomMargin + 0.8f;
            float rowHigh = rowLow + 2.6f;

            float colStep = camWidth * 2f / 3f * 0.85f;
            float[] colX = { -colStep, 0f, colStep };
            float[] rowY = { rowLow, rowHigh };

            // Layout: [bass, snare, hihat] (row 0), [cymbal, tomHigh, tomLow] (row 1)
            int[] rowIdx = { 0, 0, 0, 1, 1, 1 };
            int[] colIdx = { 0, 1, 2, 0, 1, 2 };

            float padRadius = Mathf.Min(colStep * 0.42f, 0.9f);

            for (int i = 0; i < 6; i++)
            {
                bool visible = i < _padCount;
                var go = new GameObject(PadNames[i]);
                go.transform.SetParent(transform, false);

                Vector3 pos = new Vector3(colX[colIdx[i]], rowY[rowIdx[i]], 0f);
                go.transform.position = pos;

                // Pad sprite
                var padSr = go.AddComponent<SpriteRenderer>();
                padSr.sprite = _padSprites[i];
                padSr.sortingOrder = 2;
                float sprScale = padRadius * 2f / 2.56f; // sprite is 256px
                go.transform.localScale = new Vector3(sprScale, sprScale, 1f);
                padSr.enabled = visible;

                // CircleCollider for hit detection
                var col = go.AddComponent<CircleCollider2D>();
                col.radius = padRadius / sprScale; // in local space
                col.enabled = visible;

                // Ring child
                var ringGo = new GameObject("Ring");
                ringGo.transform.SetParent(go.transform, false);
                ringGo.transform.localPosition = Vector3.zero;
                var ringSr = ringGo.AddComponent<SpriteRenderer>();
                ringSr.sprite = _spriteRingGuide;
                ringSr.sortingOrder = 3;
                ringSr.enabled = false;

                var pad = new PadObject
                {
                    go = go,
                    padSr = padSr,
                    ringSr = ringSr,
                    col = col,
                    padIndex = i,
                    originalColor = Color.white
                };
                _pads[i] = pad;
            }
        }

        public void SetActive(bool active) => _isActive = active;

        void Update()
        {
            if (!_isActive) return;

            _beatTimer += Time.deltaTime;

            if (_beatTimer >= _beatInterval)
            {
                _beatTimer -= _beatInterval;
                SpawnBeat(_beatCount);
                _beatCount++;
            }

            // Update ring animations
            UpdateRings();

            // Input
            HandleInput();

            // Auto-miss
            AutoMissNotes();

            // Stage complete check
            if (_beatCount >= _totalBeats && _activeNotes.Count == 0 && _isActive)
                OnStageComplete();
        }

        void SpawnBeat(int beat)
        {
            if (beat >= _totalBeats) return;

            // Basic beat: bass on 0,4,8... snare on 2,6,10...
            int primary = -1;
            if (beat % 4 == 0) primary = 0; // bass
            else if (beat % 4 == 2) primary = 1; // snare

            // Fill beat
            if (_enableFill && beat % 8 == 7)
                primary = Random.Range(0, Mathf.Min(_padCount, 6));

            // Random hi-hat on offbeats
            if (_padCount >= 3 && beat % 2 == 1 && Random.value < 0.7f)
                SpawnNote(2); // hihat

            if (primary >= 0 && primary < _padCount)
                SpawnNote(primary);

            // Random additional note
            if (_padCount >= 4 && Random.value < 0.3f + _stageIndex * 0.1f)
            {
                int extra = Random.Range(3, _padCount);
                SpawnNote(extra);
            }

            // Syncopation
            if (_enableSyncopation && beat % 3 == 1 && Random.value < 0.5f)
                SpawnNote(Random.Range(0, _padCount));

            // Simultaneous second pad
            if (_enableSimultaneous && primary >= 0 && Random.value < 0.4f)
            {
                int second = (primary + 1) % _padCount;
                if (second != primary) SpawnNote(second);
            }
        }

        void SpawnNote(int padIndex)
        {
            if (padIndex < 0 || padIndex >= _padCount || padIndex >= _pads.Length) return;
            var pad = _pads[padIndex];
            if (pad == null) return;

            // Ring shrink: starts large (2.5x pad scale), shrinks to 1.0x over ~1.5 beats
            float shrinkDuration = _beatInterval * 1.5f;
            float targetTime = Time.time + shrinkDuration;

            pad.ringSr.enabled = true;
            pad.ringSr.color = new Color(1f, 1f, 1f, 1f);
            pad.ringSr.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

            _activeNotes.Add(new NoteEvent
            {
                padIndex = padIndex,
                targetTime = targetTime,
                spawnTime = Time.time,
                ringScaleStart = 2.5f,
                ringScaleEnd = 0.9f,
                judged = false,
                pad = pad
            });
        }

        void UpdateRings()
        {
            float now = Time.time;
            foreach (var note in _activeNotes)
            {
                if (note.judged) continue;
                float t = (now - note.spawnTime) / (note.targetTime - note.spawnTime);
                t = Mathf.Clamp01(t);
                float scale = Mathf.Lerp(note.ringScaleStart, note.ringScaleEnd, t);
                if (note.pad.ringSr != null)
                    note.pad.ringSr.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (!mouse.leftButton.wasPressedThisFrame) return;

            Vector2 screenPos = mouse.position.ReadValue();
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            worldPos.z = 0f;

            // Find tapped pad via collider
            var hit = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));
            if (hit == null) return;

            // Find which pad
            for (int i = 0; i < _pads.Length; i++)
            {
                if (_pads[i] != null && _pads[i].col == hit)
                {
                    TryJudgePad(i);
                    return;
                }
            }
        }

        void TryJudgePad(int padIndex)
        {
            float now = Time.time;
            NoteEvent best = null;
            float bestDelta = float.MaxValue;

            foreach (var note in _activeNotes)
            {
                if (note.judged || note.padIndex != padIndex) continue;
                float delta = Mathf.Abs(now - note.targetTime);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    best = note;
                }
            }

            if (best == null) return;

            string judgement;
            Color color;
            int baseScore;

            if (bestDelta <= PerfectWindow)
            {
                judgement = "PERFECT";
                color = new Color(0f, 1f, 1f);
                baseScore = 100;
            }
            else if (bestDelta <= GreatWindow)
            {
                judgement = "GREAT";
                color = new Color(0.5f, 1f, 0.5f);
                baseScore = 60;
            }
            else if (bestDelta <= GoodWindow)
            {
                judgement = "GOOD";
                color = new Color(1f, 1f, 0.3f);
                baseScore = 20;
            }
            else
            {
                RegisterMiss(best);
                return;
            }

            _combo++;
            float multiplier = judgement == "PERFECT"
                ? Mathf.Min(3.0f, 1.0f + _combo * 0.1f)
                : judgement == "GREAT"
                    ? Mathf.Min(2.0f, 1.0f + _combo * 0.05f)
                    : 1.0f;
            _score += Mathf.RoundToInt(baseScore * multiplier);

            best.judged = true;
            StartCoroutine(HitFeedback(best.pad, judgement));

            _gameManager.UpdateScoreDisplay(_score);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.ShowJudgement(judgement, color);
        }

        void RegisterMiss(NoteEvent note)
        {
            note.judged = true;
            _combo = 0;
            _missCount++;

            if (note.pad.ringSr != null) note.pad.ringSr.enabled = false;
            StartCoroutine(MissFeedback(note.pad));

            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateMissDisplay(_missCount, MaxMiss);
            _gameManager.ShowJudgement("MISS", new Color(1f, 0.3f, 0.3f));

            StartCoroutine(ShakeCamera());

            if (_missCount >= MaxMiss)
            {
                _isActive = false;
                _gameManager.OnGameOver();
            }
        }

        void AutoMissNotes()
        {
            float now = Time.time;
            var toMiss = new List<NoteEvent>();
            foreach (var note in _activeNotes)
                if (!note.judged && now - note.targetTime > GoodWindow + 0.05f)
                    toMiss.Add(note);
            foreach (var note in toMiss)
                RegisterMiss(note);

            _activeNotes.RemoveAll(n => n.judged && Time.time - n.targetTime > 0.5f);

            // Hide rings for judged notes
            foreach (var note in _activeNotes)
                if (note.judged && note.pad.ringSr != null)
                    note.pad.ringSr.enabled = false;
        }

        void OnStageComplete()
        {
            if (!_isActive) return;
            _isActive = false;
            _gameManager.OnStageClear();
        }

        IEnumerator HitFeedback(PadObject pad, string judgement)
        {
            if (pad == null || pad.go == null) yield break;

            // Hide ring
            if (pad.ringSr != null) pad.ringSr.enabled = false;

            // Pop scale
            Vector3 origScale = pad.go.transform.localScale;
            float t = 0f;
            Color flashColor = judgement == "PERFECT" ? new Color(1f, 1f, 0.5f) : Color.white;
            pad.padSr.color = flashColor;

            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.15f;
                float s = ratio < 0.5f ? 1f + 0.25f * (ratio / 0.5f) : 1.25f - 0.25f * ((ratio - 0.5f) / 0.5f);
                if (pad.go != null) pad.go.transform.localScale = origScale * s;
                yield return null;
            }
            if (pad.go != null)
            {
                pad.go.transform.localScale = origScale;
                pad.padSr.color = Color.white;
            }
        }

        IEnumerator MissFeedback(PadObject pad)
        {
            if (pad == null || pad.go == null) yield break;
            if (pad.ringSr != null) pad.ringSr.enabled = false;

            pad.padSr.color = new Color(1f, 0.2f, 0.2f);
            yield return new WaitForSeconds(0.2f);
            if (pad.padSr != null) pad.padSr.color = Color.white;
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
                float strength = 0.08f * (1f - t / duration);
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
            if (_pads != null)
                foreach (var p in _pads)
                    if (p != null && p.go != null) Destroy(p.go);
        }
    }
}
