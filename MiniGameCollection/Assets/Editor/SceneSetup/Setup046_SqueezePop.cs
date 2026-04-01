using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game046_SqueezePop;

public static class Setup046_SqueezePop
{
    [MenuItem("Assets/Setup/046 SqueezePop")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup046] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game046_SqueezePop/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.98f, 0.96f, 0.94f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite bubbleSprite = LoadSprite(sp + "bubble.png");
        Sprite popSprite = LoadSprite(sp + "pop.png");

        var bgObj = new GameObject("Background"); bgObj.AddComponent<SpriteRenderer>().sprite = bgSprite;
        bgObj.GetComponent<SpriteRenderer>().sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<SqueezePopGameManager>();
        var pmObj = new GameObject("PopManager"); pmObj.transform.SetParent(gmObj.transform);
        var pm = pmObj.AddComponent<PopManager>();
        var pmSO = new SerializedObject(pm);
        pmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        pmSO.FindProperty("_bubbleSprite").objectReferenceValue = bubbleSprite;
        pmSO.FindProperty("_popSprite").objectReferenceValue = popSprite;
        pmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); canvasObj.AddComponent<GraphicRaycaster>();

        var remainText = CT(canvasObj.transform, "RemainingText", "残り: 15", 36, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,50), new Vector2(20,-20));
        remainText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        remainText.GetComponent<TextMeshProUGUI>().color = Color.black;
        var timerText = CT(canvasObj.transform, "TimerText", "30.0s", 36, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(200,50), new Vector2(-20,-20));
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        timerText.GetComponent<TextMeshProUGUI>().color = Color.black;

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.9f,0.95f,1f,0.95f));
        CT(clearPanel.transform, "CT", "全部ポップ！", 52, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearTimeText = CT(clearPanel.transform, "CS", "", 40, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        clearTimeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.5f,0.3f,0.6f));
        clearPanel.SetActive(false);

        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.95f,0.9f,0.9f,0.95f));
        CT(goPanel.transform, "GT", "タイムアップ！", 52, jpFont, new Vector2(0.5f,0.7f), new Vector2(0.5f,0.7f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont, new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.5f,0.2f,0.1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("SqueezePopUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<SqueezePopUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_remainingText").objectReferenceValue = remainText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearTimeText").objectReferenceValue = clearTimeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_manager").objectReferenceValue = pm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_totalBubbles").intValue = 15;
        gmSO.FindProperty("_timeLimit").floatValue = 30f;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/046_SqueezePop.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup046] SqueezePop シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f,0.3f); r.anchorMax = new Vector2(0.9f,0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
