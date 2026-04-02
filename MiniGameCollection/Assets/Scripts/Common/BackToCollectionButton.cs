using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// コレクション選択画面に戻るボタン。TopMenuのヘッダーに配置。
/// </summary>
[RequireComponent(typeof(Button))]
public class BackToCollectionButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        SceneLoader.BackToCollectionSelect();
    }
}
