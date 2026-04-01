using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game045_FingerPaint;

public static class Setup045_FingerPaint
{
    [MenuItem("Assets/Setup/045 FingerPaint")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup045] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game045_FingerPaint/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.96f, 0.94f, 0.92f); camera.orthographic = true; camera.orthographicSize = 5.5f; }

        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite brushSprite = LoadSprite(sp + "brush.png");
        Sprite targetSprite = LoadSprite(sp + "target.png");

        var bgObj = new GameObject("Background"); bgObj.AddComponent<SpriteRenderer>().sprite = bgSprite;
        bgObj.GetComponent<SpriteRenderer>().sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<FingerPaintGameManager>();
        var pmObj = new GameObject("PaintManager"); pmObj.transform.SetParent(gmObj.transform);
        var pm = pmObj.AddComponent<PaintManager>();
        var pmSO = new SerializedObject(pm);
        pmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        pmSO.FindProperty("_brushSprite").objectReferenceValue = brushSprite;
        pmSO.FindProperty("_targetSprite").objectReferenceValue = targetSprite;
        pmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); canvasObj.AddComponent<GraphicRaycaster>();

        var matchText = CT(canvasObj.transform, "MatchText", "一致率: 0%", 32, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(250,50), new Vector2(20,-20));
        matchText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        matchText.GetComponent<TextMeshProUGUI>().color = Color.black;

        // Ink slider
        var inkSliderObj = CreateSlider(canvasObj.transform, "InkSlider", new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,25), new Vector2(100,-25));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont, new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20), new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.9f,0.95f,0.9f,0.95f));
        var ct = CT(clearPanel.transform, "CT", "完成！", 52, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero);
        ct.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; ct.GetComponent<TextMeshProUGUI>().color = new Color(0.2f,0.5f,0.2f);
        var clearStarText = CT(clearPanel.transform, "CS", "", 44, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(400,80), Vector2.zero);
        clearStarText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; clearStarText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f,0.5f,0.1f);
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.3f,0.6f,0.3f));
        clearPanel.SetActive(false);

        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.95f,0.9f,0.9f,0.95f));
        var gt = CT(goPanel.transform, "GT", "インク切れ", 48, jpFont, new Vector2(0.5f,0.75f), new Vector2(0.5f,0.75f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero);
        gt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; gt.GetComponent<TextMeshProUGUI>().color = new Color(0.6f,0.2f,0.1f);
        var goText = CT(goPanel.transform, "GS", "", 36, jpFont, new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(400,60), Vector2.zero);
        goText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; goText.GetComponent<TextMeshProUGUI>().color = Color.black;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero, new Color(0.5f,0.2f,0.1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("FingerPaintUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<FingerPaintUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_matchText").objectReferenceValue = matchText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_inkSlider").objectReferenceValue = inkSliderObj.GetComponent<Slider>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearStarText").objectReferenceValue = clearStarText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = goText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_manager").objectReferenceValue = pm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_maxInk").floatValue = 100f;
        gmSO.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/045_FingerPaint.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup045] FingerPaint シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path) { if (!File.Exists(path)) return null; AssetDatabase.ImportAsset(path); var imp = AssetImporter.GetAtPath(path) as TextureImporter; if (imp != null && imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 100; imp.SaveAndReimport(); } return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    private static GameObject CreatePanel(Transform parent, string name, Color color) { var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false); obj.AddComponent<Image>().color = color; var r = obj.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.1f,0.3f); r.anchorMax = new Vector2(0.9f,0.7f); r.offsetMin = r.offsetMax = Vector2.zero; return obj; }
    private static GameObject CreateSlider(Transform parent, string name, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        var slider = obj.AddComponent<Slider>(); var r = obj.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var bg = new GameObject("Background", typeof(RectTransform)); bg.transform.SetParent(obj.transform, false);
        bg.AddComponent<Image>().color = new Color(0.8f,0.8f,0.8f); var bgR = bg.GetComponent<RectTransform>();
        bgR.anchorMin = new Vector2(0,0.25f); bgR.anchorMax = new Vector2(1,0.75f); bgR.offsetMin = bgR.offsetMax = Vector2.zero;
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(obj.transform, false);
        var faR = fillArea.GetComponent<RectTransform>(); faR.anchorMin = new Vector2(0,0.25f); faR.anchorMax = new Vector2(1,0.75f); faR.offsetMin = new Vector2(5,0); faR.offsetMax = new Vector2(-15,0);
        var fill = new GameObject("Fill", typeof(RectTransform)); fill.transform.SetParent(fillArea.transform, false);
        fill.AddComponent<Image>().color = new Color(0.3f,0.3f,0.3f); var fillR = fill.GetComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero; fillR.anchorMax = new Vector2(1,1); fillR.offsetMin = fillR.offsetMax = Vector2.zero;
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)); handleArea.transform.SetParent(obj.transform, false);
        var haR = handleArea.GetComponent<RectTransform>(); haR.anchorMin = Vector2.zero; haR.anchorMax = Vector2.one; haR.offsetMin = new Vector2(10,0); haR.offsetMax = new Vector2(-10,0);
        var handle = new GameObject("Handle", typeof(RectTransform)); handle.transform.SetParent(handleArea.transform, false);
        var hi = handle.AddComponent<Image>(); hi.color = new Color(0.2f,0.2f,0.2f); handle.GetComponent<RectTransform>().sizeDelta = new Vector2(15,0);
        slider.fillRect = fillR; slider.handleRect = handle.GetComponent<RectTransform>(); slider.targetGraphic = hi; slider.interactable = false; slider.value = 1f;
        return obj;
    }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg) { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath) { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
