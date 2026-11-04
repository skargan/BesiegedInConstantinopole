#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SharedProvisionsCapture : MonoBehaviour
{
    string output;int oldWidth,oldHeight,provincialChecks;FullScreenMode oldMode;readonly List<string> details=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-provisions38-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Provisions38"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Shared provisions verification");DontDestroyOnLoad(go);go.AddComponent<SharedProvisionsCapture>().output=path;
    }
    void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);details.Add("PASS "+label);}
    IEnumerator Start()
    {
        oldWidth=Screen.width;oldHeight=Screen.height;oldMode=Screen.fullScreenMode;
        File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());Exception failure=null;
        while(stack.Count>0){object next=null;bool moved=false;
            try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null)break;if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;
        }
        Screen.SetResolution(oldWidth,oldHeight,oldMode);yield return new WaitForSecondsRealtime(.4f);
        var all=SharedProvisionsChecks.Details.Concat(details).ToArray();File.WriteAllLines(Path.Combine(output,"details.txt"),all);
        string report=failure==null?"PROVISIONS38_CHECKS "+(all.Length+provincialChecks)+" passed; fractional stocks, ration headcounts, shortages, desertion, transfers, actual archive persistence, provincial reserves and presentation captures.":"FAILED "+failure;
        File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);if(failure!=null)Debug.LogException(failure);Application.Quit(failure==null?0:1);
    }
    IEnumerator Run()
    {
        AudioListener.volume=0;Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+25;while(!VariableSingleton.IsInitialized&&Time.realtimeSinceStartup<deadline)yield return null;yield return new WaitForSecondsRealtime(.7f);
        Check(VariableSingleton.IsInitialized,"campaign initialised");CampaignHUD.Instance.DismissReport();
        foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
        foreach(var gate in FindObjectsOfType<ProvincialConflictGate>())gate.enabled=false;
        SharedProvisionsChecks.Run();
        provincialChecks=ProvincialFoodChecks.Run()+MedicalProvisionsChecks.Run();
        details.Add("PASS ProvincialFoodChecks and MedicalProvisionsChecks completed "+provincialChecks+" checks");provincialChecks--;
        typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,VariableSingleton.EffectiveSiegeTurn+30);
        VariableSingleton.SetInt("iPopulation",100499);VariableSingleton.SetInt("iSpies",8);
        VariableSingleton.SetInt("iBalkans",2000);VariableSingleton.SetInt("iBazouks",5000);VariableSingleton.SetInt("iJanissaries",3000);
        SharedProvisions.Ensure();SharedProvisions.Set(SharedProvisions.City,3123.456);SharedProvisions.Set(SharedProvisions.Camp,421.875);
        var manager=FindObjectOfType<ManagerUI>(true);manager.gameObject.SetActive(true);CampaignHUD.Instance.DismissReport();var council=CouncilUI.For(manager);
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.25f);
            council.Market();yield return null;Capture("market-"+size.x+".png");Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text.Contains("Daily ration")),"market ration information visible at "+size.x);
            council.Spies();yield return null;Capture("intelligence-"+size.x+".png");Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text.Contains("Food consumption")),"intelligence food report visible at "+size.x);
            council.Overview();yield return null;Capture("overview-"+size.x+".png");
            if(size.x==1280){
                council.Infirmary();yield return null;Capture("infirmary-1280.png");Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text.Contains("20 patients")),"infirmary shows the extra treatment ration rule");council.Hide();
                ProvinceCampaign.Ensure();ProvinceCampaign.Set("Agent_macedonia",2);ProvincialConflictUI.Show("macedonia");yield return null;Capture("province-1280.png");Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text.Contains("Food reserve:")),"resident agent reveals provincial physical food reserve");ProvincialConflictUI.Close();
                council.ProvincialOperationsFor("macedonia");yield return null;Capture("province-operations-1280.png");
            }
            council.Hide();yield return CampaignMenuChecks(size.x);
        }
        council.Hide();
    }
    IEnumerator CampaignMenuChecks(int width)
    {
        string previousRoot=SaveGames.TestRoot;SaveGames.TestRoot=Path.Combine(output,"Menu-fixture-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(SaveGames.TestRoot);
        try{
            SaveMenu.NewCampaign(null);yield return null;
            var tips=FindObjectsOfType<CampaignChoiceTooltip>();var locked=tips.Single(t=>t.GetComponentInChildren<TMP_Text>().text=="Start at the Siege");
            Check(!locked.GetComponent<Button>().interactable&&locked.Hint.Contains("Complete the preparation turns"),"unavailable siege entry retains its hover explanation at "+width);
            locked.OnPointerEnter(null);yield return null;Capture("campaign-locked-hover-"+width+".png");locked.OnPointerExit(null);SaveMenu.Close();yield return null;
            File.WriteAllText(VariableSingleton.PreparationPresetPath,JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));
            SaveMenu.NewCampaign(null);yield return null;Canvas.ForceUpdateCanvases();Capture("new-campaign-"+width+".png");
            tips=FindObjectsOfType<CampaignChoiceTooltip>().OrderByDescending(t=>t.transform.position.y).ToArray();
            Check(tips.Length==3&&tips[0].GetComponentInChildren<TMP_Text>().text=="Normal campaign"&&tips[1].GetComponentInChildren<TMP_Text>().text=="Start at the Siege"&&tips[2].GetComponentInChildren<TMP_Text>().text=="Ironman · achievements","three campaign choices have the requested order at "+width);
            var first=(RectTransform)tips[0].transform;Check(tips.All(t=>Vector2.Distance(((RectTransform)t.transform).rect.size,first.rect.size)<.1f&&Mathf.Abs(t.transform.position.x-first.position.x)<.1f),"campaign buttons have equal size and a shared centre at "+width);
            Check(GameObject.Find("Campaign choice explanation")==null,"campaign explanations remain hidden until hover at "+width);
            tips[1].OnPointerEnter(null);yield return null;Capture("campaign-siege-hover-"+width+".png");tips[1].OnPointerExit(null);
            SaveMenu.StartAtSiege(null);yield return null;Capture("start-at-siege-"+width+".png");
            Check(!FindObjectsOfType<TMP_Text>().Any(t=>t.text=="Choose campaign mode"),"replay screen has no redundant campaign-mode heading at "+width);
            var replay=FindObjectsOfType<CampaignChoiceTooltip>();Check(replay.Length==2&&replay.All(t=>t.GetComponent<Button>().interactable),"both replay modes preserve availability at "+width);
            replay[0].OnPointerEnter(null);yield return null;Capture("start-at-siege-hover-"+width+".png");replay[0].OnPointerExit(null);
        }finally{SaveMenu.Close();SaveGames.TestRoot=previousRoot;}
    }
    void Capture(string name)
    {
        Canvas.ForceUpdateCanvases();var go=new GameObject("Provisions capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}
    }
}
#endif
