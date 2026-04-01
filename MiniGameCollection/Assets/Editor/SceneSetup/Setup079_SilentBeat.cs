using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game079_SilentBeat;

public static class Setup079_SilentBeat
{
    [MenuItem("Assets/Setup/079 SilentBeat")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup079] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game079_SilentBeat/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.06f, 0.06f, 0.08f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite circleSprite = LoadSprite(sp + "circle.png");

        var bgObj = new GameObject("Background"); var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite; bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<SilentBeatGameManager>();

        var bmObj = new GameObject("BeatManager"); bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<BeatManager>();
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        bmSO.FindProperty("_circleSprite").objectReferenceValue = circleSprite;
        bmSO.FindProperty("_targetBPM").floatValue = 60f;
        bmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        var timerText = CT(canvasObj.transform, "TimerText", "30.0s", 36, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(200,50), new Vector2(0,-15));
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; timerText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f,0.6f,0.7f);

        var accuracyText = CT(canvasObj.transform, "AccuracyText", "精度: 100%", 28, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(250,35), new Vector2(0,-60));
        accuracyText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; accuracyText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f,0.5f,0.6f);

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.2f,0.2f,0.25f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.08f,0.08f,0.12f,0.95f));
        CT(clearPanel.transform, "CT", "リズム達成", 48, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 32, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(500,60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.25f,0.25f,0.35f));
        clearPanel.SetActive(false);

        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.12f,0.06f,0.06f,0.95f));
        CT(goPanel.transform, "GT", "リズム乱れ", 48, jpFont, new Vector2(0.5f,0.7f), new Vector2(0.5f,0.7f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScoreText = CT(goPanel.transform, "GS", "", 32, jpFont, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(500,60), Vector2.zero);
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont, new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.35f,0.15f,0.1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("SilentBeatUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<SilentBeatUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_accuracyText").objectReferenceValue = accuracyText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_beatManager").objectReferenceValue = bm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_duration").floatValue = 30f;
        gmSO.FindProperty("_tolerance").floatValue = 0.3f;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/079_SilentBeat.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup079] SilentBeat シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
