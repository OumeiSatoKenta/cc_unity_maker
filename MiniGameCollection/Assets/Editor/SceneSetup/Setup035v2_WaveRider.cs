using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game035v2_WaveRider;

public static class Setup035v2_WaveRider
{
    [MenuItem("Assets/Setup/035v2 WaveRider")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup035v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game035v2_WaveRider/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.10f, 0.20f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }
        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camW = camSize * (camera != null ? camera.aspect : 0.5625f);

        // Import sprites
        string[] spritePaths = {
            sp+"Background.png", sp+"Surfer.png", sp+"Rock.png",
            sp+"Whirlpool.png", sp+"Shield.png", sp+"WaveIndicator.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg         = LoadSprite(sp + "Background.png");
        Sprite spSurfer     = LoadSprite(sp + "Surfer.png");
        Sprite spRock       = LoadSprite(sp + "Rock.png");
        Sprite spWhirlpool  = LoadSprite(sp + "Whirlpool.png");
        Sprite spShield     = LoadSprite(sp + "Shield.png");

        // Background (scrolling ocean)
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float scaleX = camW * 2f / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camSize * 2f / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY * 1.2f, 1f);
        }

        // === Rock Prefab ===
        string prefabDir = "Assets/Resources/Prefabs/Game035v2_WaveRider";
        Directory.CreateDirectory(prefabDir);

        var rockGo = new GameObject("Rock");
        var rockSr = rockGo.AddComponent<SpriteRenderer>();
        rockSr.sprite = spRock;
        rockSr.sortingOrder = 5;
        if (spRock != null)
        {
            float targetSize = 0.8f;
            float sprW = spRock.rect.width / spRock.pixelsPerUnit;
            float s = targetSize / sprW;
            rockGo.transform.localScale = new Vector3(s, s, 1f);
        }
        string rockPrefabPath = prefabDir + "/Rock.prefab";
        var rockPrefab = PrefabUtility.SaveAsPrefabAsset(rockGo, rockPrefabPath);
        Object.DestroyImmediate(rockGo);

        // === Whirlpool Prefab ===
        var wpGo = new GameObject("Whirlpool");
        var wpSr = wpGo.AddComponent<SpriteRenderer>();
        wpSr.sprite = spWhirlpool;
        wpSr.sortingOrder = 4;
        if (spWhirlpool != null)
        {
            float targetSize = 1.0f;
            float sprW = spWhirlpool.rect.width / spWhirlpool.pixelsPerUnit;
            float s = targetSize / sprW;
            wpGo.transform.localScale = new Vector3(s, s, 1f);
        }
        string wpPrefabPath = prefabDir + "/Whirlpool.prefab";
        var wpPrefab = PrefabUtility.SaveAsPrefabAsset(wpGo, wpPrefabPath);
        Object.DestroyImmediate(wpGo);
        AssetDatabase.Refresh();

        // Load prefabs
        var rockPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(rockPrefabPath);
        var wpPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(wpPrefabPath);

        // === Surfer ===
        float laneSpacing = camW * 0.42f;
        float surferY = -camSize + 2.5f;
        var surferObj = new GameObject("Surfer");
        surferObj.transform.position = new Vector3(0f, surferY, 0f);
        var surferSr = surferObj.AddComponent<SpriteRenderer>();
        surferSr.sprite = spSurfer;
        surferSr.sortingOrder = 10;
        if (spSurfer != null)
        {
            float targetH = 1.2f;
            float sprH = spSurfer.rect.height / spSurfer.pixelsPerUnit;
            float s = targetH / sprH;
            surferObj.transform.localScale = new Vector3(s, s, 1f);
        }

        // Shield visual (child of Surfer)
        var shieldGo = new GameObject("ShieldVisual");
        shieldGo.transform.SetParent(surferObj.transform);
        shieldGo.transform.localPosition = Vector3.zero;
        shieldGo.transform.localScale = Vector3.one * 1.8f;
        var shieldSr = shieldGo.AddComponent<SpriteRenderer>();
        shieldSr.sprite = spShield;
        shieldSr.sortingOrder = 11;
        shieldSr.color = new Color(0.4f, 0.7f, 1f, 0.7f);
        shieldGo.SetActive(false);

        // === GameManager root ===
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<WaveRiderGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // WaveMechanic (child of GameManager)
        var mechObj = new GameObject("WaveMechanic");
        mechObj.transform.SetParent(gmObj.transform);
        var mechanic = mechObj.AddComponent<WaveMechanic>();
        var mechSO = new SerializedObject(mechanic);
        mechSO.FindProperty("_gameManager").objectReferenceValue = gm;
        mechSO.FindProperty("_surferTransform").objectReferenceValue = surferObj.transform;
        mechSO.FindProperty("_surferRenderer").objectReferenceValue = surferSr;
        mechSO.FindProperty("_rockPrefab").objectReferenceValue = rockPrefabAsset;
        mechSO.FindProperty("_whirlpoolPrefab").objectReferenceValue = wpPrefabAsset;
        mechSO.FindProperty("_shieldVisual").objectReferenceValue = shieldGo.transform;
        mechSO.FindProperty("_mainCamera").objectReferenceValue = camera;
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

        // --- HUD ---
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 32, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 50), new Vector2(20, -20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.85f, 1f);

        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(320, 50), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var distText = CT(canvasObj.transform, "DistanceText", "残り 200m", 32, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(300, 50), new Vector2(0, -20));
        distText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        distText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.95f, 1f);

        var comboText = CT(canvasObj.transform, "ComboText", "x2 COMBO!", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 80), new Vector2(0, 200));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0f);
        comboText.SetActive(false);

        // Shield icon
        var shieldIconGo = new GameObject("ShieldIcon");
        shieldIconGo.transform.SetParent(canvasObj.transform, false);
        var shieldIconRt = shieldIconGo.AddComponent<RectTransform>();
        shieldIconRt.anchorMin = new Vector2(0f, 1f);
        shieldIconRt.anchorMax = new Vector2(0f, 1f);
        shieldIconRt.pivot = new Vector2(0f, 1f);
        shieldIconRt.sizeDelta = new Vector2(60, 60);
        shieldIconRt.anchoredPosition = new Vector2(20, -75);
        var shieldIconImg = shieldIconGo.AddComponent<Image>();
        if (spShield != null) shieldIconImg.sprite = spShield;
        shieldIconImg.color = new Color(0.4f, 0.8f, 1f, 0.9f);
        shieldIconGo.SetActive(false);

        // Menu button (bottom center)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 55), new Vector2(0, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // ReShow button (bottom-right)
        var reShowBtn = CB(canvasObj.transform, "ReShowButton", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(80, 80), new Vector2(-20, 20), new Color(0.2f, 0.5f, 0.8f, 0.9f));

        // --- InstructionPanel ---
        var ip = CreateInstructionPanel(canvasObj.transform, jpFont);
        var ipWireSO = new SerializedObject(ip);
        ipWireSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipWireSO.ApplyModifiedProperties();

        // --- StageClear Panel ---
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.12f, 0.22f, 0.95f), new Vector2(700, 350));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージ クリア！", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 80), new Vector2(0, 100));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 60), new Vector2(0, -60), new Color(0.2f, 0.7f, 0.3f, 1f));
        var scMenuBtn = CB(scPanel.transform, "StageClearMenuBtn", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 55), new Vector2(0, -130), new Color(0.35f, 0.35f, 0.45f, 1f));
        scPanel.SetActive(false);

        // --- FinalClear Panel ---
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel", new Color(0.03f, 0.12f, 0.08f, 0.97f), new Vector2(750, 420));
        var fcScore = CT(fcPanel.transform, "FinalScoreText", "全ステージクリア！\nScore: 0", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(680, 160), new Vector2(0, 80));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.6f);
        var fcRetryBtn = CB(fcPanel.transform, "FinalRetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 60), new Vector2(-140, -80), new Color(0.2f, 0.7f, 0.3f, 1f));
        var fcMenuBtn = CB(fcPanel.transform, "FinalMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 60), new Vector2(140, -80), new Color(0.35f, 0.35f, 0.45f, 1f));
        fcPanel.SetActive(false);

        // --- GameOver Panel ---
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.15f, 0.05f, 0.05f, 0.97f), new Vector2(750, 420));
        var goScore = CT(goPanel.transform, "GameOverScoreText", "ゲームオーバー\nScore: 0", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(680, 160), new Vector2(0, 80));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.5f);
        var goRetryBtn = CB(goPanel.transform, "GameOverRetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 60), new Vector2(-140, -80), new Color(0.7f, 0.3f, 0.2f, 1f));
        var goMenuBtn = CB(goPanel.transform, "GameOverMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 60), new Vector2(140, -80), new Color(0.35f, 0.35f, 0.45f, 1f));
        goPanel.SetActive(false);

        // --- WaveRiderUI ---
        var uiObj = new GameObject("WaveRiderUI");
        uiObj.transform.SetParent(gmObj.transform);
        var wrUI = uiObj.AddComponent<WaveRiderUI>();
        var uiSO = new SerializedObject(wrUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_distanceText").objectReferenceValue = distText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_shieldIcon").objectReferenceValue = shieldIconImg;
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearText").objectReferenceValue = scTitle.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = scNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_mechanic").objectReferenceValue = mechanic;
        gmSO.FindProperty("_ui").objectReferenceValue = wrUI;
        gmSO.ApplyModifiedProperties();

        // Button wiring
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(scMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(fcMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        // reShowBtn is wired via _helpButton field; InstructionPanel.Show() registers the click internally

        // Save scene
        string scenePath = "Assets/Scenes/035v2_WaveRider.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup035v2] Scene created: " + scenePath);
    }

    static InstructionPanel CreateInstructionPanel(Transform canvasParent, TMP_FontAsset font)
    {
        var panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasParent, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.10f, 0.22f, 0.97f);

        var canvasComp = panelObj.AddComponent<Canvas>();
        canvasComp.overrideSorting = true;
        canvasComp.sortingOrder = 100;
        panelObj.AddComponent<GraphicRaycaster>();

        var ip = panelObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var titleObj = CT(panelObj.transform, "TitleText", "WaveRider", 64, font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 350));
        titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.8f, 1f);

        var descObj = CT(panelObj.transform, "DescriptionText", "波に乗ってトリックを決めながらゴールを目指そう", 36, font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 60), new Vector2(0, 250));
        descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        descObj.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);
        descObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = CT(panelObj.transform, "ControlsText", "左右タップでレーン移動、タップでジャンプ・トリック", 32, font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), new Vector2(0, 130));
        ctrlObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ctrlObj.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 0.7f);
        ctrlObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = CT(panelObj.transform, "GoalText", "岩を避けてゴールまで走破しよう", 32, font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), new Vector2(0, 20));
        goalObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goalObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.4f);
        goalObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtn = CB(panelObj.transform, "StartButton", "はじめる", font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 70), new Vector2(0, -150), new Color(0.2f, 0.6f, 0.9f, 1f));

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
            else Debug.LogWarning($"[Setup035v2] Sprite not found: {path}");
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
