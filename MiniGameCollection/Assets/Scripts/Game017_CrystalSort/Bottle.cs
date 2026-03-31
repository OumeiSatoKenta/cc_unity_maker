using UnityEngine;
using TMPro;

namespace Game017_CrystalSort
{
    public class Bottle : MonoBehaviour
    {
        public int AcceptColorIndex { get; private set; }
        public int CurrentCount { get; private set; }
        public int MaxCapacity { get; private set; }

        private TextMeshPro _countText;

        public void Initialize(int acceptColor, int maxCapacity)
        {
            AcceptColorIndex = acceptColor;
            MaxCapacity = maxCapacity;
            CurrentCount = 0;
            _countText = GetComponentInChildren<TextMeshPro>();
            UpdateText();
        }

        public bool TryAdd()
        {
            if (CurrentCount >= MaxCapacity) return false;
            CurrentCount++;
            UpdateText();
            return true;
        }

        public bool IsFull()
        {
            return CurrentCount >= MaxCapacity;
        }

        private void UpdateText()
        {
            if (_countText != null)
                _countText.text = $"{CurrentCount}/{MaxCapacity}";
        }
    }
}
