using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game095_SilentWorld;

public static class Setup095_SilentWorld
{
    [MenuItem("Assets/Setup/095 SilentWorld")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup095] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game095_SilentWorld/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite   = LoadSprite(sp + "background.png");
        Sprite cellSprite = LoadSprite(sp + "cell.png");
        Sprite playerSprite = LoadSprite(sp + "player.png");
        Sprite itemSprite = LoadSprite(sp + "item.png");
        Sprite trapSprite = LoadSprite(sp + "trap.png");
        Sprite exitSprite = LoadSprite(sp + "exit.png");

        var bgObj = new GameObject("Background"); bgObj.AddComponent<SpriteRenderer>().sprite = bgSprite;
        bgObj.GetComponent<SpriteRenderer>().sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(14f, 14f, 1f);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<SilentWorldGameManager>();
        var wmObj = new GameObject("WorldManager"); wmObj.transform.SetParent(gmObj.transform); var wm = wmObj.AddComponent<WorldManager>();
        var wmSO = new SerializedObject(wm);
        wmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        wmSO.FindProperty("_cellSprite").objectReferenceValue = cellSprite;
        wmSO.FindProperty("_playerSprite").objectReferenceValue = playerSprite;
        wmSO.FindProperty("_exitSprite").objectReferenceValue = exitSprite;
        wmSO.FindProperty("_maxHints").intValue = 3;
        wmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        var timerText = CT(canvasObj.transform, "TimerText", "00:00.0", 34, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,45), new Vector2(20,-15));
        timerText.GetComponent<TextMeshProUGUI>().color = Color.white;
        var itemText = CT(canvasObj.transform, "ItemText", "音符 0/3", 30, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(200,40), new Vector2(-20,-15));
        itemText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right; itemText.GetComponent<TextMeshProUGUI>().color = new Color(0.95f,0.85f,0.1f);
        var hintText = CT(canvasObj.transform, "HintText", "ヒント 3", 28, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(180,35), new Vector2(-20,-65));
        hintText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right; hintText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f,0.9f,0.7f);

        var hintBtn = CB(canvasObj.transform, "HintButton", "ヒント", 30, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(200,60), new Vector2(0,140), new Color(0.25f,0.4f,0.3f,0.9f));
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.06f,0.14f,0.18f,0.95f));
        var clearScoreText = CT(clearPanel.transform, "CS", "", 38, jpFont, new Vector2(0.5f,0.6f), new Vector2(0.5f,0.6f), new Vector2(0.5f,0.5f), new Vector2(500,120), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.2f,0.4f,0.3f));
        clearPanel.SetActive(false);

        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f,0.06f,0.06f,0.95f));
        CT(goPanel.transform, "GT", "トラップにはまった", 42, jpFont, new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont, new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.5f,0.15f,0.1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("SilentWorldUI"); uiObj.transform.SetParent(gmObj.transform); var ui = uiObj.AddComponent<SilentWorldUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_itemText").objectReferenceValue = itemText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_hintText").objectReferenceValue = hintText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_worldManager").objectReferenceValue = wm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_totalItems").intValue = 3;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(hintBtn.GetComponent<Button>().onClick, wm.UseHint);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/095_SilentWorld.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup095] SilentWorld シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
