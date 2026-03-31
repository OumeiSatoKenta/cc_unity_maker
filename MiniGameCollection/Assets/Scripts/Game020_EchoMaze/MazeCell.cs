using UnityEngine;

namespace Game020_EchoMaze
{
    public class MazeCell : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public bool IsWall { get; private set; }
        public bool IsRevealed { get; private set; }

        private SpriteRenderer _fogRenderer;
        private GameObject _fogObj;

        public void Initialize(Vector2Int gridPos, bool isWall, GameObject fogPrefab)
        {
            GridPosition = gridPos;
            IsWall = isWall;
            IsRevealed = false;

            if (fogPrefab != null && !isWall)
            {
                _fogObj = Instantiate(fogPrefab, transform);
                _fogObj.transform.localPosition = Vector3.zero;
                _fogRenderer = _fogObj.GetComponent<SpriteRenderer>();
            }
        }

        public void Reveal()
        {
            if (IsRevealed || IsWall) return;
            IsRevealed = true;
            if (_fogObj != null) _fogObj.SetActive(false);
        }

        public void SetEchoHint(float proximity)
        {
            // Proximity 0-1, closer = brighter hint through fog
            if (_fogRenderer != null && !IsRevealed)
            {
                var c = _fogRenderer.color;
                c.a = Mathf.Lerp(0.95f, 0.4f, proximity);
                _fogRenderer.color = c;
            }
        }
    }
}
