using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game008_IcePath;

public static class Setup008_IcePath
{
    [MenuItem("Assets/Setup/008 IcePath")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup008_IcePath] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.1f, 0.15f, 0.25f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        string whiteTexPath = "Assets/Scripts/Game008_IcePath/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        {
            var wTex = new Texture2D(4, 4);
            var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white;
            wTex.SetPixels(px); wTex.Apply();
            System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG());
            Object.DestroyImmediate(wTex);
            AssetDatabase.ImportAsset(whiteTexPath);
            var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter;
            if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); }
        }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);

        string sp = "Assets/Resources/Sprites/Game008_IcePath/";
        var iceSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "ice_cell.png");
        var wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "wall_cell.png");
        var goalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "goal_cell.png");
        var playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "player.png");

        string pd = "Assets/Scripts/Game008_IcePath/";

        // Prefabs
        var icePrefab = CreateCellPrefab(pd + "IcePrefab.prefab", iceSprite, whiteSprite);
        var wallPrefab = CreateCellPrefab(pd + "WallPrefab.prefab", wallSprite, whiteSprite);
        var goalPrefab = CreateCellPrefab(pd + "GoalPrefab.prefab", goalSprite, whiteSprite);

        var playerObj = new GameObject("PlayerPrefab");
        var psr = playerObj.AddComponent<SpriteRenderer>();
        psr.sprite = playerSprite != null ? playerSprite : whiteSprite;
        psr.sortingOrder = 10;
        var playerPrefab = PrefabUtility.SaveAsPrefabAsset(playerObj, pd + "PlayerPrefab.prefab");
        Object.DestroyImmediate(playerObj);

        // GameManager + IcePathManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<IcePathGameManager>();
        var boardObj = new GameObject("IceBoard");
        boardObj.transform.SetParent(gmObj.transform);
        var im = boardObj.AddComponent<IcePathManager>();

        var imSO = new SerializedObject(im);
        imSO.FindProperty("_gridWidth").intValue = 5;
        imSO.FindProperty("_gridHeight").intValue = 5;
        imSO.FindProperty("_cellSize").floatValue = 1.0f;
        imSO.FindProperty("_icePrefab").objectReferenceValue = icePrefab;
        imSO.FindProperty("_wallPrefab").objectReferenceValue = wallPrefab;
        imSO.FindProperty("_goalPrefab").objectReferenceValue = goalPrefab;
        imSO.FindProperty("_playerPrefab").objectReferenceValue = playerPrefab;
        imSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        var moveText = CreateText(canvasObj.transform, "MoveCountText", "手数: 0", 32, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,50), new Vector2(20,-20));
        var progressText = CreateText(canvasObj.transform, "ProgressText", "", 28, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,40), new Vector2(20,-65));
        var stageText = CreateText(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont,
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,50), new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var menuBtn = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20),
            new Color(0.3f,0.3f,0.4f,0.9f));

        // Clear panel
        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0,0,0,0.8f);
        var cr = clearPanel.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.2f,0.2f); cr.anchorMax = new Vector2(0.8f,0.8f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearText = CreateText(clearPanel.transform, "ClearText", "クリア!", 48, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var restartBtn = CreateButton(clearPanel.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.3f,0.15f), new Vector2(0.3f,0.15f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero,
            new Color(0.1f,0.5f,0.3f,1f));
        var nextBtn = CreateButton(clearPanel.transform, "NextStageButton", "次のステージ", 28, jpFont,
            new Vector2(0.7f,0.15f), new Vector2(0.7f,0.15f), new Vector2(0.5f,0.5f), new Vector2(220,60), Vector2.zero,
            new Color(0.2f,0.4f,0.7f,1f));
        clearPanel.SetActive(false);

        // IcePathUI
        var uiObj = new GameObject("IcePathUI");
        var ipUI = uiObj.AddComponent<IcePathUI>();
        var uiSO = new SerializedObject(ipUI);
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_progressText").objectReferenceValue = progressText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_iceManager").objectReferenceValue = im;
        gmSO.FindProperty("_ui").objectReferenceValue = ipUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/008_IcePath.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup008_IcePath] IcePath シーンを作成しました: " + scenePath);
    }

    private static GameObject CreateCellPrefab(string path, Sprite sprite, Sprite fallback)
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(path);
        var obj = new GameObject(name);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : fallback;
        sr.sortingOrder = 5;
        obj.AddComponent<IceCell>();
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
