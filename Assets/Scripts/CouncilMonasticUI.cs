using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    void MonasticResearchPanel()
    {
        var card=SpyPanel("Monastic Research",615,620);ImperialTheme.Panel(card,false);
        MedicalText(card.transform,"MONASTIC RESEARCH",new Vector2(.035f,.91f),new Vector2(.965f,.98f),28,TextAlignmentOptions.Center);
        MedicalText(card.transform,"Monks compare manuscripts, translate neglected works and recover knowledge useful to the defence.",new Vector2(.035f,.80f),new Vector2(.965f,.91f),23,TextAlignmentOptions.Center);
        MedicalText(card.transform,MonasticResearch.TotalMonks+" monks · "+MonasticResearch.InfirmaryMonks+" tending the wounded · "+MonasticResearch.ResearchMonks+" researching",new Vector2(.04f,.735f),new Vector2(.96f,.80f),24,TextAlignmentOptions.Center);
        var track=CampaignHUD.Panel(card.transform,"Research progress",new Vector2(.05f,.68f),new Vector2(.95f,.72f));
        track.GetComponent<Image>().color=new Color(.04f,.08f,.12f);track.GetComponent<Image>().raycastTarget=false;
        var fill=CampaignHUD.Panel(track.transform,"Knowledge recovered",Vector2.zero,new Vector2(Mathf.Clamp01((float)MonasticResearch.Progress/MonasticResearch.Rules.ProgressPerSecret),1));
        fill.GetComponent<Image>().color=ImperialTheme.Gold;fill.GetComponent<Image>().raycastTarget=false;
        MedicalText(card.transform,MonasticResearch.Progress+" / "+MonasticResearch.Rules.ProgressPerSecret+" toward the next Secret.",new Vector2(.04f,.61f),new Vector2(.96f,.675f),23,TextAlignmentOptions.Center);
        string current=MonasticResearch.ResearchMonks==0?"No monks assigned—progress preserved.":"Current: +"+MonasticResearch.DailyProgress+" progress/day · next discovery in "+MonasticResearch.DaysToDiscovery(MonasticResearch.ResearchMonks)+" days.";
        MedicalText(card.transform,current,new Vector2(.04f,.55f),new Vector2(.96f,.615f),22,TextAlignmentOptions.Center);
        var pending=MedicalText(card.transform,MonasticResearch.HasPending?"Pending for "+MonasticResearch.PendingDate.ToString("d MMM")+": "+MonasticResearch.PendingResearchMonks+" researching, "+(MonasticResearch.TotalMonks-MonasticResearch.PendingResearchMonks)+" in the infirmary.":"No allocation change pending.",new Vector2(.04f,.48f),new Vector2(.96f,.55f),22,TextAlignmentOptions.Center);
        pending.color=MonasticResearch.HasPending?ImperialTheme.Gold:ImperialTheme.Ivory;
        int selected=MonasticResearch.PendingResearchMonks;
        var preview=MedicalText(card.transform,"",new Vector2(.04f,.21f),new Vector2(.96f,.37f),23,TextAlignmentOptions.Center);
        Button apply=null;
        Action refresh=()=>{
            int capacity=MonasticResearch.TotalMonks-selected;
            string estimate=selected==0?"research paused":"next discovery in "+MonasticResearch.DaysToDiscovery(selected)+" days at this allocation";
            preview.text="Preview: "+selected+" researching · +"+MonasticResearch.ProgressFor(selected)+" progress/day\nInfirmary: "+MonasticResearch.InfirmaryMonks+" → "+capacity+" care places · "+estimate+
                (MedicalRecovery.Total>capacity?"\n"+(MedicalRecovery.Total-capacity)+" patients would wait each day; their progress is preserved.":"");
            if(apply!=null)apply.interactable=selected!=MonasticResearch.PendingResearchMonks;
        };
        var slider=MonasticSlider(card.transform,selected,value=>{selected=Mathf.RoundToInt(value);refresh();});
        apply=CampaignHUD.MakeButton(card.transform,"Apply allocation · begins tomorrow",new Vector2(.17f,.095f),new Vector2(.83f,.195f),()=>{
            if(MonasticResearch.ApplyAllocation(selected,out var report)){InfirmaryPage(report);Canvas.ForceUpdateCanvases();body.parent.GetComponent<ScrollRect>().verticalNormalizedPosition=0;}
            else notice.text=report;
        });
        apply.name="Apply monk allocation";ImperialTheme.Button(apply);
        MedicalText(card.transform,"Free reassignment · no major action · new recruits join the infirmary",new Vector2(.04f,.02f),new Vector2(.96f,.085f),20,TextAlignmentOptions.Center);
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
