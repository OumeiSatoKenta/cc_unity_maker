using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game048_GlassBall;

public static class Setup048_GlassBall
{
    [MenuItem("Assets/Setup/048 GlassBall")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup048] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game048_GlassBall/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.12f, 0.15f, 0.3f); camera.orthographic = true; camera.orthographicSize = 6f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite ballSprite = LoadSprite(sp + "ball.png");
        Sprite goalSprite = LoadSprite(sp + "goal.png");
        Sprite platSprite = LoadSprite(sp + "platform.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>(); bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // Start platform
        var platObj = new GameObject("Platform");
        platObj.transform.position = new Vector3(-3f, 3f, 0f);
        var platSr = platObj.AddComponent<SpriteRenderer>(); platSr.sprite = platSprite; platSr.sortingOrder = 1;
        var platCol = platObj.AddComponent<BoxCollider2D>(); platCol.size = new Vector2(1.2f, 0.3f);
        var platRb = platObj.AddComponent<Rigidbody2D>(); platRb.bodyType = RigidbodyType2D.Static;

        // Ball
        var ballObj = new GameObject("Ball");
        ballObj.transform.position = new Vector3(-3f, 3.6f, 0f);
        var ballSr = ballObj.AddComponent<SpriteRenderer>(); ballSr.sprite = ballSprite; ballSr.sortingOrder = 3;
        ballObj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        var ballRb = ballObj.AddComponent<Rigidbody2D>();
        ballRb.mass = 0.3f; ballRb.gravityScale = 1f;
        ballRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var ballCol = ballObj.AddComponent<CircleCollider2D>(); ballCol.radius = 0.45f;
        var pmat = new PhysicsMaterial2D("GlassMat") { friction = 0.3f, bounciness = 0.4f };
        ballCol.sharedMaterial = pmat;
        var glassBall = ballObj.AddComponent<GlassBall>();

        // (GlassBall._gameManager is wired after GameManager creation below)

        // Goal
        var goalObj = new GameObject("Goal");
        goalObj.transform.position = new Vector3(3f, -4f, 0f);
        var goalSr = goalObj.AddComponent<SpriteRenderer>(); goalSr.sprite = goalSprite; goalSr.sortingOrder = 2;
        goalObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        var goalCol = goalObj.AddComponent<CircleCollider2D>(); goalCol.radius = 0.45f; goalCol.isTrigger = true;

        // FallZone
        var fzObj = new GameObject("FallZone");
        fzObj.transform.position = new Vector3(0f, -7f, 0f);
        var fzCol = fzObj.AddComponent<BoxCollider2D>(); fzCol.size = new Vector2(20f, 2f); fzCol.isTrigger = true;

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GlassBallGameManager>();

        var rmObj = new GameObject("RailManager"); rmObj.transform.SetParent(gmObj.transform);
        var rm = rmObj.AddComponent<RailManager>();
        var rmSO = new SerializedObject(rm);
        rmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        rmSO.FindProperty("_inkMax").floatValue = 100f;
        rmSO.FindProperty("_railWidth").floatValue = 0.15f;
        rmSO.ApplyModifiedProperties();

        // Wire GlassBall -> GameManager
        var gbSO = new SerializedObject(glassBall);
        gbSO.FindProperty("_gameManager").objectReferenceValue = gm;
        gbSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Impact slider (top left)
        var impactSlider = CreateSlider(canvasObj.transform, "ImpactSlider", jpFont, "衝撃",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, 40), new Vector2(20, -20),
            new Color(1f, 0.3f, 0.2f));

        // Ink slider (top right)
        var inkSlider = CreateSlider(canvasObj.transform, "InkSlider", jpFont, "インク",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(300, 40), new Vector2(-20, -20),
            new Color(0.3f, 0.6f, 1f));
        inkSlider.GetComponent<Slider>().value = 1f;

        // Menu button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(260, 55), new Vector2(20, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Clear panel
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.85f, 0.92f, 1f, 0.95f));
        CT(clearPanel.transform, "ClearTitle", "クリア！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearTimeText = CT(clearPanel.transform, "ClearTime", "", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        clearTimeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero, new Color(0.3f, 0.5f, 0.7f));
        clearPanel.SetActive(false);

        // GameOver panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.95f, 0.88f, 0.88f, 0.95f));
        CT(goPanel.transform, "GOTitle", "ゲームオーバー", 52, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero, new Color(0.5f, 0.2f, 0.1f));
        goPanel.SetActive(false);

        // UI component
        var uiObj = new GameObject("GlassBallUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<GlassBallUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_impactSlider").objectReferenceValue = impactSlider.GetComponent<Slider>();
        uiSO.FindProperty("_inkSlider").objectReferenceValue = inkSlider.GetComponent<Slider>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearTimeText").objectReferenceValue = clearTimeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_railManager").objectReferenceValue = rm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_ball").objectReferenceValue = ballObj.transform;
        gmSO.FindProperty("_impactMax").floatValue = 100f;
        gmSO.ApplyModifiedProperties();

        // Button events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/048_GlassBall.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup048] GlassBall シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreateSlider(Transform parent, string name, TMP_FontAsset font, string label, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color fillColor)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var r = obj.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var slider = obj.AddComponent<Slider>(); slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;

        // Background
        var bg = new GameObject("Background", typeof(RectTransform)); bg.transform.SetParent(obj.transform, false);
        bg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var bgR = bg.GetComponent<RectTransform>(); bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one; bgR.offsetMin = bgR.offsetMax = Vector2.zero;

        // Fill area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(obj.transform, false);
        var faR = fillArea.GetComponent<RectTransform>(); faR.anchorMin = Vector2.zero; faR.anchorMax = Vector2.one; faR.offsetMin = faR.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill", typeof(RectTransform)); fill.transform.SetParent(fillArea.transform, false);
        fill.AddComponent<Image>().color = fillColor;
        var fR = fill.GetComponent<RectTransform>(); fR.anchorMin = Vector2.zero; fR.anchorMax = Vector2.one; fR.offsetMin = fR.offsetMax = Vector2.zero;
        slider.fillRect = fR;

        // Label
        var lbl = new GameObject("Label", typeof(RectTransform)); lbl.transform.SetParent(obj.transform, false);
        var tmp = lbl.AddComponent<TextMeshProUGUI>(); tmp.text = label; tmp.fontSize = 22; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        var lr = lbl.GetComponent<RectTransform>(); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;

        return obj;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = color;
        var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero;
        return obj;
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false);
        var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white;
        if (f != null) tmp.font = f;
        var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        return o;
    }

    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    {
        var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false);
        o.AddComponent<Image>().color = bg; o.AddComponent<Button>();
        var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (f != null) tmp.font = f;
        var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
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
