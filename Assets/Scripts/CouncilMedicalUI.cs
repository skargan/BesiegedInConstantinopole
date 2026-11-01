using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    public void Infirmary() => InfirmaryPage("");

    private void InfirmaryPage(string message)
    {
        const string title="The infirmary";
        Open(title,"Hold the position and monks can rescue up to half its casualties, if places are free. Recovery takes fifteen treatment days.");
        ControlSingleton.Instance.ReturnAction=Training;

        // A single page gives the strength of the service and the cost of its work.
        var scroll=body.parent.GetComponent<ScrollRect>();
        if(scroll.verticalScrollbar!=null)scroll.verticalScrollbar.gameObject.SetActive(false);
        body.parent.gameObject.SetActive(false);
        foreach(var label in screen.GetComponentsInChildren<TMP_Text>())
            if(label.transform.parent==screen.transform && label!=notice && label.text!="Return") {
                label.alignment=TextAlignmentOptions.Center;
                label.enableAutoSizing=true;
                label.fontSizeMin=label.text==title?28:20;
                label.fontSizeMax=label.fontSize;
            }
        notice.text=message;
        notice.enableAutoSizing=true;notice.fontSizeMin=18;notice.fontSizeMax=21;

        MedicalMetric(.035f,.335f,"MONKS",MedicalRecovery.Monks.ToString("N0"),"One care place per monk");
        MedicalMetric(.350f,.650f,"WOUNDED IN CARE",MedicalRecovery.Total.ToString("N0"),MedicalRecovery.FreePlaces.ToString("N0")+" free of "+MedicalRecovery.Capacity.ToString("N0")+" places");
        MedicalMetric(.665f,.965f,"TREATMENT PROVISIONS",MedicalRecovery.DailyProvisionNeed.ToString("N0")+" / day","1 provision per 5 patients treated");

        var patients=MedicalCard("Wounded roster",new Vector2(.035f,.285f),new Vector2(.600f,.645f));
        MedicalText(patients.transform,"UNIT",new Vector2(.045f,.81f),new Vector2(.49f,.96f),22);
        MedicalText(patients.transform,"IN CARE",new Vector2(.50f,.81f),new Vector2(.72f,.96f),22,TextAlignmentOptions.Right);
        MedicalText(patients.transform,"RETURNED",new Vector2(.74f,.81f),new Vector2(.955f,.96f),22,TextAlignmentOptions.Right);
        string[] units={"iInfantry","iArchers","iCavalry","iVarangians"};
        string[] names={"Infantry","Archers","Cavalry","Varangian Guard"};
        for(int i=0;i<units.Length;i++) {
            float top=.80f-i*.15f;
            MedicalText(patients.transform,names[i],new Vector2(.045f,top-.14f),new Vector2(.49f,top),25);
            var wounded=MedicalText(patients.transform,MedicalRecovery.CountFor(units[i]).ToString("N0"),new Vector2(.50f,top-.14f),new Vector2(.72f,top),27,TextAlignmentOptions.Right);
            wounded.color=ImperialTheme.Ivory;
            MedicalText(patients.transform,MedicalRecovery.RecoveredFor(units[i]).ToString("N0"),new Vector2(.74f,top-.14f),new Vector2(.955f,top),25,TextAlignmentOptions.Right);
        }
        var rosterNote=MedicalText(patients.transform,"Men in care cannot fight or be recruited again. Returned totals cover this campaign.",new Vector2(.045f,.03f),new Vector2(.955f,.18f),20);
        rosterNote.fontStyle=FontStyles.Italic;

        var treatment=MedicalCard("Recovery outlook",new Vector2(.620f,.285f),new Vector2(.965f,.645f));
        MedicalText(treatment.transform,"RECOVERY",new Vector2(.06f,.80f),new Vector2(.94f,.96f),23,TextAlignmentOptions.Center);
        string next=MedicalRecovery.Total==0?"No patients awaiting recovery":MedicalRecovery.SoonestReturnDays+" treatment days to first return";
        MedicalText(treatment.transform,next,new Vector2(.06f,.64f),new Vector2(.94f,.81f),25,TextAlignmentOptions.Center);
        var care=MedicalText(treatment.transform,MedicalRecovery.TreatmentSummary,new Vector2(.06f,.25f),new Vector2(.94f,.63f),22);
        care.fontStyle=FontStyles.Italic;
        var report=CampaignHUD.MakeButton(treatment.transform,"Latest infirmary report",new Vector2(.06f,.055f),new Vector2(.94f,.225f),()=>{
            CampaignHUD.Instance.ShowReport("The infirmary",MedicalRecovery.LastReport,()=>{CampaignHUD.Instance.DismissReport();Infirmary();});
        });
        report.name="Infirmary latest report";report.interactable=!string.IsNullOrEmpty(MedicalRecovery.LastReport);ImperialTheme.Button(report);

        var recruitment=MedicalCard("Monastic service",new Vector2(.035f,.115f),new Vector2(.965f,.265f));
        MedicalText(recruitment.transform,"CALL FOR MORE MONKS",new Vector2(.025f,.59f),new Vector2(.635f,.93f),24);
        MedicalText(recruitment.transform,"Five monks · 1,000 gold · ready in five days\n"+MedicalRecovery.QueuedMonks.ToString("N0")+" on their way.",new Vector2(.025f,.08f),new Vector2(.635f,.59f),22);
        bool submitted=false;
        bool affordable=CouncilActions.Int("iMoney")>=1000;
        var recruit=CampaignHUD.MakeButton(recruitment.transform,affordable?"Call five monks · 1,000 gold":"Requires 1,000 gold",new Vector2(.67f,.18f),new Vector2(.975f,.83f),()=>{
            if(submitted)return;submitted=true;
            if(MedicalRecovery.RecruitMonks(out var result))InfirmaryPage(result);
            else {submitted=false;notice.text=result;}
        });
        recruit.name="Recruit five monks";recruit.interactable=affordable;ImperialTheme.Button(recruit);
    }

    private GameObject MedicalCard(string name,Vector2 min,Vector2 max)
    {
        var panel=CampaignHUD.Panel(screen.transform,name,min,max);
        ImperialTheme.Panel(panel,false);
        return panel;
    }

    private void MedicalMetric(float left,float right,string title,string value,string detail)
    {
        var card=MedicalCard(title,new Vector2(left,.67f),new Vector2(right,.805f));
        MedicalText(card.transform,title,new Vector2(.04f,.68f),new Vector2(.96f,.95f),20,TextAlignmentOptions.Center);
        var amount=MedicalText(card.transform,value,new Vector2(.04f,.28f),new Vector2(.96f,.70f),36,TextAlignmentOptions.Center);
        amount.color=ImperialTheme.Ivory;
        MedicalText(card.transform,detail,new Vector2(.04f,.04f),new Vector2(.96f,.30f),20,TextAlignmentOptions.Center);
    }

    private TMP_Text MedicalText(Transform parent,string text,Vector2 min,Vector2 max,float size,TextAlignmentOptions alignment=TextAlignmentOptions.Left)
    {
        var label=CampaignHUD.Label(parent,text,min,max,size);
        label.alignment=alignment;label.enableAutoSizing=true;
        label.fontSizeMin=Mathf.Max(18,size-3);label.fontSizeMax=size;
        return label;
    }
}
