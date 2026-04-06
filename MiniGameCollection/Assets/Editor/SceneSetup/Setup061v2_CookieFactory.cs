using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game061v2_CookieFactory;

public static class Setup061v2_CookieFactory
{
    [MenuItem("Assets/Setup/061v2 CookieFactory")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup061v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game061v2_CookieFactory/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(1f, 0.97f, 0.88f);
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
            bgObj.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        }

        // --- GameManager root ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<CookieFactoryGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // CookieManager
        var cmObj = new GameObject("CookieManager");
        cmObj.transform.SetParent(gmObj.transform);
        var cm = cmObj.AddComponent<CookieManager>();

        // --- Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD top row
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 38, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 60), new Vector2(0, -30));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var cookieText = CT(canvasObj.transform, "CookieText", "🍪 0 / 100", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(600, 55), new Vector2(0, -95));
        cookieText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        cookieText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.3f, 0f);

        // Progress bar
        var progressObj = new GameObject("ProgressBar", typeof(RectTransform));
        progressObj.transform.SetParent(canvasObj.transform, false);
        var progressRT = progressObj.GetComponent<RectTransform>();
        progressRT.anchorMin = new Vector2(0.05f, 1);
        progressRT.anchorMax = new Vector2(0.95f, 1);
        progressRT.pivot = new Vector2(0.5f, 1);
        progressRT.sizeDelta = new Vector2(0, 22);
        progressRT.anchoredPosition = new Vector2(0, -155);
        var progressBg = progressObj.AddComponent<Image>();
        progressBg.color = new Color(0.8f, 0.7f, 0.5f);
        var slider = progressObj.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(progressObj.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.offsetMin = faRT.offsetMax = Vector2.zero;

        var fillObj = new GameObject("Fill", typeof(RectTransform));
        fillObj.transform.SetParent(fillArea.transform, false);
        var fillRT = fillObj.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0, 0); fillRT.anchorMax = new Vector2(1, 1);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(1f, 0.7f, 0.2f);
        slider.fillRect = fillRT;
        slider.targetGraphic = fillImg;
        slider.interactable = false;

        // Auto rate text (right side)
        var autoRateText = CT(canvasObj.transform, "AutoRateText", "自動: 0.0/秒", 28, jpFont,
            new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(1f, 1), new Vector2(240, 50), new Vector2(-15, -175));
        autoRateText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        autoRateText.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.6f, 0.2f);

        // Combo text (center, hidden initially)
        var comboText = CT(canvasObj.transform, "ComboText", "COMBO x1", 32, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(400, 55), new Vector2(0, -185));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        comboText.SetActive(false);

        // Floating text parent (center area)
        var floatParent = new GameObject("FloatingTextParent", typeof(RectTransform));
        floatParent.transform.SetParent(canvasObj.transform, false);
        var fpRT = floatParent.GetComponent<RectTransform>();
        fpRT.anchorMin = new Vector2(0.5f, 0.5f); fpRT.anchorMax = new Vector2(0.5f, 0.5f);
        fpRT.sizeDelta = new Vector2(300, 100);
        fpRT.anchoredPosition = new Vector2(0, 200);

        // Floating text prefab (inactive template)
        var floatPrefabObj = new GameObject("FloatingTextPrefab", typeof(RectTransform));
        floatPrefabObj.transform.SetParent(floatParent.transform, false);
        var ftTMP = floatPrefabObj.AddComponent<TextMeshProUGUI>();
        ftTMP.font = jpFont;
        ftTMP.fontSize = 36;
        ftTMP.alignment = TextAlignmentOptions.Center;
        ftTMP.color = new Color(1f, 1f, 0.3f);
        floatPrefabObj.SetActive(false);

        // Big cookie button (center of screen)
        var cookieBtnObj = new GameObject("CookieButton", typeof(RectTransform));
        cookieBtnObj.transform.SetParent(canvasObj.transform, false);
        var cbRT = cookieBtnObj.GetComponent<RectTransform>();
        cbRT.anchorMin = new Vector2(0.5f, 0.5f); cbRT.anchorMax = new Vector2(0.5f, 0.5f);
        cbRT.sizeDelta = new Vector2(280, 280);
        cbRT.anchoredPosition = new Vector2(0, 150);
        var cbImg = cookieBtnObj.AddComponent<Image>();
        Sprite cookieSprite = LoadSprite(sp + "Cookie.png");
        if (cookieSprite != null) cbImg.sprite = cookieSprite;
        else cbImg.color = new Color(0.8f, 0.5f, 0.2f);
        cbImg.preserveAspect = true;
        var cookieBtn = cookieBtnObj.AddComponent<Button>();
        cookieBtn.targetGraphic = cbImg;

        // Shop Panel
        var shopPanel = new GameObject("ShopPanel", typeof(RectTransform));
        shopPanel.transform.SetParent(canvasObj.transform, false);
        var shopRT = shopPanel.GetComponent<RectTransform>();
        shopRT.anchorMin = new Vector2(0.5f, 0); shopRT.anchorMax = new Vector2(0.5f, 0);
        shopRT.pivot = new Vector2(0.5f, 0);
        shopRT.sizeDelta = new Vector2(980, 260);
        shopRT.anchoredPosition = new Vector2(0, 195);
        var shopBg = shopPanel.AddComponent<Image>();
        shopBg.color = new Color(0.5f, 0.35f, 0.15f, 0.7f);
        var shopLayout = shopPanel.AddComponent<HorizontalLayoutGroup>();
        shopLayout.spacing = 12;
        shopLayout.padding = new RectOffset(12, 12, 12, 12);
        shopLayout.childAlignment = TextAnchor.MiddleCenter;
        shopLayout.childForceExpandWidth = true;
        shopLayout.childForceExpandHeight = true;

        // Oven upgrade button
        Sprite ovenSprite = LoadSprite(sp + "Oven.png");
        var (ovenBtn, ovenBtnText) = MakeShopBtn(shopPanel.transform, "OvenButton", "オーブン\n強化 100🍪", ovenSprite, jpFont, new Color(0.8f, 0.3f, 0.1f));

        // Conveyor belt button
        Sprite convSprite = LoadSprite(sp + "ConveyorBelt.png");
        var (convBtn, convBtnText) = MakeShopBtn(shopPanel.transform, "ConveyorButton", "ベルト\n購入 500🍪", convSprite, jpFont, new Color(0.2f, 0.5f, 0.7f));
        convBtn.gameObject.SetActive(false); // unlocked stage2

        // Packaging machine button
        Sprite packSprite = LoadSprite(sp + "PackagingMachine.png");
        var (packBtn, packBtnText) = MakeShopBtn(shopPanel.transform, "PackagingButton", "包装機\n購入 3000🍪", packSprite, jpFont, new Color(0.5f, 0.1f, 0.6f));
        packBtn.gameObject.SetActive(false); // unlocked stage3

        // Special order button
        var soBtn = CB(canvasObj.transform, "SpecialOrderButton", "特注クッキー\n受注", 24, jpFont,
            new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(0f, 0), new Vector2(200, 70), new Vector2(20, 475),
            new Color(0.7f, 0.5f, 0.1f));
        soBtn.SetActive(false);

        // Special order progress panel
        var soPanelObj = new GameObject("SpecialOrderPanel", typeof(RectTransform));
        soPanelObj.transform.SetParent(canvasObj.transform, false);
        var soPanelRT = soPanelObj.GetComponent<RectTransform>();
        soPanelRT.anchorMin = new Vector2(0f, 0); soPanelRT.anchorMax = new Vector2(0f, 0);
        soPanelRT.pivot = new Vector2(0f, 0);
        soPanelRT.sizeDelta = new Vector2(200, 30);
        soPanelRT.anchoredPosition = new Vector2(20, 465);
        var soPanelBg = soPanelObj.AddComponent<Image>();
        soPanelBg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        var soSlider = soPanelObj.AddComponent<Slider>();
        soSlider.minValue = 0f; soSlider.maxValue = 1f; soSlider.value = 1f;
        soSlider.interactable = false;
        var soFillArea2 = new GameObject("Fill Area", typeof(RectTransform));
        soFillArea2.transform.SetParent(soPanelObj.transform, false);
        var soFaRT = soFillArea2.GetComponent<RectTransform>();
        soFaRT.anchorMin = Vector2.zero; soFaRT.anchorMax = Vector2.one;
        soFaRT.offsetMin = soFaRT.offsetMax = Vector2.zero;
        var soFillObj = new GameObject("Fill", typeof(RectTransform));
        soFillObj.transform.SetParent(soFillArea2.transform, false);
        var soFillRT = soFillObj.GetComponent<RectTransform>();
        soFillRT.anchorMin = new Vector2(0, 0); soFillRT.anchorMax = new Vector2(1, 1);
        soFillRT.offsetMin = soFillRT.offsetMax = Vector2.zero;
        var soFillImg = soFillObj.AddComponent<Image>();
        soFillImg.color = new Color(1f, 0.8f, 0.2f);
        soSlider.fillRect = soFillRT;
        soSlider.targetGraphic = soFillImg;
        soPanelObj.SetActive(false);

        // Breakdown panel
        var breakPanel = new GameObject("BreakdownPanel", typeof(RectTransform));
        breakPanel.transform.SetParent(canvasObj.transform, false);
        var bpRT = breakPanel.GetComponent<RectTransform>();
        bpRT.anchorMin = new Vector2(0.5f, 0.5f); bpRT.anchorMax = new Vector2(0.5f, 0.5f);
        bpRT.sizeDelta = new Vector2(500, 130);
        bpRT.anchoredPosition = new Vector2(0, -60);
        var bpImg = breakPanel.AddComponent<Image>();
        bpImg.color = new Color(0.6f, 0.1f, 0.1f, 0.9f);
        var repairText = CT(breakPanel.transform, "RepairText", "故障！修理タップ: 5回", 30, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(460, 55), Vector2.zero);
        repairText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var repairBtn = CB(breakPanel.transform, "RepairButton", "修理！", 30, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 55), Vector2.zero,
            new Color(0.8f, 0.3f, 0.1f));
        breakPanel.SetActive(false);

        // VIP order panel
        var vipPanel = new GameObject("VIPPanel", typeof(RectTransform));
        vipPanel.transform.SetParent(canvasObj.transform, false);
        var vpRT = vipPanel.GetComponent<RectTransform>();
        vpRT.anchorMin = new Vector2(0.5f, 0.5f); vpRT.anchorMax = new Vector2(0.5f, 0.5f);
        vpRT.sizeDelta = new Vector2(600, 160);
        vpRT.anchoredPosition = new Vector2(0, 80);
        var vpImg = vipPanel.AddComponent<Image>();
        vpImg.color = new Color(0.1f, 0.2f, 0.5f, 0.92f);
        var vipTimerText = CT(vipPanel.transform, "VIPTimerText", "VIP注文 30秒", 30, jpFont,
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f), new Vector2(550, 50), Vector2.zero);
        vipTimerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        vipTimerText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        var vipProgText = CT(vipPanel.transform, "VIPProgressText", "0/500", 28, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(400, 45), Vector2.zero);
        vipProgText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        // VIP slider
        var vipSliderObj = new GameObject("VIPSlider", typeof(RectTransform));
        vipSliderObj.transform.SetParent(vipPanel.transform, false);
        var vsRT = vipSliderObj.GetComponent<RectTransform>();
        vsRT.anchorMin = new Vector2(0.5f, 0.2f); vsRT.anchorMax = new Vector2(0.5f, 0.2f);
        vsRT.sizeDelta = new Vector2(520, 25);
        vsRT.anchoredPosition = Vector2.zero;
        var vsBg = vipSliderObj.AddComponent<Image>(); vsBg.color = new Color(0.3f, 0.3f, 0.3f);
        var vsSlider = vipSliderObj.AddComponent<Slider>();
        vsSlider.minValue = 0f; vsSlider.maxValue = 1f; vsSlider.value = 1f; vsSlider.interactable = false;
        var vsFillArea = new GameObject("Fill Area", typeof(RectTransform));
        vsFillArea.transform.SetParent(vipSliderObj.transform, false);
        var vsFaRT = vsFillArea.GetComponent<RectTransform>();
        vsFaRT.anchorMin = Vector2.zero; vsFaRT.anchorMax = Vector2.one;
        vsFaRT.offsetMin = vsFaRT.offsetMax = Vector2.zero;
        var vsFillGO = new GameObject("Fill", typeof(RectTransform));
        vsFillGO.transform.SetParent(vsFillArea.transform, false);
        var vsFRT = vsFillGO.GetComponent<RectTransform>();
        vsFRT.anchorMin = new Vector2(0, 0); vsFRT.anchorMax = new Vector2(1, 1);
        vsFRT.offsetMin = vsFRT.offsetMax = Vector2.zero;
        var vsFillImg = vsFillGO.AddComponent<Image>(); vsFillImg.color = Color.green;
        vsSlider.fillRect = vsFRT; vsSlider.targetGraphic = vsFillImg;
        vipPanel.SetActive(false);

        // Stage Clear Panel
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scpRT = scPanel.GetComponent<RectTransform>();
        scpRT.anchorMin = new Vector2(0.1f, 0.35f); scpRT.anchorMax = new Vector2(0.9f, 0.65f);
        scpRT.offsetMin = scpRT.offsetMax = Vector2.zero;
        var scImg = scPanel.AddComponent<Image>(); scImg.color = new Color(0.3f, 0.15f, 0f, 0.92f);
        var scTitle = CT(scPanel.transform, "Title", "ステージクリア！", 46, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(600, 75), Vector2.zero);
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);
        var scScore = CT(scPanel.transform, "ScoreText", "生産量: 100🍪", 34, jpFont,
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(500, 55), Vector2.zero);
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var scNext = CB(scPanel.transform, "NextStageButton", "次のステージへ", 32, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(300, 65), Vector2.zero,
            new Color(0.6f, 0.3f, 0.05f));
        scPanel.SetActive(false);

        // Game Clear Panel
        var gcPanel = new GameObject("GameClearPanel", typeof(RectTransform));
        gcPanel.transform.SetParent(canvasObj.transform, false);
        var gcpRT = gcPanel.GetComponent<RectTransform>();
        gcpRT.anchorMin = new Vector2(0.05f, 0.3f); gcpRT.anchorMax = new Vector2(0.95f, 0.7f);
        gcpRT.offsetMin = gcpRT.offsetMax = Vector2.zero;
        var gcImg = gcPanel.AddComponent<Image>(); gcImg.color = new Color(0.2f, 0.1f, 0f, 0.92f);
        var gcTitle = CT(gcPanel.transform, "Title", "全ステージクリア！", 46, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(700, 75), Vector2.zero);
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var gcScore = CT(gcPanel.transform, "ScoreText", "総生産量: 1M🍪", 36, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(600, 60), Vector2.zero);
        gcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcRetry = CB(gcPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.3f, 0.22f), new Vector2(0.3f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.5f, 0.25f, 0f));
        var gcMenu = CB(gcPanel.transform, "MenuButton", "メニューへ", 32, jpFont,
            new Vector2(0.7f, 0.22f), new Vector2(0.7f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.3f, 0.3f, 0.3f));
        gcPanel.SetActive(false);

        // Menu button (bottom)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ", 26, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(200, 55), new Vector2(0, 15),
            new Color(0.3f, 0.3f, 0.3f, 0.85f));

        // InstructionPanel
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipObj = new GameObject("InstructionPanel", typeof(RectTransform));
        ipObj.transform.SetParent(ipCanvas.transform, false);
        var ip = ipObj.AddComponent<InstructionPanel>();
        var ipRT = ipObj.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one;
        ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;
        var ipBg = ipObj.AddComponent<Image>(); ipBg.color = new Color(0f, 0f, 0f, 0.85f);

        var ipTitle = CT(ipObj.transform, "TitleText", "CookieFactory", 54, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(800, 80), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.4f);

        var ipDesc = CT(ipObj.transform, "DescriptionText", "タップでクッキーを焼いて工場を大きくしよう", 34, jpFont,
            new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.5f), new Vector2(900, 60), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipControls = CT(ipObj.transform, "ControlsText", "クッキーをタップして焼く・設備を買って自動化", 30, jpFont,
            new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.5f), new Vector2(900, 55), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var ipGoal = CT(ipObj.transform, "GoalText", "売上目標を達成して次のステージへ進もう", 30, jpFont,
            new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.5f), new Vector2(900, 55), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.5f);

        var ipStartBtn = CB(ipObj.transform, "StartButton", "はじめる", 38, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(300, 75), Vector2.zero,
            new Color(0.7f, 0.4f, 0.05f));

        var ipHelpBtn = CB(canvasObj.transform, "HelpButton", "?", 30, jpFont,
            new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(1f, 0), new Vector2(65, 65), new Vector2(-15, 80),
            new Color(0.5f, 0.35f, 0.1f, 0.85f));

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // --- Wire up InstructionPanel ---
        var ipSO = new UnityEditor.SerializedObject(ip);
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipControls.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_helpButton").objectReferenceValue = ipHelpBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipObj;
        ipSO.ApplyModifiedProperties();

        // Wire up CookieFactoryUI
        var ui = canvasObj.AddComponent<CookieFactoryUI>();
        var uiSO = new UnityEditor.SerializedObject(ui);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_cookieText").objectReferenceValue = cookieText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_autoRateText").objectReferenceValue = autoRateText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_progressSlider").objectReferenceValue = slider;
        uiSO.FindProperty("_ovenBtn").objectReferenceValue = ovenBtn;
        uiSO.FindProperty("_ovenBtnText").objectReferenceValue = ovenBtnText;
        uiSO.FindProperty("_conveyorBtn").objectReferenceValue = convBtn;
        uiSO.FindProperty("_conveyorBtnText").objectReferenceValue = convBtnText;
        uiSO.FindProperty("_packagingBtn").objectReferenceValue = packBtn;
        uiSO.FindProperty("_packagingBtnText").objectReferenceValue = packBtnText;
        uiSO.FindProperty("_specialOrderBtn").objectReferenceValue = soBtn.GetComponent<Button>();
        uiSO.FindProperty("_specialOrderSlider").objectReferenceValue = soSlider;
        uiSO.FindProperty("_specialOrderPanel").objectReferenceValue = soPanelObj;
        uiSO.FindProperty("_breakdownPanel").objectReferenceValue = breakPanel;
        uiSO.FindProperty("_repairText").objectReferenceValue = repairText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_repairBtn").objectReferenceValue = repairBtn.GetComponent<Button>();
        uiSO.FindProperty("_vipPanel").objectReferenceValue = vipPanel;
        uiSO.FindProperty("_vipTimerText").objectReferenceValue = vipTimerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_vipProgressText").objectReferenceValue = vipProgText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_vipSlider").objectReferenceValue = vsSlider;
        uiSO.FindProperty("_vipSliderFill").objectReferenceValue = vsFillImg;
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_floatingTextParent").objectReferenceValue = fpRT;
        uiSO.FindProperty("_floatingTextPrefab").objectReferenceValue = ftTMP;
        uiSO.ApplyModifiedProperties();

        // Wire up CookieManager
        var cmSO = new UnityEditor.SerializedObject(cm);
        cmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        cmSO.FindProperty("_ui").objectReferenceValue = ui;
        cmSO.FindProperty("_cookieButtonTransform").objectReferenceValue = cbRT;
        cmSO.ApplyModifiedProperties();

        // Wire up GameManager
        var gmSO = new UnityEditor.SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_cookieManager").objectReferenceValue = cm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Wire up buttons
        UnityEditor.Events.UnityEventTools.AddPersistentListener(cookieBtn.onClick,
            new UnityEngine.Events.UnityAction(cm.Tap));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(ovenBtn.onClick,
            new UnityEngine.Events.UnityAction(cm.BuyOven));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(convBtn.onClick,
            new UnityEngine.Events.UnityAction(cm.BuyConveyor));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(packBtn.onClick,
            new UnityEngine.Events.UnityAction(cm.BuyPackaging));

        var soButton = soBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(soButton.onClick,
            new UnityEngine.Events.UnityAction(cm.StartSpecialOrder));

        var repButton = repairBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(repButton.onClick,
            new UnityEngine.Events.UnityAction(cm.RepairTap));

        var nextButton = scNext.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextButton.onClick,
            new UnityEngine.Events.UnityAction(gm.OnNextStage));

        var retryButton = gcRetry.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(retryButton.onClick,
            new UnityEngine.Events.UnityAction(gm.OnRetry));

        // Menu buttons
        var menuBtnComp = menuBtn.GetComponent<Button>();
        menuBtnComp.gameObject.AddComponent<BackToMenuButton>();
        var gcMenuComp = gcMenu.GetComponent<Button>();
        gcMenuComp.gameObject.AddComponent<BackToMenuButton>();

        // BackToMenuButton on menu btn
        // Save scene
        string scenePath = "Assets/Scenes/061v2_CookieFactory.unity";
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup061v2] シーン作成完了: " + scenePath);
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = list.ToArray();
    }

    static Sprite LoadSprite(string path)
    {
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sp == null)
        {
            AssetDatabase.ImportAsset(path);
            sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return sp;
    }

    static (Button btn, TextMeshProUGUI text) MakeShopBtn(Transform parent, string name, string label, Sprite icon, TMP_FontAsset font, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var img = obj.AddComponent<Image>();
        img.color = color;
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        if (icon != null)
        {
            var iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(obj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.4f); iconRT.anchorMax = new Vector2(0.4f, 0.9f);
            iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
        }

        var textObj = new GameObject("Label", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);
        var textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(icon != null ? 0.38f : 0f, 0); textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(4, 4); textRT.offsetMax = new Vector2(-4, -4);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSize = 22;
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;

        return (btn, tmp);
    }

    static GameObject CT(Transform parent, string name, string text, int size, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.font = font; tmp.fontSize = size; tmp.text = text;
        tmp.color = Color.white;
        return obj;
    }

    static GameObject CB(Transform parent, string name, string text, int size, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var img = obj.AddComponent<Image>(); img.color = color;
        var btn = obj.AddComponent<Button>(); btn.targetGraphic = img;
        var labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(obj.transform, false);
        var lrt = labelObj.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.font = font; tmp.fontSize = size; tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        return obj;
    }
}
