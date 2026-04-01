using UnityEngine;

namespace Game048_GlassBall
{
    public class GlassBall : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")]
        private GlassBallGameManager _gameManager;

        private bool _hasFinished;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_hasFinished || _gameManager == null) return;
            float impact = collision.relativeVelocity.magnitude * 5f;
            if (impact > 1f) _gameManager.AddImpact(impact);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasFinished || _gameManager == null) return;
            if (other.gameObject.name == "Goal")
            {
                _hasFinished = true;
                _gameManager.OnBallReachedGoal();
            }
            else if (other.gameObject.name == "FallZone")
            {
                _hasFinished = true;
                _gameManager.OnBallFallen();
            }
        }
    }
}
