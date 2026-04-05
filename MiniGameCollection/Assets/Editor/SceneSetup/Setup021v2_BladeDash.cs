using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game021v2_BladeDash;

public static class Setup021v2_BladeDash
{
    [MenuItem("Assets/Setup/021v2 BladeDash")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup021v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game021v2_BladeDash/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.03f, 0.08f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Ensure sprites imported
        string[] spritePaths = {
            sp+"Background.png", sp+"Player.png", sp+"PlayerSlide.png",
            sp+"Blade.png", sp+"BladeLow.png", sp+"BladeHigh.png", sp+"Coin.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        // Load sprites
        Sprite spBg          = LoadSprite(sp + "Background.png");
        Sprite spPlayer      = LoadSprite(sp + "Player.png");
        Sprite spPlayerSlide = LoadSprite(sp + "PlayerSlide.png");
        Sprite spBlade       = LoadSprite(sp + "Blade.png");
        Sprite spBladeLow    = LoadSprite(sp + "BladeLow.png");
        Sprite spBladeHigh   = LoadSprite(sp + "BladeHigh.png");
        Sprite spCoin        = LoadSprite(sp + "Coin.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float camH = 5f * 2f;
            float camAsp = Camera.main != null ? Camera.main.aspect : 9f / 16f;
            float camW = 5f * camAsp * 2f;
            float scaleX = camW / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camH / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<BladeDashGameManager>();

        // StageManager (child)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_totalStages").intValue = 5;
        smSO.ApplyModifiedProperties();

        // BladeRunner (child of GameManager)
        var brObj = new GameObject("BladeRunner");
        brObj.transform.SetParent(gmObj.transform);
        var br = brObj.AddComponent<BladeRunner>();

        // Player world-space sprite (child of BladeRunner)
        float camSize = 5f;
        float camAspect = Camera.main != null ? Camera.main.aspect : 9f/16f;
        float camWidth = camSize * camAspect;
        float laneSpacing = camWidth * 0.38f;
        float playerY = -camSize + 2.8f;

        var playerObj = new GameObject("Player");
        playerObj.transform.SetParent(brObj.transform);
        var playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sprite = spPlayer;
        playerSr.sortingOrder = 5;
        if (spPlayer != null)
        {
            float pWidth = 0.48f;
            float pHeight = 0.72f;
            float sx = pWidth / (spPlayer.rect.width / spPlayer.pixelsPerUnit);
            float sy = pHeight / (spPlayer.rect.height / spPlayer.pixelsPerUnit);
            playerObj.transform.localScale = new Vector3(sx, sy, 1f);
        }
        playerObj.transform.localPosition = new Vector3(0f, playerY, 0f);

        // Player slide sprite
        var playerSlideObj = new GameObject("PlayerSlide");
        playerSlideObj.transform.SetParent(brObj.transform);
        var playerSlideSr = playerSlideObj.AddComponent<SpriteRenderer>();
        playerSlideSr.sprite = spPlayerSlide;
        playerSlideSr.sortingOrder = 5;
        if (spPlayerSlide != null)
        {
            float pWidth = 0.65f;
            float pHeight = 0.35f;
            float sx = pWidth / (spPlayerSlide.rect.width / spPlayerSlide.pixelsPerUnit);
            float sy = pHeight / (spPlayerSlide.rect.height / spPlayerSlide.pixelsPerUnit);
            playerSlideObj.transform.localScale = new Vector3(sx, sy, 1f);
        }
        playerSlideObj.transform.localPosition = new Vector3(0f, playerY, 0f);
        playerSlideObj.SetActive(false);

        // Lane divider lines (world space)
        for (int i = 0; i < 2; i++)
        {
            float lx = (i == 0) ? -laneSpacing * 0.5f : laneSpacing * 0.5f;
            var lineObj = new GameObject($"LaneLine{i}");
            lineObj.transform.position = new Vector3(lx, 0f, 0f);
            var lineSr = lineObj.AddComponent<SpriteRenderer>();
            lineSr.color = new Color(0.4f, 0.4f, 0.7f, 0.4f);
            lineSr.sortingOrder = -5;
            // Create a thin white sprite via texture
            var tex = new Texture2D(2, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.SetPixel(1, 0, Color.white);
            tex.Apply();
            lineSr.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 1), new Vector2(0.5f, 0.5f));
            lineObj.transform.localScale = new Vector3(0.04f, camSize * 2f, 1f);
        }

        // ---- Canvas ----
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var evObj = new GameObject("EventSystem");
        evObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evObj.AddComponent<InputSystemUIInputModule>();

        // HUD: Stage (top-left)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(280, 55), new Vector2(20, -30));
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.85f, 1f);

        // Score (top-center)
        var scoreText = CT(canvasObj.transform, "ScoreText", "0", 42, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(240, 60), new Vector2(0, -28));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.95f, 0.3f);

        // Target score (next to score)
        var targetScoreText = CT(canvasObj.transform, "TargetScoreText", "/ 500", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1f),
            new Vector2(180, 50), new Vector2(140, -35));
        targetScoreText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f);

        // Combo (top-right)
        var comboText = CT(canvasObj.transform, "ComboText", "", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(160, 55), new Vector2(-20, -30));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        comboText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        // Near miss text (center)
        var nearMissText = CT(canvasObj.transform, "NearMissText", "NEAR MISS!", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 70), new Vector2(0, 200));
        nearMissText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        nearMissText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 0f, 0f);
        nearMissText.SetActive(false);

        // Bottom buttons
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(300, 65), new Vector2(-185, 55), new Color(0.25f, 0.25f, 0.4f));

        var reShowBtn = CB(canvasObj.transform, "ReShowInstructionBtn", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(75, 75), new Vector2(-10, 55), new Color(0.2f, 0.4f, 0.7f));

        // ---- Stage Clear Panel ----
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.04f, 0.06f, 0.18f, 0.95f),
            new Vector2(720, 400));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(680, 70), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scComboText = CT(scPanel.transform, "StageClearComboText", "コンボ: 0", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 55), new Vector2(0, 30));
        scComboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scComboText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.6f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(320, 70), new Vector2(0, 30), new Color(0.2f, 0.55f, 0.9f));
        scPanel.SetActive(false);

        // ---- Full Clear Panel ----
        var gcPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.04f, 0.12f, 0.05f, 0.95f),
            new Vector2(700, 460));
        var gcTitle = CT(gcPanel.transform, "ClearTitle", "全ステージクリア！", 50, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(680, 70), new Vector2(0, -30));
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.5f);
        var gcTotal = CT(gcPanel.transform, "ClearTotalScoreText", "Total: 0pt", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), new Vector2(0, 20));
        gcTotal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTotal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var gcRetryBtn = CB(gcPanel.transform, "GCRetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(250, 65), new Vector2(-140, 30), new Color(0.35f, 0.5f, 0.2f));
        var gcMenuBtn = CB(gcPanel.transform, "GCMenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 65), new Vector2(100, 30), new Color(0.25f, 0.25f, 0.4f));
        gcPanel.SetActive(false);

        // ---- Game Over Panel ----
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.18f, 0.03f, 0.03f, 0.95f),
            new Vector2(700, 400));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(660, 70), new Vector2(0, -30));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);
        var goScoreText = CT(goPanel.transform, "GameOverScoreText", "Score: 0pt", 38, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(560, 60), new Vector2(0, 30));
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.7f);
        var goRetryBtn = CB(goPanel.transform, "GORetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(240, 65), new Vector2(-140, 30), new Color(0.6f, 0.3f, 0.1f));
        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(280, 65), new Vector2(100, 30), new Color(0.25f, 0.25f, 0.4f));
        goPanel.SetActive(false);

        // ---- Instruction Panel ----
        var ipPanel = CreatePanel(canvasObj.transform, "InstructionPanel", new Color(0.05f, 0.03f, 0.12f, 0.97f),
            new Vector2(0, 0));
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "BladeDash", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 220));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.3f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), new Vector2(0, 110));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.95f, 0.9f, 1f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.85f, 1f);
        ipCtrl.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 65), new Vector2(0, -90));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 80), new Vector2(0, -220), new Color(0.75f, 0.2f, 0.1f));

        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // ---- BladeDashUI component ----
        var uiObj = new GameObject("BladeDashUI");
        uiObj.transform.SetParent(gmObj.transform);
        var bdUI = uiObj.AddComponent<BladeDashUI>();
        var uiSO = new SerializedObject(bdUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_targetScoreText").objectReferenceValue = targetScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nearMissText").objectReferenceValue = nearMissText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearComboText").objectReferenceValue = scComboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = gcTotal.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = gcRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_clearMenuButton").objectReferenceValue = gcMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire BladeRunner
        var brSO = new SerializedObject(br);
        brSO.FindProperty("_gameManager").objectReferenceValue = gm;
        brSO.FindProperty("_playerRenderer").objectReferenceValue = playerSr;
        brSO.FindProperty("_playerSlideRenderer").objectReferenceValue = playerSlideSr;
        brSO.FindProperty("_bladeNormalSprite").objectReferenceValue = spBlade;
        brSO.FindProperty("_bladeLowSprite").objectReferenceValue = spBladeLow;
        brSO.FindProperty("_bladeHighSprite").objectReferenceValue = spBladeHigh;
        brSO.FindProperty("_coinSprite").objectReferenceValue = spCoin;
        brSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_bladeRunner").objectReferenceValue = br;
        gmSO.FindProperty("_ui").objectReferenceValue = bdUI;
        gmSO.ApplyModifiedProperties();

        // ---- Button Events ----
        AddButtonOnClick(menuBtn.GetComponent<Button>(),    gm, "OnReturnToMenu");
        AddButtonOnClick(nextBtn.GetComponent<Button>(),    gm, "OnNextStage");
        AddButtonOnClick(goRetryBtn.GetComponent<Button>(), gm, "OnRetry");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(),  gm, "OnReturnToMenu");
        AddButtonOnClick(gcRetryBtn.GetComponent<Button>(), gm, "OnRetry");
        AddButtonOnClick(gcMenuBtn.GetComponent<Button>(),  gm, "OnReturnToMenu");

        // ---- Save Scene ----
        string scenePath = "Assets/Scenes/021v2_BladeDash.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup021v2] Scene created: " + scenePath);
    }

    static void EnsureSpriteImport(string path)
    {
        if (!File.Exists(path)) return;
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
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
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
            else Debug.LogWarning($"[Setup021v2] Sprite not found: {path}");
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
