using UnityEngine;
using System.Collections;

namespace Game061v2_CookieFactory
{
    public class CookieFactoryGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] CookieManager _cookieManager;
        [SerializeField] CookieFactoryUI _ui;

        public enum GameState { Idle, Playing, StageClear, GameClear }
        GameState _state = GameState.Idle;

        void Start()
        {
            _instructionPanel.Show(
                "061v2",
                "CookieFactory",
                "タップでクッキーを焼いて工場を大きくしよう",
                "クッキーをタップして焼く・設備を買って自動化",
                "売上目標を達成して次のステージへ進もう"
            );
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _state = GameState.Playing;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            var config = _stageManager.GetCurrentStageConfig();
            _cookieManager.SetupStage(stageIndex, config);
            _ui.UpdateStageDisplay(stageIndex + 1);
        }

        void OnAllStagesCleared()
        {
            _state = GameState.GameClear;
            _ui.ShowGameClear(_cookieManager.TotalCookies);
        }

        public void OnStageClear()
        {
            if (_state != GameState.Playing) return;
            _state = GameState.StageClear;
            _ui.ShowStageClear(_cookieManager.TotalCookies);
        }

        public void OnNextStage()
        {
            if (_state != GameState.StageClear) return;
            _state = GameState.Playing;
            _ui.HideStageClear();
            _stageManager.CompleteCurrentStage();
        }

        public void OnRetry()
        {
            _state = GameState.Playing;
            _ui.HideGameClear();
            _cookieManager.ResetAll();
            _stageManager.StartFromBeginning();
        }

        public bool IsPlaying => _state == GameState.Playing;
    }
}
