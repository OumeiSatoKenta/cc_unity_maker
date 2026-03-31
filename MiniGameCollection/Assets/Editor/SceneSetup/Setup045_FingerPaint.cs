using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.InputSystem.UI;using TMPro;using Game045_FingerPaint;
public static class Setup045_FingerPaint
{
    [MenuItem("Assets/Setup/045 FingerPaint")]
    public static void CreateScene()
    {
        if(EditorApplication.isPlaying){Debug.LogError("[Setup045] Play モード中は実行できません。");return;}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);var jpFont=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Regular SDF.asset");
        var camera=Object.FindFirstObjectByType<Camera>();if(camera!=null){camera.backgroundColor=new Color(0.95f,0.93f,0.88f,1f);camera.orthographic=true;camera.orthographicSize=5f;}
        string whiteTexPath="Assets/Scripts/Game045_FingerPaint/WhiteSquare.png";
        if(!System.IO.File.Exists(whiteTexPath)){var wTex=new Texture2D(4,4);var px=new Color[16];for(int i=0;i<16;i++)px[i]=Color.white;wTex.SetPixels(px);wTex.Apply();System.IO.File.WriteAllBytes(whiteTexPath,wTex.EncodeToPNG());Object.DestroyImmediate(wTex);AssetDatabase.ImportAsset(whiteTexPath);var imp=AssetImporter.GetAtPath(whiteTexPath)as TextureImporter;if(imp!=null){imp.textureType=TextureImporterType.Sprite;imp.spritePixelsPerUnit=1;imp.SaveAndReimport();}}
        var whiteSprite=AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);

        // Canvas background
        var canvasBg=new GameObject("CanvasBG");var bgSr=canvasBg.AddComponent<SpriteRenderer>();bgSr.sprite=whiteSprite;bgSr.color=new Color(0.96f,0.94f,0.9f);bgSr.sortingOrder=-1;canvasBg.transform.localScale=new Vector3(9f,7f,1f);

        var gmObj=new GameObject("GameManager");var gm=gmObj.AddComponent<FingerPaintGameManager>();
        var paintObj=new GameObject("PaintBoard");paintObj.transform.SetParent(gmObj.transform);var pm=paintObj.AddComponent<PaintManager>();
        var pmSO=new SerializedObject(pm);pmSO.FindProperty("_gameManager").objectReferenceValue=gm;pmSO.ApplyModifiedProperties();

        var canvasObj=new GameObject("Canvas");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObj.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);canvasObj.AddComponent<GraphicRaycaster>();
        var timerText=CT(canvasObj.transform,"TimerText","30s",36,jpFont,new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(0.5f,1),new Vector2(150,50),new Vector2(0,-20));
        timerText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;timerText.GetComponent<TextMeshProUGUI>().color=new Color(0.3f,0.3f,0.3f);
        var strokeText=CT(canvasObj.transform,"StrokeText","ストローク: 0",24,jpFont,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(250,40),new Vector2(20,-20));
        strokeText.GetComponent<TextMeshProUGUI>().color=new Color(0.4f,0.4f,0.4f);
        var hint=CT(canvasObj.transform,"HintText","マウスで自由に描こう!",20,jpFont,new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(400,35),new Vector2(0,20));
        hint.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;hint.GetComponent<TextMeshProUGUI>().color=new Color(0.5f,0.5f,0.5f);
        var menuBtn=CB(canvasObj.transform,"MenuButton","メニューへ戻る",24,jpFont,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(240,50),new Vector2(-20,-20),new Color(0.4f,0.35f,0.3f,0.9f));

        var finPanel=new GameObject("FinishPanel",typeof(RectTransform));finPanel.transform.SetParent(canvasObj.transform,false);finPanel.AddComponent<Image>().color=new Color(0,0,0,0.8f);
        var fr=finPanel.GetComponent<RectTransform>();fr.anchorMin=new Vector2(0.2f,0.3f);fr.anchorMax=new Vector2(0.8f,0.7f);fr.offsetMin=fr.offsetMax=Vector2.zero;
        var finTitle=CT(finPanel.transform,"FinishTitle","完成!",48,jpFont,new Vector2(0.5f,0.7f),new Vector2(0.5f,0.7f),new Vector2(0.5f,0.5f),new Vector2(300,60),Vector2.zero);
        finTitle.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;finTitle.GetComponent<TextMeshProUGUI>().color=new Color(1f,0.9f,0.5f);
        var resultText=CT(finPanel.transform,"ResultText","0 ストロークの作品!",28,jpFont,new Vector2(0.5f,0.45f),new Vector2(0.5f,0.45f),new Vector2(0.5f,0.5f),new Vector2(400,50),Vector2.zero);
        resultText.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;
        var retryBtn=CB(finPanel.transform,"RetryButton","もう一度描く",28,jpFont,new Vector2(0.5f,0.15f),new Vector2(0.5f,0.15f),new Vector2(0.5f,0.5f),new Vector2(220,60),Vector2.zero,new Color(0.6f,0.4f,0.2f,1f));
        finPanel.SetActive(false);

        var uiObj=new GameObject("FingerPaintUI");var fpUI=uiObj.AddComponent<FingerPaintUI>();
        var uiSO=new SerializedObject(fpUI);uiSO.FindProperty("_timerText").objectReferenceValue=timerText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_strokeText").objectReferenceValue=strokeText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_finishPanel").objectReferenceValue=finPanel;uiSO.FindProperty("_resultText").objectReferenceValue=resultText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("_gameManager").objectReferenceValue=gm;uiSO.ApplyModifiedProperties();

        menuBtn.AddComponent<BackToMenuButton>();var gmSO=new SerializedObject(gm);gmSO.FindProperty("_paintManager").objectReferenceValue=pm;gmSO.FindProperty("_ui").objectReferenceValue=fpUI;gmSO.ApplyModifiedProperties();

        retryBtn.GetComponent<Button>().onClick.AddListener(fpUI.OnRetryButton);

        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var eo=new GameObject("EventSystem");eo.AddComponent<UnityEngine.EventSystems.EventSystem>();eo.AddComponent<InputSystemUIInputModule>();}

        string scenePath="Assets/Scenes/045_FingerPaint.unity";EditorSceneManager.SaveScene(scene,scenePath);AddSceneToBuildSettings(scenePath);
        Debug.Log("[Setup045_FingerPaint] シーンを作成しました: "+scenePath);
    }
    private static GameObject CT(Transform p,string n,string t,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var tmp=o.AddComponent<TextMeshProUGUI>();tmp.text=t;tmp.fontSize=fs;tmp.color=Color.white;if(f!=null)tmp.font=f;var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;return o;}
    private static GameObject CB(Transform p,string n,string l,float fs,TMP_FontAsset f,Vector2 amin,Vector2 amax,Vector2 piv,Vector2 sd,Vector2 ap,Color bg){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);o.AddComponent<Image>().color=bg;o.AddComponent<Button>();var r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=piv;r.sizeDelta=sd;r.anchoredPosition=ap;var t=new GameObject("Text",typeof(RectTransform));t.transform.SetParent(o.transform,false);var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=l;tmp.fontSize=fs;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;if(f!=null)tmp.font=f;var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;return o;}
    private static void AddSceneToBuildSettings(string scenePath){var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);foreach(var s in scenes)if(s.path==scenePath)return;scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();}
}
