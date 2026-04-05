using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game024v2_BubblePop;

public static class Setup024v2_BubblePop
{
    [MenuItem("Assets/Setup/024v2 BubblePop")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup024v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game024v2_BubblePop/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.05f, 0.11f, 0.16f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
        }

        // Ensure sprites imported
        string[] spritePaths = {
            sp+"Background.png", sp+"BubbleRed.png", sp+"BubbleBlue.png",
            sp+"BubbleGreen.png", sp+"BubbleIron.png", sp+"BubbleSplit.png",
            sp+"BubbleGhost.png", sp+"Heart.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg    = LoadSprite(sp + "Background.png");

        // Background
        if (spBg != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = spBg;
            bgSr.sortingOrder = -10;
            float camH = 10f;
            float camAsp = Camera.main != null ? Camera.main.aspect : 9f / 16f;
            float camW = 5f * camAsp * 2f;
            float scaleX = camW / (spBg.rect.width / spBg.pixelsPerUnit);
            float scaleY = camH / (spBg.rect.height / spBg.pixelsPerUnit);
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<BubblePopGameManager>();

        // StageManager (child)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_totalStages").intValue = 5;
        smSO.ApplyModifiedProperties();

        // BubbleController (child)
        var ctrlObj = new GameObject("BubbleController");
        ctrlObj.transform.SetParent(gmObj.transform);
        var ctrl = ctrlObj.AddComponent<BubbleController>();

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
        stageText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.9f, 1f);

        // Lives (top-center)
        var livesText = CT(canvasObj.transform, "LivesText", "♥ ♥ ♥", 40, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(280, 55), new Vector2(0, -30));
        livesText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        livesText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.4f);

        // Score (top-right)
        var scoreText = CT(canvasObj.transform, "ScoreText", "0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(220, 55), new Vector2(-20, -30));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        // Combo (below stage)
        var comboText = CT(canvasObj.transform, "ComboText", "", 32, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(360, 50), new Vector2(20, -82));
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.6f, 0.1f);

        // Fever Slider (below lives)
        var feverSliderObj = new GameObject("FeverSlider");
        feverSliderObj.transform.SetParent(canvasObj.transform, false);
        var feverSliderRt = feverSliderObj.AddComponent<RectTransform>();
        feverSliderRt.anchorMin = new Vector2(0.5f, 1f);
        feverSliderRt.anchorMax = new Vector2(0.5f, 1f);
        feverSliderRt.pivot = new Vector2(0.5f, 1f);
        feverSliderRt.sizeDelta = new Vector2(300, 20);
        feverSliderRt.anchoredPosition = new Vector2(0, -82);
        var feverSlider = feverSliderObj.AddComponent<Slider>();
        feverSlider.value = 0f;
        // Background
        var fsBackground = new GameObject("Background"); fsBackground.transform.SetParent(feverSliderObj.transform, false);
        var fsBgRt = fsBackground.AddComponent<RectTransform>();
        fsBgRt.anchorMin = Vector2.zero; fsBgRt.anchorMax = Vector2.one;
        fsBgRt.offsetMin = Vector2.zero; fsBgRt.offsetMax = Vector2.zero;
        fsBackground.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        // Fill area
        var fsFillArea = new GameObject("Fill Area"); fsFillArea.transform.SetParent(feverSliderObj.transform, false);
        var fsFillAreaRt = fsFillArea.AddComponent<RectTransform>();
        fsFillAreaRt.anchorMin = Vector2.zero; fsFillAreaRt.anchorMax = Vector2.one;
        fsFillAreaRt.offsetMin = Vector2.zero; fsFillAreaRt.offsetMax = Vector2.zero;
        var fsFill = new GameObject("Fill"); fsFill.transform.SetParent(fsFillArea.transform, false);
        var fsFillRt = fsFill.AddComponent<RectTransform>();
        fsFillRt.anchorMin = Vector2.zero; fsFillRt.anchorMax = Vector2.one;
        fsFillRt.offsetMin = Vector2.zero; fsFillRt.offsetMax = Vector2.zero;
        var fillImg = fsFill.AddComponent<Image>();
        fillImg.color = new Color(1f, 0.8f, 0.1f, 0.9f);
        feverSlider.fillRect = fsFillRt;
        feverSlider.targetGraphic = fillImg;

        // Fever Label
        var feverLabelObj = CT(canvasObj.transform, "FeverLabel", "FEVER!!", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 80), new Vector2(0, 100));
        feverLabelObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        feverLabelObj.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.1f);
        feverLabelObj.SetActive(false);

        // Bonus text (center)
        var bonusText = CT(canvasObj.transform, "BonusText", "+50", 44, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 100), new Vector2(0, 200));
        bonusText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        bonusText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);
        bonusText.SetActive(false);

        // Bottom buttons
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(300, 65), new Vector2(-185, 55), new Color(0.15f, 0.25f, 0.4f));

        var reShowBtn = CB(canvasObj.transform, "ReShowInstructionBtn", "?", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(75, 75), new Vector2(-10, 55), new Color(0.2f, 0.35f, 0.55f));

        // ---- Stage Clear Panel ----
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.1f, 0.2f, 0.95f), new Vector2(720, 380));
        var scTitle = CT(scPanel.transform, "StageClearTitle", "ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(680, 70), new Vector2(0, -30));
        scTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.85f, 1f);
        var scScoreText = CT(scPanel.transform, "StageClearScoreText", "Score: 0", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 55), new Vector2(0, 30));
        scScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(320, 70), new Vector2(0, 30), new Color(0.1f, 0.5f, 0.8f));
        scPanel.SetActive(false);

        // ---- Clear Panel ----
        var gcPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0.05f, 0.15f, 0.05f, 0.95f), new Vector2(720, 500));
        var gcTitle = CT(gcPanel.transform, "ClearTitle", "ゲームクリア！", 56, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(680, 80), new Vector2(0, -35));
        gcTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        gcTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);
        var gcTotal = CT(gcPanel.transform, "ClearScoreText", "Final Score: 0", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 60), new Vector2(0, 40));
        gcTotal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var gcRetryBtn = CB(gcPanel.transform, "ClearRetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 70), new Vector2(-145, 30), new Color(0.2f, 0.5f, 0.2f));
        var gcMenuBtn = CB(gcPanel.transform, "ClearMenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 70), new Vector2(145, 30), new Color(0.15f, 0.25f, 0.4f));
        gcPanel.SetActive(false);

        // ---- GameOver Panel ----
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0.05f, 0.95f), new Vector2(720, 500));
        var goTitle = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 56, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(680, 80), new Vector2(0, -35));
        goTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.3f, 0.2f);
        var goScoreText = CT(goPanel.transform, "GameOverScoreText", "Score: 0", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 90), new Vector2(0, 40));
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "もう一度", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 70), new Vector2(-145, 30), new Color(0.5f, 0.15f, 0.1f));
        var goMenuBtn = CB(goPanel.transform, "MenuButton", "メニューへ", jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(260, 70), new Vector2(145, 30), new Color(0.15f, 0.25f, 0.4f));
        goPanel.SetActive(false);

        // ---- InstructionPanel ----
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipC = ipCanvas.AddComponent<Canvas>();
        ipC.renderMode = RenderMode.ScreenSpaceOverlay;
        ipC.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipScaler.matchWidthOrHeight = 0.5f;
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipPanel = new GameObject("InstructionPanel");
        ipPanel.transform.SetParent(ipCanvas.transform, false);
        var ipPanelImg = ipPanel.AddComponent<Image>();
        ipPanelImg.color = new Color(0.05f, 0.1f, 0.2f, 0.97f);
        var ipRt = ipPanel.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero; ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = Vector2.zero; ipRt.offsetMax = Vector2.zero;
        var ip = ipPanel.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);

        var ipTitle = CT(ipPanel.transform, "IPTitle", "BubblePop", 64, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 90), new Vector2(0, 220));
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.85f, 1f);

        var ipDesc = CT(ipPanel.transform, "IPDescription", "", 34, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 70), new Vector2(0, 110));
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.95f, 1f);

        var ipCtrl = CT(ipPanel.transform, "IPControls", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 110), new Vector2(0, 0));
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.75f, 0.85f, 1f);
        ipCtrl.GetComponent<TextMeshProUGUI>().textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 28, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 65), new Vector2(0, -100));
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "IPStartButton", "はじめる", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 80), new Vector2(0, -230), new Color(0.1f, 0.4f, 0.7f));

        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipCtrl.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_helpButton").objectReferenceValue = reShowBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // ---- BubblePopUI component ----
        var uiObj = new GameObject("BubblePopUI");
        uiObj.transform.SetParent(gmObj.transform);
        var bpUI = uiObj.AddComponent<BubblePopUI>();
        var uiSO = new SerializedObject(bpUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_livesText").objectReferenceValue = livesText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_bonusText").objectReferenceValue = bonusText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_feverSlider").objectReferenceValue = feverSlider;
        uiSO.FindProperty("_feverLabel").objectReferenceValue = feverLabelObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = gcPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = gcTotal.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire Controller
        var ctrlSO = new SerializedObject(ctrl);
        ctrlSO.FindProperty("_gameManager").objectReferenceValue = gm;
        ctrlSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_controller").objectReferenceValue = ctrl;
        gmSO.FindProperty("_ui").objectReferenceValue = bpUI;
        gmSO.ApplyModifiedProperties();

        // Button Events
        AddButtonOnClick(menuBtn.GetComponent<Button>(),    gm, "OnReturnToMenu");
        AddButtonOnClick(reShowBtn.GetComponent<Button>(),  gm, "ShowInstructions");
        AddButtonOnClick(nextBtn.GetComponent<Button>(),    gm, "OnNextStage");
        AddButtonOnClick(goRetryBtn.GetComponent<Button>(), gm, "OnRetry");
        AddButtonOnClick(goMenuBtn.GetComponent<Button>(),  gm, "OnReturnToMenu");
        AddButtonOnClick(gcRetryBtn.GetComponent<Button>(), gm, "OnRetry");
        AddButtonOnClick(gcMenuBtn.GetComponent<Button>(),  gm, "OnReturnToMenu");

        // Save Scene
        string scenePath = "Assets/Scenes/024v2_BubblePop.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Setup024v2] Scene created: " + scenePath);
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
            else Debug.LogWarning($"[Setup024v2] Sprite not found: {path}");
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
