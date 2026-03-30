using UnityEngine;
using UnityEngine.Events;

namespace Game009_ColorMix
{
    /// <summary>
    /// ColorMix のコアロジックを担当。
    /// スライダー値を管理し、混合色を計算する。
    /// 入力処理はスライダーUIイベント経由で行う。
    /// </summary>
    public class ColorMixManager : MonoBehaviour
    {
        [SerializeField] private ColorMixUI _ui;
        [SerializeField] private ColorMixGameManager _gameManager;

        public UnityEvent<Color> OnMixChanged = new();

        private Color _targetColor;
        private float _tolerance;

        // 現在のスライダー値 (0-1)
        private float _red = 0f;
        private float _green = 0f;
        private float _blue = 0f;

        public Color CurrentMixedColor => new Color(_red, _green, _blue);
        public Color TargetColor => _targetColor;

        public void LoadLevel(Color targetColor, float tolerance)
        {
            _targetColor = targetColor;
            _tolerance = tolerance;
            _red = 0f;
            _green = 0f;
            _blue = 0f;

            _ui?.UpdateMixPreview(CurrentMixedColor);
            _ui?.UpdateTargetPreview(targetColor);
            _ui?.ResetSliders();
        }

        public void OnRedChanged(float value)
        {
            _red = value;
            NotifyMixChanged();
        }

        public void OnGreenChanged(float value)
        {
            _green = value;
            NotifyMixChanged();
        }

        public void OnBlueChanged(float value)
        {
            _blue = value;
            NotifyMixChanged();
        }

        public void SubmitMix()
        {
            _gameManager?.OnMixSubmitted(CurrentMixedColor);
        }

        private void NotifyMixChanged()
        {
            Color mixed = CurrentMixedColor;
            _ui?.UpdateMixPreview(mixed);
            OnMixChanged?.Invoke(mixed);
        }
    }
}
