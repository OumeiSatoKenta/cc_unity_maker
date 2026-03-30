using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Game009_ColorMix;

public class Setup009_ColorMix
{
    [MenuItem("Assets/Setup/009 ColorMix")]
    public static void Setup()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Exit Play Mode before running setup.");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ──────────────────────────────────────────────────
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.10f, 0.15f);
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // ── Sprites ─────────────────────────────────────────────────
        string sprDir = "Assets/Resources/Sprites/Game009_ColorMix";
        Directory.CreateDirectory(sprDir);

        Sprite sprColorBlock  = GetOrCreateSprite(sprDir + "/color_block.png",  MakeColorBlockTex());
        Sprite sprSliderBg    = GetOrCreateSprite(sprDir + "/slider_bg.png",     MakeSliderBgTex());
        Sprite sprSliderFill  = GetOrCreateSprite(sprDir + "/slider_fill.png",   MakeSliderFillTex());
        Sprite sprSliderHandle= GetOrCreateSprite(sprDir + "/slider_handle.png", MakeSliderHandleTex());
        Sprite sprPanelBg     = GetOrCreateSprite(sprDir + "/panel_bg.png",      MakePanelBgTex());

        // ── Canvas ───────────────────────────────────────────────────
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // ── Header ───────────────────────────────────────────────────
        var levelTextGo = MakeText(canvasGo.transform, "LevelText", "Level 1 / 5", 34, Color.white);
        SetRT(levelTextGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -45f), new Vector2(400f, 60f));

        var menuHeaderBtn = MakeButton(canvasGo.transform, "MenuHeaderButton", "MENU",
            new Color(0.25f, 0.25f, 0.30f));
        SetRT(menuHeaderBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -20f), new Vector2(160f, 60f));

        // ── Target color label ────────────────────────────────────────
        var targetNameGo = MakeText(canvasGo.transform, "TargetColorNameText", "Target: Orange",
            30, new Color(0.9f, 0.9f, 0.9f));
        SetRT(targetNameGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -100f), new Vector2(400f, 50f));

        // ── Color preview area ────────────────────────────────────────
        // Target color swatch (left)
        var targetLabel = MakeText(canvasGo.transform, "TargetLabel", "Target", 26,
            new Color(0.7f, 0.7f, 0.7f));
        SetRT(targetLabel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-130f, -165f), new Vector2(200f, 40f));

        var targetPreviewGo = new GameObject("TargetColorPreview");
        targetPreviewGo.transform.SetParent(canvasGo.transform, false);
        var targetImg = targetPreviewGo.AddComponent<Image>();
        targetImg.color = new Color(220 / 255f, 120 / 255f, 30 / 255f); // default orange
        targetImg.sprite = sprColorBlock;
        SetRT(targetPreviewGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-130f, -280f), new Vector2(200f, 180f));

        // Mixed color swatch (right)
        var mixedLabel = MakeText(canvasGo.transform, "MixedLabel", "Your Mix", 26,
            new Color(0.7f, 0.7f, 0.7f));
        SetRT(mixedLabel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(130f, -165f), new Vector2(200f, 40f));

        var mixedPreviewGo = new GameObject("MixedColorPreview");
        mixedPreviewGo.transform.SetParent(canvasGo.transform, false);
        var mixedImg = mixedPreviewGo.AddComponent<Image>();
        mixedImg.color = Color.black;
        mixedImg.sprite = sprColorBlock;
        SetRT(mixedPreviewGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(130f, -280f), new Vector2(200f, 180f));

        // ── Sliders ───────────────────────────────────────────────────
        // Red slider
        var redSlider  = MakeSlider(canvasGo.transform, "RedSlider",   "R",
            new Color(0.85f, 0.2f, 0.2f), sprSliderBg, sprSliderFill, sprSliderHandle,
            new Vector2(0f, -490f));
        // Green slider
        var greenSlider = MakeSlider(canvasGo.transform, "GreenSlider", "G",
            new Color(0.2f, 0.75f, 0.2f), sprSliderBg, sprSliderFill, sprSliderHandle,
            new Vector2(0f, -580f));
        // Blue slider
        var blueSlider  = MakeSlider(canvasGo.transform, "BlueSlider",  "B",
            new Color(0.2f, 0.4f, 0.90f), sprSliderBg, sprSliderFill, sprSliderHandle,
            new Vector2(0f, -670f));

        // Feedback text
        var feedbackGo = MakeText(canvasGo.transform, "FeedbackText", "", 28,
            new Color(1f, 0.8f, 0.2f));
        SetRT(feedbackGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -750f), new Vector2(500f, 50f));

        // Submit button
        var submitBtn = MakeButton(canvasGo.transform, "SubmitButton", "SUBMIT",
            new Color(0.2f, 0.6f, 0.9f));
        SetRT(submitBtn, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 100f), new Vector2(280f, 80f));

        // ── Clear Panel ──────────────────────────────────────────────
        var clearPanel = new GameObject("ClearPanel");
        clearPanel.transform.SetParent(canvasGo.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        var clearPanelRt = clearPanel.GetComponent<RectTransform>();
        clearPanelRt.anchorMin = Vector2.zero;
        clearPanelRt.anchorMax = Vector2.one;
        clearPanelRt.sizeDelta = Vector2.zero;

        var clearTitle = MakeText(clearPanel.transform, "ClearTitle", "COLOR MATCHED!", 55,
            new Color(0.4f, 0.9f, 0.5f));
        SetRT(clearTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(580f, 80f));

        var clearScore = MakeText(clearPanel.transform, "ClearScoreText", "Score: 100", 40,
            new Color(1f, 0.9f, 0.3f));
        SetRT(clearScore, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(400f, 60f));

        var restartBtn = MakeButton(clearPanel.transform, "RestartButton", "RETRY",
            new Color(0.2f, 0.55f, 0.35f));
        SetRT(restartBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(-130f, -40f), new Vector2(220f, 70f));

        var nextBtn = MakeButton(clearPanel.transform, "NextButton", "NEXT LEVEL",
            new Color(0.15f, 0.45f, 0.80f));
        SetRT(nextBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(130f, -40f), new Vector2(220f, 70f));

        var menuBtn = MakeButton(clearPanel.transform, "MenuButton", "MENU",
            new Color(0.35f, 0.35f, 0.35f));
        SetRT(menuBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -140f), new Vector2(200f, 60f));

        clearPanel.SetActive(false);

        // ── GameManager & ColorMixManager ─────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gameManager    = gmGo.AddComponent<ColorMixGameManager>();
        var colorMixMgr    = gmGo.AddComponent<ColorMixManager>();

        // ── ColorMixUI ────────────────────────────────────────────────
        var uiGo = new GameObject("ColorMixUI");
        var ui = uiGo.AddComponent<ColorMixUI>();

        {
            var so = new SerializedObject(ui);
            so.FindProperty("_levelText").objectReferenceValue         = levelTextGo.GetComponent<Text>();
            so.FindProperty("_targetColorNameText").objectReferenceValue = targetNameGo.GetComponent<Text>();
            so.FindProperty("_targetColorPreview").objectReferenceValue = targetImg;
            so.FindProperty("_mixedColorPreview").objectReferenceValue  = mixedImg;
            so.FindProperty("_feedbackText").objectReferenceValue       = feedbackGo.GetComponent<Text>();
            so.FindProperty("_redSlider").objectReferenceValue          = redSlider.GetComponent<Slider>();
            so.FindProperty("_greenSlider").objectReferenceValue        = greenSlider.GetComponent<Slider>();
            so.FindProperty("_blueSlider").objectReferenceValue         = blueSlider.GetComponent<Slider>();
            so.FindProperty("_clearPanel").objectReferenceValue         = clearPanel;
            so.FindProperty("_clearScoreText").objectReferenceValue     = clearScore.GetComponent<Text>();
            so.FindProperty("_gameManager").objectReferenceValue        = gameManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wire ColorMixManager ──────────────────────────────────────
        {
            var so = new SerializedObject(colorMixMgr);
            so.FindProperty("_ui").objectReferenceValue          = ui;
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wire GameManager ──────────────────────────────────────────
        {
            var so = new SerializedObject(gameManager);
            so.FindProperty("_colorMixManager").objectReferenceValue = colorMixMgr;
            so.FindProperty("_ui").objectReferenceValue              = ui;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wire slider OnValueChanged → ColorMixManager ──────────────
        {
            var redSliderComp = redSlider.GetComponent<Slider>();
            UnityEditor.Events.UnityEventTools.AddFloatPersistentListener(
                redSliderComp.onValueChanged, colorMixMgr.OnRedChanged, 0f);
        }
        {
            var greenSliderComp = greenSlider.GetComponent<Slider>();
            UnityEditor.Events.UnityEventTools.AddFloatPersistentListener(
                greenSliderComp.onValueChanged, colorMixMgr.OnGreenChanged, 0f);
        }
        {
            var blueSliderComp = blueSlider.GetComponent<Slider>();
            UnityEditor.Events.UnityEventTools.AddFloatPersistentListener(
                blueSliderComp.onValueChanged, colorMixMgr.OnBlueChanged, 0f);
        }

        // ── Wire buttons ──────────────────────────────────────────────
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            submitBtn.GetComponent<Button>().onClick, colorMixMgr.SubmitMix);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            restartBtn.GetComponent<Button>().onClick, ui.OnRestartClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            nextBtn.GetComponent<Button>().onClick,    ui.OnNextClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuBtn.GetComponent<Button>().onClick,    ui.OnMenuClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuHeaderBtn.GetComponent<Button>().onClick, gameManager.LoadMenu);

        // ── EventSystem ──────────────────────────────────────────────
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // ── Save ─────────────────────────────────────────────────────
        string scenesDir = "Assets/Scenes";
        Directory.CreateDirectory(scenesDir);
        string scenePath = $"{scenesDir}/009_ColorMix.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[Setup009] ColorMix scene saved to " + scenePath);
    }

    // ── Slider factory ─────────────────────────────────────────────────

    private static GameObject MakeSlider(Transform parent, string name, string label,
        Color fillColor, Sprite bg, Sprite fill, Sprite handle, Vector2 anchoredPos)
    {
        // Label
        var labelGo = new GameObject(name + "_Label");
        labelGo.transform.SetParent(parent, false);
        var lt = labelGo.AddComponent<Text>();
        lt.text = label;
        lt.fontSize = 32;
        lt.color = fillColor;
        lt.alignment = TextAnchor.MiddleLeft;
        lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        SetRT(labelGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(anchoredPos.x - 270f, anchoredPos.y), new Vector2(50f, 60f));

        // Slider root
        var sliderGo = new GameObject(name);
        sliderGo.transform.SetParent(parent, false);
        SetRT(sliderGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(anchoredPos.x + 30f, anchoredPos.y), new Vector2(460f, 60f));

        var slider = sliderGo.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        // Background
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(sliderGo.transform, false);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = bg;
        bgImg.color = new Color(0.25f, 0.25f, 0.30f);
        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.25f);
        bgRt.anchorMax = new Vector2(1f, 0.75f);
        bgRt.sizeDelta = Vector2.zero;

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGo.transform, false);
        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.offsetMin = new Vector2(5f, 0f);
        fillAreaRt.offsetMax = new Vector2(-15f, 0f);

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillArea.transform, false);
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.sprite = fill;
        fillImg.color = fillColor;
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.sizeDelta = new Vector2(10f, 0f);

        // Handle Slide Area
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGo.transform, false);
        var handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.sizeDelta = new Vector2(-20f, 0f);
        handleAreaRt.anchoredPosition = Vector2.zero;

        var handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(handleArea.transform, false);
        var handleImg = handleGo.AddComponent<Image>();
        handleImg.sprite = handle;
        handleImg.color = Color.white;
        var handleRt = handleGo.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(30f, 50f);
        handleRt.anchorMin = new Vector2(0f, 0.5f);
        handleRt.anchorMax = new Vector2(0f, 0.5f);
        handleRt.pivot = new Vector2(0.5f, 0.5f);

        // Wire slider
        slider.fillRect  = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        return sliderGo;
    }

    // ── Texture generators ────────────────────────────────────────────

    private static Sprite GetOrCreateSprite(string path, Texture2D tex)
    {
        if (!File.Exists(path))
        {
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                AssetDatabase.ImportAsset(path);
            }
        }
        Object.DestroyImmediate(tex);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    /// <summary>カラープレビュー: 角丸四角形</summary>
    private static Texture2D MakeColorBlockTex()
    {
        int sz = 128;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        Color fill = Color.white;
        Color border = new Color(0.3f, 0.3f, 0.3f, 1f);
        int bw = 4;

        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                bool isBorder = (x < bw || x >= sz - bw || y < bw || y >= sz - bw);
                px[y * sz + x] = isBorder ? border : fill;
            }

        ApplyRoundedCorners(px, sz, 12);
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    /// <summary>スライダー背景: 丸い角の帯</summary>
    private static Texture2D MakeSliderBgTex()
    {
        int w = 64, h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color fill = new Color(0.3f, 0.3f, 0.35f, 1f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, fill);
        tex.Apply();
        return tex;
    }

    /// <summary>スライダーフィル: 白い帯</summary>
    private static Texture2D MakeSliderFillTex()
    {
        int w = 64, h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color fill = Color.white;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, fill);
        tex.Apply();
        return tex;
    }

    /// <summary>スライダーハンドル: 白い丸</summary>
    private static Texture2D MakeSliderHandleTex()
    {
        int sz = 32;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        float cx = sz / 2f, cy = sz / 2f, r = sz * 0.44f, rb = sz * 0.50f;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                float dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= r) px[y * sz + x] = Color.white;
                else if (d <= rb) px[y * sz + x] = new Color(0.6f, 0.6f, 0.6f, 1f);
                else px[y * sz + x] = Color.clear;
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    /// <summary>パネル背景</summary>
    private static Texture2D MakePanelBgTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        Color fill = new Color(0.15f, 0.15f, 0.20f, 0.95f);
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
                tex.SetPixel(x, y, fill);
        tex.Apply();
        return tex;
    }

    private static void ApplyRoundedCorners(Color[] px, int sz, int r)
    {
        for (int i = 0; i < r; i++)
            for (int j = 0; j < r - i; j++)
            {
                if (i < sz && j < sz)                         px[i * sz + j]                         = Color.clear;
                if (i < sz && (sz - 1 - j) >= 0)             px[i * sz + (sz - 1 - j)]              = Color.clear;
                if ((sz - 1 - i) >= 0 && j < sz)             px[(sz - 1 - i) * sz + j]              = Color.clear;
                if ((sz - 1 - i) >= 0 && (sz - 1 - j) >= 0) px[(sz - 1 - i) * sz + (sz - 1 - j)]  = Color.clear;
            }
    }

    // ── UI helpers ─────────────────────────────────────────────────────

    private static GameObject MakeText(Transform parent, string name, string text,
        int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = anchor;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    private static GameObject MakeButton(Transform parent, string name, string label, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bg;
        go.AddComponent<Button>();
        var lbl = new GameObject("Label");
        lbl.transform.SetParent(go.transform, false);
        var t = lbl.AddComponent<Text>();
        t.text = label;
        t.fontSize = 28;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var rt = lbl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        return go;
    }

    private static void SetRT(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 pivot,
        Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes) if (s.path == scenePath) return;
        var n = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, n, scenes.Length);
        n[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = n;
    }
}
