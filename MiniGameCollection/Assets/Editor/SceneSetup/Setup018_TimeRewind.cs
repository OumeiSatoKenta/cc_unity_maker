using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game018_TimeRewind;

public static class Setup018_TimeRewind
{
    [MenuItem("Assets/Setup/018 TimeRewind")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup018_TimeRewind] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null) { camera.backgroundColor = new Color(0.08f, 0.06f, 0.15f, 1f); camera.orthographic = true; camera.orthographicSize = 5f; }

        string whiteTexPath = "Assets/Scripts/Game018_TimeRewind/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        {
            var wTex = new Texture2D(4, 4); var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white;
            wTex.SetPixels(px); wTex.Apply(); System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG()); Object.DestroyImmediate(wTex);
            AssetDatabase.ImportAsset(whiteTexPath); var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter;
            if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); }
        }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);
        string sp = "Assets/Resources/Sprites/Game018_TimeRewind/";
        string pd = "Assets/Scripts/Game018_TimeRewind/";

        var cellPrefab = SP(pd + "CellPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "cell.png"), whiteSprite, -5);
        var wallPrefab = SP(pd + "WallPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "wall.png"), whiteSprite, 3);
        var goalPrefab = SP(pd + "GoalPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "goal.png"), whiteSprite, 4);
        var ghostPrefab = SP(pd + "GhostPrefab.prefab", AssetDatabase.LoadAssetAtPath<Sprite>(sp + "ghost.png"), whiteSprite, 6);

        var pieceObj = new GameObject("PiecePrefab");
        var psr = pieceObj.AddComponent<SpriteRenderer>();
        psr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "piece.png") ?? whiteSprite;
        psr.sortingOrder = 10;
        var piecePrefab = PrefabUtility.SaveAsPrefabAsset(pieceObj, pd + "PiecePrefab.prefab");
        Object.DestroyImmediate(pieceObj);

        var gmObj = new GameObject("GameManager"); var gm = gmObj.AddComponent<TimeRewindGameManager>();
        var boardObj = new GameObject("RewindBoard"); boardObj.transform.SetParent(gmObj.transform);
        var rm = boardObj.AddComponent<TimeRewindManager>();

        var rmSO = new SerializedObject(rm);
        rmSO.FindProperty("_gridWidth").intValue = 5; rmSO.FindProperty("_gridHeight").intValue = 5;
        rmSO.FindProperty("_cellSize").floatValue = 1.0f;
        rmSO.FindProperty("_cellPrefab").objectReferenceValue = cellPrefab;
        rmSO.FindProperty("_wallPrefab").objectReferenceValue = wallPrefab;
        rmSO.FindProperty("_piecePrefab").objectReferenceValue = piecePrefab;
        rmSO.FindProperty("_goalPrefab").objectReferenceValue = goalPrefab;
        rmSO.FindProperty("_ghostPrefab").objectReferenceValue = ghostPrefab;
        rmSO.ApplyModifiedProperties();

        var canvasObj = new GameObject("Canvas"); var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        var moveText = CT(canvasObj.transform, "MoveCountText", "移動: 0", 28, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,40), new Vector2(20,-20));
        var rewindText = CT(canvasObj.transform, "RewindCountText", "巻戻: 0", 28, jpFont, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,40), new Vector2(20,-60));
        var stageText = CT(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,50), new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont, new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20), new Color(0.3f,0.3f,0.4f,0.9f));
        var rewindBtn = CB(canvasObj.transform, "RewindButton", "⏪ 巻き戻し", 24, jpFont, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0.5f), new Vector2(240,60), new Vector2(0,50), new Color(0.5f,0.3f,0.6f,1f));

        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0,0,0,0.8f);
        var cr = clearPanel.GetComponent<RectTransform>(); cr.anchorMin = new Vector2(0.2f,0.2f); cr.anchorMax = new Vector2(0.8f,0.8f); cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearText = CT(clearPanel.transform, "ClearText", "クリア!", 48, jpFont, new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var restartBtn = CB(clearPanel.transform, "RestartButton", "もう一度", 28, jpFont, new Vector2(0.3f,0.15f), new Vector2(0.3f,0.15f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero, new Color(0.1f,0.5f,0.3f,1f));
        var nextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージ", 28, jpFont, new Vector2(0.7f,0.15f), new Vector2(0.7f,0.15f), new Vector2(0.5f,0.5f), new Vector2(220,60), Vector2.zero, new Color(0.2f,0.4f,0.7f,1f));
        clearPanel.SetActive(false);

        var uiObj = new GameObject("TimeRewindUI"); var trUI = uiObj.AddComponent<TimeRewindUI>();
        var uiSO = new SerializedObject(trUI);
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_rewindCountText").objectReferenceValue = rewindText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_rewindButton").objectReferenceValue = rewindBtn.GetComponent<Button>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_rewindManager").objectReferenceValue = rm;
        gmSO.FindProperty("_ui").objectReferenceValue = trUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        { var eo = new GameObject("EventSystem"); eo.AddComponent<UnityEngine.EventSystems.EventSystem>(); eo.AddComponent<InputSystemUIInputModule>(); }

        string scenePath = "Assets/Scenes/018_TimeRewind.unity";
        EditorSceneManager.SaveScene(scene, scenePath); AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup018_TimeRewind] TimeRewind シーンを作成しました: " + scenePath);
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
