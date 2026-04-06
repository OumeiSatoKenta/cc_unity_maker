using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game063v2_StarMiner;

public static class Setup063v2_StarMiner
{
    [MenuItem("Assets/Setup/063v2 StarMiner")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup063v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game063v2_StarMiner/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.02f, 0.02f, 0.1f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "Background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            // fit to screen
            bgObj.transform.localScale = new Vector3(0.012f, 0.012f, 1f);
        }

        // Star object (center, y=1.5)
        Sprite[] starSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
            starSprites[i] = LoadSprite(sp + $"Star{i + 1}.png");

        var starObj = new GameObject("Star");
        var starSR = starObj.AddComponent<SpriteRenderer>();
        if (starSprites[0] != null) starSR.sprite = starSprites[0];
        starSR.sortingOrder = 1;
        starObj.transform.position = new Vector3(0f, 1.5f, 0f);
        starObj.transform.localScale = new Vector3(0.016f, 0.016f, 1f);
        var starCol = starObj.AddComponent<CircleCollider2D>();
        starCol.radius = 64f; // in sprite pixels (scaled → ~2u radius)

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<StarMinerGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // Configure StageManager stages
        var stages = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.15f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 0.2f },
            new StageManager.StageConfig { speedMultiplier = 3.0f, countMultiplier = 5, complexityFactor = 0.25f },
        };
        sm.SetConfigs(stages);

        // MiningManager
        var mmObj = new GameObject("MiningManager");
        mmObj.transform.SetParent(gmObj.transform);
        var mm = mmObj.AddComponent<MiningManager>();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD: Stage (top center)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 60), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        // HUD: Ore (top left)
        var oreText = CT(canvasObj.transform, "OreText", "鉱石: 0 / 100", 34, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(400, 55), new Vector2(15, -90));
        oreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        oreText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);

        // HUD: Funds (top right)
        var fundText = CT(canvasObj.transform, "FundText", "資金: 0G", 34, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(350, 55), new Vector2(-15, -90));
        fundText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        fundText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // Drill level text (left)
        var drillText = CT(canvasObj.transform, "DrillLevelText", "ドリル Lv.1", 30, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(300, 50), new Vector2(15, -145));
        drillText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        drillText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.3f);

        // Drone count text (right)
        var droneText = CT(canvasObj.transform, "DroneCountText", "ドローン: 0機", 30, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(300, 50), new Vector2(-15, -145));
        droneText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        droneText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.8f, 1f);

        // Auto rate text (center)
        var autoRateText = CT(canvasObj.transform, "AutoRateText", "自動採掘: なし", 28, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 50), new Vector2(0, -200));
        autoRateText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        autoRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 1f);

        // Combo text (center, hidden)
        var comboText = CT(canvasObj.transform, "ComboText", "Combo x2!", 44, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 65), new Vector2(0, -260));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.1f);
        comboText.SetActive(false);

        // Storm Warning Panel
        var stormPanel = new GameObject("StormWarning", typeof(RectTransform));
        stormPanel.transform.SetParent(canvasObj.transform, false);
        var swRT = stormPanel.GetComponent<RectTransform>();
        swRT.anchorMin = new Vector2(0.5f, 1); swRT.anchorMax = new Vector2(0.5f, 1);
        swRT.pivot = new Vector2(0.5f, 1);
        swRT.sizeDelta = new Vector2(600, 65);
        swRT.anchoredPosition = new Vector2(0, -330);
        var swImg = stormPanel.AddComponent<Image>();
        swImg.color = new Color(0.5f, 0.1f, 0.1f, 0.9f);
        var swText = CT(stormPanel.transform, "StormText", "☄ 小惑星嵐！採掘量半減！", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(580, 55), Vector2.zero);
        swText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        swText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        stormPanel.SetActive(false);

        // === Bottom upgrade buttons (横並び) ===
        // Drill upgrade button (left)
        var drillBtn = CB(canvasObj.transform, "DrillUpgradeButton", "ドリル強化\n10G", 26, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(220, 90), new Vector2(20, 350),
            new Color(0.7f, 0.4f, 0.1f));

        // Drone button (center, hidden initially)
        var droneBtn = CB(canvasObj.transform, "DroneButton", "ドローン解放\n50G", 24, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(220, 90), new Vector2(0, 350),
            new Color(0.1f, 0.4f, 0.7f));
        droneBtn.SetActive(false);

        // Legendary button (right, hidden initially)
        var legendaryBtn = CB(canvasObj.transform, "LegendaryButton", "伝説採掘\n100G", 24, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(220, 90), new Vector2(-20, 350),
            new Color(0.5f, 0.1f, 0.7f));
        legendaryBtn.SetActive(false);

        // Sell button (center bottom)
        var sellBtn = CB(canvasObj.transform, "SellButton", "鉱石を売却", 30, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(300, 75), new Vector2(0, 255),
            new Color(0.2f, 0.6f, 0.3f));

        // Back to menu button (bottom left)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 28, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(180, 65), new Vector2(20, 175),
            new Color(0.3f, 0.3f, 0.5f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Stage Clear Panel
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scpRT = scPanel.GetComponent<RectTransform>();
        scpRT.anchorMin = new Vector2(0.1f, 0.3f); scpRT.anchorMax = new Vector2(0.9f, 0.7f);
        scpRT.offsetMin = scpRT.offsetMax = Vector2.zero;
        var scpImg = scPanel.AddComponent<Image>();
        scpImg.color = new Color(0.05f, 0.1f, 0.3f, 0.95f);
        var scText = CT(scPanel.transform, "StageClearText", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(700, 80), Vector2.zero);
        scText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var nextStageBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 36, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(400, 80), Vector2.zero,
            new Color(0.2f, 0.3f, 0.8f));
        scPanel.SetActive(false);

        // All Clear Panel
        var gcPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        gcPanel.transform.SetParent(canvasObj.transform, false);
        var gcpRT = gcPanel.GetComponent<RectTransform>();
        gcpRT.anchorMin = new Vector2(0.1f, 0.25f); gcpRT.anchorMax = new Vector2(0.9f, 0.75f);
        gcpRT.offsetMin = gcpRT.offsetMax = Vector2.zero;
        var gcpImg = gcPanel.AddComponent<Image>();
        gcpImg.color = new Color(0.1f, 0.05f, 0.3f, 0.97f);
        var gcText = CT(gcPanel.transform, "AllClearText", "StarMiner 完全制覇！", 46, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(800, 100), Vector2.zero);
        gcText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var gcMenuBtn = CB(gcPanel.transform, "MenuButton2", "メニューへ", 34, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(320, 75), Vector2.zero,
            new Color(0.3f, 0.2f, 0.6f));
        gcMenuBtn.AddComponent<BackToMenuButton>();
        gcPanel.SetActive(false);

        // --- InstructionPanel ---
        var ipCanvas = new GameObject("InstructionPanelCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        ipCanvas.AddComponent<GraphicRaycaster>();
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);

        var ipBg = new GameObject("InstructionPanel", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvas.transform, false);
        var ipBgRT = ipBg.GetComponent<RectTransform>();
        ipBgRT.anchorMin = Vector2.zero; ipBgRT.anchorMax = Vector2.one;
        ipBgRT.offsetMin = ipBgRT.offsetMax = Vector2.zero;
        var ipBgImg = ipBg.AddComponent<Image>();
        ipBgImg.color = new Color(0f, 0f, 0f, 0.88f);
        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "StarMiner", 64, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(900, 90), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 1f);

        var ipDesc = CT(ipBg.transform, "DescriptionText", "宇宙で鉱石を掘って宇宙船を強化しよう", 38, jpFont,
            new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "タップで採掘、ボタンでアップグレード", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.7f);

        var ipGoal = CT(ipBg.transform, "GoalText", "採掘目標を達成して新しい星系を開拓しよう", 34, jpFont,
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 44, jpFont,
            new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.5f), new Vector2(380, 90), Vector2.zero,
            new Color(0.2f, 0.4f, 0.8f));

        // Help button (bottom right of main canvas)
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(80, 80), new Vector2(-20, 175),
            new Color(0.2f, 0.3f, 0.5f));

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- UI script ---
        var uiObj = new GameObject("StarMinerUI");
        var ui = uiObj.AddComponent<StarMinerUI>();

        // Wire StarMinerUI fields
        SetField(ui, "_stageText", stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_oreText", oreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_fundText", fundText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_drillLevelText", drillText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_droneCountText", droneText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_autoRateText", autoRateText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText", comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stormWarningPanel", stormPanel);
        SetField(ui, "_drillUpgradeBtn", drillBtn.GetComponent<Button>());
        SetField(ui, "_drillBtnText", drillBtn.GetComponentInChildren<TextMeshProUGUI>());
        SetField(ui, "_droneBtn", droneBtn.GetComponent<Button>());
        SetField(ui, "_droneBtnText", droneBtn.GetComponentInChildren<TextMeshProUGUI>());
        SetField(ui, "_legendaryBtn", legendaryBtn.GetComponent<Button>());
        SetField(ui, "_legendaryBtnText", legendaryBtn.GetComponentInChildren<TextMeshProUGUI>());
        SetField(ui, "_sellBtn", sellBtn.GetComponent<Button>());
        SetField(ui, "_stageClearPanel", scPanel);
        SetField(ui, "_stageClearText", scText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageBtn", nextStageBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel", gcPanel);
        SetField(ui, "_starRenderer", starSR);
        SetField(ui, "_starSprites", starSprites);

        // Wire GameManager fields
        SetField(gm, "_stageManager", sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_miningManager", mm);
        SetField(gm, "_ui", ui);

        // Wire MiningManager fields
        SetField(mm, "_gameManager", gm);
        SetField(mm, "_ui", ui);
        SetField(mm, "_starTransform", starObj.transform);
        SetField(mm, "_starRenderer", starSR);

        // Wire InstructionPanel
        SetField(ip, "_titleText", ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText", ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText", ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton", startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton", helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot", ipBg);

        // Wire button events
        drillBtn.GetComponent<Button>().onClick.AddListener(mm.UpgradeDrill);
        droneBtn.GetComponent<Button>().onClick.AddListener(mm.BuyDrone);
        legendaryBtn.GetComponent<Button>().onClick.AddListener(mm.DoLegendaryMining);
        sellBtn.GetComponent<Button>().onClick.AddListener(mm.SellOre);
        nextStageBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ui.HideStageClear();
            gm.NextStage();
        });

        // Save scene
        string scenePath = "Assets/Scenes/063v2_StarMiner.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup063v2] StarMiner シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup063v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
