using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game013_SymmetryDraw;

/// <summary>
/// SymmetryDraw のゲームシーンを自動構成する Editor スクリプト。
/// Assets > Setup > 013 SymmetryDraw から実行する。
/// </summary>
public static class Setup013_SymmetryDraw
{
    [MenuItem("Assets/Setup/013 SymmetryDraw")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup013_SymmetryDraw] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 日本語フォント読み込み
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // --- カメラ設定 ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.18f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
        }

        // --- 共用スプライト作成 ---
        string whiteTexPath = "Assets/Scripts/Game013_SymmetryDraw/WhiteSquare.png";
        EnsureWhiteTexture(whiteTexPath);
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);

        // --- 背景 ---
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = whiteSprite;
        bgSr.color = new Color(0.1f, 0.12f, 0.2f, 1f);
        bgObj.transform.localScale = new Vector3(12f, 10f, 1f);
        bgSr.sortingOrder = -20;

        // --- セルプレハブ作成 ---
        var cellPrefab = CreateCellPrefab(whiteTexPath);

        // --- お手本セルプレハブ作成 ---
        var targetCellPrefab = CreateTargetCellPrefab(whiteTexPath);

        // --- 対称線 ---
        var symmetryLineObj = new GameObject("SymmetryLine");
        var symLineSr = symmetryLineObj.AddComponent<SpriteRenderer>();
        symLineSr.sprite = whiteSprite;
        symLineSr.color = new Color(1f, 0.8f, 0.2f, 0.6f);
        symmetryLineObj.transform.position = new Vector3(0f, 0f, -0.1f);
        symmetryLineObj.transform.localScale = new Vector3(0.04f, 6f, 1f);
        symLineSr.sortingOrder = 5;

        // --- GameManager ---
        var gameManagerObj = new GameObject("GameManager");
        var gameManager = gameManagerObj.AddComponent<SymmetryDrawGameManager>();

        // --- StageData ---
        var stageDataObj = new GameObject("StageData");
        stageDataObj.transform.SetParent(gameManagerObj.transform);
        var stageData = stageDataObj.AddComponent<StageData>();

        // --- CanvasDrawManager ---
        var canvasObj = new GameObject("CanvasDrawManager");
        canvasObj.transform.SetParent(gameManagerObj.transform);
        var canvasDrawManager = canvasObj.AddComponent<CanvasDrawManager>();

        // CanvasDrawManager の設定
        var cdmSO = new SerializedObject(canvasDrawManager);
        cdmSO.FindProperty("_cellPrefab").objectReferenceValue = cellPrefab;
        cdmSO.FindProperty("_targetCellPrefab").objectReferenceValue = targetCellPrefab;
        cdmSO.FindProperty("_cellSize").floatValue = 0.5f;
        cdmSO.FindProperty("_symmetryLine").objectReferenceValue = symLineSr;
        cdmSO.ApplyModifiedProperties();

        // --- UI Canvas ---
        var uiCanvasObj = new GameObject("Canvas");
        var canvas = uiCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = uiCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        uiCanvasObj.AddComponent<GraphicRaycaster>();

        // ストローク数テキスト（左上）
        var strokeTextObj = CreateText(uiCanvasObj.transform, "StrokeCountText", "ストローク: 0", 32, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(300, 50), new Vector2(20, -20));

        // メニューへ戻るボタン（右上）
        var menuBtnObj = CreateButton(uiCanvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(240, 50), new Vector2(-20, -20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // クリアパネル（中央）
        var clearPanelObj = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanelObj.transform.SetParent(uiCanvasObj.transform, false);
        var clearBg = clearPanelObj.AddComponent<Image>();
        clearBg.color = new Color(0f, 0f, 0f, 0.8f);
        var clearRect = clearPanelObj.GetComponent<RectTransform>();
        clearRect.anchorMin = new Vector2(0.2f, 0.2f);
        clearRect.anchorMax = new Vector2(0.8f, 0.8f);
        clearRect.offsetMin = Vector2.zero;
        clearRect.offsetMax = Vector2.zero;

        // クリアテキスト
        var clearTextObj = CreateText(clearPanelObj.transform, "ClearText", "クリア!", 48, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 100), Vector2.zero);
        clearTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // リスタートボタン（クリアパネル内）
        var restartBtnObj = CreateButton(clearPanelObj.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.3f, 0.2f), new Vector2(0.3f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero,
            new Color(0.1f, 0.5f, 0.3f, 1f));

        // メニューボタン（クリアパネル内）
        var clearMenuBtnObj = CreateButton(clearPanelObj.transform, "ClearMenuButton", "メニューへ", 28, jpFont,
            new Vector2(0.7f, 0.2f), new Vector2(0.7f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero,
            new Color(0.3f, 0.3f, 0.5f, 1f));

        clearPanelObj.SetActive(false);

        // --- SymmetryDrawUI ---
        var uiObj = new GameObject("SymmetryDrawUI");
        var symmetryDrawUI = uiObj.AddComponent<SymmetryDrawUI>();

        var uiSO = new SerializedObject(symmetryDrawUI);
        uiSO.FindProperty("_strokeCountText").objectReferenceValue = strokeTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanelObj;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = clearMenuBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gameManager;
        uiSO.ApplyModifiedProperties();

        // メニューへ戻るボタン（右上）にBackToMenuButtonを追加
        menuBtnObj.AddComponent<BackToMenuButton>();

        // --- GameManager の参照設定 ---
        var gmSO = new SerializedObject(gameManager);
        gmSO.FindProperty("_canvasDrawManager").objectReferenceValue = canvasDrawManager;
        gmSO.FindProperty("_ui").objectReferenceValue = symmetryDrawUI;
        gmSO.FindProperty("_stageData").objectReferenceValue = stageData;
        gmSO.ApplyModifiedProperties();

        // --- EventSystem（新 Input System 対応）---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        // シーン保存
        string scenePath = "Assets/Scenes/013_SymmetryDraw.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup013_SymmetryDraw] SymmetryDraw シーンを作成しました: " + scenePath);
    }

    private static void EnsureWhiteTexture(string texPath)
    {
        if (!System.IO.File.Exists(texPath))
        {
            string dir = System.IO.Path.GetDirectoryName(texPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            var wTex = new Texture2D(4, 4);
            var wPixels = new Color[16];
            for (int i = 0; i < 16; i++) wPixels[i] = Color.white;
            wTex.SetPixels(wPixels);
            wTex.Apply();
            System.IO.File.WriteAllBytes(texPath, wTex.EncodeToPNG());
            Object.DestroyImmediate(wTex);
            AssetDatabase.ImportAsset(texPath);
            var wImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (wImporter != null)
            {
                wImporter.textureType = TextureImporterType.Sprite;
                wImporter.spritePixelsPerUnit = 1;
                wImporter.SaveAndReimport();
            }
        }
    }

    private static GameObject CreateCellPrefab(string whiteTexPath)
    {
        string texPath = "Assets/Scripts/Game013_SymmetryDraw/CellTexture.png";
        string prefabPath = "Assets/Scripts/Game013_SymmetryDraw/CellPrefab.prefab";

        // 白い正方形テクスチャを生成して PNG で保存
        var texture = new Texture2D(32, 32);
        var pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        System.IO.File.WriteAllBytes(texPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(texPath);

        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        var obj = new GameObject("CellPrefab");
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.2f, 0.22f, 0.28f, 0.3f);
        sr.sortingOrder = 1;

        var collider = obj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1f);
        obj.AddComponent<CellView>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        Object.DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreateTargetCellPrefab(string whiteTexPath)
    {
        string texPath = "Assets/Scripts/Game013_SymmetryDraw/TargetCellTexture.png";
        string prefabPath = "Assets/Scripts/Game013_SymmetryDraw/TargetCellPrefab.prefab";

        // お手本セル用テクスチャ
        var texture = new Texture2D(32, 32);
        var pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        System.IO.File.WriteAllBytes(texPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(texPath);

        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        var obj = new GameObject("TargetCellPrefab");
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.9f, 0.5f, 0.2f, 0.25f);
        sr.sortingOrder = 0;

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
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
