using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game031_BounceKing;

public static class Setup031_BounceKing
{
    [MenuItem("Assets/Setup/031 BounceKing")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup031_BounceKing] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f); camera.orthographic = true; camera.orthographicSize = 5f; }

        string whiteTexPath = "Assets/Scripts/Game031_BounceKing/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        { var wTex = new Texture2D(4, 4); var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white; wTex.SetPixels(px); wTex.Apply(); System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG()); Object.DestroyImmediate(wTex); AssetDatabase.ImportAsset(whiteTexPath); var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter; if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); } }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);
        string sp = "Assets/Resources/Sprites/Game031_BounceKing/"; string pd = "Assets/Scripts/Game031_BounceKing/";

        var ballPrefab = SP(pd + "BallPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "ball.png"), whiteSprite, 10);
        var paddlePrefab = SP(pd + "PaddlePrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "paddle.png"), whiteSprite, 8);
        var blockPrefab = SP(pd + "BlockPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "block.png"), whiteSprite, 5);

        // Walls
        var wallL = new GameObject("WallLeft"); var wlsr = wallL.AddComponent<SpriteRenderer>(); wlsr.sprite = whiteSprite; wlsr.color = new Color(0.15f,0.15f,0.25f); wlsr.sortingOrder = -2; wallL.transform.position = new Vector3(-5.7f, 0, 0); wallL.transform.localScale = new Vector3(0.3f, 12f, 1);
        var wallR = new GameObject("WallRight"); var wrsr = wallR.AddComponent<SpriteRenderer>(); wrsr.sprite = whiteSprite; wrsr.color = new Color(0.15f,0.15f,0.25f); wrsr.sortingOrder = -2; wallR.transform.position = new Vector3(5.7f, 0, 0); wallR.transform.localScale = new Vector3(0.3f, 12f, 1);
        var wallT = new GameObject("WallTop"); var wtsr = wallT.AddComponent<SpriteRenderer>(); wtsr.sprite = whiteSprite; wtsr.color = new Color(0.15f,0.15f,0.25f); wtsr.sortingOrder = -2; wallT.transform.position = new Vector3(0, 4.7f, 0); wallT.transform.localScale = new Vector3(12f, 0.3f, 1);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<BounceKingGameManager>();
        var boardObj = new GameObject("BounceBoard"); boardObj.transform.SetParent(gmObj.transform);
        var bm = boardObj.AddComponent<BounceManager>();
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("_ballPrefab").objectReferenceValue = ballPrefab;
        bmSO.FindProperty("_paddlePrefab").objectReferenceValue = paddlePrefab;
        bmSO.FindProperty("_blockPrefab").objectReferenceValue = blockPrefab;
        bmSO.FindProperty("_ballSpeed").floatValue = 6f;
        bmSO.FindProperty("_blockRows").intValue = 4;
        bmSO.FindProperty("_blockCols").intValue = 7;
        bmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); canvasObj.AddComponent<GraphicRaycaster>();

        var scoreText = CT(canvasObj.transform, "ScoreText", "スコア: 0", 32, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(250,50), new Vector2(20,-20));
        var livesText = CT(canvasObj.transform, "LivesText", "ライフ: 3", 28, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,40), new Vector2(20,-65));
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20), new Color(0.3f,0.3f,0.4f,0.9f));

        var resultPanel = new GameObject("ResultPanel", typeof(RectTransform)); resultPanel.transform.SetParent(canvasObj.transform, false);
        resultPanel.AddComponent<Image>().color = new Color(0,0,0,0.85f);
        var rr = resultPanel.GetComponent<RectTransform>(); rr.anchorMin = new Vector2(0.2f,0.25f); rr.anchorMax = new Vector2(0.8f,0.75f); rr.offsetMin = rr.offsetMax = Vector2.zero;
        var resultText = CT(resultPanel.transform, "ResultText", "", 44, jpFont, new Vector2(0.5f,0.6f), new Vector2(0.5f,0.6f), new Vector2(0.5f,0.5f), new Vector2(400,200), Vector2.zero);
        resultText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(resultPanel.transform, "RetryButton", "リトライ", 28, jpFont, new Vector2(0.5f,0.15f), new Vector2(0.5f,0.15f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero, new Color(0.3f,0.4f,0.8f,1f));
        resultPanel.SetActive(false);

        var uiObj = new GameObject("BounceKingUI"); var bkUI = uiObj.AddComponent<BounceKingUI>();
        var uiSO = new SerializedObject(bkUI);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_livesText").objectReferenceValue = livesText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_resultPanel").objectReferenceValue = resultPanel;
        uiSO.FindProperty("_resultText").objectReferenceValue = resultText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_bounceManager").objectReferenceValue = bm;
        gmSO.FindProperty("_ui").objectReferenceValue = bkUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/031_BounceKing.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup031_BounceKing] BounceKing シーンを作成しました: " + scenePath);
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
