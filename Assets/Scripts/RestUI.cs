using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Yarn.Unity;

public class RestUI : UIWindowFam
{
    [SerializeField] TMP_Text actionResultText;
    [SerializeField] GameObject eventPhaseUI;


    private GameObject resultPanel;
    private void OnEnable()
    {
        SaveGames.Checkpoint("action",null,"Order carried out",ActionResults.Instance.GetActionText());
        // Hide the legacy illustration, report and unbound 100% placeholder.
        foreach(Transform child in transform) child.gameObject.SetActive(false);
        if(resultPanel!=null) Destroy(resultPanel);
        resultPanel=CampaignHUD.Panel(transform,"Completed order",Vector2.zero,Vector2.one);
        ImperialTheme.Panel(resultPanel);
        var title=CampaignHUD.Label(resultPanel.transform,"Order carried out",new Vector2(.08f,.80f),new Vector2(.92f,.94f),34);title.alignment=TextAlignmentOptions.Center;
        var viewport=CampaignHUD.Panel(resultPanel.transform,"Order report",new Vector2(.07f,.24f),new Vector2(.91f,.77f));
        viewport.GetComponent<UnityEngine.UI.Image>().color=ImperialTheme.Surface;
        viewport.AddComponent<UnityEngine.UI.RectMask2D>();
        var report=CampaignHUD.Label(viewport.transform,ActionResults.Instance.GetActionText(),Vector2.zero,Vector2.one,25);report.alignment=TextAlignmentOptions.Top;
        report.margin=new Vector4(24,20,24,20);
        var scroll=viewport.AddComponent<UnityEngine.UI.ScrollRect>();scroll.horizontal=false;scroll.scrollSensitivity=35;
        scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=report.rectTransform;
        var rect=report.rectTransform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);
        Canvas.ForceUpdateCanvases();rect.sizeDelta=new Vector2(0,Mathf.Max(viewport.GetComponent<RectTransform>().rect.height,report.GetPreferredValues(report.text,viewport.GetComponent<RectTransform>().rect.width-48,9999).y+50));rect.anchoredPosition=Vector2.zero;
        CampaignHUD.AddScrollbar(scroll,resultPanel.transform,new Vector2(.92f,.24f),new Vector2(.94f,.77f));
        var button=CampaignHUD.MakeButton(resultPanel.transform,"Hear the next petition",new Vector2(.30f,.07f),new Vector2(.70f,.18f),ContinueToEvent);button.name="ButtonToContinue";
    }


    public void ContinueToEvent()
    {
        if(PreparationCampaign.Active&&PreparationCampaign.Flexible(VariableSingleton.Turn)) {
            PreparationMenu.Show(transform,node=>{eventPhaseUI.GetComponent<EventUI>().SetStartNode(node);SwitchPhase(eventPhaseUI);});return;
        }
        string nodeSelection;
        if (DailyLifeEvents.DueNode != null)
        {
            nodeSelection=DailyLifeEvents.DueNode;
            eventPhaseUI.GetComponent<EventUI>().SetStartNode(nodeSelection);
        }
        else if (SiegeHardships.DueNode != null)
        {
            nodeSelection=SiegeHardships.DueNode;
            eventPhaseUI.GetComponent<EventUI>().SetStartNode(nodeSelection);
        }
        else if (RecurringEvents.HarvestDue && !VariableSingleton.Instance.StoryScheduled())
        {
            nodeSelection="random8a";
            eventPhaseUI.GetComponent<EventUI>().SetStartNode(nodeSelection);
        }
        else if (VariableSingleton.Instance.StoryScheduled())
        {
            nodeSelection = PreparationCampaign.Active?PreparationCampaign.FixedNode(VariableSingleton.Turn):"story" + VariableSingleton.Turn;
            eventPhaseUI.GetComponent<EventUI>().SetStartNode(nodeSelection);
        }
        else
        {
            int eventId = VariableSingleton.Instance.RandomEventID();
            nodeSelection = eventId < 0 ? "quietDay" : "random" + eventId;
            var daily=DailyLifeEvents.Available().Concat(SiegeHardships.Available()).ToArray();
            int originalCount=Enumerable.Range(0,24).Count(RecurringEvents.Eligible);
            if(daily.Length>0 && UnityEngine.Random.Range(0,originalCount+daily.Length)<daily.Length)nodeSelection=daily[UnityEngine.Random.Range(0,daily.Length)];
            var optional=PreparationCampaign.OptionalNodes().Concat(CourtIntrigue.OptionalNodes()).ToArray();
            if(optional.Length>0&&UnityEngine.Random.Range(0,100)<25)nodeSelection=optional[UnityEngine.Random.Range(0,optional.Length)];
            eventPhaseUI.GetComponent<EventUI>().SetStartNode(nodeSelection);
        }
        Debug.Log("Node selected: " + nodeSelection);
        SwitchPhase(eventPhaseUI);
    }
}
