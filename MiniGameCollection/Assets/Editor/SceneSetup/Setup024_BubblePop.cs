using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game024_BubblePop;

public static class Setup024_BubblePop
{
    [MenuItem("Assets/Setup/024 BubblePop")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup024_BubblePop] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.05f, 0.1f, 0.2f, 1f); camera.orthographic = true; camera.orthographicSize = 5f; }

        string whiteTexPath = "Assets/Scripts/Game024_BubblePop/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        { var wTex = new Texture2D(4, 4); var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white; wTex.SetPixels(px); wTex.Apply(); System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG()); Object.DestroyImmediate(wTex); AssetDatabase.ImportAsset(whiteTexPath); var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter; if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); } }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);
        string sp = "Assets/Resources/Sprites/Game024_BubblePop/"; string pd = "Assets/Scripts/Game024_BubblePop/";

        var bubbleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "bubble_red.png");
        var bubbleObj = new GameObject("BubblePrefab");
        var bsr = bubbleObj.AddComponent<SpriteRenderer>(); bsr.sprite = bubbleSprite ?? whiteSprite; bsr.sortingOrder = 5;
        bubbleObj.AddComponent<CircleCollider2D>().radius = 0.4f;
        bubbleObj.AddComponent<Bubble>();
        var bubblePrefab = PrefabUtility.SaveAsPrefabAsset(bubbleObj, pd + "BubblePrefab.prefab"); Object.DestroyImmediate(bubbleObj);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<BubblePopGameManager>();
        var boardObj = new GameObject("BubbleBoard"); boardObj.transform.SetParent(gmObj.transform);
        var bm = boardObj.AddComponent<BubbleManager>();
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("_bubblePrefab").objectReferenceValue = bubblePrefab;
        bmSO.FindProperty("_spawnInterval").floatValue = 0.8f;
        bmSO.FindProperty("_bubbleSpeed").floatValue = 1.5f;
        bmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); canvasObj.AddComponent<GraphicRaycaster>();

        var scoreText = CT(canvasObj.transform, "ScoreText", "スコア: 0", 32, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(250,50), new Vector2(20,-20));
        var livesText = CT(canvasObj.transform, "LivesText", "ライフ: ♥♥♥♥♥", 28, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(300,40), new Vector2(20,-65));
        livesText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20), new Color(0.3f,0.3f,0.4f,0.9f));

        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform)); goPanel.transform.SetParent(canvasObj.transform, false);
        goPanel.AddComponent<Image>().color = new Color(0,0,0,0.85f);
        var gr = goPanel.GetComponent<RectTransform>(); gr.anchorMin = new Vector2(0.2f,0.25f); gr.anchorMax = new Vector2(0.8f,0.75f); gr.offsetMin = gr.offsetMax = Vector2.zero;
        var goText = CT(goPanel.transform, "GameOverText", "ゲームオーバー", 48, jpFont, new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        goText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 28, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero, new Color(0.2f,0.5f,0.7f,1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("BubblePopUI"); var bpUI = uiObj.AddComponent<BubblePopUI>();
        var uiSO = new SerializedObject(bpUI);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_livesText").objectReferenceValue = livesText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = goText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_bubbleManager").objectReferenceValue = bm;
        gmSO.FindProperty("_ui").objectReferenceValue = bpUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/024_BubblePop.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup024_BubblePop] BubblePop シーンを作成しました: " + scenePath);
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath)
    { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
