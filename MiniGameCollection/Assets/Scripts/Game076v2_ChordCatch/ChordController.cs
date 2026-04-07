using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game076v2_ChordCatch
{
    public class ChordController : MonoBehaviour
    {
        [SerializeField] ChordCatchGameManager _gameManager;
        [SerializeField] Transform _buttonContainer;

        public int TotalScore { get; private set; }

        // Chord definitions: name -> frequencies (Hz)
        static readonly Dictionary<string, float[]> ChordFrequencies = new()
        {
            { "C",   new[] { 261.63f, 329.63f, 392.00f } },
            { "F",   new[] { 174.61f, 220.00f, 261.63f } },
            { "G",   new[] { 196.00f, 246.94f, 293.66f } },
            { "Am",  new[] { 220.00f, 261.63f, 329.63f } },
            { "Dm",  new[] { 146.83f, 174.61f, 220.00f } },
            { "Em",  new[] { 164.81f, 196.00f, 246.94f } },
            { "G7",  new[] { 196.00f, 246.94f, 293.66f, 349.23f } },
            { "Dm7", new[] { 146.83f, 174.61f, 220.00f, 261.63f } },
            { "C/E", new[] { 164.81f, 261.63f, 329.63f, 392.00f } },
            { "F/A", new[] { 220.00f, 174.61f, 261.63f, 349.23f } },
        };

        static readonly string[][] StageChords = new[]
        {
            new[] { "C", "F", "G" },
            new[] { "C", "F", "G", "Am", "Dm", "Em" },
            new[] { "C", "F", "G", "Am", "Dm", "Em", "G7", "Dm7" },
            new[] { "C", "F", "G", "Am", "G7", "C/E", "F/A" },
            new[] { "C", "F", "G", "Am", "Dm", "Em", "G7", "C/E" },
        };

        static readonly int[] StageBpm        = { 60,  80,  100, 120, 140 };
        static readonly float[] StageAnswerTime = { 4f,  3f,  2.5f, 2f,  1.5f };
        static readonly int[] StageReplayLimit = { -1,   5,   3,   2,   1 };
        static readonly int[] StageQuestions   = { 8,   12,  16,  20,  24 };

        AudioSource _audioSource;
        int _stageIndex;
        string[] _currentChords;
        int _bpm;
        float _answerTime;
        int _replayLimit;
        int _replayCount;
        int _questionCount;
        int _currentQuestion;
        string _currentAnswer;
        float _questionStartTime;
        bool _waitingAnswer;
        bool _isActive;

        int _score;
        int _combo;
        int _missCount;
        int _correctCount;
        const int MaxMiss = 5;

        Sprite _normalSprite;
        Sprite _correctSprite;
        Sprite _wrongSprite;
        Dictionary<string, Button> _chordButtons = new();
        Button _replayButton;
        TextMeshProUGUI _replayCountText;

        Coroutine _questionCoroutine;
        Coroutine _beatCoroutine;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            _stageIndex = stageIndex;
            _currentChords = StageChords[stageIndex];
            _bpm = StageBpm[stageIndex];
            _answerTime = StageAnswerTime[stageIndex];
            _replayLimit = StageReplayLimit[stageIndex];
            _replayCount = _replayLimit == -1 ? 999 : _replayLimit;
            _questionCount = StageQuestions[stageIndex];
            _currentQuestion = 0;
            _score = 0;
            _combo = 0;
            _missCount = 0;
            _correctCount = 0;
            _waitingAnswer = false;
            _isActive = true;

            BuildButtons();
            UpdateReplayUI();
            _gameManager.UpdateProgressDisplay(0, _questionCount);

            if (_questionCoroutine != null) StopCoroutine(_questionCoroutine);
            if (_beatCoroutine != null) StopCoroutine(_beatCoroutine);
            _beatCoroutine = StartCoroutine(BeatLoop());
            _questionCoroutine = StartCoroutine(QuestionLoop());
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                if (_questionCoroutine != null) StopCoroutine(_questionCoroutine);
                if (_beatCoroutine != null) StopCoroutine(_beatCoroutine);
            }
        }

        void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;

            _normalSprite  = LoadSprite("Sprites/Game076v2_ChordCatch/chord_button");
            _correctSprite = LoadSprite("Sprites/Game076v2_ChordCatch/chord_button_correct");
            _wrongSprite   = LoadSprite("Sprites/Game076v2_ChordCatch/chord_button_wrong");
        }

        Sprite LoadSprite(string path)
        {
            return Resources.Load<Sprite>(path);
        }

        void BuildButtons()
        {
            foreach (Transform child in _buttonContainer)
                Destroy(child.gameObject);
            _chordButtons.Clear();

            int count = _currentChords.Length;
            int cols = count <= 3 ? 3 : count <= 6 ? 3 : 4;
            int rows = Mathf.CeilToInt((float)count / cols);

            float btnW = 240f;
            float btnH = 100f;
            float padX = 20f;
            float padY = 16f;
            float totalW = cols * btnW + (cols - 1) * padX;
            float startX = -totalW / 2f + btnW / 2f;
            float startY = (rows - 1) * (btnH + padY) / 2f;

            for (int i = 0; i < count; i++)
            {
                string chordName = _currentChords[i];
                int col = i % cols;
                int row = i / cols;

                var btnObj = new GameObject($"Btn_{chordName}", typeof(RectTransform));
                btnObj.transform.SetParent(_buttonContainer, false);
                var rt = btnObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(btnW, btnH);
                rt.anchoredPosition = new Vector2(
                    startX + col * (btnW + padX),
                    startY - row * (btnH + padY)
                );

                var img = btnObj.AddComponent<Image>();
                if (_normalSprite != null) img.sprite = _normalSprite;
                else img.color = new Color(0f, 0.74f, 0.83f);

                var btn = btnObj.AddComponent<Button>();
                string captured = chordName;
                btn.onClick.AddListener(() => OnChordButtonPressed(captured));

                var txtObj = new GameObject("Label", typeof(RectTransform));
                txtObj.transform.SetParent(btnObj.transform, false);
                var txtRT = txtObj.GetComponent<RectTransform>();
                txtRT.anchorMin = Vector2.zero;
                txtRT.anchorMax = Vector2.one;
                txtRT.sizeDelta = Vector2.zero;
                txtRT.anchoredPosition = Vector2.zero;
                var tmp = txtObj.AddComponent<TextMeshProUGUI>();
                tmp.text = chordName;
                tmp.fontSize = 44;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;

                var jpFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (jpFont == null)
                {
                    var fontAsset = UnityEngine.Object.FindFirstObjectByType<TextMeshProUGUI>();
                    if (fontAsset != null) tmp.font = fontAsset.font;
                }

                _chordButtons[chordName] = btn;
            }
        }

        void UpdateReplayUI()
        {
            if (_replayButton == null) return;
            _replayButton.interactable = _replayCount > 0;
            if (_replayCountText != null)
                _replayCountText.text = _replayLimit == -1 ? "∞" : $"{_replayCount}";
        }

        public void SetReplayButton(Button btn, TextMeshProUGUI countText)
        {
            _replayButton = btn;
            _replayCountText = countText;
            if (_replayButton != null)
                _replayButton.onClick.AddListener(OnReplayPressed);
            UpdateReplayUI();
        }

        IEnumerator BeatLoop()
        {
            float beatDuration = 60f / _bpm;
            while (_isActive)
            {
                _gameManager.SetBeatActive(true);
                yield return new WaitForSeconds(0.12f);
                _gameManager.SetBeatActive(false);
                yield return new WaitForSeconds(Mathf.Max(0f, beatDuration - 0.12f));
            }
        }

        IEnumerator QuestionLoop()
        {
            float beatDuration = 60f / _bpm;
            yield return new WaitForSeconds(beatDuration);

            while (_isActive && _currentQuestion < _questionCount)
            {
                _currentAnswer = _currentChords[Random.Range(0, _currentChords.Length)];
                PlayChord(_currentAnswer);
                _questionStartTime = Time.time;
                _waitingAnswer = true;
                _gameManager.UpdateProgressDisplay(_currentQuestion + 1, _questionCount);

                float elapsed = 0f;
                while (_waitingAnswer && elapsed < _answerTime)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (_waitingAnswer)
                {
                    // Timeout = Miss
                    _waitingAnswer = false;
                    RegisterMiss(true);
                }

                yield return new WaitForSeconds(beatDuration * 0.5f);
            }

            if (_isActive && _missCount < MaxMiss)
            {
                _isActive = false;
                // Check clear condition
                float accuracy = _questionCount > 0 ? (float)_correctCount / _questionCount : 0f;
                if (accuracy >= 0.5f)
                    _gameManager.OnStageClear();
                else
                    _gameManager.OnGameOver();
            }
        }

        void OnChordButtonPressed(string chordName)
        {
            if (!_isActive || !_waitingAnswer) return;
            _waitingAnswer = false;

            float elapsed = Time.time - _questionStartTime;
            float beatDuration = 60f / _bpm;
            float timingOffset = Mathf.Abs(elapsed - beatDuration);

            if (chordName == _currentAnswer)
            {
                _correctCount++;
                string judgement;
                Color color;
                int pts;

                if (timingOffset < 0.04f)
                {
                    judgement = "Perfect!";
                    color = new Color(1f, 0.9f, 0.1f);
                    float mult = 1f + Mathf.Min(_combo * 0.15f, 2f);
                    pts = Mathf.RoundToInt(150 * mult);
                }
                else if (timingOffset < 0.1f)
                {
                    judgement = "Great!";
                    color = new Color(0.1f, 1f, 0.5f);
                    float mult = 1f + Mathf.Min(_combo * 0.08f, 1.5f);
                    pts = Mathf.RoundToInt(90 * mult);
                }
                else
                {
                    judgement = "Good";
                    color = new Color(0.4f, 0.8f, 1f);
                    pts = 40;
                }

                _combo++;
                _score += pts;
                TotalScore += pts;
                _gameManager.UpdateScoreDisplay(_score);
                _gameManager.UpdateComboDisplay(_combo);
                _gameManager.ShowJudgement(judgement, color);
                _currentQuestion++;
                StartCoroutine(FlashButton(chordName, true));
            }
            else
            {
                RegisterMiss(false);
                StartCoroutine(FlashButton(chordName, false));
            }
        }

        void RegisterMiss(bool isTimeout)
        {
            _combo = 0;
            _missCount++;
            _gameManager.UpdateComboDisplay(0);
            _gameManager.UpdateMissDisplay(_missCount);
            _gameManager.ShowJudgement("Miss", new Color(1f, 0.3f, 0.3f));
            _currentQuestion++;
            StartCoroutine(CameraShake());

            if (_missCount >= MaxMiss && _isActive)
            {
                _isActive = false;
                if (_questionCoroutine != null) StopCoroutine(_questionCoroutine);
                if (_beatCoroutine != null) StopCoroutine(_beatCoroutine);
                _gameManager.OnGameOver();
            }
        }

        void OnReplayPressed()
        {
            if (!_isActive || !_waitingAnswer || _replayCount <= 0) return;
            if (_replayLimit != -1) _replayCount--;
            UpdateReplayUI();
            PlayChord(_currentAnswer);
        }

        void PlayChord(string chordName)
        {
            if (!ChordFrequencies.TryGetValue(chordName, out float[] freqs)) return;
            int sampleRate = AudioSettings.outputSampleRate;
            float duration = 1.5f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            foreach (float freq in freqs)
            {
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Mathf.Clamp01(1f - t / duration);
                    envelope = Mathf.Pow(envelope, 0.5f);
                    data[i] += Mathf.Sin(2f * Mathf.PI * freq * t) * envelope / freqs.Length;
                }
            }

            var clip = AudioClip.Create("chord", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            _audioSource.PlayOneShot(clip);
        }

        IEnumerator FlashButton(string chordName, bool correct)
        {
            if (!_chordButtons.TryGetValue(chordName, out Button btn)) yield break;
            var img = btn.GetComponent<Image>();
            if (img == null) yield break;

            var originalSprite = _normalSprite;
            img.sprite = correct ? _correctSprite : _wrongSprite;

            // Scale pulse
            var rt = btn.GetComponent<RectTransform>();
            Vector3 originalScale = rt.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float s = 1f + 0.2f * Mathf.Sin(Mathf.PI * t / 0.2f);
                rt.localScale = originalScale * s;
                yield return null;
            }
            rt.localScale = originalScale;
            img.sprite = originalSprite != null ? originalSprite : null;
        }

        IEnumerator CameraShake()
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 originalPos = cam.transform.localPosition;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-0.15f, 0.15f);
                float y = Random.Range(-0.15f, 0.15f);
                cam.transform.localPosition = originalPos + new Vector3(x, y, 0);
                yield return null;
            }
            cam.transform.localPosition = originalPos;
        }

        void OnDestroy()
        {
            // AudioClips created at runtime don't need manual cleanup (GC handles them)
        }
    }
}
