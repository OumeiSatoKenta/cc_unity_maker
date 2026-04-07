using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game074v2_NoteRain
{
    public class NoteController : MonoBehaviour
    {
        [SerializeField] Sprite _spriteNormal;
        [SerializeField] Sprite _spriteFake;
        [SerializeField] Sprite _spriteAccelerating;
        [SerializeField] Sprite _spriteCurve;
        [SerializeField] Sprite _spriteCatcher;
        [SerializeField] NoteRainGameManager _gameManager;

        // Stage config params
        int _bpm = 80;
        int _noteCount = 15;
        float _spawnRangeRatio = 0.4f;
        bool _enableFake;
        bool _enableDouble;
        bool _enableCurve;
        bool _enableAccelerating;

        float _camSize;
        float _camWidth;
        float _judgeY;
        float _spawnY;
        float _catcherHalfWidth = 1.2f;
        float _perfectWindow = 0.15f;
        float _greatWindow = 0.35f;
        float _goodWindow = 0.6f;

        GameObject _catcherObj;
        SpriteRenderer _catcherSr;
        float _catcherX;
        bool _isDragging;
        float _dragStartScreenX;
        float _dragStartWorldX;

        List<Note> _activeNotes = new List<Note>();
        List<Note> _notePool = new List<Note>();

        bool _isActive;
        int _pendingNoteCount;
        int _spawnedCount;
        float _spawnInterval;
        float _spawnTimer;
        Coroutine _shakeCoroutine;
        Vector3 _cameraOrigPos;

        int _totalScore;
        int _combo;
        int _life = 5;
        int _notesProcessed;
        int _stageNoteCount;

        public int TotalScore => _totalScore;
        public bool IsActive => _isActive;

        void Update()
        {
            if (!_isActive) return;
            HandleInput();
            CheckMissedNotes();
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            ClearAllNotes();

            _life = 5;
            _combo = 0;
            _spawnedCount = 0;
            _notesProcessed = 0;

            _camSize = Camera.main.orthographicSize;
            _camWidth = _camSize * Camera.main.aspect;
            _judgeY = -_camSize + 2.5f;
            _spawnY = _camSize - 0.5f;

            ApplyStageParams(stageIndex);
            _stageNoteCount = _noteCount;

            SetupCatcher();
            _gameManager.UpdateLifeDisplay(_life);
            _gameManager.UpdateScoreDisplay(_totalScore);
            _gameManager.UpdateComboDisplay(_combo);

            _spawnInterval = 60f / _bpm;
            _isActive = true;
            StartCoroutine(SpawnRoutine());
        }

        void ApplyStageParams(int stageIndex)
        {
            switch (stageIndex)
            {
                case 0: _bpm=80;  _noteCount=15; _spawnRangeRatio=0.4f; _enableFake=false; _enableDouble=false; _enableCurve=false; _enableAccelerating=false; break;
                case 1: _bpm=100; _noteCount=25; _spawnRangeRatio=0.85f; _enableFake=false; _enableDouble=false; _enableCurve=false; _enableAccelerating=false; break;
                case 2: _bpm=120; _noteCount=35; _spawnRangeRatio=0.85f; _enableFake=true;  _enableDouble=true;  _enableCurve=false; _enableAccelerating=false; break;
                case 3: _bpm=140; _noteCount=45; _spawnRangeRatio=0.9f;  _enableFake=true;  _enableDouble=true;  _enableCurve=false; _enableAccelerating=true;  break;
                case 4: _bpm=160; _noteCount=60; _spawnRangeRatio=0.95f; _enableFake=true;  _enableDouble=true;  _enableCurve=true;  _enableAccelerating=true;  break;
            }
        }

        void SetupCatcher()
        {
            if (_catcherObj == null)
            {
                _catcherObj = new GameObject("Catcher");
                _catcherSr = _catcherObj.AddComponent<SpriteRenderer>();
                _catcherSr.sortingOrder = 5;
            }
            _catcherX = 0f;
            _catcherObj.transform.position = new Vector3(_catcherX, _judgeY, 0f);
            _catcherObj.transform.localScale = new Vector3(2.4f, 0.5f, 1f);
            if (_spriteCatcher != null) _catcherSr.sprite = _spriteCatcher;
            _catcherObj.SetActive(true);
        }

        IEnumerator SpawnRoutine()
        {
            while (_spawnedCount < _stageNoteCount && _isActive)
            {
                SpawnNote();
                _spawnedCount++;
                if (_enableDouble && _spawnedCount < _stageNoteCount && Random.value < 0.25f)
                {
                    yield return new WaitForSeconds(0.1f);
                    SpawnNote();
                    _spawnedCount++;
                }
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        void SpawnNote()
        {
            float range = _camWidth * _spawnRangeRatio;
            float xPos = Random.Range(-range, range);
            NoteType type = NoteType.Normal;

            if (_enableFake && Random.value < 0.15f) type = NoteType.Fake;
            else if (_enableAccelerating && Random.value < 0.2f) type = NoteType.Accelerating;
            else if (_enableCurve && Random.value < 0.2f) type = NoteType.Curve;

            Note note = GetOrCreateNote();
            float fallTime = (_spawnY - _judgeY) / GetFallSpeed();
            float curveX = (type == NoteType.Curve) ? Random.Range(-0.8f, 0.8f) : 0f;
            // Adjust spawn X so curve notes land somewhere reasonable
            if (type == NoteType.Curve)
            {
                xPos = Mathf.Clamp(xPos - curveX * fallTime, -range, range);
            }

            note.transform.position = new Vector3(xPos, _spawnY, 0f);
            note.transform.localScale = Vector3.one * 0.6f;
            note.Initialize(type, GetFallSpeed(), curveX);
            note.gameObject.SetActive(true);
            SetNoteSprite(note, type);
            _activeNotes.Add(note);
        }

        float GetFallSpeed() => _bpm / 30f * 0.8f; // ~2 world units/sec at BPM80

        void SetNoteSprite(Note note, NoteType type)
        {
            var sr = note.SpriteRenderer;
            if (sr == null) return;
            sr.color = Color.white;
            sr.sortingOrder = 3;
            switch (type)
            {
                case NoteType.Normal:       sr.sprite = _spriteNormal; break;
                case NoteType.Fake:         sr.sprite = _spriteFake; break;
                case NoteType.Accelerating: sr.sprite = _spriteAccelerating; break;
                case NoteType.Curve:        sr.sprite = _spriteCurve; break;
            }
        }

        Note GetOrCreateNote()
        {
            for (int i = 0; i < _notePool.Count; i++)
            {
                var n = _notePool[i];
                if (!n.gameObject.activeSelf)
                {
                    _notePool.RemoveAt(i);
                    return n;
                }
            }
            var go = new GameObject("Note");
            go.transform.SetParent(transform);
            var note = go.AddComponent<Note>();
            go.AddComponent<SpriteRenderer>();
            return note;
        }

        void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _dragStartScreenX = mouse.position.ReadValue().x;
                _dragStartWorldX = _catcherX;
            }
            else if (mouse.leftButton.isPressed && _isDragging)
            {
                float screenDelta = mouse.position.ReadValue().x - _dragStartScreenX;
                float screenWidth = Screen.width;
                float worldDelta = screenDelta / screenWidth * (_camWidth * 2f);
                float maxX = _camWidth * _spawnRangeRatio;
                _catcherX = Mathf.Clamp(_dragStartWorldX + worldDelta, -maxX, maxX);
                _catcherObj.transform.position = new Vector3(_catcherX, _judgeY, 0f);
                CheckCatch();
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }
        }

        void CheckCatch()
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];
                if (note == null || !note.gameObject.activeSelf) { _activeNotes.RemoveAt(i); continue; }

                float dist = Mathf.Abs(note.transform.position.x - _catcherX);
                float yDist = Mathf.Abs(note.transform.position.y - _judgeY);

                if (yDist < _goodWindow && dist < _catcherHalfWidth)
                {
                    ProcessCatch(note, yDist);
                    _activeNotes.RemoveAt(i);
                }
            }
        }

        void ProcessCatch(Note note, float yDist)
        {
            _notesProcessed++;
            if (note.noteType == NoteType.Fake)
            {
                _combo = 0;
                _life--;
                _gameManager.UpdateLifeDisplay(_life);
                _gameManager.UpdateComboDisplay(_combo);
                _gameManager.ShowJudgement("FAKE!", Color.red);
                StartCoroutine(FlashNote(note, Color.red));
                StartCoroutine(CameraShake());
                if (_life <= 0) { DeactivateNote(note); TriggerGameOver(); return; }
            }
            else
            {
                string label;
                int pts;
                if (yDist < _perfectWindow) { label = "PERFECT!"; pts = 120; }
                else if (yDist < _greatWindow) { label = "GREAT"; pts = 70; }
                else { label = "GOOD"; pts = 30; }

                if (pts >= 70) _combo++;
                else { /* Good does not break combo */ }

                float multiplier = (pts == 120) ? Mathf.Min(3.0f, 1f + _combo * 0.12f)
                                 : (pts == 70)  ? Mathf.Min(2.0f, 1f + _combo * 0.06f)
                                 : 1.0f;
                int scored = Mathf.RoundToInt(pts * multiplier);
                _totalScore += scored;
                _gameManager.UpdateScoreDisplay(_totalScore);
                _gameManager.UpdateComboDisplay(_combo);
                _gameManager.ShowJudgement(label, label == "PERFECT!" ? Color.cyan : label == "GREAT" ? Color.yellow : Color.white);
                StartCoroutine(PopNote(note));
                return; // PopNote will call DeactivateNote after animation
            }
            DeactivateNote(note);
            CheckStageComplete();
        }

        void CheckMissedNotes()
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                if (!_isActive) return;
                var note = _activeNotes[i];
                if (note == null || !note.gameObject.activeSelf) { _activeNotes.RemoveAt(i); continue; }
                if (note.transform.position.y < _judgeY - 1.5f)
                {
                    if (note.noteType != NoteType.Fake)
                    {
                        _combo = 0;
                        _life--;
                        _notesProcessed++;
                        _gameManager.UpdateLifeDisplay(_life);
                        _gameManager.UpdateComboDisplay(_combo);
                        _gameManager.ShowJudgement("MISS", Color.red);
                        TriggerCameraShake();
                        if (_life <= 0) { _activeNotes.RemoveAt(i); DeactivateNote(note); TriggerGameOver(); return; }
                    }
                    else
                    {
                        _notesProcessed++;
                    }
                    _activeNotes.RemoveAt(i);
                    DeactivateNote(note);
                    CheckStageComplete();
                }
            }
        }

        void CheckStageComplete()
        {
            if (_notesProcessed >= _stageNoteCount && _spawnedCount >= _stageNoteCount)
            {
                _isActive = false;
                _gameManager.OnStageClear();
            }
        }

        void TriggerGameOver()
        {
            _isActive = false;
            StopAllCoroutines();
            _gameManager.OnGameOver();
        }

        void DeactivateNote(Note note)
        {
            note.SetActive(false);
            note.gameObject.SetActive(false);
            _notePool.Add(note);
        }

        void ClearAllNotes()
        {
            foreach (var n in _activeNotes)
            {
                if (n != null) { n.SetActive(false); n.gameObject.SetActive(false); _notePool.Add(n); }
            }
            _activeNotes.Clear();
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active) { StopAllCoroutines(); ClearAllNotes(); }
            if (_catcherObj != null) _catcherObj.SetActive(active);
        }

        IEnumerator PopNote(Note note)
        {
            if (note == null) yield break;
            note.gameObject.SetActive(true); // keep visible during animation
            float t = 0f;
            while (t < 0.15f)
            {
                if (note == null || !note.gameObject.activeSelf) yield break;
                t += Time.deltaTime;
                note.transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 0.9f, t / 0.15f);
                yield return null;
            }
            DeactivateNote(note);
            CheckStageComplete();
        }

        IEnumerator FlashNote(Note note, Color flashColor)
        {
            if (note == null || note.SpriteRenderer == null) yield break;
            note.SpriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.15f);
        }

        void TriggerCameraShake()
        {
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(CameraShake());
        }

        IEnumerator CameraShake()
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            if (_cameraOrigPos == Vector3.zero) _cameraOrigPos = cam.transform.position;
            float dur = 0.1f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                cam.transform.position = _cameraOrigPos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f);
                yield return null;
            }
            cam.transform.position = _cameraOrigPos;
            _shakeCoroutine = null;
        }

        void OnDestroy()
        {
            ClearAllNotes();
        }
    }
}
