using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game086_CityBonsai;

public static class Setup086_CityBonsai
{
    [MenuItem("Assets/Setup/086 CityBonsai")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup086] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game086_CityBonsai/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.78f, 0.74f, 0.63f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite buildingSprite = LoadSprite(sp + "building.png");
        Sprite flowerSprite = LoadSprite(sp + "flower.png");
        Sprite bonsaiBaseSprite = LoadSprite(sp + "bonsai_base.png");

        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<CityBonsaiGameManager>();

        var cmObj = new GameObject("CityBonsaiManager"); cmObj.transform.SetParent(gmObj.transform);
        var cm = cmObj.AddComponent<CityBonsaiManager>();
        var cmSO = new SerializedObject(cm);
        cmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        cmSO.FindProperty("_buildingSprite").objectReferenceValue = buildingSprite;
        cmSO.FindProperty("_flowerSprite").objectReferenceValue = flowerSprite;
        cmSO.FindProperty("_bonsaiBaseSprite").objectReferenceValue = bonsaiBaseSprite;
        cmSO.FindProperty("_buildCost").intValue = 8;
        cmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        var coinText = CT(canvasObj.transform, "CoinText", "コイン: 15", 28, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,35), new Vector2(20,-15));
        coinText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left; coinText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f,0.5f,0.2f);

        var populationText = CT(canvasObj.transform, "PopulationText", "人口: 0/30", 28, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(200,35), new Vector2(0,-15));
        populationText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; populationText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f,0.5f,0.3f);

        var buildingText = CT(canvasObj.transform, "BuildingText", "建物: 0", 28, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(150,35), new Vector2(-20,-15));
        buildingText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right; buildingText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f,0.4f,0.3f);

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.75f,0.72f,0.6f,0.95f));
        CT(clearPanel.transform, "CT", "盆栽都市完成！", 48, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 36, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.4f,0.35f,0.25f));
        clearPanel.SetActive(false);

        var uiObj = new GameObject("CityBonsaiUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<CityBonsaiUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_coinText").objectReferenceValue = coinText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_populationText").objectReferenceValue = populationText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_buildingText").objectReferenceValue = buildingText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_cityManager").objectReferenceValue = cm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_targetPopulation").intValue = 30;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/086_CityBonsai.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup086] CityBonsai シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
