using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game069v2_DungeonDigger;

public static class Setup069v2_DungeonDigger
{
    [MenuItem("Assets/Setup/069v2 DungeonDigger")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup069v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game069v2_DungeonDigger/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.07f, 0.04f, 0.16f);
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

        // Load sprites
        Sprite blockSoilSprite    = LoadSprite(sp + "block_soil.png");
        Sprite blockRockSprite    = LoadSprite(sp + "block_rock.png");
        Sprite blockLavaSprite    = LoadSprite(sp + "block_lava.png");
        Sprite blockMonsterSprite = LoadSprite(sp + "block_monster.png");
        Sprite blockBossSprite    = LoadSprite(sp + "block_boss.png");
        Sprite itemGoldSprite     = LoadSprite(sp + "item_gold.png");
        Sprite itemCopperSprite   = LoadSprite(sp + "item_copper.png");
        Sprite itemIronSprite     = LoadSprite(sp + "item_iron.png");
        Sprite itemGemSprite      = LoadSprite(sp + "item_gem.png");
        Sprite itemRareGemSprite  = LoadSprite(sp + "item_rare_gem.png");
        Sprite monsterSprite      = LoadSprite(sp + "monster.png");
        Sprite bossSprite         = LoadSprite(sp + "boss.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("DungeonDiggerGameManager");
        var gm = gmObj.AddComponent<DungeonDiggerGameManager>();

        // StageManager (child of GameManager)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<Common.StageManager>();

        // Setup stage configs
        var stageConfigs = new Common.StageManager.StageConfig[]
        {
            new Common.StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1.0f, complexityFactor = 0.0f },
            new Common.StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 1.5f, complexityFactor = 0.3f },
            new Common.StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2.0f, complexityFactor = 0.6f },
            new Common.StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3.0f, complexityFactor = 0.8f },
            new Common.StageManager.StageConfig { speedMultiplier = 2.5f, countMultiplier = 4.0f, complexityFactor = 1.0f },
        };
        SetField(sm, "_stages", stageConfigs);

        // DigManager (child of GameManager)
        var digObj = new GameObject("DigManager");
        digObj.transform.SetParent(gmObj.transform);
        var dig = digObj.AddComponent<DigManager>();

        SetField(dig, "_blockSoilSprite",    blockSoilSprite);
        SetField(dig, "_blockRockSprite",    blockRockSprite);
        SetField(dig, "_blockLavaSprite",    blockLavaSprite);
        SetField(dig, "_blockMonsterSprite", blockMonsterSprite);
        SetField(dig, "_blockBossSprite",    blockBossSprite);
        SetField(dig, "_itemGoldSprite",     itemGoldSprite);
        SetField(dig, "_itemCopperSprite",   itemCopperSprite);
        SetField(dig, "_itemIronSprite",     itemIronSprite);
        SetField(dig, "_itemGemSprite",      itemGemSprite);
        SetField(dig, "_itemRareGemSprite",  itemRareGemSprite);
        SetField(dig, "_monsterSprite",      monsterSprite);
        SetField(dig, "_bossSprite",         bossSprite);

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD - top area
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 44, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(600, 60), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var depthText = CT(canvasObj.transform, "DepthText", "深度: 0 / 50m", 40, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(500, 55), new Vector2(10, -95));
        depthText.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);

        var goldText = CT(canvasObj.transform, "GoldText", "G: 0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(300, 55), new Vector2(-10, -95));
        goldText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        goldText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        var inventoryText = CT(canvasObj.transform, "InventoryText", "アイテム: 0", 34, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(350, 50), new Vector2(10, -155));
        inventoryText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 1f);

        var drillLevelText = CT(canvasObj.transform, "DrillLevelText", "ドリル Lv.1", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(300, 50), new Vector2(-10, -155));
        drillLevelText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.3f);
        drillLevelText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        var autoRateText = CT(canvasObj.transform, "AutoRateText", "自動: なし", 30, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300, 45), new Vector2(10, -210));
        autoRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 1f);

        var comboText = CT(canvasObj.transform, "ComboText", "", 48, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(500, 60), new Vector2(0, -260));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0f);

        // Bottom buttons area
        // Row 1 (bottom): Back to Menu + Sell
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニュー", 34, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(200, 70), new Vector2(10, 15),
            new Color(0.2f, 0.2f, 0.35f));
        backBtn.AddComponent<Common.BackToMenuButton>();

        var sellBtn = CB(canvasObj.transform, "SellButton", "売却", 36, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(180, 70), new Vector2(-10, 15),
            new Color(0.5f, 0.3f, 0f));

        // Row 2: Upgrade buttons
        var drillUpgradeBtn = CB(canvasObj.transform, "DrillUpgradeButton", "強化 20G", 32, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(230, 65), new Vector2(10, 100),
            new Color(0.3f, 0.1f, 0.5f));

        var heatShieldBtn = CB(canvasObj.transform, "HeatShieldButton", "耐熱 100G", 30, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(230, 65), new Vector2(0, 100),
            new Color(0.5f, 0.1f, 0f));

        var lanternBtn = CB(canvasObj.transform, "LanternButton", "照明 80G", 30, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(230, 65), new Vector2(-10, 100),
            new Color(0.3f, 0.3f, 0f));

        // Stage Clear Panel
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.1f, 0.35f); scRT.anchorMax = new Vector2(0.9f, 0.65f);
        scRT.offsetMin = scRT.offsetMax = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.1f, 0.05f, 0.2f, 0.95f);

        var scTitle = CT(scPanel.transform, "StageClearText", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(700, 80), Vector2.zero);
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);

        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 44, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(400, 90), Vector2.zero,
            new Color(0.2f, 0.5f, 0.2f));

        scPanel.SetActive(false);

        // All Clear Panel
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.05f, 0.3f); acRT.anchorMax = new Vector2(0.95f, 0.7f);
        acRT.offsetMin = acRT.offsetMax = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.05f, 0.02f, 0.15f, 0.97f);

        var acScore = CT(acPanel.transform, "AllClearText", "全クリア！", 60, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(800, 100), Vector2.zero);
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0f);

        acPanel.SetActive(false);

        // === InstructionPanel ===
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
        var ip = ipBg.AddComponent<Common.InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "DungeonDigger", 72, jpFont,
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f), new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.5f, 1f);

        var ipDesc = CT(ipBg.transform, "DescriptionText", "地下を掘り進めてお宝を見つけよう", 40, jpFont,
            new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.5f), new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "タップで掘削、ボタンでアップグレード", 36, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(900, 70), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var ipGoal = CT(ipBg.transform, "GoalText", "深度目標を達成してステージクリア", 36, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), new Vector2(900, 70), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 48, jpFont,
            new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.5f), new Vector2(380, 100), Vector2.zero,
            new Color(0.3f, 0.1f, 0.5f));

        // Help button
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 40, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(80, 80), new Vector2(-10, 185),
            new Color(0.2f, 0.1f, 0.35f));

        // === DungeonDiggerUI ===
        var uiObj = new GameObject("DungeonDiggerUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<DungeonDiggerUI>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // === Wire Fields ===
        // DungeonDiggerUI
        SetField(ui, "_stageText",         stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_depthText",         depthText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_goldText",          goldText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_inventoryText",     inventoryText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_drillLevelText",    drillLevelText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_autoRateText",      autoRateText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",         comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearText",    scTitle.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearText",      acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_drillUpgradeButton",drillUpgradeBtn.GetComponent<Button>());
        SetField(ui, "_drillUpgradeCostText", drillUpgradeBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_heatShieldButton",  heatShieldBtn.GetComponent<Button>());
        SetField(ui, "_heatShieldButtonText", heatShieldBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_lanternButton",     lanternBtn.GetComponent<Button>());
        SetField(ui, "_lanternButtonText", lanternBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>());

        // DungeonDiggerGameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_digManager",       dig);
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
        drillUpgradeBtn.GetComponent<Button>().onClick.AddListener(() => dig.BuyDrillUpgrade());
        heatShieldBtn.GetComponent<Button>().onClick.AddListener(() => dig.BuyHeatShield());
        lanternBtn.GetComponent<Button>().onClick.AddListener(() => dig.BuyLantern());
        sellBtn.GetComponent<Button>().onClick.AddListener(() => dig.SellAll());
        nextBtn.GetComponent<Button>().onClick.AddListener(() => {
            scPanel.SetActive(false);
            gm.NextStage();
        });

        // Save scene
        string scenePath = "Assets/Scenes/069v2_DungeonDigger.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup069v2] DungeonDigger シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup069v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
