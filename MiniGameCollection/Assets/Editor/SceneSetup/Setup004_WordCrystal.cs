using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Game004_WordCrystal;

public class Setup004_WordCrystal
{
    [MenuItem("Assets/Setup/004 WordCrystal")]
    public static void Setup()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Exit Play Mode before running setup.");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ──────────────────────────────────────────────
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.04f, 0.14f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGo.AddComponent<AudioListener>();

        // ── Sprites ──────────────────────────────────────────────
        string spriteDir = "Assets/Resources/Sprites/Game004_WordCrystal";
        Directory.CreateDirectory(spriteDir);

        Sprite crystalHiddenSprite = GetOrCreateSprite(
            spriteDir + "/crystal_hidden.png", MakeCrystalHiddenTexture());
        Sprite crystalRevealedSprite = GetOrCreateSprite(
            spriteDir + "/crystal_revealed.png", MakeCrystalRevealedTexture());

        // ── GameManager (WordCrystalGameManager + CrystalManager) ─
        var gmGo = new GameObject("GameManager");
        var gameManager = gmGo.AddComponent<WordCrystalGameManager>();
        var crystalManager = gmGo.AddComponent<CrystalManager>();

        // ── Crystals (4 × 2 grid) ────────────────────────────────
        float[] xs = { -2.25f, -0.75f, 0.75f, 2.25f };
        float[] ys = { 1.5f, 0.0f };
        var crystalViews = new List<CrystalView>();

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                var cGo = new GameObject($"Crystal_{row * 4 + col}");
                cGo.transform.position = new Vector3(xs[col], ys[row], 0f);

                var sr = cGo.AddComponent<SpriteRenderer>();
                sr.sprite = crystalHiddenSprite;
                sr.sortingOrder = 0;

                var col2d = cGo.AddComponent<BoxCollider2D>();
                col2d.size = new Vector2(1.2f, 1.2f);

                var cv = cGo.AddComponent<CrystalView>();

                // Child TextMesh for letter display
                var txtGo = new GameObject("Letter");
                txtGo.transform.SetParent(cGo.transform);
                txtGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                var tm = txtGo.AddComponent<TextMesh>();
                tm.text = "";
                tm.fontSize = 60;
                tm.color = Color.white;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.fontStyle = FontStyle.Bold;

                // Wire sprite refs
                var cvSo = new SerializedObject(cv);
                cvSo.FindProperty("_crystalSprite").objectReferenceValue = crystalHiddenSprite;
                cvSo.FindProperty("_revealedSprite").objectReferenceValue = crystalRevealedSprite;
                cvSo.ApplyModifiedPropertiesWithoutUndo();

                crystalViews.Add(cv);
            }
        }

        // ── Canvas (Screen Space Overlay) ────────────────────────
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Timer text (top center)
        var timerGo = MakeText(canvasGo.transform, "TimerText", "Time: 60", 36, Color.white);
        SetAnchor(timerGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -40f), new Vector2(400f, 60f));

        // Score text (top right)
        var scoreGo = MakeText(canvasGo.transform, "ScoreText", "Score: 0", 30, Color.yellow,
            TextAnchor.MiddleRight);
        SetAnchor(scoreGo, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-10f, -40f), new Vector2(280f, 60f));

        // Current word display (above buttons)
        var wordGo = MakeText(canvasGo.transform, "CurrentWordText", "", 46, Color.cyan);
        SetAnchor(wordGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 290f), new Vector2(600f, 80f));

        // Feedback text (center)
        var feedbackGo = MakeText(canvasGo.transform, "FeedbackText", "Not a word!", 28, Color.red);
        SetAnchor(feedbackGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 60f));
        feedbackGo.SetActive(false);

        // Submit button
        var submitBtn = MakeButton(canvasGo.transform, "SubmitButton", "SUBMIT",
            new Color(0.15f, 0.55f, 0.25f));
        SetAnchor(submitBtn, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(-130f, 200f), new Vector2(220f, 70f));

        // Clear button
        var clearBtn = MakeButton(canvasGo.transform, "ClearButton", "CLEAR",
            new Color(0.55f, 0.25f, 0.15f));
        SetAnchor(clearBtn, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(130f, 200f), new Vector2(220f, 70f));

        // ── Game Over Panel ───────────────────────────────────────
        var gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasGo.transform, false);
        var panelImg = gameOverPanel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.85f);
        var panelRt = gameOverPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        var gameOverTitle = MakeText(gameOverPanel.transform, "GameOverTitle", "GAME OVER", 52,
            Color.white);
        SetAnchor(gameOverTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(500f, 80f));

        var finalScoreGo = MakeText(gameOverPanel.transform, "FinalScoreText", "Score: 0", 42,
            Color.yellow);
        SetAnchor(finalScoreGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(500f, 70f));

        var retryBtn = MakeButton(gameOverPanel.transform, "RetryButton", "RETRY",
            new Color(0.2f, 0.5f, 0.85f));
        SetAnchor(retryBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(-130f, -60f), new Vector2(220f, 70f));

        var menuBtn = MakeButton(gameOverPanel.transform, "MenuButton", "MENU",
            new Color(0.4f, 0.4f, 0.4f));
        SetAnchor(menuBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(130f, -60f), new Vector2(220f, 70f));

        gameOverPanel.SetActive(false);

        // ── WordCrystalUI ────────────────────────────────────────
        var uiGo = new GameObject("WordCrystalUI");
        var ui = uiGo.AddComponent<WordCrystalUI>();

        {
            var so = new SerializedObject(ui);
            so.FindProperty("_timerText").objectReferenceValue = timerGo.GetComponent<Text>();
            so.FindProperty("_scoreText").objectReferenceValue = scoreGo.GetComponent<Text>();
            so.FindProperty("_currentWordText").objectReferenceValue = wordGo.GetComponent<Text>();
            so.FindProperty("_feedbackText").objectReferenceValue = feedbackGo.GetComponent<Text>();
            so.FindProperty("_finalScoreText").objectReferenceValue =
                finalScoreGo.GetComponent<Text>();
            so.FindProperty("_gameOverPanel").objectReferenceValue = gameOverPanel;
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.FindProperty("_crystalManager").objectReferenceValue = crystalManager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire CrystalManager
        {
            var so = new SerializedObject(crystalManager);
            so.FindProperty("_gameManager").objectReferenceValue = gameManager;
            so.FindProperty("_ui").objectReferenceValue = ui;
            var listProp = so.FindProperty("_crystals");
            listProp.arraySize = crystalViews.Count;
            for (int i = 0; i < crystalViews.Count; i++)
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = crystalViews[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire GameManager
        {
            var so = new SerializedObject(gameManager);
            so.FindProperty("_crystalManager").objectReferenceValue = crystalManager;
            so.FindProperty("_ui").objectReferenceValue = ui;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire button onClick events
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            submitBtn.GetComponent<Button>().onClick, ui.OnSubmitClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            clearBtn.GetComponent<Button>().onClick, ui.OnClearClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            retryBtn.GetComponent<Button>().onClick, ui.OnRestartClicked);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            menuBtn.GetComponent<Button>().onClick, ui.OnMenuClicked);

        // ── EventSystem ──────────────────────────────────────────
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // ── Save Scene ───────────────────────────────────────────
        string scenesDir = "Assets/Scenes";
        Directory.CreateDirectory(scenesDir);
        string scenePath = $"{scenesDir}/004_WordCrystal.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[Setup004] WordCrystal scene created at " + scenePath);
    }

    // ── Helpers ──────────────────────────────────────────────────

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
                EditorUtility.SetDirty(ti);
                AssetDatabase.ImportAsset(path);
            }
        }
        Object.DestroyImmediate(tex);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Texture2D MakeCrystalHiddenTexture()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        // Diamond/crystal shape in purple-blue gradient
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center.x) / (size * 0.45f);
                float dy = Mathf.Abs(y - center.y) / (size * 0.45f);
                float dist = dx + dy; // Manhattan diamond shape
                if (dist <= 1f)
                {
                    float t = 1f - dist;
                    Color core = Color.Lerp(new Color(0.3f, 0.1f, 0.8f), new Color(0.6f, 0.3f, 1f), t);
                    // Highlight
                    if (x > center.x - 5 && x < center.x + 5 && y > center.y + 5 && y < center.y + 15)
                        core = Color.Lerp(core, Color.white, 0.5f);
                    pixels[y * size + x] = core;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakeCrystalRevealedTexture()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        Color revealedColor = new Color(0.9f, 0.75f, 0.2f); // gold
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center.x) / (size * 0.45f);
                float dy = Mathf.Abs(y - center.y) / (size * 0.45f);
                float dist = dx + dy;
                if (dist <= 1f)
                {
                    float t = 1f - dist;
                    Color c = Color.Lerp(new Color(0.7f, 0.5f, 0.1f), revealedColor, t);
                    pixels[y * size + x] = c;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

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
        t.resizeTextForBestFit = false;
        return go;
    }

    private static GameObject MakeButton(Transform parent, string name, string label, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        go.AddComponent<Button>();

        var lblGo = new GameObject("Label");
        lblGo.transform.SetParent(go.transform, false);
        var t = lblGo.AddComponent<Text>();
        t.text = label;
        t.fontSize = 26;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var lblRt = lblGo.GetComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.sizeDelta = Vector2.zero;
        return go;
    }

    private static void SetAnchor(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, newScenes, scenes.Length);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }
}
