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
    private bool resumed;
    private EventDecisionHistory.Recorder history;
    private string historyId;
    private string pendingHistoryParent;
    public void BeginEvent(string node)
    {
        resumed=false;Node=node;
        history=new EventDecisionHistory.Recorder(node,pendingHistoryParent);pendingHistoryParent=null;historyId=null;
        before=VariableSingleton.Instance.CaptureState();completed=false;title="Council decision";decision="";narrative="";chose=false;
    }
    public void ObserveLine(string text)
    {
        history?.Line(text);
        narrative+=(narrative.Length>0 ? "\n\n" : "")+text;
        var match=Regex.Match(text,@"<b>\s*(.*?)\s*</b>");
        if(match.Success) title=match.Groups[1].Value;
    }
    public void RecordChoice(string text) { history?.Choose(text);decision=Regex.Replace(text,"<[^>]+>",""); narrative="";chose=true; }
    public void ObserveOptions(Yarn.Unity.DialogueOption[] options){history?.Options(options);}
    public void PresentChoices() { if(chose) narrative=""; }
    public void FinishEvent()
    {
        if(completed || before==null) return;
        completed=true;
        PreparationCampaign.Complete(Node);
        string changes=EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState());
        historyId=history?.Finish(title,changes);
        string body=(decision.Length>0?"<i>"+decision+"</i>\n\n":"")+(narrative.Length>0?narrative+"\n\n":"")+changes;
        SaveGames.Checkpoint("event",Node,title+" — outcome",body);
        CampaignHUD.Instance.ShowReport(title+" — outcome",body,ContinueToAssault);MarkCalendarProgression();
    }
    public void ResumeCheckpoint(string node,string heading,string body){Node=node;completed=true;resumed=true;historyId=EventDecisionHistory.LatestId(node);CampaignHUD.Instance.ShowReport(heading,body,ContinueToAssault);MarkCalendarProgression();}
    void MarkCalendarProgression()
    {
        if(Node==ChurchCourt.ArrivalNode)return;
        // Follow-up dialogue is still part of this decision; only the final report progresses time.
        if((PreparationCampaign.Followup(Node)??SupplyEconomy.PendingNode??PoliticalDemands.PendingNode??SultanGifts.PendingNode)!=null)return;
        CampaignHUD.Instance.MarkReportAdvance(VariableSingleton.HasSiegeStarted()?"Proceed to the Ottoman siege activity.":"Proceed to the next month.");
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
        string followup=PreparationCampaign.Followup(Node) ?? SupplyEconomy.PendingNode ?? PoliticalDemands.PendingNode ?? SultanGifts.PendingNode;
        if(followup!=null && runner!=null) {pendingHistoryParent=historyId;if(resumed){resumed=false;SetStartNode(followup);gameObject.SetActive(true);}else{BeginEvent(followup);runner.StartDialogue(followup);}return;}
        if(Node==ChurchCourt.ArrivalNode && PreparationCampaign.Active && VariableSingleton.Turn==9 && runner!=null){PreparationMenu.Show(transform,node=>{resumed=false;BeginEvent(node);runner.StartDialogue(node);});return;}
        if(VariableSingleton.HasSiegeStarted()) SwitchPhase(assaultUI);
        else {VariableSingleton.IncrementTurn();SwitchPhase(managerUI);}
    }
    public void SetStartNode(string nodeName) { dialogueController.SetStartNode(nodeName); }
}
