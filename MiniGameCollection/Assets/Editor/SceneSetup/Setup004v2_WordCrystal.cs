using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game004v2_WordCrystal;

public static class Setup004v2_WordCrystal
{
    [MenuItem("Assets/Setup/004v2 WordCrystal")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup004v2] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game004v2_WordCrystal/";

        // カメラ
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.04f, 0.06f, 0.15f);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
        }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite crystalNormal = LoadSprite(sp + "crystal_normal.png");
        Sprite crystalHidden = LoadSprite(sp + "crystal_hidden.png");
        Sprite crystalBonus = LoadSprite(sp + "crystal_bonus.png");
        Sprite crystalPoison = LoadSprite(sp + "crystal_poison.png");
        Sprite letterTileSprite = LoadSprite(sp + "letter_tile.png");

        // 背景
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.025f, 0.025f, 1f) : new Vector3(16f, 14f, 1f);

        // プレハブ作成
        string prefabDir = sp;

        var crystalNormalPrefab = CreateCrystalPrefab(prefabDir + "CrystalNormalPrefab.prefab", "CrystalNormalPrefab", crystalNormal, new Color(0.2f, 0.5f, 1f));
        var crystalHiddenPrefab = CreateCrystalPrefab(prefabDir + "CrystalHiddenPrefab.prefab", "CrystalHiddenPrefab", crystalHidden, new Color(0.4f, 0.4f, 0.6f));
        var crystalBonusPrefab = CreateCrystalPrefab(prefabDir + "CrystalBonusPrefab.prefab", "CrystalBonusPrefab", crystalBonus, new Color(1f, 0.85f, 0.1f));
        var crystalPoisonPrefab = CreateCrystalPrefab(prefabDir + "CrystalPoisonPrefab.prefab", "CrystalPoisonPrefab", crystalPoison, new Color(0.7f, 0.1f, 0.3f));

        // LetterTileプレハブ（TextMeshPro子オブジェクト + CircleCollider2D）
        var letterTilePrefab = CreateLetterTilePrefab(prefabDir + "LetterTilePrefab.prefab", letterTileSprite, jpFont);

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<WordCrystalGameManager>();

        // StageManager
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();

        // WordManager
        var wmObj = new GameObject("WordManager");
        wmObj.transform.SetParent(gmObj.transform);
        var wm = wmObj.AddComponent<WordManager>();
        var wmSO = new SerializedObject(wm);
        wmSO.FindProperty("_crystalNormalPrefab").objectReferenceValue = crystalNormalPrefab;
        wmSO.FindProperty("_crystalHiddenPrefab").objectReferenceValue = crystalHiddenPrefab;
        wmSO.FindProperty("_crystalBonusPrefab").objectReferenceValue = crystalBonusPrefab;
        wmSO.FindProperty("_crystalPoisonPrefab").objectReferenceValue = crystalPoisonPrefab;
        wmSO.FindProperty("_letterTilePrefab").objectReferenceValue = letterTilePrefab;
        wmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD上部
        var stageText = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 30, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 40), new Vector2(0, -15));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var scoreText = CT(canvasObj.transform, "ScoreText", "0", 34, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 40), new Vector2(0, -55));
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

        var targetScoreText = CT(canvasObj.transform, "TargetScoreText", "目標: 500", 24, jpFont,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(220, 36), new Vector2(-15, -55));
        targetScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        targetScoreText.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        var timerText = CT(canvasObj.transform, "TimerText", "60", 38, jpFont,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(100, 50), new Vector2(20, -45));
        timerText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.7f);

        var comboText = CT(canvasObj.transform, "ComboText", "", 28, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 36), new Vector2(0, -95));
        comboText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.3f);
        comboText.SetActive(false);

        var themeLabel = CT(canvasObj.transform, "ThemeLabel", "", 24, jpFont,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(300, 32), new Vector2(0, -130));
        themeLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        themeLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.3f);
        themeLabel.SetActive(false);

        // スコアポップアップ
        var scorePopup = CT(canvasObj.transform, "ScorePopup", "", 36, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(250, 80), new Vector2(0, 150));
        scorePopup.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scorePopup.SetActive(false);

        // 単語スロット（下部エリア）
        var slotContainer = new GameObject("SlotContainer", typeof(RectTransform));
        slotContainer.transform.SetParent(canvasObj.transform, false);
        var slotContainerRect = slotContainer.GetComponent<RectTransform>();
        slotContainerRect.anchorMin = new Vector2(0, 0);
        slotContainerRect.anchorMax = new Vector2(1, 0);
        slotContainerRect.pivot = new Vector2(0.5f, 0);
        slotContainerRect.sizeDelta = new Vector2(0, 90);
        slotContainerRect.anchoredPosition = new Vector2(0, 210);

        var slotLayout = slotContainer.AddComponent<HorizontalLayoutGroup>();
        slotLayout.spacing = 6;
        slotLayout.childAlignment = TextAnchor.MiddleCenter;
        slotLayout.childForceExpandWidth = false;
        slotLayout.childForceExpandHeight = false;

        // 8スロット作成
        var slotObjects = new GameObject[8];
        var slotTexts = new TextMeshProUGUI[8];
        var slotImages = new Image[8];
        for (int i = 0; i < 8; i++)
        {
            var slotObj = new GameObject($"Slot{i}", typeof(RectTransform));
            slotObj.transform.SetParent(slotContainer.transform, false);
            var img = slotObj.AddComponent<Image>();
            img.color = new Color(0.15f, 0.2f, 0.4f, 0.9f);
            slotImages[i] = img;
            var slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(75, 80);
            var slotTxtObj = new GameObject("Text", typeof(RectTransform));
            slotTxtObj.transform.SetParent(slotObj.transform, false);
            var slotTmp = slotTxtObj.AddComponent<TextMeshProUGUI>();
            slotTmp.fontSize = 36;
            slotTmp.color = Color.white;
            slotTmp.alignment = TextAlignmentOptions.Center;
            if (jpFont != null) slotTmp.font = jpFont;
            var slotTxtRect = slotTxtObj.GetComponent<RectTransform>();
            slotTxtRect.anchorMin = Vector2.zero;
            slotTxtRect.anchorMax = Vector2.one;
            slotTxtRect.offsetMin = slotTxtRect.offsetMax = Vector2.zero;
            slotObjects[i] = slotObj;
            slotTexts[i] = slotTmp;
            slotObj.SetActive(false);
        }

        // SubmitボタンとClearボタン
        var submitBtn = CB(canvasObj.transform, "SubmitButton", "決定", 30, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(180, 65), new Vector2(-100, 140),
            new Color(0.1f, 0.45f, 0.8f));

        var clearBtn = CB(canvasObj.transform, "ClearButton", "クリア", 28, jpFont,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(150, 65), new Vector2(115, 140),
            new Color(0.35f, 0.2f, 0.5f));

        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニュー", 22, jpFont,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(150, 55), new Vector2(20, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));

        // ステージクリアパネル
        var scPanel = CreatePanel(canvasObj.transform, "StageClearPanel", new Color(0.05f, 0.1f, 0.2f, 0.95f));
        var scStageText = CT(scPanel.transform, "SCStageText", "", 36, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(550, 60), Vector2.zero);
        scStageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStageText.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.9f, 1f);
        var scScoreText = CT(scPanel.transform, "SCScoreText", "", 28, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(450, 45), Vector2.zero);
        scScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scScoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        var scStarsText = CT(scPanel.transform, "SCStarsText", "", 42, jpFont,
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 55), Vector2.zero);
        scStarsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scStarsText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.1f);
        var nextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 30, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 65), Vector2.zero, new Color(0.1f, 0.4f, 0.7f));
        scPanel.SetActive(false);

        // ゲームクリアパネル
        var clearPanel = CreatePanel(canvasObj.transform, "GameClearPanel", new Color(0.05f, 0.12f, 0.18f, 0.95f));
        CT(clearPanel.transform, "ClearTitle", "全ステージクリア！", 40, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(550, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "ClearScore", "", 30, jpFont,
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            new Vector2(450, 50), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        clearScoreText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);
        var clearMenuBtn = CB(clearPanel.transform, "ClearMenuButton", "メニューへ戻る", 28, jpFont,
            new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 60), Vector2.zero, new Color(0.2f, 0.4f, 0.3f));
        clearPanel.SetActive(false);

        // ゲームオーバーパネル
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0.05f, 0.95f));
        CT(goPanel.transform, "GOText", "タイムアップ！", 38, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), Vector2.zero).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var retryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 30, jpFont,
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 65), Vector2.zero, new Color(0.5f, 0.15f, 0.1f));
        var goMenuBtn = CB(goPanel.transform, "GOMenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(0.5f, 0.09f), new Vector2(0.5f, 0.09f), new Vector2(0.5f, 0.5f),
            new Vector2(220, 55), Vector2.zero, new Color(0.3f, 0.3f, 0.4f));
        goPanel.SetActive(false);

        // InstructionPanel
        var ipPanel = CreatePanel(canvasObj.transform, "InstructionPanel", new Color(0.02f, 0.05f, 0.15f, 0.97f));
        var ipRect = ipPanel.GetComponent<RectTransform>();
        ipRect.anchorMin = Vector2.zero; ipRect.anchorMax = Vector2.one;
        ipRect.offsetMin = ipRect.offsetMax = Vector2.zero;

        var ipTitle = CT(ipPanel.transform, "IPTitle", "", 42, jpFont,
            new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 60), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.8f, 1f);

        var ipDesc = CT(ipPanel.transform, "IPDesc", "", 28, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 50), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var ipControls = CT(ipPanel.transform, "IPControls", "", 24, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        ipControls.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipControls.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.9f, 1f);

        var ipGoal = CT(ipPanel.transform, "IPGoal", "", 24, jpFont,
            new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 60), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.6f);

        var ipStartBtn = CB(ipPanel.transform, "StartButton", "はじめる", 34, jpFont,
            new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 70), Vector2.zero, new Color(0.1f, 0.5f, 0.8f));

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 28, jpFont,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(55, 55), new Vector2(-20, 80), new Color(0.3f, 0.3f, 0.5f, 0.8f));

        // InstructionPanelコンポーネント
        var ipObj = new GameObject("InstructionPanelController");
        ipObj.transform.SetParent(gmObj.transform);
        var ip = ipObj.AddComponent<InstructionPanel>();
        var ipSO = new SerializedObject(ip);
        ipSO.FindProperty("_panelRoot").objectReferenceValue = ipPanel;
        ipSO.FindProperty("_titleText").objectReferenceValue = ipTitle.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_descriptionText").objectReferenceValue = ipDesc.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_controlsText").objectReferenceValue = ipControls.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_goalText").objectReferenceValue = ipGoal.GetComponent<TextMeshProUGUI>();
        ipSO.FindProperty("_startButton").objectReferenceValue = ipStartBtn.GetComponent<Button>();
        ipSO.FindProperty("_helpButton").objectReferenceValue = helpBtn.GetComponent<Button>();
        ipSO.ApplyModifiedProperties();

        // WordCrystalUI
        var uiObj = new GameObject("WordCrystalUI");
        uiObj.transform.SetParent(gmObj.transform);
        var wcUI = uiObj.AddComponent<WordCrystalUI>();
        var uiSO = new SerializedObject(wcUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_targetScoreText").objectReferenceValue = targetScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_comboText").objectReferenceValue = comboText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_themeLabel").objectReferenceValue = themeLabel.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_slotContainer").objectReferenceValue = slotContainer;
        uiSO.FindProperty("_scorePopupText").objectReferenceValue = scorePopup.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearPanel").objectReferenceValue = scPanel;
        uiSO.FindProperty("_stageClearStageText").objectReferenceValue = scStageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearScoreText").objectReferenceValue = scScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageClearStarsText").objectReferenceValue = scStarsText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameClearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_gameClearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_wordManager").objectReferenceValue = wm;
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;

        // スロット配列の配線
        var slotObjectsProp = uiSO.FindProperty("_slotObjects");
        slotObjectsProp.arraySize = 8;
        var slotTextsProp = uiSO.FindProperty("_slotTexts");
        slotTextsProp.arraySize = 8;
        var slotImagesProp = uiSO.FindProperty("_slotImages");
        slotImagesProp.arraySize = 8;
        for (int i = 0; i < 8; i++)
        {
            slotObjectsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotObjects[i];
            slotTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotTexts[i];
            slotImagesProp.GetArrayElementAtIndex(i).objectReferenceValue = slotImages[i];
        }
        uiSO.ApplyModifiedProperties();

        // GameManager 配線
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_stageManager").objectReferenceValue = sm;
        gmSO.FindProperty("_instructionPanel").objectReferenceValue = ip;
        gmSO.FindProperty("_wordManager").objectReferenceValue = wm;
        gmSO.FindProperty("_ui").objectReferenceValue = wcUI;
        gmSO.ApplyModifiedProperties();

        // ボタンイベント
        UnityEditor.Events.UnityEventTools.AddPersistentListener(submitBtn.GetComponent<Button>().onClick, gm.OnSubmitButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearBtn.GetComponent<Button>().onClick, gm.OnClearButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, gm.OnNextStageButtonPressed);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.GetComponent<Button>().onClick, gm.RestartStage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(clearMenuBtn.GetComponent<Button>().onClick, gm.ReturnToMenu);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/004v2_WordCrystal.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup004v2] WordCrystal シーンを作成しました: " + scenePath);
    }

    private static GameObject CreateCrystalPrefab(string path, string name, Sprite sprite, Color fallbackColor)
    {
        var obj = new GameObject(name);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = sprite != null ? Color.white : fallbackColor;
        sr.sortingOrder = 5;
        var col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.45f;
        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreateLetterTilePrefab(string path, Sprite sprite, TMP_FontAsset font)
    {
        var obj = new GameObject("LetterTilePrefab");
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
        sr.sortingOrder = 8;
        var col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        obj.AddComponent<LetterTile>();

        var textObj = new GameObject("Letter");
        textObj.transform.SetParent(obj.transform);
        textObj.transform.localPosition = Vector3.zero;
        var tmp = textObj.AddComponent<TextMeshPro>();
        tmp.fontSize = 3f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        tmp.sortingOrder = 9;

        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        return prefab;
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
        r.anchorMin = new Vector2(0.05f, 0.25f);
        r.anchorMax = new Vector2(0.95f, 0.75f);
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
        tmp.text = l; tmp.fontSize = fs; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (f != null) tmp.font = f;
        var tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
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
