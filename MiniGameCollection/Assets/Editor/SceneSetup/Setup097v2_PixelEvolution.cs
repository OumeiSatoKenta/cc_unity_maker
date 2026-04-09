using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game097v2_PixelEvolution;

public static class Setup097v2_PixelEvolution
{
    [MenuItem("Assets/Setup/097v2 PixelEvolution")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup097v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game097v2_PixelEvolution/";

        // === Camera ===
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.04f, 0.02f, 0.1f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // === Background ===
        Sprite bgSprite = LoadSprite(sp + "background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            float camS = 6f;
            float camW = camS * (camera != null ? camera.aspect : (16f / 9f));
            float scX = camW * 2f / bgSprite.bounds.size.x;
            float scY = camS * 2f / bgSprite.bounds.size.y;
            bgObj.transform.localScale = new Vector3(scX, scY, 1f);
        }

        // === Load sprites ===
        Sprite sprLv0 = LoadSprite(sp + "pixel_lv0.png");
        Sprite sprLv1 = LoadSprite(sp + "pixel_lv1.png");
        Sprite sprLv2 = LoadSprite(sp + "pixel_lv2.png");
        Sprite sprLv3 = LoadSprite(sp + "pixel_lv3.png");
        Sprite sprLv4 = LoadSprite(sp + "pixel_lv4.png");
        Sprite sprLv5 = LoadSprite(sp + "pixel_lv5.png");
        Sprite sprEnvTemp = LoadSprite(sp + "env_temp.png");
        Sprite sprEnvHumidity = LoadSprite(sp + "env_humidity.png");
        Sprite sprEnvLight = LoadSprite(sp + "env_light.png");
        Sprite sprCombo = LoadSprite(sp + "combo_icon.png");
        Sprite sprMutation = LoadSprite(sp + "mutation_icon.png");

        // === Evolution Display (SpriteRenderer in world space) ===
        var evoDisplayObj = new GameObject("EvolutionDisplay");
        var evoSr = evoDisplayObj.AddComponent<SpriteRenderer>();
        evoSr.sprite = sprLv0;
        evoSr.sortingOrder = 5;
        // Center of game area (slightly above center, below HUD)
        evoDisplayObj.transform.position = new Vector3(0, 1.2f, 0);
        evoDisplayObj.transform.localScale = Vector3.one * 2.5f;

        // === GameManager hierarchy ===
        var gmObj = new GameObject("PixelEvolutionGameManager");
        var gm = gmObj.AddComponent<PixelEvolutionGameManager>();

        // StageManager (child of GM)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 2, complexityFactor = 0.0f, stageName = "Stage 1" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 3, complexityFactor = 0.0f, stageName = "Stage 2" },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 3, complexityFactor = 0.3f, stageName = "Stage 3" },
            new StageManager.StageConfig { speedMultiplier = 1.4f, countMultiplier = 3, complexityFactor = 0.6f, stageName = "Stage 4" },
            new StageManager.StageConfig { speedMultiplier = 1.6f, countMultiplier = 3, complexityFactor = 1.0f, stageName = "Stage 5" },
        };
        sm.SetConfigs(stageConfigs);

        // EvolutionManager (child of GM)
        var emObj = new GameObject("EvolutionManager");
        emObj.transform.SetParent(gmObj.transform);
        var em = emObj.AddComponent<EvolutionManager>();

        // === Canvas (main HUD) ===
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
        var stageTextGo = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 50), new Vector2(15, -15));
        stageTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.3f);

        var scoreTextGo = CT(canvasObj.transform, "ScoreText", "Score: 0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300, 50), new Vector2(-15, -15));
        scoreTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.3f);

        var generationTextGo = CT(canvasObj.transform, "GenerationText", "世代: 0 / 20", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 50), new Vector2(0, -15));
        generationTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        generationTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.5f);

        var evolutionLevelTextGo = CT(canvasObj.transform, "EvolutionLevelText", "進化Lv: 0 / 5", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 50), new Vector2(0, -60));
        evolutionLevelTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        evolutionLevelTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.8f, 1f);

        var comboTextGo = CT(canvasObj.transform, "ComboText", "COMBO x3!", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 80), new Vector2(0, 80));
        comboTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);
        comboTextGo.SetActive(false);

        var mutationTextGo = CT(canvasObj.transform, "MutationText", "突然変異！", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 70), new Vector2(0, 150));
        mutationTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        mutationTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 1f);
        mutationTextGo.SetActive(false);

        // === Environment Panel (bottom area, above buttons) ===
        // Environment display: Temperature, Humidity, Light
        var envPanel = new GameObject("EnvironmentPanel", typeof(RectTransform));
        envPanel.transform.SetParent(canvasObj.transform, false);
        var envRT = envPanel.GetComponent<RectTransform>();
        envRT.anchorMin = new Vector2(0.5f, 0f);
        envRT.anchorMax = new Vector2(0.5f, 0f);
        envRT.pivot = new Vector2(0.5f, 0f);
        envRT.sizeDelta = new Vector2(900, 260);
        envRT.anchoredPosition = new Vector2(0, 155);
        var envPanelImg = envPanel.AddComponent<Image>();
        envPanelImg.color = new Color(0.1f, 0.05f, 0.2f, 0.8f);

        // Temp label + value + buttons (row 1: y=0.8)
        var tempLabel = CT(envPanel.transform, "TempLabel", "温度", 28, jpFont,
            new Vector2(0.05f, 0.8f), new Vector2(0.05f, 0.8f), new Vector2(0f, 0.5f),
            new Vector2(80, 38), Vector2.zero);
        tempLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.3f);

        var tempValueText = CT(envPanel.transform, "TempValue", "中", 32, jpFont,
            new Vector2(0.2f, 0.8f), new Vector2(0.2f, 0.8f), new Vector2(0.5f, 0.5f),
            new Vector2(60, 40), Vector2.zero);
        tempValueText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        tempValueText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var tempBtnLow = CB(envPanel.transform, "TempBtnLow", "低", 26, jpFont,
            new Vector2(0.34f, 0.7f), new Vector2(0.34f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.3f, 0.1f, 0.1f));
        var tempBtnMid = CB(envPanel.transform, "TempBtnMid", "中", 26, jpFont,
            new Vector2(0.46f, 0.7f), new Vector2(0.46f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.3f, 0.2f, 0.1f));
        var tempBtnHigh = CB(envPanel.transform, "TempBtnHigh", "高", 26, jpFont,
            new Vector2(0.58f, 0.7f), new Vector2(0.58f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.4f, 0.1f, 0.05f));

        // Humidity (row 2: y=0.5)
        var humLabel = CT(envPanel.transform, "HumLabel", "湿度", 28, jpFont,
            new Vector2(0.05f, 0.5f), new Vector2(0.05f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(80, 38), Vector2.zero);
        humLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.7f, 1f);

        var humValueText = CT(envPanel.transform, "HumValue", "中", 32, jpFont,
            new Vector2(0.2f, 0.5f), new Vector2(0.2f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(60, 40), Vector2.zero);
        humValueText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        humValueText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var humBtnLow = CB(envPanel.transform, "HumBtnLow", "低", 26, jpFont,
            new Vector2(0.34f, 0.4f), new Vector2(0.34f, 0.4f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.1f, 0.1f, 0.3f));
        var humBtnMid = CB(envPanel.transform, "HumBtnMid", "中", 26, jpFont,
            new Vector2(0.46f, 0.4f), new Vector2(0.46f, 0.4f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.1f, 0.2f, 0.4f));
        var humBtnHigh = CB(envPanel.transform, "HumBtnHigh", "高", 26, jpFont,
            new Vector2(0.58f, 0.4f), new Vector2(0.58f, 0.4f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.05f, 0.1f, 0.5f));

        // Light (row 3: y=0.2)
        var lightLabel = CT(envPanel.transform, "LightLabel", "光量", 28, jpFont,
            new Vector2(0.05f, 0.2f), new Vector2(0.05f, 0.2f), new Vector2(0f, 0.5f),
            new Vector2(80, 38), Vector2.zero);
        lightLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.95f, 0.4f);

        var lightValueText = CT(envPanel.transform, "LightValue", "中", 32, jpFont,
            new Vector2(0.2f, 0.2f), new Vector2(0.2f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(60, 40), Vector2.zero);
        lightValueText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        lightValueText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var lightBtnLow = CB(envPanel.transform, "LightBtnLow", "低", 26, jpFont,
            new Vector2(0.34f, 0.1f), new Vector2(0.34f, 0.1f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.2f, 0.15f, 0.05f));
        var lightBtnMid = CB(envPanel.transform, "LightBtnMid", "中", 26, jpFont,
            new Vector2(0.46f, 0.1f), new Vector2(0.46f, 0.1f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.35f, 0.28f, 0.05f));
        var lightBtnHigh = CB(envPanel.transform, "LightBtnHigh", "高", 26, jpFont,
            new Vector2(0.58f, 0.1f), new Vector2(0.58f, 0.1f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 48), Vector2.zero, new Color(0.5f, 0.4f, 0.05f));

        // === Advance Generation Button ===
        var advGenBtn = CB(canvasObj.transform, "AdvanceGenerationButton", "世代交代", 38, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(300, 70), new Vector2(0, 80), new Color(0.1f, 0.3f, 0.5f));

        // === Back to Menu Button (bottom) ===
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニュー", 32, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 55), new Vector2(0, 20), new Color(0.2f, 0.2f, 0.3f));
        backBtn.AddComponent<BackToMenuButton>();

        // === Branch Choice Panel ===
        var branchPanel = new GameObject("BranchChoicePanel", typeof(RectTransform));
        branchPanel.transform.SetParent(canvasObj.transform, false);
        var branchRT = branchPanel.GetComponent<RectTransform>();
        branchRT.anchorMin = new Vector2(0.5f, 0.5f); branchRT.anchorMax = new Vector2(0.5f, 0.5f);
        branchRT.pivot = new Vector2(0.5f, 0.5f); branchRT.sizeDelta = new Vector2(800, 450);
        var branchImg = branchPanel.AddComponent<Image>();
        branchImg.color = new Color(0.08f, 0.04f, 0.15f, 0.97f);

        var branchTitleGo = CT(branchPanel.transform, "BranchTitle", "進化の分岐を選択してください", 40, jpFont,
            new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.5f),
            new Vector2(750, 60), Vector2.zero);
        branchTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        branchTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.3f);

        var branchBtn0 = CB(branchPanel.transform, "BranchBtn0", "安全な進化", 38, jpFont,
            new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), Vector2.zero, new Color(0.1f, 0.3f, 0.1f));
        var branchBtn1 = CB(branchPanel.transform, "BranchBtn1", "複合進化", 38, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), Vector2.zero, new Color(0.2f, 0.1f, 0.3f));
        var branchBtn2 = CB(branchPanel.transform, "BranchBtn2", "★隠し進化 (+200pt)", 36, jpFont,
            new Vector2(0.5f, 0.21f), new Vector2(0.5f, 0.21f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), Vector2.zero, new Color(0.35f, 0.25f, 0.0f));
        branchBtn2.GetComponentInChildren<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);
        branchPanel.SetActive(false);

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f); scRT.sizeDelta = new Vector2(700, 400);
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.06f, 0.04f, 0.12f, 0.96f);

        var scTitleGo = CT(scPanel.transform, "StageClearText", "Stage 1 クリア！", 50, jpFont,
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f),
            new Vector2(650, 80), Vector2.zero);
        scTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.3f);

        var scScoreGo = CT(scPanel.transform, "StageClearScore", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 70), Vector2.zero);
        scScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 40, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.1f, 0.3f, 0.1f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f); acRT.sizeDelta = new Vector2(760, 440);
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.06f, 0.04f, 0.12f, 0.97f);

        var acTitleGo = CT(acPanel.transform, "AllClearTitle", "PixelEvolution\nクリア！", 52, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(720, 130), Vector2.zero);
        acTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.3f);

        var acScoreGo = CT(acPanel.transform, "AllClearScore", "Total Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        acScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var acBackBtn = CB(acPanel.transform, "BackToMenuButton2", "メニューへ", 40, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 72), Vector2.zero, new Color(0.2f, 0.2f, 0.3f));
        acBackBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goPanelRT = goPanel.GetComponent<RectTransform>();
        goPanelRT.anchorMin = new Vector2(0.5f, 0.5f); goPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        goPanelRT.pivot = new Vector2(0.5f, 0.5f); goPanelRT.sizeDelta = new Vector2(700, 420);
        var goPanelImg = goPanel.AddComponent<Image>();
        goPanelImg.color = new Color(0.12f, 0.04f, 0.04f, 0.97f);

        var goTitleGo = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 54, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(650, 80), Vector2.zero);
        goTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScoreGo = CT(goPanel.transform, "GameOverScore", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        goScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var goRetryBtn = CB(goPanel.transform, "RetryButton", "もう一度", 42, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 72), Vector2.zero, new Color(0.4f, 0.1f, 0.1f));
        goPanel.SetActive(false);

        // === InstructionPanel (separate canvas, highest sort order) ===
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
        var ipBgRT = ipBg.GetComponent<RectTransform>();
        ipBgRT.anchorMin = Vector2.zero; ipBgRT.anchorMax = Vector2.one;
        ipBgRT.offsetMin = ipBgRT.offsetMax = Vector2.zero;
        var ipBgImg = ipBg.AddComponent<Image>();
        ipBgImg.color = new Color(0.06f, 0.04f, 0.12f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "PixelEvolution", 68, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.3f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 38, jpFont,
            new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 1f, 0.8f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 32, jpFont,
            new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 150), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 0.6f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 32, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 1f, 0.6f);

        var startBtnGo = CB(ipBg.transform, "StartButton", "はじめる", 50, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.1f, 0.3f, 0.1f));

        // Help button ("?") on main canvas
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(80, 80), new Vector2(-15, 90), new Color(0.2f, 0.3f, 0.15f));

        // === Wire InstructionPanel ===
        SetField(ip, "_panelRoot",       ipBg);
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtnGo.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());

        // === PixelEvolutionUI ===
        var uiObj = new GameObject("PixelEvolutionUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<PixelEvolutionUI>();

        SetField(ui, "_stageText",           stageTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",           scoreTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_generationText",      generationTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_evolutionLevelText",  evolutionLevelTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",           comboTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_mutationText",        mutationTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_tempValueText",       tempValueText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_humidityValueText",   humValueText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_lightValueText",      lightValueText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_branchPanel",         branchPanel);
        SetField(ui, "_branchBtn0",          branchBtn0.GetComponent<Button>());
        SetField(ui, "_branchBtn1",          branchBtn1.GetComponent<Button>());
        SetField(ui, "_branchBtn2",          branchBtn2.GetComponent<Button>());
        SetField(ui, "_branchTitle",         branchTitleGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",     scPanel);
        SetField(ui, "_stageClearText",      scTitleGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearScoreText", scScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_allClearPanel",       acPanel);
        SetField(ui, "_allClearScoreText",   acScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",       goPanel);
        SetField(ui, "_gameOverScoreText",   goScoreGo.GetComponent<TextMeshProUGUI>());

        // === Wire EvolutionManager ===
        SetField(em, "_gameManager",    gm);
        SetField(em, "_ui",             ui);
        SetField(em, "_spriteLv0",      sprLv0);
        SetField(em, "_spriteLv1",      sprLv1);
        SetField(em, "_spriteLv2",      sprLv2);
        SetField(em, "_spriteLv3",      sprLv3);
        SetField(em, "_spriteLv4",      sprLv4);
        SetField(em, "_spriteLv5",      sprLv5);
        SetField(em, "_evolutionDisplay", evoSr);

        // === Wire button events ===
        // Temperature buttons
        AddOnClickInt(tempBtnLow, em, "SetTemperature", 0);
        AddOnClickInt(tempBtnMid, em, "SetTemperature", 1);
        AddOnClickInt(tempBtnHigh, em, "SetTemperature", 2);
        // Humidity buttons
        AddOnClickInt(humBtnLow, em, "SetHumidity", 0);
        AddOnClickInt(humBtnMid, em, "SetHumidity", 1);
        AddOnClickInt(humBtnHigh, em, "SetHumidity", 2);
        // Light buttons
        AddOnClickInt(lightBtnLow, em, "SetLight", 0);
        AddOnClickInt(lightBtnMid, em, "SetLight", 1);
        AddOnClickInt(lightBtnHigh, em, "SetLight", 2);
        // Advance generation
        UnityEngine.Events.UnityAction advAct = em.AdvanceGeneration;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(advGenBtn.GetComponent<Button>().onClick, advAct);
        // Branch buttons
        AddOnClickInt(branchBtn0, em, "SelectBranch", 0);
        AddOnClickInt(branchBtn1, em, "SelectBranch", 1);
        AddOnClickInt(branchBtn2, em, "SelectBranch", 2);
        // Stage clear next
        UnityEngine.Events.UnityAction nextAct = gm.NextStage;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(scNextBtn.GetComponent<Button>().onClick, nextAct);
        // Retry
        UnityEngine.Events.UnityAction retryAct = gm.RestartGame;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, retryAct);

        // === Wire GameManager ===
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_evolutionManager", em);
        SetField(gm, "_ui",               ui);

        // === EventSystem ===
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // === Save scene ===
        string scenePath = "Assets/Scenes/097v2_PixelEvolution.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup097v2] PixelEvolution シーン作成完了: " + scenePath);
    }

    static void AddOnClickInt(GameObject btnObj, EvolutionManager em, string methodName, int value)
    {
        var btn = btnObj.GetComponent<Button>();
        if (btn == null) return;
        var method = typeof(EvolutionManager).GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method == null) return;
        var action = (UnityEngine.Events.UnityAction<int>)System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction<int>), em, method);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btn.onClick, action, value);
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
        if (obj == null) return;
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
        else Debug.LogWarning($"[Setup097v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
