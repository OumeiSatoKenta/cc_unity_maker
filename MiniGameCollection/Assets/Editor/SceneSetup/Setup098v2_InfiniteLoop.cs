using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game098v2_InfiniteLoop;

public static class Setup098v2_InfiniteLoop
{
    [MenuItem("Assets/Setup/098v2 InfiniteLoop")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup098v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game098v2_InfiniteLoop/";

        // === Camera ===
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.08f, 0.04f, 0.16f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // === Background ===
        Sprite bgSprite = LoadSprite(sp + "background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            float bgCamS = 6f;
            float bgCamW = bgCamS * (camera != null ? camera.aspect : (16f / 9f));
            bgObj.transform.localScale = new Vector3(bgCamW * 2f / bgSprite.bounds.size.x, bgCamS * 2f / bgSprite.bounds.size.y, 1f);
        }

        // === Room layout (world space) ===
        // Responsive: camSize=6, topMargin=1.5, bottomMargin=3.0 => availableHeight=7.5
        // Objects arranged in two rows in the middle area
        float camSize = 6f;
        float camW = camSize * (camera != null ? camera.aspect : (16f / 9f));
        float topMargin = 1.5f;
        float bottomMargin = 3.0f;
        float availableHeight = (camSize * 2f) - topMargin - bottomMargin;
        float gameAreaTop = camSize - topMargin;
        float gameAreaBottom = -camSize + bottomMargin;
        float gameCenterY = (gameAreaTop + gameAreaBottom) * 0.5f;

        // Room background sprite (slightly smaller than game area)
        Sprite roomBg = LoadSprite(sp + "room_bg.png");
        if (roomBg != null)
        {
            var roomObj = new GameObject("RoomBackground");
            var roomSr = roomObj.AddComponent<SpriteRenderer>();
            roomSr.sprite = roomBg;
            roomSr.sortingOrder = -5;
            float roomW = camW * 1.8f;
            float roomH = availableHeight * 0.95f;
            roomObj.transform.localScale = new Vector3(roomW / roomBg.bounds.size.x, roomH / roomBg.bounds.size.y, 1f);
            roomObj.transform.position = new Vector3(0, gameCenterY, 0);
        }

        // === Room Objects (5 objects with CircleCollider2D) ===
        string[] objSpriteNames = { "obj_door", "obj_window", "obj_book", "obj_clock", "obj_picture" };
        string[] objectIds = { "door", "window", "book", "clock", "picture" };
        var roomObjects = new SpriteRenderer[5];

        // Row 1 (top): 3 objects
        float[] row1X = { -camW * 0.5f, 0f, camW * 0.5f };
        float row1Y = gameCenterY + availableHeight * 0.22f;
        // Row 2 (bottom): 2 objects
        float[] row2X = { -camW * 0.28f, camW * 0.28f };
        float row2Y = gameCenterY - availableHeight * 0.22f;

        float objScale = Mathf.Min(availableHeight / 6f, camW * 2f / 9f, 1.0f);

        for (int i = 0; i < 5; i++)
        {
            float posX = i < 3 ? row1X[i] : row2X[i - 3];
            float posY = i < 3 ? row1Y : row2Y;

            Sprite objSpr = LoadSprite(sp + objSpriteNames[i] + ".png");
            var objGo = new GameObject("RoomObject_" + objectIds[i]);
            objGo.transform.position = new Vector3(posX, posY, 0);
            objGo.transform.localScale = Vector3.one * objScale;

            var sr = objGo.AddComponent<SpriteRenderer>();
            sr.sprite = objSpr;
            sr.sortingOrder = 5;
            sr.color = new Color(0.7f, 0.7f, 0.9f, 1f);

            var col = objGo.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            roomObjects[i] = sr;
        }

        // === Flash Image (world space overlay via Canvas) ===
        // We'll create it as part of the Canvas

        // === GameManager hierarchy ===
        var gmObj = new GameObject("InfiniteLoopGameManager");
        var gm = gmObj.AddComponent<InfiniteLoopGameManager>();

        // StageManager (child of GM)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1" },
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 2, complexityFactor = 0.0f, stageName = "Stage 2" },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 2, complexityFactor = 0.0f, stageName = "Stage 3" },
            new StageManager.StageConfig { speedMultiplier = 1.4f, countMultiplier = 3, complexityFactor = 0.5f, stageName = "Stage 4" },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 1.0f, stageName = "Stage 5" },
        };
        sm.SetConfigs(stageConfigs);

        // LoopManager (child of GM)
        var lmObj = new GameObject("LoopManager");
        lmObj.transform.SetParent(gmObj.transform);
        var lm = lmObj.AddComponent<LoopManager>();

        // === Canvas ===
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // === Flash Image (full-screen overlay, always inactive by default) ===
        var flashGo = new GameObject("FlashImage", typeof(RectTransform));
        flashGo.transform.SetParent(canvasObj.transform, false);
        var flashRT = flashGo.GetComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = flashRT.offsetMax = Vector2.zero;
        var flashImg = flashGo.AddComponent<Image>();
        flashImg.color = new Color(1f, 1f, 1f, 0f);
        flashImg.raycastTarget = false;

        // === HUD (top area) ===
        var stageTextGo = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 50), new Vector2(15, -15));
        stageTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 1f, 0.3f);

        var scoreTextGo = CT(canvasObj.transform, "ScoreText", "Score: 0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(300, 50), new Vector2(-15, -15));
        scoreTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 1f, 0.3f);

        var loopCountTextGo = CT(canvasObj.transform, "LoopCountText", "残り 10 ループ", 44, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 60), new Vector2(0, -15));
        loopCountTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        loopCountTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        var reverseTextGo = CT(canvasObj.transform, "ReverseText", "【注意】逆行ループ！", 40, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(500, 60), new Vector2(0, -75));
        reverseTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        reverseTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.1f);
        reverseTextGo.SetActive(false);

        var comboTextGo = CT(canvasObj.transform, "ComboText", "コンボ x2！", 50, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 80), new Vector2(0, 100));
        comboTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);
        comboTextGo.SetActive(false);

        // === Bottom Buttons (3-layer: bottom=menu, lower=actions, top not used) ===
        // Back To Menu button (lowest, Y=15)
        var backBtn = CB(canvasObj.transform, "BackToMenuButton", "メニューへ", 30, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(200, 55), new Vector2(15, 15), new Color(0.15f, 0.1f, 0.25f));

        // Memo button (right lower)
        var memoBtn = CB(canvasObj.transform, "MemoButton", "メモ", 36, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(160, 65), new Vector2(-15, 80), new Color(0.15f, 0.2f, 0.35f));

        // Next Loop button (left lower)
        var nextLoopBtn = CB(canvasObj.transform, "NextLoopButton", "次のループ▶", 32, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(200, 65), new Vector2(15, 80), new Color(0.15f, 0.15f, 0.35f));

        // Escape button (center, prominent)
        Sprite escapeSpr = LoadSprite(sp + "btn_escape.png");
        var escapeBtnGo = CB(canvasObj.transform, "EscapeButton", "脱出を試みる！", 38, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(360, 80), new Vector2(0, 165), new Color(0.2f, 0.5f, 0.15f));

        // === Memo Panel (full-screen overlay) ===
        var memoPanel = new GameObject("MemoPanel", typeof(RectTransform));
        memoPanel.transform.SetParent(canvasObj.transform, false);
        var memoPanelRT = memoPanel.GetComponent<RectTransform>();
        memoPanelRT.anchorMin = new Vector2(0.05f, 0.2f);
        memoPanelRT.anchorMax = new Vector2(0.95f, 0.85f);
        memoPanelRT.offsetMin = memoPanelRT.offsetMax = Vector2.zero;
        var memoPanelImg = memoPanel.AddComponent<Image>();
        memoPanelImg.color = new Color(0.06f, 0.04f, 0.14f, 0.96f);

        var memoTitleGo = CT(memoPanel.transform, "MemoTitle", "【発見メモ】", 44, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 70), new Vector2(0, -15));
        memoTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        memoTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.9f);

        var memoContentGo = CT(memoPanel.transform, "MemoContent", "（まだ何も発見していない）", 34, jpFont,
            new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.85f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        var memoTMP = memoContentGo.GetComponent<TextMeshProUGUI>();
        memoTMP.alignment = TextAlignmentOptions.TopLeft;
        memoTMP.color = Color.white;
        memoTMP.enableWordWrapping = true;
        memoContentGo.GetComponent<RectTransform>().anchorMin = new Vector2(0.05f, 0.15f);
        memoContentGo.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.85f);
        memoContentGo.GetComponent<RectTransform>().offsetMin = memoContentGo.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        var memoCloseBtn = CB(memoPanel.transform, "MemoCloseButton", "閉じる", 38, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(250, 65), new Vector2(0, 15), new Color(0.3f, 0.1f, 0.25f));
        memoPanel.SetActive(false);

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scPanelRT = scPanel.GetComponent<RectTransform>();
        scPanelRT.anchorMin = new Vector2(0.5f, 0.5f); scPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        scPanelRT.pivot = new Vector2(0.5f, 0.5f); scPanelRT.sizeDelta = new Vector2(700, 440);
        var scPanelImg = scPanel.AddComponent<Image>();
        scPanelImg.color = new Color(0.04f, 0.14f, 0.08f, 0.97f);

        var scTitleGo = CT(scPanel.transform, "StageClearText", "ステージクリア！", 56, jpFont,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(650, 90), Vector2.zero);
        scTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.5f);

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 44, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.1f, 0.3f, 0.15f));
        scPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goPanelRT = goPanel.GetComponent<RectTransform>();
        goPanelRT.anchorMin = new Vector2(0.5f, 0.5f); goPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        goPanelRT.pivot = new Vector2(0.5f, 0.5f); goPanelRT.sizeDelta = new Vector2(700, 440);
        var goPanelImg = goPanel.AddComponent<Image>();
        goPanelImg.color = new Color(0.14f, 0.04f, 0.04f, 0.97f);

        var goTitleGo = CT(goPanel.transform, "GameOverTitle", "ループ制限超過...", 50, jpFont,
            new Vector2(0.5f, 0.76f), new Vector2(0.5f, 0.76f), new Vector2(0.5f, 0.5f),
            new Vector2(650, 80), Vector2.zero);
        goTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.3f);

        var goScoreGo = CT(goPanel.transform, "GameOverScore", "Score: 0", 42, jpFont,
            new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        goScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var goRetryBtn = CB(goPanel.transform, "RetryButton", "もう一度", 42, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 72), Vector2.zero, new Color(0.35f, 0.1f, 0.1f));
        goPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acPanelRT = acPanel.GetComponent<RectTransform>();
        acPanelRT.anchorMin = new Vector2(0.5f, 0.5f); acPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        acPanelRT.pivot = new Vector2(0.5f, 0.5f); acPanelRT.sizeDelta = new Vector2(750, 480);
        var acPanelImg = acPanel.AddComponent<Image>();
        acPanelImg.color = new Color(0.04f, 0.04f, 0.2f, 0.97f);

        var acTitleGo = CT(acPanel.transform, "AllClearTitle", "全ループ脱出！", 60, jpFont,
            new Vector2(0.5f, 0.76f), new Vector2(0.5f, 0.76f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 90), Vector2.zero);
        acTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acTitleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.8f, 1f);

        var acScoreGo = CT(acPanel.transform, "AllClearScore", "最終スコア: 0", 48, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(650, 70), Vector2.zero);
        acScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScoreGo.GetComponent<TextMeshProUGUI>().color = Color.white;
        acPanel.SetActive(false);

        // === Escape Failed Panel ===
        var efPanel = new GameObject("EscapeFailedPanel", typeof(RectTransform));
        efPanel.transform.SetParent(canvasObj.transform, false);
        var efRT = efPanel.GetComponent<RectTransform>();
        efRT.anchorMin = new Vector2(0.5f, 0.5f); efRT.anchorMax = new Vector2(0.5f, 0.5f);
        efRT.pivot = new Vector2(0.5f, 0.5f); efRT.sizeDelta = new Vector2(600, 120);
        efRT.anchoredPosition = new Vector2(0, 200);
        var efImg = efPanel.AddComponent<Image>();
        efImg.color = new Color(0.5f, 0.05f, 0.05f, 0.9f);
        var efTextGo = CT(efPanel.transform, "EscapeFailedText", "だまされた！ループ-2消費", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(560, 80), Vector2.zero);
        efTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        efTextGo.GetComponent<TextMeshProUGUI>().color = Color.white;
        efPanel.SetActive(false);

        // === InstructionPanel (separate canvas, highest sort order) ===
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipBg = new GameObject("InstructionPanel", typeof(RectTransform));
        ipBg.transform.SetParent(ipCanvas.transform, false);
        var ipBgRT = ipBg.GetComponent<RectTransform>();
        ipBgRT.anchorMin = Vector2.zero; ipBgRT.anchorMax = Vector2.one;
        ipBgRT.offsetMin = ipBgRT.offsetMax = Vector2.zero;
        var ipBgImg = ipBg.AddComponent<Image>();
        ipBgImg.color = new Color(0.06f, 0.04f, 0.14f, 0.97f);
        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "InfiniteLoop", 68, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.76f, 0.5f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 38, jpFont,
            new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 1f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 32, jpFont,
            new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 150), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.7f, 1f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 32, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.8f, 1f);

        var startBtnGo = CB(ipBg.transform, "StartButton", "はじめる", 50, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.2f, 0.1f, 0.35f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 36, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(80, 80), new Vector2(-15, 260), new Color(0.2f, 0.15f, 0.35f));

        // === Wire InstructionPanel ===
        SetField(ip, "_panelRoot",       ipBg);
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtnGo.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());

        // === InfiniteLoopUI ===
        var uiObj = new GameObject("InfiniteLoopUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<InfiniteLoopUI>();

        SetField(ui, "_gameManager",       gm);
        SetField(ui, "_loopManager",       lm);
        SetField(ui, "_stageText",         stageTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",         scoreTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_loopCountText",     loopCountTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",         comboTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_reverseText",       reverseTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_escapeButton",      escapeBtnGo.GetComponent<Button>());
        SetField(ui, "_nextLoopButton",    nextLoopBtn.GetComponent<Button>());
        SetField(ui, "_memoButton",        memoBtn.GetComponent<Button>());
        SetField(ui, "_backToMenuButton",  backBtn.GetComponent<Button>());
        SetField(ui, "_memoPanel",         memoPanel);
        SetField(ui, "_memoContent",       memoContentGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_memoCloseButton",   memoCloseBtn.GetComponent<Button>());
        SetField(ui, "_stageClearPanel",   scPanel);
        SetField(ui, "_stageClearText",    scTitleGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton",   scNextBtn.GetComponent<Button>());
        SetField(ui, "_gameOverPanel",     goPanel);
        SetField(ui, "_gameOverText",      goTitleGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_restartButton",     goRetryBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",     acPanel);
        SetField(ui, "_allClearText",      acScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_escapeFailedPanel", efPanel);
        SetField(ui, "_escapeFailedText",  efTextGo.GetComponent<TextMeshProUGUI>());

        // === Wire LoopManager ===
        SetField(lm, "_gameManager",  gm);
        SetField(lm, "_ui",           ui);
        SetField(lm, "_roomObjects",  roomObjects);
        SetField(lm, "_flashImage",   flashImg);

        // === Wire button events ===
        UnityEngine.Events.UnityAction nextStageAct = gm.NextStage;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(scNextBtn.GetComponent<Button>().onClick, nextStageAct);
        UnityEngine.Events.UnityAction retryAct = gm.RestartGame;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, retryAct);

        // === Wire GameManager ===
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_loopManager",      lm);
        SetField(gm, "_ui",               ui);

        // === EventSystem ===
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // === Save scene ===
        string scenePath = "Assets/Scenes/098v2_InfiniteLoop.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup098v2] InfiniteLoop シーン作成完了: " + scenePath);
    }

    static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null) { ti.textureType = TextureImporterType.Sprite; ti.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void SetField(object obj, string fieldName, object value)
    {
        if (obj == null) return;
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
        else Debug.LogWarning($"[Setup098v2] Field not found: {fieldName} on {obj.GetType().Name}");
    }

    static GameObject CT(Transform parent, string name, string text, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        return go;
    }

    static GameObject CB(Transform parent, string name, string label, int fontSize, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = color;
        go.AddComponent<Button>().targetGraphic = img;
        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var tRT = textGo.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize;
        if (font != null) tmp.font = font;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
