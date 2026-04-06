using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game064v2_AquaCity;

public static class Setup064v2_AquaCity
{
    [MenuItem("Assets/Setup/064v2 AquaCity")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup064v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game064v2_AquaCity/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.02f, 0.05f, 0.15f);
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
            bgObj.transform.localScale = new Vector3(0.012f, 0.024f, 1f);
        }

        // Load building sprites
        Sprite[] buildingSprites = new Sprite[]
        {
            LoadSprite(sp + "House.png"),
            LoadSprite(sp + "Plaza.png"),
            LoadSprite(sp + "Coral.png"),
            LoadSprite(sp + "Deco.png"),
            LoadSprite(sp + "Aquarium.png"),
            LoadSprite(sp + "DeepBase.png"),
        };

        // Load fish sprites
        Sprite[] fishSprites = new Sprite[]
        {
            LoadSprite(sp + "Fish1.png"),
            LoadSprite(sp + "Fish2.png"),
            LoadSprite(sp + "Fish3.png"),
        };

        Sprite sharkSprite = LoadSprite(sp + "Shark.png");

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<AquaCityGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        var stages = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 1, complexityFactor = 0.1f },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.2f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 0.3f },
            new StageManager.StageConfig { speedMultiplier = 3.0f, countMultiplier = 5, complexityFactor = 0.5f },
        };
        sm.SetConfigs(stages);

        // CityManager
        var cmObj = new GameObject("CityManager");
        cmObj.transform.SetParent(gmObj.transform);
        var cm = cmObj.AddComponent<CityManager>();

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
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // HUD: Population (top left)
        var popText = CT(canvasObj.transform, "PopulationText", "人口: 0 / 50", 34, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(420, 55), new Vector2(15, -90));
        popText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        popText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.8f);

        // HUD: Coins (top right)
        var coinsText = CT(canvasObj.transform, "CoinsText", "コイン: 20", 34, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(350, 55), new Vector2(-15, -90));
        coinsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        coinsText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // HUD: Auto rate (center)
        var autoRateText = CT(canvasObj.transform, "AutoRateText", "", 28, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 48), new Vector2(0, -145));
        autoRateText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        autoRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 1f);

        // Combo text (center, hidden)
        var comboText = CT(canvasObj.transform, "ComboText", "x2 コンボ!", 44, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 65), new Vector2(0, -195));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.1f);
        comboText.SetActive(false);

        // Shark Warning (center warning bar)
        var sharkWarning = CT(canvasObj.transform, "SharkWarning", "🦈 サメ出現！タップして撃退！", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(800, 60), new Vector2(0, -260));
        var swTMP = sharkWarning.GetComponent<TextMeshProUGUI>();
        swTMP.alignment = TextAlignmentOptions.Center;
        swTMP.color = new Color(1f, 0.3f, 0.2f);
        var swBg = sharkWarning.AddComponent<Image>();
        swBg.color = new Color(0.5f, 0.05f, 0.05f, 0.85f);
        sharkWarning.SetActive(false);

        // === Bottom Shop Buttons (3 columns top row, 3 columns bottom row) ===
        // Row 1 (higher): House, Plaza, Coral
        var houseBtn = CB(canvasObj.transform, "BuyHouseButton", "住宅\n無料", 26, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(165, 85), new Vector2(20, 440),
            new Color(0.5f, 0.2f, 0.7f));
        var plazaBtn = CB(canvasObj.transform, "BuyPlazaButton", "広場\n10G", 26, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(165, 85), new Vector2(0, 440),
            new Color(0.6f, 0.2f, 0.5f));
        var coralBtn = CB(canvasObj.transform, "BuyCoralButton", "珊瑚礁\n20G", 26, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(165, 85), new Vector2(-20, 440),
            new Color(0.7f, 0.2f, 0.3f));

        // Row 2 (lower): Deco, Aquarium, DeepBase (hidden initially)
        var decoBtn = CB(canvasObj.transform, "BuyDecoButton", "デコ\n30G", 26, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(165, 85), new Vector2(20, 340),
            new Color(0.2f, 0.5f, 0.5f));
        decoBtn.SetActive(false);

        var aquariumBtn = CB(canvasObj.transform, "BuyAquariumButton", "水族館\n50G", 26, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(165, 85), new Vector2(0, 340),
            new Color(0.2f, 0.3f, 0.7f));
        aquariumBtn.SetActive(false);

        var deepBaseBtn = CB(canvasObj.transform, "BuyDeepBaseButton", "深海基地\n200G", 24, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(165, 85), new Vector2(-20, 340),
            new Color(0.1f, 0.1f, 0.5f));
        deepBaseBtn.SetActive(false);

        // Menu button (bottom left)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 28, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(180, 65), new Vector2(20, 250),
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
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(420, 85), Vector2.zero,
            new Color(0.2f, 0.3f, 0.8f));
        scPanel.SetActive(false);

        // All Clear Panel
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acpRT = acPanel.GetComponent<RectTransform>();
        acpRT.anchorMin = new Vector2(0.1f, 0.25f); acpRT.anchorMax = new Vector2(0.9f, 0.75f);
        acpRT.offsetMin = acpRT.offsetMax = Vector2.zero;
        var acpImg = acPanel.AddComponent<Image>();
        acpImg.color = new Color(0.05f, 0.08f, 0.25f, 0.97f);
        var acText = CT(acPanel.transform, "AllClearText", "AquaCity 完全制覇！", 46, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(800, 100), Vector2.zero);
        acText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var acMenuBtn = CB(acPanel.transform, "MenuButton2", "メニューへ", 34, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(320, 80), Vector2.zero,
            new Color(0.3f, 0.2f, 0.6f));
        acMenuBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

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

        var ipTitle = CT(ipBg.transform, "TitleText", "AquaCity", 64, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(900, 90), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.85f, 1f);

        var ipDesc = CT(ipBg.transform, "DescriptionText", "海底に都市を作って魚を集めよう", 38, jpFont,
            new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "建物をタップしてコイン回収、ボタンで建物を購入", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.7f);

        var ipGoal = CT(ipBg.transform, "GoalText", "人口目標を達成して次のステージへ進もう", 34, jpFont,
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 44, jpFont,
            new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.5f), new Vector2(380, 90), Vector2.zero,
            new Color(0.2f, 0.35f, 0.8f));

        // Help button
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(80, 80), new Vector2(-20, 250),
            new Color(0.2f, 0.3f, 0.5f));

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- AquaCityUI ---
        var uiObj = new GameObject("AquaCityUI");
        uiObj.transform.SetParent(canvasObj.transform, false);
        var ui = uiObj.AddComponent<AquaCityUI>();

        // Wire AquaCityUI fields
        SetField(ui, "_stageText", stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_populationText", popText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_coinsText", coinsText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText", comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_autoRateText", autoRateText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_sharkWarningText", sharkWarning.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel", scPanel);
        SetField(ui, "_stageClearText", scText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton", nextStageBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel", acPanel);
        SetField(ui, "_buyHouseButton", houseBtn.GetComponent<Button>());
        SetField(ui, "_buyPlazaButton", plazaBtn.GetComponent<Button>());
        SetField(ui, "_buyCoralButton", coralBtn.GetComponent<Button>());
        SetField(ui, "_buyDecoButton", decoBtn.GetComponent<Button>());
        SetField(ui, "_buyAquariumButton", aquariumBtn.GetComponent<Button>());
        SetField(ui, "_buyDeepBaseButton", deepBaseBtn.GetComponent<Button>());
        SetField(ui, "_menuButton", menuBtn.GetComponent<Button>());
        SetField(ui, "_cityManager", cm);
        SetField(ui, "_gameManager", gm);

        // Wire CityManager fields
        SetField(cm, "_gameManager", gm);
        SetField(cm, "_ui", ui);
        SetField(cm, "_houseSprite", buildingSprites[0]);
        SetField(cm, "_plazaSprite", buildingSprites[1]);
        SetField(cm, "_coralSprite", buildingSprites[2]);
        SetField(cm, "_decoSprite", buildingSprites[3]);
        SetField(cm, "_aquariumSprite", buildingSprites[4]);
        SetField(cm, "_deepBaseSprite", buildingSprites[5]);
        SetField(cm, "_fishSprites", fishSprites);
        SetField(cm, "_sharkSprite", sharkSprite);

        // Wire GameManager fields
        SetField(gm, "_stageManager", sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_cityManager", cm);
        SetField(gm, "_ui", ui);

        // Wire InstructionPanel
        SetField(ip, "_titleText", ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText", ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText", ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton", startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton", helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot", ipBg);

        // Save scene
        string scenePath = "Assets/Scenes/064v2_AquaCity.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup064v2] AquaCity シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup064v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
