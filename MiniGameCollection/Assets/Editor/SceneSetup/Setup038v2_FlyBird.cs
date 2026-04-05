using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game038v2_FlyBird;

public static class Setup038v2_FlyBird
{
    [MenuItem("Assets/Setup/038v2 FlyBird")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup038v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game038v2_FlyBird/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.30f, 0.55f, 0.85f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Import sprites
        string[] paths = { sp+"background.png", sp+"bird.png", sp+"bird_flap.png",
                           sp+"pipe.png", sp+"pipe_cap.png", sp+"coin.png",
                           sp+"ground.png", sp+"wind_effect.png" };
        foreach (var path in paths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg       = LoadSprite(sp + "background.png");
        Sprite spBird     = LoadSprite(sp + "bird.png");
        Sprite spBirdFlap = LoadSprite(sp + "bird_flap.png");
        Sprite spPipe     = LoadSprite(sp + "pipe.png");
        Sprite spCap      = LoadSprite(sp + "pipe_cap.png");
        Sprite spCoin     = LoadSprite(sp + "coin.png");
        Sprite spGround   = LoadSprite(sp + "ground.png");
        Sprite spWind     = LoadSprite(sp + "wind_effect.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camAspect = camera != null ? camera.aspect : 0.5625f;
        float camWidth = camSize * camAspect;

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

        // Ground (top and bottom)
        CreateGround("Ground", new Vector3(0f, -camSize + 0.3f, 0f), new Vector3(camWidth * 2f, 0.6f, 1f), spGround);
        CreateGround("Ceiling", new Vector3(0f, camSize - 0.3f, 0f), new Vector3(camWidth * 2f, 0.6f, 1f), spGround);

        // === Bird ===
        var birdObj = new GameObject("Bird");
        birdObj.transform.position = new Vector3(-camWidth * 0.5f, 0f, 0f);
        birdObj.tag = "Player";

        var birdSr = birdObj.AddComponent<SpriteRenderer>();
        birdSr.sortingOrder = 5;
        if (spBird != null) birdSr.sprite = spBird;
        else birdSr.color = new Color(1f, 0.8f, 0f);
        birdObj.transform.localScale = Vector3.one * 0.5f;

        var birdRb = birdObj.AddComponent<Rigidbody2D>();
        birdRb.gravityScale = 2.0f;
        birdRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        birdRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var birdCol = birdObj.AddComponent<CircleCollider2D>();
        birdCol.radius = 0.9f;
        birdCol.isTrigger = true;

        // === GameManager root ===
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<FlyBirdGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // PipeSpawner (child of GameManager)
        var spawnerObj = new GameObject("PipeSpawner");
        spawnerObj.transform.SetParent(gmObj.transform);
        var spawner = spawnerObj.AddComponent<PipeSpawner>();
        // BirdController (on bird object)
        var birdCtrl = birdObj.AddComponent<BirdController>();
        var birdCtrlSO = new SerializedObject(birdCtrl);
        birdCtrlSO.FindProperty("_gameManager").objectReferenceValue = gm;
        if (spBird != null)     birdCtrlSO.FindProperty("_spriteNormal").objectReferenceValue = spBird;
        if (spBirdFlap != null) birdCtrlSO.FindProperty("_spriteFlap").objectReferenceValue = spBirdFlap;
        birdCtrlSO.ApplyModifiedProperties();

        var spawnerSO = new SerializedObject(spawner);
        spawnerSO.FindProperty("_gameManager").objectReferenceValue = gm;
        spawnerSO.FindProperty("_birdController").objectReferenceValue = birdCtrl;
        if (spPipe != null)  spawnerSO.FindProperty("_spritePipeBody").objectReferenceValue = spPipe;
        if (spCap != null)   spawnerSO.FindProperty("_spritePipeCap").objectReferenceValue = spCap;
        if (spCoin != null)  spawnerSO.FindProperty("_spriteCoin").objectReferenceValue = spCoin;
        if (spWind != null)  spawnerSO.FindProperty("_spriteWindEffect").objectReferenceValue = spWind;
        spawnerSO.ApplyModifiedProperties();

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // --- HUD ---
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 32, jpFont,
            new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(0f,1f),
            new Vector2(320, 50), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.95f, 0.4f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(1f,1f),
            new Vector2(320, 50), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var progressText = CT(canvasObj.transform, "ProgressText", "0 / 5", 40, jpFont,
            new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0.5f,1f),
            new Vector2(180, 55), new Vector2(0, -20));
        progressText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        progressText.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.5f);

        var comboText = CT(canvasObj.transform, "ComboText", "x2 COMBO!", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(400, 70), new Vector2(0, 100));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0f);
        comboText.SetActive(false);

        // Menu button (bottom center)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0.5f,0f),
            new Vector2(200, 55), new Vector2(0, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // ReShow button (bottom-right)
        var reShowBtn = CB(canvasObj.transform, "ReShowButton", "?", jpFont,
            new Vector2(1f,0f), new Vector2(1f,0f), new Vector2(1f,0f),
            new Vector2(80, 80), new Vector2(-20, 20), new Color(0.3f, 0.5f, 0.7f, 0.9f));

        // --- InstructionPanel ---
        var ip = CreateInstructionPanel(canvasObj.transform, jpFont);
        var ipWireSO = new SerializedObject(ip);
        ipWireSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipWireSO.ApplyModifiedProperties();

        // --- StageClear Panel ---
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.15f, 0.05f, 0.95f), new Vector2(700, 350));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージ クリア！", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(600, 80), new Vector2(0, 100));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scScore = CT(scPanel.transform, "StageClearScoreText", "Score: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(500, 60), new Vector2(0, 20));
        scScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScore.GetComponent<TextMeshProUGUI>().color = Color.white;
        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300, 60), new Vector2(0, -60), new Color(0.2f, 0.7f, 0.3f));
        var scMenuBtn = CB(scPanel.transform, "StageClearMenuBtn", "メニュー", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(200, 55), new Vector2(0, -130), new Color(0.35f, 0.35f, 0.45f));
        scPanel.SetActive(false);

        // --- FinalClear Panel ---
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel", new Color(0.02f, 0.10f, 0.06f, 0.97f), new Vector2(750, 420));
        var fcScore = CT(fcPanel.transform, "FinalScoreText", "全ステージクリア！\nScore: 0", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(680, 160), new Vector2(0, 80));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.6f);
        var fcRetry = CB(fcPanel.transform, "FinalRetryButton", "もう一度", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250, 60), new Vector2(-140, -80), new Color(0.2f, 0.7f, 0.3f));
        var fcMenu = CB(fcPanel.transform, "FinalMenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250, 60), new Vector2(140, -80), new Color(0.35f, 0.35f, 0.45f));
        fcPanel.SetActive(false);

        // --- GameOver Panel ---
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.15f, 0.03f, 0.03f, 0.97f), new Vector2(750, 420));
        var goScore = CT(goPanel.transform, "GameOverScoreText", "ゲームオーバー\nScore: 0", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(680, 160), new Vector2(0, 80));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.5f);
        var goRetry = CB(goPanel.transform, "GameOverRetryButton", "もう一度", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250, 60), new Vector2(-140, -80), new Color(0.7f, 0.3f, 0.2f));
        var goMenu = CB(goPanel.transform, "GameOverMenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250, 60), new Vector2(140, -80), new Color(0.35f, 0.35f, 0.45f));
        goPanel.SetActive(false);

        // --- FlyBirdUI ---
        var uiObj = new GameObject("FlyBirdUI");
        uiObj.transform.SetParent(gmObj.transform);
        var flyUI = uiObj.AddComponent<FlyBirdUI>();
        var uiSO = new SerializedObject(flyUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_progressText").objectReferenceValue = progressText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_birdController").objectReferenceValue = birdCtrl;
        gmSO.FindProperty("_pipeSpawner").objectReferenceValue = spawner;
        gmSO.FindProperty("_ui").objectReferenceValue = flyUI;
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
        string scenePath = "Assets/Scenes/038v2_FlyBird.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup038v2] Scene created: " + scenePath);
    }

    static void CreateGround(string name, Vector3 pos, Vector3 scale, Sprite sprite)
    {
        var obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.tag = "Ground";

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 4;
        if (sprite != null) sr.sprite = sprite;
        else sr.color = new Color(0.4f, 0.3f, 0.1f);

        var col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;
    }

    static InstructionPanel CreateInstructionPanel(Transform canvasParent, TMP_FontAsset font)
    {
        var panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasParent, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.25f, 0.5f, 0.97f);
        var canvasComp = panelObj.AddComponent<Canvas>();
        canvasComp.overrideSorting = true;
        canvasComp.sortingOrder = 100;
        panelObj.AddComponent<GraphicRaycaster>();

        var ip = panelObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var titleObj = CT(panelObj.transform, "TitleText", "FlyBird", 64, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900, 90), new Vector2(0, 350));
        titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);

        var descObj = CT(panelObj.transform, "DescriptionText", "タップで鳥を飛ばして障害物を避けよう", 36, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900, 60), new Vector2(0, 250));
        descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        descObj.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.95f, 1f);
        descObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = CT(panelObj.transform, "ControlsText", "タップで羽ばたき、離すと降下", 32, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900, 80), new Vector2(0, 130));
        ctrlObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ctrlObj.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);
        ctrlObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = CT(panelObj.transform, "GoalText", "障害物にぶつからずにゴールまで飛ぼう", 32, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900, 80), new Vector2(0, 20));
        goalObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goalObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.75f, 0.4f);
        goalObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtn = CB(panelObj.transform, "StartButton", "はじめる", font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300, 70), new Vector2(0, -150), new Color(0.2f, 0.5f, 0.9f));

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
            else Debug.LogWarning($"[Setup038v2] Sprite not found: {path}");
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
