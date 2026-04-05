using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game029v2_MeteorShield
{
    public enum MeteorShieldState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class MeteorShieldGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] ShieldController _shieldController;
        [SerializeField] MeteorSpawner _meteorSpawner;
        [SerializeField] MeteorShieldUI _ui;

        public MeteorShieldState State { get; private set; } = MeteorShieldState.WaitingInstruction;

        int _score;
        int _comboCount;
        float _comboMultiplier = 1f;
        int _currentStage;
        float _starHP = 100f;
        float _survivalTimer;
        bool _isActive;
        Coroutine _autoAdvanceCo;

        const float MaxHP = 100f;
        const float SurvivalBonusInterval = 30f;

        void Start()
        {
            _instructionPanel.Show(
                "029v2",
                "MeteorShield",
                "落下する隕石をシールドで弾いて星を守るディフェンスゲーム！",
                "画面をドラッグしてシールドを左右に動かそう",
                "星のHPがゼロになる前にできるだけ長く守り続けよう！"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _starHP = MaxHP;
            _survivalTimer = 0f;
            _isActive = true;
            State = MeteorShieldState.Playing;

            _ui.Initialize(this);
            _ui.UpdateHP(_starHP, MaxHP);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.25f, countMultiplier = 2, complexityFactor = 0.3f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.4f, countMultiplier = 2, complexityFactor = 0.5f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 1.6f, countMultiplier = 3, complexityFactor = 0.7f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 1.8f, countMultiplier = 4, complexityFactor = 1.0f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void Update()
        {
            if (!_isActive || State != MeteorShieldState.Playing) return;

            _survivalTimer += Time.deltaTime;
            _ui.UpdateTime(_survivalTimer);

            // 生存ボーナス（30秒ごと）
            if (_survivalTimer > 0f && Mathf.FloorToInt(_survivalTimer / SurvivalBonusInterval) >
                Mathf.FloorToInt((_survivalTimer - Time.deltaTime) / SurvivalBonusInterval))
            {
                int bonus = Mathf.RoundToInt(300f * _comboMultiplier);
                _score += bonus;
                _ui.UpdateScore(_score);
                _ui.ShowSurvivalBonus(bonus);
            }
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == MeteorShieldState.Clear) return;
            _currentStage = stageIndex + 1;
            _comboCount = 0;
            _comboMultiplier = 1f;
            State = MeteorShieldState.Playing;

            var config = _stageManager.GetCurrentStageConfig();
            _meteorSpawner.SetupStage(config, _currentStage);
            _shieldController.SetActive(true);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            _shieldController.SetActive(false);
            _meteorSpawner.StopSpawning();
            State = MeteorShieldState.Clear;
            _ui.ShowFinalClear(_score);
        }

        // Called by ShieldController when meteor deflected
        public void OnMeteorDeflected(bool isChainKill)
        {
            if (State != MeteorShieldState.Playing) return;

            _comboCount++;
            if (_comboCount >= 10) _comboMultiplier = 3.0f;
            else if (_comboCount >= 5) _comboMultiplier = 2.0f;
            else if (_comboCount >= 3) _comboMultiplier = 1.5f;
            else _comboMultiplier = 1.0f;

            int gained = Mathf.RoundToInt(20f * _comboMultiplier);
            if (isChainKill) gained += Mathf.RoundToInt(100f * _comboMultiplier);
            _score += gained;

            _ui.UpdateScore(_score);
            _ui.ShowCombo(_comboCount, _comboMultiplier);
        }

        // Called by MeteorSpawner when meteor hits star
        public void OnStarHit(float damage)
        {
            if (State != MeteorShieldState.Playing) return;

            _comboCount = 0;
            _comboMultiplier = 1f;
            _starHP = Mathf.Max(0f, _starHP - damage);
            _ui.UpdateHP(_starHP, MaxHP);
            _ui.ShowCombo(0, 1f);
            StartCoroutine(CameraShake(0.3f, 0.4f));

            if (_starHP <= 0f)
            {
                TriggerGameOver();
            }
        }

        // Called by MeteorSpawner when stage time is up
        public void OnStageTimeUp()
        {
            if (State != MeteorShieldState.Playing) return;

            State = MeteorShieldState.StageClear;
            _shieldController.SetActive(false);
            _meteorSpawner.StopSpawning();

            int stageBonus = 300 * _currentStage;
            _score += stageBonus;
            _ui.UpdateScore(_score);
            _ui.ShowStageClear(_currentStage, stageBonus);

            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _autoAdvanceCo = StartCoroutine(AutoAdvanceStage());
        }

        IEnumerator AutoAdvanceStage()
        {
            yield return new WaitForSeconds(2f);
            if (State == MeteorShieldState.StageClear)
                _stageManager.CompleteCurrentStage();
        }

        void TriggerGameOver()
        {
            if (State == MeteorShieldState.GameOver) return;
            if (_autoAdvanceCo != null) { StopCoroutine(_autoAdvanceCo); _autoAdvanceCo = null; }
            _shieldController.SetActive(false);
            _meteorSpawner.StopSpawning();
            State = MeteorShieldState.GameOver;
            _isActive = false;
            StartCoroutine(GameOverSequence());
        }

        IEnumerator GameOverSequence()
        {
            yield return CameraShake(0.5f, 0.6f);
            _ui.ShowGameOver(_score);
        }

        IEnumerator CameraShake(float intensity, float duration)
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 origPos = cam.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float shake = intensity * (1f - elapsed / duration);
                cam.transform.position = origPos + (Vector3)Random.insideUnitCircle * shake;
                yield return null;
            }
            cam.transform.position = origPos;
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }
    }
}
