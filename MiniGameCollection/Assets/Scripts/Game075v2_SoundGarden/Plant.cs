using System.Collections;
using UnityEngine;

namespace Game075v2_SoundGarden
{
    public class Plant : MonoBehaviour
    {
        SpriteRenderer _spriteRenderer;

        Sprite _spriteLv0;
        Sprite _spriteLv1;
        Sprite _spriteLv2;

        // Growth: 0 ~ MaxGrowth (100 per level, max 300)
        public const int MaxGrowth = 300;
        public const int GrowthPerLevel = 100;

        int _growth;
        bool _isLit;
        bool _isCompleted;
        Coroutine _pulseCoroutine;
        Coroutine _flashCoroutine;
        Color _baseColor = Color.white;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public int PlotIndex { get; private set; }
        public int Growth => _growth;
        public bool IsCompleted => _isCompleted;
        public bool IsLit => _isLit;

        public void Initialize(int plotIndex, Sprite lv0, Sprite lv1, Sprite lv2)
        {
            PlotIndex = plotIndex;
            _spriteLv0 = lv0;
            _spriteLv1 = lv1;
            _spriteLv2 = lv2;
            _growth = 0;
            _isLit = false;
            _isCompleted = false;
            UpdateSprite();
        }

        public void ResetGrowth()
        {
            _growth = 0;
            _isLit = false;
            _isCompleted = false;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
                transform.localScale = Vector3.one;
            }
            UpdateSprite();
        }

        public void SetLit(bool lit)
        {
            _isLit = lit;
            if (_spriteRenderer != null)
                _spriteRenderer.color = lit ? new Color(1f, 1f, 0.5f) : _baseColor;
        }

        public void AddGrowth(int amount)
        {
            if (_isCompleted) return;
            _growth = Mathf.Clamp(_growth + amount, 0, MaxGrowth);
            UpdateSprite();

            if (_growth >= MaxGrowth && !_isCompleted)
            {
                _isCompleted = true;
                StartCoroutine(CompletionPulse());
            }
        }

        public void FlashMiss()
        {
            if (_isCompleted) return;
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(MissFlash());
        }

        public void PulseSuccess()
        {
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = StartCoroutine(SuccessPulse());
        }

        IEnumerator SuccessPulse()
        {
            float t = 0f;
            float duration = 0.15f;
            Vector3 baseScale = Vector3.one;
            while (t < duration)
            {
                t += Time.deltaTime;
                float ratio = t / duration;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.3f, ratio * 2f)
                    : Mathf.Lerp(1.3f, 1f, (ratio - 0.5f) * 2f);
                transform.localScale = baseScale * scale;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        IEnumerator MissFlash()
        {
            if (_spriteRenderer == null) yield break;
            _spriteRenderer.color = new Color(1f, 0.2f, 0.2f);
            yield return new WaitForSeconds(0.3f);
            _spriteRenderer.color = _isLit ? new Color(1f, 1f, 0.5f) : _baseColor;
        }

        IEnumerator CompletionPulse()
        {
            if (_spriteRenderer != null)
                _spriteRenderer.color = new Color(1f, 0.95f, 0.3f);
            float t = 0f;
            float duration = 0.3f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float ratio = t / duration;
                float scale = ratio < 0.5f
                    ? Mathf.Lerp(1f, 1.8f, ratio * 2f)
                    : Mathf.Lerp(1.8f, 1f, (ratio - 0.5f) * 2f);
                transform.localScale = Vector3.one * scale;
                yield return null;
            }
            transform.localScale = Vector3.one;
            if (_spriteRenderer != null)
                _spriteRenderer.color = new Color(0.8f, 1f, 0.8f);
        }

        void UpdateSprite()
        {
            if (_spriteRenderer == null) return;
            int level = _growth / GrowthPerLevel;
            if (level == 0) _spriteRenderer.sprite = _spriteLv0;
            else if (level == 1) _spriteRenderer.sprite = _spriteLv1;
            else _spriteRenderer.sprite = _spriteLv2;
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
