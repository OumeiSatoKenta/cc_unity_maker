using UnityEngine;
using UnityEngine.UI;

namespace Game052v2_HammerNail
{
    public enum HitResult { Perfect, Good, Miss }

    public class TimingGauge : MonoBehaviour
    {
        [SerializeField] Image _indicatorImage;
        [SerializeField] Image _perfectZoneImage;
        [SerializeField] Image _goodZoneImage;

        public float GaugeValue { get; private set; } = 0f; // 0〜1
        public bool IsActive { get; private set; }

        float _speed = 1.0f;
        float _direction = 1f;
        float _perfectZoneHalf = 0.15f;
        float _goodZoneHalf = 0.10f; // around PERFECT
        bool _isIrregular;
        float _irregularTimer;
        float _irregularInterval = 1.5f;

        static readonly Color PerfectColor = new Color(1f, 0.92f, 0.2f, 0.9f);
        static readonly Color GoodColor = new Color(0.3f, 0.9f, 0.3f, 0.7f);
        static readonly Color MissColor = new Color(0.85f, 0.2f, 0.2f, 0.5f);

        public void SetupStage(float speedMultiplier, float complexityFactor)
        {
            _speed = speedMultiplier;
            _isIrregular = complexityFactor >= 0.5f;
            // perfectZoneHalf: stage1=0.15, stage2=0.11, stage3=0.10, stage4=0.075, stage5=0.075
            _perfectZoneHalf = Mathf.Lerp(0.15f, 0.075f, complexityFactor);
            _goodZoneHalf = _perfectZoneHalf + 0.08f;
            UpdateZoneImages();
        }

        public void StartGauge()
        {
            IsActive = true;
            GaugeValue = 0f;
            _direction = 1f;
            _irregularTimer = 0f;
        }

        public void StopGauge()
        {
            IsActive = false;
        }

        void Update()
        {
            if (!IsActive) return;

            float dt = Time.deltaTime;

            if (_isIrregular)
            {
                _irregularTimer += dt;
                if (_irregularTimer >= _irregularInterval)
                {
                    _irregularTimer = 0f;
                    _irregularInterval = Random.Range(0.8f, 2.5f);
                    float mult = Random.Range(0.5f, 1.8f);
                    _direction *= (Random.value > 0.3f ? 1f : -1f);
                    GaugeValue += _direction * _speed * mult * dt;
                }
                else
                {
                    GaugeValue += _direction * _speed * dt;
                }
            }
            else
            {
                GaugeValue += _direction * _speed * dt;
            }

            if (GaugeValue >= 1f) { GaugeValue = 1f; _direction = -1f; }
            if (GaugeValue <= 0f) { GaugeValue = 0f; _direction = 1f; }

            UpdateIndicator();
        }

        public HitResult GetHitResult()
        {
            float center = 0.5f;
            float dist = Mathf.Abs(GaugeValue - center);
            if (dist <= _perfectZoneHalf) return HitResult.Perfect;
            if (dist <= _goodZoneHalf) return HitResult.Good;
            return HitResult.Miss;
        }

        void UpdateIndicator()
        {
            if (_indicatorImage == null) return;
            var rt = _indicatorImage.rectTransform;
            var parentRt = rt.parent as RectTransform;
            if (parentRt == null) return;
            float width = parentRt.rect.width;
            rt.anchoredPosition = new Vector2(GaugeValue * width - width * 0.5f, rt.anchoredPosition.y);

            // Color indicator by zone
            HitResult zone = GetHitResult();
            _indicatorImage.color = zone switch {
                HitResult.Perfect => PerfectColor,
                HitResult.Good => GoodColor,
                _ => MissColor
            };
        }

        void UpdateZoneImages()
        {
            // Update perfect zone width
            if (_perfectZoneImage != null)
            {
                var rt = _perfectZoneImage.rectTransform;
                var parentRt = rt.parent as RectTransform;
                if (parentRt != null)
                {
                    float width = parentRt.rect.width;
                    rt.sizeDelta = new Vector2(_perfectZoneHalf * 2f * width, rt.sizeDelta.y);
                }
            }
            if (_goodZoneImage != null)
            {
                var rt = _goodZoneImage.rectTransform;
                var parentRt = rt.parent as RectTransform;
                if (parentRt != null)
                {
                    float width = parentRt.rect.width;
                    rt.sizeDelta = new Vector2(_goodZoneHalf * 2f * width, rt.sizeDelta.y);
                }
            }
        }
    }
}
