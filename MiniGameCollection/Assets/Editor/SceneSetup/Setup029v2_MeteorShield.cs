using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game029v2_MeteorShield;

public static class Setup029v2_MeteorShield
{
    [MenuItem("Assets/Setup/029v2 MeteorShield")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup029v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game029v2_MeteorShield/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.02f, 0.04f, 0.10f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Import sprites
        string[] spritePaths = {
            sp+"Background.png", sp+"Star.png", sp+"Shield.png",
            sp+"MeteorSmall.png", sp+"MeteorLarge.png", sp+"MeteorSplit.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg      = LoadSprite(sp + "Background.png");
        Sprite spStar    = LoadSprite(sp + "Star.png");
        Sprite spShield  = LoadSprite(sp + "Shield.png");
        Sprite spSmall   = LoadSprite(sp + "MeteorSmall.png");
        Sprite spLarge   = LoadSprite(sp + "MeteorLarge.png");
        Sprite spSplit   = LoadSprite(sp + "MeteorSplit.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            if (camera != null)
            {
                float camH = camera.orthographicSize * 2f;
                float camW = camH * camera.aspect;
                float scaleX = camW / (spBg.rect.width / spBg.pixelsPerUnit);
                float scaleY = camH / (spBg.rect.height / spBg.pixelsPerUnit);
                bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }

        // Ensure "Star" tag exists
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        bool hasStarTag = false;
        for (int i = 0; i < tagsProp.arraySize; i++) { if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Star") { hasStarTag = true; break; } }
        if (!hasStarTag) { tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize); tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Star"; tagManager.ApplyModifiedProperties(); }

        // Star (守る対象、画面下部中央)
        float camSize = camera != null ? camera.orthographicSize : 5f;
        float starY = -(camSize - 1.5f); // -3.5f
        var starObj = new GameObject("Star");
        starObj.transform.position = new Vector3(0f, starY, 0f);
        var starSr = starObj.AddComponent<SpriteRenderer>();
        starSr.sprite = spStar;
        starSr.sortingOrder = 5;
        if (spStar != null)
        {
            float sprSize = spStar.rect.width / spStar.pixelsPerUnit;
            float target = 0.8f;
            float s = target / sprSize;
            starObj.transform.localScale = new Vector3(s, s, 1f);
        }
        starObj.tag = "Star";
        var starCol = starObj.AddComponent<CircleCollider2D>();
        starCol.isTrigger = true;
        starCol.radius = 0.4f;

        // Shield (プレイヤー操作)
        float shieldY = -(camSize - 2.5f); // -2.5f
        var shieldObj = new GameObject("Shield");
        shieldObj.transform.position = new Vector3(0f, shieldY, 0f);
        var shieldSr = shieldObj.AddComponent<SpriteRenderer>();
        shieldSr.sprite = spShield;
        shieldSr.sortingOrder = 10;
        if (spShield != null)
        {
            float sprW = spShield.rect.width / spShield.pixelsPerUnit;
            float sprH = spShield.rect.height / spShield.pixelsPerUnit;
            float targetW = 3.0f;
            float targetH = 0.3f;
            shieldObj.transform.localScale = new Vector3(targetW / sprW, targetH / sprH, 1f);
        }
        var shieldCol = shieldObj.AddComponent<BoxCollider2D>();
        shieldCol.isTrigger = true;
        shieldCol.size = new Vector2(3.0f, 0.3f);
        shieldCol.offset = Vector2.zero;

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<MeteorShieldGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // ShieldController (on the Shield GameObject itself for correct transform usage)
        var shieldCtrl = shieldObj.AddComponent<ShieldController>();
        var scSO = new SerializedObject(shieldCtrl);
        scSO.FindProperty("_gameManager").objectReferenceValue = gm;
        scSO.FindProperty("_shieldSr").objectReferenceValue = shieldSr;
        scSO.FindProperty("_shieldCollider").objectReferenceValue = shieldCol;
        scSO.FindProperty("_shieldTransform").objectReferenceValue = shieldObj.transform;
        scSO.ApplyModifiedProperties();

        // MeteorSpawner (child of GameManager)
        var msObj = new GameObject("MeteorSpawner");
        msObj.transform.SetParent(gmObj.transform);
        var meteorSpawner = msObj.AddComponent<MeteorSpawner>();
        var msSO = new SerializedObject(meteorSpawner);
        msSO.FindProperty("_gameManager").objectReferenceValue = gm;
        msSO.FindProperty("_spriteSmall").objectReferenceValue = spSmall;
        msSO.FindProperty("_spriteLarge").objectReferenceValue = spLarge;
        msSO.FindProperty("_spriteSplit").objectReferenceValue = spSplit;
        msSO.ApplyModifiedProperties();

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

        // HP Bar (上部左)
        var hpBarObj = new GameObject("HPBar");
        hpBarObj.transform.SetParent(canvasObj.transform, false);
        var hpRt = hpBarObj.AddComponent<RectTransform>();
        hpRt.anchorMin = new Vector2(0f, 1f); hpRt.anchorMax = new Vector2(0f, 1f);
        hpRt.pivot = new Vector2(0f, 1f);
        hpRt.sizeDelta = new Vector2(500, 40);
        hpRt.anchoredPosition = new Vector2(20, -20);
        var hpSlider = hpBarObj.AddComponent<Slider>();
        hpSlider.minValue = 0f;
        hpSlider.maxValue = 1f;
        hpSlider.value = 1f;
        hpSlider.interactable = false;
        // Background
        var hpBg = new GameObject("Background"); hpBg.transform.SetParent(hpBarObj.transform, false);
        var hpBgRt = hpBg.AddComponent<RectTransform>();
        hpBgRt.anchorMin = Vector2.zero; hpBgRt.anchorMax = Vector2.one;
        hpBgRt.offsetMin = Vector2.zero; hpBgRt.offsetMax = Vector2.zero;
        var hpBgImg = hpBg.AddComponent<Image>(); hpBgImg.color = new Color(0.2f, 0.1f, 0.1f, 0.8f);
        // Fill Area
        var hpFillArea = new GameObject("Fill Area"); hpFillArea.transform.SetParent(hpBarObj.transform, false);
        var hpFaRt = hpFillArea.AddComponent<RectTransform>();
        hpFaRt.anchorMin = Vector2.zero; hpFaRt.anchorMax = Vector2.one;
        hpFaRt.offsetMin = new Vector2(5, 5); hpFaRt.offsetMax = new Vector2(-5, -5);
        var hpFill = new GameObject("Fill"); hpFill.transform.SetParent(hpFillArea.transform, false);
        var hpFillRt = hpFill.AddComponent<RectTransform>();
        hpFillRt.anchorMin = Vector2.zero; hpFillRt.anchorMax = Vector2.one;
        hpFillRt.offsetMin = Vector2.zero; hpFillRt.offsetMax = Vector2.zero;
        var hpFillImg = hpFill.AddComponent<Image>(); hpFillImg.color = new Color(0.2f, 0.9f, 0.3f, 1f);
        var hpSO2 = new SerializedObject(hpSlider);
        hpSO2.FindProperty("m_FillRect").objectReferenceValue = hpFillRt;
        hpSO2.ApplyModifiedProperties();
        // HP Label
        var hpLabel = CT(hpBarObj.transform, "HPLabel", "★ HP", 22, jpFont,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(80, 40), new Vector2(-45, 0));
        hpLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);
        hpLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // Score (右上)
        var scoreText = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(400, 55), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Stage (右上2行目)
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1/5", 28, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300, 45), new Vector2(-20, -70));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // Time (左上2行目)
        var timeText = CT(canvasObj.transform, "TimeText", "0s", 28, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(200, 45), new Vector2(20, -70));
        timeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        timeText.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 1f);

        // Combo Text (中央)
        var comboText = CT(canvasObj.transform, "ComboText", "", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 70), new Vector2(0, 400));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);

        // Menu Button (左下)
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(180, 65), new Vector2(20, 20), new Color(0.2f, 0.2f, 0.3f, 0.9f));

        // Question Button (右下) - InstructionPanel再表示
        var reShowBtn = CB(canvasObj.transform, "QuestionButton", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(65, 65), new Vector2(-20, 20), new Color(0.3f, 0.3f, 0.5f, 0.9f));

        // Stage Clear Panel
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel",
            new Color(0f, 0f, 0f, 0.85f), new Vector2(700, 400));
        scPanel.SetActive(false);
        var scTitle = CT(scPanel.transform, "SCTitle", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 80), new Vector2(0, 80));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);
        var scBonus = CT(scPanel.transform, "SCBonus", "", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), new Vector2(0, 0));
        scBonus.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scBonus.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Final Clear Panel
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel",
            new Color(0f, 0f, 0f, 0.85f), new Vector2(700, 500));
        fcPanel.SetActive(false);
        var fcTitle = CT(fcPanel.transform, "FCTitle", "完全防衛！", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 90), new Vector2(0, 120));
        fcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
        var fcScore = CT(fcPanel.transform, "FCScore", "", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 65), new Vector2(0, 20));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = Color.white;
        var fcRetryBtn = CB(fcPanel.transform, "FCRetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 70), new Vector2(-130, -130), new Color(0.2f, 0.5f, 0.8f));
        var fcMenuBtn = CB(fcPanel.transform, "FCMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 70), new Vector2(130, -130), new Color(0.3f, 0.3f, 0.5f));

        // Game Over Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0f, 0f, 0f, 0.85f), new Vector2(700, 500));
        goPanel.SetActive(false);
        var goTitle = CT(goPanel.transform, "GOTitle", "GAME OVER", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 90), new Vector2(0, 120));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);
        var goScore = CT(goPanel.transform, "GOScore", "", 42, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 65), new Vector2(0, 20));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = Color.white;
        var goRetryBtn = CB(goPanel.transform, "GORetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 70), new Vector2(-130, -130), new Color(0.8f, 0.3f, 0.2f));
        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 70), new Vector2(130, -130), new Color(0.3f, 0.3f, 0.5f));

        // InstructionPanel Canvas (最前面)
        var ipCanvasObj = new GameObject("InstructionCanvas");
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
        ipImg.color = new Color(0f, 0f, 0.05f, 0.92f);
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "MeteorShield", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 250));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.2f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), new Vector2(0, 130));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.95f, 0.9f, 0.85f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 140), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.8f, 0.75f);
        ipCtrl.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 65), new Vector2(0, -130));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 80), new Vector2(0, -270), new Color(0.2f, 0.4f, 0.8f));

        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // MeteorShieldUI (child of GameManager)
        var uiObj = new GameObject("MeteorShieldUI");
        uiObj.transform.SetParent(gmObj.transform);
        var msUI = uiObj.AddComponent<MeteorShieldUI>();
        var uiSO = new SerializedObject(msUI);
        uiSO.FindProperty("_hpBar").objectReferenceValue = hpSlider;
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timeText").objectReferenceValue = timeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearText").objectReferenceValue = scBonus.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalClearScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_shieldController").objectReferenceValue = shieldCtrl;
        gmSO.FindProperty("_meteorSpawner").objectReferenceValue = meteorSpawner;
        gmSO.FindProperty("_ui").objectReferenceValue = msUI;
        gmSO.ApplyModifiedProperties();

        // Button onClick wiring
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(), ip, "ReShow");
        AddButtonOnClick(goRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(fcRetryBtn.GetComponent<Button>(), gm, "RestartGame");
        AddButtonOnClick(fcMenuBtn.GetComponent<Button>(), gm, "ReturnToMenu");

        // Save scene
        string scenePath = "Assets/Scenes/029v2_MeteorShield.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup029v2] Scene created: " + scenePath);
    }

    static void EnsureSpriteImport(string path)
    {
        if (!File.Exists("MiniGameCollection/" + path) && !File.Exists(path)) return;
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
            else Debug.LogWarning($"[Setup029v2] Sprite not found: {path}");
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
