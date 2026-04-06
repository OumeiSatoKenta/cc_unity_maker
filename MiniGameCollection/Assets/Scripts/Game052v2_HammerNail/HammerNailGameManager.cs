using UnityEngine;
using UnityEngine.InputSystem;

namespace Game052v2_HammerNail
{
    public enum GameState { Idle, Playing, StageClear, AllClear, GameOver }

    public class HammerNailGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] NailManager _nailManager;
        [SerializeField] TimingGauge _timingGauge;
        [SerializeField] HammerNailUI _ui;

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }

        int _comboCount;
        int _missCount;
        const int MaxMiss = 3;

        void Start()
        {
            _instructionPanel.Show(
                "052v2",
                "HammerNail",
                "リズムよくタップして釘を打ち込もう",
                "ゲージがPERFECTゾーンにある時にタップ！タイミングで釘の沈み具合が変わるよ",
                "全ての釘を打ち込んでステージクリア！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            _instructionPanel.OnDismissed -= StartGame;
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_nailManager != null)
                _nailManager.OnAllNailsDriven -= OnAllNailsDriven_Handler;
        }

        void StartGame()
        {
            Score = 0;
            _comboCount = 0;
            _missCount = 0;

            _nailManager.OnAllNailsDriven += OnAllNailsDriven_Handler;

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { stageName = "Stage 1", speedMultiplier = 1.0f, countMultiplier = 3, complexityFactor = 0.0f },
                new StageManager.StageConfig { stageName = "Stage 2", speedMultiplier = 1.5f, countMultiplier = 5, complexityFactor = 0.2f },
                new StageManager.StageConfig { stageName = "Stage 3", speedMultiplier = 1.5f, countMultiplier = 5, complexityFactor = 0.4f },
                new StageManager.StageConfig { stageName = "Stage 4", speedMultiplier = 2.0f, countMultiplier = 7, complexityFactor = 0.6f },
                new StageManager.StageConfig { stageName = "Stage 5", speedMultiplier = 2.2f, countMultiplier = 8, complexityFactor = 0.8f },
            });

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            State = GameState.Playing;
            _missCount = 0;
            _comboCount = 0;

            var config = _stageManager.GetCurrentStageConfig();
            int stageNumber = stageIndex + 1;

            _nailManager.SetupStage(config.countMultiplier, config.complexityFactor, stageIndex);
            _timingGauge.SetupStage(config.speedMultiplier, config.complexityFactor);
            _timingGauge.StartGauge();

            _ui.UpdateStage(stageNumber, 5);
            _ui.UpdateScore(Score);
            _ui.UpdateCombo(_comboCount);
            _ui.UpdateMiss(_missCount, MaxMiss);
            _ui.UpdateRemainingNails(_nailManager.RemainingNails);
            _ui.HideStageClear();
            _ui.HideGameOver();
            _ui.HideAllClear();
        }

        void OnAllStagesCleared()
        {
            State = GameState.AllClear;
            _timingGauge.StopGauge();
            _ui.ShowAllClear(Score);
        }

        void Update()
        {
            if (State != GameState.Playing) return;

            bool tapped = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (!tapped && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                tapped = true;

            if (tapped)
            {
                OnHammerSwing();
            }
        }

        void OnAllNailsDriven_Handler()
        {
            if (State != GameState.Playing) return;
            State = GameState.StageClear;
            _timingGauge.StopGauge();
            _ui.ShowStageClear(Score);
        }

        void OnHammerSwing()
        {
            var nail = _nailManager.GetCurrentNail();
            if (nail == null) return;

            HitResult result = _timingGauge.GetHitResult();

            _nailManager.PlayHammerAnim(_nailManager.transform.position);
            _nailManager.HitCurrentNail(result);

            if (result == HitResult.Miss)
            {
                _nailManager.TiltCurrentNail();
                _comboCount = 0;
                _missCount++;
                _ui.ShowJudgment("MISS", false);
                _ui.UpdateMiss(_missCount, MaxMiss);
                _ui.UpdateCombo(0);
                CameraShake();

                if (_missCount >= MaxMiss)
                {
                    State = GameState.GameOver;
                    _timingGauge.StopGauge();
                    _ui.ShowGameOver(Score);
                    return;
                }
            }
            else
            {
                int baseScore = result == HitResult.Perfect ? 100 : 50;
                if (result == HitResult.Perfect)
                {
                    _comboCount++;
                    baseScore += _comboCount * 50;
                }
                else
                {
                    _comboCount = 0;
                }

                float multiplier = _comboCount >= 10 ? 2.0f : (_comboCount >= 5 ? 1.5f : 1.0f);
                int gained = Mathf.RoundToInt(baseScore * multiplier);
                Score += gained;

                _ui.UpdateScore(Score);
                _ui.UpdateCombo(_comboCount);
                _ui.ShowJudgment(result == HitResult.Perfect ? "PERFECT!" : "GOOD", true);
            }

            _ui.UpdateRemainingNails(_nailManager.RemainingNails);
        }

        Coroutine _shakeCoroutine;

        void CameraShake()
        {
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeCoroutine());
        }

        System.Collections.IEnumerator ShakeCoroutine()
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 origPos = cam.transform.localPosition;
            float duration = 0.3f;
            float magnitude = 0.15f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cam.transform.localPosition = origPos + (Vector3)Random.insideUnitCircle * magnitude;
                yield return null;
            }
            cam.transform.localPosition = origPos;
            _shakeCoroutine = null;
        }

        public void GoNextStage()
        {
            if (State != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void GoToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
