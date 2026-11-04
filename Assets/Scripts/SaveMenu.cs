using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public static class SaveMenu {
    static GameObject panel,overlay;
    static Transform Surface(){overlay=new GameObject("Archive overlay",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var c=overlay.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=250;var scale=overlay.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.matchWidthOrHeight=.5f;var shade=CampaignHUD.Panel(overlay.transform,"Archive shade",Vector2.zero,Vector2.one);shade.GetComponent<Image>().color=new Color(0,0,0,.65f);overlay.AddComponent<ArchiveHotkeys>();return overlay.transform;}

    public static bool Close(){if(overlay==null)return false;UnityEngine.Object.Destroy(overlay);overlay=null;panel=null;return true;}
    public static void StartCampaign(bool ironman){Close();SaveGames.PrepareNew(ironman);VariableSingleton.ContinueRequested=false;VariableSingleton.QuickStartRequested=false;SceneManager.LoadScene("Continue Game");}
    public static void NewCampaign(Transform parent){
        Close();parent=Surface();panel=CampaignHUD.Panel(parent,"Choose campaign mode",new Vector2(.20f,.12f),new Vector2(.80f,.88f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
        var title=CampaignHUD.Label(panel.transform,"Begin your reign",new Vector2(.06f,.84f),new Vector2(.94f,.96f),38);title.alignment=TextAlignmentOptions.Center;
        var normal=CampaignHUD.MakeButton(panel.transform,"Normal campaign",new Vector2(.20f,.66f),new Vector2(.80f,.77f),()=>StartCampaign(false));
        CampaignChoiceTooltip.Attach(normal,"Normal campaign starts in 1452 giving you time to prepare the city as you see fit before the siege itself starts. Achievements disabled.");
        var siege=CampaignHUD.MakeButton(panel.transform,"Start at the Siege",new Vector2(.20f,.49f),new Vector2(.80f,.60f),()=>StartAtSiege(parent));
        siege.interactable=VariableSingleton.HasPreparationPreset;
        CampaignChoiceTooltip.Attach(siege,SiegeStartExplanation());
        var ironman=CampaignHUD.MakeButton(panel.transform,"Ironman · achievements",new Vector2(.20f,.32f),new Vector2(.80f,.43f),()=>StartCampaign(true));
        CampaignChoiceTooltip.Attach(ironman,"Ironman: one rolling autosave, no manual slots or earlier checkpoints. Achievements enabled. Good luck!");
        CampaignHUD.MakeButton(panel.transform,"Cancel",new Vector2(.35f,.08f),new Vector2(.65f,.18f),()=>Close());
    }
    static string SiegeStartExplanation()=>VariableSingleton.HasPreparationPreset
        ?"Begin at the Siege, skipping all 1452/1453 monthly preparation turns, and using choices from your latest completed preparations from the previous playthrough. This option gets you straight into the fighting, minimizing time spent strategizing. Useful for advanced players, especially if you want to rerun the siege using same preparation conditions."
        :"Complete the preparation turns once to unlock Start at the Siege.";
    public static void StartAtSiege(Transform parent){
        Close();parent=Surface();panel=CampaignHUD.Panel(parent,"Start at the Siege",new Vector2(.20f,.17f),new Vector2(.80f,.83f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
        var title=CampaignHUD.Label(panel.transform,"Start at the Siege",new Vector2(.06f,.82f),new Vector2(.94f,.96f),38);title.alignment=TextAlignmentOptions.Center;
        var normal=CampaignHUD.MakeButton(panel.transform,"Normal campaign",new Vector2(.20f,.63f),new Vector2(.80f,.75f),()=>StartSiegeCampaign(false));
        var ironman=CampaignHUD.MakeButton(panel.transform,"Ironman · achievements",new Vector2(.20f,.46f),new Vector2(.80f,.58f),()=>StartSiegeCampaign(true));
        normal.interactable=ironman.interactable=VariableSingleton.HasPreparationPreset;
        CampaignChoiceTooltip.Attach(normal,SiegeStartExplanation());CampaignChoiceTooltip.Attach(ironman,SiegeStartExplanation());
        var explanation=CampaignHUD.Explanation(panel.transform,"Normal allows named saves. Ironman uses one rolling save and enables achievements.",new Vector2(.12f,.21f),new Vector2(.88f,.39f),24);explanation.alignment=TextAlignmentOptions.Center;
        CampaignHUD.MakeButton(panel.transform,"Back",new Vector2(.35f,.045f),new Vector2(.65f,.13f),()=>NewCampaign(parent));
    }
    public static void StartSiegeCampaign(bool ironman){
        if(!VariableSingleton.HasPreparationPreset)return;
        Close();SaveGames.PrepareNew(ironman);VariableSingleton.ContinueRequested=false;VariableSingleton.QuickStartRequested=true;SceneManager.LoadScene("Continue Game");
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

// Menu-only counterpart to the existing turn-action hint, without a time-cost emblem.
// Disabled choices remain hoverable so their unlock requirements are discoverable.
public sealed class CampaignChoiceTooltip:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler {
    public string Hint;GameObject tip;static CampaignChoiceTooltip owner;
    public static void Attach(Button button,string hint){button.gameObject.AddComponent<CampaignChoiceTooltip>().Hint=hint;}
    public void OnPointerEnter(PointerEventData e){Show();}public void OnPointerExit(PointerEventData e){Hide();}
    public void OnSelect(BaseEventData e){Show();}public void OnDeselect(BaseEventData e){Hide();}
    void OnDisable(){Hide();}void OnDestroy(){Hide();}
    void Hide(){if(tip!=null){tip.SetActive(false);Destroy(tip);tip=null;}if(owner==this)owner=null;}
    void Show(){
        Hide();if(owner!=null)owner.Hide();var canvas=GetComponentInParent<Canvas>();if(canvas==null)return;canvas=canvas.rootCanvas;owner=this;
        tip=new GameObject("Campaign choice explanation",typeof(RectTransform),typeof(Canvas),typeof(Image));tip.transform.SetParent(canvas.transform,false);
        var layer=tip.GetComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=700;
        tip.GetComponent<Image>().color=ImperialTheme.Navy;tip.GetComponent<Image>().raycastTarget=false;ImperialTheme.Frame(tip);
        var rect=tip.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        var parent=(RectTransform)canvas.transform;var area=parent.rect;const float margin=12,padding=18,gap=10;
        float width=Mathf.Min(560,area.width-2*margin);
        var label=CampaignHUD.Label(tip.transform,Hint,Vector2.zero,Vector2.one,21);label.alignment=TextAlignmentOptions.Center;label.enableWordWrapping=true;label.raycastTarget=false;
        float height=Mathf.Min(area.height-2*margin,Mathf.Max(64,label.GetPreferredValues(Hint,width-2*padding,float.PositiveInfinity).y+28));
        rect.sizeDelta=new Vector2(width,height);label.rectTransform.offsetMin=new Vector2(padding,14);label.rectTransform.offsetMax=new Vector2(-padding,-14);
        var corners=new Vector3[4];((RectTransform)transform).GetWorldCorners(corners);var lower=(Vector2)parent.InverseTransformPoint(corners[0]);var upper=(Vector2)parent.InverseTransformPoint(corners[2]);
        float x=(lower.x+upper.x)*.5f,y=lower.y-gap-height*.5f;if(y-height*.5f<area.yMin+margin)y=upper.y+gap+height*.5f;
        rect.anchoredPosition=new Vector2(Mathf.Clamp(x,area.xMin+margin+width*.5f,area.xMax-margin-width*.5f),Mathf.Clamp(y,area.yMin+margin+height*.5f,area.yMax-margin-height*.5f))-area.center;
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
