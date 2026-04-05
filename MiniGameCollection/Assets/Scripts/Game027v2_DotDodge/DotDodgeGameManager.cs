using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game027v2_DotDodge
{
    public enum DotDodgeState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class DotDodgeGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DotSpawner _dotSpawner;
        [SerializeField] DotDodgeUI _ui;
        [SerializeField] PlayerController _playerController;

        public DotDodgeState State { get; private set; } = DotDodgeState.WaitingInstruction;

        int _score;
        int _currentStage;
        float _comboMultiplier = 1f;
        int _comboCount = 0;
        float _lastNearMissTime = -999f;
        const float ComboWindow = 5f;

        float _survivalTime;
        Coroutine _stageTimerCoroutine;
        Coroutine _survivalScoreCoroutine;

        static readonly float[] StageDurations = { 15f, 20f, 25f, 30f, 35f };

        void Start()
        {
            _instructionPanel.Show(
                "027v2",
                "DotDodge",
                "画面を埋め尽くすドットを避け続けるサバイバル！",
                "画面をドラッグして青いプレイヤーを操作。赤いドットに当たるとゲームオーバー！",
                "全5ステージを生き延びてクリアを目指せ！ニアミスでボーナスポイントも狙え！"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _comboMultiplier = 1f;
            _comboCount = 0;
            _survivalTime = 0f;
            State = DotDodgeState.Playing;

            _ui.Initialize(this);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 2, complexityFactor = 0.2f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.4f, countMultiplier = 2, complexityFactor = 0.4f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 1.7f, countMultiplier = 3, complexityFactor = 0.6f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 1.0f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStage = stageIndex + 1;
            _comboMultiplier = 1f;
            _comboCount = 0;

            var config = _stageManager.GetCurrentStageConfig();
            _dotSpawner.SetupStage(config, _currentStage);
            _playerController.SetActive(true);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.UpdateTime(_survivalTime);
            _ui.UpdateCombo(_comboMultiplier);
            _ui.HideStageClear();

            if (_stageTimerCoroutine != null) StopCoroutine(_stageTimerCoroutine);
            if (_survivalScoreCoroutine != null) StopCoroutine(_survivalScoreCoroutine);

            float duration = (_currentStage - 1 < StageDurations.Length) ? StageDurations[_currentStage - 1] : 30f;
            _stageTimerCoroutine = StartCoroutine(StageTimer(duration));
            _survivalScoreCoroutine = StartCoroutine(SurvivalScoreLoop());
        }

        IEnumerator StageTimer(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (State == DotDodgeState.Playing)
                CompleteStageClear();
        }

        IEnumerator SurvivalScoreLoop()
        {
            while (State == DotDodgeState.Playing)
            {
                yield return new WaitForSeconds(0.1f);
                if (State != DotDodgeState.Playing) break;
                _survivalTime += 0.1f;
                _score += _currentStage;
                _ui.UpdateScore(_score);
                _ui.UpdateTime(_survivalTime);

                // Check if combo should reset
                if (_comboMultiplier > 1f && Time.time - _lastNearMissTime > ComboWindow)
                {
                    _comboMultiplier = 1f;
                    _comboCount = 0;
                    _ui.UpdateCombo(_comboMultiplier);
                }
            }
        }

        void CompleteStageClear()
        {
            if (_stageTimerCoroutine != null) StopCoroutine(_stageTimerCoroutine);
            if (_survivalScoreCoroutine != null) StopCoroutine(_survivalScoreCoroutine);
            _dotSpawner.StopSpawning();
            _playerController.SetActive(false);

            int bonus = 500 * _currentStage;
            _score += bonus;
            State = DotDodgeState.StageClear;
            _ui.UpdateScore(_score);
            _ui.ShowStageClear(_currentStage, bonus);
            StartCoroutine(AutoAdvanceStage());
        }

        IEnumerator AutoAdvanceStage()
        {
            yield return new WaitForSeconds(2f);
            if (State == DotDodgeState.StageClear)
                _stageManager.CompleteCurrentStage();
        }

        void OnAllStagesCleared()
        {
            _dotSpawner.StopSpawning();
            _playerController.SetActive(false);
            State = DotDodgeState.Clear;
            _ui.ShowFinalClear(_score, _survivalTime);
        }

        public void OnNearMiss()
        {
            if (State != DotDodgeState.Playing) return;

            _lastNearMissTime = Time.time;
            _comboCount++;

            if (_comboCount >= 5) _comboMultiplier = 3f;
            else if (_comboCount >= 3) _comboMultiplier = 2f;
            else if (_comboCount >= 1) _comboMultiplier = 1.5f;

            int bonus = Mathf.RoundToInt(30 * _comboMultiplier);
            _score += bonus;
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_comboMultiplier);
            _ui.ShowNearMissEffect();
        }

        public void OnPlayerHit()
        {
            if (State != DotDodgeState.Playing) return;
            TriggerGameOver();
        }

        void TriggerGameOver()
        {
            if (State == DotDodgeState.GameOver) return;
            if (_stageTimerCoroutine != null) StopCoroutine(_stageTimerCoroutine);
            if (_survivalScoreCoroutine != null) StopCoroutine(_survivalScoreCoroutine);
            _dotSpawner.StopSpawning();
            _playerController.SetActive(false);
            State = DotDodgeState.GameOver;
            StartCoroutine(GameOverSequence());
        }

        IEnumerator GameOverSequence()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 originalPos = cam.transform.position;
                float elapsed = 0f;
                while (elapsed < 0.4f)
                {
                    elapsed += Time.deltaTime;
                    float shakeAmount = 0.25f * (1f - elapsed / 0.4f);
                    cam.transform.position = originalPos + (Vector3)Random.insideUnitCircle * shakeAmount;
                    yield return null;
                }
                cam.transform.position = originalPos;
            }
            _ui.ShowGameOver(_score, _survivalTime);
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
