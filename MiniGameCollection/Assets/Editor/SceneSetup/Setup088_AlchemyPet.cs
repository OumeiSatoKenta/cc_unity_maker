using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game088_AlchemyPet;

public static class Setup088_AlchemyPet
{
    [MenuItem("Assets/Setup/088 AlchemyPet")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup088] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game088_AlchemyPet/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.12f, 0.08f, 0.16f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite flaskSprite = LoadSprite(sp + "flask.png");
        Sprite petSprite = LoadSprite(sp + "pet.png");

        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        // Flask visual
        var flaskObj = new GameObject("Flask"); flaskObj.transform.position = new Vector3(0f, -1f, 0f);
        var fsr = flaskObj.AddComponent<SpriteRenderer>(); fsr.sprite = flaskSprite; fsr.sortingOrder = 2;
        flaskObj.transform.localScale = Vector3.one * 1.5f;

        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<AlchemyPetGameManager>();

        var amObj = new GameObject("AlchemyManager"); amObj.transform.SetParent(gmObj.transform);
        var am = amObj.AddComponent<AlchemyManager>();
        var amSO = new SerializedObject(am);
        amSO.FindProperty("_flaskSprite").objectReferenceValue = flaskSprite;
        amSO.FindProperty("_petSprite").objectReferenceValue = petSprite;
        amSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        var petText = CT(canvasObj.transform, "PetText", "ペット: 0/5", 32, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(250,40), new Vector2(0,-15));
        petText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; petText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f,0.6f,1f);

        var elementText = CT(canvasObj.transform, "ElementText", "", 24, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(400,30), new Vector2(0,-50));
        elementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; elementText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f,0.8f,0.9f);

        var fireBtn = CB(canvasObj.transform, "FireButton", "火", 24, jpFont, new Vector2(0.15f,0), new Vector2(0.15f,0), new Vector2(0.5f,0), new Vector2(140,55), new Vector2(0,100), new Color(0.7f,0.3f,0.1f,0.9f));
        var waterBtn = CB(canvasObj.transform, "WaterButton", "水", 24, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(140,55), new Vector2(0,100), new Color(0.2f,0.4f,0.7f,0.9f));
        var earthBtn = CB(canvasObj.transform, "EarthButton", "土", 24, jpFont, new Vector2(0.85f,0), new Vector2(0.85f,0), new Vector2(0.5f,0), new Vector2(140,55), new Vector2(0,100), new Color(0.5f,0.35f,0.2f,0.9f));
        var resetBtn = CB(canvasObj.transform, "ResetButton", "リセット", 20, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(130,40), new Vector2(0,160), new Color(0.4f,0.4f,0.4f,0.9f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.12f,0.08f,0.2f,0.95f));
        CT(clearPanel.transform, "CT", "全ペット発見！", 48, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 36, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.4f,0.25f,0.5f));
        clearPanel.SetActive(false);

        var uiObj = new GameObject("AlchemyPetUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<AlchemyPetUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_petText").objectReferenceValue = petText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_elementText").objectReferenceValue = elementText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_fireButton").objectReferenceValue = fireBtn.GetComponent<Button>();
        uiSO.FindProperty("_waterButton").objectReferenceValue = waterBtn.GetComponent<Button>();
        uiSO.FindProperty("_earthButton").objectReferenceValue = earthBtn.GetComponent<Button>();
        uiSO.FindProperty("_resetButton").objectReferenceValue = resetBtn.GetComponent<Button>();
        uiSO.FindProperty("_alchemyManager").objectReferenceValue = am;
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_alchemyManager").objectReferenceValue = am;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_totalPetTypes").intValue = 5;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(fireBtn.GetComponent<Button>().onClick, am.SelectElement, 0);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(waterBtn.GetComponent<Button>().onClick, am.SelectElement, 1);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(earthBtn.GetComponent<Button>().onClick, am.SelectElement, 2);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.GetComponent<Button>().onClick, am.ClearSelection);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/088_AlchemyPet.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup088] AlchemyPet シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
