using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game002_MirrorMaze;

public static class Setup002_MirrorMaze
{
    [MenuItem("Assets/Setup/002 MirrorMaze")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup002_MirrorMaze] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // --- カメラ設定 ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.16f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        // --- 白スプライト（グリッド線・プレハブ用） ---
        string whiteTexPath = "Assets/Scripts/Game002_MirrorMaze/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        {
            var wTex = new Texture2D(4, 4);
            var wPixels = new Color[16];
            for (int i = 0; i < 16; i++) wPixels[i] = Color.white;
            wTex.SetPixels(wPixels);
            wTex.Apply();
            System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG());
            Object.DestroyImmediate(wTex);
            AssetDatabase.ImportAsset(whiteTexPath);
            var wImporter = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter;
            if (wImporter != null)
            {
                wImporter.textureType = TextureImporterType.Sprite;
                wImporter.spritePixelsPerUnit = 1;
                wImporter.SaveAndReimport();
            }
        }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);

        // --- スプライト読み込み ---
        string spritePath = "Assets/Resources/Sprites/Game002_MirrorMaze/";
        var boardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "board_background.png");
        var cellEmptySprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "cell_empty.png");
        var mirrorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "mirror.png");
        var mirrorSlotSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "mirror_slot.png");
        var wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "wall.png");
        var laserSourceSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "laser_source.png");
        var goalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "goal.png");
        var laserBeamSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "laser_beam.png");

        // --- 盤面背景 ---
        var boardBgObj = new GameObject("BoardBackground");
        var boardBgSr = boardBgObj.AddComponent<SpriteRenderer>();
        boardBgSr.sprite = boardBgSprite != null ? boardBgSprite : whiteSprite;
        boardBgSr.color = boardBgSprite != null ? Color.white : new Color(0.08f, 0.10f, 0.16f, 1f);
        boardBgObj.transform.localScale = new Vector3(3.0f, 3.0f, 1f);
        boardBgSr.sortingOrder = -10;

        // --- プレハブ生成 ---
        string prefabDir = "Assets/Scripts/Game002_MirrorMaze/";

        var mirrorObjInScene = CreateSpritePrefab(prefabDir + "MirrorPrefab.prefab", mirrorSprite, whiteSprite, 10, true);
        mirrorObjInScene.AddComponent<MirrorController>();
        var mirrorPrefab = PrefabUtility.SaveAsPrefabAsset(mirrorObjInScene, prefabDir + "MirrorPrefab.prefab");
        Object.DestroyImmediate(mirrorObjInScene);

        var wallPrefab = CreateSimplePrefab(prefabDir + "WallPrefab.prefab", "WallPrefab", wallSprite, whiteSprite, 5);
        var laserSourcePrefab = CreateSimplePrefab(prefabDir + "LaserSourcePrefab.prefab", "LaserSourcePrefab", laserSourceSprite, whiteSprite, 8);
        var goalPrefab = CreateSimplePrefab(prefabDir + "GoalPrefab.prefab", "GoalPrefab", goalSprite, whiteSprite, 8);
        var emptyCellPrefab = CreateSimplePrefab(prefabDir + "EmptyCellPrefab.prefab", "EmptyCellPrefab", cellEmptySprite, whiteSprite, -5);
        var mirrorSlotPrefab = CreateSimplePrefab(prefabDir + "MirrorSlotPrefab.prefab", "MirrorSlotPrefab", mirrorSlotSprite, whiteSprite, -3);

        // LaserBeam プレハブ
        var laserBeamObj = new GameObject("LaserBeamPrefab");
        var lbSr = laserBeamObj.AddComponent<SpriteRenderer>();
        lbSr.sprite = laserBeamSprite != null ? laserBeamSprite : whiteSprite;
        lbSr.color = laserBeamSprite != null ? Color.white : new Color(1f, 0.15f, 0.15f, 0.9f);
        lbSr.sortingOrder = 15;
        var laserBeamPrefab = PrefabUtility.SaveAsPrefabAsset(laserBeamObj, prefabDir + "LaserBeamPrefab.prefab");
        Object.DestroyImmediate(laserBeamObj);

        // --- GameManager + MazeManager ---
        var gameManagerObj = new GameObject("GameManager");
        var gameManager = gameManagerObj.AddComponent<MirrorMazeGameManager>();

        var boardObj = new GameObject("MazeBoard");
        boardObj.transform.SetParent(gameManagerObj.transform);
        var mazeManager = boardObj.AddComponent<MazeManager>();

        var mazeSO = new SerializedObject(mazeManager);
        mazeSO.FindProperty("_gridWidth").intValue = 7;
        mazeSO.FindProperty("_gridHeight").intValue = 7;
        mazeSO.FindProperty("_cellSize").floatValue = 1.0f;
        mazeSO.FindProperty("_mirrorPrefab").objectReferenceValue = mirrorPrefab;
        mazeSO.FindProperty("_wallPrefab").objectReferenceValue = wallPrefab;
        mazeSO.FindProperty("_laserSourcePrefab").objectReferenceValue = laserSourcePrefab;
        mazeSO.FindProperty("_goalPrefab").objectReferenceValue = goalPrefab;
        mazeSO.FindProperty("_laserBeamPrefab").objectReferenceValue = laserBeamPrefab;
        mazeSO.FindProperty("_emptyCellPrefab").objectReferenceValue = emptyCellPrefab;
        mazeSO.FindProperty("_mirrorSlotPrefab").objectReferenceValue = mirrorSlotPrefab;
        mazeSO.ApplyModifiedProperties();

        // --- Canvas (UI) ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 手数テキスト（左上）
        var moveTextObj = CreateText(canvasObj.transform, "MoveCountText", "手数: 0", 32, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(250, 50), new Vector2(20, -20));

        // ステージテキスト（中央上）
        var stageTextObj = CreateText(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 50), new Vector2(0, -20));
        stageTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // メニューへ戻るボタン（右上）
        var menuBtnObj = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(240, 50), new Vector2(-20, -20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // クリアパネル（中央）
        var clearPanelObj = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanelObj.transform.SetParent(canvasObj.transform, false);
        var clearBg = clearPanelObj.AddComponent<Image>();
        clearBg.color = new Color(0f, 0f, 0f, 0.8f);
        var clearRect = clearPanelObj.GetComponent<RectTransform>();
        clearRect.anchorMin = new Vector2(0.2f, 0.2f);
        clearRect.anchorMax = new Vector2(0.8f, 0.8f);
        clearRect.offsetMin = Vector2.zero;
        clearRect.offsetMax = Vector2.zero;

        var clearTextObj = CreateText(clearPanelObj.transform, "ClearText", "クリア!", 48, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 150), Vector2.zero);
        clearTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var restartBtnObj = CreateButton(clearPanelObj.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.2f, 0.15f), new Vector2(0.2f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero,
            new Color(0.1f, 0.5f, 0.3f, 1f));

        var nextStageBtnObj = CreateButton(clearPanelObj.transform, "NextStageButton", "次のステージ", 28, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 60), Vector2.zero,
            new Color(0.2f, 0.4f, 0.7f, 1f));

        var clearMenuBtnObj = CreateButton(clearPanelObj.transform, "ClearMenuButton", "メニューへ", 28, jpFont,
            new Vector2(0.8f, 0.15f), new Vector2(0.8f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero,
            new Color(0.3f, 0.3f, 0.5f, 1f));

        clearPanelObj.SetActive(false);

        // --- MirrorMazeUI ---
        var uiObj = new GameObject("MirrorMazeUI");
        var mirrorMazeUI = uiObj.AddComponent<MirrorMazeUI>();

        var uiSO = new SerializedObject(mirrorMazeUI);
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanelObj;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextStageBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gameManager;
        uiSO.ApplyModifiedProperties();

        // メニューへ戻るボタン（右上）に BackToMenuButton を追加
        menuBtnObj.AddComponent<BackToMenuButton>();

        // --- GameManager の参照設定 ---
        var gmSO = new SerializedObject(gameManager);
        gmSO.FindProperty("_mazeManager").objectReferenceValue = mazeManager;
        gmSO.FindProperty("_ui").objectReferenceValue = mirrorMazeUI;
        gmSO.ApplyModifiedProperties();

        // --- EventSystem ---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        // シーン保存
        string scenePath = "Assets/Scenes/002_MirrorMaze.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup002_MirrorMaze] MirrorMaze シーンを作成しました: " + scenePath);
    }

    private static GameObject CreateSpritePrefab(string path, Sprite sprite, Sprite fallback, int sortOrder, bool addCollider)
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(path);
        var obj = new GameObject(name);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : fallback;
        sr.color = Color.white;
        sr.sortingOrder = sortOrder;
        if (addCollider)
        {
            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.9f, 0.9f);
        }
        return obj;
    }

    private static GameObject CreateSimplePrefab(string path, string name, Sprite sprite, Sprite fallback, int sortOrder)
    {
        var obj = new GameObject(name);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : fallback;
        sr.color = Color.white;
        sr.sortingOrder = sortOrder;
        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreateText(Transform parent, string name, string text, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        if (font != null) tmp.font = font;

        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPos;

        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        var image = obj.AddComponent<Image>();
        image.color = bgColor;
        obj.AddComponent<Button>();

        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPos;

        var textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return obj;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return;
        }
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
