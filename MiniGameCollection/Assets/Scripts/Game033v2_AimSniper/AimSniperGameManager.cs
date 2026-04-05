using UnityEngine;
using System.Collections;

namespace Game033v2_AimSniper
{
    public enum AimSniperState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class AimSniperGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] AimSniperMechanic _mechanic;
        [SerializeField] AimSniperUI _ui;

        public AimSniperState State { get; private set; } = AimSniperState.WaitingInstruction;

        int _score;
        int _currentStage;
        Coroutine _autoAdvanceCo;

        void Start()
        {
            _instructionPanel.Show(
                "033v2",
                "AimSniper",
                "スコープでターゲットを狙い撃とう",
                "ドラッグでスコープ移動、タップで射撃",
                "限られた弾数で全ターゲットを撃破しよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            State = AimSniperState.Playing;

            _ui.Initialize(this);
            _ui.UpdateScore(_score);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 0.0f, countMultiplier = 3, complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 4, complexityFactor = 0.2f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 5, complexityFactor = 0.3f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 6, complexityFactor = 0.5f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 7, complexityFactor = 0.7f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == AimSniperState.Clear) return;
            _currentStage = stageIndex + 1;
            State = AimSniperState.Playing;

            var config = _stageManager.GetCurrentStageConfig();
            _mechanic.SetupStage(config, stageIndex);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            State = AimSniperState.Clear;
            _mechanic.Deactivate();
            _ui.ShowFinalClear(_score);
        }

        public void OnTargetHit(int pts, bool headshot, float comboMultiplier, int remainingTargets)
        {
            if (State != AimSniperState.Playing) return;

            _score += pts;
            _ui.UpdateScore(_score);
            _ui.UpdateTargets(remainingTargets);

            if (headshot && comboMultiplier > 1.0f)
                _ui.ShowCombo(comboMultiplier);

            if (remainingTargets <= 0)
            {
                // Stage clear - deactivate input before computing bonus
                _mechanic.Deactivate();
                int bulletBonus = _mechanic.RemainingBullets * 50;
                _score += bulletBonus;
                _ui.UpdateScore(_score);

                State = AimSniperState.StageClear;
                _ui.ShowStageClear(_currentStage, bulletBonus);

                if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
                _autoAdvanceCo = StartCoroutine(AutoAdvance());
            }
        }

        public void OnMiss(int remainingBullets, int remainingTargets)
        {
            if (State != AimSniperState.Playing) return;

            if (remainingBullets <= 0 && remainingTargets > 0)
            {
                TriggerGameOver();
            }
        }

        void TriggerGameOver()
        {
            if (State == AimSniperState.GameOver) return;
            if (_autoAdvanceCo != null) { StopCoroutine(_autoAdvanceCo); _autoAdvanceCo = null; }
            State = AimSniperState.GameOver;
            _mechanic.Deactivate();
            StartCoroutine(CameraShake(0.5f, 0.5f));
            _ui.ShowGameOver(_score);
        }

        public void OnNextStagePressed()
        {
            if (State != AimSniperState.StageClear) return;
            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _stageManager.CompleteCurrentStage();
        }

        IEnumerator AutoAdvance()
        {
            yield return new WaitForSeconds(2.5f);
            if (State == AimSniperState.StageClear)
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
