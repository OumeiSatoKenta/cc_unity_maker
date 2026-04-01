using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game066_RoboFactory;

public static class Setup066_RoboFactory
{
    [MenuItem("Assets/Setup/066 RoboFactory")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup066] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game066_RoboFactory/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.16f, 0.18f, 0.22f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite robotSprite = LoadSprite(sp + "robot.png");
        Sprite buildingSprite = LoadSprite(sp + "building.png");
        Sprite resourceSprite = LoadSprite(sp + "resource.png");

        // Background
        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // Building visual (decorative)
        var bldgObj = new GameObject("BuildingDecor");
        bldgObj.transform.position = new Vector3(2f, 1f, 0f);
        var bSr = bldgObj.AddComponent<SpriteRenderer>(); bSr.sprite = buildingSprite; bSr.sortingOrder = 1;

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<RoboFactoryGameManager>();

        var rmObj = new GameObject("RoboManager"); rmObj.transform.SetParent(gmObj.transform);
        var rm = rmObj.AddComponent<RoboManager>();
        var rmSO = new SerializedObject(rm);
        rmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        rmSO.FindProperty("_robotSprite").objectReferenceValue = robotSprite;
        rmSO.FindProperty("_resourceSprite").objectReferenceValue = resourceSprite;
        rmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        var resourceText = CT(canvasObj.transform, "ResourceText", "資源: 10", 32, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(200, 40), new Vector2(20, -15));
        resourceText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        resourceText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.3f);

        var robotText = CT(canvasObj.transform, "RobotText", "ロボ: 1体", 32, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(200, 40), new Vector2(0, -15));
        robotText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        robotText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.8f, 1f);

        var cityLevelText = CT(canvasObj.transform, "CityLevelText", "都市Lv1/5", 32, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(220, 40), new Vector2(-20, -15));
        cityLevelText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        cityLevelText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.6f);

        var robotBtn = CB(canvasObj.transform, "RobotButton", "ロボ追加\n13", 22, jpFont,
            new Vector2(0.3f, 0), new Vector2(0.3f, 0), new Vector2(0.5f, 0), new Vector2(180, 75), new Vector2(0, 90),
            new Color(0.3f, 0.5f, 0.7f, 0.9f));

        var buildBtn = CB(canvasObj.transform, "BuildButton", "建設\n20", 22, jpFont,
            new Vector2(0.7f, 0), new Vector2(0.7f, 0), new Vector2(0.5f, 0), new Vector2(180, 75), new Vector2(0, 90),
            new Color(0.5f, 0.6f, 0.3f, 0.9f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(260, 55), new Vector2(20, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.15f, 0.18f, 0.25f, 0.95f));
        CT(clearPanel.transform, "CT", "都市完成！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 36, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.3f, 0.4f, 0.6f));
        clearPanel.SetActive(false);

        // UI component
        var uiObj = new GameObject("RoboFactoryUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<RoboFactoryUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_resourceText").objectReferenceValue = resourceText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_robotText").objectReferenceValue = robotText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_cityLevelText").objectReferenceValue = cityLevelText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_robotButton").objectReferenceValue = robotBtn.GetComponent<Button>();
        uiSO.FindProperty("_robotButtonText").objectReferenceValue = robotBtn.GetComponentInChildren<TextMeshProUGUI>();
        uiSO.FindProperty("_buildButton").objectReferenceValue = buildBtn.GetComponent<Button>();
        uiSO.FindProperty("_buildButtonText").objectReferenceValue = buildBtn.GetComponentInChildren<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_roboManager").objectReferenceValue = rm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_targetCityLevel").intValue = 5;
        gmSO.ApplyModifiedProperties();

        // Button events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(robotBtn.GetComponent<Button>().onClick, rm.AddRobot);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(buildBtn.GetComponent<Button>().onClick, gm.BuildBuilding);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/066_RoboFactory.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup066] RoboFactory シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
