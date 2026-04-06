using UnityEngine;
using System.Collections;

namespace Game056v2_InflateFloat
{
    public class InflateFloatGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] BalloonController _balloon;
        [SerializeField] CourseManager _courseManager;
        [SerializeField] InflateFloatUI _ui;

        public enum GameState { Idle, Playing, StageClear, GameClear, GameOver }
        GameState _state = GameState.Idle;

        int _score;
        int _comboCount;
        int _coinsCollected;
        int _totalCoins;

        public GameState State => _state;
        public int Score => _score;
        public int ComboCount => _comboCount;

        void Start()
        {
            _instructionPanel.Show(
                "056",
                "InflateFloat",
                "風船を膨らませて障害物をかわしながら空を飛ぼう！",
                "長押しで膨らます・離すと縮む・ドラッグで左右移動",
                "ゴールフラッグまで無事に到達しよう！"
            );
            _instructionPanel.OnDismissed += StartGame;
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
            _coinsCollected = 0;
            int stageNumber = stage + 1;
            var config = _stageManager.GetCurrentStageConfig();
            _balloon.SetupStage(config, stageNumber);
            _courseManager.SetupStage(config, stageNumber);
            _totalCoins = _courseManager.TotalCoins;
            _ui.OnStageChanged(stageNumber);
            _ui.UpdateInflateGauge(0f);
            _ui.UpdateDistance(0f);
        }

        void OnAllStagesCleared()
        {
            _state = GameState.GameClear;
            _ui.ShowGameClear(_score);
        }

        public bool IsPlaying() => _state == GameState.Playing;

        public void OnCoinCollected()
        {
            if (_state != GameState.Playing) return;
            _coinsCollected++;
            _comboCount++;
            int baseScore = 100;
            float comboMult = _comboCount >= 3 ? 1.5f : 1.0f;
            _score += Mathf.RoundToInt(baseScore * comboMult);
            _ui.UpdateScore(_score);
            _ui.ShowCombo(_comboCount);
        }

        public void OnObstaclePassed()
        {
            if (_state != GameState.Playing) return;
            _score += 50;
            _ui.UpdateScore(_score);
        }

        public void OnGoalReached()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;
            _comboCount = 0;
            float perfectMult = (_coinsCollected >= _totalCoins && _totalCoins > 0) ? 2.0f : 1.0f;
            _score = Mathf.RoundToInt(_score * perfectMult);
            _ui.ShowStageClear(_score);
        }

        public void OnBalloonPopped()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.GameOver;
            _comboCount = 0;
            _ui.ShowGameOver();
            StartCoroutine(CameraShake());
        }

        public void OnInflateGaugeChanged(float ratio)
        {
            _ui?.UpdateInflateGauge(ratio);
        }

        public void OnDistanceChanged(float ratio)
        {
            _ui?.UpdateDistance(ratio);
        }

        public void OnMissCombo()
        {
            _comboCount = 0;
        }

        IEnumerator CameraShake()
        {
            if (Camera.main == null) yield break;
            var cam = Camera.main.transform;
            Vector3 origin = cam.position;
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                cam.position = origin + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f), 0);
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
