using UnityEngine;

namespace Game025_TowerDefend
{
    public class EnemyUnit : MonoBehaviour
    {
        public float Speed { get; set; }
        public int Health { get; private set; }
        public bool IsAlive => Health > 0;

        public void Initialize(float speed, int health)
        {
            Speed = speed;
            Health = health;
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (IsAlive)
                transform.position += Vector3.right * Speed * Time.deltaTime;
        }
    }
}
