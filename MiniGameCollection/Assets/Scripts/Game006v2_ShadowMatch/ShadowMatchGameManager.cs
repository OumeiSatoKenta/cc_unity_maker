using System.Collections;
using UnityEngine;

namespace Game006v2_ShadowMatch
{
    public enum GameState { WaitingInstruction, Playing, StageClear, Clear }

    public class ShadowMatchGameManager : MonoBehaviour
    {
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private InstructionPanel _instructionPanel;
        [SerializeField] private ShadowObjectController _shadowObjectController;
        [SerializeField] private ShadowMatchUI _ui;

        private GameState _state = GameState.WaitingInstruction;
        private int _score;
        private int _judgeCount;

        public bool IsPlaying => _state == GameState.Playing;
        public int Score => _score;

        private void Start()
        {
            if (_stageManager == null)
            {
                Debug.LogError("[ShadowMatchGameManager] _stageManager is not assigned.");
                return;
            }

            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;

            if (_shadowObjectController != null)
                _shadowObjectController.OnHintCountChanged += OnHintCountChanged;

            if (_instructionPanel != null)
            {
                _instructionPanel.OnDismissed += StartGame;
                _instructionPanel.Show("006", "ShadowMatch",
                    "オブジェクトを回転させて影を目標の形に合わせるパズル",
                    "ドラッグでオブジェクトを回転",
                    "影のシルエットを目標に一致させよう");
            }
            else
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            _score = 0;
            _judgeCount = 0;
            if (_ui != null) _ui.UpdateScore(0);
            _stageManager.StartFromBeginning();
        }

        private void OnStageChanged(int stageIndex)
        {
            _state = GameState.Playing;
            _judgeCount = 0;

            if (_shadowObjectController != null)
                _shadowObjectController.SetupStage(stageIndex);

            if (_ui != null)
            {
                _ui.UpdateStage(stageIndex + 1, _stageManager.TotalStages);
                _ui.UpdateScore(_score);
                _ui.UpdateMatchRate(0f);
                _ui.UpdateJudgeCount(0);
                _ui.HideAllPanels();
            }
        }

        private void OnHintCountChanged(int count)
        {
            if (_ui != null) _ui.UpdateHintCount(count);
        }

        private void OnAllStagesCleared()
        {
            _state = GameState.Clear;
            if (_shadowObjectController != null) _shadowObjectController.SetActive(false);
            if (_ui != null) _ui.ShowClearPanel(_score);
        }

        public void OnJudgeButton()
        {
            if (_state != GameState.Playing) return;
            if (_shadowObjectController == null) return;

            _judgeCount++;
            if (_ui != null) _ui.UpdateJudgeCount(_judgeCount);

            float matchRate = _shadowObjectController.CalculateMatch();
            if (_ui != null) _ui.UpdateMatchRate(matchRate);

            float multiplier = _judgeCount == 1 ? 3f : _judgeCount == 2 ? 2f : _judgeCount == 3 ? 1.5f : 1f;
            int baseScore = Mathf.RoundToInt(matchRate * 100f * 100f);
            bool perfect = _judgeCount == 1 && matchRate >= 0.95f;
            int gained = Mathf.RoundToInt(baseScore * multiplier) + (perfect ? 500 : 0);
            _score += gained;
            if (_ui != null) _ui.UpdateScore(_score);

            if (matchRate >= 0.80f)
            {
                StartCoroutine(StageClearRoutine(_stageManager.CurrentStage, gained));
            }
            else
            {
                _shadowObjectController.PlayMissFeedback();
            }
        }

        private IEnumerator StageClearRoutine(int stageIndex, int gained)
        {
            _state = GameState.StageClear;
            if (_shadowObjectController != null) _shadowObjectController.PlayClearFeedback();
            yield return new WaitForSeconds(0.5f);

            // Guard against state change during wait (e.g., OnAllStagesCleared)
            if (_state != GameState.StageClear) yield break;

            int stars = _judgeCount == 1 ? 3 : _judgeCount <= 2 ? 2 : 1;
            if (_ui != null) _ui.ShowStageClearPanel(stageIndex + 1, _score, stars);
        }

        public void OnNextStageButtonPressed()
        {
            if (_state != GameState.StageClear) return;
            _stageManager.CompleteCurrentStage();
        }

        public void OnHintButton()
        {
            if (_state != GameState.Playing) return;
            if (_shadowObjectController != null) _shadowObjectController.ShowHint();
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
            if (_instructionPanel != null)
                _instructionPanel.OnDismissed -= StartGame;
            if (_shadowObjectController != null)
                _shadowObjectController.OnHintCountChanged -= OnHintCountChanged;
        }
    }
}
