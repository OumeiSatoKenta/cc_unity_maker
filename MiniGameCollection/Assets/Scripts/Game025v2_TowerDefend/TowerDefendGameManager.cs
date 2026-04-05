using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game025v2_TowerDefend
{
    public enum TowerDefendGameState
    {
        WaitingInstruction,
        WavePrepare,
        WaveActive,
        StageClear,
        Clear,
        GameOver
    }

    public class TowerDefendGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] WaveManager _waveManager;
        [SerializeField] WallManager _wallManager;
        [SerializeField] TowerDefendUI _ui;

        public TowerDefendGameState State { get; private set; } = TowerDefendGameState.WaitingInstruction;

        int _totalScore;
        int _breachCount;
        int _maxBreachCount;
        int _currentStage;

        void Start()
        {
            _instructionPanel.Show(
                "025v2",
                "TowerDefend",
                "壁を描いて敵の侵入を阻止しよう！",
                "ドラッグで壁を描く。インクの残量に注意！壁をダブルタップすると消去できる",
                "全Waveの敵をゴールに到達させずに全5ステージをクリアしよう"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        // Stage 1: start left, goal right; Stage 5: also top start
        static readonly Vector3 StartPos1 = new(-2.25f, 0f, 0f);
        static readonly Vector3 StartPos2 = new(0f, 3.6f, 0f);
        static readonly Vector3 GoalPos   = new(2.25f, 0f, 0f);

        void StartGame()
        {
            _totalScore = 0;
            _breachCount = 0;
            _waveManager.Initialize(this, _wallManager);
            _ui.Initialize(this, _wallManager);
            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stage)
        {
            _currentStage = stage + 1;
            _breachCount = 0;
            var config = _stageManager.GetCurrentStageConfig();
            _maxBreachCount = GetMaxBreachCount(_currentStage);

            // Grid origin: camSize=5, bottomMargin=2.8 → originY = -5 + 2.8 = -2.2
            _wallManager.SetGridOrigin(new Vector3(-2.5f, -2.2f, 0f), 0.5f);
            _wallManager.SetupStage(config, _currentStage);

            // Set spawn / goal positions
            var starts = new System.Collections.Generic.List<UnityEngine.Vector3> { StartPos1 };
            if (_currentStage == 5) starts.Add(StartPos2);
            _waveManager.SetPaths(starts, GoalPos);
            _waveManager.SetupStage(config, _currentStage);
            State = TowerDefendGameState.WavePrepare;
            _ui.UpdateStageDisplay(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_totalScore);
            _ui.UpdateBreach(_breachCount, _maxBreachCount);
            _ui.UpdateWave(0, _waveManager.TotalWaves);
            _ui.ShowWaveStartButton(true);
            _ui.HideStageClearPanel();
        }

        int GetMaxBreachCount(int stage)
        {
            return stage switch
            {
                1 => 5,
                2 => 4,
                3 => 4,
                4 => 3,
                5 => 3,
                _ => 5
            };
        }

        void OnAllStagesCleared()
        {
            State = TowerDefendGameState.Clear;
            _waveManager.StopAll();
            _wallManager.SetDrawingEnabled(false);
            _ui.ShowClearPanel(_totalScore);
        }

        public void OnStartWaveButton()
        {
            if (State != TowerDefendGameState.WavePrepare) return;
            State = TowerDefendGameState.WaveActive;
            _wallManager.SetDrawingEnabled(false);
            _ui.ShowWaveStartButton(false);
            _waveManager.StartNextWave();
        }

        public void OnWaveCleared(int waveIndex)
        {
            if (State != TowerDefendGameState.WaveActive) return;
            // Wave完封ボーナス
            if (_waveManager.WasWavePerfect)
            {
                int bonus = 200 * (waveIndex + 1);
                _totalScore += bonus;
                _ui.ShowWaveBonus(bonus);
            }
            _ui.UpdateWave(waveIndex + 1, _waveManager.TotalWaves);

            if (_waveManager.HasMoreWaves)
            {
                // インク回復 & 次Wave準備
                State = TowerDefendGameState.WavePrepare;
                _wallManager.SetDrawingEnabled(true);
                _wallManager.RefillInkPartial(0.2f); // 20%回復
                _ui.ShowWaveStartButton(true);
                _ui.UpdateInk(_wallManager.InkRatio);
            }
            else
            {
                // ステージクリア
                State = TowerDefendGameState.StageClear;
                _wallManager.SetDrawingEnabled(false);
                // インク残量ボーナス
                float inkRatio = _wallManager.InkRatio;
                float inkMultiplier = inkRatio >= 0.75f ? 2.0f : inkRatio >= 0.5f ? 1.5f : 1.0f;
                int inkBonus = Mathf.RoundToInt(_totalScore * (inkMultiplier - 1f));
                _totalScore += inkBonus;
                _ui.UpdateScore(_totalScore);
                _ui.ShowStageClearPanel(_totalScore, inkBonus, inkMultiplier);
            }
        }

        public void OnEnemyReachedGoal(Enemy enemy)
        {
            if (State != TowerDefendGameState.WaveActive) return;
            _breachCount++;
            _ui.UpdateBreach(_breachCount, _maxBreachCount);
            _ui.ShowBreachEffect();
            if (_breachCount >= _maxBreachCount)
            {
                GameOver();
            }
        }

        public void OnEnemyDefeated(Enemy enemy)
        {
            if (State != TowerDefendGameState.WaveActive) return;
            int baseScore = 50;
            // 迂回ボーナス
            float detourBonus = (enemy.TravelDistance > 0 && enemy.DirectDistance > 0.001f)
                ? Mathf.RoundToInt((enemy.TravelDistance / enemy.DirectDistance - 1f) * 10f)
                : 0;
            int score = baseScore + (int)detourBonus;
            _totalScore += score;
            _ui.UpdateScore(_totalScore);
        }

        void GameOver()
        {
            State = TowerDefendGameState.GameOver;
            _waveManager.StopAll();
            _wallManager.SetDrawingEnabled(false);
            _ui.ShowGameOverPanel(_totalScore);
        }

        public void OnNextStage()
        {
            if (State != TowerDefendGameState.StageClear) return;
            _wallManager.ClearAllWalls();
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnReturnToMenu()
        {
            SceneManager.LoadScene("CollectionSelect");
        }

        public void ShowInstructions()
        {
            if (_instructionPanel != null)
                _instructionPanel.Show(
                    "025v2",
                    "TowerDefend",
                    "壁を描いて敵の侵入を阻止しよう！",
                    "ドラッグで壁を描く。インクの残量に注意！壁をダブルタップすると消去できる",
                    "全Waveの敵をゴールに到達させずに全5ステージをクリアしよう"
                );
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
    }
}
