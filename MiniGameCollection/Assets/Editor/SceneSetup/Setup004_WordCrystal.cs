using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game004_WordCrystal;

public static class Setup004_WordCrystal
{
    [MenuItem("Assets/Setup/004 WordCrystal")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup004_WordCrystal] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.12f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        // White sprite
        string whiteTexPath = "Assets/Scripts/Game004_WordCrystal/WhiteSquare.png";
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

        // Sprites
        string sp = "Assets/Resources/Sprites/Game004_WordCrystal/";
        var crystalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "crystal.png");
        var boardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(sp + "board_background.png");

        // Board background
        var boardBgObj = new GameObject("BoardBackground");
        var boardBgSr = boardBgObj.AddComponent<SpriteRenderer>();
        boardBgSr.sprite = boardBgSprite != null ? boardBgSprite : whiteSprite;
        boardBgSr.color = boardBgSprite != null ? Color.white : new Color(0.05f, 0.06f, 0.12f);
        boardBgObj.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
        boardBgSr.sortingOrder = -10;

        // Crystal prefab (with collider, CrystalController, TextMeshPro child)
        string prefabDir = "Assets/Scripts/Game004_WordCrystal/";
        var crystalObj = new GameObject("CrystalPrefab");
        var csr = crystalObj.AddComponent<SpriteRenderer>();
        csr.sprite = crystalSprite != null ? crystalSprite : whiteSprite;
        csr.color = Color.white;
        csr.sortingOrder = 5;
        var col = crystalObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);
        crystalObj.AddComponent<CrystalController>();

        // TextMeshPro child for letter display
        var letterObj = new GameObject("LetterText");
        letterObj.transform.SetParent(crystalObj.transform, false);
        var tmp3d = letterObj.AddComponent<TextMeshPro>();
        tmp3d.text = "";
        tmp3d.fontSize = 6;
        tmp3d.color = Color.white;
        tmp3d.alignment = TextAlignmentOptions.Center;
        tmp3d.sortingOrder = 10;
        var letterRect = letterObj.GetComponent<RectTransform>();
        letterRect.sizeDelta = new Vector2(1f, 1f);

        var crystalPrefab = PrefabUtility.SaveAsPrefabAsset(crystalObj, prefabDir + "CrystalPrefab.prefab");
        Object.DestroyImmediate(crystalObj);

        // GameManager + CrystalManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<WordCrystalGameManager>();

        var boardObj = new GameObject("CrystalBoard");
        boardObj.transform.SetParent(gmObj.transform);
        var cm = boardObj.AddComponent<CrystalManager>();

        var cmSO = new SerializedObject(cm);
        cmSO.FindProperty("_gridWidth").intValue = 7;
        cmSO.FindProperty("_gridHeight").intValue = 5;
        cmSO.FindProperty("_cellSize").floatValue = 1.0f;
        cmSO.FindProperty("_crystalPrefab").objectReferenceValue = crystalPrefab;
        cmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Word slots (top)
        var wordSlotsObj = CreateText(canvasObj.transform, "WordSlotsText", "_ _ _", 48, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(600, 80), new Vector2(0, -30));
        wordSlotsObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Miss count (top-left)
        var missTextObj = CreateText(canvasObj.transform, "MissCountText", "ミス: 0/3", 28, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(200, 40), new Vector2(20, -20));

        // Stage text (top-left below miss)
        var stageTextObj = CreateText(canvasObj.transform, "StageText", "ステージ 1", 28, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(200, 40), new Vector2(20, -60));

        // Menu button (top-right)
        var menuBtnObj = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(240, 50), new Vector2(-20, -20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // Clear panel
        var clearPanelObj = CreatePanel(canvasObj.transform, "ClearPanel");
        var clearTextObj = CreateText(clearPanelObj.transform, "ClearText", "全クリア!", 48, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 150), Vector2.zero);
        clearTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var restartBtnObj = CreateButton(clearPanelObj.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero,
            new Color(0.1f, 0.5f, 0.3f, 1f));
        clearPanelObj.SetActive(false);

        // GameOver panel
        var gameOverPanelObj = CreatePanel(canvasObj.transform, "GameOverPanel");
        var gameOverTextObj = CreateText(gameOverPanelObj.transform, "GameOverText", "ゲームオーバー", 48, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 150), Vector2.zero);
        gameOverTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtnObj = CreateButton(gameOverPanelObj.transform, "RetryButton", "リトライ", 28, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), Vector2.zero,
            new Color(0.6f, 0.2f, 0.2f, 1f));
        gameOverPanelObj.SetActive(false);

        // WordCrystalUI
        var uiObj = new GameObject("WordCrystalUI");
        var wcUI = uiObj.AddComponent<WordCrystalUI>();

        var uiSO = new SerializedObject(wcUI);
        uiSO.FindProperty("_wordSlotsText").objectReferenceValue = wordSlotsObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_missCountText").objectReferenceValue = missTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanelObj;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = gameOverPanelObj;
        uiSO.FindProperty("_gameOverText").objectReferenceValue = gameOverTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = retryBtnObj.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        menuBtnObj.AddComponent<BackToMenuButton>();

        // GameManager references
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_crystalManager").objectReferenceValue = cm;
        gmSO.FindProperty("_ui").objectReferenceValue = wcUI;
        gmSO.ApplyModifiedProperties();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/004_WordCrystal.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup004_WordCrystal] WordCrystal シーンを作成しました: " + scenePath);
    }

    private static GameObject CreatePanel(Transform parent, string name)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.2f, 0.2f);
        rect.anchorMax = new Vector2(0.8f, 0.8f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return obj;
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
