using UnityEngine;
using System.Collections;
using TMPro;

namespace Game059v2_WaterJug
{
    public class JugController : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _jugRenderer;
        [SerializeField] SpriteRenderer _waterRenderer;
        [SerializeField] TextMeshPro _amountText;
        [SerializeField] TextMeshPro _capacityText;
        [SerializeField] TextMeshPro _targetText;
        [SerializeField] BoxCollider2D _col;

        int _capacity;
        float _currentAmount;
        bool _isTarget;
        int _targetAmount;
        bool _isHighlighted;
        bool _isAnimating;

        static readonly Color NormalColor = new Color(0.55f, 0.85f, 0.6f, 1f);
        static readonly Color HighlightColor = new Color(1f, 0.95f, 0.3f, 1f);
        static readonly Color TargetColor = new Color(1f, 0.8f, 0.1f, 1f);
        static readonly Color AchievedColor = new Color(0.3f, 1f, 0.4f, 1f);

        public int Capacity => _capacity;
        public float CurrentAmount => _currentAmount;
        public bool IsEmpty => _currentAmount <= 0.001f;
        public bool IsFull => _currentAmount >= _capacity - 0.001f;
        public bool IsTarget => _isTarget;
        public int TargetAmount => _targetAmount;

        public void SetupJug(int capacity, float initialAmount, bool isTarget, int targetAmt, Sprite jugSprite, Sprite waterSprite)
        {
            _capacity = capacity;
            _currentAmount = Mathf.Clamp(initialAmount, 0, capacity);
            _isTarget = isTarget;
            _targetAmount = targetAmt;

            if (_jugRenderer != null && jugSprite != null)
                _jugRenderer.sprite = jugSprite;
            if (_waterRenderer != null && waterSprite != null)
                _waterRenderer.sprite = waterSprite;

            UpdateVisual();

            if (_isTarget && _targetText != null)
            {
                _targetText.text = $"目標: {_targetAmount}L";
                _targetText.color = TargetColor;
                _targetText.gameObject.SetActive(true);
            }
            else if (_targetText != null)
            {
                _targetText.gameObject.SetActive(false);
            }

            SetHighlight(false);
        }

        public float AddWater(float amount)
        {
            float canAdd = Mathf.Max(0f, _capacity - _currentAmount);
            float actual = Mathf.Min(amount, canAdd);
            _currentAmount += actual;
            UpdateVisual();
            return actual;
        }

        public float RemoveWater(float amount)
        {
            float actual = Mathf.Min(amount, _currentAmount);
            _currentAmount -= actual;
            UpdateVisual();
            return actual;
        }

        public void SetAmount(float amount)
        {
            _currentAmount = Mathf.Clamp(amount, 0, _capacity);
            UpdateVisual();
        }

        public bool CheckTargetAchieved()
        {
            if (!_isTarget) return false;
            return Mathf.Abs(_currentAmount - _targetAmount) < 0.001f;
        }

        public void SetHighlight(bool on)
        {
            _isHighlighted = on;
            if (_jugRenderer != null)
            {
                if (on)
                    _jugRenderer.color = HighlightColor;
                else if (_isTarget)
                    _jugRenderer.color = TargetColor;
                else
                    _jugRenderer.color = NormalColor;
            }
        }

        public void PlayAchieveAnimation()
        {
            StartCoroutine(AchieveCoroutine());
        }

        IEnumerator AchieveCoroutine()
        {
            if (_jugRenderer != null)
                _jugRenderer.color = AchievedColor;
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            while (elapsed < 0.25f)
            {
                float t = elapsed / 0.25f;
                float scale = 1f + 0.3f * Mathf.Sin(t * Mathf.PI);
                transform.localScale = originalScale * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = originalScale;
        }

        public void PlayErrorFlash()
        {
            StartCoroutine(ErrorCoroutine());
        }

        IEnumerator ErrorCoroutine()
        {
            Color orig = _jugRenderer != null ? _jugRenderer.color : Color.white;
            for (int i = 0; i < 3; i++)
            {
                if (_jugRenderer != null) _jugRenderer.color = new Color(1f, 0.2f, 0.2f, 1f);
                yield return new WaitForSeconds(0.07f);
                if (_jugRenderer != null) _jugRenderer.color = orig;
                yield return new WaitForSeconds(0.07f);
            }
        }

        void UpdateVisual()
        {
            if (_amountText != null)
                _amountText.text = $"{_currentAmount:0}L";
            if (_capacityText != null)
                _capacityText.text = $"/{_capacity}L";

            if (_waterRenderer != null)
            {
                float ratio = _capacity > 0 ? _currentAmount / _capacity : 0f;
                // Scale the water fill: height proportional to amount
                var s = _waterRenderer.transform.localScale;
                s.y = ratio;
                _waterRenderer.transform.localScale = s;
                // Position: bottom-aligned
                var lp = _waterRenderer.transform.localPosition;
                lp.y = -0.5f + ratio * 0.5f; // offset so bottom is fixed
                _waterRenderer.transform.localPosition = lp;
                _waterRenderer.color = new Color(0.2f, 0.6f, 0.9f, ratio > 0.01f ? 0.85f : 0f);
            }
        }
    }
}
