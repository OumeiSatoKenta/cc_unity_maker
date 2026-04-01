using UnityEngine;

namespace Game038_FlyBird
{
    public class ScoreTrigger : MonoBehaviour
    {
        private FlyBirdGameManager _gameManager;
        private bool _scored;

        public void Initialize(FlyBirdGameManager gm)
        {
            _gameManager = gm;
            _scored = false;
        }

        private void Update()
        {
            if (_scored || _gameManager == null) return;
            // 鳥がこのX座標を通過したらスコア加算
            var bird = Object.FindFirstObjectByType<FlyManager>();
            if (bird == null) return;

            // FlyManager の鳥Transform は private なので、位置比較で判定
            // ScoreTrigger のX座標が鳥のX座標を下回ったら通過とみなす
            if (transform.position.x < -2f) // BirdX = -2
            {
                _scored = true;
                _gameManager.AddScore(1);
            }
        }
    }
}
