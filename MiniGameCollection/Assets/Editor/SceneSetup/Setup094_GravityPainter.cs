using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game094_GravityPainter;

public static class Setup094_GravityPainter
{
    [MenuItem("Assets/Setup/094 GravityPainter")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup094] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game094_GravityPainter/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.1f, 0.1f, 0.12f); camera.orthographic = true; camera.orthographicSize = 6f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite cellSprite = LoadSprite(sp + "cell.png");

        var bgObj = new GameObject("Background"); bgObj.AddComponent<SpriteRenderer>().sprite = bgSprite;
        bgObj.GetComponent<SpriteRenderer>().sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(14f, 14f, 1f);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<GravityPainterGameManager>();
        var cmObj = new GameObject("PaintManager"); cmObj.transform.SetParent(gmObj.transform); var cm = cmObj.AddComponent<PaintManager>();
        var cmSO = new SerializedObject(cm);
        cmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        cmSO.FindProperty("_cellSprite").objectReferenceValue = cellSprite;
        cmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD
        var matchText = CT(canvasObj.transform, "MatchText", "一致率: 0%", 32, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(220,45), new Vector2(20,-15));
        matchText.GetComponent<TextMeshProUGUI>().color = Color.white;
        var movesText = CT(canvasObj.transform, "MovesText", "残り 8 回", 32, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(220,45), new Vector2(-20,-15));
        movesText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right; movesText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // お手本ラベル（右上）
        var previewLabel = CT(canvasObj.transform, "PreviewLabel", "お手本", 22, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(160,30), new Vector2(-20,-65));
        previewLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right; previewLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.8f,0.8f,0.8f);

        // 色選択ボタン（左側縦並び）
        Color[] colorBgs = { new Color(0.9f,0.2f,0.2f,0.9f), new Color(0.2f,0.4f,0.9f,0.9f), new Color(0.2f,0.8f,0.3f,0.9f), new Color(0.9f,0.8f,0.1f,0.9f) };
        string[] colorLabels = { "赤", "青", "緑", "黄" };
        GameObject[] colorBtns = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            float yPos = 400f - i * 130f;
            colorBtns[i] = CB(canvasObj.transform, $"ColorBtn{i}", colorLabels[i], 28, jpFont, new Vector2(0,0.5f), new Vector2(0,0.5f), new Vector2(0,0.5f), new Vector2(100,100), new Vector2(30, yPos), colorBgs[i]);
        }

        // 重力ボタン（下部十字）
        var gravUp   = CB(canvasObj.transform, "GravUp",   "↑", 36, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(110,90), new Vector2(0,290), new Color(0.3f,0.35f,0.5f,0.9f));
        var gravDown = CB(canvasObj.transform, "GravDown", "↓", 36, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(110,90), new Vector2(0,90), new Color(0.3f,0.35f,0.5f,0.9f));
        var gravLeft = CB(canvasObj.transform, "GravLeft", "←", 36, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(110,90), new Vector2(-120,190), new Color(0.3f,0.35f,0.5f,0.9f));
        var gravRight= CB(canvasObj.transform, "GravRight","→", 36, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(110,90), new Vector2(120,190), new Color(0.3f,0.35f,0.5f,0.9f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.08f,0.14f,0.2f,0.95f));
        CT(clearPanel.transform, "CT", "クリア！", 52, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "CS", "", 38, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(500,70), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.2f,0.35f,0.55f));
        clearPanel.SetActive(false);

        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f,0.06f,0.06f,0.95f));
        CT(goPanel.transform, "GT", "ゲームオーバー", 44, jpFont, new Vector2(0.5f,0.7f), new Vector2(0.5f,0.7f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScoreText = CT(goPanel.transform, "GS", "", 34, jpFont, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont, new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.5f,0.15f,0.1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("GravityPainterUI"); uiObj.transform.SetParent(gmObj.transform); var ui = uiObj.AddComponent<GravityPainterUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_matchText").objectReferenceValue = matchText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_movesText").objectReferenceValue = movesText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_paintManager").objectReferenceValue = cm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_maxMoves").intValue = 8;
        gmSO.FindProperty("_clearThreshold").floatValue = 0.6f;
        gmSO.ApplyModifiedProperties();

        // 色ボタンリスナー
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            UnityEditor.Events.UnityEventTools.AddIntPersistentListener(colorBtns[i].GetComponent<Button>().onClick, cm.SelectColor, idx);
        }
        // 重力ボタンリスナー
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(gravUp.GetComponent<Button>().onClick,    cm.DropPaint, 0);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(gravDown.GetComponent<Button>().onClick,  cm.DropPaint, 1);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(gravLeft.GetComponent<Button>().onClick,  cm.DropPaint, 2);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(gravRight.GetComponent<Button>().onClick, cm.DropPaint, 3);

        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/094_GravityPainter.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup094] GravityPainter シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f, 0.3f); r.anchorMax = new Vector2(0.9f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
