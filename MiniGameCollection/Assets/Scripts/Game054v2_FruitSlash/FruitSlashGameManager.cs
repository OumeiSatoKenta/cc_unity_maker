using UnityEngine;
using UnityEngine.InputSystem;

namespace Game054v2_FruitSlash
{
    public enum GameState { Idle, Playing, StageClear, AllClear, GameOver }

    public class FruitSlashGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] FruitManager _fruitManager;
        [SerializeField] FruitSlashUI _ui;

        public GameState State { get; private set; } = GameState.Idle;
        public int Score { get; private set; }

        int _comboCount;
        int _missStreak;
        int _life;
        const int MaxLife = 3;
        const int MissStreakLimit = 3;

        float _stageTimer;
        float _stageTimeLimit = 30f;
        int _targetScore;
        bool _perfectStage;

        // スワイプ入力
        bool _isDragging;
        Vector2 _dragStartWorld;
        Vector2 _dragLastWorld;

        readonly int[] _targetScores = { 100, 250, 450, 700, 1000 };
        readonly float[] _timeLimits = { 35f, 30f, 28f, 25f, 25f };

        void Start()
        {
            _instructionPanel.Show(
                "054v2",
                "FruitSlash",
                "飛んでくるフルーツをスワイプで切り爆弾は回避しよう！",
                "画面をドラッグしてフルーツを切断。コンボで得点倍率アップ！",
                "目標スコアに到達してステージクリア！爆弾は切らないで！"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void OnDestroy()
        {
            _instructionPanel.OnDismissed -= StartGame;
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
        }

        void StartGame()
        {
            Score = 0;
            _life = MaxLife;
            _comboCount = 0;
            _missStreak = 0;

            _fruitManager.Initialize(this);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { stageName = "Stage 1", speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
                new StageManager.StageConfig { stageName = "Stage 2", speedMultiplier = 1.3f, countMultiplier = 2, complexityFactor = 0.1f },
                new StageManager.StageConfig { stageName = "Stage 3", speedMultiplier = 1.6f, countMultiplier = 3, complexityFactor = 0.4f },
                new StageManager.StageConfig { stageName = "Stage 4", speedMultiplier = 2.0f, countMultiplier = 4, complexityFactor = 0.7f },
                new StageManager.StageConfig { stageName = "Stage 5", speedMultiplier = 2.5f, countMultiplier = 5, complexityFactor = 1.0f },
            });

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            State = GameState.Playing;
            _comboCount = 0;
            _missStreak = 0;
            _perfectStage = true;

            _targetScore = _targetScores[Mathf.Min(stageIndex, _targetScores.Length - 1)];
            _stageTimeLimit = _timeLimits[Mathf.Min(stageIndex, _timeLimits.Length - 1)];
            _stageTimer = _stageTimeLimit;

            var config = _stageManager.GetCurrentStageConfig();
            _fruitManager.SetupStage(config);
            _fruitManager.StartSpawning();

            _ui.UpdateLife(_life, MaxLife);
            _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages, _targetScore);
            _ui.UpdateScore(Score);
            _ui.UpdateCombo(0, 1f);
        }

        void OnAllStagesCleared()
        {
            State = GameState.AllClear;
            _fruitManager.StopSpawning();
            _ui.ShowAllClear(Score);
        }

        void Update()
        {
            if (State != GameState.Playing) return;

            _stageTimer -= Time.deltaTime;
            _ui.UpdateTimer(_stageTimer, _stageTimeLimit);

            if (_stageTimer <= 0f)
            {
                _fruitManager.StopSpawning();
                if (Score >= _targetScore)
                    StageClear();
                else
                    TriggerGameOver();
                return;
            }

            if (Score >= _targetScore)
            {
                _fruitManager.StopSpawning();
                StageClear();
                return;
            }

            HandleSwipeInput();
        }

        void HandleSwipeInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _dragStartWorld = ScreenToWorld(mouse.position.ReadValue());
                _dragLastWorld = _dragStartWorld;
            }
            else if (mouse.leftButton.isPressed && _isDragging)
            {
                Vector2 currentWorld = ScreenToWorld(mouse.position.ReadValue());
                if ((currentWorld - _dragLastWorld).sqrMagnitude > 0.01f)
                {
                    _fruitManager.CheckSlash(_dragLastWorld, currentWorld);
                    _dragLastWorld = currentWorld;
                }
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }
        }

        Vector2 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            return new Vector2(wp.x, wp.y);
        }

        public void OnFruitCut(int baseScore, bool isBonus)
        {
            _missStreak = 0;
            _comboCount++;
            float multiplier = GetComboMultiplier();
            int points = Mathf.RoundToInt(baseScore * multiplier);
            Score += points;
            _ui.UpdateScore(Score);
            _ui.UpdateCombo(_comboCount, multiplier);
            _ui.PlayComboEffect(_comboCount);
        }

        public void OnIceFruitCut(int baseScore)
        {
            _missStreak = 0;
            _comboCount = 0; // コンボリセット
            Score += baseScore;
            _ui.UpdateScore(Score);
            _ui.UpdateCombo(0, 1f);
            _ui.ShowIceFreezeEffect();
        }

        public void OnMultiSlash(int count)
        {
            int bonus = count * 20;
            Score += bonus;
            _ui.UpdateScore(Score);
        }

        public void OnBombCut()
        {
            _perfectStage = false;
            _comboCount = 0;
            _life--;
            _ui.UpdateLife(_life, MaxLife);
            _ui.PlayBombEffect();

            if (_life <= 0)
            {
                _fruitManager.StopSpawning();
                TriggerGameOver();
            }
        }

        public void OnFruitMissed()
        {
            if (State != GameState.Playing) return;
            _perfectStage = false;
            _missStreak++;
            if (_missStreak >= MissStreakLimit)
            {
                _missStreak = 0;
                _comboCount = 0;
                _life--;
                _ui.UpdateLife(_life, MaxLife);
                if (_life <= 0)
                {
                    _fruitManager.StopSpawning();
                    TriggerGameOver();
                }
            }
        }

        float GetComboMultiplier()
        {
            if (_comboCount >= 20) return 3.0f;
            if (_comboCount >= 10) return 2.0f;
            if (_comboCount >= 5) return 1.5f;
            return 1.0f;
        }

        void StageClear()
        {
            if (State == GameState.StageClear) return;
            if (_perfectStage)
                Score = Mathf.RoundToInt(Score * 1.5f);

            State = GameState.StageClear;
            _fruitManager.ClearAllFruits();
            _ui.ShowStageClear(_stageManager.CurrentStage + 1, Score);
        }

        void TriggerGameOver()
        {
            State = GameState.GameOver;
            _fruitManager.ClearAllFruits();
            _ui.ShowGameOver(Score);
        }

        public void OnNextStage()
        {
            if (State != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
