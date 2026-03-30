using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game011_FoldPaper;

/// <summary>
/// FoldPaper のゲームシーンを自動構成する Editor スクリプト。
/// Assets > Setup > 011 FoldPaper から実行する。
/// </summary>
public static class Setup011_FoldPaper
{
    [MenuItem("Assets/Setup/011 FoldPaper")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup011_FoldPaper] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 日本語フォント読み込み
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // --- カメラ設定 ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.12f, 0.15f, 0.22f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        // --- スプライト生成・読み込み ---
        string spriteDir = "Assets/Resources/Sprites/Game011_FoldPaper";
        EnsureSprites(spriteDir);

        var paperCellSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/paper_cell.png");
        var foldLineSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{spriteDir}/fold_line.png");

        // --- 背景 ---
        string whiteTexPath = "Assets/Scripts/Game011_FoldPaper/WhiteSquare.png";
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

        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = whiteSprite;
        bgSr.color = new Color(0.1f, 0.12f, 0.18f, 1f);
        bgObj.transform.localScale = new Vector3(12f, 12f, 1f);
        bgSr.sortingOrder = -10;

        // --- セルプレハブ ---
        var cellPrefab = CreateCellPrefab(paperCellSprite);

        // --- 折り線プレハブ ---
        var foldLinePrefab = CreateFoldLinePrefab(foldLineSprite);

        // --- GameManager + PaperManager ---
        var gameManagerObj = new GameObject("GameManager");
        var gameManager = gameManagerObj.AddComponent<FoldPaperGameManager>();

        var paperObj = new GameObject("PaperManager");
        paperObj.transform.SetParent(gameManagerObj.transform);
        var paperManager = paperObj.AddComponent<PaperManager>();

        // PaperManager の設定
        var pmSO = new SerializedObject(paperManager);
        pmSO.FindProperty("_cellPrefab").objectReferenceValue = cellPrefab;
        pmSO.FindProperty("_foldLinePrefab").objectReferenceValue = foldLinePrefab;
        pmSO.FindProperty("_cellSize").floatValue = 0.8f;
        pmSO.ApplyModifiedProperties();

        // --- Canvas (UI) ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 折り手数テキスト（左上）
        var foldCountTextObj = CreateText(canvasObj.transform, "FoldCountText", "折り: 0 回", 32, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(300, 50), new Vector2(20, -20));

        // ステージテキスト（中央上）
        var stageTextObj = CreateText(canvasObj.transform, "StageText", "ステージ 1", 36, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 50), new Vector2(0, -20));
        stageTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // メニューへ戻るボタン（右上）
        var menuBtnObj = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(240, 50), new Vector2(-20, -20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // Undoボタン（左下）
        var undoBtnObj = CreateButton(canvasObj.transform, "UndoButton", "一手戻す", 24, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(200, 50), new Vector2(20, 20),
            new Color(0.5f, 0.35f, 0.2f, 0.9f));

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

        // クリアテキスト
        var clearTextObj = CreateText(clearPanelObj.transform, "ClearText", "完成!", 48, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 100), Vector2.zero);
        clearTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // リスタートボタン（クリアパネル内）
        var restartBtnObj = CreateButton(clearPanelObj.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.2f, 0.2f), new Vector2(0.2f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(180, 60), Vector2.zero,
            new Color(0.1f, 0.5f, 0.3f, 1f));

        // 次のステージボタン（クリアパネル内）
        var nextStageBtnObj = CreateButton(clearPanelObj.transform, "NextStageButton", "次へ", 28, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(180, 60), Vector2.zero,
            new Color(0.2f, 0.4f, 0.6f, 1f));

        // メニューボタン（クリアパネル内）
        var clearMenuBtnObj = CreateButton(clearPanelObj.transform, "ClearMenuButton", "メニューへ", 28, jpFont,
            new Vector2(0.8f, 0.2f), new Vector2(0.8f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(180, 60), Vector2.zero,
            new Color(0.3f, 0.3f, 0.5f, 1f));

        clearPanelObj.SetActive(false);

        // --- FoldPaperUI ---
        var uiObj = new GameObject("FoldPaperUI");
        var foldPaperUI = uiObj.AddComponent<FoldPaperUI>();

        var uiSO = new SerializedObject(foldPaperUI);
        uiSO.FindProperty("_foldCountText").objectReferenceValue = foldCountTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanelObj;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextStageBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = clearMenuBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_undoButton").objectReferenceValue = undoBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gameManager;
        uiSO.ApplyModifiedProperties();

        // メニューへ戻るボタン（右上）にBackToMenuButtonを追加
        menuBtnObj.AddComponent<BackToMenuButton>();

        // --- GameManager の参照設定 ---
        var gmSO = new SerializedObject(gameManager);
        gmSO.FindProperty("_paperManager").objectReferenceValue = paperManager;
        gmSO.FindProperty("_ui").objectReferenceValue = foldPaperUI;
        gmSO.ApplyModifiedProperties();

        // --- EventSystem（新 Input System 対応）---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        // シーン保存
        string scenePath = "Assets/Scenes/011_FoldPaper.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup011_FoldPaper] FoldPaper シーンを作成しました: " + scenePath);
    }

    private static void EnsureSprites(string dir)
    {
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }

        // paper_cell スプライト
        string cellPath = $"{dir}/paper_cell.png";
        if (!System.IO.File.Exists(cellPath))
        {
            var tex = new Texture2D(64, 64);
            var pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 64;
                int y = i / 64;
                // 紙の質感: 少しクリーム色の正方形に薄い枠線
                bool border = x < 2 || x >= 62 || y < 2 || y >= 62;
                pixels[i] = border
                    ? new Color(0.75f, 0.7f, 0.6f, 1f)
                    : new Color(0.95f, 0.92f, 0.85f, 1f);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(cellPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(cellPath);
            SetSpriteImportSettings(cellPath, 64);
        }

        // fold_line スプライト
        string linePath = $"{dir}/fold_line.png";
        if (!System.IO.File.Exists(linePath))
        {
            var tex = new Texture2D(64, 64);
            var pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 64;
                int y = i / 64;
                // 破線パターンの折り線
                bool dash = (x / 8) % 2 == 0 || (y / 8) % 2 == 0;
                pixels[i] = dash
                    ? new Color(0.9f, 0.3f, 0.2f, 0.9f)
                    : new Color(0.9f, 0.3f, 0.2f, 0.4f);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(linePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(linePath);
            SetSpriteImportSettings(linePath, 64);
        }
    }

    private static void SetSpriteImportSettings(string path, int ppu)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
            importer.SaveAndReimport();
        }
    }

    private static GameObject CreateCellPrefab(Sprite sprite)
    {
        string prefabPath = "Assets/Scripts/Game011_FoldPaper/PaperCellPrefab.prefab";

        var obj = new GameObject("PaperCellPrefab");
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.95f, 0.92f, 0.85f, 1f);
        sr.sortingOrder = 1;

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        Object.DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreateFoldLinePrefab(Sprite sprite)
    {
        string prefabPath = "Assets/Scripts/Game011_FoldPaper/FoldLinePrefab.prefab";

        var obj = new GameObject("FoldLinePrefab");
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.9f, 0.3f, 0.2f, 0.8f);
        sr.sortingOrder = 50;

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

        // ボタンラベル
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
