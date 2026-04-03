using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game003_GravitySwitch;

public static class Setup003_GravitySwitch
{
    [MenuItem("Assets/Setup/003 GravitySwitch")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup003_GravitySwitch] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // --- Camera ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.14f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
        }

        // --- White sprite ---
        string whiteTexPath = "Assets/Scripts/Game003_GravitySwitch/WhiteSquare.png";
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

        // --- Sprites ---
        string spritePath = "Assets/Resources/Sprites/Game003_GravitySwitch/";
        var boardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "board_background.png");
        var floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "floor.png");
        var wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "wall.png");
        var ballSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "ball.png");
        var goalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "goal.png");

        // --- Board background ---
        var boardBgObj = new GameObject("BoardBackground");
        var boardBgSr = boardBgObj.AddComponent<SpriteRenderer>();
        boardBgSr.sprite = boardBgSprite != null ? boardBgSprite : whiteSprite;
        boardBgSr.color = boardBgSprite != null ? Color.white : new Color(0.06f, 0.08f, 0.14f);
        boardBgObj.transform.localScale = new Vector3(3.0f, 3.0f, 1f);
        boardBgSr.sortingOrder = -10;

        // --- Prefabs ---
        string prefabDir = "Assets/Scripts/Game003_GravitySwitch/";

        var floorPrefab = CreateSimplePrefab(prefabDir + "FloorPrefab.prefab", "FloorPrefab", floorSprite, whiteSprite, -5);
        var wallPrefab = CreateSimplePrefab(prefabDir + "WallPrefab.prefab", "WallPrefab", wallSprite, whiteSprite, 5);
        var goalPrefab = CreateSimplePrefab(prefabDir + "GoalPrefab.prefab", "GoalPrefab", goalSprite, whiteSprite, 3);

        // Ball prefab (with collider + BallController)
        var ballObjInScene = new GameObject("BallPrefab");
        var ballSr = ballObjInScene.AddComponent<SpriteRenderer>();
        ballSr.sprite = ballSprite != null ? ballSprite : whiteSprite;
        ballSr.color = Color.white;
        ballSr.sortingOrder = 10;
        ballObjInScene.AddComponent<BallController>();
        var ballPrefab = PrefabUtility.SaveAsPrefabAsset(ballObjInScene, prefabDir + "BallPrefab.prefab");
        Object.DestroyImmediate(ballObjInScene);

        // --- GameManager + GravityManager ---
        var gameManagerObj = new GameObject("GameManager");
        var gameManager = gameManagerObj.AddComponent<GravitySwitchGameManager>();

        var boardObj = new GameObject("GravityBoard");
        boardObj.transform.SetParent(gameManagerObj.transform);
        var gravityManager = boardObj.AddComponent<GravityManager>();

        var gmSO = new SerializedObject(gravityManager);
        gmSO.FindProperty("_gridWidth").intValue = 7;
        gmSO.FindProperty("_gridHeight").intValue = 7;
        gmSO.FindProperty("_cellSize").floatValue = 1.0f;
        gmSO.FindProperty("_ballPrefab").objectReferenceValue = ballPrefab;
        gmSO.FindProperty("_wallPrefab").objectReferenceValue = wallPrefab;
        gmSO.FindProperty("_goalPrefab").objectReferenceValue = goalPrefab;
        gmSO.FindProperty("_floorPrefab").objectReferenceValue = floorPrefab;
        gmSO.ApplyModifiedProperties();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Move count (top-left)
        var moveTextObj = CreateText(canvasObj.transform, "MoveCountText", "手数: 0", 32, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(250, 50), new Vector2(20, -20));

        // Stage text (top-center)
        var stageTextObj = CreateText(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 50), new Vector2(0, -20));
        stageTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Menu button (top-right)
        var menuBtnObj = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(240, 50), new Vector2(-20, -20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // Direction buttons (bottom area)
        var upBtn = CreateButton(canvasObj.transform, "UpButton", "↑", 36, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f),
            new Vector2(80, 80), new Vector2(0, 180),
            new Color(0.2f, 0.5f, 0.7f, 1f));
        var downBtn = CreateButton(canvasObj.transform, "DownButton", "↓", 36, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f),
            new Vector2(80, 80), new Vector2(0, 30),
            new Color(0.2f, 0.5f, 0.7f, 1f));
        var leftBtn = CreateButton(canvasObj.transform, "LeftButton", "←", 36, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f),
            new Vector2(80, 80), new Vector2(-100, 105),
            new Color(0.2f, 0.5f, 0.7f, 1f));
        var rightBtn = CreateButton(canvasObj.transform, "RightButton", "→", 36, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f),
            new Vector2(80, 80), new Vector2(100, 105),
            new Color(0.2f, 0.5f, 0.7f, 1f));

        // Clear panel
        var clearPanelObj = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanelObj.transform.SetParent(canvasObj.transform, false);
        clearPanelObj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
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
            new Vector2(0.3f, 0.15f), new Vector2(0.3f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero,
            new Color(0.1f, 0.5f, 0.3f, 1f));

        var nextStageBtnObj = CreateButton(clearPanelObj.transform, "NextStageButton", "次のステージ", 28, jpFont,
            new Vector2(0.7f, 0.15f), new Vector2(0.7f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 60), Vector2.zero,
            new Color(0.2f, 0.4f, 0.7f, 1f));

        clearPanelObj.SetActive(false);

        // --- GravitySwitchUI ---
        var uiObj = new GameObject("GravitySwitchUI");
        var gsUI = uiObj.AddComponent<GravitySwitchUI>();

        var uiSO = new SerializedObject(gsUI);
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanelObj;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextStageBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_upButton").objectReferenceValue = upBtn.GetComponent<Button>();
        uiSO.FindProperty("_downButton").objectReferenceValue = downBtn.GetComponent<Button>();
        uiSO.FindProperty("_leftButton").objectReferenceValue = leftBtn.GetComponent<Button>();
        uiSO.FindProperty("_rightButton").objectReferenceValue = rightBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gameManager;
        uiSO.ApplyModifiedProperties();

        // BackToMenuButton on top-right
        menuBtnObj.AddComponent<BackToMenuButton>();

        // GameManager references
        var gameManagerSO = new SerializedObject(gameManager);
        gameManagerSO.FindProperty("_gravityManager").objectReferenceValue = gravityManager;
        gameManagerSO.FindProperty("_ui").objectReferenceValue = gsUI;
        gameManagerSO.ApplyModifiedProperties();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        // Save scene
        string scenePath = "Assets/Scenes/003_GravitySwitch.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup003_GravitySwitch] GravitySwitch シーンを作成しました: " + scenePath);
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
        obj.AddComponent<Image>().color = bgColor;
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
