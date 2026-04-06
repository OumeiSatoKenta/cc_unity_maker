using UnityEngine;
using System.Collections;

namespace Game055v2_DustSweep
{
    public class DustSweepGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] DustBoard _dustBoard;
        [SerializeField] DustSweepUI _ui;

        public enum GameState { Idle, Playing, StageClear, GameClear, GameOver }
        GameState _state = GameState.Idle;

        int _score;
        int _comboCount;
        float _totalSwipeDistance;
        int _itemsFound;

        public int Score => _score;
        public int ComboCount => _comboCount;

        void Start()
        {
            _instructionPanel.Show(
                "055",
                "DustSweep",
                "砂埃をスワイプで拭き取るクリーニングゲーム",
                "画面をドラッグして砂埃を除去しよう。ブラシサイズも切り替えられる！",
                "清潔度100%を達成してステージクリア！"
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
            _totalSwipeDistance = 0f;
            _itemsFound = 0;
            int stageNumber = stage + 1; // 0-based -> 1-based
            _dustBoard.SetupStage(_stageManager.GetCurrentStageConfig(), stageNumber);
            _ui.OnStageChanged(stageNumber);
        }

        void OnAllStagesCleared()
        {
            _state = GameState.GameClear;
            _ui.ShowGameClear(_score);
        }

        public bool IsPlaying() => _state == GameState.Playing;

        public void OnCleanlinessUpdate(float cleanliness)
        {
            _ui?.UpdateCleanliness(cleanliness);
        }

        public void OnTimerUpdate(float remaining)
        {
            _ui?.UpdateTimer(remaining);
        }

        public void OnSwipe(Vector2 worldPos, float brushRadius)
        {
            if (_state != GameState.Playing) return;
        }

        public void OnDustCleared(float amount)
        {
            // called from DustBoard when dust is cleared
        }

        public void OnSwipeDistance(float dist)
        {
            _totalSwipeDistance += dist;
        }

        public void OnCleanlinessReached100()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;

            // calculate score
            int timeBonus = Mathf.RoundToInt(_dustBoard.RemainingTime * 50f);
            int itemBonus = _itemsFound * 200;
            float completeMult = (_itemsFound >= _dustBoard.TotalItems && _dustBoard.TotalItems > 0) ? 2.0f : 1.0f;
            float efficiencyMult = CalcEfficiencyMult();
            _score = Mathf.RoundToInt((timeBonus + itemBonus) * completeMult * efficiencyMult);

            _ui.ShowStageClear(_score);
        }

        float CalcEfficiencyMult()
        {
            // max swipe distance heuristic: boardSize * 10
            float maxDist = 80f;
            float ratio = Mathf.Clamp01(_totalSwipeDistance / maxDist);
            return Mathf.Lerp(1.5f, 1.0f, ratio);
        }

        public void OnItemFound()
        {
            _itemsFound++;
            _comboCount++;
            _ui.ShowCombo(_comboCount);
        }

        public void OnDangerZoneHit()
        {
            _comboCount = 0;
            _dustBoard.SubtractTime(5f);
            _ui.ShowPenalty();
            StartCoroutine(CameraShake());
        }

        IEnumerator CameraShake()
        {
            var cam = Camera.main.transform;
            Vector3 origin = cam.position;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                cam.position = origin + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.position = origin;
        }

        public void OnTimeUp()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.GameOver;
            _ui.ShowGameOver();
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
