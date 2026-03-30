using UnityEngine;

namespace Game013_SymmetryDraw
{
    /// <summary>
    /// SymmetryDraw のゲーム全体を制御する。
    /// お手本との一致判定、ステージ管理、クリア判定を担当する。
    /// </summary>
    public class SymmetryDrawGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("キャンバス管理コンポーネント")]
        private CanvasDrawManager _canvasDrawManager;

        [SerializeField, Tooltip("UI管理コンポーネント")]
        private SymmetryDrawUI _ui;

        [SerializeField, Tooltip("ステージデータ管理")]
        private StageData _stageData;

        private int _currentStage;
        private int _strokeCount;
        private bool _isCleared;

        public int StrokeCount => _strokeCount;

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            _currentStage = 0;
            _strokeCount = 0;
            _isCleared = false;

            if (_ui != null)
            {
                _ui.UpdateStrokeCount(_strokeCount);
                _ui.HideClearPanel();
            }

            LoadStage(_currentStage);
        }

        private void LoadStage(int stageIndex)
        {
            if (_stageData == null || _canvasDrawManager == null) return;

            var pattern = _stageData.GetPattern(stageIndex);
            _canvasDrawManager.Initialize(pattern);
        }

        /// <summary>
        /// ストロークが完了したときに呼ばれる。
        /// </summary>
        public void OnStrokeCompleted()
        {
            if (_isCleared) return;

            _strokeCount++;
            if (_ui != null) _ui.UpdateStrokeCount(_strokeCount);

            // 一致判定
            if (_canvasDrawManager != null && _canvasDrawManager.CheckMatch())
            {
                _isCleared = true;
                if (_ui != null) _ui.ShowClearPanel(_strokeCount);
            }
        }

        public void RestartGame()
        {
            StartGame();
        }
    }
}
