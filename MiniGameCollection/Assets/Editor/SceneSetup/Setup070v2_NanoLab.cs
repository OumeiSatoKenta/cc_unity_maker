using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game070v2_NanoLab;

public static class Setup070v2_NanoLab
{
    [MenuItem("Assets/Setup/070v2 NanoLab")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup070v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game070v2_NanoLab/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.07f, 0.16f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.047f, 0.047f, 1f);
        }

        // === GameManager hierarchy ===
        var gmObj = new GameObject("NanoLabGameManager");
        var gm = gmObj.AddComponent<NanoLabGameManager>();

        // StageManager (child of GameManager)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.3f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 0.6f },
            new StageManager.StageConfig { speedMultiplier = 3.0f, countMultiplier = 5, complexityFactor = 0.8f },
            new StageManager.StageConfig { speedMultiplier = 4.0f, countMultiplier = 8, complexityFactor = 1.0f },
        };
        SetField(sm, "_configs", stageConfigs);

        // NanoMachineManager (child of GameManager)
        var nmObj = new GameObject("NanoMachineManager");
        nmObj.transform.SetParent(gmObj.transform);
        var nm = nmObj.AddComponent<NanoMachineManager>();
        SetField(nm, "_gameManager", gm);

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- HUD Top ---
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 44, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(600, 60), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var eraText = CT(canvasObj.transform, "EraText", "現在: 原子時代", 40, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700, 55), new Vector2(0, -90));
        eraText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        eraText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.5f, 1f);

        var nanoCountText = CT(canvasObj.transform, "NanoCountText", "ナノマシン: 0", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(500, 55), new Vector2(10, -150));
        nanoCountText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.9f, 1f);

        var autoRateText = CT(canvasObj.transform, "AutoRateText", "自動: なし", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(400, 50), new Vector2(-10, -150));
        autoRateText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        autoRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);

        var prestigeMultText = CT(canvasObj.transform, "PrestigeMultText", "倍率: ×1.0", 34, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(350, 50), new Vector2(10, -205));
        prestigeMultText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "スコア: 0", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(400, 50), new Vector2(-10, -205));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 0.5f);

        // --- Tap Area (center) ---
        Sprite tapSprite = LoadSprite(sp + "tap_area.png");
        var tapBtn = CB(canvasObj.transform, "TapButton", "タップで増殖！", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(480, 480), new Vector2(0, 100),
            new Color(0.3f, 0.05f, 0.55f));
        if (tapSprite != null) tapBtn.GetComponent<Image>().sprite = tapSprite;
        tapBtn.GetComponent<Button>().onClick.AddListener(() => nm.OnTap());

        // --- Tech Scroll Panel ---
        var techScrollObj = new GameObject("TechScrollView", typeof(RectTransform));
        techScrollObj.transform.SetParent(canvasObj.transform, false);
        var techScrollRT = techScrollObj.GetComponent<RectTransform>();
        techScrollRT.anchorMin = new Vector2(0f, 0f);
        techScrollRT.anchorMax = new Vector2(1f, 0f);
        techScrollRT.pivot = new Vector2(0.5f, 0f);
        techScrollRT.sizeDelta = new Vector2(0, 380);
        techScrollRT.anchoredPosition = new Vector2(0, 170);

        var scrollRect = techScrollObj.AddComponent<ScrollRect>();
        var scrollImg = techScrollObj.AddComponent<Image>();
        scrollImg.color = new Color(0.05f, 0.03f, 0.12f, 0.9f);

        var contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(techScrollObj.transform, false);
        var contentRT = contentObj.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0, 600);
        contentRT.anchoredPosition = Vector2.zero;

        var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 8;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRT;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // Tech node button prefab (a simple template button)
        Sprite techBgSprite = LoadSprite(sp + "tech_node_bg.png");
        var techPrefab = new GameObject("TechNodePrefab", typeof(RectTransform));
        techPrefab.transform.SetParent(contentObj.transform, false);
        var prefabRT = techPrefab.GetComponent<RectTransform>();
        prefabRT.sizeDelta = new Vector2(0, 90);
        var prefabImg = techPrefab.AddComponent<Image>();
        if (techBgSprite != null) prefabImg.sprite = techBgSprite;
        prefabImg.color = new Color(0.4f, 0.2f, 0.6f, 1f);
        techPrefab.AddComponent<Button>();
        var layoutEl = techPrefab.AddComponent<LayoutElement>();
        layoutEl.preferredHeight = 90;
        layoutEl.flexibleWidth = 1;

        var techNameGo = CT(techPrefab.transform, "TechName", "技術名", 36, jpFont,
            new Vector2(0f, 0.6f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0, 0), new Vector2(10, -5));
        techNameGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var techDescGo = CT(techPrefab.transform, "TechDesc", "説明", 28, jpFont,
            new Vector2(0f, 0f), new Vector2(1f, 0.55f), new Vector2(0f, 0f), new Vector2(0, 0), new Vector2(10, 5));
        techDescGo.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);
        techPrefab.SetActive(false); // used as template only

        // --- Prestige Button ---
        var prestigeBtn = CB(canvasObj.transform, "PrestigeButton", "時代進化！", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(380, 80), new Vector2(0, 160),
            new Color(0.6f, 0.2f, 0f));
        prestigeBtn.GetComponent<Button>().onClick.AddListener(() => nm.DoPrestige());

        // --- Back to Menu Button ---
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニュー", 34, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(200, 70), new Vector2(10, 15),
            new Color(0.2f, 0.2f, 0.35f));
        backBtn.AddComponent<BackToMenuButton>();

        // --- Mutation Notification ---
        var mutNotif = new GameObject("MutationNotification", typeof(RectTransform));
        mutNotif.transform.SetParent(canvasObj.transform, false);
        var mutRT = mutNotif.GetComponent<RectTransform>();
        mutRT.anchorMin = new Vector2(0.1f, 0.5f);
        mutRT.anchorMax = new Vector2(0.9f, 0.5f);
        mutRT.sizeDelta = new Vector2(0, 80);
        mutRT.anchoredPosition = new Vector2(0, 150);
        var mutImg = mutNotif.AddComponent<Image>();
        mutImg.color = new Color(0.1f, 0.05f, 0.2f, 0.92f);

        var mutText = CT(mutNotif.transform, "MutationText", "", 36, jpFont,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        mutText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        mutNotif.SetActive(false);

        // --- Stage Clear Panel ---
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.1f, 0.3f); scRT.anchorMax = new Vector2(0.9f, 0.7f);
        scRT.offsetMin = scRT.offsetMax = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.05f, 0.03f, 0.15f, 0.97f);

        var scTitle = CT(scPanel.transform, "StageClearText", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(800, 100), Vector2.zero);
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.5f, 1f);

        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 44, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(400, 90), Vector2.zero,
            new Color(0.2f, 0.5f, 0.2f));

        scPanel.SetActive(false);

        // --- All Clear Panel ---
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.05f, 0.3f); acRT.anchorMax = new Vector2(0.95f, 0.7f);
        acRT.offsetMin = acRT.offsetMax = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.05f, 0.02f, 0.15f, 0.97f);

        var acTitleText = CT(acPanel.transform, "AllClearTitle", "全クリア！", 60, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(800, 100), Vector2.zero);
        acTitleText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitleText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0f);

        var acScore = CT(acPanel.transform, "AllClearScoreText", "最終スコア: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(800, 80), Vector2.zero);
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = Color.white;

        acPanel.SetActive(false);

        // --- Flash Overlay ---
        Sprite flashSprite = LoadSprite(sp + "flash_overlay.png");
        var flashObj = new GameObject("FlashOverlay", typeof(RectTransform));
        flashObj.transform.SetParent(canvasObj.transform, false);
        var flashRT = flashObj.GetComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero; flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = flashRT.offsetMax = Vector2.zero;
        var flashImg = flashObj.AddComponent<Image>();
        flashImg.color = new Color(0.8f, 0.9f, 1f, 0f);
        if (flashSprite != null) flashImg.sprite = flashSprite;
        flashObj.GetComponent<RectTransform>().SetAsLastSibling();
        flashObj.SetActive(false);

        // === InstructionPanel Canvas ===
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipBg = new GameObject("InstructionPanel", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvas.transform, false);
        var ipRT = ipBg.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one;
        ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;
        var ipImg = ipBg.AddComponent<Image>();
        ipImg.color = new Color(0.05f, 0.02f, 0.15f, 0.97f);
        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "NanoLab", 72, jpFont,
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f), new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.5f, 1f);

        var ipDesc = CT(ipBg.transform, "DescriptionText", "ナノマシンを増やして科学技術を進化させよう", 40, jpFont,
            new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "タップで増殖、ボタンで研究・時代進化", 36, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(900, 70), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var ipGoal = CT(ipBg.transform, "GoalText", "時代目標を達成してステージクリア", 36, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), new Vector2(900, 70), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 48, jpFont,
            new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.5f), new Vector2(380, 100), Vector2.zero,
            new Color(0.3f, 0.1f, 0.5f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 40, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(80, 80), new Vector2(-10, 90),
            new Color(0.2f, 0.1f, 0.35f));

        // === NanoLabUI ===
        var uiObj = new GameObject("NanoLabUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<NanoLabUI>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // === Wire Fields ===
        // NanoLabUI
        SetField(ui, "_nanoManager",         nm);
        SetField(ui, "_stageText",           stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nanoCountText",        nanoCountText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_eraText",              eraText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_autoRateText",         autoRateText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_prestigeMultText",     prestigeMultText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",            scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_techNodeContainer",    contentObj.transform);
        SetField(ui, "_techNodeButtonPrefab", techPrefab);
        SetField(ui, "_prestigeButton",       prestigeBtn.GetComponent<Button>());
        SetField(ui, "_prestigeButtonText",   prestigeBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",      scPanel);
        SetField(ui, "_stageClearText",       scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",        acPanel);
        SetField(ui, "_allClearScoreText",    acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_flashOverlay",         flashImg);
        SetField(ui, "_mutationNotification", mutNotif);
        SetField(ui, "_mutationText",         mutText.GetComponent<TextMeshProUGUI>());

        // NanoLabGameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_nanoManager",      nm);
        SetField(gm, "_ui",               ui);

        // InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Button events
        nextBtn.GetComponent<Button>().onClick.AddListener(() => {
            scPanel.SetActive(false);
            gm.NextStage();
        });

        // Save scene
        string scenePath = "Assets/Scenes/070v2_NanoLab.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup070v2] NanoLab シーン作成完了: " + scenePath);
    }

    static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null) { ti.textureType = TextureImporterType.Sprite; ti.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void SetField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
        else Debug.LogWarning($"[Setup070v2] Field not found: {fieldName} on {obj.GetType().Name}");
    }

    static GameObject CT(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        return go;
    }

    static GameObject CB(Transform parent, string name, string label, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var tRT = textGo.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
