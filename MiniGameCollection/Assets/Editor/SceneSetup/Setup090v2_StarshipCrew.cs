using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game090v2_StarshipCrew;

public static class Setup090v2_StarshipCrew
{
    [MenuItem("Assets/Setup/090v2 StarshipCrew")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup090v2] Play モード中は実行できません。"); return; }
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        string sp = "Assets/Resources/Sprites/Game090v2_StarshipCrew/";

        // === Camera ===
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.03f, 0.03f, 0.16f);
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
            float camSize = 6f;
            float camWidth = camSize * (16f / 9f);
            float scaleX = camWidth * 2f / bgSprite.bounds.size.x;
            float scaleY = camSize * 2f / bgSprite.bounds.size.y;
            bgObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // === Load sprites ===
        Sprite sprCrew0 = LoadSprite(sp + "crew_0.png");
        Sprite sprCrew1 = LoadSprite(sp + "crew_1.png");
        Sprite sprCrew2 = LoadSprite(sp + "crew_2.png");
        Sprite sprCrew3 = LoadSprite(sp + "crew_3.png");
        Sprite sprCrew4 = LoadSprite(sp + "crew_4.png");
        Sprite sprCrew5 = LoadSprite(sp + "crew_5.png");
        Sprite sprCrew6 = LoadSprite(sp + "crew_6.png");
        Sprite sprCrew7 = LoadSprite(sp + "crew_7.png");
        Sprite sprCrew8 = LoadSprite(sp + "crew_8.png");
        Sprite sprCrew9 = LoadSprite(sp + "crew_9.png");

        Sprite sprMissionEasy     = LoadSprite(sp + "mission_easy.png");
        Sprite sprMissionMedium   = LoadSprite(sp + "mission_medium.png");
        Sprite sprMissionHard     = LoadSprite(sp + "mission_hard.png");
        Sprite sprMissionVeryHard = LoadSprite(sp + "mission_veryhard.png");
        Sprite sprMissionBoss     = LoadSprite(sp + "mission_boss.png");

        Sprite sprEquipCombat  = LoadSprite(sp + "equip_combat.png");
        Sprite sprEquipEngine  = LoadSprite(sp + "equip_engine.png");
        Sprite sprEquipMedical = LoadSprite(sp + "equip_medical.png");

        // === GameManager hierarchy ===
        var gmObj = new GameObject("StarshipCrewGameManager");
        var gm = gmObj.AddComponent<StarshipCrewGameManager>();

        // StageManager (child of GM)
        var smObj = new GameObject("StageManager");
        smObj.transform.SetParent(gmObj.transform);
        var sm = smObj.AddComponent<StageManager>();
        var stageConfigs = new StageManager.StageConfig[]
        {
            new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 1, complexityFactor = 0.0f, stageName = "Stage 1" },
            new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 1, complexityFactor = 0.3f, stageName = "Stage 2" },
            new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 2, complexityFactor = 0.5f, stageName = "Stage 3" },
            new StageManager.StageConfig { speedMultiplier = 1.8f, countMultiplier = 2, complexityFactor = 0.8f, stageName = "Stage 4" },
            new StageManager.StageConfig { speedMultiplier = 2.0f, countMultiplier = 3, complexityFactor = 1.0f, stageName = "Stage 5" },
        };
        sm.SetConfigs(stageConfigs);

        // CrewManager (child of GM)
        var crewMgrObj = new GameObject("CrewManager");
        crewMgrObj.transform.SetParent(gmObj.transform);
        var crewMgr = crewMgrObj.AddComponent<CrewManager>();

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

        // === HUD (top area) ===
        var stageTextGo = CT(canvasObj.transform, "StageText", "Stage 1 / 5", 38, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(320, 48), new Vector2(15, -15));
        stageTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.5f);

        var scoreTextGo = CT(canvasObj.transform, "ScoreText", "Score: 0", 38, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(320, 48), new Vector2(-15, -15));
        scoreTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        scoreTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.85f, 0.5f);

        var requiredTextGo = CT(canvasObj.transform, "RequiredText", "クリア: 0 / 2", 32, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400, 42), new Vector2(0, -15));
        requiredTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        requiredTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 0.7f);

        var comboTextGo = CT(canvasObj.transform, "ComboText", "", 34, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(600, 44), new Vector2(0, -65));
        comboTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        comboTextGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.2f);

        // === Crew Cards Area (upper-middle area) ===
        var crewAreaBg = new GameObject("CrewArea", typeof(RectTransform));
        crewAreaBg.transform.SetParent(canvasObj.transform, false);
        var crewAreaRT = crewAreaBg.GetComponent<RectTransform>();
        crewAreaRT.anchorMin = new Vector2(0f, 0.55f); crewAreaRT.anchorMax = new Vector2(1f, 0.88f);
        crewAreaRT.offsetMin = new Vector2(10, 0); crewAreaRT.offsetMax = new Vector2(-10, 0);
        var crewAreaImg = crewAreaBg.AddComponent<Image>();
        crewAreaImg.color = new Color(0.05f, 0.08f, 0.25f, 0.85f);

        var crewLabelGo = CT(crewAreaBg.transform, "CrewLabel", "クルー選択 (最大3人)", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(700, 40), new Vector2(0, -5));
        crewLabelGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        crewLabelGo.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.8f, 1f);

        var compatTextGo = CT(crewAreaBg.transform, "CompatText", "", 28, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(900, 38), new Vector2(0, 5));
        compatTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // CrewCards container (GridLayout)
        var crewContainer = new GameObject("CrewCardsContainer", typeof(RectTransform));
        crewContainer.transform.SetParent(crewAreaBg.transform, false);
        var crewContRT = crewContainer.GetComponent<RectTransform>();
        crewContRT.anchorMin = new Vector2(0f, 0.12f); crewContRT.anchorMax = new Vector2(1f, 0.88f);
        crewContRT.offsetMin = new Vector2(5, 0); crewContRT.offsetMax = new Vector2(-5, 0);
        var crewLayout = crewContainer.AddComponent<GridLayoutGroup>();
        crewLayout.cellSize = new Vector2(175, 110);
        crewLayout.spacing = new Vector2(6, 6);
        crewLayout.childAlignment = TextAnchor.UpperCenter;
        crewLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        crewLayout.constraintCount = 5;

        // CrewCard Prefab
        var crewCardPrefab = new GameObject("CrewCardPrefab", typeof(RectTransform));
        crewCardPrefab.transform.SetParent(canvasObj.transform, false);
        crewCardPrefab.SetActive(false);
        var ccImg = crewCardPrefab.AddComponent<Image>();
        ccImg.color = new Color(0.2f, 0.3f, 0.5f, 0.9f);
        crewCardPrefab.AddComponent<Button>().targetGraphic = ccImg;
        // Portrait child
        var portraitGo = new GameObject("Portrait", typeof(RectTransform));
        portraitGo.transform.SetParent(crewCardPrefab.transform, false);
        var portraitRT = portraitGo.GetComponent<RectTransform>();
        portraitRT.anchorMin = new Vector2(0f, 0.4f); portraitRT.anchorMax = new Vector2(1f, 1f);
        portraitRT.offsetMin = new Vector2(5, 0); portraitRT.offsetMax = new Vector2(-5, -2);
        portraitGo.AddComponent<Image>().preserveAspect = true;
        // Name text
        var ccNameGo = new GameObject("NameText", typeof(RectTransform));
        ccNameGo.transform.SetParent(crewCardPrefab.transform, false);
        var ccNameRT = ccNameGo.GetComponent<RectTransform>();
        ccNameRT.anchorMin = new Vector2(0f, 0.2f); ccNameRT.anchorMax = new Vector2(1f, 0.42f);
        ccNameRT.offsetMin = ccNameRT.offsetMax = Vector2.zero;
        var ccNameTMP = ccNameGo.AddComponent<TextMeshProUGUI>();
        ccNameTMP.fontSize = 18; ccNameTMP.alignment = TextAlignmentOptions.Center;
        if (jpFont != null) ccNameTMP.font = jpFont;
        ccNameTMP.color = Color.white;
        // Skills text
        var ccSkillGo = new GameObject("SkillText", typeof(RectTransform));
        ccSkillGo.transform.SetParent(crewCardPrefab.transform, false);
        var ccSkillRT = ccSkillGo.GetComponent<RectTransform>();
        ccSkillRT.anchorMin = new Vector2(0f, 0f); ccSkillRT.anchorMax = new Vector2(1f, 0.22f);
        ccSkillRT.offsetMin = ccSkillRT.offsetMax = Vector2.zero;
        var ccSkillTMP = ccSkillGo.AddComponent<TextMeshProUGUI>();
        ccSkillTMP.fontSize = 14; ccSkillTMP.alignment = TextAlignmentOptions.Center;
        if (jpFont != null) ccSkillTMP.font = jpFont;
        ccSkillTMP.color = new Color(0.7f, 0.9f, 0.8f);

        // === Mission Area (lower-middle) ===
        var missionAreaBg = new GameObject("MissionArea", typeof(RectTransform));
        missionAreaBg.transform.SetParent(canvasObj.transform, false);
        var missionAreaRT = missionAreaBg.GetComponent<RectTransform>();
        missionAreaRT.anchorMin = new Vector2(0f, 0.18f); missionAreaRT.anchorMax = new Vector2(1f, 0.54f);
        missionAreaRT.offsetMin = new Vector2(10, 0); missionAreaRT.offsetMax = new Vector2(-10, 0);
        var missionAreaImg = missionAreaBg.AddComponent<Image>();
        missionAreaImg.color = new Color(0.08f, 0.05f, 0.20f, 0.85f);

        var missionLabelGo = CT(missionAreaBg.transform, "MissionLabel", "ミッション選択", 30, jpFont,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(700, 40), new Vector2(0, -5));
        missionLabelGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        missionLabelGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.7f, 0.5f);

        var successRateTextGo = CT(missionAreaBg.transform, "SuccessRateText", "ミッションを選択", 32, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(800, 40), new Vector2(0, 52));
        successRateTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        successRateTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.8f);

        // Mission buttons container (vertical layout)
        var missionContainer = new GameObject("MissionButtonsContainer", typeof(RectTransform));
        missionContainer.transform.SetParent(missionAreaBg.transform, false);
        var missionContRT = missionContainer.GetComponent<RectTransform>();
        missionContRT.anchorMin = new Vector2(0f, 0.13f); missionContRT.anchorMax = new Vector2(1f, 0.88f);
        missionContRT.offsetMin = new Vector2(8, 0); missionContRT.offsetMax = new Vector2(-8, 0);
        var missionLayout = missionContainer.AddComponent<GridLayoutGroup>();
        missionLayout.cellSize = new Vector2(480, 55);
        missionLayout.spacing = new Vector2(10, 8);
        missionLayout.childAlignment = TextAnchor.UpperCenter;
        missionLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        missionLayout.constraintCount = 2;

        // Mission button prefab
        var missionBtnPrefab = new GameObject("MissionButtonPrefab", typeof(RectTransform));
        missionBtnPrefab.transform.SetParent(canvasObj.transform, false);
        missionBtnPrefab.SetActive(false);
        var mbImg = missionBtnPrefab.AddComponent<Image>();
        mbImg.color = new Color(0.3f, 0.2f, 0.5f, 0.9f);
        missionBtnPrefab.AddComponent<Button>().targetGraphic = mbImg;
        // Name text
        var mbNameGo = new GameObject("NameText", typeof(RectTransform));
        mbNameGo.transform.SetParent(missionBtnPrefab.transform, false);
        var mbNameRT = mbNameGo.GetComponent<RectTransform>();
        mbNameRT.anchorMin = new Vector2(0f, 0.5f); mbNameRT.anchorMax = new Vector2(1f, 1f);
        mbNameRT.offsetMin = new Vector2(8, 0); mbNameRT.offsetMax = new Vector2(-8, 0);
        var mbNameTMP = mbNameGo.AddComponent<TextMeshProUGUI>();
        mbNameTMP.fontSize = 24; mbNameTMP.alignment = TextAlignmentOptions.Left;
        if (jpFont != null) mbNameTMP.font = jpFont;
        mbNameTMP.color = Color.white;
        // Difficulty text
        var mbDiffGo = new GameObject("DifficultyText", typeof(RectTransform));
        mbDiffGo.transform.SetParent(missionBtnPrefab.transform, false);
        var mbDiffRT = mbDiffGo.GetComponent<RectTransform>();
        mbDiffRT.anchorMin = new Vector2(0f, 0f); mbDiffRT.anchorMax = new Vector2(1f, 0.52f);
        mbDiffRT.offsetMin = new Vector2(8, 0); mbDiffRT.offsetMax = new Vector2(-8, 0);
        var mbDiffTMP = mbDiffGo.AddComponent<TextMeshProUGUI>();
        mbDiffTMP.fontSize = 18; mbDiffTMP.alignment = TextAlignmentOptions.Left;
        if (jpFont != null) mbDiffTMP.font = jpFont;
        mbDiffTMP.color = new Color(0.8f, 0.7f, 1f);

        // === Dispatch / Cancel buttons ===
        var dispatchBtn = CB(canvasObj.transform, "DispatchButton", "派遣する", 36, jpFont,
            new Vector2(0.6f, 0f), new Vector2(0.95f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 55), new Vector2(0, 80), new Color(0.2f, 0.5f, 0.8f));
        dispatchBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.55f, 0f);
        dispatchBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0f);
        dispatchBtn.GetComponent<RectTransform>().offsetMin = new Vector2(0, 75);
        dispatchBtn.GetComponent<RectTransform>().offsetMax = new Vector2(0, 130);
        dispatchBtn.GetComponent<Button>().interactable = false;

        var cancelBtn = CB(canvasObj.transform, "CancelButton", "キャンセル", 30, jpFont,
            new Vector2(0.05f, 0f), new Vector2(0.45f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 55), new Vector2(0, 80), new Color(0.4f, 0.25f, 0.1f));
        cancelBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.05f, 0f);
        cancelBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.45f, 0f);
        cancelBtn.GetComponent<RectTransform>().offsetMin = new Vector2(0, 75);
        cancelBtn.GetComponent<RectTransform>().offsetMax = new Vector2(0, 130);

        // === Equipment Panel (Stage 3+) ===
        var equipPanel = new GameObject("EquipmentPanel", typeof(RectTransform));
        equipPanel.transform.SetParent(canvasObj.transform, false);
        var equipRT = equipPanel.GetComponent<RectTransform>();
        equipRT.anchorMin = new Vector2(0f, 0.12f); equipRT.anchorMax = new Vector2(1f, 0.18f);
        equipRT.offsetMin = new Vector2(10, 0); equipRT.offsetMax = new Vector2(-10, 0);
        var equipBg = equipPanel.AddComponent<Image>();
        equipBg.color = new Color(0.1f, 0.08f, 0.2f, 0.85f);

        var equipLabelGo = CT(equipPanel.transform, "EquipLabel", "装備:", 26, jpFont,
            new Vector2(0f, 0.5f), new Vector2(0.12f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 38), Vector2.zero);
        equipLabelGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        equipLabelGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var equipCombatBtn = CB(equipPanel.transform, "EquipCombatBtn", "⚔ 戦闘", 24, jpFont,
            new Vector2(0.14f, 0.1f), new Vector2(0.38f, 0.9f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), Vector2.zero, new Color(0.5f, 0.1f, 0.1f));
        equipCombatBtn.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        equipCombatBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        var equipEngineBtn = CB(equipPanel.transform, "EquipEngineBtn", "🔧 技術", 24, jpFont,
            new Vector2(0.40f, 0.1f), new Vector2(0.64f, 0.9f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), Vector2.zero, new Color(0.1f, 0.3f, 0.5f));
        equipEngineBtn.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        equipEngineBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        var equipMedicalBtn = CB(equipPanel.transform, "EquipMedicalBtn", "💊 医療", 24, jpFont,
            new Vector2(0.66f, 0.1f), new Vector2(0.90f, 0.9f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), Vector2.zero, new Color(0.1f, 0.4f, 0.2f));
        equipMedicalBtn.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        equipMedicalBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        equipPanel.SetActive(false);

        // === Result Panel ===
        var resultPanel = new GameObject("ResultPanel", typeof(RectTransform));
        resultPanel.transform.SetParent(canvasObj.transform, false);
        var resultRT = resultPanel.GetComponent<RectTransform>();
        resultRT.anchorMin = new Vector2(0.1f, 0.35f); resultRT.anchorMax = new Vector2(0.9f, 0.65f);
        resultRT.offsetMin = resultRT.offsetMax = Vector2.zero;
        var resultBg = resultPanel.AddComponent<Image>();
        resultBg.color = new Color(0.1f, 0.4f, 0.15f, 0.96f);

        var resultTitleGo = CT(resultPanel.transform, "ResultTitle", "ミッション成功!", 52, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 80), Vector2.zero);
        resultTitleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        resultTitleGo.GetComponent<TextMeshProUGUI>().color = Color.white;

        var resultScoreGo = CT(resultPanel.transform, "ResultScore", "+0pt", 46, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 70), Vector2.zero);
        resultScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        resultScoreGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.3f);

        var resultBonusGo = CT(resultPanel.transform, "ResultBonus", "", 32, jpFont,
            new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.5f),
            new Vector2(700, 48), Vector2.zero);
        resultBonusGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        resultBonusGo.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.8f);

        var continueBtn = CB(resultPanel.transform, "ContinueButton", "続ける", 40, jpFont,
            new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.5f),
            new Vector2(280, 60), Vector2.zero, new Color(0.2f, 0.35f, 0.55f));

        resultPanel.SetActive(false);

        // === Stage Clear Panel ===
        var scPanel = new GameObject("StageClearPanel", typeof(RectTransform));
        scPanel.transform.SetParent(canvasObj.transform, false);
        var scRT = scPanel.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0.1f, 0.35f); scRT.anchorMax = new Vector2(0.9f, 0.65f);
        scRT.offsetMin = scRT.offsetMax = Vector2.zero;
        var scBg = scPanel.AddComponent<Image>();
        scBg.color = new Color(0.05f, 0.2f, 0.1f, 0.97f);

        var scTextGo = CT(scPanel.transform, "StageClearText", "ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 80), Vector2.zero);
        scTextGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        scTextGo.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 1f, 0.5f);

        var scNextBtn = CB(scPanel.transform, "NextStageButton", "次のステージへ", 42, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 65), Vector2.zero, new Color(0.15f, 0.4f, 0.2f));
        scPanel.SetActive(false);

        // === All Clear Panel ===
        var acPanel = new GameObject("AllClearPanel", typeof(RectTransform));
        acPanel.transform.SetParent(canvasObj.transform, false);
        var acRT = acPanel.GetComponent<RectTransform>();
        acRT.anchorMin = new Vector2(0.1f, 0.3f); acRT.anchorMax = new Vector2(0.9f, 0.7f);
        acRT.offsetMin = acRT.offsetMax = Vector2.zero;
        var acBg = acPanel.AddComponent<Image>();
        acBg.color = new Color(0.05f, 0.1f, 0.3f, 0.97f);

        var acScoreGo = CT(acPanel.transform, "AllClearScore", "全クリア！\nFinalScore: 0", 50, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 160), Vector2.zero);
        acScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        acScoreGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);

        var acBackBtn = CB(acPanel.transform, "BackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(340, 65), Vector2.zero, new Color(0.2f, 0.35f, 0.5f));
        acBackBtn.AddComponent<BackToMenuButton>();
        acPanel.SetActive(false);

        // === Game Over Panel ===
        var goPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        goPanel.transform.SetParent(canvasObj.transform, false);
        var goRT = goPanel.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.1f, 0.3f); goRT.anchorMax = new Vector2(0.9f, 0.7f);
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;
        var goBg = goPanel.AddComponent<Image>();
        goBg.color = new Color(0.25f, 0.05f, 0.05f, 0.97f);

        var goScoreGo = CT(goPanel.transform, "GameOverScore", "ゲームオーバー\nScore: 0", 50, jpFont,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(800, 160), Vector2.zero);
        goScoreGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        goScoreGo.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.4f, 0.4f);

        var goRetryBtn = CB(goPanel.transform, "BackButton", "メニューへ", 42, jpFont,
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f),
            new Vector2(340, 65), Vector2.zero, new Color(0.4f, 0.15f, 0.15f));
        goRetryBtn.AddComponent<BackToMenuButton>();
        goPanel.SetActive(false);

        // === Screen Flash overlay ===
        var flashGo = new GameObject("ScreenFlash", typeof(RectTransform));
        flashGo.transform.SetParent(canvasObj.transform, false);
        var flashRT = flashGo.GetComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero; flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = flashRT.offsetMax = Vector2.zero;
        var flashImg = flashGo.AddComponent<Image>();
        flashImg.color = new Color(1f, 0f, 0f, 0f);
        flashImg.raycastTarget = false;

        // === Bottom persistent buttons ===
        var backBtn = CB(canvasObj.transform, "BackButton", "メニューへ", 28, jpFont,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(180, 55), new Vector2(12, 12), new Color(0.15f, 0.2f, 0.4f, 0.9f));
        backBtn.AddComponent<BackToMenuButton>();

        var helpBtn = CB(canvasObj.transform, "HelpButton", "?", 40, jpFont,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(60, 60), new Vector2(-12, 75), new Color(0.2f, 0.25f, 0.45f, 0.9f));

        // === StarshipCrewUI component ===
        var uiGo = new GameObject("StarshipCrewUI");
        var ui = uiGo.AddComponent<StarshipCrewUI>();

        SetField(ui, "_stageText",             stageTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_scoreText",             scoreTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_comboText",             comboTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_requiredText",          requiredTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_crewCardsContainer",    crewContainer.transform);
        SetField(ui, "_crewCardPrefab",        crewCardPrefab);
        SetField(ui, "_missionButtonsContainer", missionContainer.transform);
        SetField(ui, "_missionButtonPrefab",   missionBtnPrefab);
        SetField(ui, "_dispatchButton",        dispatchBtn.GetComponent<Button>());
        SetField(ui, "_cancelButton",          cancelBtn.GetComponent<Button>());
        SetField(ui, "_successRateText",       successRateTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_compatText",            compatTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_equipmentPanel",        equipPanel);
        SetField(ui, "_equipCombatBtn",        equipCombatBtn.GetComponent<Button>());
        SetField(ui, "_equipEngineBtn",        equipEngineBtn.GetComponent<Button>());
        SetField(ui, "_equipMedicalBtn",       equipMedicalBtn.GetComponent<Button>());
        SetField(ui, "_resultPanel",           resultPanel);
        SetField(ui, "_resultTitleText",       resultTitleGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_resultScoreText",       resultScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_resultBonusText",       resultBonusGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_continueButton",        continueBtn.GetComponent<Button>());
        SetField(ui, "_resultBgImage",         resultBg);
        SetField(ui, "_stageClearPanel",       scPanel);
        SetField(ui, "_stageClearText",        scTextGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_nextStageButton",       scNextBtn.GetComponent<Button>());
        SetField(ui, "_allClearPanel",         acPanel);
        SetField(ui, "_allClearScoreText",     acScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_gameOverPanel",         goPanel);
        SetField(ui, "_gameOverScoreText",     goScoreGo.GetComponent<TextMeshProUGUI>());
        SetField(ui, "_screenFlash",           flashImg);
        SetField(ui, "_gameManager",           gm);
        SetField(ui, "_crewManager",           crewMgr);

        // Button event wiring
        scNextBtn.GetComponent<Button>().onClick.AddListener(() => gm.NextStage());
        continueBtn.GetComponent<Button>().onClick.AddListener(() => ui.OnContinueClicked());
        dispatchBtn.GetComponent<Button>().onClick.AddListener(() => crewMgr.OnDispatchClicked());
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => crewMgr.OnCancelClicked());

        // === Wire CrewManager ===
        SetField(crewMgr, "_gameManager",      gm);
        SetField(crewMgr, "_ui",               ui);
        SetField(crewMgr, "_sprCrew0",         sprCrew0);
        SetField(crewMgr, "_sprCrew1",         sprCrew1);
        SetField(crewMgr, "_sprCrew2",         sprCrew2);
        SetField(crewMgr, "_sprCrew3",         sprCrew3);
        SetField(crewMgr, "_sprCrew4",         sprCrew4);
        SetField(crewMgr, "_sprCrew5",         sprCrew5);
        SetField(crewMgr, "_sprCrew6",         sprCrew6);
        SetField(crewMgr, "_sprCrew7",         sprCrew7);
        SetField(crewMgr, "_sprCrew8",         sprCrew8);
        SetField(crewMgr, "_sprCrew9",         sprCrew9);
        SetField(crewMgr, "_sprMissionEasy",     sprMissionEasy);
        SetField(crewMgr, "_sprMissionMedium",   sprMissionMedium);
        SetField(crewMgr, "_sprMissionHard",     sprMissionHard);
        SetField(crewMgr, "_sprMissionVeryHard", sprMissionVeryHard);
        SetField(crewMgr, "_sprMissionBoss",     sprMissionBoss);
        SetField(crewMgr, "_sprEquipCombat",   sprEquipCombat);
        SetField(crewMgr, "_sprEquipEngine",   sprEquipEngine);
        SetField(crewMgr, "_sprEquipMedical",  sprEquipMedical);

        // === InstructionPanel ===
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
        ipBgImg.color = new Color(0.03f, 0.05f, 0.2f, 0.97f);

        var ip = ipBg.AddComponent<InstructionPanel>();

        var ipTitle = CT(ipBg.transform, "TitleText", "StarshipCrew", 68, jpFont,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 100), Vector2.zero);
        ipTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipTitle.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.8f, 1f);

        var ipDesc = CT(ipBg.transform, "DescText", "", 38, jpFont,
            new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.85f, 0.9f);

        var ipCtrl = CT(ipBg.transform, "ControlsText", "", 32, jpFont,
            new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 150), Vector2.zero);
        ipCtrl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipCtrl.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 0.8f);

        var ipGoal = CT(ipBg.transform, "GoalText", "", 32, jpFont,
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f),
            new Vector2(900, 80), Vector2.zero);
        ipGoal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        ipGoal.GetComponent<TextMeshProUGUI>().color = new Color(0.4f, 0.9f, 0.5f);

        var startBtn = CB(ipBg.transform, "StartButton", "はじめる", 50, jpFont,
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f),
            new Vector2(380, 72), Vector2.zero, new Color(0.2f, 0.4f, 0.65f));

        // Wire InstructionPanel
        SetField(ip, "_titleText",       ipTitle.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_descriptionText", ipDesc.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_controlsText",    ipCtrl.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_goalText",        ipGoal.GetComponent<TextMeshProUGUI>());
        SetField(ip, "_startButton",     startBtn.GetComponent<Button>());
        SetField(ip, "_helpButton",      helpBtn.GetComponent<Button>());
        SetField(ip, "_panelRoot",       ipBg);

        // Wire GameManager
        SetField(gm, "_stageManager",     sm);
        SetField(gm, "_instructionPanel", ip);
        SetField(gm, "_crewManager",      crewMgr);
        SetField(gm, "_ui",              ui);

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // Save scene
        string scenePath = "Assets/Scenes/090v2_StarshipCrew.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup090v2] StarshipCrew シーン作成完了: " + scenePath);
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
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
        else Debug.LogWarning($"[Setup090v2] Field not found: {fieldName} on {obj.GetType().Name}");
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
