using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game031v2_BounceKing;

public static class Setup031v2_BounceKing
{
    [MenuItem("Assets/Setup/031v2 BounceKing")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup031v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game031v2_BounceKing/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.2f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"Background.png", sp+"Paddle.png", sp+"Ball.png",
            sp+"BlockNormal.png", sp+"BlockHard.png", sp+"BlockBoss.png",
            sp+"ItemExpand.png", sp+"ItemMultiBall.png", sp+"ItemShrink.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg      = LoadSprite(sp + "Background.png");
        Sprite spPaddle  = LoadSprite(sp + "Paddle.png");
        Sprite spBall    = LoadSprite(sp + "Ball.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camW    = camSize * (camera != null ? camera.aspect : 0.5625f);

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float scaleX = camW * 2f / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camSize * 2f / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // Walls
        // Top Wall
        var topWall = new GameObject("WallTop");
        var topCol = topWall.AddComponent<BoxCollider2D>();
        topWall.tag = "Wall";
        topWall.transform.position = new Vector3(0f, camSize + 0.5f, 0f);
        topCol.size = new Vector2(camW * 2f + 2f, 1f);

        // Left Wall
        var leftWall = new GameObject("WallLeft");
        var leftCol = leftWall.AddComponent<BoxCollider2D>();
        leftWall.tag = "Wall";
        leftWall.transform.position = new Vector3(-camW - 0.5f, 0f, 0f);
        leftCol.size = new Vector2(1f, camSize * 2f + 2f);

        // Right Wall
        var rightWall = new GameObject("WallRight");
        var rightCol = rightWall.AddComponent<BoxCollider2D>();
        rightWall.tag = "Wall";
        rightWall.transform.position = new Vector3(camW + 0.5f, 0f, 0f);
        rightCol.size = new Vector2(1f, camSize * 2f + 2f);

        // Paddle
        float paddleY = -camSize + 2.8f;
        var paddleObj = new GameObject("Paddle");
        paddleObj.tag = "Paddle";
        paddleObj.transform.position = new Vector3(0f, paddleY, 0f);
        var paddleSr = paddleObj.AddComponent<SpriteRenderer>();
        paddleSr.sortingOrder = 5;
        if (spPaddle != null)
        {
            paddleSr.sprite = spPaddle;
            float targetW = 2.0f;
            float targetH = 0.28f;
            float scX = targetW / (spPaddle.rect.width / spPaddle.pixelsPerUnit);
            float scY = targetH / (spPaddle.rect.height / spPaddle.pixelsPerUnit);
            paddleObj.transform.localScale = new Vector3(scX, scY, 1f);
        }
        var paddleCol = paddleObj.AddComponent<BoxCollider2D>();
        paddleCol.size = new Vector2(1f, 1f); // scaled by transform
        var paddleCtrl = paddleObj.AddComponent<PaddleController>();

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<BounceKingGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // BlockManager (child of GameManager)
        var bmObj = new GameObject("BlockManager");
        bmObj.transform.SetParent(gmObj.transform);
        var blockMgr = bmObj.AddComponent<BlockManager>();
        var bmSO = new SerializedObject(blockMgr);
        bmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        bmSO.ApplyModifiedProperties();

        // Canvas (main HUD)
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- HUD Elements ---
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 32, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 50), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.85f, 1f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(380, 50), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var lifeText = CT(canvasObj.transform, "LifeText", "♥♥♥", 36, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 50), new Vector2(0, -20));
        lifeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        lifeText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.4f);

        var blocksText = CT(canvasObj.transform, "BlocksText", "Blocks: 0", 28, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(240, 45), new Vector2(20, -65));
        blocksText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        blocksText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var comboText = CT(canvasObj.transform, "ComboText", "Combo x1", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 80), new Vector2(0, 100));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        comboText.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        // Back to menu button (bottom)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(180, 55), new Vector2(0, 15), new Color(0.2f, 0.25f, 0.35f));

        // --- Stage Clear Panel ---
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel",
            new Color(0f, 0.05f, 0.1f, 0.88f), new Vector2(700, 500));
        scPanel.SetActive(false);

        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 140));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);

        var scText = CT(scPanel.transform, "StageClearText", "", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 40));
        scText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scText.GetComponent<TextMeshProUGUI>().color = Color.white;
        scText.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 75), new Vector2(0, -100), new Color(0.1f, 0.5f, 0.2f));

        var scMenuBtn = CB(scPanel.transform, "SCMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 60), new Vector2(0, -190), new Color(0.2f, 0.25f, 0.35f));

        // --- Final Clear Panel ---
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel",
            new Color(0f, 0.05f, 0.15f, 0.92f), new Vector2(700, 500));
        fcPanel.SetActive(false);

        var fcTitle = CT(fcPanel.transform, "FCTitle", "全ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 80), new Vector2(0, 150));
        fcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);

        var fcScore = CT(fcPanel.transform, "FCScore", "Final Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), new Vector2(0, 40));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = Color.white;

        var fcRetryBtn = CB(fcPanel.transform, "FCRetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 70), new Vector2(-130, -110), new Color(0.2f, 0.5f, 0.8f));

        var fcMenuBtn = CB(fcPanel.transform, "FCMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 70), new Vector2(130, -110), new Color(0.2f, 0.25f, 0.35f));

        // --- Game Over Panel ---
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0.1f, 0f, 0f, 0.9f), new Vector2(700, 500));
        goPanel.SetActive(false);

        var goTitle = CT(goPanel.transform, "GOTitle", "ゲームオーバー", 52, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 80), new Vector2(0, 150));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.2f);

        var goScore = CT(goPanel.transform, "GOScore", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 70), new Vector2(0, 40));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = Color.white;

        var goRetryBtn = CB(goPanel.transform, "GORetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 70), new Vector2(-130, -110), new Color(0.6f, 0.2f, 0.2f));

        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 70), new Vector2(130, -110), new Color(0.2f, 0.25f, 0.35f));

        // --- InstructionPanel Canvas ---
        var ipCanvasObj = new GameObject("InstructionPanelCanvas");
        var ipCanvas = ipCanvasObj.AddComponent<Canvas>();
        ipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvas.sortingOrder = 100;
        var ipScaler = ipCanvasObj.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvasObj.AddComponent<GraphicRaycaster>();

        var ipPanel = new GameObject("InstructionPanel");
        ipPanel.transform.SetParent(ipCanvasObj.transform, false);
        var ipImg = ipPanel.AddComponent<Image>();
        ipImg.color = new Color(0f, 0.03f, 0.08f, 0.93f);
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "BounceKing", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 260));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), new Vector2(0, 140));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.95f, 0.9f, 0.85f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 160), new Vector2(0, -10));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.8f, 0.75f);
        ipCtrl.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 65), new Vector2(0, -160));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 80), new Vector2(0, -300), new Color(0.2f, 0.4f, 0.8f));

        // ? button (re-show instruction)
        var reShowBtn = CB(canvasObj.transform, "HelpButton", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(70, 70), new Vector2(-15, 80), new Color(0.2f, 0.3f, 0.5f));

        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // BounceKingUI (child of GameManager)
        var uiObj = new GameObject("BounceKingUI");
        uiObj.transform.SetParent(gmObj.transform);
        var bkUI = uiObj.AddComponent<BounceKingUI>();
        var uiSO = new SerializedObject(bkUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_lifeText").objectReferenceValue = lifeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_blocksText").objectReferenceValue = blocksText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearText").objectReferenceValue = scText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = scNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_paddle").objectReferenceValue = paddleCtrl;
        gmSO.FindProperty("_blockManager").objectReferenceValue = blockMgr;
        gmSO.FindProperty("_ui").objectReferenceValue = bkUI;
        gmSO.FindProperty("_ballSprite").objectReferenceValue = spBall;
        gmSO.ApplyModifiedProperties();

        // Wire PaddleController
        var paddleSO = new SerializedObject(paddleCtrl);
        paddleSO.FindProperty("_gameManager").objectReferenceValue = gm;
        paddleSO.ApplyModifiedProperties();

        // Button onClick wiring
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(), ip, "ReShow");
        AddButtonOnClick(scNextBtn.GetComponent<Button>(), bkUI, "OnNextStagePressed");
        AddButtonOnClick(scMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(fcRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(fcMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(goRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");

        // Save scene
        string scenePath = "Assets/Scenes/031v2_BounceKing.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup031v2] Scene created: " + scenePath);
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
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = sizeDelta; rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = color;
        return go;
    }

    static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null) sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            else Debug.LogWarning($"[Setup031v2] Sprite not found: {path}");
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
