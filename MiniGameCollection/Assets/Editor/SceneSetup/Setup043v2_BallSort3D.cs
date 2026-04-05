using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game043v2_BallSort3D;

public static class Setup043v2_BallSort3D
{
    [MenuItem("Assets/Setup/043v2 BallSort3D")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup043v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game043v2_BallSort3D/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.04f, 0.09f, 0.16f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"background.png",
            sp+"tube.png",
            sp+"ball_r.png", sp+"ball_g.png", sp+"ball_b.png", sp+"ball_y.png", sp+"ball_m.png",
            sp+"lid.png", sp+"lock_icon.png", sp+"rotate_icon.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg         = LoadSprite(sp + "background.png");
        Sprite spTube       = LoadSprite(sp + "tube.png");
        Sprite spBallR      = LoadSprite(sp + "ball_r.png");
        Sprite spBallG      = LoadSprite(sp + "ball_g.png");
        Sprite spBallB      = LoadSprite(sp + "ball_b.png");
        Sprite spBallY      = LoadSprite(sp + "ball_y.png");
        Sprite spBallM      = LoadSprite(sp + "ball_m.png");
        Sprite spLid        = LoadSprite(sp + "lid.png");
        Sprite spLock       = LoadSprite(sp + "lock_icon.png");
        Sprite spRotate     = LoadSprite(sp + "rotate_icon.png");

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

        // === GameManager root ===
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<BallSort3DGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // BallSort3DMechanic (child of GameManager)
        var mechObj = new GameObject("BallSort3DMechanic");
        mechObj.transform.SetParent(gmObj.transform);
        var mech = mechObj.AddComponent<BallSort3DMechanic>();

        // Wire Mechanic sprites
        // _ui wired after ui component created below — see second mechSO block
        var mechSO = new SerializedObject(mech);
        mechSO.FindProperty("_gameManager").objectReferenceValue = gm;
        if (spTube   != null) mechSO.FindProperty("_spriteTube").objectReferenceValue     = spTube;
        if (spBallR  != null) mechSO.FindProperty("_spriteBallR").objectReferenceValue    = spBallR;
        if (spBallG  != null) mechSO.FindProperty("_spriteBallG").objectReferenceValue    = spBallG;
        if (spBallB  != null) mechSO.FindProperty("_spriteBallB").objectReferenceValue    = spBallB;
        if (spBallY  != null) mechSO.FindProperty("_spriteBallY").objectReferenceValue    = spBallY;
        if (spBallM  != null) mechSO.FindProperty("_spriteBallM").objectReferenceValue    = spBallM;
        if (spLid    != null) mechSO.FindProperty("_spriteLid").objectReferenceValue      = spLid;
        if (spLock   != null) mechSO.FindProperty("_spriteLockIcon").objectReferenceValue = spLock;
        if (spRotate != null) mechSO.FindProperty("_spriteRotateIcon").objectReferenceValue = spRotate;
        mechSO.ApplyModifiedProperties();

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // === InstructionPanel ===
        var ip = CreateInstructionPanel(canvasObj.transform, jpFont);

        // === HUD ===
        var stageText  = CT(canvasObj.transform, "StageText",  "Stage 1 / 5", 36, jpFont,
            new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(280,50), new Vector2(10,-15));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        var scoreText  = CT(canvasObj.transform, "ScoreText",  "Score: 0", 32, jpFont,
            new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(260,50), new Vector2(-10,-15));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);

        var moveText   = CT(canvasObj.transform, "MoveCountText", "Moves: 0", 28, jpFont,
            new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(200,44), new Vector2(0,-15));
        moveText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        moveText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var comboText  = CT(canvasObj.transform, "ComboText", "Combo x2!", 34, jpFont,
            new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(280,50), new Vector2(0,-65));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.1f);
        comboText.SetActive(false);

        // Timer panel (hidden by default, shown on Stage 5)
        var timerPanel = new GameObject("TimerPanel");
        timerPanel.transform.SetParent(canvasObj.transform, false);
        var timerPanelRt = timerPanel.AddComponent<RectTransform>();
        timerPanelRt.anchorMin = new Vector2(0.5f,1f); timerPanelRt.anchorMax = new Vector2(0.5f,1f);
        timerPanelRt.pivot = new Vector2(0.5f,1f); timerPanelRt.sizeDelta = new Vector2(200,50);
        timerPanelRt.anchoredPosition = new Vector2(0,-15);
        timerPanel.SetActive(false);
        var timerText = CT(timerPanel.transform, "TimerText", "Time: 120", 36, jpFont,
            Vector2.zero, Vector2.one, new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        timerText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // === Bottom Buttons ===
        var undoBtn  = CB(canvasObj.transform, "UndoButton", "Undo", jpFont,
            new Vector2(0.25f,0f), new Vector2(0.25f,0f), new Vector2(0.5f,0f),
            new Vector2(160,55), new Vector2(0,75), new Color(0.3f,0.5f,0.8f));
        var resetBtn = CB(canvasObj.transform, "ResetButton", "Reset", jpFont,
            new Vector2(0.75f,0f), new Vector2(0.75f,0f), new Vector2(0.5f,0f),
            new Vector2(160,55), new Vector2(0,75), new Color(0.6f,0.4f,0.2f));
        var menuBtn  = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0.5f,0f),
            new Vector2(160,55), new Vector2(0,15), new Color(0.3f,0.3f,0.35f));

        // === Stage Clear Panel ===
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f,0.1f,0.2f,0.95f), new Vector2(500,320));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(460,70), new Vector2(0,110));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,1f,0.5f);
        var scScore = CT(scPanel.transform, "StageClearScore", "Score: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(460,55), new Vector2(0,30));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.3f);
        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(260,60), new Vector2(0,-60), new Color(0.2f,0.6f,0.9f));
        var scMenuBtn = CB(scPanel.transform, "MenuButton2", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,55), new Vector2(0,-130), new Color(0.35f,0.35f,0.4f));
        scPanel.SetActive(false);

        // === Final Clear Panel ===
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel", new Color(0.05f,0.1f,0.2f,0.95f), new Vector2(520,360));
        var fcTitle = CT(fcPanel.transform, "FinalClearTitle", "全ステージクリア！", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(500,70), new Vector2(0,130));
        fcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.2f);
        var fcScore = CT(fcPanel.transform, "FinalScore", "Final Score: 0", 40, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(500,60), new Vector2(0,50));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.3f);
        var fcRetry = CB(fcPanel.transform, "RetryButton", "もう一度", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(220,60), new Vector2(-120,-80), new Color(0.2f,0.6f,0.9f));
        var fcMenu  = CB(fcPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(220,60), new Vector2(120,-80), new Color(0.35f,0.35f,0.4f));
        fcPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.1f,0.05f,0.05f,0.95f), new Vector2(480,320));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "行き詰まり！", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(460,70), new Vector2(0,110));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.3f,0.3f);
        var goScore = CT(goPanel.transform, "GameOverScore", "Score: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(460,55), new Vector2(0,30));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.3f);
        var goRetry = CB(goPanel.transform, "RetryButton", "リセット", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(-110,-60), new Color(0.2f,0.6f,0.9f));
        var goMenu  = CB(goPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(200,60), new Vector2(110,-60), new Color(0.35f,0.35f,0.4f));
        goPanel.SetActive(false);

        // === BallSort3DUI (child of GameManager) ===
        var uiObj = new GameObject("BallSort3DUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<BallSort3DUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.FindProperty("_mechanic").objectReferenceValue = mech;
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_moveCountText").objectReferenceValue = moveText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerPanel").objectReferenceValue = timerPanel;
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
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
        AddButtonOnClick(undoBtn.GetComponent<Button>(), ui, "OnUndoButtonClicked");
        AddButtonOnClick(resetBtn.GetComponent<Button>(), gm, "RetryGame");

        // Save scene
        string scenePath = "Assets/Scenes/043v2_BallSort3D.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup043v2] Scene created: " + scenePath);
    }

    static InstructionPanel CreateInstructionPanel(Transform canvasParent, TMP_FontAsset font)
    {
        var panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasParent, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.09f, 0.18f, 0.97f);
        var canvasComp = panelObj.AddComponent<Canvas>();
        canvasComp.overrideSorting = true;
        canvasComp.sortingOrder = 100;
        panelObj.AddComponent<GraphicRaycaster>();

        var ip = panelObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var titleObj = CT(panelObj.transform, "TitleText", "BallSort3D", 64, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,90), new Vector2(0,350));
        titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,0.85f,1f);

        var descObj = CT(panelObj.transform, "DescriptionText", "色付きボールを同じ色のチューブに揃えよう", 36, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,60), new Vector2(0,250));
        descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        descObj.GetComponent<TextMeshProUGUI>().color = new Color(0.9f,0.95f,1f);
        descObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = CT(panelObj.transform, "ControlsText", "チューブをタップしてボールを移動。同色か空のチューブに入れられる", 30, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,80), new Vector2(0,130));
        ctrlObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ctrlObj.GetComponent<TextMeshProUGUI>().color = new Color(0.7f,1f,0.7f);
        ctrlObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = CT(panelObj.transform, "GoalText", "全チューブを同じ色のボールだけにしたらクリア", 30, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,80), new Vector2(0,20));
        goalObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goalObj.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.85f,0.4f);
        goalObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtn = CB(panelObj.transform, "StartButton", "はじめる", font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300,70), new Vector2(0,-150), new Color(0.2f,0.55f,0.85f));

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
            else Debug.LogWarning($"[Setup043v2] Sprite not found: {path}");
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
