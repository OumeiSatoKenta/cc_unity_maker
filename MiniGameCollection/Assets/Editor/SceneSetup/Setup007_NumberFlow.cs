using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Game007_NumberFlow;

public class Setup007_NumberFlow
{
    [MenuItem("Assets/Setup/007 NumberFlow")]
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
        cam.backgroundColor = new Color(0.10f, 0.12f, 0.18f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // ── Sprites ─────────────────────────────────────────────────
        string sprDir = "Assets/Resources/Sprites/Game007_NumberFlow";
        Directory.CreateDirectory(sprDir);

        Sprite sprCell     = GetOrCreateSprite(sprDir + "/cell_normal.png",   MakeCellTex(new Color(0.28f, 0.30f, 0.36f)));
        Sprite sprVisited  = GetOrCreateSprite(sprDir + "/cell_visited.png",  MakeCellTex(new Color(0.20f, 0.50f, 0.88f)));
        Sprite sprCurrent  = GetOrCreateSprite(sprDir + "/cell_current.png",  MakeCellTex(new Color(0.15f, 0.90f, 0.68f)));
        Sprite sprStart    = GetOrCreateSprite(sprDir + "/cell_start.png",    MakeCellTex(new Color(0.22f, 0.82f, 0.32f)));
        Sprite sprEnd      = GetOrCreateSprite(sprDir + "/cell_end.png",      MakeCellTex(new Color(0.92f, 0.76f, 0.12f)));

        // ── Grid container ───────────────────────────────────────────
        const int GridSize = 4;
        const float CellSize = 1.8f;
        const float CellSpacing = 0.1f;
        float step = CellSize + CellSpacing;
        float offset = (GridSize - 1) * step * 0.5f;

        // Cell prefab sprite path
        string cellPrefabPath = "Assets/Resources/Sprites/Game007_NumberFlow/cell_normal.png";
        var cellPrefab = CreateCellPrefab(cellPrefabPath, sprCell);

        // Level data (same as NumberFlowGameManager)
        int[][,] levels = new int[][,]
        {
            new int[4, 4]
            {
                {  1,  2,  3,  4 },
                { 12, 13, 14,  5 },
                { 11, 16, 15,  6 },
                { 10,  9,  8,  7 },
            },
        };

        // ── GameManager ──────────────────────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gameManager = gmGo.AddComponent<NumberFlowGameManager>();
        var flowManager = gmGo.AddComponent<NumberFlowManager>();

        // Build cells
        var cells = new NumberCell[GridSize * GridSize];
        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                float x = c * step - offset;
                float y = -(r * step - offset);  // top row = high y

                var cellGo = new GameObject($"Cell_{r}_{c}");
                cellGo.transform.position = new Vector3(x, y - 0.5f, 0f);
                cellGo.transform.localScale = new Vector3(CellSize, CellSize, 1f);

                var sr = cellGo.AddComponent<SpriteRenderer>();
                sr.sprite = sprCell;
                sr.sortingOrder = 1;

                var col2d = cellGo.AddComponent<BoxCollider2D>();
                col2d.size = Vector2.one;

                var nc = cellGo.AddComponent<NumberCell>();

                // TextMesh child for number
                var textGo = new GameObject("NumberText");
                textGo.transform.SetParent(cellGo.transform);
                textGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                textGo.transform.localScale = new Vector3(1f / CellSize, 1f / CellSize, 1f);
                var tm = textGo.AddComponent<TextMesh>();
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.fontSize = 42;
                tm.color = Color.white;
                tm.text = levels[0][r, c].ToString();

                var mr = textGo.GetComponent<MeshRenderer>();
                if (mr != null) mr.sortingOrder = 2;

                cells[r * GridSize + c] = nc;
            }
        }

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

        // Step progress text (below level)
        var stepTextGo = MakeText(canvasGo.transform, "StepText", "0 / 16", 28,
            new Color(0.7f, 0.85f, 1f));
        SetRT(stepTextGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -105f), new Vector2(300f, 50f));

        // Menu button (top right)
        var menuHeaderBtn = MakeButton(canvasGo.transform, "MenuHeaderButton", "MENU",
            new Color(0.25f, 0.25f, 0.30f));
        SetRT(menuHeaderBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -20f), new Vector2(160f, 60f));

        // ── Clear Panel ──────────────────────────────────────────────
        var clearPanel = new GameObject("ClearPanel");
        clearPanel.transform.SetParent(canvasGo.transform, false);
        clearPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        var clearPanelRt = clearPanel.GetComponent<RectTransform>();
        clearPanelRt.anchorMin = Vector2.zero;
        clearPanelRt.anchorMax = Vector2.one;
        clearPanelRt.sizeDelta = Vector2.zero;

        var clearTitle = MakeText(clearPanel.transform, "ClearTitle", "COMPLETE!", 60,
            new Color(0.15f, 0.90f, 0.68f));
        SetRT(clearTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(500f, 80f));

        var clearSub = MakeText(clearPanel.transform, "ClearSub", "All cells filled!", 30,
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

        // ── NumberFlowUI ─────────────────────────────────────────────
        var uiGo = new GameObject("NumberFlowUI");
        var ui = uiGo.AddComponent<NumberFlowUI>();

        {
            var so = new SerializedObject(ui);
            so.FindProperty("_levelText").objectReferenceValue = levelTextGo.GetComponent<Text>();
            so.FindProperty("_stepText").objectReferenceValue = stepTextGo.GetComponent<Text>();
            so.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wire NumberFlowManager ────────────────────────────────────
        {
            var so = new SerializedObject(flowManager);
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.FindProperty("_ui").objectReferenceValue = ui;
            // Serialize cells array
            var cellsProp = so.FindProperty("_cells");
            cellsProp.arraySize = cells.Length;
            for (int i = 0; i < cells.Length; i++)
                cellsProp.GetArrayElementAtIndex(i).objectReferenceValue = cells[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Wire GameManager ──────────────────────────────────────────
        {
            var so = new SerializedObject(gameManager);
            so.FindProperty("_flowManager").objectReferenceValue = flowManager;
            so.FindProperty("_ui").objectReferenceValue = ui;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire OnCleared event in NumberFlowManager -> GameManager.OnCleared
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            flowManager.OnCleared, gameManager.OnCleared);

        // OnLevelCleared (UnityEvent<int>) -> wired via GameManager's OnCleared calls ui directly at runtime

        // Wire buttons
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            restartBtn.GetComponent<Button>().onClick, gameManager.ResetLevel);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            nextBtn.GetComponent<Button>().onClick, gameManager.LoadNextLevel);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuBtn.GetComponent<Button>().onClick, gameManager.LoadMenu);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuHeaderBtn.GetComponent<Button>().onClick, gameManager.LoadMenu);

        // ── EventSystem ──────────────────────────────────────────────
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // ── Save ─────────────────────────────────────────────────────
        string scenesDir = "Assets/Scenes";
        Directory.CreateDirectory(scenesDir);
        string scenePath = $"{scenesDir}/007_NumberFlow.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[Setup007] NumberFlow scene saved to " + scenePath);
    }

    // ── Cell Prefab ───────────────────────────────────────────────────

    private static GameObject CreateCellPrefab(string texPath, Sprite sprite)
    {
        // Prefab is only used as template reference; actual cells are instantiated in Setup
        return null; // Not needed for direct GameObject creation approach
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

    private static Texture2D MakeCellTex(Color fillColor)
    {
        int sz = 64;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        var px = new Color[sz * sz];

        Color border = new Color(fillColor.r * 0.6f, fillColor.g * 0.6f, fillColor.b * 0.6f, 1f);
        int borderW = 3;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                bool isBorder = (x < borderW || x >= sz - borderW ||
                                 y < borderW || y >= sz - borderW);
                px[y * sz + x] = isBorder ? border : fillColor;
            }
        }

        // Rounded corners: clear the very corners
        for (int r = 0; r < 5; r++)
        {
            for (int cr = 0; cr < 5 - r; cr++)
            {
                // top-left
                if (r < sz && cr < sz) px[r * sz + cr] = Color.clear;
                // top-right
                if (r < sz && (sz - 1 - cr) >= 0) px[r * sz + (sz - 1 - cr)] = Color.clear;
                // bottom-left
                if ((sz - 1 - r) >= 0 && cr < sz) px[(sz - 1 - r) * sz + cr] = Color.clear;
                // bottom-right
                if ((sz - 1 - r) >= 0 && (sz - 1 - cr) >= 0) px[(sz - 1 - r) * sz + (sz - 1 - cr)] = Color.clear;
            }
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
