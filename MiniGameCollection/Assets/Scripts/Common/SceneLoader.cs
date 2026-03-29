using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を管理する静的ユーティリティクラス。
/// 全ゲームおよびTopMenuから使用される。
/// </summary>
public static class SceneLoader
{
    private const string TOP_MENU_SCENE = "TopMenu";

    /// <summary>
    /// 指定のゲームシーンへ遷移する。
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
}
