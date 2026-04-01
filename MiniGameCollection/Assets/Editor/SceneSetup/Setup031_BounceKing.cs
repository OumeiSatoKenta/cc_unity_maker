using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;
using Game031_BounceKing;

public static class Setup031_BounceKing
{
    [MenuItem("Assets/Setup/031 BounceKing")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Setup031] Play モード中は実行できません。"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");

        string sp = "Assets/Resources/Sprites/Game031_BounceKing/";

        // Camera
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.04f, 0.06f, 0.16f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
        }

        // PhysicsMaterial2D
        string matPath = sp + "BallBouncy.physicsMaterial2D";
        var bouncyMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
        if (bouncyMat == null)
        {
            bouncyMat = new PhysicsMaterial2D("BallBouncy");
            bouncyMat.bounciness = 1f;
            bouncyMat.friction = 0f;
            AssetDatabase.CreateAsset(bouncyMat, matPath);
            AssetDatabase.SaveAssets();
        }

        // スプライト読み込み
        Sprite bgSprite = LoadSprite(sp + "background.png");
        Sprite ballSprite = LoadSprite(sp + "ball.png");
        Sprite paddleSprite = LoadSprite(sp + "paddle.png");
        Sprite blockRed = LoadSprite(sp + "block_red.png");
        Sprite blockOrange = LoadSprite(sp + "block_orange.png");
        Sprite blockYellow = LoadSprite(sp + "block_yellow.png");
        Sprite blockGreen = LoadSprite(sp + "block_green.png");
        Sprite blockBlue = LoadSprite(sp + "block_blue.png");

        // Background
        var bgObj = new GameObject("Background");
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = bgSprite;
        bgSr.sortingOrder = -10;
        bgObj.transform.localScale = bgSprite != null ? new Vector3(0.02f, 0.0115f, 1f) : new Vector3(12f, 12f, 1f);
        if (bgSprite == null) bgSr.color = new Color(0.04f, 0.06f, 0.16f);

        // 壁 (Left, Right, Top)
        CreateWall("WallLeft",  new Vector3(-4.6f, 0f, 0f), new Vector2(0.2f, 11f));
        CreateWall("WallRight", new Vector3( 4.6f, 0f, 0f), new Vector2(0.2f, 11f));
        CreateWall("WallTop",   new Vector3( 0f, 5.6f, 0f), new Vector2(9.4f, 0.2f));

        // Paddle
        var paddleObj = new GameObject("Paddle");
        paddleObj.transform.position = new Vector3(0f, -4.2f, 0f);
        paddleObj.transform.localScale = new Vector3(1.8f, 0.25f, 1f);
        var paddleSr = paddleObj.AddComponent<SpriteRenderer>();
        paddleSr.sprite = paddleSprite;
        paddleSr.sortingOrder = 3;
        if (paddleSprite == null) paddleSr.color = new Color(0.6f, 0.6f, 1f);
        var paddleRb = paddleObj.AddComponent<Rigidbody2D>();
        paddleRb.bodyType = RigidbodyType2D.Kinematic;
        paddleRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var paddleCol = paddleObj.AddComponent<BoxCollider2D>();
        paddleCol.size = new Vector2(1f, 1f);
        if (bouncyMat != null) paddleCol.sharedMaterial = bouncyMat;

        // GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<BounceKingGameManager>();

        // BreakoutManager (child of GameManager)
        var bmObj = new GameObject("BreakoutManager");
        bmObj.transform.SetParent(gmObj.transform);
        var bm = bmObj.AddComponent<BreakoutManager>();
        var bmSO = new SerializedObject(bm);
        bmSO.FindProperty("_gameManager").objectReferenceValue = gm;
        bmSO.FindProperty("_paddleTransform").objectReferenceValue = paddleObj.transform;
        bmSO.FindProperty("_ballSprite").objectReferenceValue = ballSprite;
        bmSO.FindProperty("_bouncyMaterial").objectReferenceValue = bouncyMat;

        // blockSprites array
        var blockSpritesProp = bmSO.FindProperty("_blockSprites");
        blockSpritesProp.arraySize = 5;
        blockSpritesProp.GetArrayElementAtIndex(0).objectReferenceValue = blockRed;
        blockSpritesProp.GetArrayElementAtIndex(1).objectReferenceValue = blockOrange;
        blockSpritesProp.GetArrayElementAtIndex(2).objectReferenceValue = blockYellow;
        blockSpritesProp.GetArrayElementAtIndex(3).objectReferenceValue = blockGreen;
        blockSpritesProp.GetArrayElementAtIndex(4).objectReferenceValue = blockBlue;
        bmSO.ApplyModifiedProperties();

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Score Text
        var scoreTextObj = CT(canvasObj.transform, "ScoreText", "Score: 0", 36, jpFont,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300, 50), new Vector2(20, -20));
        scoreTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Lives Text
        var livesTextObj = CT(canvasObj.transform, "LivesText", "Lives: 3", 36, jpFont,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(200, 50), new Vector2(-20, -20));
        livesTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // Menu Button
        var menuBtn = CB(canvasObj.transform, "MenuButton", "メニューへ戻る", 28, jpFont,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(280, 60), new Vector2(0, 20),
            new Color(0.3f, 0.3f, 0.4f, 0.9f));
        menuBtn.AddComponent<BackToMenuButton>();

        // Clear Panel
        var clearPanel = CreatePanel(canvasObj.transform, "ClearPanel", new Color(0f, 0.3f, 0f, 0.9f));
        var clearTitleText = CT(clearPanel.transform, "ClearTitle", "ステージクリア！", 52, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero);
        clearTitleText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearScoreText = CT(clearPanel.transform, "ClearScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        clearScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var clearRetryBtn = CB(clearPanel.transform, "RetryButton", "もう一度", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.1f, 0.5f, 0.1f, 1f));
        clearPanel.SetActive(false);

        // GameOver Panel
        var goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.05f, 0f, 0.9f));
        var goTitleText = CT(goPanel.transform, "GameOverTitle", "ゲームオーバー", 52, jpFont,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(600, 100), Vector2.zero);
        goTitleText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goScoreText = CT(goPanel.transform, "GameOverScoreText", "Score: 0", 40, jpFont,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400, 60), Vector2.zero);
        goScoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        var goRetryBtn = CB(goPanel.transform, "RetryButton", "リトライ", 32, jpFont,
            new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(220, 65), Vector2.zero,
            new Color(0.5f, 0.2f, 0.1f, 1f));
        goPanel.SetActive(false);

        // BounceKingUI (child of GameManager)
        var uiObj = new GameObject("BounceKingUI");
        uiObj.transform.SetParent(gmObj.transform);
        var ui = uiObj.AddComponent<BounceKingUI>();
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("_scoreText").objectReferenceValue = scoreTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_livesText").objectReferenceValue = livesTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue = clearPanel;
        uiSO.FindProperty("_clearScoreText").objectReferenceValue = clearScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearRetryButton").objectReferenceValue = clearRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_gameOverRetryButton").objectReferenceValue = goRetryBtn.GetComponent<Button>();
        uiSO.FindProperty("_menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_breakoutManager").objectReferenceValue = bm;
        gmSO.FindProperty("_ui").objectReferenceValue = ui;
        gmSO.FindProperty("_initialLives").intValue = 3;
        gmSO.ApplyModifiedProperties();

        // Retry buttons → GameManager.RestartGame()
        var clearRetryBtnComp = clearRetryBtn.GetComponent<Button>();
        var goRetryBtnComp = goRetryBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            clearRetryBtnComp.onClick, gm.RestartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            goRetryBtnComp.onClick, gm.RestartGame);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eo = new GameObject("EventSystem");
            eo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eo.AddComponent<InputSystemUIInputModule>();
        }

        // Save
        string scenePath = "Assets/Scenes/031_BounceKing.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup031] BounceKing シーンを作成しました: " + scenePath);
    }

    private static void CreateWall(string name, Vector3 pos, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.position = pos;
        var col = obj.AddComponent<BoxCollider2D>();
        col.size = size;
        var rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
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
        r.anchorMin = new Vector2(0.1f, 0.3f);
        r.anchorMax = new Vector2(0.9f, 0.7f);
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
