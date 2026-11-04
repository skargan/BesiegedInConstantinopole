using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Selection is presentation state only; TechnologySystem remains authoritative.
public partial class CouncilUI
{
    int selectedTechnology;
    void ShowTechnologyDesk()
    {
        Open("Technologies", "");
        SceneBackdrop.Apply(screen,"technologies",1f); // Keep the courier and pigeon in view on ultrawide screens.
        foreach(Transform child in screen.transform)
            if(child.name!="Scene artwork"&&child.name!="Imperial goldwork")child.gameObject.SetActive(false);
        var desk=CampaignHUD.Panel(screen.transform,"Technology desk",new Vector2(.505f,.025f),new Vector2(.985f,.975f));
        ImperialTheme.Panel(desk,false);
        MedicalText(desk.transform,"TECHNOLOGIES",new Vector2(.025f,.91f),new Vector2(.63f,.985f),30);
        TechnologySecretsCounter.Attach(desk.transform);
        var counter=desk.transform.Find("Available research secrets").GetComponent<RectTransform>();
        counter.anchorMin=new Vector2(.68f,.895f);counter.anchorMax=new Vector2(.975f,.985f);

        var list=CampaignHUD.Panel(desk.transform,"Technology list",new Vector2(.025f,.19f),new Vector2(.365f,.88f));
        list.GetComponent<Image>().color=ImperialTheme.Navy;
        selectedTechnology=Mathf.Clamp(selectedTechnology,0,TechnologySystem.All.Length-1);
        for(int i=0;i<TechnologySystem.All.Length;i++){
            int index=i;var item=TechnologySystem.All[i];bool active=VariableSingleton.GetBoolVariable(item.Key);
            float top=1f-(float)i/TechnologySystem.All.Length, bottom=1f-(float)(i+1)/TechnologySystem.All.Length;
            var button=CampaignHUD.MakeButton(list.transform,item.Name,new Vector2(0,bottom+.003f),new Vector2(1,top-.003f),()=>{
                selectedTechnology=index;ShowTechnologyDesk();
            });
            button.name="Select technology "+i;ImperialTheme.Button(button);
            var label=button.GetComponentInChildren<TMP_Text>();label.fontSize=21;label.enableAutoSizing=true;label.fontSizeMin=18;label.fontSizeMax=21;
            label.rectTransform.anchorMin=new Vector2(.035f,.08f);label.rectTransform.anchorMax=new Vector2(.965f,.92f);
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            // The shared button's six-unit vertical padding is too tall for catalogue rows.
            label.margin=new Vector4(6,0,6,0);
            if(active)label.color=new Color(.62f,.82f,.67f);
            if(i==selectedTechnology){button.GetComponent<Image>().color=new Color(.28f,.22f,.10f);label.color=ImperialTheme.Gold;}
        }
        var detail=CampaignHUD.Panel(desk.transform,"Selected technology",new Vector2(.385f,.19f),new Vector2(.975f,.88f));
        ImperialTheme.Panel(detail,false);
        var tech=TechnologySystem.All[selectedTechnology];bool unlocked=VariableSingleton.GetBoolVariable(tech.Key);
        MedicalText(detail.transform,tech.Name,new Vector2(.04f,.885f),new Vector2(.96f,.985f),28,TextAlignmentOptions.Center);
        TechnologyArtwork.Attach(detail.transform,tech.Key,new Vector2(.035f,.49f),new Vector2(.965f,.88f));
        var description=MedicalText(detail.transform,tech.Effect+"\n\n<color=#B8CFDA><i>"+tech.History+"</i></color>",new Vector2(.045f,.16f),new Vector2(.955f,.48f),22);
        description.name="Technology description";description.fontSizeMin=18;
        int choice=selectedTechnology;bool submitted=false;
        string caption=unlocked?"Active":CouncilActions.Int("iSecrets")<tech.Cost?"Requires "+tech.Cost+" secrets":"Research · "+tech.Cost+" secrets";
        var research=CampaignHUD.MakeTurnButton(detail.transform,caption,new Vector2(.06f,.025f),new Vector2(.94f,.14f),()=>{
            if(submitted)return;submitted=true;
            bool ok=TechnologySystem.Unlock(choice,out var report);Finish(ok,report);
            if(!ok)submitted=false;
        });
        research.name="Research selected technology";research.interactable=!unlocked&&CouncilActions.Int("iSecrets")>=tech.Cost;ImperialTheme.Button(research);
        var summary=MedicalText(desk.transform,MonasticResearch.ResearchMonks+" monks researching · +"+MonasticResearch.DailyProgress+" progress/day\n"+MonasticResearch.Progress+" / "+MonasticResearch.Rules.ProgressPerSecret+" toward the next secret",new Vector2(.025f,.105f),new Vector2(.63f,.18f),20);
        summary.name="Technology monastery summary";
        var monastery=CampaignHUD.MakeButton(desk.transform,"Monasteries",new Vector2(.66f,.105f),new Vector2(.975f,.18f),()=>{
            Infirmary();ControlSingleton.Instance.ReturnAction=Technologies;
        });ImperialTheme.Button(monastery);monastery.name="Technology monasteries";
        var back=CampaignHUD.MakeButton(desk.transform,"Return",new Vector2(.25f,.025f),new Vector2(.75f,.085f),()=>ControlSingleton.Instance.GoBack());
        ImperialTheme.Button(back);back.name="Technology return";
        // Retain a visible error surface for a balance change between opening and confirming.
        notice=MedicalText(screen.transform,"",new Vector2(.025f,.025f),new Vector2(.425f,.12f),23);
    }
}
