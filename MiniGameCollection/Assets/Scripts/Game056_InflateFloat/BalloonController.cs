using UnityEngine;

namespace Game056_InflateFloat
{
    public class BalloonController : MonoBehaviour
    {
        private InflateFloatGameManager _gameManager;
        private bool _hasFinished;

        public void Initialize(InflateFloatGameManager gm)
        {
            _gameManager = gm;
            _hasFinished = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasFinished || _gameManager == null) return;
            if (other.gameObject.name == "Goal")
            {
                _hasFinished = true;
                _gameManager.OnReachedGoal();
            }
            else if (other.gameObject.name == "Spike")
            {
                _hasFinished = true;
                _gameManager.OnBalloonPopped();
            }
        }
    }
}
