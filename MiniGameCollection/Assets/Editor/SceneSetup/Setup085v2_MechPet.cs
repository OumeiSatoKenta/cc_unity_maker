using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game085v2_MechPet;

public static class Setup085v2_MechPet
{
    [MenuItem("Assets/Setup/085v2 MechPet")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup085v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game085v2_MechPet/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.12f, 0.10f, 0.14f);
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

        // Load part sprites
        Sprite headNormal    = LoadSprite(sp + "head_normal.png");
        Sprite headSpeed     = LoadSprite(sp + "head_speed.png");
        Sprite headShield    = LoadSprite(sp + "head_shield.png");
        Sprite headHeavy     = LoadSprite(sp + "head_heavy.png");
        Sprite headLegendary = LoadSprite(sp + "head_legendary.png");

        Sprite bodyNormal    = LoadSprite(sp + "body_normal.png");
        Sprite bodySpeed     = LoadSprite(sp + "body_speed.png");
        Sprite bodyShield    = LoadSprite(sp + "body_shield.png");
        Sprite bodyHeavy     = LoadSprite(sp + "body_heavy.png");
        Sprite bodyLegendary = LoadSprite(sp + "body_legendary.png");

        Sprite armNormal     = LoadSprite(sp + "arm_normal.png");
        Sprite armSpeed      = LoadSprite(sp + "arm_speed.png");
        Sprite armShield     = LoadSprite(sp + "arm_shield.png");
        Sprite armHeavy      = LoadSprite(sp + "arm_heavy.png");
        Sprite armLegendary  = LoadSprite(sp + "arm_legendary.png");

        Sprite legNormal     = LoadSprite(sp + "leg_normal.png");
        Sprite legSpeed      = LoadSprite(sp + "leg_speed.png");
        Sprite legShield     = LoadSprite(sp + "leg_shield.png");
        Sprite legHeavy      = LoadSprite(sp + "leg_heavy.png");
        Sprite legLegendary  = LoadSprite(sp + "leg_legendary.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("MechPetGameManager");
        var gm = gmObj.AddComponent<MechPetGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 2, complexityFactor = 0.25f },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 3, complexityFactor = 0.5f },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 4, complexityFactor = 0.75f },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 5, complexityFactor = 1.0f },
        };
        sm.SetConfigs(stageConfigs);

        // MechPetManager
        var mpObj = new GameObject("MechPetManager");
        mpObj.transform.SetParent(gmObj.transform);
        var mp = mpObj.AddComponent<MechPetManager>();

        // Robot Display (child of MechPetManager for logical grouping, but positioned in world space)
        var robotDisplay = new GameObject("RobotDisplay");
        robotDisplay.transform.SetParent(mpObj.transform);
        robotDisplay.transform.localPosition = Vector3.zero;

        var headObj = new GameObject("HeadRenderer");
        headObj.transform.SetParent(robotDisplay.transform);
        var headSR = headObj.AddComponent<SpriteRenderer>();
        headSR.sprite = headNormal;
        headSR.sortingOrder = 2;
        headObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        var bodyObj = new GameObject("BodyRenderer");
        bodyObj.transform.SetParent(robotDisplay.transform);
        var bodySR = bodyObj.AddComponent<SpriteRenderer>();
        bodySR.sprite = bodyNormal;
        bodySR.sortingOrder = 1;
        bodyObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        var leftArmObj = new GameObject("LeftArmRenderer");
        leftArmObj.transform.SetParent(robotDisplay.transform);
        var leftArmSR = leftArmObj.AddComponent<SpriteRenderer>();
        leftArmSR.sprite = armNormal;
        leftArmSR.sortingOrder = 0;
        leftArmObj.transform.localScale = new Vector3(1.0f, 1.0f, 1f);

        var rightArmObj = new GameObject("RightArmRenderer");
        rightArmObj.transform.SetParent(robotDisplay.transform);
        var rightArmSR = rightArmObj.AddComponent<SpriteRenderer>();
        rightArmSR.sprite = armNormal;
        rightArmSR.flipX = true;
        rightArmSR.sortingOrder = 0;
        rightArmObj.transform.localScale = new Vector3(1.0f, 1.0f, 1f);

        var legObj = new GameObject("LegRenderer");
        legObj.transform.SetParent(robotDisplay.transform);
        var legSR = legObj.AddComponent<SpriteRenderer>();
        legSR.sprite = legNormal;
        legSR.sortingOrder = 0;
        legObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        // Wire part sprites to MechPetManager
        SetField(mp, "_headNormal",    headNormal);
        SetField(mp, "_headSpeed",     headSpeed);
        SetField(mp, "_headShield",    headShield);
        SetField(mp, "_headHeavy",     headHeavy);
        SetField(mp, "_headLegendary", headLegendary);
        SetField(mp, "_bodyNormal",    bodyNormal);
        SetField(mp, "_bodySpeed",     bodySpeed);
        SetField(mp, "_bodyShield",    bodyShield);
        SetField(mp, "_bodyHeavy",     bodyHeavy);
        SetField(mp, "_bodyLegendary", bodyLegendary);
        SetField(mp, "_armNormal",     armNormal);
        SetField(mp, "_armSpeed",      armSpeed);
        SetField(mp, "_armShield",     armShield);
        SetField(mp, "_armHeavy",      armHeavy);
        SetField(mp, "_armLegendary",  armLegendary);
        SetField(mp, "_legNormal",     legNormal);
        SetField(mp, "_legSpeed",      legSpeed);
        SetField(mp, "_legShield",     legShield);
        SetField(mp, "_legHeavy",      legHeavy);
        SetField(mp, "_legLegendary",  legLegendary);
        SetField(mp, "_headRenderer",    headSR);
        SetField(mp, "_bodyRenderer",    bodySR);
        SetField(mp, "_leftArmRenderer", leftArmSR);
        SetField(mp, "_rightArmRenderer",rightArmSR);
        SetField(mp, "_legRenderer",     legSR);

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

        // === HUD top ===
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 44, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.7f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 44, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(320, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.7f);

        var comboText = CT(canvasObj.transform, "ComboText", "", 38, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 50), new Vector2(0, -90));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var synergyText = CT(canvasObj.transform, "SynergyText", "", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(800, 55), new Vector2(0, -150));
        synergyText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        synergyText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.5f);

        var messageText = CT(canvasObj.transform, "MessageText", "", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 70), new Vector2(0, 200));
        messageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        messageText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.2f);
        messageText.SetActive(false);

        // === Energy Slider ===
        var energyPanel = new GameObject("EnergyPanel", typeof(RectTransform));
        energyPanel.transform.SetParent(canvasObj.transform, false);
        var epRT = energyPanel.GetComponent<RectTransform>();
        epRT.anchorMin = new Vector2(0.5f, 0f);
        epRT.anchorMax = new Vector2(0.5f, 0f);
        epRT.pivot = new Vector2(0.5f, 0f);
        epRT.sizeDelta = new Vector2(900, 70);
        epRT.anchoredPosition = new Vector2(0, 420);

        var energyLabel = CT(energyPanel.transform, "EnergyLabel", "Energy", 36, jpFont,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(170, 55), new Vector2(0, 0));
        energyLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.8f);

        var sliderObj = new GameObject("EnergySlider", typeof(RectTransform));
        sliderObj.transform.SetParent(energyPanel.transform, false);
        var sliderRT = sliderObj.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.18f, 0f);
        sliderRT.anchorMax = new Vector2(1f, 1f);
        sliderRT.offsetMin = sliderRT.offsetMax = Vector2.zero;

        var slider = sliderObj.AddComponent<Slider>();

        var bgSlider = new GameObject("Background", typeof(RectTransform));
        bgSlider.transform.SetParent(sliderObj.transform, false);
        var bgSRT = bgSlider.GetComponent<RectTransform>();
        bgSRT.anchorMin = Vector2.zero; bgSRT.anchorMax = Vector2.one;
        bgSRT.offsetMin = bgSRT.offsetMax = Vector2.zero;
        var bgSliderImg = bgSlider.AddComponent<Image>();
        bgSliderImg.color = new Color(0.2f, 0.2f, 0.3f);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.offsetMin = faRT.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.9f, 0.5f);

        slider.fillRect = fillRT;
        slider.targetGraphic = fillImg;
        slider.value = 1f;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false;

        // === Slot Buttons (horizontal, bottom mid area) ===
        string[] slotNames = { "頭", "胴体", "腕", "脚" };
        float[] slotXPositions = { -400f, -130f, 130f, 400f };
        var slotButtons = new Button[4];
        var slotTexts = new TextMeshProUGUI[4];
        var slotColors = new Color[] {
            new Color(0.5f, 0.35f, 0.2f),
            new Color(0.45f, 0.3f, 0.15f),
            new Color(0.4f, 0.28f, 0.12f),
            new Color(0.35f, 0.25f, 0.1f)
        };

        for (int i = 0; i < 4; i++)
        {
            var slot = new GameObject($"Slot{slotNames[i]}Button", typeof(RectTransform));
            slot.transform.SetParent(canvasObj.transform, false);
            var slotRT = slot.GetComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0.5f, 0f);
            slotRT.anchorMax = new Vector2(0.5f, 0f);
            slotRT.pivot = new Vector2(0.5f, 0f);
            slotRT.sizeDelta = new Vector2(230, 100);
            slotRT.anchoredPosition = new Vector2(slotXPositions[i], 285);
            var slotImg = slot.AddComponent<Image>();
            slotImg.color = slotColors[i];
            slotButtons[i] = slot.AddComponent<Button>();
            slotButtons[i].targetGraphic = slotImg;

            var slotTextGo = CT(slot.transform, "SlotText", $"{slotNames[i]}\nノーマル", 32, jpFont,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(220, 95), Vector2.zero);
            slotTexts[i] = slotTextGo.GetComponent<TextMeshProUGUI>();
            slotTexts[i].alignment = TextAlignmentOptions.Center;
            slotTexts[i].color = Color.white;

            int capturedIndex = i;
            slotButtons[i].onClick.AddListener(() => mp.CycleSlot(capturedIndex));
        }

        // === Mission & Charge buttons ===
        var missionBtn = CB(canvasObj.transform, "MissionButton", "ミッション\n開始", 38, jpFont,
            new Vector2(0.35f, 0f), new Vector2(0.35f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 110), new Vector2(0, 135), new Color(0.7f, 0.25f, 0.1f));

        var chargeBtn = CB(canvasObj.transform, "ChargeButton", "エネルギー\nチャージ", 38, jpFont,
            new Vector2(0.65f, 0f), new Vector2(0.65f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 110), new Vector2(0, 135), new Color(0.1f, 0.5f, 0.35f));

        missionBtn.GetComponent<Button>().onClick.AddListener(() => mp.StartMission());
        chargeBtn.GetComponent<Button>().onClick.AddListener(() => mp.ChargeEnergy());

        // Back button
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 36, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 60), new Vector2(0, 15), new Color(0.25f, 0.2f, 0.15f, 0.9f));
        backBtn.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.5f, 0.5f); scRT.anchorMax = new Vector2(0.5f, 0.5f);
        scRT.pivot = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta = new Vector2(700, 340);
        scRT.anchoredPosition = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>();
        scImg.color = new Color(0.15f, 0.12f, 0.2f, 0.97f);

        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 60, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(640, 80), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.4f);

        var scScore = CT(scPanel.transform, "SCScore", "", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 70), new Vector2(0, 20));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.9f);

        var nextBtn = CB(scPanel.transform, "NextButton", "次のステージへ", 44, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(420, 70), new Vector2(0, 50), new Color(0.3f, 0.5f, 0.7f));
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
        acImg.color = new Color(0.1f, 0.08f, 0.18f, 0.97f);

        var acTitle = CT(acPanel.transform, "ACTitle", "ミッション\nコンプリート！", 54, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 130), new Vector2(0, -30));
        acTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var acScore = CT(acPanel.transform, "ACScore", "Final Score: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), new Vector2(0, 30));
        acScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScore.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.85f, 0.95f);

        var acBack = CB(acPanel.transform, "ACBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(340, 65), new Vector2(0, 50), new Color(0.3f, 0.22f, 0.4f));
        acBack.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.5f, 0.5f); goRT.anchorMax = new Vector2(0.5f, 0.5f);
        goRT.pivot = new Vector2(0.5f, 0.5f);
        goRT.sizeDelta = new Vector2(700, 340);
        goRT.anchoredPosition = Vector2.zero;
        var goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.2f, 0.05f, 0.05f, 0.97f);

        var goTitle = CT(goPanel.transform, "GOTitle", "ゲームオーバー", 60, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 90), Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.2f);

        var goBack = CB(goPanel.transform, "GOBackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(340, 65), Vector2.zero, new Color(0.35f, 0.15f, 0.1f));
        goBack.GetComponent<Button>().onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu"));
        goPanel.SetActive(false);

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
        ipBgImg.color = new Color(0.1f, 0.08f, 0.15f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "MechPet", 72, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 110), new Vector2(0, 0));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 0.3f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 40, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.78f, 0.7f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 34, jpFont,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 160), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.85f, 0.9f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 34, jpFont,
            new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 52, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 75), new Vector2(0, 0), new Color(0.3f, 0.4f, 0.7f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 44, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-15, 450), new Color(0.25f, 0.2f, 0.35f, 0.9f));

        // === MechPetUI ===
        var uiObj = new GameObject("MechPetUI");
        var ui = uiObj.AddComponent<MechPetUI>();

        SetField(ui, "_stageText",      stageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",      scoreText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",      comboText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_synergyText",    synergyText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_messageText",    messageText.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_energySlider",   slider);
        SetField(ui, "_slotButtons",    slotButtons);
        SetField(ui, "_slotTexts",      slotTexts);
        SetField(ui, "_missionButton",  missionBtn.GetComponent<Button>());
        SetField(ui, "_chargeButton",   chargeBtn.GetComponent<Button>());
        SetField(ui, "_stageClearPanel",      scPanel);
        SetField(ui, "_stageClearScoreText",  scScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton",      nextBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",        acPanel);
        SetField(ui, "_allClearScoreText",    acScore.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",        goPanel);
        SetField(ui, "_mechPetManager",       mp);

        // Wire MechPetManager
        SetField(mp, "_gameManager", gm);
        SetField(mp, "_ui",          ui);

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_mechPetManager",   mp);
        SetField(gm, "_ui",               ui);

        // Next stage button
        nextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());

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
        string scenePath = "Assets/Scenes/085v2_MechPet.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup085v2] MechPet シーン作成完了: " + scenePath);
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
        else Debug.LogWarning($"[Setup085v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
