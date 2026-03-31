using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game030_FingerRacer;

public static class Setup030_FingerRacer
{
    [MenuItem("Assets/Setup/030 FingerRacer")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup030] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.49f, 0.78f, 0.31f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        // WhiteSquare
        string whiteTexPath = "Assets/Scripts/Game030_FingerRacer/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        {
            var wTex = new Texture2D(4, 4);
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            wTex.SetPixels(px); wTex.Apply();
            System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG());
            Object.DestroyImmediate(wTex);
            AssetDatabase.ImportAsset(whiteTexPath);
            var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter;
            if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); }
        }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);

        string sp = "Assets/Resources/Sprites/Game030_FingerRacer/";

        // スプライト読み込み
        var carSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "car.png");
        var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "background.png");
        var finishSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "finish.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite ?? whiteSprite;
        bgSr.sortingOrder = -10;
        if (bgSprite == null) bgSr.color = new Color(0.49f, 0.78f, 0.31f);
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.04f, 0.04f, 1f) : new Vector3(10f, 10f, 1f);

        // Car
        var carObj = new GameObject("Car");
        var carSr = carObj.AddComponent<SpriteRenderer>();
        carSr.sprite = carSprite ?? whiteSprite;
        carSr.sortingOrder = 10;
        carObj.transform.position = new Vector3(0f, 0f, 0f);
        if (carSprite != null) carObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        else { carObj.transform.localScale = new Vector3(0.5f, 0.8f, 1f); carSr.color = new Color(0.9f, 0.2f, 0.2f); }

        // FinishLine
        var finishObj = new GameObject("FinishLine");
        var finishSr = finishObj.AddComponent<SpriteRenderer>();
        finishSr.sprite = finishSprite ?? whiteSprite;
        finishSr.sortingOrder = 8;
        if (finishSprite != null) finishObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        else { finishObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f); finishSr.color = new Color(1f, 0.9f, 0f); }
        finishObj.SetActive(false);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<FingerRacerGameManager>();

        // RaceManager (child of GameManager)
        var rmObj = new GameObject("RaceManager");
        rmObj.transform.SetParent(gmObj.transform);
        var rm = rmObj.AddComponent<RaceManager>();
        var rmSO = new SerializedObject(rm);
        rmSO.FindProperty("_carTransform").objectReferenceValue = carObj.transform;
        rmSO.FindProperty("_finishLineTransform").objectReferenceValue = finishObj.transform;
        rmSO.FindProperty("_baseSpeed").floatValue = 3f;
        rmSO.FindProperty("_minPathLength").floatValue = 5f;
        rmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Phase Text
        var phaseText = CT(canvasObj.transform, "PhaseText", "コースを描いてください", 36, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(600, 60), new Vector2(0, -20));
        phaseText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        phaseText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.1f, 0.1f);

        // Time Text
        var timeTextObj = CT(canvasObj.transform, "TimeText", "30.0s", 40, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(200, 60), new Vector2(0, 20));
        timeTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        timeTextObj.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.1f, 0.1f);
        timeTextObj.SetActive(false);

        // Menu Button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(240, 50), new Vector2(-20, -20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Clear Panel
        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0f, 0.3f, 0f, 0.9f);
        var cr = clearPanel.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.15f, 0.25f); cr.anchorMax = new Vector2(0.85f, 0.75f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearText = CT(clearPanel.transform, "ClearText", "ゴール！", 52, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(500, 160), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 28, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.1f, 0.5f, 0.1f, 1f));
        clearPanel.SetActive(false);

        // GameOver Panel
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        goPanel.AddComponent<Image>().color = new Color(0.2f, 0.05f, 0f, 0.9f);
        var gr = goPanel.GetComponent<RectTransform>();
        gr.anchorMin = new Vector2(0.15f, 0.25f); gr.anchorMax = new Vector2(0.85f, 0.75f);
        gr.offsetMin = gr.offsetMax = Vector2.zero;
        var goText = CT(goPanel.transform, "GameOverText", "タイムオーバー！", 48, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(500, 150), Vector2.zero);
        goText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 28, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.5f, 0.2f, 0.1f, 1f));
        goPanel.SetActive(false);

        // FingerRacerUI (child of GameManager)
        var uiObj = new GameObject("FingerRacerUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<FingerRacerUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_phaseText").objectReferenceValue = phaseText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timeText").objectReferenceValue = timeTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = goText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_raceManager").objectReferenceValue = rm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_timeLimit").floatValue = 30f;
        gmSO.ApplyModifiedProperties();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        // Save
        string scenePath = "Assets/Scenes/030_FingerRacer.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup030] FingerRacer シーンを作成しました: " + scenePath);
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath)
    { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
