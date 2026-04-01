using UnityEngine;

namespace Game051_DrawBridge
{
    public class BridgeCharacter : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private DrawBridgeGameManager _gameManager;
        private bool _hasFinished;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasFinished || _gameManager == null) return;
            if (other.gameObject.name == "Goal")
            {
                _hasFinished = true;
                _gameManager.OnCharacterReachedGoal();
            }
            else if (other.gameObject.name == "FallZone")
            {
                _hasFinished = true;
                _gameManager.OnCharacterFallen();
            }
        }
    }
}
