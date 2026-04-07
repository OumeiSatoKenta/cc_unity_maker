using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game081v2_PetBonsai;

public static class Setup081v2_PetBonsai
{
    [MenuItem("Assets/Setup/081v2 PetBonsai")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup081v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game081v2_PetBonsai/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.96f, 0.92f, 0.86f);
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
            float camSize = 6f;
            float camWidth = camSize * (16f / 9f);
            float scaleX = camWidth * 2f / bgSprite.bounds.size.x;
            float scaleY = camSize * 2f / bgSprite.bounds.size.y;
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // === GameManager hierarchy ===
        var gmObj = new GameObject("PetBonsaiGameManager");
        var gm = gmObj.AddComponent<PetBonsaiGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f,  countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.0f,  countMultiplier = 1, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.0f,  countMultiplier = 1, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 1.2f,  countMultiplier = 1, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 1.2f,  countMultiplier = 2, complexityFactor = 1.0f },
        };
        SetField(sm, "_configs", stageConfigs);

        // PetBonsaiManager
        var bmObj = new GameObject("PetBonsaiManager");
        bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<PetBonsaiManager>();

        // Wire sprites to BonsaiManager
        SetField(bm, "_branchNormal",   LoadSprite(sp + "branch_normal.png"));
        SetField(bm, "_branchOvergrown", LoadSprite(sp + "branch_overgrown.png"));
        SetField(bm, "_branchPest",     LoadSprite(sp + "branch_pest.png"));
        SetField(bm, "_gm",             gm);

        // Bonsai world objects (trunk, pot)
        Sprite trunkSp = LoadSprite(sp + "tree_trunk.png");
        Sprite potSp   = LoadSprite(sp + "pot.png");
        Sprite leafSp  = LoadSprite(sp + "leaf_cluster.png");

        if (trunkSp != null)
        {
            var trunk = new GameObject("Trunk");
            trunk.transform.SetParent(bmObj.transform);
            trunk.transform.position = new Vector3(0f, -1.0f, 0f);
            trunk.transform.localScale = new Vector3(1.2f, 1.8f, 1f);
            var sr = trunk.AddComponent<SpriteRenderer>();
            sr.sprite = trunkSp;
            sr.sortingOrder = 1;
        }

        if (potSp != null)
        {
            var pot = new GameObject("Pot");
            pot.transform.SetParent(bmObj.transform);
            pot.transform.position = new Vector3(0f, -2.8f, 0f);
            pot.transform.localScale = new Vector3(1.8f, 1.4f, 1f);
            var sr = pot.AddComponent<SpriteRenderer>();
            sr.sprite = potSp;
            sr.sortingOrder = 0;
        }

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // === HUD (top area) ===
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 44, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(350, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.3f, 0.1f);

        var beautyText = CT(canvasObj.transform, "BeautyText", "美しさ: 0", 44, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(380, 55), new Vector2(-20, -30));
        beautyText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        beautyText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.5f, 0.1f);

        var waterText = CT(canvasObj.transform, "WaterText", "水やり: 0/3", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(380, 50), new Vector2(20, -90));
        waterText.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.6f, 0.9f);

        var seasonText = CT(canvasObj.transform, "SeasonText", "", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(200, 50), new Vector2(-20, -90));
        seasonText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // Growth slider (below HUD)
        var growthSliderObj = new GameObject("GrowthSlider", typeof(RectTransform));
        growthSliderObj.transform.SetParent(canvasObj.transform, false);
        var gsRT = growthSliderObj.GetComponent<RectTransform>();
        gsRT.anchorMin = new Vector2(0f, 1f);
        gsRT.anchorMax = new Vector2(1f, 1f);
        gsRT.pivot = new Vector2(0.5f, 1f);
        gsRT.sizeDelta = new Vector2(-40f, 30f);
        gsRT.anchoredPosition = new Vector2(0, -145);
        var growthSlider = growthSliderObj.AddComponent<Slider>();
        growthSlider.minValue = 0f;
        growthSlider.maxValue = 1f;
        growthSlider.value = 0f;
        {
            var bgObj = new GameObject("Background", typeof(RectTransform));
            bgObj.transform.SetParent(growthSliderObj.transform, false);
            var bgRT2 = bgObj.GetComponent<RectTransform>();
            bgRT2.anchorMin = Vector2.zero; bgRT2.anchorMax = Vector2.one;
            bgRT2.offsetMin = bgRT2.offsetMax = Vector2.zero;
            var bgImg2 = bgObj.AddComponent<Image>();
            bgImg2.color = new Color(0.8f, 0.7f, 0.5f, 0.5f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(growthSliderObj.transform, false);
            var faRT2 = fillArea.GetComponent<RectTransform>();
            faRT2.anchorMin = new Vector2(0f, 0.05f); faRT2.anchorMax = new Vector2(1f, 0.95f);
            faRT2.offsetMin = new Vector2(2, 0); faRT2.offsetMax = new Vector2(-2, 0);

            var fillObj = new GameObject("Fill", typeof(RectTransform));
            fillObj.transform.SetParent(fillArea.transform, false);
            var fillRT = fillObj.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.75f, 0.3f);
            growthSlider.fillRect = fillRT;

            var labelGO = CT(growthSliderObj.transform, "GrowthLabel", "成長", 24, jpFont,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(80, 30), new Vector2(-80, 0));
            labelGO.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.6f, 0.2f);
        }

        var comboText = CT(canvasObj.transform, "ComboText", "", 56, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 200));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        comboText.SetActive(false);

        var feedbackText = CT(canvasObj.transform, "FeedbackText", "", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 80), new Vector2(0, 80));
        feedbackText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        feedbackText.SetActive(false);

        var rivalText = CT(canvasObj.transform, "RivalText", "", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 50), new Vector2(0, -200));
        rivalText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        rivalText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.3f, 0.3f);
        rivalText.SetActive(false);

        // === Bottom buttons ===
        Sprite wateringSp = LoadSprite(sp + "watering_can.png");
        Sprite fertSp     = LoadSprite(sp + "fertilizer.png");

        // Water button
        var waterBtn = CB(canvasObj.transform, "WaterButton", "水やり", 44, jpFont,
            new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.5f, 0f),
            new Vector2(220, 80), new Vector2(0, 80), new Color(0.2f, 0.55f, 0.85f, 0.9f));
        if (wateringSp != null)
        {
            var iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(waterBtn.transform, false);
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0f, 0.5f);
            iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.pivot = new Vector2(0f, 0.5f);
            iconRT.sizeDelta = new Vector2(50, 50);
            iconRT.anchoredPosition = new Vector2(5, 0);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = wateringSp;
            iconImg.preserveAspect = true;
        }

        // Fertilize button
        var fertBtn = CB(canvasObj.transform, "FertilizeButton", "肥料", 44, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(220, 80), new Vector2(0, 80), new Color(0.75f, 0.5f, 0.15f, 0.9f));

        // Showcase button
        var showcaseBtn = CB(canvasObj.transform, "ShowcaseButton", "品評会", 44, jpFont,
            new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.5f, 0f),
            new Vector2(220, 80), new Vector2(0, 80), new Color(0.7f, 0.2f, 0.5f, 0.9f));

        // Back to menu button
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 36, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 60), new Vector2(0, 12), new Color(0.2f, 0.2f, 0.2f, 0.8f));

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        {
            var rt = scPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = scPanel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);

            var scTitle = CT(scPanel.transform, "StageClearText", "ステージクリア！", 72, jpFont,
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
                new Vector2(800, 100), new Vector2(0, 0));
            scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

            var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 52, jpFont,
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f),
                new Vector2(450, 85), new Vector2(0, 0), new Color(0.2f, 0.6f, 0.2f));
            nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());

            scPanel.SetActive(false);
        }

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        {
            var rt = acPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = acPanel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.7f);

            CT(acPanel.transform, "AllClearText", "全ステージクリア！", 72, jpFont,
                new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
                new Vector2(900, 100), new Vector2(0, 0))
                .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            var acScore = CT(acPanel.transform, "AllClearScoreText", "最終スコア: 0", 56, jpFont,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(700, 80), new Vector2(0, 0));
            acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            acPanel.SetActive(false);
        }

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        {
            var rt = goPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = goPanel.AddComponent<Image>();
            img.color = new Color(0.1f, 0f, 0f, 0.75f);

            CT(goPanel.transform, "GameOverText", "枯れてしまった...", 68, jpFont,
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
                new Vector2(800, 100), new Vector2(0, 0))
                .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            var goScore = CT(goPanel.transform, "GameOverScoreText", "スコア: 0", 52, jpFont,
                new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.5f),
                new Vector2(600, 70), new Vector2(0, 0));
            goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            goPanel.SetActive(false);
        }

        // === InstructionPanel ===
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        ipCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ((CanvasScaler)ipCanvas.GetComponent<CanvasScaler>()).referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ip = ipCanvas.AddComponent<InstructionPanel>();

        var ipBg = new GameObject("IPBackground", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvas.transform, false);
        {
            var rt = ipBg.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = ipBg.AddComponent<Image>();
            img.color = new Color(0.05f, 0.02f, 0.0f, 0.92f);

            var ipTitle = CT(ipBg.transform, "TitleText", "", 72, jpFont,
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
                new Vector2(900, 90), new Vector2(0, 0));
            ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.7f, 0.3f);

            var ipDesc = CT(ipBg.transform, "DescText", "", 40, jpFont,
                new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.5f),
                new Vector2(900, 90), new Vector2(0, 0));
            ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            var ipCtrl = CT(ipBg.transform, "ControlsText", "", 34, jpFont,
                new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.5f),
                new Vector2(900, 140), new Vector2(0, 0));
            ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

            var ipGoal = CT(ipBg.transform, "GoalText", "", 34, jpFont,
                new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.5f),
                new Vector2(900, 90), new Vector2(0, 0));
            ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.5f);

            var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
                new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
                new Vector2(400, 75), new Vector2(0, 0), new Color(0.3f, 0.15f, 0.05f));

            var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(65, 65), new Vector2(-15, 80), new Color(0.2f, 0.1f, 0.05f, 0.9f));

            SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
            SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
            SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
            SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
            SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
            SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
            SetField(ip, "_panelRoot",       ipBg);
        }

        // === PetBonsaiUI ===
        var uiObj = new GameObject("PetBonsaiUI");
        var ui = uiObj.AddComponent<PetBonsaiUI>();

        SetField(ui, "_stageText",        stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_beautyText",       beautyText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_growthSlider",     growthSlider);
        SetField(ui, "_waterText",        waterText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_seasonText",       seasonText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",        comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_feedbackText",     feedbackText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_rivalText",        rivalText.GetComponent<TextMeshProUGUI>());

        // Find AllClearPanel children
        var acScoreGO = acPanel.transform.Find("AllClearScoreText");
        var goScoreGO = goPanel.transform.Find("GameOverScoreText");
        var scTitleGO = scPanel.transform.Find("StageClearText");

        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearText",    scTitleGO != null ? scTitleGO.GetComponent<TextMeshProUGUI>() : null);
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearScoreText", acScoreGO != null ? acScoreGO.GetComponent<TextMeshProUGUI>() : null);
        SetField(ui, "_gameOverPanel",     goPanel);
        SetField(ui, "_gameOverScoreText", goScoreGO != null ? goScoreGO.GetComponent<TextMeshProUGUI>() : null);

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_bonsaiManager",    bm);
        SetField(gm, "_ui",               ui);

        // Button listeners
        waterBtn.GetComponent<Button>().onClick.AddListener(() => bm.DoWater());
        fertBtn.GetComponent<Button>().onClick.AddListener(() => bm.DoFertilize());
        showcaseBtn.GetComponent<Button>().onClick.AddListener(() => bm.DoShowcase());
        backBtn.AddComponent<BackToMenuButton>();

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/081v2_PetBonsai.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup081v2] PetBonsai シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup081v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
        go.AddComponent<Button>().targetGraphic = img;
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
