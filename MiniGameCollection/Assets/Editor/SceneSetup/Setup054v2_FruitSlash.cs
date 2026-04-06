using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game054v2_FruitSlash;

public static class Setup054v2_FruitSlash
{
    [MenuItem("Assets/Setup/054v2 FruitSlash")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup054v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game054v2_FruitSlash/";

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.08f, 0.14f, 0.08f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
        }

        // Background
        Sprite bgSprite = LoadSprite(sp + "Background.png");
        if (bgSprite != null)
        {
            var bgObj = new GameObject("Background");
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(0.02f, 0.0115f, 1f);
        }

        // Fruit sprites
        Sprite[] fruitSprites = new Sprite[]
        {
            LoadSprite(sp + "Apple.png"),
            LoadSprite(sp + "Watermelon.png"),
            LoadSprite(sp + "GoldFruit.png"),
            LoadSprite(sp + "IceFruit.png"),
            LoadSprite(sp + "Bomb.png"),
            LoadSprite(sp + "BigBomb.png"),
        };

        // GameManager root
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<FruitSlashGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager"); smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // FruitManager
        var fmObj = new GameObject("FruitManager"); fmObj.transform.SetParent(gmObj.transform);
        var fm = fmObj.AddComponent<FruitManager>();
        var fmSO = new SerializedObject(fm);
        var fruitSpriteProp = fmSO.FindProperty("_fruitSprites");
        fruitSpriteProp.arraySize = fruitSprites.Length;
        for (int i = 0; i < fruitSprites.Length; i++)
            fruitSpriteProp.GetArrayElementAtIndex(i).objectReferenceValue = fruitSprites[i];
        fmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 34, jpFont,
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(400,50), new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var scoreText = CT(canvasObj.transform, "ScoreText", "0", 40, jpFont,
            new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(300,55), new Vector2(-20,-20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        var targetText = CT(canvasObj.transform, "TargetScoreText", "目標: 100", 28, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(250,40), new Vector2(20,-20));
        targetText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 1f, 0.8f);

        var timerText = CT(canvasObj.transform, "TimerText", "35s", 36, jpFont,
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(200,50), new Vector2(0,-70));
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Progress slider
        var sliderObj = new GameObject("ProgressSlider", typeof(RectTransform));
        sliderObj.transform.SetParent(canvasObj.transform, false);
        var slider = sliderObj.AddComponent<Slider>();
        var sliderRT = sliderObj.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0,1); sliderRT.anchorMax = new Vector2(1,1);
        sliderRT.pivot = new Vector2(0.5f,1); sliderRT.sizeDelta = new Vector2(-40,20);
        sliderRT.anchoredPosition = new Vector2(0,-125);
        var sliderBg = new GameObject("Background", typeof(RectTransform));
        sliderBg.transform.SetParent(sliderObj.transform, false);
        sliderBg.AddComponent<Image>().color = new Color(0.2f,0.2f,0.2f,0.8f);
        var bgRT = sliderBg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one; faRT.offsetMin = faRT.offsetMax = Vector2.zero;
        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        fill.AddComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.targetGraphic = fill.GetComponent<Image>();
        slider.minValue = 0; slider.maxValue = 100; slider.value = 0;
        slider.interactable = false;

        // Hearts (life)
        var heartObjs = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var h = new GameObject($"Heart{i}", typeof(RectTransform));
            h.transform.SetParent(canvasObj.transform, false);
            var img = h.AddComponent<Image>();
            img.color = Color.red;
            var rt = h.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1);
            rt.sizeDelta = new Vector2(50,50);
            rt.anchoredPosition = new Vector2(20 + i * 55f, -140);
            heartObjs[i] = h;
            // Heart shape text
            var ht = new GameObject("Txt", typeof(RectTransform));
            ht.transform.SetParent(h.transform, false);
            var tmp = ht.AddComponent<TextMeshProUGUI>();
            tmp.text = "♥"; tmp.fontSize = 40; tmp.color = Color.red;
            tmp.alignment = TextAlignmentOptions.Center;
            if (jpFont) tmp.font = jpFont;
            var htr = ht.GetComponent<RectTransform>();
            htr.anchorMin = Vector2.zero; htr.anchorMax = Vector2.one; htr.offsetMin = htr.offsetMax = Vector2.zero;
            h.GetComponent<Image>().color = Color.clear; // Use text only
        }

        // Combo text
        var comboText = CT(canvasObj.transform, "ComboText", "", 44, jpFont,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(500,80), new Vector2(0, 50));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        // Menu button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 26, jpFont,
            new Vector2(0,0), new Vector2(0,0), new Vector2(0,0), new Vector2(260,55), new Vector2(20,20),
            new Color(0.3f,0.3f,0.4f,0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Stage Clear Panel
        var clearPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.85f,0.95f,0.85f,0.95f));
        var clearText = CT(clearPanel.transform, "ClearText", "Stage クリア！", 48, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(600,100), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        clearText.GetComponent<TextMeshProUGUI>().color = new Color(0.1f,0.5f,0.1f);
        var nextBtn = CB(clearPanel.transform, "NextStageButton", "次のステージへ", 32, jpFont,
            new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(280,65), Vector2.zero,
            new Color(0.2f,0.6f,0.2f));
        clearPanel.SetActive(false);

        // Game Over Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.95f,0.85f,0.85f,0.95f));
        CT(goPanel.transform, "GOTitle", "ゲームオーバー", 48, jpFont,
            new Vector2(0.5f,0.7f), new Vector2(0.5f,0.7f), new Vector2(0.5f,0.5f), new Vector2(600,80), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScoreText = CT(goPanel.transform, "GOScore", "スコア: 0", 34, jpFont,
            new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(500,60), Vector2.zero);
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.5f,0.25f), new Vector2(0.5f,0.25f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero,
            new Color(0.6f,0.2f,0.2f));
        goPanel.SetActive(false);

        // All Clear Panel
        var acPanel = CreatePanel(canvasObj.transform, "AllClearPanel", new Color(0.9f,0.95f,0.8f,0.95f));
        CT(acPanel.transform, "ACTitle", "全ステージクリア！", 44, jpFont,
            new Vector2(0.5f,0.72f), new Vector2(0.5f,0.72f), new Vector2(0.5f,0.5f), new Vector2(700,80), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var acScoreText = CT(acPanel.transform, "ACScore", "最終スコア: 0", 34, jpFont,
            new Vector2(0.5f,0.55f), new Vector2(0.5f,0.55f), new Vector2(0.5f,0.5f), new Vector2(500,60), Vector2.zero);
        acScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var acRetryBtn = CB(acPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f,0.22f), new Vector2(0.5f,0.22f), new Vector2(0.5f,0.5f), new Vector2(220,65), Vector2.zero,
            new Color(0.3f,0.6f,0.2f));
        acPanel.SetActive(false);

        // InstructionPanel
        var ipCanvas = new GameObject("InstructionCanvas");
        var ipCanvasComp = ipCanvas.AddComponent<Canvas>();
        ipCanvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        ipCanvasComp.sortingOrder = 100;
        var ipScaler = ipCanvas.AddComponent<CanvasScaler>();
        ipScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ipScaler.referenceResolution = new Vector2(1080, 1920);
        ipCanvas.AddComponent<GraphicRaycaster>();

        var ipRoot = new GameObject("InstructionPanel", typeof(RectTransform));
        ipRoot.transform.SetParent(ipCanvas.transform, false);
        ipRoot.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        var ipRT = ipRoot.GetComponent<RectTransform>();
        ipRT.anchorMin = Vector2.zero; ipRT.anchorMax = Vector2.one;
        ipRT.offsetMin = ipRT.offsetMax = Vector2.zero;

        var ipTitle = CT(ipRoot.transform, "TitleText", "FruitSlash", 64, jpFont,
            new Vector2(0.5f,0.8f), new Vector2(0.5f,0.8f), new Vector2(0.5f,0.5f), new Vector2(800,100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.4f,1f,0.4f);

        var ipDesc = CT(ipRoot.transform, "DescriptionText", "", 34, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(900,80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipControls = CT(ipRoot.transform, "ControlsText", "", 30, jpFont,
            new Vector2(0.5f,0.52f), new Vector2(0.5f,0.52f), new Vector2(0.5f,0.5f), new Vector2(900,120), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.9f,0.9f,0.7f);

        var ipGoal = CT(ipRoot.transform, "GoalText", "", 30, jpFont,
            new Vector2(0.5f,0.38f), new Vector2(0.5f,0.38f), new Vector2(0.5f,0.5f), new Vector2(900,80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.8f,1f,0.8f);

        var ipStartBtn = CB(ipRoot.transform, "StartButton", "はじめる", 36, jpFont,
            new Vector2(0.5f,0.2f), new Vector2(0.5f,0.2f), new Vector2(0.5f,0.5f), new Vector2(260,70), Vector2.zero,
            new Color(0.2f,0.6f,0.2f));

        var ipHelpBtn = CB(canvasObj.transform, "HelpButton", "?", 32, jpFont,
            new Vector2(1,0), new Vector2(1,0), new Vector2(1,0), new Vector2(65,65), new Vector2(-20,90),
            new Color(0.3f,0.5f,0.3f,0.9f));

        var ip = ipRoot.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipRoot;
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipControls.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_helpButton").objectReferenceValue = ipHelpBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // FruitSlashUI
        var uiObj = new GameObject("FruitSlashUI"); uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<FruitSlashUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_targetScoreText").objectReferenceValue = targetText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_progressSlider").objectReferenceValue = slider;
        var heartProp = uiSO.FindProperty("_heartObjects");
        heartProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
            heartProp.GetArrayElementAtIndex(i).objectReferenceValue = heartObjs[i];
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_stageClearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_retryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_allClearPanel").objectReferenceValue = acPanel;
        uiSO.FindProperty("_allClearScoreText").objectReferenceValue = acScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_allClearRetryButton").objectReferenceValue = acRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_fruitManager").objectReferenceValue = fm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.ApplyModifiedProperties();

        // Button listeners
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, gm.OnNextStage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(acRetryBtn.GetComponent<Button>().onClick, gm.RestartGame);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/054v2_FruitSlash.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup054v2] FruitSlash シーンを作成しました: " + scenePath);
    }

    private static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = color;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.05f, 0.3f); r.anchorMax = new Vector2(0.95f, 0.7f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        return obj;
    }

    private static GameObject CT(Transform p, string n, string t, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap)
    {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        var tmp = o.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = fs; tmp.color = Color.white;
        if (f != null) tmp.font = f;
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        return o;
    }

    private static GameObject CB(Transform p, string n, string l, float fs, TMP_FontAsset f,
        Vector2 amin, Vector2 amax, Vector2 piv, Vector2 sd, Vector2 ap, Color bg)
    {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        o.AddComponent<Image>().color = bg;
        o.AddComponent<Button>();
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.pivot = piv; r.sizeDelta = sd; r.anchoredPosition = ap;
        var t = new GameObject("Text", typeof(RectTransform));
        t.transform.SetParent(o.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (f != null) tmp.font = f;
        var tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        return o;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
