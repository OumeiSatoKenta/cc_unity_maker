using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Game078v2_EchoBack
{
    public class EchoManager : MonoBehaviour
    {
        [SerializeField] EchoBackGameManager _gameManager;
        [SerializeField] EchoBackUI _ui;
        [SerializeField] Button[] _keyButtons;
        [SerializeField] Image[] _keyImages;
        [SerializeField] AudioSource _audioSource;

        // Stage parameters
        int _bpm = 70;
        int _keyCount = 4;
        int _replayMax = -1; // -1 = unlimited
        int _patternLengthMin = 3;
        int _patternLengthMax = 4;
        bool _hasRests = false;
        bool _hasChords = false;
        bool _hasReverse = false;
        bool _hasTempoChange = false;
        int _stageIndex = 0;

        // Runtime state
        bool _isActive = false;
        int _combo = 0;
        int _totalScore = 0;
        int _missCount = 0;
        int _replayCount = 0;
        List<int> _pattern = new List<int>(); // note indices (-1 = rest)
        List<float> _patternTimes = new List<float>(); // expected time offsets (seconds)
        List<bool> _isRest = new List<bool>();
        int _inputIndex = 0;
        float _patternStartTime;
        float _beatInterval;
        bool _isListening = false;
        bool _isInputting = false;
        bool _awaitingInput = false;
        int _perfectCount = 0;
        int _levelInStage = 0;
        int _maxLevels = 5;
        Color[] _keyColors;
        Color[] _defaultKeyColors;
        Coroutine _listenCoroutine;
        bool _isPatternComplete = false;

        // Procedural audio: sine wave clips per key
        AudioClip[] _noteClips;
        float[] _noteFreqRatios = { 1.0f, 1.122f, 1.26f, 1.335f, 1.498f, 1.682f, 1.888f, 2.0f };
        float _baseFreq = 261.63f; // C4

        public int TotalScore => _totalScore;
        public void ResetScore() { _totalScore = 0; }

        void Awake()
        {
            GenerateNoteClips();
        }

        void GenerateNoteClips()
        {
            _noteClips = new AudioClip[8];
            int sampleRate = AudioSettings.outputSampleRate;
            float duration = 0.5f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            for (int i = 0; i < 8; i++)
            {
                float freq = _baseFreq * _noteFreqRatios[i];
                float[] data = new float[samples];
                for (int s = 0; s < samples; s++)
                {
                    float t = s / (float)sampleRate;
                    float envelope = Mathf.Clamp01(1f - t / duration * 2f);
                    data[s] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.6f;
                }
                var clip = AudioClip.Create($"Note{i}", samples, 1, sampleRate, false);
                clip.SetData(data, 0);
                _noteClips[i] = clip;
            }
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _isActive = true;
            _isListening = false;
            _isInputting = false;
            _awaitingInput = false;
            // _totalScore is cumulative across stages - do NOT reset here
            _combo = 0;
            _missCount = 0;
            _levelInStage = 0;
            _isPatternComplete = false;

            // Map stageIndex to parameters
            switch (stageIndex)
            {
                case 0: _bpm = 70; _keyCount = 4; _replayMax = -1; _patternLengthMin = 3; _patternLengthMax = 4; _hasRests = false; _hasChords = false; _hasReverse = false; _hasTempoChange = false; _maxLevels = 3; break;
                case 1: _bpm = 90; _keyCount = 5; _replayMax = 3; _patternLengthMin = 4; _patternLengthMax = 6; _hasRests = true; _hasChords = false; _hasReverse = false; _hasTempoChange = false; _maxLevels = 4; break;
                case 2: _bpm = 110; _keyCount = 7; _replayMax = 2; _patternLengthMin = 5; _patternLengthMax = 8; _hasRests = false; _hasChords = true; _hasReverse = false; _hasTempoChange = false; _maxLevels = 4; break;
                case 3: _bpm = 130; _keyCount = 7; _replayMax = 1; _patternLengthMin = 6; _patternLengthMax = 10; _hasRests = false; _hasChords = false; _hasReverse = true; _hasTempoChange = false; _maxLevels = 5; break;
                case 4: _bpm = 150; _keyCount = 7; _replayMax = 0; _patternLengthMin = 8; _patternLengthMax = 12; _hasRests = false; _hasChords = false; _hasReverse = false; _hasTempoChange = true; _maxLevels = 5; break;
            }
            _replayCount = _replayMax;
            _beatInterval = 60f / _bpm;

            SetupKeyColors();
            _gameManager.UpdatePhase("聴取中...");
            _gameManager.UpdateReplayCount(_replayCount);
            _gameManager.UpdateProgressDots(0, 0);

            // Start first level
            StartNewLevel();
        }

        void SetupKeyColors()
        {
            // rhythm category: cyan/magenta palette
            _keyColors = new Color[]
            {
                new Color(0.0f, 0.8f, 1.0f),   // cyan
                new Color(0.8f, 0.2f, 1.0f),   // magenta
                new Color(0.2f, 1.0f, 0.6f),   // green
                new Color(1.0f, 0.5f, 0.1f),   // orange
                new Color(1.0f, 0.2f, 0.4f),   // red
                new Color(0.3f, 0.5f, 1.0f),   // blue
                new Color(1.0f, 0.9f, 0.2f),   // yellow
                new Color(0.9f, 0.5f, 0.8f),   // pink
            };

            for (int i = 0; i < _keyImages.Length; i++)
            {
                bool active = i < _keyCount;
                if (_keyButtons[i] != null) _keyButtons[i].gameObject.SetActive(active);
                if (active && _keyImages[i] != null)
                    _keyImages[i].color = _keyColors[i % _keyColors.Length];
            }
        }

        void StartNewLevel()
        {
            if (!_isActive) return;
            _levelInStage++;
            _missCount = 0;
            _inputIndex = 0;
            _perfectCount = 0;
            _isPatternComplete = false;
            GeneratePattern();
            _gameManager.UpdateProgressDots(0, _pattern.Count);
            if (_listenCoroutine != null) StopCoroutine(_listenCoroutine);
            _listenCoroutine = StartCoroutine(PlayPatternCoroutine(false));
        }

        void GeneratePattern()
        {
            _pattern.Clear();
            _patternTimes.Clear();
            _isRest.Clear();
            int length = Random.Range(_patternLengthMin, _patternLengthMax + 1);
            // Add one beat per level (gradual difficulty ramp within stage)
            length = Mathf.Min(length + _levelInStage - 1, _patternLengthMax + 2);

            float t = 0f;
            for (int i = 0; i < length; i++)
            {
                bool isRest = _hasRests && i > 0 && Random.value < 0.2f;
                _isRest.Add(isRest);
                if (isRest)
                    _pattern.Add(-1);
                else
                    _pattern.Add(Random.Range(0, _keyCount));
                _patternTimes.Add(t);
                // Use tempo-changed interval for stage5 (mirrors PlayPatternCoroutine)
                float bpmToUse = (_hasTempoChange && i >= length / 2) ? _bpm * 1.3f : _bpm;
                t += 60f / bpmToUse;
            }
        }

        IEnumerator PlayPatternCoroutine(bool isReplay)
        {
            _isListening = true;
            _isInputting = false;
            _awaitingInput = false;
            _gameManager.UpdatePhase(isReplay ? "リプレイ中..." : "聴取中...");

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < _pattern.Count; i++)
            {
                if (!_isActive) yield break;
                int noteIdx = _pattern[i];
                bool rest = _isRest[i];
                float bpmToUse = _bpm;
                // Stage5: tempo change in the middle
                if (_hasTempoChange && i >= _pattern.Count / 2)
                    bpmToUse = _bpm * 1.3f;
                float interval = 60f / bpmToUse;

                if (!rest && noteIdx >= 0 && noteIdx < _keyCount)
                {
                    PlayNote(noteIdx);
                    HighlightKey(noteIdx, true);
                    yield return new WaitForSeconds(0.18f);
                    HighlightKey(noteIdx, false);
                    yield return new WaitForSeconds(interval - 0.18f);
                }
                else
                {
                    // rest: silent beat
                    yield return new WaitForSeconds(interval);
                }
            }

            yield return new WaitForSeconds(0.3f);
            StartInputPhase();
        }

        void StartInputPhase()
        {
            if (!_isActive) return;
            _isListening = false;
            _isInputting = true;
            _awaitingInput = true;
            _inputIndex = 0;
            _patternStartTime = Time.time;
            _gameManager.UpdatePhase("入力中！");
            _gameManager.UpdateProgressDots(0, GetExpectedInputCount());
        }

        int GetExpectedInputCount()
        {
            int count = 0;
            for (int i = 0; i < _isRest.Count; i++)
                if (!_isRest[i]) count++;
            return count;
        }

        // Map input index (rest-skipped, possibly reversed) to original pattern index for timing
        int GetPatternIndexForInputIndex(int inputIdx)
        {
            var nonRestIndices = new System.Collections.Generic.List<int>();
            for (int i = 0; i < _isRest.Count; i++)
                if (!_isRest[i]) nonRestIndices.Add(i);
            if (_hasReverse) nonRestIndices.Reverse();
            if (inputIdx < 0 || inputIdx >= nonRestIndices.Count) return -1;
            return nonRestIndices[inputIdx];
        }

        // Get expected note sequence for input (rests skipped, possibly reversed)
        List<int> GetExpectedSequence()
        {
            var seq = new List<int>();
            for (int i = 0; i < _pattern.Count; i++)
                if (!_isRest[i]) seq.Add(_pattern[i]);
            if (_hasReverse) seq.Reverse();
            return seq;
        }

        public void OnKeyPressed(int keyIndex)
        {
            if (!_isActive || !_awaitingInput) return;

            var expected = GetExpectedSequence();
            if (_inputIndex >= expected.Count) return;

            float tapTime = Time.time;
            // Use pre-computed pattern times (accounts for tempo changes in stage 5)
            int notePatternIndex = GetPatternIndexForInputIndex(_inputIndex);
            float noteOffset = (notePatternIndex >= 0 && notePatternIndex < _patternTimes.Count) ? _patternTimes[notePatternIndex] : _inputIndex * _beatInterval;
            float expectedTime = _patternStartTime + noteOffset;
            float diff = Mathf.Abs(tapTime - expectedTime);

            int expectedNote = expected[_inputIndex];
            bool correctNote = (keyIndex == expectedNote);

            string judgement;
            Color judgementColor;
            int points = 0;

            if (!correctNote || diff > 0.25f)
            {
                // Miss
                judgement = "Miss";
                judgementColor = Color.red;
                points = 0;
                _combo = 0;
                _missCount++;
                StartCoroutine(FlashKeyRed(keyIndex));
                _gameManager.UpdateComboDisplay(0);
                if (_missCount >= 3)
                {
                    _awaitingInput = false;
                    _gameManager.ShowJudgement(judgement, judgementColor);
                    StartCoroutine(TriggerGameOverDelay());
                    return;
                }
            }
            else if (diff <= 0.05f)
            {
                judgement = "Perfect";
                judgementColor = new Color(1f, 0.9f, 0.1f);
                float mult = Mathf.Min(1f + _combo * 0.12f, 3f);
                points = Mathf.RoundToInt(120 * mult);
                _combo++;
                _perfectCount++;
                StartCoroutine(PopKeyScale(keyIndex));
            }
            else if (diff <= 0.12f)
            {
                judgement = "Great";
                judgementColor = new Color(0.3f, 1f, 0.5f);
                float mult = Mathf.Min(1f + _combo * 0.06f, 2f);
                points = Mathf.RoundToInt(70 * mult);
                _combo++;
                StartCoroutine(PopKeyScale(keyIndex));
            }
            else
            {
                judgement = "Good";
                judgementColor = new Color(0.5f, 0.8f, 1f);
                points = 25;
                _combo++;
                StartCoroutine(PopKeyScale(keyIndex));
            }

            _totalScore += points;
            PlayNote(keyIndex);
            _inputIndex++;
            _gameManager.ShowJudgement(judgement, judgementColor);
            _gameManager.UpdateScoreDisplay(_totalScore);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateProgressDots(_inputIndex, expected.Count);

            if (_inputIndex >= expected.Count)
            {
                _awaitingInput = false;
                StartCoroutine(CheckPatternComplete());
            }
        }

        IEnumerator CheckPatternComplete()
        {
            yield return new WaitForSeconds(0.5f);
            if (!_isActive) yield break;

            // Check perfect pattern bonus
            bool allPerfect = (_perfectCount == GetExpectedInputCount());
            if (allPerfect)
            {
                _totalScore = Mathf.RoundToInt(_totalScore * 1.5f);
                _gameManager.ShowJudgement("PERFECT ECHO!", new Color(1f, 0.6f, 1f));
                StartCoroutine(RainbowKeys());
                yield return new WaitForSeconds(0.8f);
            }
            _perfectCount = 0;
            _gameManager.UpdateScoreDisplay(_totalScore);

            // Next level or stage clear
            if (_levelInStage >= _maxLevels)
            {
                _isActive = false;
                _isInputting = false;
                _gameManager.OnStageClear();
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
                StartNewLevel();
            }
        }

        IEnumerator TriggerGameOverDelay()
        {
            yield return new WaitForSeconds(0.8f);
            if (_isActive)
            {
                _isActive = false;
                _isInputting = false;
                _gameManager.OnGameOver();
            }
        }

        public void OnReplayPressed()
        {
            if (!_isActive) return;
            if (_replayMax >= 0 && _replayCount <= 0) return;
            if (_isListening) return;
            if (_replayMax >= 0) _replayCount--;
            _gameManager.UpdateReplayCount(_replayCount);
            _inputIndex = 0;
            _isInputting = false;
            _awaitingInput = false;
            if (_listenCoroutine != null) StopCoroutine(_listenCoroutine);
            _listenCoroutine = StartCoroutine(PlayPatternCoroutine(true));
        }

        void PlayNote(int index)
        {
            if (index < 0 || index >= _noteClips.Length) return;
            _audioSource.PlayOneShot(_noteClips[index], 0.8f);
        }

        void HighlightKey(int index, bool on)
        {
            if (index < 0 || index >= _keyImages.Length) return;
            if (_keyImages[index] == null) return;
            _keyImages[index].color = on ? Color.white : _keyColors[index % _keyColors.Length];
        }

        IEnumerator PopKeyScale(int index)
        {
            if (index < 0 || index >= _keyButtons.Length) yield break;
            var t = _keyButtons[index].transform;
            float dur = 0.15f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                float ratio = elapsed / dur;
                float scale = ratio < 0.5f ? Mathf.Lerp(1f, 1.2f, ratio * 2f) : Mathf.Lerp(1.2f, 1f, (ratio - 0.5f) * 2f);
                t.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        IEnumerator FlashKeyRed(int index)
        {
            if (index < 0 || index >= _keyImages.Length) yield break;
            var img = _keyImages[index];
            if (img == null) yield break;
            img.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            img.color = _keyColors[index % _keyColors.Length];
        }

        IEnumerator RainbowKeys()
        {
            float dur = 0.6f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                for (int i = 0; i < _keyCount && i < _keyImages.Length; i++)
                {
                    if (_keyImages[i] == null) continue;
                    float hue = (elapsed / dur + i / (float)_keyCount) % 1f;
                    _keyImages[i].color = Color.HSVToRGB(hue, 0.8f, 1f);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            // Restore
            for (int i = 0; i < _keyCount && i < _keyImages.Length; i++)
                if (_keyImages[i] != null)
                    _keyImages[i].color = _keyColors[i % _keyColors.Length];
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                if (_listenCoroutine != null) StopCoroutine(_listenCoroutine);
                _isListening = false;
                _isInputting = false;
                _awaitingInput = false;
            }
        }

        void OnDestroy()
        {
            for (int i = 0; i < _noteClips.Length; i++)
                if (_noteClips[i] != null) Destroy(_noteClips[i]);
        }
    }
}
