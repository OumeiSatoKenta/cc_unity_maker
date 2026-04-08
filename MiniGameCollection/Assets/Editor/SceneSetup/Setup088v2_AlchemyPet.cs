using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game088v2_AlchemyPet;

public static class Setup088v2_AlchemyPet
{
    [MenuItem("Assets/Setup/088v2 AlchemyPet")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup088v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game088v2_AlchemyPet/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.12f, 0.08f, 0.06f);
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

        // Load material sprites
        string[] matNames = {
            "fire", "water", "earth", "wind",
            "flame", "ice", "sand", "storm",
            "rock", "thunder", "poison", "light",
            "bomb", "powder", "stardust", "scale"
        };
        var materialSprites = new Sprite[matNames.Length];
        for (int i = 0; i < matNames.Length; i++)
            materialSprites[i] = LoadSprite(sp + $"material_{matNames[i]}.png");

        // Load pet sprites
        string[] petNames = {
            "salamander","phoenix","golem","aquadrake","windbird",
            "frostcat","rocktor","thunderwolf","poisonfox","lightbun",
            "stormeagle","flamelion","earthboar","icepanda","sandcrab",
            "rarebear","voidcat","crystalbird","magmadragon","shadowwolf",
            "starphoenix","legenddrake","ancientowl","voiddragon","cosmiccat"
        };
        var petSprites = new Sprite[petNames.Length];
        for (int i = 0; i < petNames.Length; i++)
            petSprites[i] = LoadSprite(sp + $"pet_{petNames[i]}.png");

        Sprite slotEmptySprite = LoadSprite(sp + "slot_empty.png");
        Sprite slotFilledSprite = LoadSprite(sp + "slot_filled.png");
        Sprite silhouetteSprite = LoadSprite(sp + "pet_silhouette.png");
        Sprite cauldronSprite = LoadSprite(sp + "cauldron.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("AlchemyPetGameManager");
        var gm = gmObj.AddComponent<AlchemyPetGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f,  stageName = "Stage 1" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.25f, stageName = "Stage 2" },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 2, complexityFactor = 0.5f,  stageName = "Stage 3" },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.75f, stageName = "Stage 4" },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 1.0f,  stageName = "Stage 5" },
        };
        sm.SetConfigs(stageConfigs);

        // AlchemyManager
        var amObj = new GameObject("AlchemyManager");
        amObj.transform.SetParent(gmObj.transform);
        var am = amObj.AddComponent<AlchemyManager>();

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
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 40, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300, 50), new Vector2(15, -15));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 0.5f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 40, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300, 50), new Vector2(-15, -15));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 0.5f);

        var missText = CT(canvasObj.transform, "MissText", "失敗: 0/3", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(240, 45), new Vector2(15, -62));
        missText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var goalText = CT(canvasObj.transform, "GoalText", "図鑑: 0/2", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(240, 45), new Vector2(-15, -62));
        goalText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        goalText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var comboText = CT(canvasObj.transform, "ComboText", "", 38, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 50), new Vector2(0, -110));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var messageText = CT(canvasObj.transform, "MessageText", "", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 60), new Vector2(0, 200));
        messageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        messageText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.3f);

        // === Alchemy Area (center) ===
        // Cauldron display
        var cauldronObj = new GameObject("CauldronImage", typeof(RectTransform));
        cauldronObj.transform.SetParent(canvasObj.transform, false);
        var cauldronRT = cauldronObj.GetComponent<RectTransform>();
        cauldronRT.anchorMin = new Vector2(0.5f, 0.5f); cauldronRT.anchorMax = new Vector2(0.5f, 0.5f);
        cauldronRT.pivot = new Vector2(0.5f, 0.5f);
        cauldronRT.sizeDelta = new Vector2(160, 160);
        cauldronRT.anchoredPosition = new Vector2(0f, 100f);
        var cauldronImg = cauldronObj.AddComponent<Image>();
        if (cauldronSprite != null) cauldronImg.sprite = cauldronSprite;
        cauldronImg.preserveAspect = true;

        // Slot container (horizontal, above cauldron center)
        var slotContainer = new GameObject("SlotContainer", typeof(RectTransform));
        slotContainer.transform.SetParent(canvasObj.transform, false);
        var slotContainerRT = slotContainer.GetComponent<RectTransform>();
        slotContainerRT.anchorMin = new Vector2(0.5f, 0.5f); slotContainerRT.anchorMax = new Vector2(0.5f, 0.5f);
        slotContainerRT.pivot = new Vector2(0.5f, 0.5f);
        slotContainerRT.sizeDelta = new Vector2(600, 120);
        slotContainerRT.anchoredPosition = new Vector2(0f, 300f);

        const int maxSlots = 3;
        var slotObjects = new GameObject[maxSlots];
        var slotImages = new Image[maxSlots];
        float[] slotXPos = { -200f, 0f, 200f };

        for (int i = 0; i < maxSlots; i++)
        {
            var slotGo = new GameObject($"Slot{i}", typeof(RectTransform));
            slotGo.transform.SetParent(slotContainer.transform, false);
            var slotRT = slotGo.GetComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0.5f, 0.5f); slotRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotRT.pivot = new Vector2(0.5f, 0.5f);
            slotRT.sizeDelta = new Vector2(110, 110);
            slotRT.anchoredPosition = new Vector2(slotXPos[i], 0f);

            var slotBg = slotGo.AddComponent<Image>();
            slotBg.sprite = slotEmptySprite;
            slotBg.color = new Color(0.5f, 0.3f, 0.1f, 0.5f);

            slotObjects[i] = slotGo;
            slotImages[i] = slotBg;

            // Add slot label
            var slotLabel = CT(slotGo.transform, "SlotLabel", $"スロット{i+1}", 20, jpFont,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f),
                new Vector2(100, 24), new Vector2(0, -2));
            slotLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            slotLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.6f, 0.4f);
        }

        // Combine button (錬金釜ボタン)
        var combineBtn = CB(canvasObj.transform, "CombineButton", "錬金！", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 70), new Vector2(0, 180), new Color(0.9f, 0.6f, 0.1f));
        // onClick wired in AlchemyManager.Start()

        // === Pet Area (right side) ===
        var petContainer = new GameObject("PetContainer", typeof(RectTransform));
        petContainer.transform.SetParent(canvasObj.transform, false);
        var petContainerRT = petContainer.GetComponent<RectTransform>();
        petContainerRT.anchorMin = new Vector2(0.7f, 0.5f); petContainerRT.anchorMax = new Vector2(0.7f, 0.5f);
        petContainerRT.pivot = new Vector2(0.5f, 0.5f);
        petContainerRT.sizeDelta = new Vector2(250, 300);
        petContainerRT.anchoredPosition = new Vector2(0f, 50f);

        var petBg = new GameObject("PetBackground", typeof(RectTransform));
        petBg.transform.SetParent(petContainer.transform, false);
        var petBgRT = petBg.GetComponent<RectTransform>();
        petBgRT.anchorMin = Vector2.zero; petBgRT.anchorMax = Vector2.one;
        petBgRT.offsetMin = petBgRT.offsetMax = Vector2.zero;
        var petBgImg = petBg.AddComponent<Image>();
        petBgImg.color = new Color(0.15f, 0.10f, 0.08f, 0.85f);

        var petImgGo = new GameObject("PetImage", typeof(RectTransform));
        petImgGo.transform.SetParent(petContainer.transform, false);
        var petImgRT = petImgGo.GetComponent<RectTransform>();
        petImgRT.anchorMin = new Vector2(0.5f, 0.6f); petImgRT.anchorMax = new Vector2(0.5f, 0.6f);
        petImgRT.pivot = new Vector2(0.5f, 0.5f);
        petImgRT.sizeDelta = new Vector2(120, 120);
        petImgRT.anchoredPosition = Vector2.zero;
        var petImg = petImgGo.AddComponent<Image>();
        petImg.color = new Color(0f, 0f, 0f, 0f);
        petImg.preserveAspect = true;

        var petNameGo = CT(petContainer.transform, "PetNameText", "", 28, jpFont,
            new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 40), Vector2.zero);
        petNameGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        petNameGo.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.7f, 0.5f);

        var petLevelGo = CT(petContainer.transform, "PetLevelText", "", 26, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 35), Vector2.zero);
        petLevelGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        petLevelGo.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.9f, 0.5f);

        var feedBtn = CB(petContainer.transform, "FeedButton", "餌やり", 30, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(160, 55), new Vector2(0, 10), new Color(0.35f, 0.6f, 0.25f));
        feedBtn.GetComponent<Button>().interactable = false;
        // onClick wired in AlchemyManager.Start()

        // === Inventory Panel (left side + bottom) ===
        var invContainer = new GameObject("InventoryContainer", typeof(RectTransform));
        invContainer.transform.SetParent(canvasObj.transform, false);
        var invRT = invContainer.GetComponent<RectTransform>();
        invRT.anchorMin = new Vector2(0f, 0f); invRT.anchorMax = new Vector2(0f, 0f);
        invRT.pivot = new Vector2(0f, 0f);
        invRT.sizeDelta = new Vector2(1080, 360);
        invRT.anchoredPosition = new Vector2(0f, 80f);
        var invBg = invContainer.AddComponent<Image>();
        invBg.color = new Color(0.10f, 0.07f, 0.05f, 0.85f);

        // Material buttons (4 per row, up to 16 total)
        int matBtnCount = 16;
        var matButtons = new Button[matBtnCount];
        var matButtonImages = new Image[matBtnCount];
        var matCountTexts = new TextMeshProUGUI[matBtnCount];

        float btnW = 240f;
        float btnH = 80f;
        float btnPadX = 20f;
        float btnPadY = 15f;
        float startX = 20f;
        float startY = -20f;

        string[] matDisplayNames = {
            "火", "水", "土", "風",
            "炎", "氷", "砂", "嵐",
            "岩", "雷", "毒", "光",
            "爆薬", "禁断", "星屑", "鱗"
        };

        for (int i = 0; i < matBtnCount; i++)
        {
            int col = i % 4;
            int row = i / 4;
            float bx = startX + col * (btnW + btnPadX);
            float by = startY - row * (btnH + btnPadY);

            var btnGo = new GameObject($"MatBtn_{i:D2}", typeof(RectTransform));
            btnGo.transform.SetParent(invContainer.transform, false);
            var btnRT = btnGo.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0f, 1f); btnRT.anchorMax = new Vector2(0f, 1f);
            btnRT.pivot = new Vector2(0f, 1f);
            btnRT.sizeDelta = new Vector2(btnW, btnH);
            btnRT.anchoredPosition = new Vector2(bx, by);

            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.25f, 0.18f, 0.12f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btnGo.SetActive(i < 4); // Only Stage1 materials visible initially

            // Material icon
            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(btnGo.transform, false);
            var iconRT = iconGo.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0f, 0.5f); iconRT.anchorMax = new Vector2(0f, 0.5f);
            iconRT.pivot = new Vector2(0f, 0.5f);
            iconRT.sizeDelta = new Vector2(60, 60);
            iconRT.anchoredPosition = new Vector2(10f, 0f);
            var iconImg = iconGo.AddComponent<Image>();
            if (materialSprites[i] != null) iconImg.sprite = materialSprites[i];
            iconImg.preserveAspect = true;

            // Name text
            var nameGo = CT(btnGo.transform, "NameText", matDisplayNames[i], 26, jpFont,
                new Vector2(0f, 0.6f), new Vector2(1f, 0.6f), new Vector2(0f, 0.5f),
                new Vector2(0, 30), new Vector2(80, 0));
            nameGo.GetComponent<TextMeshProUGUI>().color = Color.white;

            // Count text
            var cntGo = CT(btnGo.transform, "CountText", "5", 28, jpFont,
                new Vector2(1f, 0.4f), new Vector2(1f, 0.4f), new Vector2(1f, 0.5f),
                new Vector2(80, 30), new Vector2(-10, 0));
            cntGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
            cntGo.GetComponent<TextMeshProUGUI>().color = Color.white;

            matButtons[i] = btn;
            matButtonImages[i] = iconImg;
            matCountTexts[i] = cntGo.GetComponent<TextMeshProUGUI>();

            // Button click: select for current active slot (default slot 0)
            // Runtime wiring done in AlchemyManager.Start() via _materialButtons array
        }

        // Slot clear buttons — wired to AlchemyManager.ClearSlotN() via component method
        // Buttons are collected and passed to AlchemyManager for runtime wiring in Start()
        var clearSlotButtons = new Button[maxSlots];
        for (int s = 0; s < maxSlots; s++)
        {
            var clearBtn = CB(slotContainer.transform, $"ClearSlot{s}Btn", "×", 28, jpFont,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f),
                new Vector2(36, 36), new Vector2(slotXPos[s] + 42f, 0f), new Color(0.5f, 0.1f, 0.1f));
            clearSlotButtons[s] = clearBtn.GetComponent<Button>();
        }

        // === Discovery Panel ===
        var discPanel = new GameObject("DiscoveryPanel", typeof(RectTransform));
        discPanel.transform.SetParent(canvasObj.transform, false);
        var discRT = discPanel.GetComponent<RectTransform>();
        discRT.anchorMin = new Vector2(0.5f, 0.5f); discRT.anchorMax = new Vector2(0.5f, 0.5f);
        discRT.pivot = new Vector2(0.5f, 0.5f);
        discRT.sizeDelta = new Vector2(600, 400);
        discRT.anchoredPosition = Vector2.zero;
        var discBg = discPanel.AddComponent<Image>();
        discBg.color = new Color(0.1f, 0.08f, 0.06f, 0.97f);

        var discTitle = CT(discPanel.transform, "DiscTitle", "新発見！", 60, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(560, 80), new Vector2(0, -20));
        discTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        discTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var discPetImgGo = new GameObject("DiscPetImage", typeof(RectTransform));
        discPetImgGo.transform.SetParent(discPanel.transform, false);
        var discPetRT = discPetImgGo.GetComponent<RectTransform>();
        discPetRT.anchorMin = new Vector2(0.5f, 0.5f); discPetRT.anchorMax = new Vector2(0.5f, 0.5f);
        discPetRT.pivot = new Vector2(0.5f, 0.5f);
        discPetRT.sizeDelta = new Vector2(150, 150);
        discPetRT.anchoredPosition = new Vector2(0f, 30f);
        var discPetImg = discPetImgGo.AddComponent<Image>();
        discPetImg.preserveAspect = true;

        var discScoreGo = CT(discPanel.transform, "DiscScoreText", "+20pt", 44, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(560, 60), new Vector2(0, 20));
        discScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        discScoreGo.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.5f);
        discPanel.SetActive(false);

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 340);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.15f, 0.10f, 0.08f, 0.97f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 0.4f);

        var scStageText = CT(scPanel.transform, "SCStageText", "Stage 1 Clear!", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 60), new Vector2(0, 20));
        scStageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStageText.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.8f, 0.7f);

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(400, 65), new Vector2(0, 50), new Color(0.5f, 0.35f, 0.15f));
        nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.5f, 0.5f); acRT.anchorMax = new Vector2(0.5f, 0.5f);
        acRT.pivot = new Vector2(0.5f, 0.5f);
        acRT.sizeDelta = new Vector2(700, 380);
        acRT.anchoredPosition = Vector2.zero;
        var acImg = acPanel.AddComponent<Image>();
        acImg.color = new Color(0.12f, 0.09f, 0.06f, 0.97f);

        var acTitle = CT(acPanel.transform, "ACTitle", "図鑑\nコンプリート！", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 130), new Vector2(0, -25));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var acScore = CT(acPanel.transform, "ACScore", "Final Score: 0", 46, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), new Vector2(0, 25));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.8f, 0.7f);

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 40, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(320, 60), new Vector2(0, 50), new Color(0.35f, 0.25f, 0.18f));
        acBack.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT2 = goPanel.GetComponent<RectTransform>();
        goRT2.anchorMin = new Vector2(0.5f, 0.5f); goRT2.anchorMax = new Vector2(0.5f, 0.5f);
        goRT2.pivot = new Vector2(0.5f, 0.5f);
        goRT2.sizeDelta = new Vector2(700, 340);
        goRT2.anchoredPosition = Vector2.zero;
        var goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.22f, 0.06f, 0.04f, 0.97f);

        var goTitle = CT(goPanel.transform, "GOTitle", "ゲームオーバー", 56, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 80), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.2f);

        var goSubTitle = CT(goPanel.transform, "GOSubTitle", "釜が壊れました...", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 50), Vector2.zero);
        goSubTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goSubTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.6f, 0.6f);

        var goBack = CB(goPanel.transform, "GOBackButton", "メニューへ", 40, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 60), Vector2.zero, new Color(0.4f, 0.15f, 0.1f));
        goBack.AddComponent<BackToMenuButton>();
        goPanel.SetActive(false);

        // === Back + Help Buttons ===
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 32, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(220, 55), new Vector2(15, 15), new Color(0.25f, 0.18f, 0.12f, 0.9f));
        backBtn.AddComponent<BackToMenuButton>();

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 42, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(60, 60), new Vector2(-15, 360), new Color(0.3f, 0.22f, 0.18f, 0.9f));

        // === UI Component ===
        var uiGo = new GameObject("AlchemyPetUI");
        var ui = uiGo.AddComponent<AlchemyPetUI>();

        SetField(ui, "_stageText", stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText", scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText", comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_missText", missText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_goalText", goalText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_messageText", messageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_slotImages", slotImages);
        SetField(ui, "_slotObjects", slotObjects);
        SetField(ui, "_combineButton", combineBtn.GetComponent<Button>());
        SetField(ui, "_petImage", petImg);
        SetField(ui, "_petNameText", petNameGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_petLevelText", petLevelGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_feedButton", feedBtn.GetComponent<Button>());
        SetField(ui, "_stageClearPanel", scPanel);
        SetField(ui, "_stageClearText", scStageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel", goPanel);
        SetField(ui, "_allClearPanel", acPanel);
        SetField(ui, "_allClearScoreText", acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_discoveryPanel", discPanel);
        SetField(ui, "_discoveryPetImage", discPetImg);
        SetField(ui, "_discoveryScoreText", discScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_materialButtons", matButtons);
        SetField(ui, "_materialButtonImages", matButtonImages);
        SetField(ui, "_materialCountTexts", matCountTexts);

        // Wire AlchemyManager
        SetField(am, "_gameManager", gm);
        SetField(am, "_ui", ui);
        SetField(am, "_materialButtons", matButtons);
        SetField(am, "_clearSlotButtons", clearSlotButtons);
        SetField(am, "_combineButton", combineBtn.GetComponent<Button>());
        SetField(am, "_feedButton", feedBtn.GetComponent<Button>());

        // Initialize AlchemyManager with sprites
        am.Initialize(petSprites, materialSprites, silhouetteSprite);

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
        var ipBgRT = ipBg.GetComponent<RectTransform>();
        ipBgRT.anchorMin = Vector2.zero; ipBgRT.anchorMax = Vector2.one;
        ipBgRT.offsetMin = ipBgRT.offsetMax = Vector2.zero;
        var ipBgImg = ipBg.AddComponent<Image>();
        ipBgImg.color = new Color(0.12f, 0.08f, 0.06f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "AlchemyPet", 68, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.75f, 0.3f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 38, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.75f, 0.65f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 32, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 150), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.8f, 0.7f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 32, jpFont,
            new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 50, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 70), Vector2.zero, new Color(0.5f, 0.35f, 0.2f));

        // Wire InstructionPanel
        SetField(ip, "_titleText", ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText", ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText", ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton", startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton", helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot", ipBg);

        // Wire GameManager
        SetField(gm, "_stageManager", sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_alchemyManager", am);
        SetField(gm, "_ui", ui);

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/088v2_AlchemyPet.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup088v2] AlchemyPet シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup088v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
