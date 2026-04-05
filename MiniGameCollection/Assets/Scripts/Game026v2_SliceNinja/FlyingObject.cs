using UnityEngine;
using System.Collections;

namespace Game026v2_SliceNinja
{
    public enum ObjectType
    {
        Fruit,
        Bomb,
        FrozenFruit,
        MiniFruit,
        StealthBomb
    }

    public class FlyingObject : MonoBehaviour
    {
        SpriteRenderer _spriteRenderer;
        Sprite _fruitSprite;
        Sprite _bombSprite;
        Sprite _frozenFruitSprite;
        Sprite _miniFruitSprite;

        public ObjectType Type { get; private set; }
        public bool IsBomb => Type == ObjectType.Bomb || Type == ObjectType.StealthBomb;
        public bool IsSliced { get; private set; }

        int _hitRequired = 1;
        int _hitCount = 0;
        SliceNinjaGameManager _gameManager;
        Camera _mainCamera;
        float _camSize;

        static readonly Color FrozenColor = new Color(0.6f, 0.85f, 1f);
        static readonly Color BombColor = new Color(0.9f, 0.2f, 0.2f);

        public void SetSprites(Sprite fruit, Sprite bomb, Sprite frozen, Sprite mini, SpriteRenderer sr)
        {
            _fruitSprite = fruit;
            _bombSprite = bomb;
            _frozenFruitSprite = frozen;
            _miniFruitSprite = mini;
            _spriteRenderer = sr;
        }

        public void Initialize(ObjectType type, SliceNinjaGameManager gm, Camera cam)
        {
            Type = type;
            _gameManager = gm;
            _mainCamera = cam;
            _camSize = cam.orthographicSize;
            IsSliced = false;
            _hitCount = 0;
            _hitRequired = 1; // always reset

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            ApplyTypeAppearance();

            if (type == ObjectType.MiniFruit)
                transform.localScale = Vector3.one * 0.55f;
            else
                transform.localScale = Vector3.one;

            if (type == ObjectType.Bomb)
                StartCoroutine(BlinkEffect());
            if (type == ObjectType.FrozenFruit)
                StartCoroutine(BlinkEffect());
            if (type == ObjectType.StealthBomb)
                StartCoroutine(StealthRevealEffect());
        }

        void ApplyTypeAppearance()
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.color = Color.white;
            switch (Type)
            {
                case ObjectType.Fruit:
                    if (_fruitSprite) _spriteRenderer.sprite = _fruitSprite;
                    break;
                case ObjectType.Bomb:
                    if (_bombSprite) _spriteRenderer.sprite = _bombSprite;
                    _spriteRenderer.color = BombColor;
                    break;
                case ObjectType.FrozenFruit:
                    if (_frozenFruitSprite) _spriteRenderer.sprite = _frozenFruitSprite;
                    _spriteRenderer.color = FrozenColor;
                    _hitRequired = 2;
                    break;
                case ObjectType.MiniFruit:
                    if (_miniFruitSprite) _spriteRenderer.sprite = _miniFruitSprite;
                    break;
                case ObjectType.StealthBomb:
                    if (_fruitSprite) _spriteRenderer.sprite = _fruitSprite; // disguised as fruit
                    _spriteRenderer.color = Color.white;
                    break;
            }
        }

        IEnumerator BlinkEffect()
        {
            while (!IsSliced && this != null)
            {
                if (IsSliced) yield break;
                float t = Mathf.PingPong(Time.time * 3f, 1f);
                if (_spriteRenderer != null)
                {
                    if (Type == ObjectType.Bomb)
                        _spriteRenderer.color = Color.Lerp(BombColor, Color.red, t);
                    else if (Type == ObjectType.FrozenFruit)
                        _spriteRenderer.color = Color.Lerp(FrozenColor, Color.cyan, t);
                }
                yield return null;
            }
        }

        IEnumerator StealthRevealEffect()
        {
            // Stealth bomb looks like fruit for 1.5s, then reveals
            yield return new WaitForSeconds(1.5f);
            if (IsSliced || this == null) yield break;

            // Quick flash to hint
            for (int i = 0; i < 3; i++)
            {
                if (IsSliced || _spriteRenderer == null) yield break;
                _spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.08f);
                if (IsSliced || _spriteRenderer == null) yield break;
                _spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.08f);
            }

            if (!IsSliced && _spriteRenderer != null)
            {
                if (_bombSprite) _spriteRenderer.sprite = _bombSprite;
                _spriteRenderer.color = BombColor;
            }
        }

        // Returns true if this call resulted in a completed slice
        public bool TrySlice()
        {
            if (IsSliced) return false;

            _hitCount++;
            if (_hitCount < _hitRequired)
            {
                StartCoroutine(CrackFlash());
                return false;
            }

            IsSliced = true;

            if (IsBomb)
            {
                StartCoroutine(BombExplodeEffect(() =>
                {
                    if (_gameManager != null) _gameManager.OnBombCut();
                    Destroy(gameObject);
                }));
            }
            else
            {
                StartCoroutine(SliceEffect(() => Destroy(gameObject)));
                if (_gameManager != null) _gameManager.OnObjectSliced(Type);
            }
            return true;
        }

        IEnumerator CrackFlash()
        {
            if (_spriteRenderer == null) yield break;
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (!IsSliced && _spriteRenderer != null)
                _spriteRenderer.color = FrozenColor;
        }

        IEnumerator SliceEffect(System.Action onComplete)
        {
            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < 0.2f)
            {
                if (this == null) yield break;
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float s = 1f + 0.4f * Mathf.Sin(ratio * Mathf.PI);
                transform.localScale = startScale * s;
                if (_spriteRenderer != null)
                    _spriteRenderer.color = new Color(1f, 1f, 0.5f, 1f - ratio);
                yield return null;
            }
            onComplete?.Invoke();
        }

        IEnumerator BombExplodeEffect(System.Action onComplete)
        {
            float t = 0f;
            while (t < 0.3f)
            {
                if (this == null) yield break;
                t += Time.deltaTime;
                float ratio = t / 0.3f;
                transform.localScale = Vector3.one * (1f + ratio * 2f);
                if (_spriteRenderer != null)
                    _spriteRenderer.color = new Color(1f, 0.3f * (1f - ratio), 0f, 1f - ratio);
                yield return null;
            }
            onComplete?.Invoke();
        }

        void Update()
        {
            // Check if fallen off screen
            if (this == null || IsSliced) return;
            if (transform.position.y < -_camSize - 1.5f)
            {
                IsSliced = true; // prevent double call
                if (_gameManager != null) _gameManager.OnObjectMissed(Type);
                Destroy(gameObject);
            }
        }
    }
}
