using UnityEngine;

namespace Game009_ColorMix
{
    public class ColorMixManager : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _playerColorDisplay;
        [SerializeField] private SpriteRenderer _targetColorDisplay;

        private Color _targetColor;
        private Color _playerColor;
        private float _matchThreshold = 0.1f;
        private int _currentStage;

        private ColorMixGameManager _gameManager;

        public static int StageCount => 3;

        private void Awake()
        {
            _gameManager = GetComponentInParent<ColorMixGameManager>();
        }

        public void SetupStage(int stageIndex)
        {
            _currentStage = stageIndex;
            _targetColor = GetTargetColor(stageIndex);
            _playerColor = new Color(0.5f, 0.5f, 0.5f, 1f);

            if (_targetColorDisplay != null)
                _targetColorDisplay.color = _targetColor;
            if (_playerColorDisplay != null)
                _playerColorDisplay.color = _playerColor;
        }

        public void SetRedValue(float value)
        {
            _playerColor.r = value;
            UpdatePlayerDisplay();
            CheckMatch();
        }

        public void SetGreenValue(float value)
        {
            _playerColor.g = value;
            UpdatePlayerDisplay();
            CheckMatch();
        }

        public void SetBlueValue(float value)
        {
            _playerColor.b = value;
            UpdatePlayerDisplay();
            CheckMatch();
        }

        public Color GetPlayerColor() => _playerColor;
        public Color GetTargetColor() => _targetColor;

        public float GetMatchPercentage()
        {
            float dr = Mathf.Abs(_playerColor.r - _targetColor.r);
            float dg = Mathf.Abs(_playerColor.g - _targetColor.g);
            float db = Mathf.Abs(_playerColor.b - _targetColor.b);
            float avgDiff = (dr + dg + db) / 3f;
            return Mathf.Clamp01(1f - avgDiff) * 100f;
        }

        public bool IsMatched()
        {
            float dr = Mathf.Abs(_playerColor.r - _targetColor.r);
            float dg = Mathf.Abs(_playerColor.g - _targetColor.g);
            float db = Mathf.Abs(_playerColor.b - _targetColor.b);
            return dr < _matchThreshold && dg < _matchThreshold && db < _matchThreshold;
        }

        private void UpdatePlayerDisplay()
        {
            if (_playerColorDisplay != null)
                _playerColorDisplay.color = _playerColor;
        }

        private void CheckMatch()
        {
            if (_gameManager != null)
            {
                _gameManager.OnColorChanged(GetMatchPercentage());
                if (IsMatched())
                    _gameManager.OnColorMatched();
            }
        }

        private Color GetTargetColor(int index)
        {
            switch (index % StageCount)
            {
                case 0: return new Color(0.9f, 0.2f, 0.3f, 1f);  // Red-ish
                case 1: return new Color(0.2f, 0.7f, 0.4f, 1f);  // Green-ish
                case 2: return new Color(0.3f, 0.4f, 0.9f, 1f);  // Blue-ish
                default: return Color.red;
            }
        }
    }
}
