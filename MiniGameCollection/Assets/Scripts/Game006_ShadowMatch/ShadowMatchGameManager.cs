using UnityEngine;
using UnityEngine.Events;

namespace Game006_ShadowMatch
{
    public class ShadowMatchGameManager : MonoBehaviour
    {
        [SerializeField] private RotationController _rotController;
        [SerializeField] private ShadowMatchUI _ui;
        [SerializeField] private Transform _targetTransform; // static target silhouette

        // Target angles for each level (degrees, Z rotation)
        private static readonly float[] TargetAngles = { 50f, 130f, 220f };

        public bool IsPlaying { get; private set; }

        public UnityEvent<int> OnLevelCleared = new();

        private int _currentLevel;

        private void Start() => LoadLevel(0);

        public void LoadLevel(int level)
        {
            _currentLevel = Mathf.Clamp(level, 0, TargetAngles.Length - 1);
            IsPlaying = true;

            float angle = TargetAngles[_currentLevel];
            if (_targetTransform)
                _targetTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            _rotController?.SetLevel(angle);
            _ui?.SetLevelText($"Level {_currentLevel + 1} / {TargetAngles.Length}");
            _ui?.HideClearPanel();
        }

        public void OnSolved()
        {
            IsPlaying = false;
            OnLevelCleared?.Invoke(_currentLevel);
        }

        public void LoadNextLevel()
        {
            LoadLevel((_currentLevel + 1) % TargetAngles.Length);
        }

        public void LoadMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu");
        }
    }
}
