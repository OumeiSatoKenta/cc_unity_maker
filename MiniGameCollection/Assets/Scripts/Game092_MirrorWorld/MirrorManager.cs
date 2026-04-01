using UnityEngine;
using UnityEngine.InputSystem;

namespace Game092_MirrorWorld
{
    public class MirrorManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private MirrorWorldGameManager _gameManager;
        [SerializeField, Tooltip("上プレイヤー")] private Sprite _playerTopSprite;
        [SerializeField, Tooltip("下プレイヤー")] private Sprite _playerBotSprite;
        [SerializeField, Tooltip("ゴール")] private Sprite _goalSprite;
        [SerializeField, Tooltip("トラップ")] private Sprite _trapSprite;
        [SerializeField, Tooltip("壁")] private Sprite _wallSprite;

        private Camera _mainCamera;
        private bool _isActive;
        private int _gridSize = 5;
        private float _cellSize = 1f;
        private int _topR, _topC, _botR, _botC;
        private int _goalTopR, _goalTopC, _goalBotR, _goalBotC;
        private GameObject _topPlayer, _botPlayer;
        private float _topOffsetY = 2f;
        private float _botOffsetY = -3f;

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame() { _isActive = true; SetupStage(); }
        public void StopGame() { _isActive = false; }
        public void NextStage() { SetupStage(); }

        private void SetupStage()
        {
            _topR = 0; _topC = 0; _botR = 0; _botC = _gridSize - 1;
            _goalTopR = _gridSize - 1; _goalTopC = _gridSize - 1;
            _goalBotR = _gridSize - 1; _goalBotC = 0;

            if (_topPlayer != null) Destroy(_topPlayer);
            if (_botPlayer != null) Destroy(_botPlayer);

            _topPlayer = CreateObj("TopPlayer", _playerTopSprite, _topR, _topC, _topOffsetY, 5);
            _botPlayer = CreateObj("BotPlayer", _playerBotSprite, _botR, _botC, _botOffsetY, 5);
            CreateObj("GoalTop", _goalSprite, _goalTopR, _goalTopC, _topOffsetY, 2);
            CreateObj("GoalBot", _goalSprite, _goalBotR, _goalBotC, _botOffsetY, 2);
        }

        private GameObject CreateObj(string name, Sprite sprite, int r, int c, float yOffset, int order)
        {
            var obj = new GameObject(name);
            obj.transform.position = CellToWorld(r, c, yOffset);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.sortingOrder = order;
            obj.transform.localScale = Vector3.one * 0.35f;
            return obj;
        }

        private void Update()
        {
            if (!_isActive) return;
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = -_mainCamera.transform.position.z;
            Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

            Vector2 topPos = CellToWorld(_topR, _topC, _topOffsetY);
            Vector2 diff = wp - topPos;
            int dr = 0, dc = 0;
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y)) dc = diff.x > 0 ? 1 : -1;
            else dr = diff.y > 0 ? -1 : 1;

            // Top moves normally, bottom moves mirrored (dr inverted)
            int newTopR = Mathf.Clamp(_topR + dr, 0, _gridSize - 1);
            int newTopC = Mathf.Clamp(_topC + dc, 0, _gridSize - 1);
            int newBotR = Mathf.Clamp(_botR - dr, 0, _gridSize - 1); // mirrored
            int newBotC = Mathf.Clamp(_botC + dc, 0, _gridSize - 1);

            _topR = newTopR; _topC = newTopC;
            _botR = newBotR; _botC = newBotC;

            _topPlayer.transform.position = CellToWorld(_topR, _topC, _topOffsetY);
            _botPlayer.transform.position = CellToWorld(_botR, _botC, _botOffsetY);
            _gameManager.OnPlayerMoved();

            // Check goals
            bool topAtGoal = (_topR == _goalTopR && _topC == _goalTopC);
            bool botAtGoal = (_botR == _goalBotR && _botC == _goalBotC);
            if (topAtGoal && botAtGoal) _gameManager.OnBothReachedGoal();
        }

        private Vector3 CellToWorld(int r, int c, float yOffset)
        {
            float totalW = _gridSize * _cellSize;
            float startX = -totalW / 2f + _cellSize / 2f;
            float startY = (_gridSize - 1) * _cellSize / 2f;
            return new Vector3(startX + c * _cellSize, startY - r * _cellSize + yOffset, 0f);
        }
    }
}
