using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.InputSystem.UI;using TMPro;using Game044_TiltMaze;
public static class Setup044_TiltMaze
{
    [MenuItem("Assets/Setup/044 TiltMaze")]
    public static void CreateScene()
    {
        if(EditorApplication.isPlaying){Debug.LogError("[Setup044] Play モード中は実行できません。");return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);var jpFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera=Object.FindFirstObjectByType<Camera>();if(camera!=null){camera.backgroundColor=new Color(0.12f,0.14f,0.2f,1f);camera.orthographic=true;camera.orthographicSize=4f;}
        string whiteTexPath="Assets/Scripts/Game044_TiltMaze/WhiteSquare.png";
        if(!System.IO.File.Exists(whiteTexPath)){var wTex=new Texture2D(4,4);var px=new Color[16];for(int i=0;i<16;i++)px[i]=Color.white;wTex.SetPixels(px);wTex.Apply();System.IO.File.WriteAllBytes(whiteTexPath,wTex.EncodeToPNG());Object.DestroyImmediate(wTex);AssetDatabase.ImportAsset(whiteTexPath);var imp=AssetImporter.GetAtPath(whiteTexPath)as TextureImporter;if(imp!=null){imp.textureType=TextureImporterType.Sprite;imp.spritePixelsPerUnit=1;imp.SaveAndReimport();}}

        var gmObj=new GameObject("GameManager");var gm=gmObj.AddComponent<TiltMazeGameManager>();
        var boardObj=new GameObject("MazeBoard");boardObj.transform.SetParent(gmObj.transform);var mm=boardObj.AddComponent<MazeManager>();
        var mmSO=new SerializedObject(mm);mmSO.FindProperty("_gameManager").objectReferenceValue=gm;mmSO.ApplyModifiedProperties();

        var canvasObj=new GameObject("Canvas");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);canvasObj.AddComponent<GraphicRaycaster>();
        var stageText=CT(canvasObj.transform,"StageText","ステージ 1",32,jpFont,new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(300,50),new Vector2(0,-20));
        stageText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var timerText=CT(canvasObj.transform,"TimerText","0.0s",28,jpFont,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(150,40),new Vector2(20,-20));
        var hint=CT(canvasObj.transform,"HintText","矢印キー/WASD/クリックで操作",18,jpFont,new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(450,35),new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;hint.GetComponent<TextMeshProUGUI>().color=new Color(0.5f,0.5f,0.6f);
        var menuBtn=CB(canvasObj.transform,"MenuButton","メニューへ戻る",24,jpFont,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(240,50),new Vector2(-20,-20),new Color(0.3f,0.3f,0.4f,0.9f));

        var clrPanel=new GameObject("ClearPanel",typeof(RectTransform));clrPanel.transform.SetParent(canvasObj.transform,false);clrPanel.AddComponent<Image>().color=new Color(0,0,0,0.85f);
        var cr=clrPanel.GetComponent<RectTransform>();cr.anchorMin=new Vector2(0.2f,0.25f);cr.anchorMax=new Vector2(0.8f,0.75f);cr.offsetMin=cr.offsetMax=Vector2.zero;
        var clrTitle=CT(clrPanel.transform,"ClearTitle","クリア!",48,jpFont,new Vector2(0.5f,0.75f),new Vector2(0.5f,0.75f),new Vector2(0.5f,0.5f),new Vector2(300,60),Vector2.zero);
        clrTitle.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;clrTitle.GetComponent<TextMeshProUGUI>().color=new Color(0.3f,1f,0.5f);
        var clrTime=CT(clrPanel.transform,"ClearTimeText","0.0秒でクリア!",32,jpFont,new Vector2(0.5f,0.5f),new Vector2(0.5f,0.5f),new Vector2(0.5f,0.5f),new Vector2(350,50),Vector2.zero);
        clrTime.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var nextBtn=CB(clrPanel.transform,"NextButton","次のステージ",28,jpFont,new Vector2(0.5f,0.2f),new Vector2(0.5f,0.2f),new Vector2(0.5f,0.5f),new Vector2(220,60),new Vector2(0,10),new Color(0.2f,0.6f,0.3f,1f));
        var retryBtn=CB(clrPanel.transform,"RetryButton","リトライ",24,jpFont,new Vector2(0.5f,0.05f),new Vector2(0.5f,0.05f),new Vector2(0.5f,0.5f),new Vector2(180,50),Vector2.zero,new Color(0.5f,0.5f,0.6f,1f));
        clrPanel.SetActive(false);

        var uiObj=new GameObject("TiltMazeUI");var tmUI=uiObj.AddComponent<TiltMazeUI>();
        var uiSO=new SerializedObject(tmUI);uiSO.FindProperty("_timerText").objectReferenceValue=timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_stageText").objectReferenceValue=stageText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_clearPanel").objectReferenceValue=clrPanel;uiSO.FindProperty("_clearTimeText").objectReferenceValue=clrTime.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameManager").objectReferenceValue=gm;uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();var gmSO=new SerializedObject(gm);gmSO.FindProperty("_mazeManager").objectReferenceValue=mm;gmSO.FindProperty("_ui").objectReferenceValue=tmUI;gmSO.ApplyModifiedProperties();

        nextBtn.GetComponent<Button>().onClick.AddListener(tmUI.OnNextButton);
        retryBtn.GetComponent<Button>().onClick.AddListener(tmUI.OnRetryButton);

        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var eo=new GameObject("EventSystem");eo.AddComponent<UnityEngine.EventSystems.EventSystem>();eo.AddComponent<InputSystemUIInputModule>();}

        string scenePath="Assets/Scenes/044_TiltMaze.unity";EditorSceneManager.SaveScene(scene,scenePath);AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup044_TiltMaze] シーンを作成しました: "+scenePath);
    }
    private static GameObject CT(Transform p,string n,string t,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var tmp=o.AddComponent<TextMeshProUGUI>();tmp.text=t;tmp.fontSize=fs;tmp.color=Color.white;if(f!=null)tmp.font=f;var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;return o;}
    private static GameObject CB(Transform p,string n,string l,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap,Color bg){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);o.AddComponent<Image>().color=bg;o.AddComponent<Button>();var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;var t=new GameObject("Text",typeof(RectTransform));t.transform.SetParent(o.transform,false);var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=l;tmp.fontSize=fs;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;if(f!=null)tmp.font=f;var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;return o;}
    private static void AddSceneToBuildSettings(string scenePath){var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);foreach(var s in scenes)if(s.path==scenePath)return;scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
}
