using System.Collections;
using UnityEngine;

namespace Game053v2_SlideBlitz
{
    /// <summary>
    /// 個別タイルのデータとビジュアルフィードバックを管理する
    /// </summary>
    public class TileObject : MonoBehaviour
    {
        public int Number { get; private set; }
        public bool IsBlank { get; private set; }
        public bool IsFrozen { get; private set; }

        private SpriteRenderer _sr;
        private Color _baseColor;
        private bool _isAnimating;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Setup(int number, bool isBlank, bool isFrozen, Sprite normalSprite, Sprite frozenSprite, Sprite blankSprite)
        {
            Number = number;
            IsBlank = isBlank;
            IsFrozen = isFrozen;

            if (isBlank)
            {
                _sr.sprite = blankSprite;
                _sr.color = new Color(1f, 1f, 1f, 0.3f);
            }
            else if (isFrozen)
            {
                _sr.sprite = frozenSprite;
                _sr.color = Color.white;
            }
            else
            {
                _sr.sprite = normalSprite;
                _sr.color = Color.white;
            }
            _baseColor = _sr.color;
        }

        /// <summary>正しい位置に収まった演出: スケールポップ + 黄色フラッシュ</summary>
        public void PlayCorrectAnimation()
        {
            if (_isAnimating) return;
            StartCoroutine(CorrectAnimCoroutine());
        }

        private IEnumerator CorrectAnimCoroutine()
        {
            _isAnimating = true;
            // 黄色フラッシュ
            _sr.color = new Color(1f, 0.95f, 0.3f, 1f);
            // スケールポップ
            transform.localScale = Vector3.one;
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                float s = 1f + Mathf.Sin(t / 0.15f * Mathf.PI) * 0.3f;
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.one;
            // 色を戻す
            float fadeT = 0f;
            while (fadeT < 0.1f)
            {
                fadeT += Time.deltaTime;
                _sr.color = Color.Lerp(new Color(1f, 0.95f, 0.3f, 1f), _baseColor, fadeT / 0.1f);
                yield return null;
            }
            _sr.color = _baseColor;
            _isAnimating = false;
        }

        /// <summary>固定タイルを操作しようとした時: 赤フラッシュ + シェイク</summary>
        public void PlayFrozenAnimation()
        {
            if (_isAnimating) return;
            StartCoroutine(FrozenAnimCoroutine());
        }

        private IEnumerator FrozenAnimCoroutine()
        {
            _isAnimating = true;
            Vector3 origPos = transform.localPosition;
            _sr.color = new Color(1f, 0.3f, 0.3f, 1f);
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float shake = Mathf.Sin(elapsed * 50f) * 0.05f;
                transform.localPosition = origPos + new Vector3(shake, 0f, 0f);
                yield return null;
            }
            transform.localPosition = origPos;
            _sr.color = _baseColor;
            _isAnimating = false;
        }

        /// <summary>パズル完成時: 金色に変化するウェーブ演出</summary>
        public void PlayCompleteAnimation(float delay)
        {
            StartCoroutine(CompleteAnimCoroutine(delay));
        }

        private IEnumerator CompleteAnimCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            // 金色に変化
            float t = 0f;
            Color gold = new Color(1f, 0.84f, 0f, 1f);
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                _sr.color = Color.Lerp(_baseColor, gold, t / 0.2f);
                float s = 1f + Mathf.Sin(t / 0.2f * Mathf.PI) * 0.25f;
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            _sr.color = gold;
            transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
