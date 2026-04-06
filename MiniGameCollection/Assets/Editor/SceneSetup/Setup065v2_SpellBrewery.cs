using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game065v2_SpellBrewery;

public static class Setup065v2_SpellBrewery
{
    [MenuItem("Assets/Setup/065v2 SpellBrewery")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup065v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game065v2_SpellBrewery/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.02f, 0.1f);
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
            bgObj.transform.localScale = new Vector3(0.024f, 0.012f, 1f);
        }

        // Cauldron (world object, center)
        Sprite cauldronSprite = LoadSprite(sp + "Cauldron.png");
        var cauldronObj = new GameObject("Cauldron");
        var cauldronSR = cauldronObj.AddComponent<SpriteRenderer>();
        cauldronSR.sprite = cauldronSprite;
        cauldronSR.sortingOrder = 5;
        cauldronObj.transform.position = new Vector3(0f, 0.5f, 0f);
        cauldronObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

        // Load ingredient sprites
        Sprite[] ingredientSprites = new Sprite[]
        {
            LoadSprite(sp + "Ingredient_Fire.png"),
            LoadSprite(sp + "Ingredient_Water.png"),
            LoadSprite(sp + "Ingredient_Earth.png"),
            LoadSprite(sp + "Ingredient_Air.png"),
            LoadSprite(sp + "Ingredient_Light.png"),
        };

        // Load potion sprites
        Sprite[] potionSprites = new Sprite[]
        {
            LoadSprite(sp + "Potion_Fire.png"),
            LoadSprite(sp + "Potion_Water.png"),
            LoadSprite(sp + "Potion_Earth.png"),
            LoadSprite(sp + "Potion_Air.png"),
            LoadSprite(sp + "Potion_Light.png"),
            LoadSprite(sp + "Potion_Storm.png"),
            LoadSprite(sp + "Potion_Nature.png"),
            LoadSprite(sp + "Potion_Legendary.png"),
        };

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<SpellBreweryGameManager>();

        // StageManager (child of GameManager)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        var stages = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 0.0f, countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.1f },
            new StageManager.StageConfig { speedMultiplier = 1.4f, countMultiplier = 2, complexityFactor = 0.2f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 0.3f },
            new StageManager.StageConfig { speedMultiplier = 3.0f, countMultiplier = 5, complexityFactor = 0.5f },
        };
        sm.SetConfigs(stages);

        // BreweryManager (child of GameManager)
        var bmObj = new GameObject("BreweryManager");
        bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<BreweryManager>();

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
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.7f, 1f);

        // HUD: Gold (top left)
        var goldText = CT(canvasObj.transform, "GoldText", "💰 0G", 34, jpFont,
            new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(0f, 1), new Vector2(420, 55), new Vector2(15, -90));
        goldText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        goldText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // HUD: Target (top right)
        var targetText = CT(canvasObj.transform, "TargetText", "目標: 100G", 34, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(380, 55), new Vector2(-15, -90));
        targetText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        targetText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.8f);

        // Combo text
        var comboText = CT(canvasObj.transform, "ComboText", "🔥 COMBO x2!", 44, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(600, 65), new Vector2(0, -155));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.SetActive(false);

        // New element announcement text
        var newElementText = CT(canvasObj.transform, "NewElementText", "", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(900, 60), new Vector2(0, -210));
        newElementText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        newElementText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        newElementText.SetActive(false);

        // Cauldron slots display
        var cauldronSlotsText = CT(canvasObj.transform, "CauldronSlotsText", "（空）", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 60), new Vector2(0, 200));
        cauldronSlotsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        cauldronSlotsText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Brewing indicator
        var brewingIndicator = CT(canvasObj.transform, "BrewingIndicator", "✨ 醸造中...", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 60), new Vector2(0, 130));
        brewingIndicator.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        brewingIndicator.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);
        brewingIndicator.SetActive(false);

        // Brew button
        var brewBtn = CB(canvasObj.transform, "BrewButton", "🧪 醸造", 32, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(220, 75), new Vector2(-130, 850),
            new Color(0.4f, 0.2f, 0.7f));
        brewBtn.SetActive(false);

        // Clear cauldron button
        var clearBtn = CB(canvasObj.transform, "ClearCauldronButton", "🗑 クリア", 30, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(180, 75), new Vector2(110, 850),
            new Color(0.5f, 0.2f, 0.2f));

        // Sell all button
        var sellAllBtn = CB(canvasObj.transform, "SellAllButton", "💰 全て販売", 30, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(250, 75), new Vector2(0, 760),
            new Color(0.2f, 0.5f, 0.2f));

        // Ingredient buttons (5, bottom row)
        string[] ingLabels = { "🔥\nFire", "💧\nWater", "🌿\nEarth", "💨\nAir", "✨\nLight" };
        Color[] ingColors = {
            new Color(0.7f, 0.2f, 0.1f),
            new Color(0.1f, 0.3f, 0.7f),
            new Color(0.2f, 0.5f, 0.1f),
            new Color(0.3f, 0.6f, 0.7f),
            new Color(0.6f, 0.5f, 0.1f)
        };
        float[] ingX = { -400f, -200f, 0f, 200f, 400f };
        var ingredientButtons = new GameObject[5];
        var ingredientCountTexts = new TextMeshProUGUI[5];
        var ingredientNameTexts = new TextMeshProUGUI[5];

        for (int i = 0; i < 5; i++)
        {
            var btn = CB(canvasObj.transform, $"IngredientButton_{i}", "", 28, jpFont,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(150, 140), new Vector2(ingX[i], 660),
                ingColors[i]);
            ingredientButtons[i] = btn;

            // Name text (top half)
            var nameGo = CT(btn.transform, "NameText", ingLabels[i], 22, jpFont,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(140, 80), new Vector2(0, 0));
            nameGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            ingredientNameTexts[i] = nameGo.GetComponent<TextMeshProUGUI>();

            // Count text (bottom half)
            var countGo = CT(btn.transform, "CountText", "5", 30, jpFont,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(140, 55), new Vector2(0, 5));
            countGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            countGo.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            ingredientCountTexts[i] = countGo.GetComponent<TextMeshProUGUI>();
        }
        // Air and Light hidden initially (stage 2+)
        ingredientButtons[3].SetActive(false);
        ingredientButtons[4].SetActive(false);

        // Potion inventory (8 slots, 2 rows of 4)
        string[] potionLabels = { "🔥Fire\n×0", "💧Water\n×0", "🌿Earth\n×0", "💨Air\n×0",
                                  "✨Light\n×0", "⚡Storm\n×0", "🍃Nature\n×0", "🌟Legend\n×0" };
        Color[] potionColors = {
            new Color(0.6f,0.15f,0.1f), new Color(0.1f,0.25f,0.6f), new Color(0.15f,0.4f,0.1f), new Color(0.2f,0.5f,0.6f),
            new Color(0.5f,0.4f,0.1f), new Color(0.3f,0.1f,0.6f), new Color(0.1f,0.45f,0.25f), new Color(0.4f,0.1f,0.4f)
        };
        var potionSlots = new GameObject[8];
        var potionCountTexts = new TextMeshProUGUI[8];
        var potionSellButtons = new Button[8];
        float[] pX = { -380f, -190f, 0f, 190f, -380f, -190f, 0f, 190f };
        float[] pY = { 480f, 480f, 480f, 480f, 380f, 380f, 380f, 380f };

        for (int i = 0; i < 8; i++)
        {
            var pSlot = CB(canvasObj.transform, $"PotionSlot_{i}", potionLabels[i], 20, jpFont,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(175, 80), new Vector2(pX[i], pY[i]), potionColors[i]);
            potionSlots[i] = pSlot;
            potionCountTexts[i] = pSlot.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            potionSellButtons[i] = pSlot.GetComponent<Button>();
            pSlot.SetActive(false);
        }

        // Order panel
        var orderPanel = new GameObject("OrderPanel", typeof(RectTransform));
        orderPanel.transform.SetParent(canvasObj.transform, false);
        var opRT = orderPanel.GetComponent<RectTransform>();
        opRT.anchorMin = new Vector2(0f, 0); opRT.anchorMax = new Vector2(1f, 0);
        opRT.pivot = new Vector2(0.5f, 0);
        opRT.sizeDelta = new Vector2(0, 100);
        opRT.anchoredPosition = new Vector2(0, 280);
        var opImg = orderPanel.AddComponent<Image>();
        opImg.color = new Color(0.4f, 0.25f, 0.0f, 0.9f);

        var orderText = CT(orderPanel.transform, "OrderText", "📜 注文: Fire Potion", 30, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(700, 50), Vector2.zero);
        orderText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        orderText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.4f);

        var orderBonusText = CT(orderPanel.transform, "OrderBonusText", "報酬: 60G (2倍！)", 26, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(500, 40), Vector2.zero);
        orderBonusText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        orderBonusText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);

        // Order timer bar
        var timerBarObj = new GameObject("OrderTimerBar", typeof(RectTransform));
        timerBarObj.transform.SetParent(orderPanel.transform, false);
        var timerBarRT = timerBarObj.GetComponent<RectTransform>();
        timerBarRT.anchorMin = new Vector2(0.05f, 0); timerBarRT.anchorMax = new Vector2(0.95f, 0);
        timerBarRT.pivot = new Vector2(0.5f, 0);
        timerBarRT.sizeDelta = new Vector2(0, 8);
        timerBarRT.anchoredPosition = new Vector2(0, 5);
        var timerBarImg = timerBarObj.AddComponent<Image>();
        timerBarImg.color = new Color(1f, 0.5f, 0f);
        timerBarImg.type = Image.Type.Filled;
        timerBarImg.fillMethod = Image.FillMethod.Horizontal;
        timerBarImg.fillAmount = 1f;
        orderPanel.SetActive(false);

        // Menu button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 28, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(180, 65), new Vector2(20, 200),
            new Color(0.3f, 0.2f, 0.4f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Stage Clear Panel
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scpRT = scPanel.GetComponent<RectTransform>();
        scpRT.anchorMin = new Vector2(0.1f, 0.3f); scpRT.anchorMax = new Vector2(0.9f, 0.7f);
        scpRT.offsetMin = scpRT.offsetMax = Vector2.zero;
        var scpImg = scPanel.AddComponent<Image>();
        scpImg.color = new Color(0.08f, 0.04f, 0.18f, 0.95f);

        var scText = CT(scPanel.transform, "StageClearText", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(700, 80), Vector2.zero);
        scText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        var nextStageBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 36, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(420, 85), Vector2.zero,
            new Color(0.3f, 0.15f, 0.6f));
        scPanel.SetActive(false);

        // All Clear Panel
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acpRT = acPanel.GetComponent<RectTransform>();
        acpRT.anchorMin = new Vector2(0.1f, 0.25f); acpRT.anchorMax = new Vector2(0.9f, 0.75f);
        acpRT.offsetMin = acpRT.offsetMax = Vector2.zero;
        var acpImg = acPanel.AddComponent<Image>();
        acpImg.color = new Color(0.05f, 0.02f, 0.15f, 0.97f);

        var acText = CT(acPanel.transform, "AllClearText", "✨ SpellBrewery 完全制覇！", 42, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(900, 100), Vector2.zero);
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

        var ipTitle = CT(ipBg.transform, "TitleText", "SpellBrewery", 64, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(900, 90), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.6f, 1f);

        var ipDesc = CT(ipBg.transform, "DescriptionText", "材料を集めて魔法のポーションを作ろう", 38, jpFont,
            new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ipCtrl = CT(ipBg.transform, "ControlsText", "材料をタップして釜に投入\n醸造ボタンでポーション完成\n販売してゴールド獲得", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900, 120), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.7f);

        var ipGoal = CT(ipBg.transform, "GoalText", "ポーション販売目標を達成してステージクリア", 32, jpFont,
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(900, 65), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.6f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 44, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(380, 90), Vector2.zero,
            new Color(0.3f, 0.15f, 0.65f));

        // Help button
        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(80, 80), new Vector2(-20, 200),
            new Color(0.2f, 0.2f, 0.4f));

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- SpellBreweryUI ---
        var uiObj = new GameObject("SpellBreweryUI");
        uiObj.transform.SetParent(canvasObj.transform, false);
        var ui = uiObj.AddComponent<SpellBreweryUI>();

        // Wire SpellBreweryUI fields
        SetField(ui, "_gameManager", gm);
        SetField(ui, "_breweryManager", bm);
        SetField(ui, "_stageText", stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_goldText", goldText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_targetText", targetText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText", comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_newElementText", newElementText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_ingredientButtons", GetButtons(ingredientButtons));
        SetField(ui, "_ingredientCountTexts", ingredientCountTexts);
        SetField(ui, "_ingredientNameTexts", ingredientNameTexts);
        SetField(ui, "_cauldronSlotsText", cauldronSlotsText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_brewButton", brewBtn.GetComponent<Button>());
        SetField(ui, "_clearCauldronButton", clearBtn.GetComponent<Button>());
        SetField(ui, "_potionSlots", potionSlots);
        SetField(ui, "_potionCountTexts", potionCountTexts);
        SetField(ui, "_potionSellButtons", potionSellButtons);
        SetField(ui, "_sellAllButton", sellAllBtn.GetComponent<Button>());
        SetField(ui, "_brewingIndicator", brewingIndicator);
        SetField(ui, "_orderPanel", orderPanel);
        SetField(ui, "_orderText", orderText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_orderBonusText", orderBonusText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_orderTimerBar", timerBarImg);
        SetField(ui, "_stageClearPanel", scPanel);
        SetField(ui, "_stageClearText", scText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton", nextStageBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel", acPanel);
        SetField(ui, "_menuButton", menuBtn.GetComponent<Button>());

        // Wire BreweryManager fields
        SetField(bm, "_gameManager", gm);
        SetField(bm, "_ui", ui);
        SetField(bm, "_cauldronObj", cauldronObj);
        SetField(bm, "_ingredientSprites", ingredientSprites);
        SetField(bm, "_potionSprites", potionSprites);

        // Wire GameManager fields
        SetField(gm, "_stageManager", sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_breweryManager", bm);
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
        string scenePath = "Assets/Scenes/065v2_SpellBrewery.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup065v2] SpellBrewery シーン作成完了: " + scenePath);
    }

    static Button[] GetButtons(GameObject[] gos)
    {
        var btns = new Button[gos.Length];
        for (int i = 0; i < gos.Length; i++)
            btns[i] = gos[i]?.GetComponent<Button>();
        return btns;
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
        else Debug.LogWarning($"[Setup065v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
