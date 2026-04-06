using UnityEngine;

namespace Game054v2_FruitSlash
{
    public enum FruitType
    {
        Apple,
        Watermelon,
        Gold,
        Ice,
        Bomb,
        BigBomb
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class FruitObject : MonoBehaviour
    {
        public FruitType Type { get; private set; }
        public bool IsSliced { get; private set; }

        public int Score
        {
            get
            {
                return Type switch
                {
                    FruitType.Apple => 10,
                    FruitType.Watermelon => 30,
                    FruitType.Gold => 100,
                    FruitType.Ice => 15,
                    _ => 0
                };
            }
        }

        public bool IsBomb => Type == FruitType.Bomb || Type == FruitType.BigBomb;

        private Rigidbody2D _rb;
        private float _lifetime = 4f;
        private float _timer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(FruitType type, Vector2 launchVelocity)
        {
            Type = type;
            IsSliced = false;
            _timer = 0f;
            _rb.linearVelocity = launchVelocity;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
                Destroy(gameObject);
        }

        public void Slice()
        {
            if (IsSliced) return;
            IsSliced = true;
        }
    }
}
