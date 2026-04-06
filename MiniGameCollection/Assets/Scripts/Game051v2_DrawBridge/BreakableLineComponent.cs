using UnityEngine;

namespace Game051v2_DrawBridge
{
    /// <summary>
    /// Attached to drawn lines in Stage 4 - breaks when ball collides with excessive weight
    /// </summary>
    public class BreakableLineComponent : MonoBehaviour
    {
        private float _lineLength;
        private float _threshold = float.MaxValue; // default: won't break until Initialize is called
        private bool _broken = false;
        private float _breakTimer = 0f;

        public void Initialize(float lineLength, float threshold)
        {
            _lineLength = lineLength;
            _threshold = threshold;
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (_broken) return;
            if (!col.gameObject.CompareTag("Ball")) return;

            if (_lineLength > _threshold)
            {
                _broken = true;
                // Flash red before breaking
                var lr = GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.startColor = Color.red;
                    lr.endColor = Color.red;
                }
            }
        }

        void Update()
        {
            if (!_broken) return;
            _breakTimer += Time.deltaTime;
            if (_breakTimer >= 0.3f)
            {
                Destroy(gameObject);
            }
        }
    }
}
