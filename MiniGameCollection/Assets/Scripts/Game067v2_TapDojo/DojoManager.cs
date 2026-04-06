using UnityEngine;
using UnityEngine.InputSystem;
using Common;
using System.Collections;

namespace Game067v2_TapDojo
{
    public class DojoManager : MonoBehaviour
    {
        [SerializeField] TapDojoGameManager _gameManager;
        [SerializeField] SpriteRenderer _martialArtistRenderer;
        [SerializeField] Camera _camera;

        // Stage targets
        static readonly long[] StageTargets = { 1000, 5000, 20000, 80000, 300000 };
        static readonly string[] RankNames = { "白帯", "黄帯", "緑帯", "茶帯", "黒帯", "師範" };

        // Tech definitions: name, baseCost, tapBonus, description
        static readonly string[] TechNames = { "正拳突き", "回し蹴り", "虎砲", "師範位" };
        static readonly long[] TechCosts = { 50, 200, 0, 5000 }; // 虎砲 is event-unlocked (cost 0)
        static readonly int[] TechTapBonus = { 1, 2, 5, 0 };

        bool _isActive;
        int _stageIndex;
        float _speedMultiplier;
        float _countMultiplier;
        float _complexityFactor;

        long _mp;
        long _stageTarget;
        int _baseTapValue = 1;
        float _autoRateBase = 0f;
        float _autoRateBonus = 0f;
        float _autoTimer = 0f;
        bool _autoUnlocked = false;

        bool[] _techUnlocked = new bool[4]; // seiken, mawashi, tohou, shihan
        bool _tournamentUnlocked = false;
        bool _trainingUnlocked = false;
        bool _shihanTestUnlocked = false;
        bool _shihanCleared = false;

        // Combo
        int _combo = 0;
        float _comboTimer = 0f;
        const float ComboTimeout = 0.5f;

        // Visual feedback coroutine
        Coroutine _pulseCoroutine;
        Color _defaultColor = Color.white;

        // Tournament / Training state
        bool _tournamentRunning = false;
        bool _trainingRunning = false;
        float _trainingTimer = 0f;
        int _trainingTaps = 0;
        const float TrainingDuration = 15f;
        const int TrainingTapGoal = 30;

        public long TotalScore => _mp;

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            StopAllCoroutines();
            _pulseCoroutine = null;
            if (_martialArtistRenderer != null) _martialArtistRenderer.color = _defaultColor;

            _stageIndex = stageIndex;
            _speedMultiplier = config.speedMultiplier;
            _countMultiplier = config.countMultiplier;
            _complexityFactor = config.complexityFactor;

            _mp = 0;
            _combo = 0;
            _comboTimer = 0f;
            _autoTimer = 0f;
            _tournamentRunning = false;
            _trainingRunning = false;
            _trainingTimer = 0f;
            _trainingTaps = 0;

            // Base tap value
            _baseTapValue = Mathf.RoundToInt(1 * _countMultiplier);

            // Auto rate
            _autoRateBase = stageIndex >= 1 ? 1f * _speedMultiplier : 0f;
            _autoUnlocked = stageIndex >= 1;

            // Feature unlocks
            _tournamentUnlocked = stageIndex >= 2;
            _trainingUnlocked = stageIndex >= 3;
            _shihanTestUnlocked = stageIndex >= 4;

            // Tech unlocks persist across stages (intentional: player keeps skills)

            _stageTarget = StageTargets[Mathf.Clamp(stageIndex, 0, StageTargets.Length - 1)];

            _isActive = true;
            NotifyUI();
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        void Update()
        {
            if (!_isActive) return;

            // Combo timeout
            if (_combo > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    _combo = 0;
                    NotifyComboUI();
                }
            }

            // Auto MP
            if (_autoUnlocked && _autoRateBase > 0)
            {
                _autoTimer += Time.deltaTime;
                if (_autoTimer >= 1f)
                {
                    float totalRate = (_autoRateBase + _autoRateBonus) * (_shihanCleared ? 2f : 1f);
                    long autoGain = Mathf.RoundToInt(totalRate);
                    AddMP(autoGain);
                    _autoTimer -= 1f;
                    _gameManager.UpdateAutoRateDisplay(totalRate);
                }
            }

            // Training timer
            if (_trainingRunning)
            {
                _trainingTimer -= Time.deltaTime;
                _gameManager.UpdateTrainingTimer(true, Mathf.Max(0f, _trainingTimer), _trainingTaps, TrainingTapGoal);
                if (_trainingTimer <= 0f)
                {
                    EndTraining(false);
                }
            }

            // Tap detection (click on tap area)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 worldPos = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                // Tap area is the central area - check if within radius 2.0 units of center
                if (worldPos.magnitude < 2.5f && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    OnTap();
                }
            }
        }

        void OnTap()
        {
            if (!_isActive) return;

            // Combo update
            _combo++;
            _comboTimer = ComboTimeout;
            float multiplier = GetComboMultiplier();

            // Training tap count
            if (_trainingRunning)
            {
                _trainingTaps++;
                if (_trainingTaps >= TrainingTapGoal)
                {
                    EndTraining(true);
                }
            }

            // Calculate MP gain
            long gain = Mathf.RoundToInt(_baseTapValue * multiplier);
            // Apply tech bonuses
            if (_techUnlocked[0]) gain += TechTapBonus[0]; // seiken
            if (_techUnlocked[1]) gain += TechTapBonus[1]; // mawashi
            if (_techUnlocked[2]) gain += TechTapBonus[2]; // tohou

            AddMP(gain);
            NotifyComboUI();

            // Visual feedback: pulse
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = StartCoroutine(PulseArtist());
        }

        IEnumerator PulseArtist()
        {
            if (_martialArtistRenderer == null) yield break;
            var t = _martialArtistRenderer.transform;
            t.localScale = Vector3.one * 1.15f;
            yield return new WaitForSeconds(0.05f);
            t.localScale = Vector3.one;
        }

        float GetComboMultiplier()
        {
            if (_combo >= 60) return 5f;
            if (_combo >= 30) return 3f;
            if (_combo >= 10) return 2f;
            return 1f;
        }

        void AddMP(long amount)
        {
            _mp += amount;
            _gameManager.UpdateMPDisplay(_mp, _stageTarget);

            // Update rank display
            string rankName = GetCurrentRankName();
            _gameManager.UpdateRankDisplay(rankName);

            // Check stage clear
            if (_mp >= _stageTarget)
            {
                _isActive = false;
                _gameManager.OnStageClear();
            }
        }

        string GetCurrentRankName()
        {
            if (_stageTarget <= 0) return RankNames[0];
            float progress = (float)_mp / _stageTarget;
            int rankIndex = Mathf.Min(_stageIndex + Mathf.FloorToInt(progress), RankNames.Length - 1);
            return RankNames[Mathf.Clamp(rankIndex, 0, RankNames.Length - 1)];
        }

        // --- Tech System ---
        public void BuyTech(int techIndex)
        {
            if (!_isActive) return;
            if (_techUnlocked[techIndex]) return;
            if (techIndex == 2) return; // tohou is event-only
            if (techIndex == 3) return; // shihan is test-only

            if (_mp >= TechCosts[techIndex])
            {
                _mp -= TechCosts[techIndex];
                _techUnlocked[techIndex] = true;
                NotifyTechFlash();
                _gameManager.UpdateMPDisplay(_mp, _stageTarget);
                NotifyUI();
            }
        }

        IEnumerator TechFlashRoutine()
        {
            if (_martialArtistRenderer == null) yield break;
            _martialArtistRenderer.color = new Color(0.5f, 1f, 0.5f);
            yield return new WaitForSeconds(0.3f);
            _martialArtistRenderer.color = _defaultColor;
        }

        void NotifyTechFlash()
        {
            StartCoroutine(TechFlashRoutine());
        }

        // --- Tournament System ---
        public void EnterTournament()
        {
            if (!_isActive || !_tournamentUnlocked) return;
            if (_tournamentRunning) return;
            long cost = 500;
            if (_mp < cost) return;

            _mp -= cost;
            _tournamentRunning = true;
            _gameManager.UpdateMPDisplay(_mp, _stageTarget);
            StartCoroutine(TournamentRoutine());
        }

        IEnumerator TournamentRoutine()
        {
            yield return new WaitForSeconds(2f);
            _tournamentRunning = false;
            if (!_isActive) yield break;
            float winChance = 0.6f - _complexityFactor * 0.2f; // harder = slightly lower base
            bool won = Random.value < winChance;
            if (won)
            {
                long reward = 1000 * Mathf.RoundToInt(_countMultiplier);
                AddMP(reward);
                StartCoroutine(WinFlashRoutine());
            }
        }

        IEnumerator WinFlashRoutine()
        {
            if (_camera != null)
            {
                var origPos = _camera.transform.localPosition;
                for (int i = 0; i < 8; i++)
                {
                    _camera.transform.localPosition = origPos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
                    yield return new WaitForSeconds(0.03f);
                }
                _camera.transform.localPosition = origPos;
            }
            if (_martialArtistRenderer != null)
            {
                _martialArtistRenderer.color = new Color(1f, 0.9f, 0f);
                yield return new WaitForSeconds(0.3f);
                _martialArtistRenderer.color = _defaultColor;
            }
        }

        // --- Intensive Training ---
        public void StartIntensiveTraining()
        {
            if (!_isActive || !_trainingUnlocked) return;
            if (_trainingRunning || _techUnlocked[2]) return;

            _trainingRunning = true;
            _trainingTimer = TrainingDuration;
            _trainingTaps = 0;
        }

        void EndTraining(bool success)
        {
            _trainingRunning = false;
            _trainingTimer = 0f;
            _gameManager.UpdateTrainingTimer(false, 0f, 0, TrainingTapGoal);
            if (success)
            {
                _techUnlocked[2] = true;
                NotifyTechFlash();
                NotifyUI();
            }
        }

        // --- Shihan Test ---
        public void StartShihanTest()
        {
            if (!_isActive || !_shihanTestUnlocked) return;
            if (_shihanCleared) return;
            // Require all 3 normal techs unlocked
            if (!_techUnlocked[0] || !_techUnlocked[1] || !_techUnlocked[2]) return;
            long cost = 5000;
            if (_mp < cost) return;

            _mp -= cost;
            _shihanCleared = true;
            _techUnlocked[3] = true;
            // autoRate bonus x2 handled via flag
            _gameManager.UpdateMPDisplay(_mp, _stageTarget);
            NotifyTechFlash();
            NotifyUI();
        }

        void NotifyComboUI()
        {
            _gameManager.UpdateComboDisplay(_combo, GetComboMultiplier());
        }

        void NotifyUI()
        {
            bool[] affordable = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                affordable[i] = (i < TechCosts.Length) && (_mp >= TechCosts[i]);
            }
            _gameManager.UpdateTechButtons(_techUnlocked, affordable, _autoUnlocked, _tournamentUnlocked, _trainingUnlocked, _shihanTestUnlocked);
        }
    }
}
