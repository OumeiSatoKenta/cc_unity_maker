using UnityEngine;
using System.Collections;

namespace Game032v2_SpinCutter
{
    public enum SpinCutterState
    {
        WaitingInstruction,
        WaitingLaunch,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class SpinCutterGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] SpinCutterMechanic _mechanic;
        [SerializeField] SpinCutterUI _ui;

        public SpinCutterState State { get; private set; } = SpinCutterState.WaitingInstruction;

        int _score;
        int _currentStage;
        Coroutine _autoAdvanceCo;

        void Start()
        {
            _instructionPanel.Show(
                "032v2",
                "SpinCutter",
                "回転する刃の軌道を調整して敵を一掃しよう",
                "スライダーで半径・速度を調整、ボタンで発射",
                "できるだけ少ない発射回数で全敵を倒そう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            State = SpinCutterState.WaitingLaunch;

            _ui.Initialize(this);
            _ui.UpdateScore(_score);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 3, complexityFactor = 0.0f,  stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 5, complexityFactor = 0.25f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.3f, countMultiplier = 6, complexityFactor = 0.4f,  stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 8, complexityFactor = 0.5f,  stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 1.7f, countMultiplier = 10, complexityFactor = 0.6f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == SpinCutterState.Clear) return;
            _currentStage = stageIndex + 1;
            State = SpinCutterState.WaitingLaunch;

            var config = _stageManager.GetCurrentStageConfig();
            _mechanic.SetupStage(config, stageIndex);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            State = SpinCutterState.Clear;
            _ui.ShowFinalClear(_score);
        }

        public void OnLaunchPressed()
        {
            if (State != SpinCutterState.WaitingLaunch && State != SpinCutterState.Playing) return;
            if (!_mechanic.HasRemainingLaunches()) return;

            State = SpinCutterState.Playing;
            _mechanic.LaunchBlade();
            _ui.UpdateLaunches(_mechanic.RemainingLaunches);
        }

        public void OnAllEnemiesDefeated(int enemiesKilledThisLaunch, int remainingLaunches, float comboMultiplier)
        {
            if (State != SpinCutterState.Playing && State != SpinCutterState.WaitingLaunch) return;

            int bonus = remainingLaunches * 100;
            int comboBonus = Mathf.RoundToInt(enemiesKilledThisLaunch * 100 * (comboMultiplier - 1f));
            _score += enemiesKilledThisLaunch * 100 + bonus + comboBonus;

            State = SpinCutterState.StageClear;
            _ui.UpdateScore(_score);
            _ui.ShowStageClear(_currentStage, bonus + comboBonus);

            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _autoAdvanceCo = StartCoroutine(AutoAdvance());
        }

        public void OnEnemyDefeated(int pts)
        {
            _score += pts;
            _ui.UpdateScore(_score);
            _ui.UpdateEnemies(_mechanic.RemainingEnemies);
        }

        public void OnBladeLanded()
        {
            // Blade finished without killing all enemies
            if (State != SpinCutterState.Playing) return;

            _ui.UpdateLaunches(_mechanic.RemainingLaunches);

            if (_mechanic.RemainingEnemies <= 0)
            {
                // All defeated
                OnAllEnemiesDefeated(0, _mechanic.RemainingLaunches, 1f);
                return;
            }

            if (!_mechanic.HasRemainingLaunches())
            {
                TriggerGameOver();
            }
            else
            {
                State = SpinCutterState.WaitingLaunch;
            }
        }

        void TriggerGameOver()
        {
            if (State == SpinCutterState.GameOver) return;
            if (_autoAdvanceCo != null) { StopCoroutine(_autoAdvanceCo); _autoAdvanceCo = null; }
            State = SpinCutterState.GameOver;
            StartCoroutine(CameraShake(0.5f, 0.5f));
            _ui.ShowGameOver(_score);
        }

        public void OnNextStagePressed()
        {
            if (State != SpinCutterState.StageClear) return;
            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _stageManager.CompleteCurrentStage();
        }

        IEnumerator AutoAdvance()
        {
            yield return new WaitForSeconds(2.5f);
            if (State == SpinCutterState.StageClear)
                _stageManager.CompleteCurrentStage();
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
            SceneLoader.LoadGame(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void ReturnToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
