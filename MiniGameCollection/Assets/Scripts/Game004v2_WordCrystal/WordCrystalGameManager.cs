using System.Collections;
using UnityEngine;

namespace Game004v2_WordCrystal
{
    public enum GameState { WaitingInstruction, Playing, StageClear, Clear, GameOver }

    public class WordCrystalGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private WordManager _wordManager;
        [SerializeField] private WordCrystalUI _ui;

        private GameState _state = GameState.WaitingInstruction;
        private int _score;
        private int _comboCount;
        private float _timeLeft;
        private bool _timerRunning;
        private int _targetScore;

        private static readonly WordManager.StageConfig[] _stageConfigs = new[]
        {
            new WordManager.StageConfig { stageIndex=0, timeLimit=60, targetScore=500,  crystalCount=8,  hasHidden=false, hasBonus=false, hasPoison=false, theme=null },
            new WordManager.StageConfig { stageIndex=1, timeLimit=60, targetScore=800,  crystalCount=9,  hasHidden=true,  hasBonus=false, hasPoison=false, theme=null },
            new WordManager.StageConfig { stageIndex=2, timeLimit=50, targetScore=1200, crystalCount=10, hasHidden=true,  hasBonus=true,  hasPoison=false, theme=null },
            new WordManager.StageConfig { stageIndex=3, timeLimit=40, targetScore=1500, crystalCount=10, hasHidden=true,  hasBonus=true,  hasPoison=true,  theme=null },
            new WordManager.StageConfig { stageIndex=4, timeLimit=35, targetScore=2000, crystalCount=11, hasHidden=true,  hasBonus=true,  hasPoison=true,  theme="animal" },
        };

        public bool IsPlaying => _state == GameState.Playing;

        private void Start()
        {
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _wordManager.OnWordResult += OnWordResult;
            _wordManager.OnPoisonHit += OnPoisonHit;

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("004", "WordCrystal",
                    "クリスタルから文字を集めて英単語を作るパズル",
                    "タップでクリスタル破壊、文字タイルをタップして並べる",
                    "制限時間内に目標スコアを達成しよう");
            }
            else
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            _score = 0;
            _comboCount = 0;
            if (_ui != null) _ui.UpdateScore(0);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            _state = GameState.Playing;
            var config = stageIndex < _stageConfigs.Length ? _stageConfigs[stageIndex] : _stageConfigs[0];
            _targetScore = config.targetScore;
            _timeLeft = config.timeLimit;
            _timerRunning = true;

            _wordManager.SetupStage(config);

            if (_ui != null)
            {
                _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
                _ui.UpdateTargetScore(_targetScore);
                _ui.UpdateScore(_score);
                _ui.UpdateCombo(0);
                _ui.HideAllPanels();
                _ui.SetThemeLabel(config.theme);
            }
        }

        private void OnAllStagesCleared()
        {
            _timerRunning = false;
            _state = GameState.Clear;
            _wordManager.SetActive(false);
            if (_ui != null) _ui.ShowClearPanel(_score);
        }

        private void Update()
        {
            if (!_timerRunning || _state != GameState.Playing) return;

            _timeLeft -= Time.deltaTime;
            if (_ui != null) _ui.UpdateTimer(Mathf.Max(0f, _timeLeft));

            if (_timeLeft <= 0f)
            {
                _timerRunning = false;
                OnTimerEnd();
            }
        }

        private void OnTimerEnd()
        {
            _wordManager.SetActive(false);
            if (_score >= _targetScore)
            {
                _state = GameState.StageClear;
                int stars = _score >= _targetScore * 2 ? 3 : _score >= _targetScore * 3 / 2 ? 2 : 1;
                if (_ui != null) _ui.ShowStageClearPanel(_stageManager.CurrentStage + 1, _score, stars);
            }
            else
            {
                _comboCount = 0;
                _state = GameState.GameOver;
                if (_ui != null) _ui.ShowGameOverPanel();
            }
        }

        private void OnWordResult(string word, int baseScore, bool isCorrect)
        {
            if (!isCorrect)
            {
                _comboCount = 0;
                if (_ui != null)
                {
                    _ui.UpdateCombo(0);
                    _ui.FlashSlots(false);
                }
                return;
            }

            _comboCount++;
            float multiplier = _comboCount >= 4 ? 3f : _comboCount == 3 ? 2f : _comboCount == 2 ? 1.5f : 1f;
            int gained = Mathf.RoundToInt(baseScore * multiplier);
            _score += gained;

            if (_ui != null)
            {
                _ui.UpdateScore(_score);
                _ui.UpdateCombo(_comboCount);
                _ui.FlashSlots(true);
                _ui.ShowScorePopup(gained, multiplier);
            }

            // 目標スコア達成チェック
            if (_score >= _targetScore && _state == GameState.Playing)
            {
                _timerRunning = false;
                _state = GameState.StageClear;
                _wordManager.SetActive(false);
                int stars = _score >= _targetScore * 2 ? 3 : _score >= _targetScore * 3 / 2 ? 2 : 1;
                if (_ui != null) _ui.ShowStageClearPanel(_stageManager.CurrentStage + 1, _score, stars);
            }
        }

        private void OnPoisonHit()
        {
            _timeLeft = Mathf.Max(0f, _timeLeft - 5f);
            if (_ui != null) _ui.FlashTimer();
        }

        public void OnSubmitButton()
        {
            if (_state != GameState.Playing) return;
            _wordManager.SubmitWord();
        }

        public void OnClearButton()
        {
            if (_state != GameState.Playing) return;
            _wordManager.ClearSlots();
        }

        public void OnNextStageButtonPressed()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void RestartStage()
        {
            if (_state != GameState.GameOver) return;
            _score = 0;
            _comboCount = 0;
            OnStageChanged(_stageManager.CurrentStage);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu");
        }

        private void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.OnStageChanged -= OnStageChanged;
                _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            }
            if (_wordManager != null)
            {
                _wordManager.OnWordResult -= OnWordResult;
                _wordManager.OnPoisonHit -= OnPoisonHit;
            }
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
        }
    }
}
