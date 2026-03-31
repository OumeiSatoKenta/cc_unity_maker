using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game021_BladeDash;

public static class Setup021_BladeDash
{
    [MenuItem("Assets/Setup/021 BladeDash")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup021_BladeDash] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.08f, 0.08f, 0.15f, 1f); camera.orthographic = true; camera.orthographicSize = 5f; }

        string whiteTexPath = "Assets/Scripts/Game021_BladeDash/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        { var wTex = new Texture2D(4, 4); var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white; wTex.SetPixels(px); wTex.Apply(); System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG()); Object.DestroyImmediate(wTex); AssetDatabase.ImportAsset(whiteTexPath); var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter; if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); } }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);
        string sp = "Assets/Resources/Sprites/Game021_BladeDash/"; string pd = "Assets/Scripts/Game021_BladeDash/";

        var playerPrefab = SP(pd + "PlayerPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "player.png"), whiteSprite, 10);
        var bladePrefab = SP(pd + "BladePrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "blade.png"), whiteSprite, 5);
        var coinPrefab = SP(pd + "CoinPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "coin.png"), whiteSprite, 5);

        // Lane dividers
        for (int i = -1; i <= 1; i++)
        {
            var lane = new GameObject($"LaneDivider_{i}");
            var sr = lane.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSprite;
            sr.color = new Color(0.2f, 0.22f, 0.3f, 0.5f);
            sr.sortingOrder = -5;
            lane.transform.position = new Vector3(i * 2f + 1f, 0, 0);
            lane.transform.localScale = new Vector3(0.04f, 12f, 1f);
        }

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<BladeDashGameManager>();
        var boardObj = new GameObject("RunBoard"); boardObj.transform.SetParent(gmObj.transform);
        var rm = boardObj.AddComponent<RunManager>();
        var rmSO = new SerializedObject(rm);
        rmSO.FindProperty("_bladePrefab").objectReferenceValue = bladePrefab;
        rmSO.FindProperty("_coinPrefab").objectReferenceValue = coinPrefab;
        rmSO.FindProperty("_playerPrefab").objectReferenceValue = playerPrefab;
        rmSO.FindProperty("_laneWidth").floatValue = 2.0f;
        rmSO.FindProperty("_scrollSpeed").floatValue = 4.0f;
        rmSO.FindProperty("_spawnInterval").floatValue = 1.2f;
        rmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); canvasObj.AddComponent<GraphicRaycaster>();

        var scoreText = CT(canvasObj.transform, "ScoreText", "スコア: 0", 36, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,60), new Vector2(0,-20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20), new Color(0.3f,0.3f,0.4f,0.9f));

        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform)); goPanel.transform.SetParent(canvasObj.transform, false);
        goPanel.AddComponent<Image>().color = new Color(0,0,0,0.85f);
        var gr = goPanel.GetComponent<RectTransform>(); gr.anchorMin = new Vector2(0.2f,0.25f); gr.anchorMax = new Vector2(0.8f,0.75f); gr.offsetMin = gr.offsetMax = Vector2.zero;
        var goText = CT(goPanel.transform, "GameOverText", "ゲームオーバー", 48, jpFont, new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        goText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 28, jpFont, new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero, new Color(0.6f,0.2f,0.2f,1f));
        goPanel.SetActive(false);

        var uiObj = new GameObject("BladeDashUI"); var bdUI = uiObj.AddComponent<BladeDashUI>();
        var uiSO = new SerializedObject(bdUI);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = goText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_runManager").objectReferenceValue = rm;
        gmSO.FindProperty("_ui").objectReferenceValue = bdUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/021_BladeDash.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup021_BladeDash] BladeDash シーンを作成しました: " + scenePath);
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
