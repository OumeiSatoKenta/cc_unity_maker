using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game051_DrawBridge;

public static class Setup051_DrawBridge
{
    [MenuItem("Assets/Setup/051 DrawBridge")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup051] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game051_DrawBridge/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.7f, 0.82f, 0.94f); camera.orthographic = true; camera.orthographicSize = 6f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite charSprite = LoadSprite(sp + "character.png");
        Sprite cliffSprite = LoadSprite(sp + "cliff.png");
        Sprite flagSprite = LoadSprite(sp + "flag.png");

        // Background
        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // Left cliff
        var leftCliff = new GameObject("LeftCliff");
        leftCliff.transform.position = new Vector3(-4f, -2f, 0f);
        var lcSr = leftCliff.AddComponent<SpriteRenderer>(); lcSr.sprite = cliffSprite; lcSr.sortingOrder = 1;
        leftCliff.transform.localScale = new Vector3(1f, 0.5f, 1f);
        var lcCol = leftCliff.AddComponent<BoxCollider2D>(); lcCol.size = new Vector2(1.2f, 2.5f);
        var lcRb = leftCliff.AddComponent<Rigidbody2D>(); lcRb.bodyType = RigidbodyType2D.Static;

        // Right cliff
        var rightCliff = new GameObject("RightCliff");
        rightCliff.transform.position = new Vector3(4f, -2.5f, 0f);
        var rcSr = rightCliff.AddComponent<SpriteRenderer>(); rcSr.sprite = cliffSprite; rcSr.sortingOrder = 1;
        rightCliff.transform.localScale = new Vector3(1f, 0.5f, 1f);
        var rcCol = rightCliff.AddComponent<BoxCollider2D>(); rcCol.size = new Vector2(1.2f, 2.5f);
        var rcRb = rightCliff.AddComponent<Rigidbody2D>(); rcRb.bodyType = RigidbodyType2D.Static;

        // Character (starts on left cliff, kinematic until Go)
        var charObj = new GameObject("Character");
        charObj.transform.position = new Vector3(-4f, -0.3f, 0f);
        var charSr = charObj.AddComponent<SpriteRenderer>(); charSr.sprite = charSprite; charSr.sortingOrder = 5;
        charObj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        var charRb = charObj.AddComponent<Rigidbody2D>();
        charRb.bodyType = RigidbodyType2D.Kinematic;
        charRb.gravityScale = 1.5f;
        charRb.freezeRotation = true;
        var charCol = charObj.AddComponent<CircleCollider2D>(); charCol.radius = 0.25f;
        var bridgeChar = charObj.AddComponent<BridgeCharacter>();

        // Goal (on right cliff)
        var goalObj = new GameObject("Goal");
        goalObj.transform.position = new Vector3(4.5f, -0.8f, 0f);
        var goalSr = goalObj.AddComponent<SpriteRenderer>(); goalSr.sprite = flagSprite; goalSr.sortingOrder = 4;
        var goalCol = goalObj.AddComponent<BoxCollider2D>(); goalCol.size = new Vector2(0.5f, 1f); goalCol.isTrigger = true;

        // FallZone
        var fzObj = new GameObject("FallZone");
        fzObj.transform.position = new Vector3(0f, -7f, 0f);
        var fzCol = fzObj.AddComponent<BoxCollider2D>(); fzCol.size = new Vector2(20f, 2f); fzCol.isTrigger = true;

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<DrawBridgeGameManager>();

        var bmObj = new GameObject("BridgeManager"); bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<BridgeManager>();
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        bmSO.FindProperty("_inkMax").floatValue = 80f;
        bmSO.FindProperty("_lineWidth").floatValue = 0.12f;
        bmSO.ApplyModifiedProperties();

        // Wire BridgeCharacter
        var bcSO = new SerializedObject(bridgeChar);
        bcSO.FindProperty("_gameManager").objectReferenceValue = gm;
        bcSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Ink slider
        var inkSlider = CreateSlider(canvasObj.transform, "InkSlider", jpFont, "インク",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 35), new Vector2(0, -20), new Color(0.4f, 0.3f, 0.2f));
        inkSlider.GetComponent<Slider>().value = 1f;

        // Go button
        var goBtn = CB(canvasObj.transform, "GoButton", "GO!", 32, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(180, 60), new Vector2(0, 80), new Color(0.2f, 0.6f, 0.3f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(260, 55), new Vector2(20, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.85f, 0.95f, 0.85f, 0.95f));
        CT(clearPanel.transform, "CT", "クリア！", 52, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearTimeText = CT(clearPanel.transform, "CS", "", 40, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        clearTimeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.3f,0.5f,0.3f));
        clearPanel.SetActive(false);

        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.95f,0.88f,0.88f,0.95f));
        CT(goPanel.transform, "GT", "落下！", 52, jpFont, new Vector2(0.5f,0.7f), new Vector2(0.5f,0.7f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont, new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.5f,0.2f,0.1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("DrawBridgeUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<DrawBridgeUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_inkSlider").objectReferenceValue = inkSlider.GetComponent<Slider>();
        uiSO.FindProperty("_goButton").objectReferenceValue = goBtn.GetComponent<Button>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearTimeText").objectReferenceValue = clearTimeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_bridgeManager").objectReferenceValue = bm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_character").objectReferenceValue = charRb;
        gmSO.FindProperty("_walkSpeed").floatValue = 2f;
        gmSO.ApplyModifiedProperties();

        // Button events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goBtn.GetComponent<Button>().onClick, bm.FinishDrawingAndGo);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/051_DrawBridge.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup051] DrawBridge シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreateSlider(Transform parent, string name, TMP_FontAsset font, string label, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color fillColor) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); var r = obj.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var slider = obj.AddComponent<Slider>(); slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f; var bg = new GameObject("Background", typeof(RectTransform)); bg.transform.SetParent(obj.transform, false); bg.AddComponent<Image>().color = new Color(0.2f,0.2f,0.2f,0.8f); var bgR = bg.GetComponent<RectTransform>(); bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one; bgR.offsetMin = bgR.offsetMax = Vector2.zero; var fa = new GameObject("Fill Area", typeof(RectTransform)); fa.transform.SetParent(obj.transform, false); var faR = fa.GetComponent<RectTransform>(); faR.anchorMin = Vector2.zero; faR.anchorMax = Vector2.one; faR.offsetMin = faR.offsetMax = Vector2.zero; var fill = new GameObject("Fill", typeof(RectTransform)); fill.transform.SetParent(fa.transform, false); fill.AddComponent<Image>().color = fillColor; var fR = fill.GetComponent<RectTransform>(); fR.anchorMin = Vector2.zero; fR.anchorMax = Vector2.one; fR.offsetMin = fR.offsetMax = Vector2.zero; slider.fillRect = fR; var lbl = new GameObject("Label", typeof(RectTransform)); lbl.transform.SetParent(obj.transform, false); var tmp = lbl.AddComponent<TextMeshProUGUI>(); tmp.text = label; tmp.fontSize = 20; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (font != null) tmp.font = font; var lr = lbl.GetComponent<RectTransform>(); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero; return obj; }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f,0.3f); r.anchorMax = new Vector2(0.9f,0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
