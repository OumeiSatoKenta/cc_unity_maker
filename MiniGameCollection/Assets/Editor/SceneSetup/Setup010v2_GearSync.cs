using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game010v2_GearSync;

public static class Setup010v2_GearSync
{
    [MenuItem("Assets/Setup/010v2 GearSync")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup010v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game010v2_GearSync/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.14f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Sprites
        Sprite spBg         = LoadSprite(sp + "Background.png");
        Sprite spSmallGear  = LoadSprite(sp + "SmallGear.png");
        Sprite spLargeGear  = LoadSprite(sp + "LargeGear.png");
        Sprite spPowerSrc   = LoadSprite(sp + "PowerSource.png");
        Sprite spGoalGear   = LoadSprite(sp + "GoalGear.png");
        Sprite spFixedGear  = LoadSprite(sp + "FixedGear.png");
        Sprite spBelt       = LoadSprite(sp + "Belt.png");
        Sprite spGridCell   = LoadSprite(sp + "GridCell.png");
        Sprite spArrowCW    = LoadSprite(sp + "ArrowCW.png");
        Sprite spArrowCCW   = LoadSprite(sp + "ArrowCCW.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GearSyncGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_totalStages").intValue = 5;
        smSO.ApplyModifiedProperties();

        // GearSyncManager
        var managerObj = new GameObject("GearSyncManager");
        managerObj.transform.SetParent(gmObj.transform);
        var gearMgr = managerObj.AddComponent<GearSyncManager>();

        // ---- Canvas ----
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var evObj = new GameObject("EventSystem");
        evObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evObj.AddComponent<InputSystemUIInputModule>();

        // HUD: Stage (top left)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(360, 55), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // Score (top right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 34, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(360, 55), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        // Combo (top center)
        var comboText = CT(canvasObj.transform, "ComboText", "", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 45), new Vector2(0, -70));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.2f);

        // Test count (top center-left)
        var testCountText = CT(canvasObj.transform, "TestCountText", "テスト: 0回", 28, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300, 45), new Vector2(20, -70));
        testCountText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.5f);

        // ---- Bottom Panel: Parts list + Test button ----
        var bottomPanel = new GameObject("BottomPanel");
        bottomPanel.transform.SetParent(canvasObj.transform, false);
        var bpRt = bottomPanel.AddComponent<RectTransform>();
        bpRt.anchorMin = new Vector2(0f, 0f);
        bpRt.anchorMax = new Vector2(1f, 0f);
        bpRt.pivot = new Vector2(0.5f, 0f);
        bpRt.sizeDelta = new Vector2(0, 220);
        bpRt.anchoredPosition = Vector2.zero;
        var bpImg = bottomPanel.AddComponent<Image>();
        bpImg.color = new Color(0.1f, 0.15f, 0.25f, 0.9f);

        // Small gear button
        var sgBtn = CreatePartButton(bottomPanel.transform, "SmallGearBtn", "小歯車", spSmallGear, jpFont,
            new Vector2(-320, 110));
        var sgCountText = CT(bottomPanel.transform, "PartsSmallText", "×2", 26, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 35), new Vector2(-225, 60));
        sgCountText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        sgCountText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Large gear button
        var lgBtn = CreatePartButton(bottomPanel.transform, "LargeGearBtn", "大歯車", spLargeGear, jpFont,
            new Vector2(0, 110));
        var lgCountText = CT(bottomPanel.transform, "PartsLargeText", "×0", 26, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 35), new Vector2(95, 60));
        lgCountText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        lgCountText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Belt button
        var beltBtn = CreatePartButton(bottomPanel.transform, "BeltBtn", "ベルト", spBelt, jpFont,
            new Vector2(320, 110));
        var beltCountText = CT(bottomPanel.transform, "PartsBeltText", "×0", 26, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(80, 35), new Vector2(415, 60));
        beltCountText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        beltCountText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Test button
        var testBtn = CB(bottomPanel.transform, "TestButton", "起動テスト", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 65), new Vector2(0, 30),
            new Color(0.2f, 0.7f, 0.3f));

        // Menu button (bottom center)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 55), new Vector2(0, 10),
            new Color(0.3f, 0.3f, 0.4f));

        // ---- Stage Clear Panel ----
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.05f, 0.15f, 0.92f),
            new Vector2(700, 500));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 70), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scScore = CT(scPanel.transform, "StageClearScore", "+1000", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), new Vector2(0, 20));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);
        var scStars = CT(scPanel.transform, "StageClearStars", "★★★", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 70), new Vector2(0, -40));
        scStars.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStars.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(320, 70), new Vector2(0, 30),
            new Color(0.2f, 0.6f, 0.9f));
        scPanel.SetActive(false);

        // ---- Game Clear Panel ----
        var gcPanel = CreatePanel(canvasObj.transform, "GameClearPanel", new Color(0.05f, 0.1f, 0.05f, 0.95f),
            new Vector2(700, 500));
        var gcTitle = CT(gcPanel.transform, "GameClearTitle", "全ステージクリア！", 50, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 70), new Vector2(0, -30));
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.5f);
        var gcScore = CT(gcPanel.transform, "GameClearScore", "Total: 0", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), new Vector2(0, 20));
        gcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var gcMenuBtn = CB(gcPanel.transform, "GCMenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(300, 65), new Vector2(0, 30),
            new Color(0.3f, 0.3f, 0.5f));
        gcPanel.SetActive(false);

        // ---- Instruction Panel ----
        var ipPanel = CreatePanel(canvasObj.transform, "InstructionPanel", new Color(0.04f, 0.05f, 0.15f, 0.97f),
            new Vector2(0, 0));
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "GearSync", 60, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 200));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.8f, 1f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 60), new Vector2(0, 100));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.85f, 1f);

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 32, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 60), new Vector2(0, -80));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 75), new Vector2(0, -200),
            new Color(0.2f, 0.6f, 0.9f));

        // "?" button (re-show instruction)
        var reShowBtn = CB(canvasObj.transform, "ReShowInstructionBtn", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(75, 75), new Vector2(-10, 230),
            new Color(0.2f, 0.4f, 0.7f));

        // Wire InstructionPanel
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // ---- GearSyncUI ----
        var uiObj = new GameObject("GearSyncUI");
        uiObj.transform.SetParent(gmObj.transform);
        var gearUI = uiObj.AddComponent<GearSyncUI>();
        var uiSO = new SerializedObject(gearUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_testCountText").objectReferenceValue = testCountText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_partsSmallText").objectReferenceValue = sgCountText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_partsLargeText").objectReferenceValue = lgCountText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_partsBeltText").objectReferenceValue = beltCountText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearStarsText").objectReferenceValue = scStars.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = gcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_smallGearButton").objectReferenceValue = sgBtn.GetComponent<Button>();
        uiSO.FindProperty("_largeGearButton").objectReferenceValue = lgBtn.GetComponent<Button>();
        uiSO.FindProperty("_beltButton").objectReferenceValue = beltBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // ---- Wire GearSyncManager ----
        var gmgrSO = new SerializedObject(gearMgr);
        gmgrSO.FindProperty("_gameManager").objectReferenceValue = gm;
        gmgrSO.FindProperty("_ui").objectReferenceValue = gearUI;
        gmgrSO.FindProperty("_smallGearSprite").objectReferenceValue = spSmallGear;
        gmgrSO.FindProperty("_largeGearSprite").objectReferenceValue = spLargeGear;
        gmgrSO.FindProperty("_powerSourceSprite").objectReferenceValue = spPowerSrc;
        gmgrSO.FindProperty("_goalGearSprite").objectReferenceValue = spGoalGear;
        gmgrSO.FindProperty("_fixedGearSprite").objectReferenceValue = spFixedGear;
        gmgrSO.FindProperty("_beltSprite").objectReferenceValue = spBelt;
        gmgrSO.FindProperty("_gridCellSprite").objectReferenceValue = spGridCell;
        gmgrSO.FindProperty("_arrowCWSprite").objectReferenceValue = spArrowCW;
        gmgrSO.FindProperty("_arrowCCWSprite").objectReferenceValue = spArrowCCW;
        gmgrSO.ApplyModifiedProperties();

        // ---- Wire GameManager ----
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_gearSyncManager").objectReferenceValue = gearMgr;
        gmSO.FindProperty("_ui").objectReferenceValue = gearUI;
        gmSO.ApplyModifiedProperties();

        // ---- Button Events ----
        AddButtonOnClick(testBtn.GetComponent<Button>(), gearMgr, "RunTest");
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");
        AddButtonOnClick(nextBtn.GetComponent<Button>(), gm, "OnNextStage");
        AddButtonOnClick(gcMenuBtn.GetComponent<Button>(), gm, "OnReturnToMenu");
        AddButtonOnClick(sgBtn.GetComponent<Button>(), gearMgr, "SelectSmallGear");
        AddButtonOnClick(lgBtn.GetComponent<Button>(), gearMgr, "SelectLargeGear");
        AddButtonOnClick(beltBtn.GetComponent<Button>(), gearMgr, "SelectBelt");
        // "?" ボタンは InstructionPanel.Show を再度呼ぶため、GameManager 経由で対応予定
        // ReShow メソッドが未実装のためバインドなし

        // ---- Save Scene ----
        string scenePath = "Assets/Scenes/010v2_GearSync.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup010v2] Scene created: " + scenePath);
    }

    // ---- Helper: Create Text ----
    static GameObject CT(Transform parent, string name, string text, int size, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        if (font) tmp.font = font;
        tmp.enableWordWrapping = false;
        return go;
    }

    // ---- Helper: Create Button ----
    static GameObject CB(Transform parent, string name, string label, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(go.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 30;
        if (font) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    // ---- Helper: Part Button (icon + label) ----
    static GameObject CreatePartButton(Transform parent, string name, string label, Sprite icon, TMP_FontAsset font,
        Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(160, 155);
        rt.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.25f, 0.4f);
        var btn = go.AddComponent<Button>();

        if (icon != null)
        {
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(go.transform, false);
            var irt = iconObj.AddComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.5f, 0.5f); irt.anchorMax = new Vector2(0.5f, 0.5f);
            irt.sizeDelta = new Vector2(90, 90);
            irt.anchoredPosition = new Vector2(0, 20);
            var iImg = iconObj.AddComponent<Image>();
            iImg.sprite = icon;
            iImg.preserveAspect = true;
        }

        var lGo = new GameObject("Label");
        lGo.transform.SetParent(go.transform, false);
        var lrt = lGo.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0f); lrt.anchorMax = new Vector2(1f, 0f);
        lrt.pivot = new Vector2(0.5f, 0f);
        lrt.sizeDelta = new Vector2(0, 36);
        lrt.anchoredPosition = new Vector2(0, 5);
        var tmp = lGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 24;
        if (font) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.8f, 0.9f, 1f);
        return go;
    }

    // ---- Helper: Create Panel ----
    static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    // ---- Helper: Load Sprite ----
    static Sprite LoadSprite(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            Debug.LogWarning($"[Setup010v2] Sprite not found: {path}");
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path)
            ?? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    // ---- Helper: Add Button OnClick ----
    static void AddButtonOnClick(Button btn, Object target, string methodName)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var onClick = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        onClick.arraySize++;
        var call = onClick.GetArrayElementAtIndex(onClick.arraySize - 1);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
        call.FindPropertyRelative("m_MethodName").stringValue = methodName;
        call.FindPropertyRelative("m_Mode").intValue = 1;
        call.FindPropertyRelative("m_CallState").intValue = 2;
        so.ApplyModifiedProperties();
    }

    // ---- Helper: Add Scene to Build Settings ----
    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes) if (s.path == scenePath) return;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, newScenes, scenes.Length);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
