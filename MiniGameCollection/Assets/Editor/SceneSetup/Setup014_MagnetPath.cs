using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game014_MagnetPath;

public static class Setup014_MagnetPath
{
    [MenuItem("Assets/Setup/014 MagnetPath")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup014_MagnetPath] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.06f, 0.07f, 0.14f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        string whiteTexPath = "Assets/Scripts/Game014_MagnetPath/WhiteSquare.png";
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

        string sp = "Assets/Resources/Sprites/Game014_MagnetPath/";
        var cellBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "cell_bg.png");
        var magnetNSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "magnet_n.png");
        var wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "wall.png");
        var goalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "goal.png");
        var ballSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "ball.png");

        string pd = "Assets/Scripts/Game014_MagnetPath/";

        // Prefabs
        var magnetObj = new GameObject("MagnetPrefab");
        var msr = magnetObj.AddComponent<SpriteRenderer>();
        msr.sprite = magnetNSprite != null ? magnetNSprite : whiteSprite;
        msr.sortingOrder = 8;
        magnetObj.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
        magnetObj.AddComponent<MagnetController>();
        var magnetPrefab = PrefabUtility.SaveAsPrefabAsset(magnetObj, pd + "MagnetPrefab.prefab");
        Object.DestroyImmediate(magnetObj);

        var wallPrefab = CreateSimplePrefab(pd + "WallPrefab.prefab", wallSprite, whiteSprite, 5);
        var goalPrefab = CreateSimplePrefab(pd + "GoalPrefab.prefab", goalSprite, whiteSprite, 3);
        var cellBgPrefab = CreateSimplePrefab(pd + "CellBgPrefab.prefab", cellBgSprite, whiteSprite, -5);

        var ballObj = new GameObject("BallPrefab");
        var bsr = ballObj.AddComponent<SpriteRenderer>();
        bsr.sprite = ballSprite != null ? ballSprite : whiteSprite;
        bsr.sortingOrder = 10;
        var ballPrefab = PrefabUtility.SaveAsPrefabAsset(ballObj, pd + "BallPrefab.prefab");
        Object.DestroyImmediate(ballObj);

        // GameManager + MagnetManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<MagnetPathGameManager>();
        var boardObj = new GameObject("MagnetBoard");
        boardObj.transform.SetParent(gmObj.transform);
        var mm = boardObj.AddComponent<MagnetManager>();

        var mmSO = new SerializedObject(mm);
        mmSO.FindProperty("_gridWidth").intValue = 5;
        mmSO.FindProperty("_gridHeight").intValue = 5;
        mmSO.FindProperty("_cellSize").floatValue = 1.0f;
        mmSO.FindProperty("_magnetPrefab").objectReferenceValue = magnetPrefab;
        mmSO.FindProperty("_wallPrefab").objectReferenceValue = wallPrefab;
        mmSO.FindProperty("_goalPrefab").objectReferenceValue = goalPrefab;
        mmSO.FindProperty("_ballPrefab").objectReferenceValue = ballPrefab;
        mmSO.FindProperty("_cellBgPrefab").objectReferenceValue = cellBgPrefab;
        mmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        var moveText = CT(canvasObj.transform, "MoveCountText", "手数: 0", 32, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(200,50), new Vector2(20,-20));
        var stageText = CT(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont,
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,50), new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var hint = CT(canvasObj.transform, "HintText", "磁石をタップでN/S切替。鉄球をゴールへ導こう", 20, jpFont,
            new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(600,40), new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        hint.GetComponent<TextMeshProUGUI>().color = new Color(0.6f,0.6f,0.7f);
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20),
            new Color(0.3f,0.3f,0.4f,0.9f));

        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0,0,0,0.8f);
        var cr = clearPanel.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.2f,0.2f); cr.anchorMax = new Vector2(0.8f,0.8f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearText = CT(clearPanel.transform, "ClearText", "クリア!", 48, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var restartBtn = CB(clearPanel.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.3f,0.15f), new Vector2(0.3f,0.15f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero,
            new Color(0.1f,0.5f,0.3f,1f));
        var nextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージ", 28, jpFont,
            new Vector2(0.7f,0.15f), new Vector2(0.7f,0.15f), new Vector2(0.5f,0.5f), new Vector2(220,60), Vector2.zero,
            new Color(0.2f,0.4f,0.7f,1f));
        clearPanel.SetActive(false);

        var uiObj = new GameObject("MagnetPathUI");
        var mpUI = uiObj.AddComponent<MagnetPathUI>();
        var uiSO = new SerializedObject(mpUI);
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_magnetManager").objectReferenceValue = mm;
        gmSO.FindProperty("_ui").objectReferenceValue = mpUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/014_MagnetPath.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup014_MagnetPath] MagnetPath シーンを作成しました: " + scenePath);
    }

    private static GameObject CreateSimplePrefab(string path, Sprite sprite, Sprite fallback, int order)
    {
        var obj = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : fallback;
        sr.sortingOrder = order;
        var p = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        return p;
    }

    private static GameObject CT(Transform parent, string name, string text, float fontSize,
        TMP_FontAsset font, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = Color.white;
        if (font != null) tmp.font = font;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        return obj;
    }

    private static GameObject CB(Transform parent, string name, string label, float fontSize,
        TMP_FontAsset font, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = bg;
        obj.AddComponent<Button>();
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var t = new GameObject("Text", typeof(RectTransform));
        t.transform.SetParent(obj.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        var tr = t.GetComponent<RectTransform>();
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
