using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game061_CookieFactory;

public static class Setup061_CookieFactory
{
    [MenuItem("Assets/Setup/061 CookieFactory")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup061] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game061_CookieFactory/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.96f, 0.92f, 0.84f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite cookieSprite = LoadSprite(sp + "cookie.png");
        Sprite ovenSprite = LoadSprite(sp + "oven.png");

        // Background
        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // Big cookie display in center
        var bigCookie = new GameObject("BigCookie");
        bigCookie.transform.position = new Vector3(0f, -1f, 0f);
        var bcSr = bigCookie.AddComponent<SpriteRenderer>();
        bcSr.sprite = cookieSprite; bcSr.sortingOrder = 2;
        bigCookie.transform.localScale = new Vector3(2f, 2f, 1f);

        // Oven decoration
        var ovenObj = new GameObject("Oven");
        ovenObj.transform.position = new Vector3(-2.5f, -3f, 0f);
        var ovSr = ovenObj.AddComponent<SpriteRenderer>();
        ovSr.sprite = ovenSprite; ovSr.sortingOrder = 1;

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<CookieFactoryGameManager>();

        var fmObj = new GameObject("FactoryManager"); fmObj.transform.SetParent(gmObj.transform);
        var fm = fmObj.AddComponent<FactoryManager>();
        var fmSO = new SerializedObject(fm);
        fmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        fmSO.FindProperty("_cookieSprite").objectReferenceValue = cookieSprite;
        fmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Cookie count (top left)
        var cookieText = CT(canvasObj.transform, "CookieText", "クッキー: 0", 36, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, 50), new Vector2(20, -20));
        cookieText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        cookieText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.3f, 0.1f);

        // Sales (top right)
        var salesText = CT(canvasObj.transform, "SalesText", "売上: 0/500", 36, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(300, 50), new Vector2(-20, -20));
        salesText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        salesText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.3f, 0.1f);

        // Oven upgrade button (bottom area)
        var ovenBtn = CB(canvasObj.transform, "OvenButton", "オーブンLv0\n10枚", 24, jpFont,
            new Vector2(0.25f, 0), new Vector2(0.25f, 0), new Vector2(0.5f, 0), new Vector2(220, 80), new Vector2(0, 90),
            new Color(0.7f, 0.4f, 0.2f, 0.9f));

        // Conveyor upgrade button
        var conveyorBtn = CB(canvasObj.transform, "ConveyorButton", "コンベアLv0\n25枚", 24, jpFont,
            new Vector2(0.75f, 0), new Vector2(0.75f, 0), new Vector2(0.5f, 0), new Vector2(220, 80), new Vector2(0, 90),
            new Color(0.4f, 0.5f, 0.7f, 0.9f));

        // Menu button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(260, 55), new Vector2(20, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Clear panel
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.95f, 0.9f, 0.8f, 0.95f));
        CT(clearPanel.transform, "CT", "目標達成！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.6f, 0.4f, 0.2f));
        clearPanel.SetActive(false);

        // UI component
        var uiObj = new GameObject("CookieFactoryUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<CookieFactoryUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_cookieText").objectReferenceValue = cookieText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_salesText").objectReferenceValue = salesText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_ovenButton").objectReferenceValue = ovenBtn.GetComponent<Button>();
        uiSO.FindProperty("_ovenButtonText").objectReferenceValue = ovenBtn.GetComponentInChildren<TextMeshProUGUI>();
        uiSO.FindProperty("_conveyorButton").objectReferenceValue = conveyorBtn.GetComponent<Button>();
        uiSO.FindProperty("_conveyorButtonText").objectReferenceValue = conveyorBtn.GetComponentInChildren<TextMeshProUGUI>();
        uiSO.FindProperty("_factoryManager").objectReferenceValue = fm;
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_factoryManager").objectReferenceValue = fm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_targetSales").intValue = 500;
        gmSO.ApplyModifiedProperties();

        // Button events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(ovenBtn.GetComponent<Button>().onClick, fm.UpgradeOven);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(conveyorBtn.GetComponent<Button>().onClick, fm.UpgradeConveyor);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/061_CookieFactory.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup061] CookieFactory シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = color;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        return obj;
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false);
        var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white;
        if (f != null) tmp.font = f;
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        return o;
    }

    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    {
        var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false);
        o.AddComponent<Image>().color = bg; o.AddComponent<Button>();
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (f != null) tmp.font = f;
        var tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        return o;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
