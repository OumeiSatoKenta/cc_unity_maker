using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using Game053v2_SlideBlitz;

public static class Setup053v2_SlideBlitz
{
    [MenuItem("Assets/Setup/053v2 SlideBlitz")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup053v2] Play モード中は実行できません。"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game053v2_SlideBlitz/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.17f, 0.37f, 0.19f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.gameObject.name = "Main Camera";
            camera.gameObject.tag = "MainCamera";
        }

        // Ensure sprite imports
        string[] spritePaths = {
            sp+"Background.png", sp+"Tile_Normal.png",
            sp+"Tile_Frozen.png", sp+"Tile_Blank.png"
        };
        foreach (var path in spritePaths) EnsureSpriteImport(path);
        AssetDatabase.Refresh();

        Sprite spBg      = LoadSprite(sp + "Background.png");
        Sprite spNormal  = LoadSprite(sp + "Tile_Normal.png");
        Sprite spFrozen  = LoadSprite(sp + "Tile_Frozen.png");
        Sprite spBlank   = LoadSprite(sp + "Tile_Blank.png");

        float camSize = camera != null ? camera.orthographicSize : 5f;
        float camWidth = camera != null ? camSize * camera.aspect : 2.8f;

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

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<SlideBlitzGameManager>();

        // StageManager (child of GameManager)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform, false);
        var sm = smObj.AddComponent<StageManager>();

        // SlideManager (child of GameManager)
        var slideObj = new GameObject("SlideManager");
        slideObj.transform.SetParent(gmObj.transform, false);
        var slideManager = slideObj.AddComponent<SlideManager>();
        var slideSO = new SerializedObject(slideManager);
        if (spNormal != null) slideSO.FindProperty("_spriteNormal").objectReferenceValue = spNormal;
        if (spFrozen != null) slideSO.FindProperty("_spriteFrozen").objectReferenceValue = spFrozen;
        if (spBlank != null)  slideSO.FindProperty("_spriteBlank").objectReferenceValue = spBlank;
        slideSO.FindProperty("_font").objectReferenceValue = jpFont;
        slideSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<GraphicRaycaster>();
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem
        var esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // HUD - top
        var stageObj = CreateText(canvasObj.transform, "StageText", "Stage 1 / 5", jpFont, 36,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(300f, 50f), new Vector2(20f, -20f), Color.white);

        var timerObj = CreateText(canvasObj.transform, "TimerText", "60", jpFont, 36,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(220f, 50f), new Vector2(-20f, -20f), Color.white);

        var movesObj = CreateText(canvasObj.transform, "MovesText", "手数: 0", jpFont, 30,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(200f, 45f), new Vector2(20f, -75f), new Color(0.9f, 0.9f, 0.5f));

        var comboObj = CreateText(canvasObj.transform, "ComboText", "", jpFont, 40,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(400f, 60f), new Vector2(0f, 250f), new Color(1f, 0.85f, 0.2f));
        comboObj.SetActive(false);

        // Bottom buttons
        var menuBtnObj = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(220f, 55f), new Vector2(20f, 15f), new Color(0.3f, 0.3f, 0.35f));

        var resetBtnObj = CreateButton(canvasObj.transform, "ResetButton", "リセット", jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(160f, 55f), new Vector2(-20f, 15f), new Color(0.6f, 0.4f, 0.1f));

        // Stage Clear Panel
        var scPanelObj = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.1f, 0.08f, 0.92f), new Vector2(700f, 500f));
        scPanelObj.SetActive(false);
        var scTitleObj = CreateText(scPanelObj.transform, "ClearTitle", "ステージクリア！", jpFont, 50,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600f, 70f), new Vector2(0f, 130f), new Color(1f, 0.9f, 0.2f));
        var scScoreObj = CreateText(scPanelObj.transform, "StageScoreText", "+0", jpFont, 44,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 60f), new Vector2(0f, 50f), Color.white);
        var scTotalObj = CreateText(scPanelObj.transform, "TotalScoreText", "合計: 0", jpFont, 34,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 50f), new Vector2(0f, -30f), new Color(0.8f, 0.9f, 0.8f));
        var scNextBtnObj = CreateButton(scPanelObj.transform, "NextStageButton", "次のステージへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(280f, 65f), new Vector2(0f, -140f), new Color(0.15f, 0.55f, 0.2f));

        // Game Over Panel
        var goPanelObj = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.15f, 0.03f, 0.03f, 0.92f), new Vector2(700f, 450f));
        goPanelObj.SetActive(false);
        CreateText(goPanelObj.transform, "GOTitle", "タイムアップ！", jpFont, 50,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600f, 70f), new Vector2(0f, 120f), new Color(1f, 0.3f, 0.3f));
        var goScoreObj = CreateText(goPanelObj.transform, "GameOverScoreText", "スコア: 0", jpFont, 38,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 60f), new Vector2(0f, 20f), Color.white);
        var goRetryBtnObj = CreateButton(goPanelObj.transform, "RetryButton", "リトライ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200f, 65f), new Vector2(-110f, -100f), new Color(0.2f, 0.4f, 0.6f));
        var goMenuBtnObj = CreateButton(goPanelObj.transform, "MenuButtonGO", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200f, 65f), new Vector2(110f, -100f), new Color(0.3f, 0.3f, 0.35f));

        // All Clear Panel
        var acPanelObj = CreatePanel(canvasObj.transform, "AllClearPanel", new Color(0.05f, 0.1f, 0.08f, 0.95f), new Vector2(700f, 500f));
        acPanelObj.SetActive(false);
        CreateText(acPanelObj.transform, "ACTitle", "全ステージクリア！", jpFont, 50,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(650f, 70f), new Vector2(0f, 130f), new Color(1f, 0.9f, 0.2f));
        var acScoreObj = CreateText(acPanelObj.transform, "AllClearScoreText", "最終スコア: 0", jpFont, 38,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600f, 60f), new Vector2(0f, 30f), Color.white);
        var acRetryBtnObj = CreateButton(acPanelObj.transform, "RetryButtonAC", "もう一度", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200f, 65f), new Vector2(-110f, -110f), new Color(0.2f, 0.4f, 0.6f));
        var acMenuBtnObj = CreateButton(acPanelObj.transform, "MenuButtonAC", "メニューへ", jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200f, 65f), new Vector2(110f, -110f), new Color(0.3f, 0.3f, 0.35f));

        // InstructionPanel
        var ip = BuildInstructionPanel(jpFont);

        // SlideBlitzUI
        var uiObj = new GameObject("SlideBlitzUI");
        uiObj.transform.SetParent(gmObj.transform, false);
        var ui = uiObj.AddComponent<SlideBlitzUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerText").objectReferenceValue = timerObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_movesText").objectReferenceValue = movesObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanelObj;
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_totalScoreInClearText").objectReferenceValue = scTotalObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanelObj;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_allClearPanel").objectReferenceValue = acPanelObj;
        uiSO.FindProperty("_allClearScoreText").objectReferenceValue = acScoreObj.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // Wire up GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_slideManager").objectReferenceValue = slideManager;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Button events
        var nextBtn = scNextBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.onClick, gm.NextStage);

        var retryBtn = goRetryBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.onClick, gm.RetryStage);

        var resetBtn = resetBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.onClick, gm.RetryStage);

        // Menu button SceneLoader
        AddMenuButtonListener(menuBtnObj);
        AddMenuButtonListener(goMenuBtnObj);
        AddMenuButtonListener(acMenuBtnObj);

        // AC retry
        var acRetryBtn = acRetryBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(acRetryBtn.onClick, gm.RetryStage);

        // Save scene
        string scenePath = "Assets/Scenes/053v2_SlideBlitz.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);

        Debug.Log("[Setup053v2] SlideBlitz シーン作成完了: " + scenePath);
    }

    static void AddMenuButtonListener(GameObject btnObj)
    {
        var btn = btnObj.GetComponent<Button>();
        if (btn == null) return;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick,
            (UnityEngine.Events.UnityAction)SceneLoader.BackToMenu);
    }

    static InstructionPanel BuildInstructionPanel(TMP_FontAsset font)
    {
        var ipCanvasObj = new GameObject("InstructionPanelCanvas");
        var ipCanvas = ipCanvasObj.AddComponent<Canvas>();
        ipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvas.sortingOrder = 100;
        ipCanvasObj.AddComponent<GraphicRaycaster>();
        var ipCanvasScaler = ipCanvasObj.AddComponent<CanvasScaler>();
        ipCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipCanvasScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvasScaler.matchWidthOrHeight = 0.5f;

        var ip = ipCanvasObj.AddComponent<InstructionPanel>();

        var panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(ipCanvasObj.transform, false);
        var panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.1f, 0.08f, 0.92f);

        var titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        var titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.sizeDelta = new Vector2(800f, 90f); titleRt.anchoredPosition = new Vector2(0f, 250f);
        var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.font = font; titleTmp.fontSize = 60; titleTmp.alignment = TextAlignmentOptions.Center; titleTmp.color = Color.white;

        var descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(panelObj.transform, false);
        var descRt = descObj.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.5f, 0.5f); descRt.anchorMax = new Vector2(0.5f, 0.5f);
        descRt.sizeDelta = new Vector2(800f, 80f); descRt.anchoredPosition = new Vector2(0f, 140f);
        var descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.font = font; descTmp.fontSize = 34; descTmp.alignment = TextAlignmentOptions.Center;
        descTmp.color = Color.white; descTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var ctrlObj = new GameObject("ControlsText");
        ctrlObj.transform.SetParent(panelObj.transform, false);
        var ctrlRt = ctrlObj.AddComponent<RectTransform>();
        ctrlRt.anchorMin = new Vector2(0.5f, 0.5f); ctrlRt.anchorMax = new Vector2(0.5f, 0.5f);
        ctrlRt.sizeDelta = new Vector2(800f, 120f); ctrlRt.anchoredPosition = new Vector2(0f, 0f);
        var ctrlTmp = ctrlObj.AddComponent<TextMeshProUGUI>();
        ctrlTmp.font = font; ctrlTmp.fontSize = 28; ctrlTmp.alignment = TextAlignmentOptions.Center;
        ctrlTmp.color = new Color(0.9f, 0.9f, 0.7f); ctrlTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var goalObj = new GameObject("GoalText");
        goalObj.transform.SetParent(panelObj.transform, false);
        var goalRt = goalObj.AddComponent<RectTransform>();
        goalRt.anchorMin = new Vector2(0.5f, 0.5f); goalRt.anchorMax = new Vector2(0.5f, 0.5f);
        goalRt.sizeDelta = new Vector2(800f, 80f); goalRt.anchoredPosition = new Vector2(0f, -100f);
        var goalTmp = goalObj.AddComponent<TextMeshProUGUI>();
        goalTmp.font = font; goalTmp.fontSize = 30; goalTmp.alignment = TextAlignmentOptions.Center;
        goalTmp.color = new Color(1f, 0.85f, 0.3f); goalTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var startBtnObj = new GameObject("StartButton");
        startBtnObj.transform.SetParent(panelObj.transform, false);
        var startBtnRt = startBtnObj.AddComponent<RectTransform>();
        startBtnRt.anchorMin = new Vector2(0.5f, 0.5f); startBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
        startBtnRt.sizeDelta = new Vector2(260f, 70f); startBtnRt.anchoredPosition = new Vector2(0f, -220f);
        startBtnObj.AddComponent<Image>().color = new Color(0.15f, 0.55f, 0.2f);
        var startBtn = startBtnObj.AddComponent<Button>();
        var startLabelObj = new GameObject("Label");
        startLabelObj.transform.SetParent(startBtnObj.transform, false);
        var slRt = startLabelObj.AddComponent<RectTransform>();
        slRt.anchorMin = Vector2.zero; slRt.anchorMax = Vector2.one;
        slRt.offsetMin = Vector2.zero; slRt.offsetMax = Vector2.zero;
        var slTmp = startLabelObj.AddComponent<TextMeshProUGUI>();
        slTmp.font = font; slTmp.text = "はじめる";
        slTmp.fontSize = 38; slTmp.alignment = TextAlignmentOptions.Center; slTmp.color = Color.white;

        var qBtnObj = new GameObject("QuestionButton");
        qBtnObj.transform.SetParent(ipCanvasObj.transform, false);
        var qBtnRt = qBtnObj.AddComponent<RectTransform>();
        qBtnRt.anchorMin = new Vector2(1f, 0f); qBtnRt.anchorMax = new Vector2(1f, 0f);
        qBtnRt.pivot = new Vector2(1f, 0f);
        qBtnRt.sizeDelta = new Vector2(70f, 70f); qBtnRt.anchoredPosition = new Vector2(-10f, 10f);
        qBtnObj.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 0.9f);
        var qBtn = qBtnObj.AddComponent<Button>();
        var qLabelObj = new GameObject("Label");
        qLabelObj.transform.SetParent(qBtnObj.transform, false);
        var qlRt = qLabelObj.AddComponent<RectTransform>();
        qlRt.anchorMin = Vector2.zero; qlRt.anchorMax = Vector2.one;
        qlRt.offsetMin = Vector2.zero; qlRt.offsetMax = Vector2.zero;
        var qlTmp = qLabelObj.AddComponent<TextMeshProUGUI>();
        qlTmp.font = font; qlTmp.text = "?";
        qlTmp.fontSize = 40; qlTmp.alignment = TextAlignmentOptions.Center; qlTmp.color = Color.white;
        qBtn.onClick.AddListener(() => panelObj.SetActive(true));

        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = panelObj;
        ipSO.FindProperty("_titleText").objectReferenceValue = titleTmp;
        ipSO.FindProperty("_descriptionText").objectReferenceValue = descTmp;
        ipSO.FindProperty("_controlsText").objectReferenceValue = ctrlTmp;
        ipSO.FindProperty("_goalText").objectReferenceValue = goalTmp;
        ipSO.FindProperty("_startButton").objectReferenceValue = startBtn;
        ipSO.FindProperty("_helpButton").objectReferenceValue = qBtn;
        ipSO.ApplyModifiedProperties();

        return ip;
    }

    static GameObject CreateText(Transform parent, string name, string text, TMP_FontAsset font, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.font = font; tmp.text = text; tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = color;
        return obj;
    }

    static GameObject CreateButton(Transform parent, string name, string label, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos, Color bgColor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        obj.AddComponent<Image>().color = bgColor;
        obj.AddComponent<Button>();
        if (!string.IsNullOrEmpty(label))
        {
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            var lRt = labelObj.AddComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero; lRt.offsetMax = Vector2.zero;
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font; tmp.text = label; tmp.fontSize = 30;
            tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        }
        return obj;
    }

    static GameObject CreatePanel(Transform parent, string name, Color bgColor, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
        obj.AddComponent<Image>().color = bgColor;
        return obj;
    }

    static void EnsureSpriteImport(string path)
    {
        if (!File.Exists(path)) return;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
    }

    static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        var list = new List<EditorBuildSettingsScene>(scenes)
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[Setup053v2] シーンをBuildSettingsに追加: {scenePath}");
    }
}
