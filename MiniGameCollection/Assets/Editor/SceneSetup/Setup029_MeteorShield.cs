using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game029_MeteorShield;

public static class Setup029_MeteorShield
{
    [MenuItem("Assets/Setup/029 MeteorShield")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup029] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.04f, 0.05f, 0.12f, 1f); camera.orthographic = true; camera.orthographicSize = 5f; }

        string whiteTexPath = "Assets/Scripts/Game029_MeteorShield/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        { var wTex = new Texture2D(4, 4); var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white; wTex.SetPixels(px); wTex.Apply(); System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG()); Object.DestroyImmediate(wTex); AssetDatabase.ImportAsset(whiteTexPath); var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter; if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); } }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);
        string sp = "Assets/Resources/Sprites/Game029_MeteorShield/"; string pd = "Assets/Scripts/Game029_MeteorShield/";

        // スプライト読み込み
        var meteorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "meteor.png");
        var shieldSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "shield.png");
        var starSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "star.png");
        var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "background.png");

        // Prefab作成
        var meteorPrefab = SP(pd + "MeteorPrefab.prefab", meteorSprite, whiteSprite, 5);

        // 背景
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite ?? whiteSprite;
        bgSr.sortingOrder = -10;
        bgSr.color = bgSprite != null ? Color.white : new Color(0.04f, 0.05f, 0.12f);
        bgObj.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        // 星
        var starObj = new GameObject("Star");
        var starSr = starObj.AddComponent<SpriteRenderer>();
        starSr.sprite = starSprite ?? whiteSprite;
        starSr.sortingOrder = 2;
        starObj.transform.position = new Vector3(0f, -4.2f, 0f);
        if (starSprite != null) starObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        // シールド
        var shieldObj = new GameObject("Shield");
        var shieldSr = shieldObj.AddComponent<SpriteRenderer>();
        shieldSr.sprite = shieldSprite ?? whiteSprite;
        shieldSr.sortingOrder = 8;
        shieldObj.transform.position = new Vector3(0f, -3.5f, 0f);
        if (shieldSprite != null) shieldObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        else { shieldObj.transform.localScale = new Vector3(128f, 16f, 1f); shieldSr.color = new Color(0.4f, 0.7f, 1f); }

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<MeteorShieldGameManager>();

        // ShieldManager (child of GameManager)
        var smObj = new GameObject("ShieldManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<ShieldManager>();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_shield").objectReferenceValue = shieldObj.transform;
        smSO.FindProperty("_star").objectReferenceValue = starObj.transform;
        smSO.FindProperty("_meteorPrefab").objectReferenceValue = meteorPrefab;
        smSO.FindProperty("_initialSpawnInterval").floatValue = 1.2f;
        smSO.FindProperty("_minSpawnInterval").floatValue = 0.4f;
        smSO.FindProperty("_initialFallSpeed").floatValue = 3f;
        smSO.FindProperty("_maxFallSpeed").floatValue = 6f;
        smSO.FindProperty("_shieldHalfWidth").floatValue = 1.0f;
        smSO.FindProperty("_shieldY").floatValue = -3.5f;
        smSO.FindProperty("_starRadius").floatValue = 0.5f;
        smSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HP Text
        var hpText = CT(canvasObj.transform, "HpText", "\u2665\u2665\u2665", 48, jpFont, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, 60), new Vector2(20, -20));
        hpText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        // Time Text
        var timeText = CT(canvasObj.transform, "TimeText", "60.0s", 36, jpFont, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(200, 60), new Vector2(0, -20));
        timeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Menu Button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(240, 50), new Vector2(-20, -20), new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Clear Panel
        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0, 0.1f, 0.3f, 0.9f);
        var cr = clearPanel.GetComponent<RectTransform>(); cr.anchorMin = new Vector2(0.15f, 0.2f); cr.anchorMax = new Vector2(0.85f, 0.8f); cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearText = CT(clearPanel.transform, "ClearText", "クリア!", 48, jpFont, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(400, 150), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 28, jpFont, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero, new Color(0.2f, 0.4f, 0.6f, 1f));
        clearPanel.SetActive(false);

        // GameOver Panel
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        goPanel.AddComponent<Image>().color = new Color(0.15f, 0, 0, 0.9f);
        var gr = goPanel.GetComponent<RectTransform>(); gr.anchorMin = new Vector2(0.15f, 0.2f); gr.anchorMax = new Vector2(0.85f, 0.8f); gr.offsetMin = gr.offsetMax = Vector2.zero;
        var goText = CT(goPanel.transform, "GameOverText", "ゲームオーバー", 48, jpFont, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(400, 150), Vector2.zero);
        goText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 28, jpFont, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero, new Color(0.5f, 0.2f, 0.2f, 1f));
        goPanel.SetActive(false);

        // UI Component
        var uiObj = new GameObject("MeteorShieldUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<MeteorShieldUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_hpText").objectReferenceValue = hpText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timeText").objectReferenceValue = timeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = goText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_shieldManager").objectReferenceValue = sm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_maxHp").intValue = 3;
        gmSO.FindProperty("_clearTime").floatValue = 60f;
        gmSO.ApplyModifiedProperties();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        // Save
        string scenePath = "Assets/Scenes/029_MeteorShield.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup029] MeteorShield シーンを作成しました: " + scenePath);
    }

    private static GameObject SP(string path, Sprite sprite, Sprite fallback, int order)
    { var o = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path)); var sr = o.AddComponent<SpriteRenderer>(); sr.sprite = sprite ?? fallback; sr.sortingOrder = order; var p = PrefabUtility.SaveAsPrefabAsset(o, path); Object.DestroyImmediate(o); return p; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath)
    { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
