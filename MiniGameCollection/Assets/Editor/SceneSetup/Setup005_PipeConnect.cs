using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Game005_PipeConnect;

public class Setup005_PipeConnect
{
    [MenuItem("Assets/Setup/005 PipeConnect")]
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
        cam.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
        cam.orthographic = true;
        cam.orthographicSize = 4f;
        camGo.AddComponent<AudioListener>();

        // ── Sprites ─────────────────────────────────────────────────
        string spriteDir = "Assets/Resources/Sprites/Game005_PipeConnect";
        Directory.CreateDirectory(spriteDir);

        Sprite sprEmpty    = GetOrCreateSprite(spriteDir + "/pipe_empty.png",    MakeEmptyTex());
        Sprite sprStraight = GetOrCreateSprite(spriteDir + "/pipe_straight.png", MakeStraightTex());
        Sprite sprBend     = GetOrCreateSprite(spriteDir + "/pipe_bend.png",     MakeBendTex());
        Sprite sprT        = GetOrCreateSprite(spriteDir + "/pipe_t.png",        MakeTTex());
        Sprite sprCross    = GetOrCreateSprite(spriteDir + "/pipe_cross.png",    MakeCrossTex());
        Sprite sprSource   = GetOrCreateSprite(spriteDir + "/pipe_source.png",   MakeSourceTex());
        Sprite sprGoal     = GetOrCreateSprite(spriteDir + "/pipe_goal.png",     MakeGoalTex());

        // ── GameManager ─────────────────────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gameManager = gmGo.AddComponent<PipeConnectGameManager>();
        var pipeManager = gmGo.AddComponent<PipeManager>();

        // ── 5×5 Grid ────────────────────────────────────────────────
        // tile(row, col) at world pos (col-2, 2-row, 0)
        var tiles = new PipeTile[25];
        var tileGo = new GameObject("TileGrid");
        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                var go = new GameObject($"Tile_{r}_{c}");
                go.transform.SetParent(tileGo.transform);
                go.transform.position = new Vector3(c - 2f, 2f - r, 0f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprEmpty;
                sr.sortingOrder = 0;

                var col2d = go.AddComponent<BoxCollider2D>();
                col2d.size = new Vector2(0.95f, 0.95f);

                tiles[r * 5 + c] = go.AddComponent<PipeTile>();
            }
        }

        // ── Canvas ──────────────────────────────────────────────────
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Level text (top-left)
        var levelGo = MakeText(canvasGo.transform, "LevelText", "Level 1 / 3", 32, Color.white,
            TextAnchor.MiddleLeft);
        SetRT(levelGo, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, -40f), new Vector2(300f, 60f));

        // Move count (top-right)
        var moveGo = MakeText(canvasGo.transform, "MoveText", "Moves: 0", 32, Color.white,
            TextAnchor.MiddleRight);
        SetRT(moveGo, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-10f, -40f), new Vector2(260f, 60f));

        // ── Clear Panel ─────────────────────────────────────────────
        var clearPanel = new GameObject("ClearPanel");
        clearPanel.transform.SetParent(canvasGo.transform, false);
        var panelImg = clearPanel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.88f);
        var panelRt = clearPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        var clearTitle = MakeText(clearPanel.transform, "ClearTitle", "CLEAR!", 58,
            new Color(0.4f, 1f, 0.5f));
        SetRT(clearTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(500f, 80f));

        var clearResult = MakeText(clearPanel.transform, "ClearResultText",
            "Cleared in 0 moves!", 34, Color.white);
        SetRT(clearResult, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(500f, 60f));

        var nextBtn = MakeButton(clearPanel.transform, "NextLevelButton", "NEXT",
            new Color(0.15f, 0.55f, 0.8f));
        SetRT(nextBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(-130f, -60f), new Vector2(220f, 70f));

        var menuBtn = MakeButton(clearPanel.transform, "MenuButton", "MENU",
            new Color(0.4f, 0.4f, 0.4f));
        SetRT(menuBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(130f, -60f), new Vector2(220f, 70f));

        clearPanel.SetActive(false);

        // ── PipeConnectUI ────────────────────────────────────────────
        var uiGo = new GameObject("PipeConnectUI");
        var ui = uiGo.AddComponent<PipeConnectUI>();

        {
            var so = new SerializedObject(ui);
            so.FindProperty("_levelText").objectReferenceValue = levelGo.GetComponent<Text>();
            so.FindProperty("_moveText").objectReferenceValue = moveGo.GetComponent<Text>();
            so.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
            so.FindProperty("_clearResultText").objectReferenceValue =
                clearResult.GetComponent<Text>();
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire PipeManager
        {
            var so = new SerializedObject(pipeManager);
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            var listProp = so.FindProperty("_tiles");
            listProp.arraySize = 25;
            for (int i = 0; i < 25; i++)
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = tiles[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire GameManager
        {
            var so = new SerializedObject(gameManager);
            so.FindProperty("_pipeManager").objectReferenceValue = pipeManager;
            so.FindProperty("_ui").objectReferenceValue = ui;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire OnSolved → gameManager.OnSolved
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            pipeManager.OnSolved, gameManager.OnSolved);

        // Wire buttons
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            nextBtn.GetComponent<Button>().onClick, ui.OnNextLevelClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuBtn.GetComponent<Button>().onClick, ui.OnMenuClicked);

        // ── EventSystem ─────────────────────────────────────────────
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // ── Save ────────────────────────────────────────────────────
        string scenesDir = "Assets/Scenes";
        Directory.CreateDirectory(scenesDir);
        string scenePath = $"{scenesDir}/005_PipeConnect.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[Setup005] PipeConnect scene saved to " + scenePath);
    }

    // ── Sprite generators ────────────────────────────────────────────

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

    private static Color TileBg => new Color(0.18f, 0.22f, 0.3f);
    private static Color PipeColor => new Color(0.35f, 0.7f, 1f);
    private static Color PipeDark => new Color(0.15f, 0.45f, 0.75f);

    private static void FillBg(Color[] px, int sz)
    {
        Color bg = TileBg;
        for (int i = 0; i < px.Length; i++) px[i] = bg;
    }

    private static void DrawRect(Color[] px, int sz, int x0, int y0, int w, int h, Color c)
    {
        for (int y = y0; y < y0 + h; y++)
            for (int x = x0; x < x0 + w; x++)
                if (x >= 0 && x < sz && y >= 0 && y < sz) px[y * sz + x] = c;
    }

    private static Texture2D MakeTex(int sz = 64)
    {
        var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        FillBg(px, sz);
        t.SetPixels(px);
        t.Apply();
        return t;
    }

    // pipe_empty: just dark tile
    private static Texture2D MakeEmptyTex()
    {
        int sz = 64;
        var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        FillBg(px, sz);
        t.SetPixels(px); t.Apply(); return t;
    }

    // pipe_straight: vertical channel (rotation 0 = U+D)
    private static Texture2D MakeStraightTex()
    {
        int sz = 64;
        var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        FillBg(px, sz);
        DrawRect(px, sz, 25, 0, 14, 64, PipeDark);   // channel walls
        DrawRect(px, sz, 27, 0, 10, 64, PipeColor);   // channel fill
        t.SetPixels(px); t.Apply(); return t;
    }

    // pipe_bend: U+R corner (rotation 0)
    private static Texture2D MakeBendTex()
    {
        int sz = 64;
        var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        FillBg(px, sz);
        // Vertical arm (top to center)
        DrawRect(px, sz, 25, 32, 14, 32, PipeDark);
        DrawRect(px, sz, 27, 32, 10, 32, PipeColor);
        // Horizontal arm (center to right)
        DrawRect(px, sz, 32, 25, 32, 14, PipeDark);
        DrawRect(px, sz, 32, 27, 32, 10, PipeColor);
        // Corner fill
        DrawRect(px, sz, 27, 27, 15, 15, PipeColor);
        t.SetPixels(px); t.Apply(); return t;
    }

    // pipe_t: U+R+D (missing L)
    private static Texture2D MakeTTex()
    {
        int sz = 64;
        var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        FillBg(px, sz);
        // Vertical full
        DrawRect(px, sz, 25, 0, 14, 64, PipeDark);
        DrawRect(px, sz, 27, 0, 10, 64, PipeColor);
        // Horizontal right half
        DrawRect(px, sz, 32, 25, 32, 14, PipeDark);
        DrawRect(px, sz, 32, 27, 32, 10, PipeColor);
        DrawRect(px, sz, 27, 27, 15, 10, PipeColor);
        t.SetPixels(px); t.Apply(); return t;
    }

    // pipe_cross: all four
    private static Texture2D MakeCrossTex()
    {
        int sz = 64;
        var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        FillBg(px, sz);
        DrawRect(px, sz, 25, 0, 14, 64, PipeDark);
        DrawRect(px, sz, 27, 0, 10, 64, PipeColor);
        DrawRect(px, sz, 0, 25, 64, 14, PipeDark);
        DrawRect(px, sz, 0, 27, 64, 10, PipeColor);
        t.SetPixels(px); t.Apply(); return t;
    }

    // pipe_source: green tile with arrow pointing right
    private static Texture2D MakeSourceTex()
    {
        int sz = 64;
        var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        Color src = new Color(0.15f, 0.75f, 0.35f);
        Color arrow = Color.white;
        for (int i = 0; i < px.Length; i++) px[i] = src;
        // Arrow pointing right: horizontal bar + triangle tip
        DrawRect(px, sz, 12, 27, 30, 10, arrow); // stem
        // Triangle tip
        for (int y = 20; y <= 44; y++)
        {
            int tip = 42 + (y < 32 ? y - 20 : 44 - y);
            for (int x = 42; x <= tip; x++)
                if (x < sz && y < sz) px[y * sz + x] = arrow;
        }
        // Horizontal channel on right half
        DrawRect(px, sz, 32, 27, 32, 10, PipeColor);
        t.SetPixels(px); t.Apply(); return t;
    }

    // pipe_goal: gold tile with star-like opening on left
    private static Texture2D MakeGoalTex()
    {
        int sz = 64;
        var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];
        Color goal = new Color(0.9f, 0.72f, 0.1f);
        Color star = Color.white;
        for (int i = 0; i < px.Length; i++) px[i] = goal;
        // Star (simple 4-point cross)
        DrawRect(px, sz, 0, 27, 32, 10, PipeColor);  // left opening
        DrawRect(px, sz, 24, 20, 16, 24, star);       // vertical bar
        DrawRect(px, sz, 20, 24, 24, 16, star);       // horizontal bar
        t.SetPixels(px); t.Apply(); return t;
    }

    // ── UI helpers ────────────────────────────────────────────────────

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
        t.fontSize = 26;
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
