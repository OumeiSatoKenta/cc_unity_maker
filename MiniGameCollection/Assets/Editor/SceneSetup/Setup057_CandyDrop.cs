using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.InputSystem.UI;using TMPro;using Game057_CandyDrop;
public static class Setup057_CandyDrop
{
    [MenuItem("Assets/Setup/057 CandyDrop")]
    public static void CreateScene()
    {
        if(EditorApplication.isPlaying){Debug.LogError("[Setup057] Play モード中は実行できません。");return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);var jpFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera=Object.FindFirstObjectByType<Camera>();if(camera!=null){camera.backgroundColor=new Color(0.95f,0.88f,0.92f,1f);camera.orthographic=true;camera.orthographicSize=5f;}

        var gmObj=new GameObject("GameManager");var gm=gmObj.AddComponent<CandyDropGameManager>();
        var boardObj=new GameObject("CandyBoard");boardObj.transform.SetParent(gmObj.transform);var cm=boardObj.AddComponent<CandyManager>();
        var cmSO=new SerializedObject(cm);cmSO.FindProperty("_gameManager").objectReferenceValue=gm;cmSO.ApplyModifiedProperties();

        var canvasObj=new GameObject("Canvas");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);canvasObj.AddComponent<GraphicRaycaster>();
        var scoreText=CT(canvasObj.transform,"ScoreText","スコア: 0",32,jpFont,new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(250,50),new Vector2(0,-20));
        scoreText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;scoreText.GetComponent<TextMeshProUGUI>().color=new Color(0.4f,0.2f,0.3f);
        var hint=CT(canvasObj.transform,"HintText","左右移動+スペース/離してドロップ!",18,jpFont,new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(500,35),new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;hint.GetComponent<TextMeshProUGUI>().color=new Color(0.5f,0.4f,0.45f);
        var menuBtn=CB(canvasObj.transform,"MenuButton","メニューへ戻る",24,jpFont,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(240,50),new Vector2(-20,-20),new Color(0.5f,0.35f,0.4f,0.9f));

        var goPanel=new GameObject("GameOverPanel",typeof(RectTransform));goPanel.transform.SetParent(canvasObj.transform,false);goPanel.AddComponent<Image>().color=new Color(0,0,0,0.85f);
        var gr=goPanel.GetComponent<RectTransform>();gr.anchorMin=new Vector2(0.2f,0.25f);gr.anchorMax=new Vector2(0.8f,0.75f);gr.offsetMin=gr.offsetMax=Vector2.zero;
        CT(goPanel.transform,"T","崩壊!",48,jpFont,new Vector2(0.5f,0.7f),new Vector2(0.5f,0.7f),new Vector2(0.5f,0.5f),new Vector2(300,60),Vector2.zero).GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var finalText=CT(goPanel.transform,"FS","スコア: 0",36,jpFont,new Vector2(0.5f,0.45f),new Vector2(0.5f,0.45f),new Vector2(0.5f,0.5f),new Vector2(300,50),Vector2.zero);
        finalText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;finalText.GetComponent<TextMeshProUGUI>().color=new Color(1f,0.7f,0.8f);
        var retryBtn=CB(goPanel.transform,"RetryBtn","リトライ",28,jpFont,new Vector2(0.5f,0.15f),new Vector2(0.5f,0.15f),new Vector2(0.5f,0.5f),new Vector2(200,60),Vector2.zero,new Color(0.7f,0.3f,0.4f,1f));
        goPanel.SetActive(false);

        var uiObj=new GameObject("CandyDropUI");var cdUI=uiObj.AddComponent<CandyDropUI>();
        var uiSO=new SerializedObject(cdUI);uiSO.FindProperty("_scoreText").objectReferenceValue=scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue=goPanel;uiSO.FindProperty("_finalScoreText").objectReferenceValue=finalText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameManager").objectReferenceValue=gm;uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();var gmSO=new SerializedObject(gm);gmSO.FindProperty("_candyManager").objectReferenceValue=cm;gmSO.FindProperty("_ui").objectReferenceValue=cdUI;gmSO.ApplyModifiedProperties();
        retryBtn.GetComponent<Button>().onClick.AddListener(cdUI.OnRetryButton);

        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var eo=new GameObject("EventSystem");eo.AddComponent<UnityEngine.EventSystems.EventSystem>();eo.AddComponent<InputSystemUIInputModule>();}

        string scenePath="Assets/Scenes/057_CandyDrop.unity";EditorSceneManager.SaveScene(scene,scenePath);AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup057_CandyDrop] シーンを作成しました: "+scenePath);
    }
    private static GameObject CT(Transform p,string n,string t,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var tmp=o.AddComponent<TextMeshProUGUI>();tmp.text=t;tmp.fontSize=fs;tmp.color=Color.white;if(f!=null)tmp.font=f;var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;return o;}
    private static GameObject CB(Transform p,string n,string l,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap,Color bg){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);o.AddComponent<Image>().color=bg;o.AddComponent<Button>();var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;var t=new GameObject("Text",typeof(RectTransform));t.transform.SetParent(o.transform,false);var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=l;tmp.fontSize=fs;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;if(f!=null)tmp.font=f;var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;return o;}
    private static void AddSceneToBuildSettings(string scenePath){var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);foreach(var s in scenes)if(s.path==scenePath)return;scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
}
