using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game034v2_DropZone;

public static class Setup034v2_DropZone
{
    [MenuItem("Assets/Setup/034v2 DropZone")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup034v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game034v2_DropZone/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.07f, 0.10f, 0.18f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }
        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camW = camSize * (camera != null ? camera.aspect : 0.5625f);

        // Import sprites
        string[] spritePaths = {
            sp+"Background.png",
            sp+"ItemFruit.png", sp+"ItemTrash.png", sp+"ItemRecycle.png",
            sp+"ItemTrickyFruit.png", sp+"ItemTrickyTrash.png", sp+"ItemBonus.png",
            sp+"ZoneFruit.png", sp+"ZoneTrash.png", sp+"ZoneRecycle.png", sp+"ZoneBonus.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg           = LoadSprite(sp + "Background.png");
        Sprite spFruit        = LoadSprite(sp + "ItemFruit.png");
        Sprite spTrash        = LoadSprite(sp + "ItemTrash.png");
        Sprite spRecycle      = LoadSprite(sp + "ItemRecycle.png");
        Sprite spTrickyFruit  = LoadSprite(sp + "ItemTrickyFruit.png");
        Sprite spTrickyTrash  = LoadSprite(sp + "ItemTrickyTrash.png");
        Sprite spBonus        = LoadSprite(sp + "ItemBonus.png");
        Sprite spZoneFruit    = LoadSprite(sp + "ZoneFruit.png");
        Sprite spZoneTrash    = LoadSprite(sp + "ZoneTrash.png");
        Sprite spZoneRecycle  = LoadSprite(sp + "ZoneRecycle.png");
        Sprite spZoneBonus    = LoadSprite(sp + "ZoneBonus.png");

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

        // Zones (4 total, placed responsively)
        Sprite[] zoneSprites = { spZoneFruit, spZoneTrash, spZoneRecycle, spZoneBonus };
        string[] zoneLabels  = { "フルーツ", "ゴミ", "リサイクル", "ボーナス" };
        Color[] zoneLabelCols = {
            new Color(1f, 0.6f, 0.5f),
            new Color(0.7f, 0.7f, 0.8f),
            new Color(0.5f, 1f, 0.6f),
            new Color(1f, 0.9f, 0.3f)
        };

        var zoneObjs = new Transform[4];
        var zoneRenderers = new SpriteRenderer[4];
        float zoneY = -camSize + 1.8f;
        for (int i = 0; i < 4; i++)
        {
            var zoneGo = new GameObject($"Zone{i}");
            var zoneSr = zoneGo.AddComponent<SpriteRenderer>();
            zoneSr.sprite = zoneSprites[i];
            zoneSr.sortingOrder = 2;

            // Scale zone to fit its share of screen width (will be repositioned by mechanic)
            float zoneTargetW = 1.6f;
            if (zoneSprites[i] != null)
            {
                float sprW = zoneSprites[i].rect.width / zoneSprites[i].pixelsPerUnit;
                float s = zoneTargetW / sprW;
                zoneGo.transform.localScale = new Vector3(s, s, 1f);
            }

            // BoxCollider for drop detection
            var col = zoneGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(zoneTargetW, 0.8f);
            col.isTrigger = true;

            // Initial position (will be overridden by SetupStage)
            float initSpacing = camW * 2f / 4f;
            zoneGo.transform.position = new Vector3(-camW + initSpacing * (i + 0.5f), zoneY, 0f);
            zoneGo.SetActive(i < 2); // start with 2 zones visible

            zoneObjs[i] = zoneGo.transform;
            zoneRenderers[i] = zoneSr;
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<DropZoneGameManager>();

        // StageManager (child of GameManager)
        var stageObj = new GameObject("StageManager");
        stageObj.transform.SetParent(gmObj.transform);
        var stageMgr = stageObj.AddComponent<StageManager>();

        // DropZoneMechanic (child of GameManager)
        var mechObj = new GameObject("DropZoneMechanic");
        mechObj.transform.SetParent(gmObj.transform);
        var mechanic = mechObj.AddComponent<DropZoneMechanic>();
        var mechSO = new SerializedObject(mechanic);
        mechSO.FindProperty("_gameManager").objectReferenceValue = gm;
        mechSO.FindProperty("_spriteFruit").objectReferenceValue = spFruit;
        mechSO.FindProperty("_spriteTrash").objectReferenceValue = spTrash;
        mechSO.FindProperty("_spriteRecycle").objectReferenceValue = spRecycle;
        mechSO.FindProperty("_spriteTrickyFruit").objectReferenceValue = spTrickyFruit;
        mechSO.FindProperty("_spriteTrickyTrash").objectReferenceValue = spTrickyTrash;
        mechSO.FindProperty("_spriteBonus").objectReferenceValue = spBonus;

        // Wire zones array
        var zonesProp = mechSO.FindProperty("_zones");
        zonesProp.arraySize = 4;
        for (int i = 0; i < 4; i++) zonesProp.GetArrayElementAtIndex(i).objectReferenceValue = zoneObjs[i];
        var zoneRendProp = mechSO.FindProperty("_zoneRenderers");
        zoneRendProp.arraySize = 4;
        for (int i = 0; i < 4; i++) zoneRendProp.GetArrayElementAtIndex(i).objectReferenceValue = zoneRenderers[i];
        mechSO.ApplyModifiedProperties();

        // Canvas
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
            new Vector2(380, 50), new Vector2(-20, -20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        var missText = CT(canvasObj.transform, "MissText", "ミス: 0/3", 32, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(250, 50), new Vector2(0, -20));
        missText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        missText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.4f);

        var comboText = CT(canvasObj.transform, "ComboText", "×2 COMBO!", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 80), new Vector2(0, 200));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0f);
        comboText.SetActive(false);

        // Zone label hints at bottom
        // (shown as Canvas world-space labels below zones - simpler: just static UI labels)

        // --- Menu button (bottom center) ---
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(200, 55), new Vector2(0, 20), new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // ReShow button (bottom-right)
        var reShowBtn = CB(canvasObj.transform, "ReShowButton", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(80, 80), new Vector2(-20, 20), new Color(0.2f, 0.5f, 0.8f, 0.9f));

        // --- InstructionPanel ---
        var ip = CreateInstructionPanel(canvasObj.transform, jpFont);
        var ipSOWire = new SerializedObject(ip);
        ipSOWire.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSOWire.ApplyModifiedProperties();

        // --- StageClear Panel ---
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.1f, 0.15f, 0.25f, 0.95f), new Vector2(700, 350));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "Stage 1 クリア！", 48, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(600,80), new Vector2(0,100));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        var scBonus = CT(scPanel.transform, "StageClearBonus", "", 36, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(600,60), new Vector2(0,20));
        scBonus.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scBonus.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.6f);
        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300,60), new Vector2(0,-80), new Color(0.2f, 0.7f, 0.3f, 1f));
        var scMenuBtn = CB(scPanel.transform, "StageClearMenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(200,55), new Vector2(0,-145), new Color(0.35f, 0.35f, 0.45f, 1f));
        scPanel.SetActive(false);

        // --- FinalClear Panel ---
        var fcPanel = CreatePanel(canvasObj.transform, "FinalClearPanel", new Color(0.05f, 0.12f, 0.08f, 0.97f), new Vector2(750, 420));
        var fcScore = CT(fcPanel.transform, "FinalScoreText", "全ステージクリア！\nScore: 0", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(680,160), new Vector2(0,80));
        fcScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        fcScore.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);
        var fcRetryBtn = CB(fcPanel.transform, "FinalRetryButton", "もう一度", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250,60), new Vector2(-140,-80), new Color(0.2f, 0.7f, 0.3f, 1f));
        var fcMenuBtn = CB(fcPanel.transform, "FinalMenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250,60), new Vector2(140,-80), new Color(0.35f, 0.35f, 0.45f, 1f));
        fcPanel.SetActive(false);

        // --- GameOver Panel ---
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.15f, 0.05f, 0.05f, 0.97f), new Vector2(750, 420));
        var goScore = CT(goPanel.transform, "GameOverScoreText", "ゲームオーバー\nScore: 0", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(680,160), new Vector2(0,80));
        goScore.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.5f, 0.5f);
        var goRetryBtn = CB(goPanel.transform, "GameOverRetryButton", "もう一度", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250,60), new Vector2(-140,-80), new Color(0.7f, 0.3f, 0.2f, 1f));
        var goMenuBtn = CB(goPanel.transform, "GameOverMenuButton", "メニュー", jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(250,60), new Vector2(140,-80), new Color(0.35f, 0.35f, 0.45f, 1f));
        goPanel.SetActive(false);

        // --- DropZoneUI ---
        var uiObj = new GameObject("DropZoneUI");
        uiObj.transform.SetParent(gmObj.transform);
        var dzUI = uiObj.AddComponent<DropZoneUI>();

        var uiSO = new SerializedObject(dzUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_missText").objectReferenceValue = missText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearTitle").objectReferenceValue = scTitle.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearBonus").objectReferenceValue = scBonus.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = scNextBtn.GetComponent<Button>();
        uiSO.FindProperty("_stageClearMenuButton").objectReferenceValue = scMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_finalClearPanel").objectReferenceValue = fcPanel;
        uiSO.FindProperty("_finalScoreText").objectReferenceValue = fcScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = fcRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = fcMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScore.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton2").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = stageMgr;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_mechanic").objectReferenceValue = mechanic;
        gmSO.FindProperty("_ui").objectReferenceValue = dzUI;
        gmSO.ApplyModifiedProperties();

        // Button wiring
        AddButtonOnClick(menuBtn.GetComponent<Button>(), gm, "ReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(), ip, "ReShow");

        // Save scene
        string scenePath = "Assets/Scenes/034v2_DropZone.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup034v2] Scene created: " + scenePath);
    }

    static InstructionPanel CreateInstructionPanel(Transform canvasParent, TMP_FontAsset font)
    {
        var panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasParent, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.07f, 0.10f, 0.18f, 0.97f);

        var canvasComp = panelObj.AddComponent<Canvas>();
        canvasComp.overrideSorting = true;
        canvasComp.sortingOrder = 100;
        panelObj.AddComponent<GraphicRaycaster>();

        var ip = panelObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var titleObj = CT(panelObj.transform, "TitleText", "DropZone", 64, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,90), new Vector2(0,350));
        titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.8f, 0.3f);

        var descObj = CT(panelObj.transform, "DescriptionText", "落ちてくるアイテムを正しい箱に仕分けしよう", 36, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,60), new Vector2(0,250));
        descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        descObj.GetComponent<TextMeshProUGUI>().color = new Color(0.8f,0.9f,1f);
        descObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = CT(panelObj.transform, "ControlsText", "アイテムをドラッグして正しいゾーンにドロップ", 32, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,100), new Vector2(0,130));
        ctrlObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ctrlObj.GetComponent<TextMeshProUGUI>().color = new Color(0.7f,0.9f,0.7f);
        ctrlObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = CT(panelObj.transform, "GoalText", "ミス3回以内で全アイテムを仕分けしよう", 32, font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(900,80), new Vector2(0,20));
        goalObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goalObj.GetComponent<TextMeshProUGUI>().color = new Color(1f,0.7f,0.4f);
        goalObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtn = CB(panelObj.transform, "StartButton", "はじめる", font,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(300,70), new Vector2(0,-150), new Color(0.2f, 0.6f, 0.9f, 1f));

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
            else Debug.LogWarning($"[Setup034v2] Sprite not found: {path}");
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
