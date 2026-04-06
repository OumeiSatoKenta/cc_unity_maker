using UnityEngine;
using System.Collections;

namespace Game050v2_BubbleSort
{
    public enum BubbleType { Normal, Fixed, Timer, Bomb }

    public class BubbleCell : MonoBehaviour
    {
        public int ColorIndex { get; private set; }
        public BubbleType BubbleType { get; private set; }
        public float TimerRemaining { get; private set; }
        public bool IsActive { get; private set; }
        public int GridCol { get; set; }
        public int GridRow { get; set; }

        private SpriteRenderer _sr;
        private bool _isTimerRunning;
        private float _timerDuration;
        private System.Action<BubbleCell> _onTimerExpired;
        private Coroutine _scaleCoroutine;
        private Vector3 _baseScale;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
        }

        public void Setup(int colorIndex, BubbleType type, Sprite sprite, int col, int row)
        {
            ColorIndex = colorIndex;
            BubbleType = type;
            GridCol = col;
            GridRow = row;
            IsActive = true;
            _isTimerRunning = false;
            if (_sr == null) _sr = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = sprite;
            _sr.color = Color.white;
            _baseScale = transform.localScale;
        }

        public void SetSprite(Sprite sprite)
        {
            if (_sr != null) _sr.sprite = sprite;
        }

        public void SetColorIndex(int colorIndex, Sprite sprite)
        {
            ColorIndex = colorIndex;
            if (_sr != null)
            {
                _sr.sprite = sprite;
                _sr.color = Color.white;
            }
        }

        public void StartTimer(float duration, float speedMult, System.Action<BubbleCell> onExpired)
        {
            _timerDuration = duration / speedMult;
            TimerRemaining = _timerDuration;
            _onTimerExpired = onExpired;
            _isTimerRunning = true;
        }

        public void StopTimer()
        {
            _isTimerRunning = false;
        }

        void Update()
        {
            if (!_isTimerRunning) return;
            TimerRemaining -= Time.deltaTime;
            if (TimerRemaining <= 0f)
            {
                _isTimerRunning = false;
                _onTimerExpired?.Invoke(this);
            }
        }

        public void SetHighlight(bool highlighted)
        {
            if (_sr == null) return;
            if (highlighted)
            {
                _baseScale = transform.localScale;
                transform.localScale = _baseScale * 1.15f;
                _sr.color = new Color(1f, 1f, 0.7f, 1f);
            }
            else
            {
                transform.localScale = _baseScale;
                _sr.color = Color.white;
            }
        }

        public void PlaySwapAnimation()
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(ScalePop());
        }

        public void PlayDissolveAnimation(System.Action onComplete)
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(DissolveOut(onComplete));
        }

        public void PlayTimerExpireFlash()
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(RedFlash());
        }

        IEnumerator ScalePop()
        {
            Vector3 orig = _baseScale;
            float t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, 1.3f, t / 0.1f);
                transform.localScale = orig * s;
                yield return null;
            }
            t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1.3f, 1f, t / 0.1f);
                transform.localScale = orig * s;
                yield return null;
            }
            transform.localScale = orig;
        }

        IEnumerator DissolveOut(System.Action onComplete)
        {
            // Flash white
            _sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            _sr.color = new Color(1f, 0.9f, 0.3f, 1f);
            yield return new WaitForSeconds(0.05f);

            Vector3 orig = transform.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, 0f, t / 0.2f);
                transform.localScale = orig * s;
                _sr.color = new Color(1f, 1f, 0.3f, 1f - t / 0.2f);
                yield return null;
            }
            transform.localScale = Vector3.zero;
            onComplete?.Invoke();
        }

        IEnumerator RedFlash()
        {
            _sr.color = new Color(1f, 0.2f, 0.2f, 1f);
            yield return new WaitForSeconds(0.15f);
            _sr.color = Color.white;
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            gameObject.SetActive(active);
        }
    }
}
