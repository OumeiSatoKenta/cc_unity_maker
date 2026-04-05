using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game026v2_SliceNinja
{
    public enum SliceNinjaGameState
    {
        WaitingInstruction,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class SliceNinjaGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] SliceManager _sliceManager;
        [SerializeField] SliceNinjaUI _ui;

        public SliceNinjaGameState State { get; private set; } = SliceNinjaGameState.WaitingInstruction;

        int _score;
        int _missCount;
        const int MaxMiss = 3;
        int _currentStage;
        float _comboMultiplier = 1f;
        int _maxCombo = 0;
        Camera _mainCamera;

        // Stage timer
        Coroutine _stageTimerCoroutine;
        static readonly float[] StageDurations = { 30f, 30f, 60f, 60f, 60f };

        void Start()
        {
            _instructionPanel.Show(
                "026v2",
                "SliceNinja",
                "飛んでくる物体をスワイプで切れ！爆弾だけは絶対に切るな！",
                "マウスでドラッグしてスワイプ軌跡を描く。軌跡に触れた物体が切断される",
                "ミス3回以内・爆弾を避けながら全5ステージをサバイバルしよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _missCount = 0;
            _comboMultiplier = 1f;
            _maxCombo = 0;
            State = SliceNinjaGameState.Playing;

            _mainCamera = Camera.main;

            // Initialize UI with this manager
            _ui.Initialize(this);

            // Set custom stage configs (speedMultiplier, countMultiplier:int, complexityFactor)
            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.26f, countMultiplier = 1, complexityFactor = 0.2f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.4f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 1.83f, countMultiplier = 2, complexityFactor = 0.6f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 2.17f, countMultiplier = 2, complexityFactor = 1.0f, stageName = "Stage 5" },
            });

            _sliceManager.Initialize(this, _mainCamera);

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            _currentStage = stageIndex + 1;
            _missCount = 0;
            _comboMultiplier = 1f;

            var config = _stageManager.GetCurrentStageConfig();
            _sliceManager.SetupStage(config, _currentStage);

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.UpdateMiss(_missCount, MaxMiss);
            _ui.UpdateCombo(_comboMultiplier);
            _ui.HideStageClear();

            if (_stageTimerCoroutine != null)
                StopCoroutine(_stageTimerCoroutine);
            float duration = (_currentStage - 1 < StageDurations.Length) ? StageDurations[_currentStage - 1] : 30f;
            _stageTimerCoroutine = StartCoroutine(StageTimer(duration));
        }

        IEnumerator StageTimer(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (State == SliceNinjaGameState.Playing)
                CompleteStageClear();
        }

        void CompleteStageClear()
        {
            _sliceManager.StopSpawning();
            State = SliceNinjaGameState.StageClear;
            _ui.ShowStageClear(_currentStage);
            StartCoroutine(AutoAdvanceStage());
        }

        IEnumerator AutoAdvanceStage()
        {
            yield return new WaitForSeconds(2f);
            if (State == SliceNinjaGameState.StageClear)
                _stageManager.CompleteCurrentStage();
        }

        void OnAllStagesCleared()
        {
            _sliceManager.StopSpawning();
            State = SliceNinjaGameState.Clear;
            _ui.ShowFinalClear(_score);
        }

        public void AddScore(int points, float comboMult)
        {
            _comboMultiplier = comboMult;
            int c = Mathf.RoundToInt(comboMult * 10);
            if (c > _maxCombo) _maxCombo = c;
            _score += points;
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_comboMultiplier);
        }

        public void OnObjectSliced(ObjectType type)
        {
            // Score is handled in SliceManager with combo calc
        }

        public void OnObjectMissed(ObjectType type)
        {
            if (State != SliceNinjaGameState.Playing) return;
            if (type == ObjectType.Bomb || type == ObjectType.StealthBomb) return; // bombs falling off is OK

            _sliceManager.ResetCombo();
            _missCount++;
            _ui.UpdateMiss(_missCount, MaxMiss);

            if (_missCount >= MaxMiss)
                TriggerGameOver();
        }

        public void OnBombCut()
        {
            if (State != SliceNinjaGameState.Playing) return;
            TriggerGameOver();
        }

        void TriggerGameOver()
        {
            if (State == SliceNinjaGameState.GameOver) return;
            if (_stageTimerCoroutine != null)
                StopCoroutine(_stageTimerCoroutine);
            _sliceManager.StopSpawning();
            _sliceManager.DisableInput();
            State = SliceNinjaGameState.GameOver;
            StartCoroutine(GameOverSequence());
        }

        IEnumerator GameOverSequence()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            Vector3 originalPos = _mainCamera.transform.position;
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                float shakeAmount = 0.2f * (1f - elapsed / 0.4f);
                _mainCamera.transform.position = originalPos + (Vector3)Random.insideUnitCircle * shakeAmount;
                yield return null;
            }
            _mainCamera.transform.position = originalPos;

            _ui.ShowGameOver(_score, _maxCombo);
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
