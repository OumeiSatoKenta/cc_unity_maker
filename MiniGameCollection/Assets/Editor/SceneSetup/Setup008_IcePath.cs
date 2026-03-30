using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Game008_IcePath;

public class Setup008_IcePath
{
    [MenuItem("Assets/Setup/008 IcePath")]
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
        cam.backgroundColor = new Color(0.08f, 0.14f, 0.22f);
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // ── Sprites ─────────────────────────────────────────────────
        string sprDir = "Assets/Resources/Sprites/Game008_IcePath";
        Directory.CreateDirectory(sprDir);

        Sprite sprIce     = GetOrCreateSprite(sprDir + "/ice_cell.png",     MakeIceTex());
        Sprite sprVisited = GetOrCreateSprite(sprDir + "/visited_cell.png",  MakeVisitedTex());
        Sprite sprWall    = GetOrCreateSprite(sprDir + "/wall_cell.png",     MakeWallTex());
        Sprite sprPlayer  = GetOrCreateSprite(sprDir + "/player.png",        MakePlayerTex());

        // ── Grid ─────────────────────────────────────────────────────
        const int GridSize = 5;
        const float CellSize = 1.6f;
        const float CellSpacing = 0.08f;
        float step = CellSize + CellSpacing;
        float offset = (GridSize - 1) * step * 0.5f;

        // Layout for Level 1 (all ice, to match GameManager Level 1)
        int[,] previewLayout = new int[5, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
        };

        // ── GameManager ──────────────────────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gameManager = gmGo.AddComponent<IcePathGameManager>();
        var icePathMgr  = gmGo.AddComponent<IcePathManager>();

        // Build cells
        var cells = new IceCell[GridSize * GridSize];
        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                float x = c * step - offset;
                float y = -(r * step - offset) + 0.3f;

                var cellGo = new GameObject($"Cell_{r}_{c}");
                cellGo.transform.position = new Vector3(x, y, 0f);
                cellGo.transform.localScale = new Vector3(CellSize, CellSize, 1f);

                var sr = cellGo.AddComponent<SpriteRenderer>();
                var cType = previewLayout[r, c] == 1 ? IceCell.CellType.Wall : IceCell.CellType.Ice;
                sr.sprite = cType == IceCell.CellType.Wall ? sprWall : sprIce;
                sr.sortingOrder = 1;

                var col2d = cellGo.AddComponent<BoxCollider2D>();
                col2d.size = Vector2.one;

                var ic = cellGo.AddComponent<IceCell>();
                ic.Init(r, c, cType, sprIce, sprVisited, sprWall);

                cells[r * GridSize + c] = ic;
            }
        }

        // ── Player visual ─────────────────────────────────────────────
        var playerGo = new GameObject("Player");
        playerGo.transform.position = new Vector3(
            0 * step - offset,
            -(0 * step - offset) + 0.3f,
            -0.5f);
        playerGo.transform.localScale = new Vector3(CellSize * 0.7f, CellSize * 0.7f, 1f);
        var playerSr = playerGo.AddComponent<SpriteRenderer>();
        playerSr.sprite = sprPlayer;
        playerSr.sortingOrder = 3;

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
        var levelTextGo = MakeText(canvasGo.transform, "LevelText", "Level 1 / 3", 34, Color.white);
        SetRT(levelTextGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -45f), new Vector2(400f, 60f));

        // Progress text (below level)
        var progressTextGo = MakeText(canvasGo.transform, "ProgressText", "0 / 25", 28,
            new Color(0.6f, 0.9f, 1f));
        SetRT(progressTextGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -105f), new Vector2(300f, 50f));

        // Menu button (top right)
        var menuHeaderBtn = MakeButton(canvasGo.transform, "MenuHeaderButton", "MENU",
            new Color(0.25f, 0.25f, 0.30f));
        SetRT(menuHeaderBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -20f), new Vector2(160f, 60f));

        // ── Direction buttons (bottom area) ──────────────────────────
        // Up button
        var upBtn = MakeButton(canvasGo.transform, "UpButton", "▲",
            new Color(0.15f, 0.45f, 0.75f));
        SetRT(upBtn, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 250f), new Vector2(100f, 80f));

        // Down button
        var downBtn = MakeButton(canvasGo.transform, "DownButton", "▼",
            new Color(0.15f, 0.45f, 0.75f));
        SetRT(downBtn, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 155f), new Vector2(100f, 80f));

        // Left button
        var leftBtn = MakeButton(canvasGo.transform, "LeftButton", "◀",
            new Color(0.15f, 0.45f, 0.75f));
        SetRT(leftBtn, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-110f, 200f), new Vector2(100f, 80f));

        // Right button
        var rightBtn = MakeButton(canvasGo.transform, "RightButton", "▶",
            new Color(0.15f, 0.45f, 0.75f));
        SetRT(rightBtn, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(110f, 200f), new Vector2(100f, 80f));

        // ── Clear Panel ──────────────────────────────────────────────
        var clearPanel = new GameObject("ClearPanel");
        clearPanel.transform.SetParent(canvasGo.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        var clearPanelRt = clearPanel.GetComponent<RectTransform>();
        clearPanelRt.anchorMin = Vector2.zero;
        clearPanelRt.anchorMax = Vector2.one;
        clearPanelRt.sizeDelta = Vector2.zero;

        var clearTitle = MakeText(clearPanel.transform, "ClearTitle", "COMPLETE!", 60,
            new Color(0.3f, 0.85f, 1f));
        SetRT(clearTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(500f, 80f));

        var clearSub = MakeText(clearPanel.transform, "ClearSub", "All cells covered!", 30,
            new Color(0.75f, 0.75f, 0.75f));
        SetRT(clearSub, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(500f, 50f));

        var restartBtn = MakeButton(clearPanel.transform, "RestartButton", "RETRY",
            new Color(0.2f, 0.55f, 0.35f));
        SetRT(restartBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(-130f, -60f), new Vector2(220f, 70f));

        var nextBtn = MakeButton(clearPanel.transform, "NextButton", "NEXT LEVEL",
            new Color(0.15f, 0.45f, 0.80f));
        SetRT(nextBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(130f, -60f), new Vector2(220f, 70f));

        var menuBtn = MakeButton(clearPanel.transform, "MenuButton", "MENU",
            new Color(0.35f, 0.35f, 0.35f));
        SetRT(menuBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(200f, 60f));

        clearPanel.SetActive(false);

        // ── IcePathUI ─────────────────────────────────────────────────
        var uiGo = new GameObject("IcePathUI");
        var ui = uiGo.AddComponent<IcePathUI>();

        {
            var so = new SerializedObject(ui);
            so.FindProperty("_levelText").objectReferenceValue      = levelTextGo.GetComponent<Text>();
            so.FindProperty("_progressText").objectReferenceValue   = progressTextGo.GetComponent<Text>();
            so.FindProperty("_clearPanel").objectReferenceValue     = clearPanel;
            so.FindProperty("_gameManager").objectReferenceValue    = gameManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wire IcePathManager ────────────────────────────────────────
        {
            var so = new SerializedObject(icePathMgr);
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.FindProperty("_ui").objectReferenceValue          = ui;
            var cellsProp = so.FindProperty("_cells");
            cellsProp.arraySize = cells.Length;
            for (int i = 0; i < cells.Length; i++)
                cellsProp.GetArrayElementAtIndex(i).objectReferenceValue = cells[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Set player reference
        icePathMgr.SetPlayerGo(playerGo);

        // ── Wire GameManager ──────────────────────────────────────────
        {
            var so = new SerializedObject(gameManager);
            so.FindProperty("_icePathManager").objectReferenceValue = icePathMgr;
            so.FindProperty("_ui").objectReferenceValue             = ui;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire OnCleared event
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            icePathMgr.OnCleared, gameManager.OnCleared);

        // Wire direction buttons
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            upBtn.GetComponent<Button>().onClick,    icePathMgr.SlideUp);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            downBtn.GetComponent<Button>().onClick,  icePathMgr.SlideDown);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            leftBtn.GetComponent<Button>().onClick,  icePathMgr.SlideLeft);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            rightBtn.GetComponent<Button>().onClick, icePathMgr.SlideRight);

        // Wire clear panel buttons
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            restartBtn.GetComponent<Button>().onClick, gameManager.ResetLevel);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            nextBtn.GetComponent<Button>().onClick,    gameManager.LoadNextLevel);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuBtn.GetComponent<Button>().onClick,    gameManager.LoadMenu);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuHeaderBtn.GetComponent<Button>().onClick, gameManager.LoadMenu);

        // ── EventSystem ──────────────────────────────────────────────
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // ── Save ─────────────────────────────────────────────────────
        string scenesDir = "Assets/Scenes";
        Directory.CreateDirectory(scenesDir);
        string scenePath = $"{scenesDir}/008_IcePath.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[Setup008] IcePath scene saved to " + scenePath);
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

    /// <summary>氷マス: 薄青色の半透明タイル</summary>
    private static Texture2D MakeIceTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];

        Color fill   = new Color(0.55f, 0.82f, 0.96f, 1.0f);
        Color shine  = new Color(0.80f, 0.95f, 1.00f, 1.0f);
        Color border = new Color(0.30f, 0.55f, 0.75f, 1.0f);
        int bw = 3;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                bool isBorder = (x < bw || x >= sz - bw || y < bw || y >= sz - bw);
                // Highlight top-left region for ice shine effect
                bool isShine = (!isBorder && x < sz * 0.35f && y > sz * 0.65f);
                px[y * sz + x] = isBorder ? border : (isShine ? shine : fill);
            }
        }

        // Rounded corners
        ApplyRoundedCorners(px, sz, 6);

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    /// <summary>通過済みマス: 青紫色（足跡の色）</summary>
    private static Texture2D MakeVisitedTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];

        Color fill   = new Color(0.25f, 0.45f, 0.80f, 1.0f);
        Color border = new Color(0.15f, 0.28f, 0.55f, 1.0f);
        int bw = 3;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                bool isBorder = (x < bw || x >= sz - bw || y < bw || y >= sz - bw);
                px[y * sz + x] = isBorder ? border : fill;
            }
        }

        ApplyRoundedCorners(px, sz, 6);
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    /// <summary>壁マス: 暗いグレー（岩や障害物）</summary>
    private static Texture2D MakeWallTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];

        Color fill   = new Color(0.30f, 0.30f, 0.35f, 1.0f);
        Color border = new Color(0.18f, 0.18f, 0.20f, 1.0f);
        Color detail = new Color(0.40f, 0.40f, 0.45f, 1.0f);
        int bw = 4;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                bool isBorder = (x < bw || x >= sz - bw || y < bw || y >= sz - bw);
                // Add some texture pattern to wall
                bool isDetail = (!isBorder && ((x + y) % 16 < 3));
                px[y * sz + x] = isBorder ? border : (isDetail ? detail : fill);
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    /// <summary>プレイヤー: 丸い白いキャラ</summary>
    private static Texture2D MakePlayerTex()
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];

        float cx = sz / 2f;
        float cy = sz / 2f;
        float r  = sz * 0.42f;
        float rb = sz * 0.46f;

        Color body    = new Color(1.0f, 0.92f, 0.30f, 1.0f); // yellow
        Color outline = new Color(0.70f, 0.55f, 0.10f, 1.0f);
        Color shine   = new Color(1.0f, 1.0f, 0.80f, 1.0f);

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= r)
                {
                    // Shine in top-left
                    bool isShine = (dx < -r * 0.2f && dy > r * 0.2f && dist < r * 0.65f);
                    px[y * sz + x] = isShine ? shine : body;
                }
                else if (dist <= rb)
                    px[y * sz + x] = outline;
                else
                    px[y * sz + x] = Color.clear;
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    private static void ApplyRoundedCorners(Color[] px, int sz, int r)
    {
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < r - i; j++)
            {
                if (i < sz && j < sz)                               px[i * sz + j]             = Color.clear;
                if (i < sz && (sz - 1 - j) >= 0)                   px[i * sz + (sz - 1 - j)]  = Color.clear;
                if ((sz - 1 - i) >= 0 && j < sz)                   px[(sz - 1 - i) * sz + j]  = Color.clear;
                if ((sz - 1 - i) >= 0 && (sz - 1 - j) >= 0)       px[(sz - 1 - i) * sz + (sz - 1 - j)] = Color.clear;
            }
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
