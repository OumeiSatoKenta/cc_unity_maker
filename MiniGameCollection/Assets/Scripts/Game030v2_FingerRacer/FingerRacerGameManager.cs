using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game030v2_FingerRacer
{
    public enum FingerRacerState
    {
        WaitingInstruction,
        Drawing,
        Racing,
        StageClear,
        Clear,
        GameOver
    }

    public class FingerRacerGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CourseDrawer _courseDrawer;
        [SerializeField] CarController _carController;
        [SerializeField] RivalCarController _rivalCarController;
        [SerializeField] FingerRacerUI _ui;

        public FingerRacerState State { get; private set; } = FingerRacerState.WaitingInstruction;

        int _score;
        int _comboCount;
        float _comboMultiplier = 1f;
        int _currentStage;
        int _courseOutCount;
        float _raceTimer;
        bool _isActive;
        Coroutine _autoAdvanceCo;

        const int MaxCourseOut = 3;
        const int BaseFinishScore = 10000;

        void Start()
        {
            _instructionPanel.Show(
                "030v2",
                "FingerRacer",
                "指でコースを描いて車をゴールへ導こう！",
                "画面をドラッグしてコースを描き、スタートボタンでレース開始！\n直線でタップするとブースト加速！",
                "コースアウト3回以内でゴールに到達しよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _courseOutCount = 0;
            _raceTimer = 0f;
            _isActive = true;
            State = FingerRacerState.Drawing;

            _ui.Initialize(this);
            _ui.UpdateCourseOut(_courseOutCount, MaxCourseOut);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 2, complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.25f, countMultiplier = 3, complexityFactor = 0.3f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 4, complexityFactor = 0.5f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 1.75f, countMultiplier = 5, complexityFactor = 0.7f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 6, complexityFactor = 1.0f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void Update()
        {
            if (!_isActive || State != FingerRacerState.Racing) return;
            _raceTimer += Time.deltaTime;
            _ui.UpdateTime(_raceTimer);
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == FingerRacerState.Clear) return;
            _currentStage = stageIndex + 1;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _courseOutCount = 0;
            State = FingerRacerState.Drawing;

            var config = _stageManager.GetCurrentStageConfig();
            _courseDrawer.SetupStage(config, _currentStage);
            _carController.SetupStage(config, _currentStage);

            // Stage 5: activate rival car
            if (_rivalCarController != null)
                _rivalCarController.gameObject.SetActive(_currentStage >= 5);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.UpdateCourseOut(_courseOutCount, MaxCourseOut);
            _ui.HideStageClear();
            _ui.ShowDrawingUI(true);
        }

        void OnAllStagesCleared()
        {
            State = FingerRacerState.Clear;
            _isActive = false;
            _ui.ShowFinalClear(_score);
        }

        public void OnStartRacePressed()
        {
            if (State != FingerRacerState.Drawing) return;
            var points = _courseDrawer.GetCoursePoints();
            if (points == null || points.Length < 5) return;

            _raceTimer = 0f;
            State = FingerRacerState.Racing;
            _ui.ShowDrawingUI(false);
            _carController.StartRace(points);
            if (_rivalCarController != null && _rivalCarController.gameObject.activeSelf)
                _rivalCarController.StartRival(_carController.transform.position, _courseDrawer.GoalPosition);
        }

        public void OnCheckpointPassed(int checkpointIndex)
        {
            if (State != FingerRacerState.Racing) return;
            _score += Mathf.RoundToInt(100f * _comboMultiplier);
            _ui.UpdateScore(_score);
        }

        public void OnBoostSuccess()
        {
            if (State != FingerRacerState.Racing) return;
            _comboCount++;
            if (_comboCount >= 3) _comboMultiplier = 2.0f;
            else if (_comboCount >= 2) _comboMultiplier = 1.5f;
            else _comboMultiplier = 1.0f;

            int gained = Mathf.RoundToInt(50f * _comboMultiplier);
            _score += gained;
            _ui.UpdateScore(_score);
            _ui.ShowCombo(_comboCount, _comboMultiplier);
        }

        public void OnBoostFail()
        {
            _comboCount = 0;
            _comboMultiplier = 1f;
            _ui.ShowCombo(0, 1f);
        }

        public void OnCourseOut()
        {
            if (State != FingerRacerState.Racing) return;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _courseOutCount++;
            _ui.UpdateCourseOut(_courseOutCount, MaxCourseOut);
            _ui.ShowCombo(0, 1f);
            StartCoroutine(CameraShake(0.3f, 0.4f));

            if (_courseOutCount >= MaxCourseOut)
            {
                TriggerGameOver();
            }
        }

        public void OnGoalReached()
        {
            if (State != FingerRacerState.Racing) return;
            State = FingerRacerState.StageClear;

            // Time-based score
            int timeBonus = Mathf.Max(0, BaseFinishScore - Mathf.RoundToInt(_raceTimer * 100f));
            // Perfect bonus
            int perfectBonus = (_courseOutCount == 0) ? 1000 : 0;
            int stageScore = timeBonus + perfectBonus;
            _score += stageScore;
            _ui.UpdateScore(_score);
            _ui.ShowStageClear(_currentStage, stageScore, perfectBonus > 0);

            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _autoAdvanceCo = StartCoroutine(AutoAdvanceStage());
        }

        IEnumerator AutoAdvanceStage()
        {
            yield return new WaitForSeconds(2.5f);
            if (State == FingerRacerState.StageClear)
                _stageManager.CompleteCurrentStage();
        }

        void TriggerGameOver()
        {
            if (State == FingerRacerState.GameOver) return;
            if (_autoAdvanceCo != null) { StopCoroutine(_autoAdvanceCo); _autoAdvanceCo = null; }
            _carController.StopRace();
            _rivalCarController?.StopRival();
            State = FingerRacerState.GameOver;
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

        public void NotifyBoostUpdate(int current, int max)
        {
            _ui?.UpdateBoost(current, max);
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
