using UnityEngine;

namespace Game040v2_DashDungeon
{
    /// <summary>
    /// Bridges a UI Button to DashDungeonMechanic.OnDirectionInput().
    /// Attached to direction buttons by Setup040v2_DashDungeon.
    /// </summary>
    public class DirectionButtonHandler : MonoBehaviour
    {
        [SerializeField] DashDungeonMechanic _mechanic;
        [SerializeField] int _dirX;
        [SerializeField] int _dirY;

        public void OnClick()
        {
            if (_mechanic != null)
                _mechanic.OnDirectionInput(new Vector2Int(_dirX, _dirY));
        }
    }
}
