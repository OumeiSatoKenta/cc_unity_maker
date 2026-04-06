using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game044v2_TiltMaze;

public static class Setup044v2_TiltMaze
{
    [MenuItem("Assets/Setup/044v2 TiltMaze")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup044v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game044v2_TiltMaze/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.04f, 0.1f, 0.04f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"background.png", sp+"ball.png", sp+"wall.png", sp+"hole.png",
            sp+"goal.png", sp+"coin.png", sp+"ice_floor.png", sp+"warp_in.png", sp+"warp_out.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "background.png");
        Sprite spBall     = LoadSprite(sp + "ball.png");
        Sprite spWall     = LoadSprite(sp + "wall.png");
        Sprite spHole     = LoadSprite(sp + "hole.png");
        Sprite spGoal     = LoadSprite(sp + "goal.png");
        Sprite spCoin     = LoadSprite(sp + "coin.png");
        Sprite spIce      = LoadSprite(sp + "ice_floor.png");
        Sprite spWarpIn   = LoadSprite(sp + "warp_in.png");
        Sprite spWarpOut  = LoadSprite(sp + "warp_out.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camWidth = camera != null ? camSize * camera.aspect : camSize * 0.5625f;

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float scaleX = camWidth * 2f / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camSize * 2f / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Tags setup
        SetupTags();

        // === GameManager root ===
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<TiltMazeGameManager>();

        // StageManager (child)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // TiltMazeMechanic (child)
        var mechObj = new GameObject("TiltMazeMechanic");
        mechObj.transform.SetParent(gmObj.transform);
        var mech = mechObj.AddComponent<TiltMazeMechanic>();

        // Wire Mechanic sprites
        var mechSO = new SerializedObject(mech);
        mechSO.FindProperty("_gameManager").objectReferenceValue = gm;
        if (spBall    != null) mechSO.FindProperty("_spriteBall").objectReferenceValue    = spBall;
        if (spWall    != null) mechSO.FindProperty("_spriteWall").objectReferenceValue    = spWall;
        if (spHole    != null) mechSO.FindProperty("_spriteHole").objectReferenceValue    = spHole;
        if (spGoal    != null) mechSO.FindProperty("_spriteGoal").objectReferenceValue    = spGoal;
        if (spCoin    != null) mechSO.FindProperty("_spriteCoin").objectReferenceValue    = spCoin;
        if (spIce     != null) mechSO.FindProperty("_spriteIceFloor").objectReferenceValue = spIce;
        if (spWarpIn  != null) mechSO.FindProperty("_spriteWarpIn").objectReferenceValue  = spWarpIn;
        if (spWarpOut != null) mechSO.FindProperty("_spriteWarpOut").objectReferenceValue = spWarpOut;
        mechSO.ApplyModifiedProperties();

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // === InstructionPanel ===
        var ip = CreateInstructionPanel(canvasObj.transform, jpFont);

        // === HUD ===
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 36, jpFont,
            new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(280,50), new Vector2(10,-15));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 32, jpFont,
            new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(260,50), new Vector2(-10,-15));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);

        var timerText = CT(canvasObj.transform, "TimerText", "Time: 60", 36, jpFont,
            new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(220,50), new Vector2(0,-15));
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        timerText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var lifeText = CT(canvasObj.transform, "LifeText", "♥ ♥ ♥", 32, jpFont,
            new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(200,44), new Vector2(10,-60));
        lifeText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var coinText = CT(canvasObj.transform, "CoinText", "Coin: 0/0", 28, jpFont,
            new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(200,44), new Vector2(-10,-60));
        coinText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        coinText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);

        // Brake gauge (slider)
        var brakeObj = new GameObject("BrakeSlider");
        brakeObj.transform.SetParent(canvasObj.transform, false);
        var brakeSO = new SerializedObject(brakeObj);
        var brakeRt = brakeObj.AddComponent<RectTransform>();
        brakeRt.anchorMin = new Vector2(0.1f, 0f);
        brakeRt.anchorMax = new Vector2(0.9f, 0f);
        brakeRt.pivot = new Vector2(0.5f, 0f);
        brakeRt.sizeDelta = new Vector2(0, 30);
        brakeRt.anchoredPosition = new Vector2(0, 140);

        var bgImg = new GameObject("Background"); bgImg.transform.SetParent(brakeObj.transform, false);
        var bgImgRt = bgImg.AddComponent<RectTransform>();
        bgImgRt.anchorMin = Vector2.zero; bgImgRt.anchorMax = Vector2.one;
        bgImgRt.offsetMin = Vector2.zero; bgImgRt.offsetMax = Vector2.zero;
        var bgImgC = bgImg.AddComponent<Image>(); bgImgC.color = new Color(0.2f,0.2f,0.3f,0.8f);

        var fillArea = new GameObject("Fill Area"); fillArea.transform.SetParent(brakeObj.transform, false);
        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero; fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = Vector2.zero; fillAreaRt.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill"); fill.transform.SetParent(fillArea.transform, false);
        var fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>(); fillImg.color = new Color(0.3f, 0.6f, 1f, 0.9f);

        var brakeSlider = brakeObj.AddComponent<Slider>();
        brakeSlider.fillRect = fillRt;
        brakeSlider.direction = Slider.Direction.LeftToRight;
        brakeSlider.minValue = 0f;
        brakeSlider.maxValue = 1f;
        brakeSlider.value = 1f;

        // Brake label
        var brakeLabel = CT(canvasObj.transform, "BrakeLabelText", "ブレーキ", 22, jpFont,
            new Vector2(0.1f,0f), new Vector2(0.1f,0f), new Vector2(0f,0f), new Vector2(120,30), new Vector2(0,175));
        brakeLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.7f,0.8f,1f);

        // === Buttons ===
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0.5f,0f),
            new Vector2(160,55), new Vector2(0,15), new Color(0.3f,0.3f,0.35f));

        // === Stage Clear Panel ===
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f,0.12f,0.05f,0.95f), new Vector2(520,360));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,70), new Vector2(0,130));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,1f,0.5f);
        var scScore = CT(scPanel.transform, "StageClearScore", "Score: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,55), new Vector2(0,55));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.3f);
        var scBonus = CT(scPanel.transform, "StageClearBonus", "", 28, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,44), new Vector2(0,-10));
        scBonus.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scBonus.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.7f,0.2f);
        scBonus.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;
        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(260,60), new Vector2(0,-70), new Color(0.2f,0.6f,0.2f));
        var scMenuBtn = CB(scPanel.transform, "MenuButton2", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,55), new Vector2(0,-140), new Color(0.35f,0.35f,0.4f));
        scPanel.SetActive(false);

        // === Final Clear Panel ===
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel", new Color(0.05f,0.12f,0.05f,0.95f), new Vector2(540,380));
        var fcTitle = CT(fcPanel.transform, "FinalClearTitle", "全ステージクリア！", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(520,70), new Vector2(0,145));
        fcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);
        var fcScore = CT(fcPanel.transform, "FinalScore", "Final Score: 0", 40, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(520,60), new Vector2(0,60));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);
        var fcRetry = CB(fcPanel.transform, "RetryButton", "もう一度", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(220,60), new Vector2(-120,-80), new Color(0.2f,0.6f,0.2f));
        var fcMenu = CB(fcPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(220,60), new Vector2(120,-80), new Color(0.35f,0.35f,0.4f));
        fcPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.1f,0.05f,0.05f,0.95f), new Vector2(500,340));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,70), new Vector2(0,120));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);
        var goScore = CT(goPanel.transform, "GameOverScore", "Score: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(480,55), new Vector2(0,40));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);
        var goRetry = CB(goPanel.transform, "RetryButton", "リトライ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(-110,-60), new Color(0.2f,0.6f,0.2f));
        var goMenu = CB(goPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(110,-60), new Color(0.35f,0.35f,0.4f));
        goPanel.SetActive(false);

        // === TiltMazeUI (child of GameManager) ===
        var uiObj = new GameObject("TiltMazeUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<TiltMazeUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_lifeText").objectReferenceValue = lifeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_coinText").objectReferenceValue = coinText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_brakeSlider").objectReferenceValue = brakeSlider;
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearBonusText").objectReferenceValue = scBonus.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire _ui back into mechanic
        var mechSO2 = new SerializedObject(mech);
        mechSO2.FindProperty("_ui").objectReferenceValue = ui;
        mechSO2.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_mechanic").objectReferenceValue = mech;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Button wiring
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(scMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(scNextBtn.GetComponent<Button>(), gm, "AdvanceToNextStage");
        AddButtonOnClick(fcMenu.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(fcRetry.GetComponent<Button>(), gm, "RetryGame");
        AddButtonOnClick(goMenu.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(goRetry.GetComponent<Button>(), gm, "RetryGame");

        // Save scene
        string scenePath = "Assets/Scenes/044v2_TiltMaze.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup044v2] Scene created: " + scenePath);
    }

    static void SetupTags()
    {
        string[] tags = { "Hole", "Goal", "Coin" };
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProperty = tagManager.FindProperty("tags");
        foreach (var tag in tags)
        {
            bool found = false;
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                if (tagsProperty.GetArrayElementAtIndex(i).stringValue == tag) { found = true; break; }
            }
            if (!found)
            {
                tagsProperty.arraySize++;
                tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).stringValue = tag;
            }
        }
        tagManager.ApplyModifiedProperties();
    }

    static InstructionPanel CreateInstructionPanel(Transform canvasParent, TMP_FontAsset font)
    {
        var panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasParent, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.1f, 0.04f, 0.97f);
        var canvasComp = panelObj.AddComponent<Canvas>();
        canvasComp.overrideSorting = true;
        canvasComp.sortingOrder = 100;
        panelObj.AddComponent<GraphicRaycaster>();

        var ip = panelObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var titleObj = CT(panelObj.transform, "TitleText", "TiltMaze", 64, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,90), new Vector2(0,350));
        titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,1f,0.6f);

        var descObj = CT(panelObj.transform, "DescriptionText", "迷路を傾けてボールをゴールへ転がそう", 36, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,60), new Vector2(0,250));
        descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        descObj.GetComponent<TextMeshProUGUI>().color = new Color(0.9f,0.95f,1f);
        descObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = CT(panelObj.transform, "ControlsText", "画面をドラッグして迷路を傾ける。長押しでブレーキ", 30, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,80), new Vector2(0,130));
        ctrlObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ctrlObj.GetComponent<TextMeshProUGUI>().color = new Color(0.7f,1f,0.7f);
        ctrlObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = CT(panelObj.transform, "GoalText", "穴に落ちずにボールをゴールまで届けよう", 30, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,80), new Vector2(0,20));
        goalObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goalObj.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.4f);
        goalObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtn = CB(panelObj.transform, "StartButton", "はじめる", font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300,70), new Vector2(0,-150), new Color(0.2f,0.55f,0.25f));

        ipSO.FindProperty("_titleText").objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = descObj.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ctrlObj.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = goalObj.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = startBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = panelObj;
        ipSO.ApplyModifiedProperties();

        panelObj.SetActive(false);
        return ip;
    }

    static void EnsureSpriteImport(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { AssetDatabase.ImportAsset(path); importer = AssetImporter.GetAtPath(path) as TextureImporter; }
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }

    static GameObject CT(Transform parent, string name, string text, int size, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        if (font) tmp.font = font;
        tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        return go;
    }

    static GameObject CB(Transform parent, string name, string label, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        var img = go.AddComponent<Image>(); img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;
        var labelObj = new GameObject("Label"); labelObj.transform.SetParent(go.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 28; if (font) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        return go;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 sizeDelta)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f,0.5f); rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.pivot = new Vector2(0.5f,0.5f); rt.sizeDelta = sizeDelta; rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = color;
        return go;
    }

    static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null) sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f));
            else Debug.LogWarning($"[Setup044v2] Sprite not found: {path}");
        }
        return sprite;
    }

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

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes) if (s.path == scenePath) return;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
