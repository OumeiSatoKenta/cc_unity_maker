using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.InputSystem.UI;using TMPro;using Game047_SpinBalance;
public static class Setup047_SpinBalance
{
    [MenuItem("Assets/Setup/047 SpinBalance")]
    public static void CreateScene()
    {
        if(EditorApplication.isPlaying){Debug.LogError("[Setup047] Play モード中は実行できません。");return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);var jpFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera=Object.FindFirstObjectByType<Camera>();if(camera!=null){camera.backgroundColor=new Color(0.08f,0.1f,0.15f,1f);camera.orthographic=true;camera.orthographicSize=5f;}

        var gmObj=new GameObject("GameManager");var gm=gmObj.AddComponent<SpinBalanceGameManager>();
        var boardObj=new GameObject("BalanceBoard");boardObj.transform.SetParent(gmObj.transform);var bm=boardObj.AddComponent<BalanceManager>();
        var bmSO=new SerializedObject(bm);bmSO.FindProperty("_gameManager").objectReferenceValue=gm;bmSO.ApplyModifiedProperties();

        var canvasObj=new GameObject("Canvas");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);canvasObj.AddComponent<GraphicRaycaster>();
        var timerText=CT(canvasObj.transform,"TimerText","0.0s",32,jpFont,new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(200,50),new Vector2(0,-20));
        timerText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var pieceText=CT(canvasObj.transform,"PieceText","コマ: 0",28,jpFont,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(200,40),new Vector2(20,-20));
        var hint=CT(canvasObj.transform,"HintText","左右クリックで盤面を回転!",20,jpFont,new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(400,35),new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;hint.GetComponent<TextMeshProUGUI>().color=new Color(0.5f,0.5f,0.6f);
        var menuBtn=CB(canvasObj.transform,"MenuButton","メニューへ戻る",24,jpFont,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(240,50),new Vector2(-20,-20),new Color(0.3f,0.3f,0.4f,0.9f));

        var goPanel=new GameObject("GameOverPanel",typeof(RectTransform));goPanel.transform.SetParent(canvasObj.transform,false);goPanel.AddComponent<Image>().color=new Color(0,0,0,0.85f);
        var gr=goPanel.GetComponent<RectTransform>();gr.anchorMin=new Vector2(0.2f,0.25f);gr.anchorMax=new Vector2(0.8f,0.75f);gr.offsetMin=gr.offsetMax=Vector2.zero;
        var goTitle=CT(goPanel.transform,"GOTitle","落下!",48,jpFont,new Vector2(0.5f,0.7f),new Vector2(0.5f,0.7f),new Vector2(0.5f,0.5f),new Vector2(300,60),Vector2.zero);
        goTitle.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var resultText=CT(goPanel.transform,"ResultText","0.0秒 / 0コマ",32,jpFont,new Vector2(0.5f,0.45f),new Vector2(0.5f,0.45f),new Vector2(0.5f,0.5f),new Vector2(350,50),Vector2.zero);
        resultText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;resultText.GetComponent<TextMeshProUGUI>().color=new Color(1f,0.85f,0.3f);
        var retryBtn=CB(goPanel.transform,"RetryButton","リトライ",28,jpFont,new Vector2(0.5f,0.15f),new Vector2(0.5f,0.15f),new Vector2(0.5f,0.5f),new Vector2(200,60),Vector2.zero,new Color(0.3f,0.5f,0.8f,1f));
        goPanel.SetActive(false);

        var uiObj=new GameObject("SpinBalanceUI");var sbUI=uiObj.AddComponent<SpinBalanceUI>();
        var uiSO=new SerializedObject(sbUI);uiSO.FindProperty("_timerText").objectReferenceValue=timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_pieceText").objectReferenceValue=pieceText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameOverPanel").objectReferenceValue=goPanel;uiSO.FindProperty("_resultText").objectReferenceValue=resultText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameManager").objectReferenceValue=gm;uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();var gmSO=new SerializedObject(gm);gmSO.FindProperty("_balanceManager").objectReferenceValue=bm;gmSO.FindProperty("_ui").objectReferenceValue=sbUI;gmSO.ApplyModifiedProperties();

        retryBtn.GetComponent<Button>().onClick.AddListener(sbUI.OnRetryButton);

        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var eo=new GameObject("EventSystem");eo.AddComponent<UnityEngine.EventSystems.EventSystem>();eo.AddComponent<InputSystemUIInputModule>();}

        string scenePath="Assets/Scenes/047_SpinBalance.unity";EditorSceneManager.SaveScene(scene,scenePath);AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup047_SpinBalance] シーンを作成しました: "+scenePath);
    }
    private static GameObject CT(Transform p,string n,string t,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var tmp=o.AddComponent<TextMeshProUGUI>();tmp.text=t;tmp.fontSize=fs;tmp.color=Color.white;if(f!=null)tmp.font=f;var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;return o;}
    private static GameObject CB(Transform p,string n,string l,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap,Color bg){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);o.AddComponent<Image>().color=bg;o.AddComponent<Button>();var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;var t=new GameObject("Text",typeof(RectTransform));t.transform.SetParent(o.transform,false);var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=l;tmp.fontSize=fs;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;if(f!=null)tmp.font=f;var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;return o;}
    private static void AddSceneToBuildSettings(string scenePath){var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);foreach(var s in scenes)if(s.path==scenePath)return;scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
}
