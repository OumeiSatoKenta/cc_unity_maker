using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game083v2_StarChef;

public static class Setup083v2_StarChef
{
    [MenuItem("Assets/Setup/083v2 StarChef")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup083v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game083v2_StarChef/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.05f, 0.03f, 0.12f);
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
        var gmObj = new GameObject("StarChefGameManager");
        var gm = gmObj.AddComponent<StarChefGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 0.5f, countMultiplier = 0, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 2.5f, countMultiplier = 4, complexityFactor = 1.0f },
        };
        SetField(sm, "_configs", stageConfigs);

        // CookingManager (as child of GameManager)
        var cmObj = new GameObject("CookingManager");
        cmObj.transform.SetParent(gmObj.transform);
        var cm = cmObj.AddComponent<CookingManager>();

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

        // HUD Panel (top)
        var hudPanel = new GameObject("HUDPanel", typeof(RectTransform));
        hudPanel.transform.SetParent(canvasObj.transform, false);
        var hudRT = hudPanel.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0f, 1f); hudRT.anchorMax = new Vector2(1f, 1f);
        hudRT.pivot = new Vector2(0.5f, 1f);
        hudRT.sizeDelta = new Vector2(0f, 100f);
        hudRT.anchoredPosition = new Vector2(0f, -10f);
        var hudBg = hudPanel.AddComponent<Image>();
        hudBg.color = new Color(0.05f, 0.05f, 0.15f, 0.85f);

        var stageText = CT(hudPanel.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0.05f, 0.5f), new Vector2(0.4f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 60f), new Vector2(0f, 0f));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var scoreText = CT(hudPanel.transform, "ScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.55f, 0.5f), new Vector2(0.95f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 60f), new Vector2(0f, 0f));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        // Fail counter (below HUD)
        var failText = CT(canvasObj.transform, "FailText", "失敗: 0/3", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300f, 50f), new Vector2(0f, -115f));
        failText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);
        failText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Combo text (center screen, hidden by default)
        var comboText = CT(canvasObj.transform, "ComboText", "Combo x3!", 56, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 80f), new Vector2(0f, 0f));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        comboText.SetActive(false);

        // === Ingredient Selection Panel ===
        var ingPanel = new GameObject("IngredientPanel", typeof(RectTransform));
        ingPanel.transform.SetParent(canvasObj.transform, false);
        var ingRT = ingPanel.GetComponent<RectTransform>();
        ingRT.anchorMin = new Vector2(0f, 0f); ingRT.anchorMax = new Vector2(1f, 0f);
        ingRT.pivot = new Vector2(0.5f, 0f);
        ingRT.sizeDelta = new Vector2(0f, 700f);
        ingRT.anchoredPosition = new Vector2(0f, 60f);
        var ingBg = ingPanel.AddComponent<Image>();
        ingBg.color = new Color(0.05f, 0.08f, 0.18f, 0.92f);

        // Recipe hint text
        var recipeHint = CT(ingPanel.transform, "RecipeHintText", "ヒント: 星のスープ\n星の粉 + 月光ジュース", 32, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(900f, 90f), new Vector2(0f, -10f));
        recipeHint.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.85f, 1f);
        recipeHint.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Ingredient buttons (2 rows of 3)
        string[] ingNames = { "星の粉", "月光ジュース", "銀河ハーブ", "宇宙塩", "ネビュラソース", "彗星クリーム" };
        string[] ingSprites = { "stardust.png", "moonjuice.png", "galaxyherb.png", "spacesalt.png", "nebulasauce.png", "cometcream.png" };
        Color[] ingColors = {
            new Color(0.4f, 0.35f, 0.1f),
            new Color(0.1f, 0.2f, 0.5f),
            new Color(0.1f, 0.35f, 0.15f),
            new Color(0.25f, 0.28f, 0.35f),
            new Color(0.3f, 0.1f, 0.4f),
            new Color(0.35f, 0.3f, 0.2f)
        };

        var ingButtons = new Button[6];
        var ingNameTexts = new TextMeshProUGUI[6];
        var ingIcons = new Image[6];

        for (int i = 0; i < 6; i++)
        {
            int row = i / 3;
            int col = i % 3;
            float xPos = (col - 1) * 330f;
            float yPos = 530f - row * 200f;

            var btnGo = new GameObject($"IngredientButton_{i}", typeof(RectTransform));
            btnGo.transform.SetParent(ingPanel.transform, false);
            var btnRT = btnGo.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0f); btnRT.anchorMax = new Vector2(0.5f, 0f);
            btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.sizeDelta = new Vector2(280f, 180f);
            btnRT.anchoredPosition = new Vector2(xPos, yPos);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = ingColors[i];
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            // Icon
            Sprite ingSprite = LoadSprite(sp + ingSprites[i]);
            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(btnGo.transform, false);
            var iconRT = iconGo.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.55f); iconRT.anchorMax = new Vector2(0.5f, 0.55f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(80f, 80f);
            iconRT.anchoredPosition = Vector2.zero;
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = ingSprite;
            iconImg.preserveAspect = true;

            // Name label
            var nameGo = new GameObject("NameText", typeof(RectTransform));
            nameGo.transform.SetParent(btnGo.transform, false);
            var nameRT = nameGo.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0f); nameRT.anchorMax = new Vector2(1f, 0.4f);
            nameRT.offsetMin = new Vector2(5f, 5f); nameRT.offsetMax = new Vector2(-5f, 0f);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = ingNames[i];
            nameTmp.fontSize = 28;
            if (jpFont != null) nameTmp.font = jpFont;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = Color.white;

            // Wire onclick
            int capturedI = i;
            btn.onClick.AddListener(() => {
                var cookMgr = Object.FindFirstObjectByType<CookingManager>();
                if (cookMgr != null) cookMgr.OnIngredientButtonTapped(capturedI);
            });

            ingButtons[i] = btn;
            ingNameTexts[i] = nameTmp;
            ingIcons[i] = iconImg;
        }

        // Cook button
        var cookBtn = CB(ingPanel.transform, "CookButton", "調理開始！", 48, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(500f, 80f), new Vector2(0f, 15f), new Color(0.5f, 0.25f, 0.05f));
        cookBtn.GetComponent<Button>().interactable = false;
        cookBtn.GetComponent<Button>().onClick.AddListener(() => {
            var cookMgr = Object.FindFirstObjectByType<CookingManager>();
            if (cookMgr != null) cookMgr.OnCookButtonTapped();
        });

        // === Heating Panel ===
        var heatPanel = new GameObject("HeatingPanel", typeof(RectTransform));
        heatPanel.transform.SetParent(canvasObj.transform, false);
        var heatRT = heatPanel.GetComponent<RectTransform>();
        heatRT.anchorMin = new Vector2(0f, 0.2f); heatRT.anchorMax = new Vector2(1f, 0.75f);
        heatRT.offsetMin = new Vector2(40f, 0f); heatRT.offsetMax = new Vector2(-40f, 0f);
        var heatBg = heatPanel.AddComponent<Image>();
        heatBg.color = new Color(0.08f, 0.05f, 0.15f, 0.95f);

        // Pot icon
        Sprite potSprite = LoadSprite(sp + "pot.png");
        if (potSprite != null)
        {
            var potGo = new GameObject("PotIcon", typeof(RectTransform));
            potGo.transform.SetParent(heatPanel.transform, false);
            var potRT = potGo.GetComponent<RectTransform>();
            potRT.anchorMin = new Vector2(0.5f, 0.6f); potRT.anchorMax = new Vector2(0.5f, 0.6f);
            potRT.pivot = new Vector2(0.5f, 0.5f);
            potRT.sizeDelta = new Vector2(200f, 200f);
            potRT.anchoredPosition = Vector2.zero;
            var potImg = potGo.AddComponent<Image>();
            potImg.sprite = potSprite;
            potImg.preserveAspect = true;
        }

        // Heat gauge label
        var gaugeLabel = CT(heatPanel.transform, "GaugeLabel", "タイミングよくタップ！", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(800f, 50f), new Vector2(0f, -30f));
        gaugeLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gaugeLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.85f, 1f);

        // Heat Gauge Slider
        var heatSliderGo = new GameObject("HeatGauge", typeof(RectTransform));
        heatSliderGo.transform.SetParent(heatPanel.transform, false);
        var heatSliderRT = heatSliderGo.GetComponent<RectTransform>();
        heatSliderRT.anchorMin = new Vector2(0.05f, 0.25f); heatSliderRT.anchorMax = new Vector2(0.95f, 0.45f);
        heatSliderRT.offsetMin = Vector2.zero; heatSliderRT.offsetMax = Vector2.zero;

        var heatSlider = heatSliderGo.AddComponent<Slider>();
        heatSlider.minValue = 0f; heatSlider.maxValue = 1f; heatSlider.value = 0f;
        heatSlider.interactable = false;

        var sliderBgGo = new GameObject("SliderBackground", typeof(RectTransform));
        sliderBgGo.transform.SetParent(heatSliderGo.transform, false);
        var sliderBgRT = sliderBgGo.GetComponent<RectTransform>();
        sliderBgRT.anchorMin = Vector2.zero; sliderBgRT.anchorMax = Vector2.one;
        sliderBgRT.offsetMin = sliderBgRT.offsetMax = Vector2.zero;
        var sliderBgImg = sliderBgGo.AddComponent<Image>();
        sliderBgImg.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        heatSlider.targetGraphic = sliderBgImg;

        var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaGo.transform.SetParent(heatSliderGo.transform, false);
        var fillAreaRT = fillAreaGo.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero; fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = fillAreaRT.offsetMax = Vector2.zero;

        var fillGo = new GameObject("Fill", typeof(RectTransform));
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        var fillRT = fillGo.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color = new Color(1f, 0.5f, 0.1f, 1f);
        heatSlider.fillRect = fillRT;

        // Optimal zone overlay
        var optimalZoneGo = new GameObject("OptimalZone", typeof(RectTransform));
        optimalZoneGo.transform.SetParent(heatSliderGo.transform, false);
        var ozRT = optimalZoneGo.GetComponent<RectTransform>();
        ozRT.anchorMin = new Vector2(0.35f, 0f); ozRT.anchorMax = new Vector2(0.65f, 1f);
        ozRT.offsetMin = ozRT.offsetMax = Vector2.zero;
        var ozImg = optimalZoneGo.AddComponent<Image>();
        ozImg.color = new Color(0.3f, 1f, 0.5f, 0.4f);

        // Tap button (large, center)
        var tapBtn = CB(heatPanel.transform, "TapButton", "タップ！", 56, jpFont,
            new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0f),
            new Vector2(600f, 100f), new Vector2(0f, 10f), new Color(0.6f, 0.2f, 0.05f));

        // Result text (shown briefly after tap)
        var resultText = CT(heatPanel.transform, "ResultText", "", 60, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(600f, 120f), new Vector2(0f, 0f));
        resultText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        resultText.SetActive(false);

        heatPanel.SetActive(false);

        // === Back to Menu button ===
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニューへ", 36, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(180f, 55f), new Vector2(10f, 10f), new Color(0.1f, 0.1f, 0.2f, 0.85f));
        var backComp = backBtn.AddComponent<BackToMenuButton>();

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.1f, 0.3f); scRT.anchorMax = new Vector2(0.9f, 0.7f);
        scRT.offsetMin = scRT.offsetMax = Vector2.zero;
        var scBg = scPanel.AddComponent<Image>();
        scBg.color = new Color(0.05f, 0.1f, 0.25f, 0.97f);

        var scTitle = CT(scPanel.transform, "StageClearText", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(700f, 90f), new Vector2(0f, 0f));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var scStageNum = CT(scPanel.transform, "StageNumberText", "Stage 1 クリア！", 44, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(600f, 70f), new Vector2(0f, 0f));
        scStageNum.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStageNum.GetComponent<TextMeshProUGUI>().color = Color.white;

        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 44, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 75f), new Vector2(0f, 0f), new Color(0.1f, 0.3f, 0.5f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.05f, 0.2f); acRT.anchorMax = new Vector2(0.95f, 0.8f);
        acRT.offsetMin = acRT.offsetMax = Vector2.zero;
        var acBg = acPanel.AddComponent<Image>();
        acBg.color = new Color(0.05f, 0.1f, 0.25f, 0.97f);

        var acTitle = CT(acPanel.transform, "AllClearTitle", "レストラン★5達成！", 56, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(800f, 90f), new Vector2(0f, 0f));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var acScore = CT(acPanel.transform, "AllClearScore", "最終スコア: 0", 48, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(700f, 70f), new Vector2(0f, 0f));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var acBack = CB(acPanel.transform, "BackButton", "メニューへ戻る", 44, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(460f, 75f), new Vector2(0f, 0f), new Color(0.1f, 0.2f, 0.4f));
        acBack.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.05f, 0.2f); goRT.anchorMax = new Vector2(0.95f, 0.8f);
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;
        var goBg = goPanel.AddComponent<Image>();
        goBg.color = new Color(0.2f, 0.05f, 0.05f, 0.97f);

        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 60, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(700f, 90f), new Vector2(0f, 0f));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScore = CT(goPanel.transform, "GameOverScore", "スコア: 0", 48, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f),
            new Vector2(600f, 70f), new Vector2(0f, 0f));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var goBack = CB(goPanel.transform, "BackButton", "メニューへ戻る", 44, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(460f, 75f), new Vector2(0f, 0f), new Color(0.3f, 0.1f, 0.1f));
        goBack.AddComponent<BackToMenuButton>();
        goPanel.SetActive(false);

        // === InstructionPanel ===
        var ipCanvasGo = new GameObject("InstructionCanvas");
        var ipCanvas = ipCanvasGo.AddComponent<Canvas>();
        ipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvas.sortingOrder = 100;
        var ipScaler = ipCanvasGo.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvasGo.AddComponent<GraphicRaycaster>();

        var ipBg = new GameObject("InstructionPanel", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvasGo.transform, false);
        var ipBgRT = ipBg.GetComponent<RectTransform>();
        ipBgRT.anchorMin = Vector2.zero; ipBgRT.anchorMax = Vector2.one;
        ipBgRT.offsetMin = ipBgRT.offsetMax = Vector2.zero;
        var ipBgImg = ipBg.AddComponent<Image>();
        ipBgImg.color = new Color(0.05f, 0.03f, 0.12f, 0.96f);
        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "StarChef", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900f, 100f), new Vector2(0f, 0f));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900f, 90f), new Vector2(0f, 0f));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 34, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900f, 140f), new Vector2(0f, 0f));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 34, jpFont,
            new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.5f),
            new Vector2(900f, 90f), new Vector2(0f, 0f));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.6f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(400f, 75f), new Vector2(0f, 0f), new Color(0.3f, 0.15f, 0.05f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65f, 65f), new Vector2(-15f, 290f), new Color(0.1f, 0.1f, 0.2f, 0.9f));

        // === StarChefUI ===
        var uiObj = new GameObject("StarChefUI");
        var ui = uiObj.AddComponent<StarChefUI>();

        SetField(ui, "_stageText",   stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",   scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",   comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_failText",    failText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearStageText", scStageNum.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton",    nextBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",      acPanel);
        SetField(ui, "_allClearScoreText",  acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",      goPanel);
        SetField(ui, "_gameOverScoreText",  goScore.GetComponent<TextMeshProUGUI>());

        // Wire Next Stage button
        nextBtn.GetComponent<Button>().onClick.AddListener(() => {
            var gmgr = Object.FindFirstObjectByType<StarChefGameManager>();
            if (gmgr != null) gmgr.NextStage();
        });

        // Wire CookingManager UI references
        SetField(cm, "_heatGauge",           heatSlider);
        SetField(cm, "_optimalZone",         ozImg);
        SetField(cm, "_tapButton",           tapBtn.GetComponent<Button>());
        SetField(cm, "_resultText",          resultText.GetComponent<TextMeshProUGUI>());
        SetField(cm, "_ingredientPanel",     ingPanel);
        SetField(cm, "_ingredientButtons",   ingButtons);
        SetField(cm, "_ingredientNameTexts", ingNameTexts);
        SetField(cm, "_ingredientIcons",     ingIcons);
        SetField(cm, "_cookButton",          cookBtn.GetComponent<Button>());
        SetField(cm, "_recipeHintText",      recipeHint.GetComponent<TextMeshProUGUI>());
        SetField(cm, "_heatingPanel",        heatPanel);

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_cookingManager",   cm);
        SetField(gm, "_ui",               ui);

        // Wire InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/083v2_StarChef.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup083v2] StarChef シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup083v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
