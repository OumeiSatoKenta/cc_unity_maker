using UnityEngine;
using System.Collections;

namespace Game032v2_SpinCutter
{
    public class BladeController : MonoBehaviour
    {
        float _radius;
        float _angularSpeed; // degrees per second
        float _duration;
        float _elapsed;
        Vector3 _pivot;
        float _currentAngle;
        bool _active;
        SpinCutterMechanic _mechanic;
        int _enemiesKilled;
        SpriteRenderer _sr;

        public int EnemiesKilled => _enemiesKilled;

        public void Initialize(Vector3 pivot, float radius, float angularSpeed, float duration, SpinCutterMechanic mechanic)
        {
            _pivot = pivot;
            _radius = radius;
            _angularSpeed = angularSpeed;
            _duration = duration;
            _mechanic = mechanic;
            _active = true;
            _currentAngle = 0f;
            _elapsed = 0f;
            _enemiesKilled = 0;
            _sr = GetComponent<SpriteRenderer>();

            // Start at right side of pivot
            transform.position = pivot + new Vector3(radius, 0f, 0f);
        }

        void Update()
        {
            if (!_active) return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration)
            {
                Die();
                return;
            }

            _currentAngle += _angularSpeed * Time.deltaTime;
            float rad = _currentAngle * Mathf.Deg2Rad;
            transform.position = _pivot + new Vector3(Mathf.Cos(rad) * _radius, Mathf.Sin(rad) * _radius, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, _currentAngle);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!_active) return;

            if (other.gameObject.CompareTag("Enemy"))
            {
                var enemy = other.GetComponent<EnemyController>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(this);
                    _enemiesKilled++;
                    StartCoroutine(ComboEffect());
                }
            }
            else if (other.gameObject.CompareTag("Obstacle"))
            {
                _mechanic.OnBladeHitObstacle();
                Die();
            }
        }

        IEnumerator ComboEffect()
        {
            Vector3 origScale = transform.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float ratio = t / 0.2f;
                float s = ratio < 0.5f ? Mathf.Lerp(1f, 1.4f, ratio * 2f) : Mathf.Lerp(1.4f, 1f, (ratio - 0.5f) * 2f);
                transform.localScale = origScale * s;
                yield return null;
            }
            transform.localScale = origScale;
        }

        void Die()
        {
            _active = false;
            StopAllCoroutines();
            _mechanic.OnBladeDied(this);
            Destroy(gameObject);
        }
    }
}
