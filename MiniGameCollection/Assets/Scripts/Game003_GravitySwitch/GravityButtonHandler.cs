using UnityEngine;
using UnityEngine.UI;

namespace Game003_GravitySwitch
{
    /// <summary>
    /// 重力方向ボタンと GravityManager を繋ぐコンポーネント。
    /// SetupスクリプトでButtonに追加し、_direction を設定して使用する。
    /// 入力処理は全てこのクラスを経由して GravityManager に委譲する。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GravityButtonHandler : MonoBehaviour
    {
        [SerializeField] private GravityManager _gravityManager;
        [SerializeField] private int _direction; // 0=Up, 1=Down, 2=Left, 3=Right

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() => _gravityManager?.ApplyGravity(_direction));
        }
    }
}
