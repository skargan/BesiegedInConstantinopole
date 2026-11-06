#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Explicit exported-player opt-in. All purchases and archives use an isolated test campaign.
public sealed class MarketCapture : MonoBehaviour
{
    string output;CouncilUI council;VariableSingleton.CampaignSave baseline;
    readonly List<string> results=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-market-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Market"));Directory.CreateDirectory(folder);
        SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Market verification");DontDestroyOnLoad(go);go.AddComponent<MarketCapture>().output=folder;
    }
    void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);results.Add("PASS "+name);}
    string State(){var state=VariableSingleton.Instance.CaptureState();state.randomState=default(UnityEngine.Random.State);return JsonUtility.ToJson(state);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    GameObject Desk()=>GameObject.Find("Market desk");
    TMP_InputField Input()=>Desk().GetComponentsInChildren<TMP_InputField>().First(f=>f.name=="Market quantity iProvisions");
    Button Button(string name)=>Desk().GetComponentsInChildren<Button>().First(b=>b.name==name);
    Rect Bounds(RectTransform rect){var c=new Vector3[4];rect.GetWorldCorners(c);var p=c.Select(v=>RectTransformUtility.WorldToScreenPoint(null,v)).ToArray();return Rect.MinMaxRect(p.Min(v=>v.x),p.Min(v=>v.y),p.Max(v=>v.x),p.Max(v=>v.y));}
    bool Inside(Rect outer,Rect inner)=>inner.xMin>=outer.xMin-2&&inner.xMax<=outer.xMax+2&&inner.yMin>=outer.yMin-2&&inner.yMax<=outer.yMax+2;
    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;
            try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;
        }
        string summary="MARKET_CAPTURE "+results.Count+" checks passed at 1600x900 and 1280x720; isolated purchases and save reload verified.";
        File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),summary);Debug.Log(summary);Application.Quit(0);
    }
    IEnumerator Run()
    {
        AudioListener.volume=0;Application.runInBackground=true;yield return null;
        Check(SaveGames.Automated,"archives isolated before campaign start");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+20;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized,"campaign initialized");yield return new WaitForSecondsRealtime(.5f);
        CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());baseline=VariableSingleton.Instance.CaptureState();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);string suffix=size.x+"x"+size.y;
            Check(Screen.width==size.x&&Screen.height==size.y,suffix+" resolution applied");
            var fixture=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));fixture.turn=1;
            VariableSingleton.Instance.RestoreValidated(fixture);MarketPurchaseLimits.ResetForRestore();VariableSingleton.SetInt("iMoney",1234567);
            string before=State(),rng=Rng();council.Market();yield return null;Canvas.ForceUpdateCanvases();
            Check(before==State()&&rng==Rng(),suffix+" opening market preserves campaign and RNG");
            var bounds=Bounds(Desk().GetComponent<RectTransform>());
            Check(bounds.width<=Screen.width*.48f && bounds.xMin>=Screen.width*.5f && Inside(new Rect(0,0,Screen.width,Screen.height),bounds),suffix+" market fits right half of screen");
            var bad=new List<string>();foreach(var label in Desk().GetComponentsInChildren<TMP_Text>()){label.ForceMeshUpdate();if(label.isTextOverflowing)bad.Add(label.text);}
            Check(bad.Count==0,suffix+" all market text fits: "+string.Join(" | ",bad));
            Check(Desk().GetComponentsInChildren<TMP_InputField>().Length==3 && !Desk().GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("1,234,567")),suffix+" all resources editable without duplicate treasury");
            var stepPanel=Desk().transform.Find("Market quantity step");
            Check(stepPanel!=null&&stepPanel.GetComponentsInChildren<Button>().Length==3&&new[]{1,10,100}.All(n=>Button("Market step "+n).transform.parent==stepPanel),suffix+" quantity label and all steps share their compact panel");
            Check(Desk().GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("Daily food consumption:")&&t.text.Contains("Food remaining:")),suffix+" daily consumption and food horizon shown together");
            Check(Button("Market emergency levy").GetComponent<TurnActionButton>()!=null && new[]{"iProvisions","iMaterials","iAmmunition"}.All(k=>Button("Market buy "+k).GetComponent<TurnActionButton>()==null),suffix+" only levy commits time");
            yield return Capture("market-"+suffix);
            var merchant=FindObjectsOfType<LocationHotspot>().First(h=>h.name=="Hotspot · Merchant");Check(merchant.Shape.Length>=35,suffix+" merchant silhouette retains fine contour points");merchant.SetHighlighted(true);yield return Capture("market-merchant-outline-"+suffix);merchant.SetHighlighted(false);
            Input().text="1";int gold=DailyLifeEvents.I("iMoney"),food=DailyLifeEvents.I("iProvisions"),turn=VariableSingleton.Turn;
            Button("Market buy iProvisions").onClick.Invoke();yield return null;
            Check(DailyLifeEvents.I("iProvisions")==food+1&&DailyLifeEvents.I("iMoney")==gold-2&&MarketPurchaseLimits.Remaining("iProvisions")==MarketPurchaseLimits.ProvisionsPerTurn-1&&VariableSingleton.Turn==turn,suffix+" real quantity-one button buys exact amount without ending month");
            Input().text="13";Button("Market buy iProvisions").onClick.Invoke();yield return null;
            Check(DailyLifeEvents.I("iProvisions")==food+14&&MarketPurchaseLimits.Remaining("iProvisions")==MarketPurchaseLimits.ProvisionsPerTurn-14,suffix+" typed thirteen buys exact amount");
            Input().text=(MarketPurchaseLimits.Remaining("iProvisions")+1).ToString();
            Check(!Button("Market buy iProvisions").interactable,suffix+" over-limit draft disables purchase");before=State();
            Button("Market buy iProvisions").onClick.Invoke();yield return null;Check(before==State(),suffix+" bypassed disabled click is also rejected by model");
            Check(File.Exists(VariableSingleton.SavePath),suffix+" successful purchase wrote isolated archive");
            Check(SaveGames.Load(VariableSingleton.SavePath,out string error),suffix+" archive reload: "+error);council.Market();yield return null;
            Check(MarketPurchaseLimits.Remaining("iProvisions")==MarketPurchaseLimits.ProvisionsPerTurn-14,suffix+" disk reload preserves shared allowance");
            Button("Market step 1").onClick.Invoke();yield return null;Input().text="13";Button("Market increase iProvisions").onClick.Invoke();Check(Input().text=="14",suffix+" plus-one exact adjustment");
            Button("Market decrease iProvisions").onClick.Invoke();Check(Input().text=="13",suffix+" minus-one exact adjustment");
            Button("Market step 100").onClick.Invoke();yield return null;Button("Market increase iProvisions").onClick.Invoke();Check(Input().text=="113",suffix+" hundred-step adjustment");
            Input().text="";Check(!Button("Market buy iProvisions").interactable,suffix+" empty draft cannot buy");
            Input().text="2147483648";Check(!Button("Market buy iProvisions").interactable,suffix+" overflowing draft cannot buy");
            Input().text="13";yield return Capture("market-after-purchases-"+suffix);council.Hide();
        }
    }
    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.15f);Canvas.ForceUpdateCanvases();
        var go=new GameObject("Market capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        }finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}
    }
}
#endif
