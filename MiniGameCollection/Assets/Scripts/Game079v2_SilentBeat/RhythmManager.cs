using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game079v2_SilentBeat
{
    public class RhythmManager : MonoBehaviour
    {
        [SerializeField] SilentBeatGameManager _gameManager;

        struct StageParams
        {
            public float bpm;
            public int guideTaps;
            public bool hasVisualPulse;
            public bool hasBpmChange;
            public bool hasWaveBpm;
            public bool hasRandomChange;
            public int totalTapCount;
        }

        static readonly StageParams[] StageTable = new StageParams[]
        {
            new StageParams { bpm=60f,  guideTaps=8, hasVisualPulse=true,  hasBpmChange=false, hasWaveBpm=false, hasRandomChange=false, totalTapCount=20 },
            new StageParams { bpm=90f,  guideTaps=4, hasVisualPulse=false, hasBpmChange=false, hasWaveBpm=false, hasRandomChange=false, totalTapCount=25 },
            new StageParams { bpm=120f, guideTaps=4, hasVisualPulse=false, hasBpmChange=true,  hasWaveBpm=false, hasRandomChange=false, totalTapCount=30 },
            new StageParams { bpm=80f,  guideTaps=4, hasVisualPulse=false, hasBpmChange=false, hasWaveBpm=true,  hasRandomChange=false, totalTapCount=40 },
            new StageParams { bpm=150f, guideTaps=2, hasVisualPulse=false, hasBpmChange=false, hasWaveBpm=false, hasRandomChange=true,  totalTapCount=50 },
        };

        const float PerfectThreshold = 0.020f;
        const float GreatThreshold   = 0.050f;
        const float GoodThreshold    = 0.100f;

        const int PerfectBase = 150;
        const int GreatBase   = 80;
        const int GoodBase    = 30;

        bool _isActive;
        StageParams _currentParams;
        float _currentBpm;
        float _expectedInterval;
        float _lastTapTime;
        bool _playPhaseActive;
        int _tapCount;
        int _comboCount;
        int _consecutiveMiss;
        int _score;
        int _perfectCount;
        bool _firstTap;
        bool _bpmChangeTriggered;

        float _waveTimer;
        const float WavePeriod = 8f;
        float _nextBpmChangeTime;

        Coroutine _bpmFlashCoroutine;

        public int TotalScore => _score;

        public void ResetScore()
        {
            _score = 0;
            _comboCount = 0;
            _consecutiveMiss = 0;
            _perfectCount = 0;
            _isActive = false;
        }

        public void SetActive(bool active) => _isActive = active;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _bpmFlashCoroutine = null;
            _currentParams = StageTable[Mathf.Clamp(stageIndex, 0, StageTable.Length - 1)];
            _currentBpm = _currentParams.bpm;
            _expectedInterval = 60f / _currentBpm;
            _tapCount = 0;
            _comboCount = 0;
            _consecutiveMiss = 0;
            _perfectCount = 0;
            _playPhaseActive = false;
            _isActive = true;
            _waveTimer = 0f;
            _nextBpmChangeTime = 0f;
            _bpmChangeTriggered = false;

            _gameManager.UpdateProgress(0, _currentParams.totalTapCount);
            _gameManager.UpdateBpm(_currentBpm);

            StartCoroutine(RunGuidePhase());
        }

        IEnumerator RunGuidePhase()
        {
            _gameManager.ShowGuidePhase();

            float interval = 60f / _currentBpm;

            yield return new WaitForSeconds(0.5f);
            if (!_isActive) yield break;

            for (int i = 0; i < _currentParams.guideTaps; i++)
            {
                if (!_isActive) yield break;

                _gameManager.FlashTapArea(true);
                yield return new WaitForSeconds(0.1f);
                _gameManager.FlashTapArea(false);
                yield return new WaitForSeconds(interval - 0.1f);
            }

            _gameManager.HideGuidePhase();

            yield return new WaitForSeconds(0.3f);
            if (!_isActive) yield break;

            StartPlayPhase();
        }

        void StartPlayPhase()
        {
            _playPhaseActive = true;
            _firstTap = true;
            _lastTapTime = 0f;

            if (_currentParams.hasWaveBpm)
                _waveTimer = 0f;
            if (_currentParams.hasRandomChange)
                _nextBpmChangeTime = Time.time + Random.Range(3f, 6f);
        }

        void Update()
        {
            if (!_isActive || !_playPhaseActive) return;

            if (_currentParams.hasWaveBpm)
            {
                _waveTimer += Time.deltaTime;
                float wave = Mathf.Sin(_waveTimer / WavePeriod * Mathf.PI * 2f);
                _currentBpm = 80f + (120f - 80f) * (wave * 0.5f + 0.5f);
                _expectedInterval = 60f / _currentBpm;
                _gameManager.UpdateBpm(_currentBpm);
            }
            else if (_currentParams.hasRandomChange && Time.time >= _nextBpmChangeTime)
            {
                _currentBpm = Random.Range(100f, 180f);
                _expectedInterval = 60f / _currentBpm;
                _gameManager.UpdateBpm(_currentBpm);
                _nextBpmChangeTime = Time.time + Random.Range(3f, 7f);
                TriggerBpmFlash();
            }

            bool tapped = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                tapped = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                tapped = true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                tapped = true;

            if (tapped)
                HandleTap();
        }

        void HandleTap()
        {
            float now = Time.time;

            // BPM change at halfway for stage 3 (once only)
            if (_currentParams.hasBpmChange && !_bpmChangeTriggered
                && _tapCount >= _currentParams.totalTapCount / 2)
            {
                _bpmChangeTriggered = true;
                _currentBpm = 90f;
                _expectedInterval = 60f / _currentBpm;
                _gameManager.UpdateBpm(_currentBpm);
                TriggerBpmFlash();
            }

            if (_firstTap)
            {
                _firstTap = false;
                _lastTapTime = now;
                _gameManager.FlashTapArea(true);
                StartCoroutine(ResetTapVisual());
                return;
            }

            float actualInterval = now - _lastTapTime;
            _lastTapTime = now;

            float deviation = actualInterval - _expectedInterval;
            float absDeviation = Mathf.Abs(deviation);

            _gameManager.FlashTapArea(true);
            StartCoroutine(ResetTapVisual());
            _gameManager.UpdateAccuracyIndicator(deviation / GoodThreshold);

            string judgement;
            Color judgementColor;

            if (absDeviation < PerfectThreshold)
            {
                judgement = "Perfect!!";
                judgementColor = new Color(1f, 0.85f, 0f);
                _comboCount++;
                _consecutiveMiss = 0;
                _perfectCount++;
                float multiplier = Mathf.Min(1f + _comboCount * 0.15f, 4f);
                _score += Mathf.RoundToInt(PerfectBase * multiplier);
            }
            else if (absDeviation < GreatThreshold)
            {
                judgement = "Great!";
                judgementColor = new Color(0.2f, 1f, 0.4f);
                _comboCount++;
                _consecutiveMiss = 0;
                float multiplier = Mathf.Min(1f + _comboCount * 0.08f, 2.5f);
                _score += Mathf.RoundToInt(GreatBase * multiplier);
            }
            else if (absDeviation < GoodThreshold)
            {
                judgement = "Good";
                judgementColor = new Color(0.3f, 0.8f, 1f);
                _comboCount++;
                _consecutiveMiss = 0;
                _score += GoodBase;
            }
            else
            {
                judgement = "Miss";
                judgementColor = new Color(1f, 0.2f, 0.2f);
                _comboCount = 0;
                _consecutiveMiss++;
            }

            _gameManager.ShowJudgement(judgement, judgementColor);
            _gameManager.UpdateComboDisplay(_comboCount);
            _gameManager.UpdateScoreDisplay(_score);

            if (_consecutiveMiss >= 3)
            {
                _playPhaseActive = false;
                _isActive = false;
                _gameManager.OnGameOver();
                return;
            }

            _tapCount++;
            _gameManager.UpdateProgress(_tapCount, _currentParams.totalTapCount);

            if (_tapCount >= _currentParams.totalTapCount)
            {
                _playPhaseActive = false;
                _isActive = false;

                // All-Perfect bonus: all evaluated taps (totalTapCount - 1, excluding first) were Perfect
                if (_perfectCount >= _currentParams.totalTapCount - 1)
                {
                    _score = Mathf.RoundToInt(_score * 3f);
                    _gameManager.ShowJudgement("完全内部時計！", new Color(1f, 0.9f, 0f));
                }

                _gameManager.UpdateScoreDisplay(_score);
                _gameManager.OnStageClear();
            }
        }

        void TriggerBpmFlash()
        {
            if (_bpmFlashCoroutine != null) StopCoroutine(_bpmFlashCoroutine);
            _bpmFlashCoroutine = StartCoroutine(BpmChangeFlash());
        }

        IEnumerator ResetTapVisual()
        {
            yield return new WaitForSeconds(0.1f);
            _gameManager.FlashTapArea(false);
        }

        IEnumerator BpmChangeFlash()
        {
            if (!_isActive) yield break;
            _gameManager.ShowJudgement("BPM Change!", new Color(1f, 0.5f, 0f));
            yield return new WaitForSeconds(0.8f);
        }
    }
}
