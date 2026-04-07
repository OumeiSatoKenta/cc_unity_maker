using UnityEngine;
using UnityEngine.UI;

namespace Game080v2_FreqFight
{
    public class FreqFightManager : MonoBehaviour
    {
        public enum BattlePhase { Attack, Defense }

        FreqFightGameManager _gameManager;

        [SerializeField] Slider _playerFreqSlider;
        [SerializeField] Slider _playerFreqSlider2;

        // Stage parameters
        float _bpm = 60f;
        float _freqMin = 200f;
        float _freqMax = 400f;
        float _enemyMaxHp = 200f;
        bool _hasDefensePhase = false;
        bool _hasFakeout = false;
        int _enemyCount = 1;

        // Runtime state
        float _beatInterval;
        float _beatTimer;
        float _enemyFreq;
        float _playerFreq;
        float _enemyFreq2;
        float _playerFreq2;
        float _enemyHp;
        float _enemyHp2;
        float _playerHp = 100f;
        int _combo;
        int _totalScore;
        BattlePhase _currentPhase = BattlePhase.Attack;
        int _beatCount;
        bool _isActive;
        bool _fakeoutTriggered;

        // Enemy attack damage per defense phase
        const float EnemyAttackDamage = 15f;
        const float DefensedDamage = 5f;

        public int TotalScore => _totalScore;

        void Awake()
        {
            _gameManager = GetComponentInParent<FreqFightGameManager>();
        }

        public void ResetScore()
        {
            _totalScore = 0;
            _combo = 0;
            _playerHp = 100f;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        public void SetupStage(StageManager.StageConfig config, int stageIndex)
        {
            // Determine stage parameters from stageIndex
            switch (stageIndex)
            {
                case 0:
                    _bpm = 60f; _freqMin = 200f; _freqMax = 400f;
                    _enemyMaxHp = 200f; _hasDefensePhase = false; _hasFakeout = false; _enemyCount = 1;
                    break;
                case 1:
                    _bpm = 80f; _freqMin = 200f; _freqMax = 600f;
                    _enemyMaxHp = 350f; _hasDefensePhase = true; _hasFakeout = false; _enemyCount = 1;
                    break;
                case 2:
                    _bpm = 100f; _freqMin = 150f; _freqMax = 800f;
                    _enemyMaxHp = 500f; _hasDefensePhase = true; _hasFakeout = true; _enemyCount = 1;
                    break;
                case 3:
                    _bpm = 120f; _freqMin = 100f; _freqMax = 1000f;
                    _enemyMaxHp = 400f; _hasDefensePhase = true; _hasFakeout = true; _enemyCount = 2;
                    break;
                case 4:
                    _bpm = 140f; _freqMin = 100f; _freqMax = 1200f;
                    _enemyMaxHp = 800f; _hasDefensePhase = true; _hasFakeout = true; _enemyCount = 1;
                    break;
            }

            _beatInterval = 60f / _bpm;
            _beatTimer = _beatInterval;
            _beatCount = 0;
            _combo = 0;
            _enemyHp = _enemyMaxHp;
            _enemyHp2 = _enemyMaxHp;
            _playerFreq = (_freqMin + _freqMax) * 0.5f;
            _playerFreq2 = (_freqMin + _freqMax) * 0.5f;
            _currentPhase = BattlePhase.Attack;
            _fakeoutTriggered = false;

            // Set slider ranges
            if (_playerFreqSlider != null)
            {
                _playerFreqSlider.minValue = 0f;
                _playerFreqSlider.maxValue = 1f;
                _playerFreqSlider.value = 0.5f;
                _playerFreqSlider.onValueChanged.RemoveAllListeners();
                _playerFreqSlider.onValueChanged.AddListener(OnSlider1Changed);
            }
            if (_playerFreqSlider2 != null)
            {
                bool show2 = _enemyCount >= 2;
                _playerFreqSlider2.gameObject.SetActive(show2);
                if (show2)
                {
                    _playerFreqSlider2.minValue = 0f;
                    _playerFreqSlider2.maxValue = 1f;
                    _playerFreqSlider2.value = 0.5f;
                    _playerFreqSlider2.onValueChanged.RemoveAllListeners();
                    _playerFreqSlider2.onValueChanged.AddListener(OnSlider2Changed);
                }
            }

            // Generate first enemy frequency
            _enemyFreq = GenerateEnemyFreq();
            _enemyFreq2 = GenerateEnemyFreq();

            UpdatePhaseDisplay();
            UpdateHpDisplays();
            UpdateEnemyFreqMarkers();

            _isActive = true;
        }

        float GenerateEnemyFreq()
        {
            return Random.Range(_freqMin, _freqMax);
        }

        void OnSlider1Changed(float value)
        {
            _playerFreq = _freqMin + value * (_freqMax - _freqMin);
        }

        void OnSlider2Changed(float value)
        {
            _playerFreq2 = _freqMin + value * (_freqMax - _freqMin);
        }

        void Update()
        {
            if (!_isActive) return;

            _beatTimer -= Time.deltaTime;

            // Fakeout: change enemy freq shortly before beat
            if (_hasFakeout && !_fakeoutTriggered && _beatTimer < _beatInterval * 0.15f)
            {
                if (Random.value < 0.35f)
                {
                    _enemyFreq = GenerateEnemyFreq();
                    if (_enemyCount >= 2) _enemyFreq2 = GenerateEnemyFreq();
                    UpdateEnemyFreqMarkers();
                    _fakeoutTriggered = true;
                }
            }

            if (_beatTimer <= 0f)
            {
                _beatTimer = _beatInterval;
                _fakeoutTriggered = false;
                OnBeat();
            }
        }

        void OnBeat()
        {
            _beatCount++;
            _gameManager.PulseBeat();

            if (_currentPhase == BattlePhase.Attack)
            {
                // Judge enemy 1
                Judge(_playerFreq, _enemyFreq, 0);

                // Judge enemy 2 (stage 4)
                if (_isActive && _enemyCount >= 2)
                    Judge(_playerFreq2, _enemyFreq2, 1);

                // Switch to defense every 4 beats if hasDefensePhase
                if (_hasDefensePhase && _beatCount % 4 == 0)
                {
                    _currentPhase = BattlePhase.Defense;
                    UpdatePhaseDisplay();
                }
                else
                {
                    // Prepare next enemy freq
                    _enemyFreq = GenerateEnemyFreq();
                    if (_enemyCount >= 2) _enemyFreq2 = GenerateEnemyFreq();
                    UpdateEnemyFreqMarkers();
                }
            }
            else
            {
                // Defense phase
                float centerFreq = (_freqMin + _freqMax) * 0.5f;
                float tolerance = (_freqMax - _freqMin) * 0.15f;
                bool defended1 = Mathf.Abs(_playerFreq - centerFreq) <= tolerance;
                bool defended2 = !(_enemyCount >= 2) || Mathf.Abs(_playerFreq2 - centerFreq) <= tolerance;

                float damage = EnemyAttackDamage;
                if (defended1 && defended2) damage = DefensedDamage;
                else if (defended1 || defended2) damage = (EnemyAttackDamage + DefensedDamage) * 0.5f;

                _playerHp -= damage;
                _playerHp = Mathf.Max(0f, _playerHp);
                _gameManager.UpdatePlayerHp(_playerHp / 100f);

                if (_playerHp <= 0f)
                {
                    _isActive = false;
                    _gameManager.OnGameOver();
                    return;
                }

                _currentPhase = BattlePhase.Attack;
                UpdatePhaseDisplay();
                _enemyFreq = GenerateEnemyFreq();
                if (_enemyCount >= 2) _enemyFreq2 = GenerateEnemyFreq();
                UpdateEnemyFreqMarkers();
            }
        }

        void Judge(float playerFreq, float enemyFreq, int enemyIndex)
        {
            float diff = Mathf.Abs(playerFreq - enemyFreq);
            string judgement;
            float damage;
            Color color;

            if (diff <= 5f)
            {
                judgement = "Perfect!";
                damage = 25f;
                color = new Color(1f, 0.85f, 0f);
                _combo++;
            }
            else if (diff <= 15f)
            {
                judgement = "Great!";
                damage = 15f;
                color = new Color(0.3f, 1f, 0.5f);
                _combo++;
            }
            else if (diff <= 30f)
            {
                judgement = "Good";
                damage = 5f;
                color = Color.white;
                _combo++;
            }
            else
            {
                judgement = "Miss";
                damage = 0f;
                color = new Color(1f, 0.3f, 0.3f);
                _combo = 0;
            }

            // Calculate combo multiplier
            float multiplier = 1f;
            if (diff <= 5f)
                multiplier = Mathf.Min(1f + _combo * 0.2f, 4f);
            else if (diff <= 15f)
                multiplier = Mathf.Min(1f + _combo * 0.1f, 2.5f);

            int scoreGain = Mathf.RoundToInt(damage * multiplier);
            _totalScore += scoreGain;

            // Apply damage to correct enemy
            if (enemyIndex == 0)
            {
                _enemyHp -= damage;
                _enemyHp = Mathf.Max(0f, _enemyHp);
                _gameManager.UpdateEnemyHp(_enemyHp / _enemyMaxHp, 0);
                if (damage > 0f) _gameManager.ShakeEnemy(0);

                if (_enemyHp <= 0f)
                {
                    if (_enemyCount < 2 || _enemyHp2 <= 0f)
                    {
                        _isActive = false;
                        _gameManager.OnStageClear();
                        return;
                    }
                }
            }
            else
            {
                _enemyHp2 -= damage;
                _enemyHp2 = Mathf.Max(0f, _enemyHp2);
                _gameManager.UpdateEnemyHp(_enemyHp2 / _enemyMaxHp, 1);
                if (damage > 0f) _gameManager.ShakeEnemy(1);

                if (_enemyHp2 <= 0f && _enemyHp <= 0f)
                {
                    _isActive = false;
                    _gameManager.OnStageClear();
                    return;
                }
            }

            _gameManager.ShowJudgement(judgement, color);
            _gameManager.UpdateComboDisplay(_combo);
            _gameManager.UpdateScoreDisplay(_totalScore);
        }

        void UpdatePhaseDisplay()
        {
            string phaseText = _currentPhase == BattlePhase.Attack ? "攻撃フェーズ" : "防御フェーズ";
            _gameManager.UpdatePhase(phaseText);
        }

        void UpdateHpDisplays()
        {
            _gameManager.UpdatePlayerHp(_playerHp / 100f);
            _gameManager.UpdateEnemyHp(_enemyHp / _enemyMaxHp, 0);
            if (_enemyCount >= 2)
                _gameManager.UpdateEnemyHp(_enemyHp2 / _enemyMaxHp, 1);
        }

        void UpdateEnemyFreqMarkers()
        {
            float range = _freqMax - _freqMin;
            float normalizedFreq1 = range > 0f ? (_enemyFreq - _freqMin) / range : 0.5f;
            _gameManager.UpdateEnemyFreqMarker(normalizedFreq1, 0);
            if (_enemyCount >= 2)
            {
                float normalizedFreq2 = range > 0f ? (_enemyFreq2 - _freqMin) / range : 0.5f;
                _gameManager.UpdateEnemyFreqMarker(normalizedFreq2, 1);
            }
        }
    }
}
