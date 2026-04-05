using UnityEngine;
using System.Collections;

namespace Game032v2_SpinCutter
{
    public class EnemyController : MonoBehaviour
    {
        public bool IsDead { get; private set; }
        bool _isMoving;
        float _moveSpeed;
        float _moveRange;
        Vector3 _originPos;
        float _moveTimer;
        Vector3 _moveTarget;
        SpriteRenderer _sr;

        public void Initialize(bool isMoving, float moveSpeed = 1.5f, float moveRange = 1.2f)
        {
            IsDead = false;
            _isMoving = isMoving;
            _moveSpeed = moveSpeed;
            _moveRange = moveRange;
            _originPos = transform.position;
            _moveTarget = RandomTarget();
            _sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (IsDead || !_isMoving) return;

            transform.position = Vector3.MoveTowards(transform.position, _moveTarget, _moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _moveTarget) < 0.05f)
            {
                _moveTarget = RandomTarget();
            }
        }

        Vector3 RandomTarget()
        {
            float ox = _originPos.x + Random.Range(-_moveRange, _moveRange);
            float oy = _originPos.y + Random.Range(-_moveRange * 0.5f, _moveRange * 0.5f);
            return new Vector3(ox, oy, 0f);
        }

        public void TakeDamage(BladeController blade)
        {
            if (IsDead) return;
            IsDead = true;
            StartCoroutine(DeathEffect());
        }

        IEnumerator DeathEffect()
        {
            if (_sr != null)
            {
                float t = 0f;
                Color orig = _sr.color;
                while (t < 0.3f)
                {
                    t += Time.deltaTime;
                    float ratio = t / 0.3f;
                    _sr.color = Color.Lerp(Color.red, new Color(1f, 1f, 1f, 0f), ratio);
                    transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, ratio);
                    yield return null;
                }
            }
            Destroy(gameObject);
        }
    }
}
