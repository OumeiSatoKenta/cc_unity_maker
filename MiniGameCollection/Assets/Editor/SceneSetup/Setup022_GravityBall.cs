using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game022_GravityBall;

public static class Setup022_GravityBall
{
    [MenuItem("Assets/Setup/022 GravityBall")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup022_GravityBall] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.05f, 0.08f, 0.15f, 1f); camera.orthographic = true; camera.orthographicSize = 5f; }

        string whiteTexPath = "Assets/Scripts/Game022_GravityBall/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        { var wTex = new Texture2D(4, 4); var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white; wTex.SetPixels(px); wTex.Apply(); System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG()); Object.DestroyImmediate(wTex); AssetDatabase.ImportAsset(whiteTexPath); var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter; if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); } }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);
        string sp = "Assets/Resources/Sprites/Game022_GravityBall/"; string pd = "Assets/Scripts/Game022_GravityBall/";

        var ballPrefab = SP(pd + "BallPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "ball.png"), whiteSprite, 10);
        var obsPrefab = SP(pd + "ObstaclePrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "obstacle.png"), whiteSprite, 5);

        // Ceiling and floor lines
        var ceiling = new GameObject("Ceiling"); var csr = ceiling.AddComponent<SpriteRenderer>(); csr.sprite = whiteSprite; csr.color = new Color(0.25f,0.27f,0.35f); csr.sortingOrder = -2; ceiling.transform.position = new Vector3(0, 4.2f, 0); ceiling.transform.localScale = new Vector3(20f, 0.15f, 1f);
        var floor = new GameObject("Floor"); var fsr = floor.AddComponent<SpriteRenderer>(); fsr.sprite = whiteSprite; fsr.color = new Color(0.25f,0.27f,0.35f); fsr.sortingOrder = -2; floor.transform.position = new Vector3(0, -4.2f, 0); floor.transform.localScale = new Vector3(20f, 0.15f, 1f);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<GravityBallGameManager>();
        var boardObj = new GameObject("BallBoard"); boardObj.transform.SetParent(gmObj.transform);
        var bm = boardObj.AddComponent<GravityBallManager>();
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("_ballPrefab").objectReferenceValue = ballPrefab;
        bmSO.FindProperty("_obstaclePrefab").objectReferenceValue = obsPrefab;
        bmSO.FindProperty("_gravityStrength").floatValue = 12f;
        bmSO.FindProperty("_scrollSpeed").floatValue = 3f;
        bmSO.FindProperty("_spawnInterval").floatValue = 2f;
        bmSO.FindProperty("_ceilingY").floatValue = 4f;
        bmSO.FindProperty("_floorY").floatValue = -4f;
        bmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); canvasObj.AddComponent<GraphicRaycaster>();

        var scoreText = CT(canvasObj.transform, "ScoreText", "スコア: 0", 36, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,60), new Vector2(0,-20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var hint = CT(canvasObj.transform, "HintText", "タップで重力反転！障害物を避けよう", 20, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(500,40), new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center; hint.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,0.5f,0.6f);
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20), new Color(0.3f,0.3f,0.4f,0.9f));

        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform)); goPanel.transform.SetParent(canvasObj.transform, false);
        goPanel.AddComponent<Image>().color = new Color(0,0,0,0.85f);
        var gr = goPanel.GetComponent<RectTransform>(); gr.anchorMin = new Vector2(0.2f,0.25f); gr.anchorMax = new Vector2(0.8f,0.75f); gr.offsetMin = gr.offsetMax = Vector2.zero;
        var goText = CT(goPanel.transform, "GameOverText", "ゲームオーバー", 48, jpFont, new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        goText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 28, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero, new Color(0.6f,0.2f,0.2f,1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("GravityBallUI"); var gbUI = uiObj.AddComponent<GravityBallUI>();
        var uiSO = new SerializedObject(gbUI);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = goText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_ballManager").objectReferenceValue = bm;
        gmSO.FindProperty("_ui").objectReferenceValue = gbUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/022_GravityBall.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup022_GravityBall] GravityBall シーンを作成しました: " + scenePath);
    }

    private static GameObject SP(string path, Sprite sprite, Sprite fallback, int order)
    { var o = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path)); var sr = o.AddComponent<SpriteRenderer>(); sr.sprite = sprite ?? fallback; sr.sortingOrder = order; var p = PrefabUtility.SaveAsPrefabAsset(o, path); Object.DestroyImmediate(o); return p; }
    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath)
    { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
