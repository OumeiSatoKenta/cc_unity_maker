using UnityEngine;

namespace Game011_FoldPaper
{
    /// <summary>
    /// FoldPaper のゲーム全体を制御する。
    /// ステージ管理、折り手数管理、クリア判定を担当する。
    /// </summary>
    public class FoldPaperGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("紙の折り畳み管理")]
        private PaperManager _paperManager;

        [SerializeField, Tooltip("UI管理コンポーネント")]
        private FoldPaperUI _ui;

        private int _foldCount;
        private int _currentStage;
        private bool _isCleared;

        public int FoldCount => _foldCount;
        public int CurrentStage => _currentStage;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _foldCount = 0;
            _currentStage = 0;
            _isCleared = false;

            if (_paperManager != null) _paperManager.InitializeStage(_currentStage);
            if (_ui != null) _ui.UpdateFoldCount(_foldCount);
            if (_ui != null) _ui.UpdateStageText(_currentStage);
            if (_ui != null) _ui.HideClearPanel();
        }

        /// <summary>
        /// 折りが実行されたときに呼ばれる。手数を加算しクリア判定を行う。
        /// </summary>
        public void OnFolded()
        {
            if (_isCleared) return;

            _foldCount++;
            if (_ui != null) _ui.UpdateFoldCount(_foldCount);

            if (_paperManager != null && _paperManager.CheckMatchesTarget())
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_foldCount);
            }
        }

        /// <summary>
        /// 一手戻す
        /// </summary>
        public void UndoFold()
        {
            if (_isCleared) return;
            if (_paperManager != null && _paperManager.UndoLastFold())
            {
                _foldCount = Mathf.Max(0, _foldCount - 1);
                if (_ui != null) _ui.UpdateFoldCount(_foldCount);
            }
        }

        /// <summary>
        /// 次のステージに進む
        /// </summary>
        public void NextStage()
        {
            _currentStage++;
            if (_currentStage >= PaperManager.TotalStages)
            {
                _currentStage = 0;
            }
            _foldCount = 0;
            _isCleared = false;
            if (_paperManager != null) _paperManager.InitializeStage(_currentStage);
            if (_ui != null) _ui.UpdateFoldCount(_foldCount);
            if (_ui != null) _ui.UpdateStageText(_currentStage);
            if (_ui != null) _ui.HideClearPanel();
        }

        public void RestartGame()
        {
            _foldCount = 0;
            _isCleared = false;
            if (_paperManager != null) _paperManager.InitializeStage(_currentStage);
            if (_ui != null) _ui.UpdateFoldCount(_foldCount);
            if (_ui != null) _ui.HideClearPanel();
        }
    }
}
