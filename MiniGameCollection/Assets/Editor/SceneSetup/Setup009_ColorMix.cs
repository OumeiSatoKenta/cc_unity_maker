using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using Game009_ColorMix;

public static class Setup009_ColorMix
{
    [MenuItem("Assets/Setup/009 ColorMix")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Setup009_ColorMix] Play モード中は実行できません。");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.06f, 0.07f, 0.12f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        string whiteTexPath = "Assets/Scripts/Game009_ColorMix/WhiteSquare.png";
        if (!System.IO.File.Exists(whiteTexPath))
        {
            var wTex = new Texture2D(4, 4);
            var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = Color.white;
            wTex.SetPixels(px); wTex.Apply();
            System.IO.File.WriteAllBytes(whiteTexPath, wTex.EncodeToPNG());
            Object.DestroyImmediate(wTex);
            AssetDatabase.ImportAsset(whiteTexPath);
            var imp = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter;
            if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spritePixelsPerUnit = 1; imp.SaveAndReimport(); }
        }
        var whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);

        // Target color display (left)
        var targetObj = new GameObject("TargetColor");
        var targetSr = targetObj.AddComponent<SpriteRenderer>();
        targetSr.sprite = whiteSprite;
        targetSr.color = Color.red;
        targetSr.sortingOrder = 5;
        targetObj.transform.position = new Vector3(-2.5f, 1.5f, 0f);
        targetObj.transform.localScale = new Vector3(3f, 3f, 1f);

        // Player color display (right)
        var playerObj = new GameObject("PlayerColor");
        var playerSr = playerObj.AddComponent<SpriteRenderer>();
        playerSr.sprite = whiteSprite;
        playerSr.color = new Color(0.5f, 0.5f, 0.5f);
        playerSr.sortingOrder = 5;
        playerObj.transform.position = new Vector3(2.5f, 1.5f, 0f);
        playerObj.transform.localScale = new Vector3(3f, 3f, 1f);

        // Labels above blocks
        var targetLabel = new GameObject("TargetLabel");
        var tlTmp = targetLabel.AddComponent<TextMeshPro>();
        tlTmp.text = "目標"; tlTmp.fontSize = 4; tlTmp.color = Color.white;
        tlTmp.alignment = TextAlignmentOptions.Center;
        targetLabel.transform.position = new Vector3(-2.5f, 3.5f, 0f);

        var playerLabel = new GameObject("PlayerLabel");
        var plTmp = playerLabel.AddComponent<TextMeshPro>();
        plTmp.text = "あなたの色"; plTmp.fontSize = 4; plTmp.color = Color.white;
        plTmp.alignment = TextAlignmentOptions.Center;
        playerLabel.transform.position = new Vector3(2.5f, 3.5f, 0f);

        // GameManager + ColorMixManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<ColorMixGameManager>();
        var boardObj = new GameObject("ColorBoard");
        boardObj.transform.SetParent(gmObj.transform);
        var cm = boardObj.AddComponent<ColorMixManager>();

        var cmSO = new SerializedObject(cm);
        cmSO.FindProperty("_playerColorDisplay").objectReferenceValue = playerSr;
        cmSO.FindProperty("_targetColorDisplay").objectReferenceValue = targetSr;
        cmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        var stageText = CreateText(canvasObj.transform, "StageText", "ステージ 1", 32, jpFont,
            new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(300,50), new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var matchText = CreateText(canvasObj.transform, "MatchText", "一致度: 0%", 28, jpFont,
            new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(250,40), new Vector2(20,-20));

        var menuBtn = CreateButton(canvasObj.transform, "MenuButton", "メニューへ戻る", 24, jpFont,
            new Vector2(1,1), new Vector2(1,1), new Vector2(1,1), new Vector2(240,50), new Vector2(-20,-20),
            new Color(0.3f,0.3f,0.4f,0.9f));

        // RGB Sliders (bottom area)
        var redSlider = CreateSlider(canvasObj.transform, "RedSlider", new Color(0.8f,0.2f,0.2f), jpFont, "R",
            new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0.5f),
            new Vector2(500,40), new Vector2(0, 160));
        var greenSlider = CreateSlider(canvasObj.transform, "GreenSlider", new Color(0.2f,0.7f,0.2f), jpFont, "G",
            new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0.5f),
            new Vector2(500,40), new Vector2(0, 100));
        var blueSlider = CreateSlider(canvasObj.transform, "BlueSlider", new Color(0.2f,0.2f,0.8f), jpFont, "B",
            new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0.5f),
            new Vector2(500,40), new Vector2(0, 40));

        // Clear panel
        var clearPanel = new GameObject("ClearPanel", typeof(RectTransform));
        clearPanel.transform.SetParent(canvasObj.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0,0,0,0.8f);
        var cr = clearPanel.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.2f,0.2f); cr.anchorMax = new Vector2(0.8f,0.8f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;
        var clearText = CreateText(clearPanel.transform, "ClearText", "クリア!", 48, jpFont,
            new Vector2(0.5f,0.65f), new Vector2(0.5f,0.65f), new Vector2(0.5f,0.5f), new Vector2(400,150), Vector2.zero);
        clearText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var restartBtn = CreateButton(clearPanel.transform, "RestartButton", "もう一度", 28, jpFont,
            new Vector2(0.3f,0.15f), new Vector2(0.3f,0.15f), new Vector2(0.5f,0.5f), new Vector2(200,60), Vector2.zero,
            new Color(0.1f,0.5f,0.3f,1f));
        var nextBtn = CreateButton(clearPanel.transform, "NextStageButton", "次のステージ", 28, jpFont,
            new Vector2(0.7f,0.15f), new Vector2(0.7f,0.15f), new Vector2(0.5f,0.5f), new Vector2(220,60), Vector2.zero,
            new Color(0.2f,0.4f,0.7f,1f));
        clearPanel.SetActive(false);

        // ColorMixUI
        var uiObj = new GameObject("ColorMixUI");
        var cmUI = uiObj.AddComponent<ColorMixUI>();
        var uiSO = new SerializedObject(cmUI);
        uiSO.FindProperty("_stageText").objectReferenceValue = stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_matchText").objectReferenceValue = matchText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_redSlider").objectReferenceValue = redSlider.GetComponent<Slider>();
        uiSO.FindProperty("_greenSlider").objectReferenceValue = greenSlider.GetComponent<Slider>();
        uiSO.FindProperty("_blueSlider").objectReferenceValue = blueSlider.GetComponent<Slider>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearText").objectReferenceValue = clearText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("_nextStageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameManager").objectReferenceValue = gm;
        uiSO.FindProperty("_colorManager").objectReferenceValue = cm;
        uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_colorManager").objectReferenceValue = cm;
        gmSO.FindProperty("_ui").objectReferenceValue = cmUI;
        gmSO.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<InputSystemUIInputModule>();
        }

        string scenePath = "Assets/Scenes/009_ColorMix.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup009_ColorMix] ColorMix シーンを作成しました: " + scenePath);
    }

    private static GameObject CreateSlider(Transform parent, string name, Color fillColor,
        TMP_FontAsset font, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = pivot;
        rect.sizeDelta = sizeDelta; rect.anchoredPosition = anchoredPos;

        // Background
        var bgObj = new GameObject("Background", typeof(RectTransform));
        bgObj.transform.SetParent(obj.transform, false);
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.18f, 0.25f);
        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(40, 0); bgRect.offsetMax = Vector2.zero;

        // Fill area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(obj.transform, false);
        var faRect = fillArea.GetComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero; faRect.anchorMax = Vector2.one;
        faRect.offsetMin = new Vector2(45, 5); faRect.offsetMax = new Vector2(-5, -5);

        var fillObj = new GameObject("Fill", typeof(RectTransform));
        fillObj.transform.SetParent(fillArea.transform, false);
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = fillColor;
        var fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;

        // Handle
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(obj.transform, false);
        var haRect = handleArea.GetComponent<RectTransform>();
        haRect.anchorMin = Vector2.zero; haRect.anchorMax = Vector2.one;
        haRect.offsetMin = new Vector2(50, 0); haRect.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);

        // Slider component
        var slider = obj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;

        // Label
        var labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(obj.transform, false);
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 24; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        var lRect = labelObj.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0, 0); lRect.anchorMax = new Vector2(0, 1);
        lRect.pivot = new Vector2(0.5f, 0.5f);
        lRect.sizeDelta = new Vector2(40, 0);
        lRect.anchoredPosition = new Vector2(20, 0);

        return obj;
    }

    private static GameObject CreateText(Transform parent, string name, string text, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = Color.white;
        if (font != null) tmp.font = font;
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax; r.pivot = pivot; r.sizeDelta = sizeDelta; r.anchoredPosition = anchoredPos;
        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, float fontSize,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color bgColor)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = bgColor;
        obj.AddComponent<Button>();
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax; r.pivot = pivot; r.sizeDelta = sizeDelta; r.anchoredPosition = anchoredPos;
        var tObj = new GameObject("Text", typeof(RectTransform));
        tObj.transform.SetParent(obj.transform, false);
        var tmp = tObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        var tr = tObj.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
        return obj;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes) if (s.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
