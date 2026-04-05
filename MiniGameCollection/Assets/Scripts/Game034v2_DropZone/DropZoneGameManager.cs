using UnityEngine;
using System.Collections;

namespace Game034v2_DropZone
{
    public enum DropZoneState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class DropZoneGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DropZoneMechanic _mechanic;
        [SerializeField] DropZoneUI _ui;

        public DropZoneState State { get; private set; } = DropZoneState.WaitingInstruction;

        int _score;
        int _combo;
        int _missCount;
        int _currentStage;
        const int MaxMiss = 3;
        Coroutine _autoAdvanceCo;

        void Start()
        {
            _instructionPanel.Show(
                "034v2",
                "DropZone",
                "落ちてくるアイテムを正しい箱に仕分けしよう",
                "アイテムをドラッグして正しいゾーンにドロップ",
                "ミス3回以内で全アイテムを仕分けしよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _combo = 0;
            _missCount = 0;
            State = DropZoneState.Playing;

            _ui.Initialize(this);
            _ui.UpdateScore(_score);
            _ui.UpdateMiss(_missCount, MaxMiss);
            _ui.UpdateCombo(0);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 5,  complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 6,  complexityFactor = 0.0f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 8,  complexityFactor = 0.5f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 2.5f, countMultiplier = 9,  complexityFactor = 0.3f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 3.5f, countMultiplier = 10, complexityFactor = 0.3f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == DropZoneState.Clear) return;
            _currentStage = stageIndex + 1;
            State = DropZoneState.Playing;

            var config = _stageManager.GetCurrentStageConfig();
            _mechanic.SetupStage(config, stageIndex);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.UpdateMiss(_missCount, MaxMiss);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            State = DropZoneState.Clear;
            _mechanic.Deactivate();
            _ui.ShowFinalClear(_score);
        }

        public void OnCorrectDrop(bool isBonus)
        {
            if (State != DropZoneState.Playing) return;

            _combo++;
            int comboMultiplier = Mathf.Clamp(_combo, 1, 5);
            int pts = isBonus ? 0 : 10 * comboMultiplier;
            _score += pts;

            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_combo);

            if (isBonus)
            {
                // ライフ回復（ミスを1減らす）
                _missCount = Mathf.Max(0, _missCount - 1);
                _ui.UpdateMiss(_missCount, MaxMiss);
            }
        }

        public void OnWrongDrop()
        {
            if (State != DropZoneState.Playing) return;

            _combo = 0;
            _missCount++;
            _ui.UpdateCombo(0);
            _ui.UpdateMiss(_missCount, MaxMiss);

            StartCoroutine(CameraShake(0.2f, 0.3f));

            if (_missCount >= MaxMiss)
            {
                TriggerGameOver();
            }
        }

        public void OnStageClear()
        {
            if (State != DropZoneState.Playing) return;

            _mechanic.Deactivate();
            int bonus = (MaxMiss - _missCount) * 100;
            _score += bonus;
            _ui.UpdateScore(_score);

            State = DropZoneState.StageClear;
            _ui.ShowStageClear(_currentStage, bonus);

            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _autoAdvanceCo = StartCoroutine(AutoAdvance());
        }

        void TriggerGameOver()
        {
            if (State == DropZoneState.GameOver) return;
            if (_autoAdvanceCo != null) { StopCoroutine(_autoAdvanceCo); _autoAdvanceCo = null; }
            State = DropZoneState.GameOver;
            _mechanic.Deactivate();
            _ui.ShowGameOver(_score);
        }

        public void OnNextStagePressed()
        {
            if (State != DropZoneState.StageClear) return;
            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _stageManager.CompleteCurrentStage();
        }

        IEnumerator AutoAdvance()
        {
            yield return new WaitForSeconds(3.0f);
            if (State == DropZoneState.StageClear)
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
