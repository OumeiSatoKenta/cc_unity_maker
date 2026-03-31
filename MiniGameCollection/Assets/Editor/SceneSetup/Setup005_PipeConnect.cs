using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game005_PipeConnect;

public static class Setup005_PipeConnect
{
    [MenuItem("Assets/Setup/005 PipeConnect")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup005_PipeConnect] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.11f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
        }

        // White sprite
        string whiteTexPath = "Assets/Scripts/Game005_PipeConnect/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        {
            var wTex = new Texture2D(4, 4);
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            wTex.SetPixels(px);
            wTex.Apply();
            System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG());
            Object.DestroyImmediate(wTex);
            AssetDatabase.ImportAsset(whiteTexPath);
            var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter;
            if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); }
        }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);

        string sp = "Assets/Resources/Sprites/Game005_PipeConnect/";
        var straightSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "pipe_straight.png");
        var bendSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "pipe_bend.png");
        var crossSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "pipe_cross.png");
        var tSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "pipe_t.png");
        var sourceSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "source.png");
        var goalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "goal.png");
        var cellBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "cell_bg.png");
        var boardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "board_background.png");

        // Board background
        var bgObj = new GameObject("BoardBackground");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = boardBgSprite != null ? boardBgSprite : whiteSprite;
        bgSr.color = boardBgSprite != null ? Color.white : new Color(0.05f, 0.06f, 0.11f);
        bgObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        bgSr.sortingOrder = -10;

        string pd = "Assets/Scripts/Game005_PipeConnect/";

        var straightPrefab = CreatePipePrefab(pd + "PipeStraightPrefab.prefab", straightSprite, whiteSprite);
        var bendPrefab = CreatePipePrefab(pd + "PipeBendPrefab.prefab", bendSprite, whiteSprite);
        var crossPrefab = CreatePipePrefab(pd + "PipeCrossPrefab.prefab", crossSprite, whiteSprite);
        var tPrefab = CreatePipePrefab(pd + "PipeTJunctionPrefab.prefab", tSprite, whiteSprite);
        var sourcePrefab = CreatePipePrefab(pd + "SourcePrefab.prefab", sourceSprite, whiteSprite);
        var goalPrefab = CreatePipePrefab(pd + "GoalPrefab.prefab", goalSprite, whiteSprite);
        var cellBgPrefab = CreateSimplePrefab(pd + "CellBgPrefab.prefab", "CellBgPrefab", cellBgSprite, whiteSprite, -5);

        // GameManager + PipeManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<PipeConnectGameManager>();
        var boardObj = new GameObject("PipeBoard");
        boardObj.transform.SetParent(gmObj.transform);
        var pm = boardObj.AddComponent<PipeManager>();

        var pmSO = new SerializedObject(pm);
        pmSO.FindProperty("_gridWidth").intValue = 5;
        pmSO.FindProperty("_gridHeight").intValue = 5;
        pmSO.FindProperty("_cellSize").floatValue = 1.0f;
        pmSO.FindProperty("_pipeStraightPrefab").objectReferenceValue = straightPrefab;
        pmSO.FindProperty("_pipeBendPrefab").objectReferenceValue = bendPrefab;
        pmSO.FindProperty("_pipeCrossPrefab").objectReferenceValue = crossPrefab;
        pmSO.FindProperty("_pipeTJunctionPrefab").objectReferenceValue = tPrefab;
        pmSO.FindProperty("_sourcePrefab").objectReferenceValue = sourcePrefab;
        pmSO.FindProperty("_goalPrefab").objectReferenceValue = goalPrefab;
        pmSO.FindProperty("_cellBgPrefab").objectReferenceValue = cellBgPrefab;
        pmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        var moveTextObj = CreateText(canvasObj.transform, "MoveCountText", "手数: 0", 32, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(250, 50), new Vector2(20, -20));
        var stageTextObj = CreateText(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(300, 50), new Vector2(0, -20));
        stageTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var menuBtnObj = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(240, 50), new Vector2(-20, -20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // Clear panel
        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);
        var cr = clearPanel.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.2f, 0.2f); cr.anchorMax = new Vector2(0.8f, 0.8f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearTextObj = CreateText(clearPanel.transform, "ClearText", "クリア!", 48, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(400, 150), Vector2.zero);
        clearTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var restartBtn = CreateButton(clearPanel.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.3f, 0.15f), new Vector2(0.3f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(200, 60), Vector2.zero,
            new Color(0.1f, 0.5f, 0.3f, 1f));
        var nextBtn = CreateButton(clearPanel.transform, "NextStageButton", "次のステージ", 28, jpFont,
            new Vector2(0.7f, 0.15f), new Vector2(0.7f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(220, 60), Vector2.zero,
            new Color(0.2f, 0.4f, 0.7f, 1f));
        clearPanel.SetActive(false);

        // PipeConnectUI
        var uiObj = new GameObject("PipeConnectUI");
        var pcUI = uiObj.AddComponent<PipeConnectUI>();
        var uiSO = new SerializedObject(pcUI);
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtnObj.AddComponent<BackToMenuButton>();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_pipeManager").objectReferenceValue = pm;
        gmSO.FindProperty("_ui").objectReferenceValue = pcUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/005_PipeConnect.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup005_PipeConnect] PipeConnect シーンを作成しました: " + scenePath);
    }

    private static GameObject CreatePipePrefab(string path, Sprite sprite, Sprite fallback)
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(path);
        var obj = new GameObject(name);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : fallback;
        sr.sortingOrder = 5;
        var col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);
        obj.AddComponent<PipeTile>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreateSimplePrefab(string path, string name, Sprite sprite, Sprite fallback, int sortOrder)
    {
        var obj = new GameObject(name);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : fallback;
        sr.sortingOrder = sortOrder;
        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreateText(Transform parent, string name, string text, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = Color.white;
        if (font != null) tmp.font = font;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax; r.pivot = pivot; r.sizeDelta = sizeDelta; r.anchoredPosition = anchoredPos;
        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = bgColor;
        obj.AddComponent<Button>();
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax; r.pivot = pivot; r.sizeDelta = sizeDelta; r.anchoredPosition = anchoredPos;
        var tObj = new GameObject("Text", typeof(RectTransform));
        tObj.transform.SetParent(obj.transform, false);
        var tmp = tObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        var tr = tObj.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        return obj;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
