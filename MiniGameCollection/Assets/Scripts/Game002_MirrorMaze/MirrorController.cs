using UnityEngine;

namespace Game002_MirrorMaze
{
    public enum MirrorType { Slash, Backslash }

    public class MirrorController : MonoBehaviour
    {
        public Vector2Int GridPos { get; set; }
        public MirrorType Type { get; private set; }

        private SpriteRenderer _sr;

        public void Init(Vector2Int gridPos, MirrorType type, Sprite slashSprite, Sprite backslashSprite)
        {
            GridPos = gridPos;
            Type = type;
            _sr = GetComponent<SpriteRenderer>();
            UpdateSprite(slashSprite, backslashSprite);
        }

        public void UpdateSprite(Sprite slashSprite, Sprite backslashSprite)
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            _sr.sprite = Type == MirrorType.Slash ? slashSprite : backslashSprite;
        }
    }
}
