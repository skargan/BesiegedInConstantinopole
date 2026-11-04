using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation only. MedicalRecovery and MonasticResearch remain authoritative.
public partial class CouncilUI
{
    int medicalTab;
    Transform medicalCanvas;
    public void Infirmary() { medicalTab=0; InfirmaryPage(""); }

    private void InfirmaryPage(string message)
    {
        Open("Infirmary and Monasteries", "");
        ControlSingleton.Instance.ReturnAction=Training;
        SceneBackdrop.Apply(screen,"infirmary");
        foreach(Transform child in screen.transform)
            if(child.name!="Scene artwork" && child.name!="Imperial goldwork")child.gameObject.SetActive(false);
        var desk=CampaignHUD.Panel(screen.transform,"Infirmary desk",new Vector2(.505f,.025f),new Vector2(.985f,.975f));
        ImperialTheme.Panel(desk,false);medicalCanvas=desk.transform;
        MedicalText(medicalCanvas,"INFIRMARY & MONASTERIES",new Vector2(.035f,.935f),new Vector2(.965f,.99f),29,TextAlignmentOptions.Center);
        MedicalText(medicalCanvas,MonasticResearch.TotalMonks.ToString("N0")+" monks  ·  "+MonasticResearch.InfirmaryMonks.ToString("N0")+" tending the wounded  ·  "+MonasticResearch.ResearchMonks.ToString("N0")+" researching",
            new Vector2(.035f,.87f),new Vector2(.965f,.935f),22,TextAlignmentOptions.Center);
        string[] tabs={"Infirmary","Monastic research"};
        for(int i=0;i<tabs.Length;i++) {
            int tab=i;float left=.035f+i*.475f;
            var button=CampaignHUD.MakeButton(medicalCanvas,tabs[i],new Vector2(left,.795f),new Vector2(left+.455f,.86f),()=>{medicalTab=tab;InfirmaryPage("");});
            button.name="Medical tab "+i;button.interactable=medicalTab!=i;ImperialTheme.Button(button);
        }
        if(medicalTab==0)MedicalTreatmentPanel();else MonasticResearchPanel();

        var recruitment=MedicalCard("Monastic service",new Vector2(.025f,.105f),new Vector2(.975f,.23f));
        MedicalText(recruitment.transform,"Five monks · 1,000 gold\nReady in five days · "+MedicalRecovery.QueuedMonks.ToString("N0")+" on their way",new Vector2(.025f,.08f),new Vector2(.60f,.94f),21);
        bool submitted=false;
        var recruit=CampaignHUD.MakeButton(recruitment.transform,"Call five monks",new Vector2(.625f,.17f),new Vector2(.975f,.83f),()=>{
            if(submitted)return;submitted=true;
            if(MedicalRecovery.RecruitMonks(out var result))InfirmaryPage(result);
            else {submitted=false;notice.text=result;notice.transform.parent.gameObject.SetActive(true);}
        });
        recruit.name="Recruit five monks";recruit.interactable=CouncilActions.Int("iMoney")>=1000;ImperialTheme.Button(recruit);
        var back=CampaignHUD.MakeButton(medicalCanvas,"Return",new Vector2(.24f,.025f),new Vector2(.76f,.09f),()=>ControlSingleton.Instance.GoBack());
        back.name="Medical return";ImperialTheme.Button(back);
        var noticePanel=CampaignHUD.Panel(screen.transform,"Medical feedback",new Vector2(.025f,.025f),new Vector2(.485f,.19f));
        ImperialTheme.Panel(noticePanel,false);
        notice=MedicalText(noticePanel.transform,message,new Vector2(.04f,.06f),new Vector2(.96f,.94f),23);
        noticePanel.SetActive(!string.IsNullOrEmpty(message));
    }

    void MedicalTreatmentPanel()
    {
        var metrics=MedicalCard("Treatment overview",new Vector2(.025f,.665f),new Vector2(.975f,.78f));
        MedicalText(metrics.transform,MedicalRecovery.Total.ToString("N0")+" in care  ·  "+MedicalRecovery.FreePlaces.ToString("N0")+" free / "+MedicalRecovery.Capacity.ToString("N0")+" places",
            new Vector2(.025f,.50f),new Vector2(.975f,.96f),24);
        MedicalText(metrics.transform,"Extra care: "+SharedProvisions.Format(MedicalRecovery.DailyProvisionNeed)+" provisions/day · 1 per 20 patients",new Vector2(.025f,.04f),new Vector2(.975f,.50f),21);
        var patients=MedicalCard("Wounded roster",new Vector2(.025f,.405f),new Vector2(.975f,.65f));
        MedicalText(patients.transform,"UNIT",new Vector2(.025f,.83f),new Vector2(.49f,.99f),20);
        MedicalText(patients.transform,"IN CARE",new Vector2(.50f,.83f),new Vector2(.72f,.99f),20,TextAlignmentOptions.Right);
        MedicalText(patients.transform,"RETURNED",new Vector2(.73f,.83f),new Vector2(.975f,.99f),20,TextAlignmentOptions.Right);
        string[] names={"Infantry","Archers","Cavalry","Varangian Guard","Engineers"};
        for(int i=0;i<MedicalRecovery.Units.Length;i++) {
            float top=.83f-i*.16f;string unit=MedicalRecovery.Units[i];
            MedicalText(patients.transform,names[i],new Vector2(.025f,top-.16f),new Vector2(.49f,top),22);
            MedicalText(patients.transform,MedicalRecovery.CountFor(unit).ToString("N0"),new Vector2(.50f,top-.16f),new Vector2(.72f,top),23,TextAlignmentOptions.Right);
            MedicalText(patients.transform,MedicalRecovery.RecoveredFor(unit).ToString("N0"),new Vector2(.73f,top-.16f),new Vector2(.975f,top),23,TextAlignmentOptions.Right);
        }
        string next=MedicalRecovery.Total==0?"No patients awaiting recovery":MedicalRecovery.SoonestReturnDays+" treatment days to first return";
        MedicalText(medicalCanvas,next,new Vector2(.045f,.35f),new Vector2(.955f,.40f),23);
        MedicalText(medicalCanvas,"15 treatment days · One care place per monk",new Vector2(.045f,.305f),new Vector2(.955f,.35f),20);
        var report=CampaignHUD.MakeButton(medicalCanvas,"Care details & report",new Vector2(.12f,.245f),new Vector2(.88f,.302f),()=>{
            CampaignHUD.Instance.ShowReport("The infirmary",
                "Hold the position and monks can rescue up to half its casualties, if places are free.\n\n"+
                MedicalRecovery.TreatmentSummary+"\n\nMen in care cannot fight or be recruited again. Returned totals cover this campaign.\n\n"+MedicalRecovery.LastReport,
                ()=>{CampaignHUD.Instance.DismissReport();InfirmaryPage("");});
        });report.name="Infirmary latest report";ImperialTheme.Button(report);
        // Shortage warnings belong in the overview, not behind the details control.
        int waiting=Mathf.Max(0,MedicalRecovery.Total-MedicalRecovery.TreatableToday);
        if(waiting>0) {
            var label=medicalCanvas.GetComponentsInChildren<TMP_Text>();
            foreach(var text in label)if(text.text=="15 treatment days · One care place per monk"){
                text.text=waiting.ToString("N0")+" waiting · insufficient monks or provisions";
                text.color=new Color(.94f,.66f,.38f);break;
            }
        }
    }
    private GameObject MedicalCard(string name,Vector2 min,Vector2 max)
    {
        var panel=CampaignHUD.Panel(medicalCanvas,name,min,max);ImperialTheme.Panel(panel,false);return panel;
    }
    private TMP_Text MedicalText(Transform parent,string text,Vector2 min,Vector2 max,float size,TextAlignmentOptions alignment=TextAlignmentOptions.Left)
    {
        var label=CampaignHUD.Label(parent,text,min,max,size);label.alignment=alignment;label.enableAutoSizing=true;
        label.fontSizeMin=Mathf.Max(18,size-3);label.fontSizeMax=size;return label;
    }
}
