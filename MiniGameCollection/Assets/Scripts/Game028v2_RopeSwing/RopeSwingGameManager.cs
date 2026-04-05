using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game028v2_RopeSwing
{
    public enum RopeSwingState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class RopeSwingGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] RopeController _ropeController;
        [SerializeField] PlatformManager _platformManager;
        [SerializeField] RopeSwingUI _ui;

        public RopeSwingState State { get; private set; } = RopeSwingState.WaitingInstruction;

        int _score;
        int _currentStage;
        int _comboCount;
        float _comboMultiplier = 1f;
        float _stageStartTime;
        bool _isActive;
        Coroutine _autoAdvanceCo;

        void Start()
        {
            _instructionPanel.Show(
                "028v2",
                "RopeSwing",
                "ロープを掴んで振り子で飛び移るアクションゲーム！",
                "画面をタップしてロープを掴み、離して足場に飛び移ろう",
                "全ての足場を渡りきって、ゴールに着地しよう！"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _isActive = true;
            State = RopeSwingState.Playing;

            _ui.Initialize(this);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 3, complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 5, complexityFactor = 0.1f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 7, complexityFactor = 0.3f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 1.4f, countMultiplier = 9, complexityFactor = 0.5f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 1.6f, countMultiplier = 12, complexityFactor = 0.7f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == RopeSwingState.Clear) return;
            _currentStage = stageIndex + 1;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _stageStartTime = Time.time;
            State = RopeSwingState.Playing;

            var config = _stageManager.GetCurrentStageConfig();
            _platformManager.SetupStage(config, _currentStage);
            _ropeController.SetupStage(config, _currentStage);
            _ropeController.PlaceOnFirstPlatform();
            _ropeController.SetActive(true);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            _ropeController.SetActive(false);
            State = RopeSwingState.Clear;
            _ui.ShowFinalClear(_score);
        }

        // Called by RopeController on landing
        public void OnLanding(float landingAccuracy)
        {
            if (State != RopeSwingState.Playing) return;

            string grade;
            int baseScore;
            if (landingAccuracy <= 0.5f)
            {
                grade = "Perfect!";
                baseScore = 200;
            }
            else if (landingAccuracy <= 0.7f)
            {
                grade = "Good!";
                baseScore = 100;
            }
            else
            {
                grade = "OK";
                baseScore = 50;
                _comboCount = 0;
                _comboMultiplier = 1f;
            }

            if (landingAccuracy <= 0.7f)
            {
                _comboCount++;
                if (_comboCount >= 4) _comboMultiplier = 3.0f;
                else if (_comboCount >= 3) _comboMultiplier = 2.0f;
                else if (_comboCount >= 2) _comboMultiplier = 1.5f;
                else _comboMultiplier = 1.0f;
            }

            int gained = Mathf.RoundToInt(baseScore * _comboMultiplier);
            _score += gained;
            _ui.UpdateScore(_score);
            _ui.ShowLandingFeedback(grade, _comboCount);

            // Scale pulse on player
            _ropeController.PlayLandingEffect();
        }

        // Called by RopeController when landing on goal platform
        public void OnGoalReached()
        {
            if (State != RopeSwingState.Playing) return;

            float elapsed = Time.time - _stageStartTime;
            int timeBonus = Mathf.Max(0, Mathf.RoundToInt(1000f - elapsed * 20f));
            int stageBonus = 500 * _currentStage;
            _score += stageBonus + timeBonus;

            State = RopeSwingState.StageClear;
            _ropeController.SetActive(false);
            _ui.UpdateScore(_score);
            _ui.ShowStageClear(_currentStage, stageBonus + timeBonus);
            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _autoAdvanceCo = StartCoroutine(AutoAdvanceStage());
        }

        IEnumerator AutoAdvanceStage()
        {
            yield return new WaitForSeconds(2f);
            if (State == RopeSwingState.StageClear)
                _stageManager.CompleteCurrentStage();
        }

        // Called by RopeController on fall
        public void OnPlayerFall()
        {
            if (State != RopeSwingState.Playing) return;
            TriggerGameOver();
        }

        void TriggerGameOver()
        {
            if (State == RopeSwingState.GameOver) return;
            _ropeController.SetActive(false);
            State = RopeSwingState.GameOver;
            StartCoroutine(GameOverSequence());
        }

        IEnumerator GameOverSequence()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 origPos = cam.transform.position;
                float elapsed = 0f;
                while (elapsed < 0.4f)
                {
                    elapsed += Time.deltaTime;
                    float shake = 0.3f * (1f - elapsed / 0.4f);
                    cam.transform.position = origPos + (Vector3)Random.insideUnitCircle * shake;
                    yield return null;
                }
                cam.transform.position = origPos;
            }
            _ui.ShowGameOver(_score);
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
