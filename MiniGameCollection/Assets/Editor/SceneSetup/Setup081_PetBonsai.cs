using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game081_PetBonsai;

public static class Setup081_PetBonsai
{
    [MenuItem("Assets/Setup/081 PetBonsai")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup081] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game081_PetBonsai/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.9f, 0.84f, 0.72f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite bonsaiSprite = LoadSprite(sp + "bonsai.png");
        Sprite potSprite = LoadSprite(sp + "pot.png");

        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<PetBonsaiGameManager>();

        var bmObj = new GameObject("BonsaiManager"); bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<BonsaiManager>();
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("_bonsaiSprite").objectReferenceValue = bonsaiSprite;
        bmSO.FindProperty("_potSprite").objectReferenceValue = potSprite;
        bmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        var beautyText = CT(canvasObj.transform, "BeautyText", "美: 0/100", 32, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(220,40), new Vector2(20,-15));
        beautyText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left; beautyText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,0.3f,0.1f);

        var growthText = CT(canvasObj.transform, "GrowthText", "Lv1", 28, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(100,35), new Vector2(-20,-15));
        growthText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right; growthText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f,0.5f,0.2f);

        var waterSlider = CreateSlider(canvasObj.transform, "WaterSlider", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,20), new Vector2(0,-55), new Color(0.3f,0.6f,1f));
        waterSlider.GetComponent<Slider>().value = 0.5f;

        var waterBtn = CB(canvasObj.transform, "WaterButton", "水やり", 26, jpFont, new Vector2(0.3f,0), new Vector2(0.3f,0), new Vector2(0.5f,0), new Vector2(180,65), new Vector2(0,90), new Color(0.3f,0.5f,0.7f,0.9f));
        var pruneBtn = CB(canvasObj.transform, "PruneButton", "剪定", 26, jpFont, new Vector2(0.7f,0), new Vector2(0.7f,0), new Vector2(0.5f,0), new Vector2(180,65), new Vector2(0,90), new Color(0.5f,0.4f,0.2f,0.9f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.9f,0.85f,0.7f,0.95f));
        CT(clearPanel.transform, "CT", "品評会優勝！", 48, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 36, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.4f,0.3f,0.2f));
        clearPanel.SetActive(false);

        var uiObj = new GameObject("PetBonsaiUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<PetBonsaiUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_beautyText").objectReferenceValue = beautyText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_growthText").objectReferenceValue = growthText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_waterSlider").objectReferenceValue = waterSlider.GetComponent<Slider>();
        uiSO.FindProperty("_waterButton").objectReferenceValue = waterBtn.GetComponent<Button>();
        uiSO.FindProperty("_pruneButton").objectReferenceValue = pruneBtn.GetComponent<Button>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_bonsaiManager").objectReferenceValue = bm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_targetBeauty").intValue = 100;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(waterBtn.GetComponent<Button>().onClick, bm.Water);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(pruneBtn.GetComponent<Button>().onClick, bm.Prune);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/081_PetBonsai.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup081] PetBonsai シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreateSlider(Transform parent, string name, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color fillColor) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); var r = obj.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var slider = obj.AddComponent<Slider>(); slider.minValue = 0f; slider.maxValue = 1f; var bg = new GameObject("Background", typeof(RectTransform)); bg.transform.SetParent(obj.transform, false); bg.AddComponent<Image>().color = new Color(0.2f,0.2f,0.2f,0.6f); var bgR = bg.GetComponent<RectTransform>(); bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one; bgR.offsetMin = bgR.offsetMax = Vector2.zero; var fa = new GameObject("Fill Area", typeof(RectTransform)); fa.transform.SetParent(obj.transform, false); var faR = fa.GetComponent<RectTransform>(); faR.anchorMin = Vector2.zero; faR.anchorMax = Vector2.one; faR.offsetMin = faR.offsetMax = Vector2.zero; var fill = new GameObject("Fill", typeof(RectTransform)); fill.transform.SetParent(fa.transform, false); fill.AddComponent<Image>().color = fillColor; var fR = fill.GetComponent<RectTransform>(); fR.anchorMin = Vector2.zero; fR.anchorMax = Vector2.one; fR.offsetMin = fR.offsetMax = Vector2.zero; slider.fillRect = fR; return obj; }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
