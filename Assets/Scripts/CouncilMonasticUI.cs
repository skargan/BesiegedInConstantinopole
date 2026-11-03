using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    void MonasticResearchPanel()
    {
        var card=MedicalCard("Monastic Research",new Vector2(.025f,.245f),new Vector2(.975f,.78f));
        MedicalText(card.transform,"Recover knowledge from manuscripts and neglected works.",new Vector2(.03f,.88f),new Vector2(.97f,.98f),22);
        MedicalText(card.transform,MonasticResearch.Progress+" / "+MonasticResearch.Rules.ProgressPerSecret+" toward the next Secret · +"+MonasticResearch.DailyProgress+" / day",new Vector2(.03f,.80f),new Vector2(.97f,.88f),22);
        var track=CampaignHUD.Panel(card.transform,"Research progress",new Vector2(.035f,.755f),new Vector2(.965f,.78f));
        track.GetComponent<Image>().color=new Color(.04f,.08f,.12f);track.GetComponent<Image>().raycastTarget=false;
        var fill=CampaignHUD.Panel(track.transform,"Knowledge recovered",Vector2.zero,new Vector2(Mathf.Clamp01((float)MonasticResearch.Progress/MonasticResearch.Rules.ProgressPerSecret),1));
        fill.GetComponent<Image>().color=ImperialTheme.Gold;fill.GetComponent<Image>().raycastTarget=false;
        string current=MonasticResearch.ResearchMonks==0?"Research paused · progress preserved":"Next discovery in "+MonasticResearch.DaysToDiscovery(MonasticResearch.ResearchMonks)+" days at the current allocation";
        MedicalText(card.transform,current,new Vector2(.03f,.675f),new Vector2(.97f,.75f),21);
        var pending=MedicalText(card.transform,MonasticResearch.HasPending?"From "+MonasticResearch.PendingDate.ToString("d MMM")+": "+MonasticResearch.PendingResearchMonks+" researching · "+(MonasticResearch.TotalMonks-MonasticResearch.PendingResearchMonks)+" in care":"No allocation change pending",new Vector2(.03f,.59f),new Vector2(.97f,.675f),21);
        pending.color=MonasticResearch.HasPending?ImperialTheme.Gold:ImperialTheme.Ivory;
        int selected=MonasticResearch.PendingResearchMonks;
        var preview=MedicalText(card.transform,"",new Vector2(.03f,.21f),new Vector2(.97f,.48f),22);
        Button apply=null;
        Action refresh=()=>{
            int capacity=MonasticResearch.TotalMonks-selected;
            string estimate=selected==0?"research paused":"next Secret in "+MonasticResearch.DaysToDiscovery(selected)+" days";
            preview.text="Preview: "+selected+" researching · +"+MonasticResearch.ProgressFor(selected)+" progress/day\nCare places: "+MonasticResearch.InfirmaryMonks+" → "+capacity+" · "+estimate+
                (MedicalRecovery.Total>capacity?"\n"+(MedicalRecovery.Total-capacity)+" patients would wait; their progress is preserved.":"");
            if(apply!=null)apply.interactable=selected!=MonasticResearch.PendingResearchMonks;
        };
        var slider=MonasticSlider(card.transform,selected,value=>{selected=Mathf.RoundToInt(value);refresh();});
        var sliderRect=slider.GetComponent<RectTransform>();sliderRect.anchorMin=new Vector2(.08f,.485f);sliderRect.anchorMax=new Vector2(.92f,.585f);
        apply=CampaignHUD.MakeButton(card.transform,"Apply allocation · tomorrow",new Vector2(.08f,.085f),new Vector2(.92f,.195f),()=>{
            if(MonasticResearch.ApplyAllocation(selected,out var report))InfirmaryPage(report);
            else {notice.text=report;notice.transform.parent.gameObject.SetActive(true);}
        });
        apply.name="Apply monk allocation";ImperialTheme.Button(apply);
        MedicalText(card.transform,"Free reassignment · recruits join the infirmary",new Vector2(.03f,.01f),new Vector2(.97f,.075f),19,TextAlignmentOptions.Center);
        refresh();
    }
    Slider MonasticSlider(Transform parent,int value,UnityEngine.Events.UnityAction<float> changed)
    {
        var root=CampaignHUD.Panel(parent,"Research monk allocation",new Vector2(.08f,.385f),new Vector2(.92f,.46f));
        root.GetComponent<Image>().color=Color.clear;
        var rail=CampaignHUD.Panel(root.transform,"Slider rail",new Vector2(0,.42f),new Vector2(1,.58f));rail.GetComponent<Image>().color=new Color(.18f,.23f,.28f);
        var fill=CampaignHUD.Panel(rail.transform,"Gold allocation",Vector2.zero,Vector2.one);fill.GetComponent<Image>().color=ImperialTheme.Gold;
        var handleArea=CampaignHUD.Panel(root.transform,"Handle area",Vector2.zero,Vector2.one);handleArea.GetComponent<Image>().color=Color.clear;
        var handle=CampaignHUD.Panel(handleArea.transform,"Monk allocation handle",new Vector2(0,.5f),new Vector2(0,.5f));
        handle.GetComponent<RectTransform>().sizeDelta=new Vector2(23,23);handle.GetComponent<Image>().color=ImperialTheme.Gold;
        var slider=root.AddComponent<Slider>();slider.wholeNumbers=true;slider.minValue=0;slider.maxValue=MonasticResearch.TotalMonks;
        slider.fillRect=fill.GetComponent<RectTransform>();slider.handleRect=handle.GetComponent<RectTransform>();slider.targetGraphic=handle.GetComponent<Image>();
        slider.SetValueWithoutNotify(value);slider.onValueChanged.AddListener(changed);slider.interactable=MonasticResearch.TotalMonks>0;
        return slider;
    }
    void MonasticTechnologySummary()
    {
        var row=SpyPanel("Monastic Research summary",0,155);
        MedicalText(row.transform,"MONASTIC RESEARCH\n"+MonasticResearch.Summary,new Vector2(.025f,.08f),new Vector2(.70f,.95f),22);
        var button=CampaignHUD.MakeButton(row.transform,"Infirmary and\nMonasteries",new Vector2(.74f,.15f),new Vector2(.975f,.85f),()=>{Infirmary();ControlSingleton.Instance.ReturnAction=Technologies;});
        ImperialTheme.Button(button);
    }
}
