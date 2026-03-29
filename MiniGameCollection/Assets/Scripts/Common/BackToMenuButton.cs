using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全ゲームシーンに配置する「メニューへ戻る」ボタン。
/// Button コンポーネントの OnClick に自動でリスナーを登録する。
/// </summary>
[RequireComponent(typeof(Button))]
public class BackToMenuButton : MonoBehaviour
{
    private void Awake()
    {
        var button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[BackToMenuButton] Button コンポーネントが見つかりません");
            return;
        }

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        SceneLoader.BackToMenu();
    }
}
