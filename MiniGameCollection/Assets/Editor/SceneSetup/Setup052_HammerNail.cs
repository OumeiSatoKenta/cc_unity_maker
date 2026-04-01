using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game052_HammerNail;

public static class Setup052_HammerNail
{
    [MenuItem("Assets/Setup/052 HammerNail")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup052] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game052_HammerNail/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.95f, 0.92f, 0.86f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite nailSprite = LoadSprite(sp + "nail.png");
        Sprite hammerSprite = LoadSprite(sp + "hammer.png");
        Sprite boardSprite = LoadSprite(sp + "board.png");

        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // Board
        var boardObj = new GameObject("Board");
        boardObj.transform.position = new Vector3(0f, -1.5f, 0f);
        var boardSr = boardObj.AddComponent<SpriteRenderer>(); boardSr.sprite = boardSprite; boardSr.sortingOrder = 1;
        boardObj.transform.localScale = new Vector3(1f, 1f, 1f);

        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<HammerNailGameManager>();

        var nmObj = new GameObject("NailManager"); nmObj.transform.SetParent(gmObj.transform);
        var nm = nmObj.AddComponent<NailManager>();
        var nmSO = new SerializedObject(nm);
        nmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        nmSO.FindProperty("_nailSprite").objectReferenceValue = nailSprite;
        nmSO.FindProperty("_hammerSprite").objectReferenceValue = hammerSprite;
        nmSO.FindProperty("_targetDepth").floatValue = 2f;
        nmSO.FindProperty("_sweetSpotDuration").floatValue = 0.3f;
        nmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        var progressText = CT(canvasObj.transform, "ProgressText", "0/5", 36, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,50), new Vector2(20,-20));
        progressText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        progressText.GetComponent<TextMeshProUGUI>().color = Color.black;

        var missText = CT(canvasObj.transform, "MissText", "ミス: 0/3", 36, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(250,50), new Vector2(-20,-20));
        missText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        missText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.2f, 0.1f);

        // Timing gauge slider (center)
        var gaugeSlider = CreateSlider(canvasObj.transform, "GaugeSlider", jpFont, "",
            new Vector2(0.5f,0.15f), new Vector2(0.5f,0.15f), new Vector2(0.5f,0.5f), new Vector2(500,30), Vector2.zero, new Color(0.2f,0.8f,0.3f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.9f,0.95f,0.9f,0.95f));
        CT(clearPanel.transform, "CT", "全釘打ち込み！", 48, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 40, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.3f,0.5f,0.3f));
        clearPanel.SetActive(false);

        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.95f,0.9f,0.9f,0.95f));
        CT(goPanel.transform, "GT", "釘曲がった！", 48, jpFont, new Vector2(0.5f,0.7f), new Vector2(0.5f,0.7f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScoreText = CT(goPanel.transform, "GS", "", 40, jpFont, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont, new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.5f,0.2f,0.1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("HammerNailUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<HammerNailUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_progressText").objectReferenceValue = progressText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_missText").objectReferenceValue = missText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gaugeSlider").objectReferenceValue = gaugeSlider.GetComponent<Slider>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.FindProperty("_nailManager").objectReferenceValue = nm;
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_nailManager").objectReferenceValue = nm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_totalNails").intValue = 5;
        gmSO.FindProperty("_maxMisses").intValue = 3;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/052_HammerNail.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup052] HammerNail シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreateSlider(Transform parent, string name, TMP_FontAsset font, string label, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color fillColor) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); var r = obj.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var slider = obj.AddComponent<Slider>(); slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f; var bg = new GameObject("Background", typeof(RectTransform)); bg.transform.SetParent(obj.transform, false); bg.AddComponent<Image>().color = new Color(0.2f,0.2f,0.2f,0.8f); var bgR = bg.GetComponent<RectTransform>(); bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one; bgR.offsetMin = bgR.offsetMax = Vector2.zero; var fa = new GameObject("Fill Area", typeof(RectTransform)); fa.transform.SetParent(obj.transform, false); var faR = fa.GetComponent<RectTransform>(); faR.anchorMin = Vector2.zero; faR.anchorMax = Vector2.one; faR.offsetMin = faR.offsetMax = Vector2.zero; var fill = new GameObject("Fill", typeof(RectTransform)); fill.transform.SetParent(fa.transform, false); fill.AddComponent<Image>().color = fillColor; var fR = fill.GetComponent<RectTransform>(); fR.anchorMin = Vector2.zero; fR.anchorMax = Vector2.one; fR.offsetMin = fR.offsetMax = Vector2.zero; slider.fillRect = fR; if (!string.IsNullOrEmpty(label)) { var lbl = new GameObject("Label", typeof(RectTransform)); lbl.transform.SetParent(obj.transform, false); var tmp = lbl.AddComponent<TextMeshProUGUI>(); tmp.text = label; tmp.fontSize = 20; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (font != null) tmp.font = font; var lr = lbl.GetComponent<RectTransform>(); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero; } return obj; }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f,0.3f); r.anchorMax = new Vector2(0.9f,0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
