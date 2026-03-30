using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game003_GravitySwitch;

/// <summary>
/// GravitySwitch のゲームシーンを自動構成する Editor スクリプト。
/// Assets > Setup > 003 GravitySwitch から実行する。
/// </summary>
public static class Setup003_GravitySwitch
{
    [MenuItem("Assets/Setup/003 GravitySwitch")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup003_GravitySwitch] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // --- カメラ設定 ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.16f);
            camera.orthographic = true;
            camera.orthographicSize = 5.0f;
        }

        // --- GameManager ---
        var gmGo = new GameObject("GameManager");
        var gameManager = gmGo.AddComponent<GravitySwitchGameManager>();

        // --- GravityManager (GameManager の子) ---
        var gravGo = new GameObject("GravityManager");
        gravGo.transform.SetParent(gmGo.transform);
        var gravityManager = gravGo.AddComponent<GravityManager>();

        var gravSO = new SerializedObject(gravityManager);
        gravSO.FindProperty("_gameManager").objectReferenceValue = gameManager;
        gravSO.FindProperty("_cellSize").floatValue = 1.0f;
        gravSO.ApplyModifiedProperties();

        // --- Canvas ---
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasGo.AddComponent<GraphicRaycaster>();

        // 手数テキスト（左上）
        var moveTextGo = CreateText(canvasGo.transform, "MoveCountText", "手数: 0", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(220f, 60f), new Vector2(20f, -30f));

        // タイトル（上中央）
        CreateText(canvasGo.transform, "TitleText", "GravitySwitch", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400f, 60f), new Vector2(0f, -30f));

        // メニューへ戻るボタン（右上）
        var menuBtnGo = CreateButton(canvasGo.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(220f, 55f), new Vector2(-20f, -30f),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtnGo.AddComponent<BackToMenuButton>();

        // --- 重力方向ボタン（下部）---
        // ▲ Up
        var btnUpGo = CreateGravityButton(canvasGo.transform, "BtnUp", "▲", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(110f, 110f), new Vector2(0f, 250f),
            new Color(0.18f, 0.42f, 0.72f, 0.9f));

        // ▼ Down
        var btnDownGo = CreateGravityButton(canvasGo.transform, "BtnDown", "▼", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(110f, 110f), new Vector2(0f, 80f),
            new Color(0.18f, 0.42f, 0.72f, 0.9f));

        // ◀ Left
        var btnLeftGo = CreateGravityButton(canvasGo.transform, "BtnLeft", "◀", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(110f, 110f), new Vector2(-130f, 165f),
            new Color(0.18f, 0.42f, 0.72f, 0.9f));

        // ▶ Right
        var btnRightGo = CreateGravityButton(canvasGo.transform, "BtnRight", "▶", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(110f, 110f), new Vector2(130f, 165f),
            new Color(0.18f, 0.42f, 0.72f, 0.9f));

        // 各重力ボタンに GravityButtonHandler を追加
        AddGravityHandler(btnUpGo,    gravityManager, 0); // Up
        AddGravityHandler(btnDownGo,  gravityManager, 1); // Down
        AddGravityHandler(btnLeftGo,  gravityManager, 2); // Left
        AddGravityHandler(btnRightGo, gravityManager, 3); // Right

        // --- クリアパネル ---
        var clearPanelGo = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanelGo.transform.SetParent(canvasGo.transform, false);
        var cpImg = clearPanelGo.AddComponent<Image>();
        cpImg.color = new Color(0f, 0f, 0f, 0.85f);
        var cpRect = clearPanelGo.GetComponent<RectTransform>();
        cpRect.anchorMin = new Vector2(0.1f, 0.25f);
        cpRect.anchorMax = new Vector2(0.9f, 0.75f);
        cpRect.offsetMin = Vector2.zero;
        cpRect.offsetMax = Vector2.zero;

        var clearTextGo = CreateText(clearPanelGo.transform, "ClearText", "クリア!", 52, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(400f, 130f), Vector2.zero);
        clearTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var restartBtnGo = CreateButton(clearPanelGo.transform, "RestartButton", "もう一度", 30, jpFont,
            new Vector2(0.25f, 0.2f), new Vector2(0.25f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(190f, 65f), Vector2.zero,
            new Color(0.1f, 0.5f, 0.3f));

        var nextBtnGo = CreateButton(clearPanelGo.transform, "NextLevelButton", "次のステージ", 28, jpFont,
            new Vector2(0.75f, 0.2f), new Vector2(0.75f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(210f, 65f), Vector2.zero,
            new Color(0.1f, 0.3f, 0.6f));

        clearPanelGo.SetActive(false);

        // --- GravitySwitchUI ---
        var uiGo = new GameObject("GravitySwitchUI");
        var ui = uiGo.AddComponent<GravitySwitchUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_moveCountText").objectReferenceValue  = moveTextGo.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue     = clearPanelGo;
        uiSO.FindProperty("_clearText").objectReferenceValue      = clearTextGo.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue  = restartBtnGo.GetComponent<Button>();
        uiSO.FindProperty("_nextLevelButton").objectReferenceValue = nextBtnGo.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue    = gameManager;
        uiSO.ApplyModifiedProperties();

        // --- GameManager 参照設定 ---
        var gmSO = new SerializedObject(gameManager);
        gmSO.FindProperty("_gravityManager").objectReferenceValue = gravityManager;
        gmSO.FindProperty("_ui").objectReferenceValue             = ui;
        gmSO.ApplyModifiedProperties();

        // --- EventSystem（新 Input System 対応）---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
        }

        // --- シーン保存 ---
        string scenePath = "Assets/Scenes/003_GravitySwitch.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup003_GravitySwitch] シーンを作成しました: " + scenePath);
    }

    private static void AddGravityHandler(GameObject btnGo, GravityManager gravMgr, int dir)
    {
        var handler = btnGo.AddComponent<GravityButtonHandler>();
        var so = new SerializedObject(handler);
        so.FindProperty("_gravityManager").objectReferenceValue = gravMgr;
        so.FindProperty("_direction").intValue = dir;
        so.ApplyModifiedProperties();
    }

    private static GameObject CreateText(Transform parent, string name, string text, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        if (font != null) tmp.font = font;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPos;

        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = bgColor;
        go.AddComponent<Button>();

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPos;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;

        var tr = textGo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        return go;
    }

    private static GameObject CreateGravityButton(Transform parent, string name, string symbol, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        return CreateButton(parent, name, symbol, fontSize, font, anchorMin, anchorMax, pivot,
            sizeDelta, anchoredPos, bgColor);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return;
        }
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
