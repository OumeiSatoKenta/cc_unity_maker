using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を管理する静的ユーティリティクラス。
/// 全ゲームおよびTopMenuから使用される。
/// </summary>
public static class SceneLoader
{
    private const string TOP_MENU_SCENE = "TopMenu";
    private const string COLLECTION_SELECT_SCENE = "CollectionSelect";

    /// <summary>
    /// 現在選択中のコレクション名。TopMenuManager が参照する。
    /// </summary>
    public static string CurrentCollection { get; set; } = "classic";

    /// <summary>
    /// 指定のゲームシーンへ遷移する。
    /// InstructionPanel の表示フラグをリセットして毎回スタートボタンを表示する。
    /// </summary>
    public static void LoadGame(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] sceneName が null または空です");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// TopMenuシーンへ戻る。
    /// </summary>
    public static void BackToMenu()
    {
        SceneManager.LoadScene(TOP_MENU_SCENE);
    }

    /// <summary>
    /// コレクション選択画面へ遷移する。
    /// </summary>
    public static void LoadCollectionMenu(string collection)
    {
        CurrentCollection = collection;
        SceneManager.LoadScene(TOP_MENU_SCENE);
    }

    /// <summary>
    /// コレクション選択画面へ戻る。
    /// </summary>
    public static void BackToCollectionSelect()
    {
        SceneManager.LoadScene(COLLECTION_SELECT_SCENE);
    }
}
