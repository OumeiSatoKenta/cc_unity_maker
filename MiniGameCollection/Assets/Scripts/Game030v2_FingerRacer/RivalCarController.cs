using UnityEngine;

namespace Game030v2_FingerRacer
{
    /// <summary>
    /// Simple AI rival car that follows a fixed zigzag path (Stage 5 only).
    /// </summary>
    public class RivalCarController : MonoBehaviour
    {
        [SerializeField] FingerRacerGameManager _gameManager;
        [SerializeField] SpriteRenderer _rivalSr;

        Vector3[] _path;
        int _currentIndex;
        float _speed = 4.0f;
        bool _isActive;

        public void StartRival(Vector3 start, Vector3 goal)
        {
            // Build a simple S-curve path
            float camSize = Camera.main != null ? Camera.main.orthographicSize : 5f;
            float camW = camSize * (Camera.main != null ? Camera.main.aspect : 0.5625f);
            float steps = 8;
            _path = new Vector3[(int)steps + 2];
            _path[0] = start;
            for (int i = 1; i <= (int)steps; i++)
            {
                float t = i / steps;
                float y = Mathf.Lerp(start.y, goal.y, t);
                float x = (i % 2 == 0) ? -camW * 0.3f : camW * 0.3f;
                _path[i] = new Vector3(x, y, 0f);
            }
            _path[(int)steps + 1] = goal;
            _currentIndex = 0;
            _isActive = true;
            transform.position = start;
        }

        public void StopRival()
        {
            _isActive = false;
        }

        void Update()
        {
            if (!_isActive || _path == null || _currentIndex >= _path.Length) return;
            Vector3 target = _path[_currentIndex];
            Vector3 dir = (target - transform.position).normalized;
            if (dir.magnitude > 0.01f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 10f);
            }
            transform.position = Vector3.MoveTowards(transform.position, target, _speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target) < 0.1f)
                _currentIndex++;
        }

        public bool HasFinished => _path != null && _currentIndex >= _path.Length;
    }
}
