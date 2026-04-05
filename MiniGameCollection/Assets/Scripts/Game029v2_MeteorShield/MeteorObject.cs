using UnityEngine;
using System.Collections;

namespace Game029v2_MeteorShield
{
    public enum MeteorType
    {
        Small,
        Large,
        Split
    }

    public class MeteorObject : MonoBehaviour
    {
        public MeteorType MeteorType { get; private set; }
        public int HP { get; private set; }
        public bool IsDeflected { get; private set; }

        [SerializeField] SpriteRenderer _sr;
        MeteorSpawner _spawner;
        Vector2 _velocity;
        bool _isActive;
        float _speedMultiplier;
        bool _isDestroying;

        static readonly Color NearColor = new Color(1f, 0.3f, 0.3f, 1f);
        static readonly Color FarColor = Color.white;

        public void Initialize(MeteorType type, Vector2 startPos, Vector2 direction, float speed, MeteorSpawner spawner)
        {
            MeteorType = type;
            _spawner = spawner;
            _isActive = true;
            _isDestroying = false;
            IsDeflected = false;

            HP = (type == MeteorType.Large) ? 2 : 1;
            _speedMultiplier = speed;
            _velocity = direction.normalized * speed;
            transform.position = startPos;

            // サイズ設定
            float scale = type == MeteorType.Large ? 1.2f : (type == MeteorType.Split ? 0.8f : 0.6f);
            transform.localScale = new Vector3(scale, scale, 1f);
            _sr.color = FarColor;
        }

        void Update()
        {
            if (!_isActive || _isDestroying) return;

            transform.position += (Vector3)_velocity * Time.deltaTime;

            // 画面外（上・左・右）に出たら削除
            float camH = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camW = Camera.main != null ? camH * Camera.main.aspect : 3f;
            Vector3 pos = transform.position;
            if (pos.y > camH + 1f || pos.x < -camW - 1f || pos.x > camW + 1f)
            {
                DestroyMeteor(false);
                return;
            }

            // 危険度に応じて色変化（星はy=-3.5付近）
            float dangerRatio = Mathf.Clamp01((-pos.y - 1f) / 3f);
            _sr.color = Color.Lerp(FarColor, NearColor, dangerRatio);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_isDestroying || !_isActive) return;

            if (other.CompareTag("Star"))
            {
                float damage = MeteorType == MeteorType.Large ? 20f : 10f;
                _spawner?.OnMeteorHitStar(this, damage);
                DestroyMeteor(false);
            }
        }

        // Called by ShieldController when this meteor is hit by shield
        public bool OnShieldHit(Vector2 shieldPos, float shieldWidth)
        {
            if (_isDestroying || !_isActive) return false;
            if (HP <= 0) return false;

            HP--;
            if (HP > 0)
            {
                // 大隕石は2回目のヒットまで待つ（点滅）
                StartCoroutine(FlashEffect());
                // 速度増加
                _velocity = new Vector2(_velocity.x * 0.5f, Mathf.Abs(_velocity.y) * 0.8f);
                return false;
            }

            // 弾き返し処理
            IsDeflected = true;
            float hitOffset = (transform.position.x - shieldPos.x) / (shieldWidth * 0.5f);
            hitOffset = Mathf.Clamp(hitOffset, -1f, 1f);
            float reflectAngle = hitOffset * 60f; // -60度〜+60度
            float speed = _velocity.magnitude * 1.1f;
            float radUp = Mathf.Deg2Rad * (90f + reflectAngle);
            _velocity = new Vector2(Mathf.Cos(radUp) * speed, Mathf.Sin(radUp) * speed);

            StartCoroutine(DeflectEffect());

            // 分裂隕石の場合は分裂
            if (MeteorType == MeteorType.Split)
            {
                _spawner?.SpawnSplitFragments(transform.position);
                DestroyMeteor(true);
                return true;
            }

            return true;
        }

        // Called when deflected meteor hits another meteor
        public void OnChainHit()
        {
            if (_isDestroying || !_isActive) return;
            StartCoroutine(ChainKillEffect());
            DestroyMeteor(true);
        }

        void DestroyMeteor(bool deflected)
        {
            if (_isDestroying) return;
            _isDestroying = true;
            _isActive = false;
            StopAllCoroutines();
            _spawner?.OnMeteorDestroyed(this, deflected);
            Destroy(gameObject);
        }

        IEnumerator FlashEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                _sr.color = Color.white;
                yield return new WaitForSeconds(0.06f);
                _sr.color = NearColor;
                yield return new WaitForSeconds(0.06f);
            }
            _sr.color = FarColor;
        }

        IEnumerator DeflectEffect()
        {
            Vector3 orig = transform.localScale;
            Vector3 big = orig * 1.3f;
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(orig, big, t / 0.075f <= 1f ? t / 0.075f : 2f - t / 0.075f);
                yield return null;
            }
            transform.localScale = orig;
        }

        IEnumerator ChainKillEffect()
        {
            _sr.color = new Color(1f, 0.9f, 0.2f, 1f);
            Vector3 orig = transform.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = 1f - t / 0.2f;
                transform.localScale = orig * (1f + ratio * 0.5f);
                _sr.color = new Color(1f, 0.9f, 0.2f, ratio);
                yield return null;
            }
        }
    }
}
