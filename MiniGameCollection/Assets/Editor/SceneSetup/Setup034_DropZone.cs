using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game034_DropZone;

public static class Setup034_DropZone
{
    [MenuItem("Assets/Setup/034 DropZone")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup034_DropZone] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f); camera.orthographic = true; camera.orthographicSize = 5f; }

        string whiteTexPath = "Assets/Scripts/Game034_DropZone/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        { var wTex = new Texture2D(4, 4); var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white; wTex.SetPixels(px); wTex.Apply(); System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG()); Object.DestroyImmediate(wTex); AssetDatabase.ImportAsset(whiteTexPath); var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter; if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); } }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);
        string sp = "Assets/Resources/Sprites/Game034_DropZone/"; string pd = "Assets/Scripts/Game034_DropZone/";

        // Zone visuals at bottom
        Color[] zoneColors = { new Color(0.86f,0.24f,0.24f,0.3f), new Color(0.24f,0.63f,0.24f,0.3f), new Color(0.24f,0.39f,0.86f,0.3f) };
        string[] zoneLabels = { "赤", "緑", "青" };
        float[] zoneX = { -3f, 0f, 3f };
        for (int i = 0; i < 3; i++)
        {
            var zone = new GameObject($"Zone_{i}"); var zsr = zone.AddComponent<SpriteRenderer>(); zsr.sprite = whiteSprite; zsr.color = zoneColors[i]; zsr.sortingOrder = -3;
            zone.transform.position = new Vector3(zoneX[i], -3.5f, 0); zone.transform.localScale = new Vector3(2.5f, 1.5f, 1);
        }

        var itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "item_red.png");
        var itemObj = new GameObject("ItemPrefab"); var isr = itemObj.AddComponent<SpriteRenderer>(); isr.sprite = itemSprite ?? whiteSprite; isr.sortingOrder = 5;
        itemObj.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.5f); itemObj.AddComponent<DropItem>();
        var itemPrefab = PrefabUtility.SaveAsPrefabAsset(itemObj, pd + "ItemPrefab.prefab"); Object.DestroyImmediate(itemObj);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<DropZoneGameManager>();
        var boardObj = new GameObject("DropBoard"); boardObj.transform.SetParent(gmObj.transform);
        var dm = boardObj.AddComponent<DropManager>();
        var dmSO = new SerializedObject(dm);
        dmSO.FindProperty("_itemPrefab").objectReferenceValue = itemPrefab;
        dmSO.FindProperty("_spawnInterval").floatValue = 1.2f;
        dmSO.FindProperty("_gameTime").floatValue = 30f;
        dmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); canvasObj.AddComponent<GraphicRaycaster>();

        var scoreText = CT(canvasObj.transform, "ScoreText", "スコア: 0", 28, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,40), new Vector2(20,-20));
        var missText = CT(canvasObj.transform, "MissText", "ミス: 0", 28, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,40), new Vector2(20,-60));
        var timeText = CT(canvasObj.transform, "TimeText", "残り: 30秒", 32, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(250,50), new Vector2(0,-20));
        timeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20), new Color(0.3f,0.3f,0.4f,0.9f));

        var resultPanel = new GameObject("ResultPanel", typeof(RectTransform)); resultPanel.transform.SetParent(canvasObj.transform, false);
        resultPanel.AddComponent<Image>().color = new Color(0,0,0,0.85f);
        var rr = resultPanel.GetComponent<RectTransform>(); rr.anchorMin = new Vector2(0.2f,0.2f); rr.anchorMax = new Vector2(0.8f,0.8f); rr.offsetMin = rr.offsetMax = Vector2.zero;
        var resultText = CT(resultPanel.transform, "ResultText", "", 40, jpFont, new Vector2(0.5f,0.6f), new Vector2(0.5f,0.6f), new Vector2(0.5f,0.5f), new Vector2(400,200), Vector2.zero);
        resultText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(resultPanel.transform, "RetryButton", "リトライ", 28, jpFont, new Vector2(0.5f,0.1f), new Vector2(0.5f,0.1f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero, new Color(0.3f,0.4f,0.6f,1f));
        resultPanel.SetActive(false);

        var uiObj = new GameObject("DropZoneUI"); var dzUI = uiObj.AddComponent<DropZoneUI>();
        var uiSO = new SerializedObject(dzUI);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_missText").objectReferenceValue = missText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timeText").objectReferenceValue = timeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_resultPanel").objectReferenceValue = resultPanel;
        uiSO.FindProperty("_resultText").objectReferenceValue = resultText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_dropManager").objectReferenceValue = dm;
        gmSO.FindProperty("_ui").objectReferenceValue = dzUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/034_DropZone.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup034_DropZone] DropZone シーンを作成しました: " + scenePath);
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); var tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white; if (f != null) tmp.font = f; var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; return o; }
    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    { var o = new GameObject(n, typeof(RectTransform)); o.transform.SetParent(p, false); o.AddComponent<Image>().color = bg; o.AddComponent<Button>(); var r = o.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap; var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(o.transform, false); var tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center; if (f != null) tmp.font = f; var tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero; return o; }
    private static void AddSceneToBuildSettings(string scenePath)
    { var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes); foreach (var s in scenes) if (s.path == scenePath) return; scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
}
