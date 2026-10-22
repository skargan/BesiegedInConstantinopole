using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public static class SaveMenu {
    static GameObject panel,overlay;
    static Transform Surface(){overlay=new GameObject("Archive overlay",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var c=overlay.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=250;var scale=overlay.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.matchWidthOrHeight=.5f;var shade=CampaignHUD.Panel(overlay.transform,"Archive shade",Vector2.zero,Vector2.one);shade.GetComponent<Image>().color=new Color(0,0,0,.65f);overlay.AddComponent<ArchiveHotkeys>();return overlay.transform;}

    public static bool Close(){if(overlay==null)return false;UnityEngine.Object.Destroy(overlay);overlay=null;panel=null;return true;}
    public static void StartCampaign(bool ironman){Close();SaveGames.PrepareNew(ironman);VariableSingleton.ContinueRequested=false;VariableSingleton.QuickStartRequested=false;SceneManager.LoadScene("Continue Game");}
    public static void NewCampaign(Transform parent){
        Close();parent=Surface();panel=CampaignHUD.Panel(parent,"Choose campaign mode",new Vector2(.12f,.13f),new Vector2(.88f,.87f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
        CampaignHUD.Label(panel.transform,"Begin your reign",new Vector2(.06f,.82f),new Vector2(.94f,.96f),38);
        CampaignHUD.Explanation(panel.transform,"Normal: named saves and earlier checkpoints. Achievements are disabled.\n\nIronman: one rolling save per campaign. Completed decisions and turns are saved automatically. No manual slots or earlier checkpoints. Only Ironman campaigns can earn achievements.\n\nAssault timing remains optional in either mode.",new Vector2(.06f,.25f),new Vector2(.94f,.79f),27);
        CampaignHUD.MakeButton(panel.transform,"Normal campaign",new Vector2(.06f,.12f),new Vector2(.47f,.23f),()=>StartCampaign(false));
        CampaignHUD.MakeButton(panel.transform,"Ironman · achievements",new Vector2(.53f,.12f),new Vector2(.94f,.23f),()=>StartCampaign(true));
        CampaignHUD.MakeButton(panel.transform,"Cancel",new Vector2(.35f,.025f),new Vector2(.65f,.095f),()=>Close());
    }
    public static void Show(Transform parent){
        Close();parent=Surface();panel=CampaignHUD.Panel(parent,"Campaign archives",new Vector2(.025f,.025f),new Vector2(.975f,.975f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
        CampaignHUD.Label(panel.transform,"Campaign archives",new Vector2(.04f,.91f),new Vector2(.96f,.985f),35);
        string mode=VariableSingleton.IsInitialized?(SaveGames.Ironman?"Ironman · rolling autosave · achievements enabled":"Normal campaign · achievements disabled"):"Select a campaign to continue";
        CampaignHUD.Explanation(panel.transform,mode+"\nSave format "+SaveGames.Format+" · "+CouncilUI.BuildLabel,new Vector2(.04f,.80f),new Vector2(.96f,.91f),20);
        var viewport=CampaignHUD.Panel(panel.transform,"Saved campaigns",new Vector2(.04f,.27f),new Vector2(.93f,.79f));viewport.AddComponent<RectMask2D>();
        var entries=SaveGames.Entries();var content=CampaignHUD.Panel(viewport.transform,"Archive rows",Vector2.zero,Vector2.one);content.GetComponent<Image>().color=Color.clear;
        var cr=content.GetComponent<RectTransform>();cr.anchorMin=new Vector2(0,1);cr.anchorMax=Vector2.one;cr.pivot=new Vector2(.5f,1);cr.sizeDelta=new Vector2(0,Mathf.Max(300,entries.Count*135));
        if(entries.Count==0)CampaignHUD.Label(content.transform,"No campaigns saved here yet.",new Vector2(.04f,.5f),new Vector2(.96f,.95f),27);
        for(int i=0;i<entries.Count;i++){
            var entry=entries[i];float top=1-i*135f/cr.sizeDelta.y,bottom=1-(i+1)*135f/cr.sizeDelta.y;
            var row=CampaignHUD.Panel(content.transform,"Save "+i,new Vector2(.01f,bottom+.01f),new Vector2(.99f,top-.01f));ImperialTheme.Panel(row,false);
            var a=entry.archive;string desc=a==null?Path.GetFileName(entry.path):a.name+" · "+(a.ironman?"IRONMAN":"Normal")+" · "+(a.calendar??("Turn "+a.campaign.turn))+"\n"+a.build+" · format "+a.saveFormat+" · population schema "+a.campaign.version+"\n"+a.savedUtc;
            if(entry.error!=null)desc+="\n"+entry.error;else if(a.build!=CouncilUI.BuildLabel)desc+="\nDifferent game build; save format supported.";
            var text=CampaignHUD.Label(row.transform,desc,new Vector2(.025f,.06f),new Vector2(.76f,.96f),21);text.enableAutoSizing=true;text.fontSizeMin=16;
            var load=CampaignHUD.MakeButton(row.transform,entry.Compatible?"Load":"Unavailable",new Vector2(.79f,.23f),new Vector2(.975f,.76f),()=>ConfirmLoad(parent,entry));load.interactable=entry.Compatible;
        }
        var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=cr;scroll.horizontal=false;scroll.scrollSensitivity=40;scroll.movementType=ScrollRect.MovementType.Clamped;CampaignHUD.AddScrollbar(scroll,panel.transform,new Vector2(.94f,.27f),new Vector2(.955f,.79f));
        var inputObject=new GameObject("Save name",typeof(RectTransform),typeof(Image),typeof(TMP_InputField));inputObject.transform.SetParent(panel.transform,false);var ir=inputObject.GetComponent<RectTransform>();ir.anchorMin=new Vector2(.04f,.16f);ir.anchorMax=new Vector2(.66f,.235f);ir.offsetMin=ir.offsetMax=Vector2.zero;inputObject.GetComponent<Image>().color=ImperialTheme.Surface;
        var label=CampaignHUD.Label(inputObject.transform,"",new Vector2(.03f,.05f),new Vector2(.97f,.95f),24);var input=inputObject.GetComponent<TMP_InputField>();input.textViewport=ir;input.textComponent=(TextMeshProUGUI)label;input.characterLimit=60;input.text="Council · "+(VariableSingleton.IsInitialized?VariableSingleton.GetDate().ToString("d MMM yyyy"):"new save");input.interactable=SaveGames.CanManualSave;
        var hint=CampaignHUD.Label(panel.transform,"Files: "+SaveGames.Root+"\nCopy a .besieged.json file to export it; place one here to import. Ironman backups are recovery files, not selectable slots.",new Vector2(.04f,.065f),new Vector2(.96f,.145f),16);
        var save=CampaignHUD.MakeButton(panel.transform,"Save campaign",new Vector2(.70f,.16f),new Vector2(.96f,.235f),()=>{if(SaveGames.Named(input.text,out var message))Show(parent);else hint.text=message;});save.interactable=SaveGames.CanManualSave;
        CampaignHUD.MakeButton(panel.transform,"Open save folder",new Vector2(.04f,.015f),new Vector2(.39f,.06f),SaveGames.OpenFolder);
        CampaignHUD.MakeButton(panel.transform,"Return",new Vector2(.67f,.015f),new Vector2(.96f,.06f),()=>Close());
    }
    static void ConfirmLoad(Transform parent,SaveGames.Entry entry){
        Close();parent=Surface();panel=CampaignHUD.Panel(parent,"Load campaign confirmation",new Vector2(.18f,.22f),new Vector2(.82f,.78f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
        CampaignHUD.Label(panel.transform,"Continue this campaign?",new Vector2(.06f,.77f),new Vector2(.94f,.95f),33);
        CampaignHUD.Explanation(panel.transform,entry.archive.name+"\n"+entry.archive.build+" · save format "+entry.archive.saveFormat+"\n\nCurrent unsaved progress will be left behind. The selected campaign keeps its original mode. Loading a Normal or legacy campaign does not enable achievements.",new Vector2(.06f,.26f),new Vector2(.94f,.75f),25);
        CampaignHUD.MakeButton(panel.transform,"Cancel",new Vector2(.06f,.07f),new Vector2(.46f,.20f),()=>Show(parent));
        CampaignHUD.MakeButton(panel.transform,"Load campaign",new Vector2(.54f,.07f),new Vector2(.94f,.20f),()=>{SaveGames.RequestedFile=entry.path;VariableSingleton.ContinueRequested=true;VariableSingleton.QuickStartRequested=false;Close();SceneManager.LoadScene("Continue Game");});
    }
}

public class ArchiveHotkeys:MonoBehaviour {void Update(){if(ControlSingleton.Instance==null&&Input.GetKeyDown(KeyCode.Escape))SaveMenu.Close();}}

// Resume only at declared safe boundaries; never replay a paid decision or reroll its reward.
public class SaveResume : MonoBehaviour {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Register(){SceneManager.sceneLoaded-=Attach;SceneManager.sceneLoaded+=Attach;}
    static void Attach(Scene scene,LoadSceneMode mode){if(UnityEngine.Object.FindObjectOfType<VariableSingleton>()!=null)new GameObject("Save continuation").AddComponent<SaveResume>();}
    System.Collections.IEnumerator Start(){
        yield return null;yield return null;
        var a=SaveGames.PendingResume;SaveGames.PendingResume=null;
        if(a!=null){
            var manager=UnityEngine.Object.FindObjectOfType<ManagerUI>(true);
            if(a.phase=="action"){manager.gameObject.SetActive(false);ActionResults.Instance.Message=a.report;UnityEngine.Object.FindObjectOfType<RestUI>(true).gameObject.SetActive(true);}
            else if(a.phase=="event"){manager.gameObject.SetActive(false);UnityEngine.Object.FindObjectOfType<EventUI>(true).ResumeCheckpoint(a.node,a.title,a.report);}
            else if(a.phase=="advance"){manager.gameObject.SetActive(false);CampaignHUD.Instance.ShowReport(a.title??"Siege report",a.report??"The operation is complete.",()=>{CampaignHUD.Instance.DismissReport();VariableSingleton.IncrementTurn();manager.gameObject.SetActive(true);});}
            else if(a.phase=="ended")CampaignHUD.Instance.ShowReport(a.title??"Campaign ended",a.report??"This campaign has ended.",()=>SceneManager.LoadScene("Menu"));
        }
        if(!string.IsNullOrEmpty(SaveGames.LastError)&&CampaignHUD.Instance!=null)CampaignHUD.Instance.ShowReport("Save could not be loaded",SaveGames.LastError,()=>SceneManager.LoadScene("Menu"));
        Destroy(gameObject);
    }
}
