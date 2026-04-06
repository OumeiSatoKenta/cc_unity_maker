using UnityEngine;
using System.Collections;

namespace Game058v2_ThreadNeedle
{
    public class ThreadNeedleGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] NeedleController _needleController;
        [SerializeField] ThreadNeedleUI _ui;

        public enum GameState { Idle, Playing, StageClear, GameClear, GameOver }
        GameState _state = GameState.Idle;

        int _score;
        int _totalScore;
        int _comboCount;
        float _comboMultiplier = 1.0f;
        int _missCount;
        const int MaxMiss = 3;

        public GameState State => _state;
        public int Score => _score;
        public int ComboCount => _comboCount;
        public int MissCount => _missCount;

        void Start()
        {
            _ui.Init(this);
            _instructionPanel.Show(
                "058",
                "ThreadNeedle",
                "揺れる針穴に糸を通そう！",
                "針穴が正面に来たらタップして糸を射出",
                "全ラウンドの針穴に糸を通してステージクリア！"
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
            _score = 0;
            _comboCount = 0;
            _comboMultiplier = 1.0f;
            _missCount = 0;
            int stageNumber = stage + 1;
            var config = _stageManager.GetCurrentStageConfig();
            _needleController.SetupStage(config, stageNumber);
            _ui.OnStageChanged(stageNumber);
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(0, _comboMultiplier);
            _ui.UpdateMiss(0);
        }

        void OnAllStagesCleared()
        {
            _state = GameState.GameClear;
            _ui.ShowGameClear(_totalScore);
        }

        public bool IsPlaying() => _state == GameState.Playing;

        public void AddScore(int baseScore, bool isCenter)
        {
            if (_state != GameState.Playing) return;
            _comboCount++;
            if (_comboCount >= 10) _comboMultiplier = 2.0f;
            else if (_comboCount >= 5) _comboMultiplier = 1.5f;
            else _comboMultiplier = 1.0f;

            int points = Mathf.RoundToInt(baseScore * _comboMultiplier);
            _score += points;
            _totalScore += points;
            _ui.UpdateScore(_score);
            _ui.UpdateCombo(_comboCount, _comboMultiplier);
        }

        public void OnMiss()
        {
            if (_state != GameState.Playing) return;
            _missCount++;
            _comboCount = 0;
            _comboMultiplier = 1.0f;
            _ui.UpdateCombo(0, 1.0f);
            _ui.UpdateMiss(_missCount);
            StartCoroutine(CameraShake());

            if (_missCount >= MaxMiss)
            {
                _state = GameState.GameOver;
                _needleController.StopNeedle();
                _ui.ShowGameOver(_score);
            }
        }

        public void OnStageClear()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;
            int clearBonus = 500;
            _score += clearBonus;
            _totalScore += clearBonus;
            _ui.UpdateScore(_score);
            _ui.ShowStageClear(_score);
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

        IEnumerator CameraShake()
        {
            if (Camera.main == null) yield break;
            var cam = Camera.main.transform;
            Vector3 origin = cam.position;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                cam.position = origin + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f), 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.position = origin;
        }
    }
}
