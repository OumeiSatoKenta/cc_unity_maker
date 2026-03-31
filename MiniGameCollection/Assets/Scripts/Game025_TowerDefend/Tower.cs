using UnityEngine;

namespace Game025_TowerDefend
{
    public class Tower : MonoBehaviour
    {
        public float Range { get; set; } = 2.5f;
        public float FireRate { get; set; } = 1f;
        public int Damage { get; set; } = 1;

        private float _fireTimer;

        public void Initialize(float range, float fireRate, int damage)
        {
            Range = range;
            FireRate = fireRate;
            Damage = damage;
            _fireTimer = 0f;
        }

        private void Update()
        {
            _fireTimer -= Time.deltaTime;
        }

        public bool CanFire()
        {
            return _fireTimer <= 0f;
        }

        public void ResetFireTimer()
        {
            _fireTimer = 1f / FireRate;
        }
    }
}
