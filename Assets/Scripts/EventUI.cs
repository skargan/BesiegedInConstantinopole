using UnityEngine;
using Yarn.Unity;
using System.Text.RegularExpressions;

public class EventUI : UIWindowFam
{
    [SerializeField] GameObject assaultUI;
    [SerializeField] GameObject managerUI;
    [SerializeField] DialogueController dialogueController;
    private VariableSingleton.CampaignSave before;
    private bool completed;
    public string Node {get;private set;}
    private string title="Council decision";
    private string decision="";
    private string narrative="";
    private bool chose;
    public void BeginEvent(string node)
    {
        Node=node;
        before=VariableSingleton.Instance.CaptureState();completed=false;title="Council decision";decision="";narrative="";chose=false;
    }
    public void ObserveLine(string text)
    {
        narrative+=(narrative.Length>0 ? "\n\n" : "")+text;
        var match=Regex.Match(text,@"<b>\s*(.*?)\s*</b>");
        if(match.Success) title=match.Groups[1].Value;
    }
    public void RecordChoice(string text) { decision=Regex.Replace(text,"<[^>]+>",""); narrative="";chose=true; }
    public void PresentChoices() { if(chose) narrative=""; }
    public void FinishEvent()
    {
        if(completed || before==null) return;
        completed=true;
        PreparationCampaign.Complete(Node);
        string changes=EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState());
        CampaignHUD.Instance.ShowReport(title+" — outcome",(decision.Length>0 ? "<i>"+decision+"</i>\n\n" : "")+(narrative.Length>0 ? narrative+"\n\n" : "")+changes,ContinueToAssault);
    }
    public void ContinueToAssault()
    {
        var runner=GetComponentInChildren<DialogueRunner>(true);
        if(runner!=null && runner.IsDialogueRunning)
        {
            // The visible Continue control advances text; Enter is never required to unlock it.
            foreach(var line in GetComponentsInChildren<LineView>()) line.OnContinueClicked();
            return;
        }
        CampaignHUD.Instance.DismissReport();
        CampaignHUD.Instance.CheckEnding();
        if(CampaignHUD.Instance.HasModal) return;
        string followup=PreparationCampaign.Followup(Node) ?? SupplyEconomy.PendingNode ?? SultanGifts.PendingNode;
        if(followup!=null && runner!=null) {BeginEvent(followup);runner.StartDialogue(followup);return;}
        if(VariableSingleton.HasSiegeStarted()) SwitchPhase(assaultUI);
        else {VariableSingleton.IncrementTurn();SwitchPhase(managerUI);}
    }
    public void SetStartNode(string nodeName) { dialogueController.SetStartNode(nodeName); }
}
