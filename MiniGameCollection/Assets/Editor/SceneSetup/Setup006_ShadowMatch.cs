using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Game006_ShadowMatch;

public class Setup006_ShadowMatch
{
    [MenuItem("Assets/Setup/006 ShadowMatch")]
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
        cam.backgroundColor = new Color(0.07f, 0.09f, 0.14f);
        cam.orthographic = true;
        cam.orthographicSize = 4f;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // ── Sprites ─────────────────────────────────────────────────
        string sprDir = "Assets/Resources/Sprites/Game006_ShadowMatch";
        Directory.CreateDirectory(sprDir);

        Sprite sprObject = GetOrCreateSprite(sprDir + "/object_3d.png",      MakeObjectTex());
        Sprite sprShadow = GetOrCreateSprite(sprDir + "/shadow.png",         MakeShadowTex());
        Sprite sprTarget = GetOrCreateSprite(sprDir + "/target_outline.png", MakeTargetOutlineTex());
        Sprite sprWallBg = GetOrCreateSprite(sprDir + "/wall_bg.png",        MakeWallBgTex());

        // ── Object display (left side) ───────────────────────────────
        var objDisplayGo = new GameObject("ObjectDisplay");
        objDisplayGo.transform.position = new Vector3(-1.6f, 0.8f, 0f);

        var objSprGo = new GameObject("ObjectSprite");
        objSprGo.transform.SetParent(objDisplayGo.transform);
        objSprGo.transform.localPosition = Vector3.zero;
        objSprGo.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        var objSr = objSprGo.AddComponent<SpriteRenderer>();
        objSr.sprite = sprObject;
        objSr.sortingOrder = 1;

        // ── Shadow area (right side) ─────────────────────────────────
        var shadowAreaGo = new GameObject("ShadowArea");
        shadowAreaGo.transform.position = new Vector3(1.6f, 0.8f, 0f);

        var wallBgGo = new GameObject("WallBg");
        wallBgGo.transform.SetParent(shadowAreaGo.transform);
        wallBgGo.transform.localPosition = Vector3.zero;
        wallBgGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f);
        var wallSr = wallBgGo.AddComponent<SpriteRenderer>();
        wallSr.sprite = sprWallBg;
        wallSr.sortingOrder = 0;

        var targetGo = new GameObject("TargetSprite");
        targetGo.transform.SetParent(shadowAreaGo.transform);
        targetGo.transform.localPosition = Vector3.zero;
        targetGo.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        var targetSr = targetGo.AddComponent<SpriteRenderer>();
        targetSr.sprite = sprTarget;
        targetSr.sortingOrder = 1;

        var shadowGo = new GameObject("ShadowSprite");
        shadowGo.transform.SetParent(shadowAreaGo.transform);
        shadowGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        shadowGo.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        var shadowSr = shadowGo.AddComponent<SpriteRenderer>();
        shadowSr.sprite = sprShadow;
        shadowSr.sortingOrder = 2;

        // ── GameManager ──────────────────────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gameManager = gmGo.AddComponent<ShadowMatchGameManager>();
        var rotController = gmGo.AddComponent<RotationController>();

        // ── Canvas ───────────────────────────────────────────────────
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Level text (top center)
        var levelGo = MakeText(canvasGo.transform, "LevelText", "Level 1 / 3", 34, Color.white);
        SetRT(levelGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -40f), new Vector2(400f, 60f));

        // Left label
        var leftLabel = MakeText(canvasGo.transform, "LabelObject", "DRAG TO ROTATE", 22,
            new Color(0.65f, 0.65f, 0.65f));
        SetRT(leftLabel, new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -230f), new Vector2(280f, 40f));

        // Right label
        var rightLabel = MakeText(canvasGo.transform, "LabelTarget", "MATCH THIS", 22,
            new Color(1f, 0.85f, 0.2f));
        SetRT(rightLabel, new Vector2(0.75f, 0.5f), new Vector2(0.75f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -230f), new Vector2(280f, 40f));

        // Divider line (vertical)
        var divGo = new GameObject("Divider");
        divGo.transform.SetParent(canvasGo.transform, false);
        var divImg = divGo.AddComponent<Image>();
        divImg.color = new Color(1f, 1f, 1f, 0.15f);
        SetRT(divGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(2f, 500f));

        // Match bar background
        var matchBgGo = new GameObject("MatchBarBg");
        matchBgGo.transform.SetParent(canvasGo.transform, false);
        matchBgGo.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
        SetRT(matchBgGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 270f), new Vector2(500f, 36f));

        // Match bar fill
        var matchFillGo = new GameObject("MatchBarFill");
        matchFillGo.transform.SetParent(matchBgGo.transform, false);
        var matchFillImg = matchFillGo.AddComponent<Image>();
        matchFillImg.color = new Color(0.3f, 0.9f, 0.45f);
        matchFillImg.type = Image.Type.Filled;
        matchFillImg.fillMethod = Image.FillMethod.Horizontal;
        matchFillImg.fillAmount = 0f;
        var fillRt = matchFillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;

        // Match % text
        var matchTextGo = MakeText(canvasGo.transform, "MatchText", "0%", 30, Color.white);
        SetRT(matchTextGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 320f), new Vector2(200f, 40f));

        // "MATCH" label for bar
        var matchLabelGo = MakeText(canvasGo.transform, "MatchLabel", "MATCH", 22,
            new Color(0.6f, 0.6f, 0.6f));
        SetRT(matchLabelGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(-220f, 270f), new Vector2(100f, 36f));

        // ── Clear Panel ──────────────────────────────────────────────
        var clearPanel = new GameObject("ClearPanel");
        clearPanel.transform.SetParent(canvasGo.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        var clearPanelRt = clearPanel.GetComponent<RectTransform>();
        clearPanelRt.anchorMin = Vector2.zero;
        clearPanelRt.anchorMax = Vector2.one;
        clearPanelRt.sizeDelta = Vector2.zero;

        var clearTitle = MakeText(clearPanel.transform, "ClearTitle", "PERFECT!", 60,
            new Color(1f, 0.85f, 0.2f));
        SetRT(clearTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(500f, 80f));

        var clearSub = MakeText(clearPanel.transform, "ClearSub", "Shadow matched!", 30,
            new Color(0.75f, 0.75f, 0.75f));
        SetRT(clearSub, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(500f, 50f));

        var nextBtn = MakeButton(clearPanel.transform, "NextButton", "NEXT LEVEL",
            new Color(0.15f, 0.55f, 0.8f));
        SetRT(nextBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(-130f, -60f), new Vector2(220f, 70f));

        var menuBtn = MakeButton(clearPanel.transform, "MenuButton", "MENU",
            new Color(0.35f, 0.35f, 0.35f));
        SetRT(menuBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(130f, -60f), new Vector2(220f, 70f));

        clearPanel.SetActive(false);

        // ── ShadowMatchUI ────────────────────────────────────────────
        var uiGo = new GameObject("ShadowMatchUI");
        var ui = uiGo.AddComponent<ShadowMatchUI>();

        {
            var so = new SerializedObject(ui);
            so.FindProperty("_levelText").objectReferenceValue = levelGo.GetComponent<Text>();
            so.FindProperty("_matchFill").objectReferenceValue = matchFillImg;
            so.FindProperty("_matchText").objectReferenceValue = matchTextGo.GetComponent<Text>();
            so.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire RotationController
        {
            var so = new SerializedObject(rotController);
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.FindProperty("_ui").objectReferenceValue = ui;
            so.FindProperty("_objectTransform").objectReferenceValue = objSprGo.transform;
            so.FindProperty("_shadowTransform").objectReferenceValue = shadowGo.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire GameManager
        {
            var so = new SerializedObject(gameManager);
            so.FindProperty("_rotController").objectReferenceValue = rotController;
            so.FindProperty("_ui").objectReferenceValue = ui;
            so.FindProperty("_targetTransform").objectReferenceValue = targetGo.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire buttons
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            nextBtn.GetComponent<Button>().onClick, ui.OnNextLevelClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuBtn.GetComponent<Button>().onClick, ui.OnMenuClicked);

        // ── EventSystem ──────────────────────────────────────────────
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // ── Save ─────────────────────────────────────────────────────
        string scenesDir = "Assets/Scenes";
        Directory.CreateDirectory(scenesDir);
        string scenePath = $"{scenesDir}/006_ShadowMatch.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[Setup006] ShadowMatch scene saved to " + scenePath);
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

    // L-shape: left bar (col 30-60, row 10-100) + bottom-right block (col 60-95, row 65-100)
    private static void DrawLShape(Color[] px, int sz, Color fill)
    {
        // Vertical bar
        for (int y = 10; y < 100; y++)
            for (int x = 30; x < 62; x++)
                if (x < sz && y < sz) px[y * sz + x] = fill;
        // Horizontal bottom
        for (int y = 65; y < 100; y++)
            for (int x = 62; x < 96; x++)
                if (x < sz && y < sz) px[y * sz + x] = fill;
    }

    // object_3d.png – isometric-looking blue L-shape
    private static Texture2D MakeObjectTex()
    {
        int sz = 128;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

        Color main  = new Color(0.25f, 0.55f, 0.95f, 1f);
        Color light = new Color(0.5f,  0.75f, 1.0f,  1f);
        Color dark  = new Color(0.1f,  0.3f,  0.65f, 1f);

        DrawLShape(px, sz, main);

        // Top highlight (top 3 rows of each bar)
        for (int y = 10; y < 13; y++)
            for (int x = 30; x < 62; x++)
                if (x < sz && y < sz && px[y * sz + x].a > 0) px[y * sz + x] = light;
        for (int y = 65; y < 68; y++)
            for (int x = 62; x < 96; x++)
                if (x < sz && y < sz && px[y * sz + x].a > 0) px[y * sz + x] = light;

        // Left edge highlight
        for (int y = 10; y < 100; y++)
            for (int x = 30; x < 33; x++)
                if (x < sz && y < sz && px[y * sz + x].a > 0) px[y * sz + x] = light;

        // Right/bottom shadow
        for (int y = 10; y < 100; y++)
            for (int x = 59; x < 62; x++)
                if (x < sz && y < sz && px[y * sz + x].a > 0) px[y * sz + x] = dark;
        for (int y = 97; y < 100; y++)
            for (int x = 30; x < 96; x++)
                if (x < sz && y < sz && px[y * sz + x].a > 0) px[y * sz + x] = dark;

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // shadow.png – dark gray L-shape silhouette
    private static Texture2D MakeShadowTex()
    {
        int sz = 128;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        DrawLShape(px, sz, new Color(0.12f, 0.12f, 0.15f, 0.92f));
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // target_outline.png – yellow outline of the L-shape (4px border, transparent fill)
    private static Texture2D MakeTargetOutlineTex()
    {
        int sz = 128;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

        Color outline = new Color(1f, 0.82f, 0.1f, 0.9f);
        // Draw 4px-wide outer L, then cut interior to create outline
        // Expand each side by 4px
        for (int y = 6; y < 104; y++)
            for (int x = 26; x < 66; x++)
                if (x < sz && y < sz) px[y * sz + x] = outline;
        for (int y = 61; y < 104; y++)
            for (int x = 58; x < 100; x++)
                if (x < sz && y < sz) px[y * sz + x] = outline;

        // Clear interior (shrink by 4px = original shape)
        for (int y = 10; y < 100; y++)
            for (int x = 30; x < 62; x++)
                if (x < sz && y < sz) px[y * sz + x] = Color.clear;
        for (int y = 65; y < 100; y++)
            for (int x = 62; x < 96; x++)
                if (x < sz && y < sz) px[y * sz + x] = Color.clear;

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // wall_bg.png – soft light gray background for shadow wall
    private static Texture2D MakeWallBgTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        Color c = new Color(0.88f, 0.88f, 0.92f, 1f);
        for (int i = 0; i < px.Length; i++) px[i] = c;
        // Subtle vignette
        Vector2 center = new Vector2(sz / 2f, sz / 2f);
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / (sz * 0.7f);
                float v = Mathf.Clamp01(1f - d * 0.25f);
                px[y * sz + x] = Color.Lerp(new Color(0.78f, 0.78f, 0.83f), c, v);
            }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
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
        t.fontSize = 24;
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
