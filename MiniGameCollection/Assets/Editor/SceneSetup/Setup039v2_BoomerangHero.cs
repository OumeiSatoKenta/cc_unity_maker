using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game039v2_BoomerangHero;

public static class Setup039v2_BoomerangHero
{
    [MenuItem("Assets/Setup/039v2 BoomerangHero")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup039v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game039v2_BoomerangHero/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.10f, 0.04f, 0.15f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"background.png", sp+"player.png", sp+"boomerang.png",
            sp+"enemy_normal.png", sp+"enemy_shielded.png", sp+"enemy_moving.png",
            sp+"wall.png", sp+"shield.png", sp+"hit_effect.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg           = LoadSprite(sp + "background.png");
        Sprite spPlayer       = LoadSprite(sp + "player.png");
        Sprite spBoomerang    = LoadSprite(sp + "boomerang.png");
        Sprite spEnemyNormal  = LoadSprite(sp + "enemy_normal.png");
        Sprite spEnemyShield  = LoadSprite(sp + "enemy_shielded.png");
        Sprite spEnemyMoving  = LoadSprite(sp + "enemy_moving.png");
        Sprite spWall         = LoadSprite(sp + "wall.png");
        Sprite spShield       = LoadSprite(sp + "shield.png");
        Sprite spHitEffect    = LoadSprite(sp + "hit_effect.png");

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

        // Player object (static, just visual)
        var playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(-camWidth * 0.55f, 0f, 0f);
        var playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sortingOrder = 5;
        playerSr.sprite = spPlayer;
        playerObj.transform.localScale = Vector3.one * 0.7f;

        // === GameManager root ===
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<BoomerangHeroGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // BoomerangMechanic (child of GameManager)
        var mechObj = new GameObject("BoomerangMechanic");
        mechObj.transform.SetParent(gmObj.transform);
        var mech = mechObj.AddComponent<BoomerangMechanic>();

        // TrajectoryLine
        var lineObj = new GameObject("TrajectoryLine");
        lineObj.transform.SetParent(mechObj.transform);
        var lr = lineObj.AddComponent<LineRenderer>();
        lr.startWidth = 0.05f;
        lr.endWidth = 0.02f;
        lr.useWorldSpace = true;
        lr.enabled = false;
        // Set trajectory line material
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(1f, 0.9f, 0.3f, 0.8f);
        lr.endColor = new Color(1f, 0.9f, 0.3f, 0.1f);
        lr.positionCount = 0;

        // Wire BoomerangMechanic
        var mechSO = new SerializedObject(mech);
        mechSO.FindProperty("_gameManager").objectReferenceValue = gm;
        mechSO.FindProperty("_trajectoryLine").objectReferenceValue = lr;
        if (spBoomerang != null)   mechSO.FindProperty("_spriteBoomerang").objectReferenceValue = spBoomerang;
        if (spEnemyNormal != null) mechSO.FindProperty("_spriteEnemyNormal").objectReferenceValue = spEnemyNormal;
        if (spEnemyShield != null) mechSO.FindProperty("_spriteEnemyShielded").objectReferenceValue = spEnemyShield;
        if (spEnemyMoving != null) mechSO.FindProperty("_spriteEnemyMoving").objectReferenceValue = spEnemyMoving;
        if (spWall != null)        mechSO.FindProperty("_spriteWall").objectReferenceValue = spWall;
        if (spHitEffect != null)   mechSO.FindProperty("_spriteHitEffect").objectReferenceValue = spHitEffect;
        if (spShield != null)      mechSO.FindProperty("_spriteShield").objectReferenceValue = spShield;
        mechSO.ApplyModifiedProperties();

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

        // --- HUD (top) ---
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 32, jpFont,
            new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(0f,1f),
            new Vector2(300, 50), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(1f,1f),
            new Vector2(300, 50), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var ammoText = CT(canvasObj.transform, "AmmoText", "弾: 3 / 3", 34, jpFont,
            new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(0f,1f),
            new Vector2(250, 50), new Vector2(20, -70));
        ammoText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        ammoText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.3f);

        var enemyCountText = CT(canvasObj.transform, "EnemyCountText", "敵: 2", 34, jpFont,
            new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(1f,1f),
            new Vector2(200, 50), new Vector2(-20, -70));
        enemyCountText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        enemyCountText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.5f);

        // Score popup (center)
        var scorePopup = CT(canvasObj.transform, "ScorePopupText", "+30", 56, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300, 80), new Vector2(0, 50));
        scorePopup.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scorePopup.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f, 1f);
        scorePopup.SetActive(false);

        // --- Bottom buttons ---
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0.5f,0f),
            new Vector2(200, 55), new Vector2(0, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));

        var reShowBtn = CB(canvasObj.transform, "ReShowButton", "?", jpFont,
            new Vector2(1f,0f), new Vector2(1f,0f), new Vector2(1f,0f),
            new Vector2(80, 80), new Vector2(-20, 20), new Color(0.3f, 0.5f, 0.7f, 0.9f));

        // --- InstructionPanel ---
        var ip = CreateInstructionPanel(canvasObj.transform, jpFont);
        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // --- StageClear Panel ---
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.15f, 0.05f, 0.95f), new Vector2(700, 380));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージ クリア！", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(600, 80), new Vector2(0, 110));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scScore = CT(scPanel.transform, "StageClearScoreText", "Score: 0", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(500, 60), new Vector2(0, 25));
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
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel", new Color(0.02f, 0.10f, 0.06f, 0.97f), new Vector2(750, 440));
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
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.15f, 0.03f, 0.03f, 0.97f), new Vector2(750, 440));
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

        // --- BoomerangHeroUI ---
        var uiObj = new GameObject("BoomerangHeroUI");
        uiObj.transform.SetParent(gmObj.transform);
        var bhUI = uiObj.AddComponent<BoomerangHeroUI>();
        var uiSO = new SerializedObject(bhUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_ammoText").objectReferenceValue = ammoText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_enemyCountText").objectReferenceValue = enemyCountText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scorePopupText").objectReferenceValue = scorePopup.GetComponent<TextMeshProUGUI>();
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
        gmSO.FindProperty("_mechanic").objectReferenceValue = mech;
        gmSO.FindProperty("_ui").objectReferenceValue = bhUI;
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
        string scenePath = "Assets/Scenes/039v2_BoomerangHero.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup039v2] Scene created: " + scenePath);
    }

    static InstructionPanel CreateInstructionPanel(Transform canvasParent, TMP_FontAsset font)
    {
        var panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasParent, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.04f, 0.18f, 0.97f);
        var canvasComp = panelObj.AddComponent<Canvas>();
        canvasComp.overrideSorting = true;
        canvasComp.sortingOrder = 100;
        panelObj.AddComponent<GraphicRaycaster>();

        var ip = panelObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var titleObj = CT(panelObj.transform, "TitleText", "BoomerangHero", 64, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900, 90), new Vector2(0, 350));
        titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        var descObj = CT(panelObj.transform, "DescriptionText", "ブーメランを壁で反射させて敵を倒そう", 36, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900, 60), new Vector2(0, 250));
        descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        descObj.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.95f, 1f);
        descObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = CT(panelObj.transform, "ControlsText", "ドラッグで角度と力を調整、リリースで発射", 32, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900, 80), new Vector2(0, 130));
        ctrlObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ctrlObj.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);
        ctrlObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = CT(panelObj.transform, "GoalText", "限られた弾数で全ての敵を倒そう", 32, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900, 80), new Vector2(0, 20));
        goalObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goalObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.75f, 0.4f);
        goalObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtn = CB(panelObj.transform, "StartButton", "はじめる", font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300, 70), new Vector2(0, -150), new Color(0.4f, 0.2f, 0.7f));

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
            else Debug.LogWarning($"[Setup039v2] Sprite not found: {path}");
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
