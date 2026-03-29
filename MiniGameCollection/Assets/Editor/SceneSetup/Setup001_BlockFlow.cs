using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game001_BlockFlow;

/// <summary>
/// BlockFlow のゲームシーンを自動構成する Editor スクリプト。
/// Assets > Setup > 001 BlockFlow から実行する。
/// </summary>
public static class Setup001_BlockFlow
{
    [MenuItem("Assets/Setup/001 BlockFlow")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup001_BlockFlow] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 日本語フォント読み込み
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // --- カメラ設定 ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.15f, 0.18f, 0.25f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        // --- 共用スプライト読み込み ---
        // 盤面背景（アセット画像）
        string boardBgPath = "Assets/Resources/Sprites/Game001_BlockFlow/board_background.png";
        var boardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(boardBgPath);

        // グリッド線用の白スプライト
        string whiteTexPath = "Assets/Scripts/Game001_BlockFlow/WhiteSquare.png";
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

        // --- 盤面背景 ---
        var boardBgObj = new GameObject("BoardBackground");
        var boardBgSr = boardBgObj.AddComponent<SpriteRenderer>();
        if (boardBgSprite != null)
        {
            boardBgSr.sprite = boardBgSprite;
            boardBgSr.color = Color.white;
            // board_background.png は 256x256, pixelsPerUnit=100 → 約2.56ワールド単位
            // 5x5盤面 * cellSize 1.2 = 6.0 → スケール約 2.4
            boardBgObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        }
        else
        {
            boardBgSr.sprite = whiteSprite;
            boardBgSr.color = new Color(0.12f, 0.14f, 0.2f, 1f);
            boardBgObj.transform.localScale = new Vector3(7.5f, 7.5f, 1f);
        }
        boardBgSr.sortingOrder = -10;

        // --- グリッド線 ---
        for (int i = 0; i <= 5; i++)
        {
            float pos = (i - 2.5f) * 1.2f;

            // 縦線
            var vLine = new GameObject($"VLine_{i}");
            var vSr = vLine.AddComponent<SpriteRenderer>();
            vSr.sprite = whiteSprite;
            vSr.color = new Color(0.25f, 0.28f, 0.35f, 0.5f);
            vLine.transform.position = new Vector3(pos + 0.6f, 0, 0);
            vLine.transform.localScale = new Vector3(0.03f, 7f, 1f);
            vSr.sortingOrder = -5;

            // 横線
            var hLine = new GameObject($"HLine_{i}");
            var hSr = hLine.AddComponent<SpriteRenderer>();
            hSr.sprite = whiteSprite;
            hSr.color = new Color(0.25f, 0.28f, 0.35f, 0.5f);
            hLine.transform.position = new Vector3(0, pos + 0.6f, 0);
            hLine.transform.localScale = new Vector3(7f, 0.03f, 1f);
            hSr.sortingOrder = -5;
        }

        // --- ブロックプレハブ ---
        var blockPrefab = CreateBlockPrefab();

        // --- GameManager + BoardManager ---
        var gameManagerObj = new GameObject("GameManager");
        var gameManager = gameManagerObj.AddComponent<BlockFlowGameManager>();

        var boardObj = new GameObject("Board");
        boardObj.transform.SetParent(gameManagerObj.transform);
        var boardManager = boardObj.AddComponent<BoardManager>();

        // BoardManager の設定
        var boardSO = new SerializedObject(boardManager);
        boardSO.FindProperty("_boardWidth").intValue = 5;
        boardSO.FindProperty("_boardHeight").intValue = 5;
        boardSO.FindProperty("_colorCount").intValue = 3;
        boardSO.FindProperty("_blocksPerColor").intValue = 3;
        boardSO.FindProperty("_blockPrefab").objectReferenceValue = blockPrefab;
        boardSO.FindProperty("_cellSize").floatValue = 1.2f;
        boardSO.ApplyModifiedProperties();

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

        // --- BlockFlowUI ---
        var uiObj = new GameObject("BlockFlowUI");
        var blockFlowUI = uiObj.AddComponent<BlockFlowUI>();

        var uiSO = new SerializedObject(blockFlowUI);
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveTextObj.GetComponent<TextMeshProUGUI>();
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
        gmSO.FindProperty("_boardManager").objectReferenceValue = boardManager;
        gmSO.FindProperty("_ui").objectReferenceValue = blockFlowUI;
        gmSO.ApplyModifiedProperties();

        // --- EventSystem（新 Input System 対応）---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        // シーン保存
        string scenePath = "Assets/Scenes/001_BlockFlow.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup001_BlockFlow] BlockFlow シーンを作成しました: " + scenePath);
    }

    private static GameObject CreateBlockPrefab()
    {
        // テクスチャをアセットとして保存（ランタイムスプライトはプレハブに保持できないため）
        string texPath = "Assets/Scripts/Game001_BlockFlow/BlockTexture.png";
        string prefabPath = "Assets/Scripts/Game001_BlockFlow/BlockPrefab.prefab";

        // 白い正方形テクスチャを生成して PNG で保存
        var texture = new Texture2D(64, 64);
        var pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        byte[] pngData = texture.EncodeToPNG();
        Object.DestroyImmediate(texture);
        System.IO.File.WriteAllBytes(texPath, pngData);
        AssetDatabase.ImportAsset(texPath);

        // テクスチャのインポート設定をスプライト用に変更
        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.SaveAndReimport();
        }

        // 保存したアセットからスプライトを読み込み
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        var obj = new GameObject("BlockPrefab");
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
        sr.sortingOrder = 10;

        var collider = obj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1f);
        obj.AddComponent<BlockController>();

        // プレハブとして保存
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
