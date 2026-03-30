using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using Game012_BridgeBuilder;

public static class Setup012_BridgeBuilder
{
    [MenuItem("Assets/Setup/012 BridgeBuilder")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup012] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // --- Camera ---
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.55f, 0.82f, 0.95f, 1f); // sky blue
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // --- Sprites ---
        EnsureSprite("Assets/Resources/Sprites/Game012_BridgeBuilder/plank.png", 64, 16,
            new Color(0.65f, 0.45f, 0.2f));
        EnsureSprite("Assets/Resources/Sprites/Game012_BridgeBuilder/support.png", 8, 48,
            new Color(0.5f, 0.5f, 0.55f));
        EnsureSprite("Assets/Resources/Sprites/Game012_BridgeBuilder/cliff.png", 128, 128,
            new Color(0.45f, 0.35f, 0.25f));
        EnsureSprite("Assets/Resources/Sprites/Game012_BridgeBuilder/car.png", 48, 24,
            new Color(0.85f, 0.2f, 0.2f));
        EnsureSprite("Assets/Resources/Sprites/Game012_BridgeBuilder/bg_sky.png", 256, 128,
            new Color(0.55f, 0.82f, 0.95f));

        var cliffSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Resources/Sprites/Game012_BridgeBuilder/cliff.png");
        var carSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Resources/Sprites/Game012_BridgeBuilder/car.png");

        // --- Cliffs (left and right) ---
        var leftCliff = new GameObject("LeftCliff");
        var leftSr = leftCliff.AddComponent<SpriteRenderer>();
        leftSr.sprite = cliffSprite;
        leftSr.sortingOrder = 2;
        leftCliff.transform.position = new Vector3(-5.5f, -3f, 0f);
        leftCliff.transform.localScale = new Vector3(4f, 4f, 1f);

        var rightCliff = new GameObject("RightCliff");
        var rightSr = rightCliff.AddComponent<SpriteRenderer>();
        rightSr.sprite = cliffSprite;
        rightSr.sortingOrder = 2;
        rightCliff.transform.position = new Vector3(5.5f, -3f, 0f);
        rightCliff.transform.localScale = new Vector3(4f, 4f, 1f);

        // --- Gap indicator ---
        var gapBg = new GameObject("GapBackground");
        var gapSr = gapBg.AddComponent<SpriteRenderer>();
        gapSr.sprite = cliffSprite;
        gapSr.color = new Color(0.2f, 0.35f, 0.55f, 0.3f);
        gapSr.sortingOrder = 1;
        gapBg.transform.position = new Vector3(0f, -2f, 0f);
        gapBg.transform.localScale = new Vector3(6f, 3f, 1f);

        // --- Car ---
        var carObj = new GameObject("Car");
        var carSr = carObj.AddComponent<SpriteRenderer>();
        carSr.sprite = carSprite;
        carSr.sortingOrder = 10;
        carObj.transform.position = new Vector3(-4.5f, -0.5f, 0f);
        carObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        carObj.SetActive(false);

        // --- Bridge Parent ---
        var bridgeParent = new GameObject("BridgeParent");

        // --- GameManager ---
        var gmObj = new GameObject("GameManager");
        var gameManager = gmObj.AddComponent<BridgeBuilderGameManager>();
        var bridgeManager = gmObj.AddComponent<BridgeManager>();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- EventSystem ---
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- UI Elements ---
        // Level text
        var levelTextObj = CreateUIText(canvasObj.transform, "LevelText",
            new Vector2(0, 900), new Vector2(400, 60), "Level 1 / 3", 32,
            TextAnchor.MiddleCenter, Color.white);
        var levelText = levelTextObj.GetComponent<Text>();

        // Budget text
        var budgetTextObj = CreateUIText(canvasObj.transform, "BudgetText",
            new Vector2(0, 840), new Vector2(300, 50), "Parts: 5", 28,
            TextAnchor.MiddleCenter, Color.white);
        var budgetText = budgetTextObj.GetComponent<Text>();

        // Build buttons parent
        var buildBtnsObj = new GameObject("BuildButtons");
        buildBtnsObj.transform.SetParent(canvasObj.transform, false);
        var buildBtnsRt = buildBtnsObj.AddComponent<RectTransform>();
        buildBtnsRt.anchoredPosition = new Vector2(0, -750);
        buildBtnsRt.sizeDelta = new Vector2(800, 80);

        // Plank button
        var plankBtn = CreateUIButton(buildBtnsObj.transform, "PlankBtn",
            new Vector2(-200, 0), new Vector2(180, 70), "Plank",
            new Color(0.65f, 0.45f, 0.2f));

        // Support button
        var supportBtn = CreateUIButton(buildBtnsObj.transform, "SupportBtn",
            new Vector2(0, 0), new Vector2(180, 70), "Support",
            new Color(0.5f, 0.5f, 0.55f));

        // Undo button
        var undoBtn = CreateUIButton(buildBtnsObj.transform, "UndoBtn",
            new Vector2(200, 0), new Vector2(180, 70), "Undo",
            new Color(0.7f, 0.3f, 0.3f));

        // Test button
        var testBtn = CreateUIButton(canvasObj.transform, "TestBtn",
            new Vector2(0, -850), new Vector2(300, 80), "TEST",
            new Color(0.2f, 0.7f, 0.3f));

        // Clear panel
        var clearPanel = new GameObject("ClearPanel");
        clearPanel.transform.SetParent(canvasObj.transform, false);
        var clearRt = clearPanel.AddComponent<RectTransform>();
        clearRt.anchoredPosition = Vector2.zero;
        clearRt.sizeDelta = new Vector2(600, 400);
        var clearImg = clearPanel.AddComponent<Image>();
        clearImg.color = new Color(0f, 0f, 0f, 0.85f);

        CreateUIText(clearPanel.transform, "ClearText",
            new Vector2(0, 100), new Vector2(400, 80), "CLEAR!", 48,
            TextAnchor.MiddleCenter, new Color(0.2f, 0.9f, 0.3f));

        var nextBtn = CreateUIButton(clearPanel.transform, "NextBtn",
            new Vector2(0, -20), new Vector2(250, 70), "Next Level",
            new Color(0.2f, 0.5f, 0.9f));

        var menuBtn = CreateUIButton(clearPanel.transform, "MenuBtn",
            new Vector2(0, -110), new Vector2(250, 70), "Menu",
            new Color(0.5f, 0.5f, 0.5f));

        clearPanel.SetActive(false);

        // --- Wire up UI component ---
        var uiComp = canvasObj.AddComponent<BridgeBuilderUI>();

        // Use SerializedObject to set references
        var so = new SerializedObject(uiComp);
        so.FindProperty("_levelText").objectReferenceValue = levelText;
        so.FindProperty("_budgetText").objectReferenceValue = budgetText;
        so.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        so.FindProperty("_buildButtons").objectReferenceValue = buildBtnsObj;
        so.FindProperty("_testButton").objectReferenceValue = testBtn.GetComponent<Button>();
        so.FindProperty("_gameManager").objectReferenceValue = gameManager;
        so.FindProperty("_bridgeManager").objectReferenceValue = bridgeManager;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Wire up GameManager
        var gmSo = new SerializedObject(gameManager);
        gmSo.FindProperty("_bridgeManager").objectReferenceValue = bridgeManager;
        gmSo.FindProperty("_ui").objectReferenceValue = uiComp;
        gmSo.ApplyModifiedPropertiesWithoutUndo();

        // Wire up BridgeManager
        var bmSo = new SerializedObject(bridgeManager);
        bmSo.FindProperty("_gameManager").objectReferenceValue = gameManager;
        bmSo.FindProperty("_ui").objectReferenceValue = uiComp;
        bmSo.FindProperty("_bridgeParent").objectReferenceValue = bridgeParent.transform;
        bmSo.FindProperty("_carTransform").objectReferenceValue = carObj.transform;
        bmSo.FindProperty("_carRenderer").objectReferenceValue = carSr;
        bmSo.ApplyModifiedPropertiesWithoutUndo();

        // Wire up button events
        WireButton(plankBtn, uiComp, "OnPlankSelected");
        WireButton(supportBtn, uiComp, "OnSupportSelected");
        WireButton(undoBtn, uiComp, "OnUndoClicked");
        WireButton(testBtn, uiComp, "OnTestClicked");
        WireButton(nextBtn, uiComp, "OnNextLevelClicked");
        WireButton(menuBtn, uiComp, "OnMenuClicked");

        // --- Save ---
        string scenePath = "Assets/Scenes/012_BridgeBuilder.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup012] BridgeBuilder scene created at " + scenePath);
    }

    private static void EnsureSprite(string path, int w, int h, Color color)
    {
        string dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        if (!System.IO.File.Exists(path))
        {
            var tex = new Texture2D(w, h);
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.SaveAndReimport();
        }
    }

    private static GameObject CreateUIText(Transform parent, string name,
        Vector2 pos, Vector2 size, string text, int fontSize,
        TextAnchor alignment, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var t = obj.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = color;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return obj;
    }

    private static GameObject CreateUIButton(Transform parent, string name,
        Vector2 pos, Vector2 size, string label, Color bgColor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = obj.AddComponent<Image>();
        img.color = bgColor;
        obj.AddComponent<Button>();

        CreateUIText(obj.transform, "Label", Vector2.zero, size, label, 24,
            TextAnchor.MiddleCenter, Color.white);

        return obj;
    }

    private static void WireButton(GameObject btnObj, Object target, string method)
    {
        var btn = btnObj.GetComponent<Button>();
        if (btn == null) return;
        var action = new UnityEngine.Events.UnityAction(() => { });
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick,
            System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, method)
                as UnityEngine.Events.UnityAction);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
