using UnityEngine;
using System.Collections;

namespace Game057v2_CandyDrop
{
    public class CandyDropGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CandySpawner _spawner;
        [SerializeField] TowerChecker _towerChecker;
        [SerializeField] CandyDropUI _ui;

        public enum GameState { Idle, Playing, StageClear, GameClear, GameOver }
        GameState _state = GameState.Idle;

        int _score;       // current stage score
        int _totalScore;  // cumulative across stages
        int _comboCount;
        float _comboMultiplier = 1.0f;
        int _consecutiveSuccess;

        public GameState State => _state;
        public int Score => _score;
        public int ComboCount => _comboCount;

        void Start()
        {
            _ui.Init(this);
            _instructionPanel.Show(
                "057",
                "CandyDrop",
                "落下するキャンディを積み上げよう！",
                "ドラッグで位置を決めてタップで落とす",
                "目標ラインまでキャンディを積み上げよう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }

        void StartGame()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _state = GameState.Playing;
            _score = 0;  // per-stage score, _totalScore persists
            _comboCount = 0;
            _comboMultiplier = 1.0f;
            _consecutiveSuccess = 0;
            int stageNumber = stage + 1;
            var config = _stageManager.GetCurrentStageConfig();
            _spawner.SetupStage(config, stageNumber);
            _towerChecker.SetupStage(config, stageNumber);
            _ui.OnStageChanged(stageNumber);
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(0, _comboMultiplier);
        }

        void OnAllStagesCleared()
        {
            _state = GameState.GameClear;
            _ui.ShowGameClear(_totalScore);
        }

        public bool IsPlaying() => _state == GameState.Playing;

        public void OnCandyLanded()
        {
            if (_state != GameState.Playing) return;
            _consecutiveSuccess++;
            _comboCount++;
            if (_comboCount % 5 == 0) _comboMultiplier += 0.5f;

            int baseScore = 50;
            _score += Mathf.RoundToInt(baseScore * _comboMultiplier);
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_comboCount, _comboMultiplier);
        }

        public void OnColorBonus(int bonusScore)
        {
            if (_state != GameState.Playing) return;
            _score += Mathf.RoundToInt(bonusScore * _comboMultiplier);
            _ui.UpdateScore(_score);
            StartCoroutine(ColorBonusFlash());
        }

        public void OnTowerCollapsed()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.GameOver;
            _comboCount = 0;
            _comboMultiplier = 1.0f;
            _spawner.StopSpawning();
            _ui.ShowGameOver(_score);
            StartCoroutine(CameraShake());
        }

        public void OnGoalReached(float heightRatio)
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;
            _spawner.StopSpawning();
            int clearBonus = 1000;
            _score += clearBonus;
            _totalScore += _score;
            _ui.ShowStageClear(_score);
        }

        public void OnHeightChanged(float ratio)
        {
            _ui?.UpdateHeightGauge(ratio);
        }

        IEnumerator ColorBonusFlash()
        {
            yield return null;
        }

        IEnumerator CameraShake()
        {
            if (Camera.main == null) yield break;
            var cam = Camera.main.transform;
            Vector3 origin = cam.position;
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                cam.position = origin + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.position = origin;
        }

        public void OnNextStage()
        {
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void OnBackToMenu()
        {
            SceneLoader.BackToMenu();
        }
    }
}
