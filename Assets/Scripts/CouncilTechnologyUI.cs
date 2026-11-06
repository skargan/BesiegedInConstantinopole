using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Selection is presentation state only; TechnologySystem remains authoritative.
public partial class CouncilUI
{
    int selectedTechnology;
    void ShowTechnologyDesk()
    {
        Open("Research and Rediscovery", "");
        SceneBackdrop.Apply(screen,"technologies-compact",1f); // Keep the courier and pigeon in view on ultrawide screens.
        foreach(Transform child in screen.transform)
            if(child.name!="Scene artwork"&&child.name!="Imperial goldwork")child.gameObject.SetActive(false);
        TechnologyDiscoveryRoom.Attach(screen);
        var desk=CampaignHUD.Panel(screen.transform,"Technology desk",new Vector2(.505f,.025f),new Vector2(.985f,.975f));
        ImperialTheme.Panel(desk,false);

        var heading=MedicalText(desk.transform,"RESEARCH AND REDISCOVERY",new Vector2(.025f,.91f),new Vector2(.65f,.985f),26);
        heading.fontSizeMin=18;
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
        var detail=CampaignHUD.Panel(desk.transform,"Selected technology",new Vector2(.385f,.34f),new Vector2(.975f,.88f));
        ImperialTheme.Panel(detail,false);
        var tech=TechnologySystem.All[selectedTechnology];bool unlocked=VariableSingleton.GetBoolVariable(tech.Key);
        MedicalText(detail.transform,tech.Name,new Vector2(.04f,.85f),new Vector2(.96f,.985f),28,TextAlignmentOptions.Center);
        TechnologyArtwork.Attach(detail.transform,tech.Key,new Vector2(.035f,.025f),new Vector2(.965f,.84f));
        var description=MedicalText(desk.transform,tech.Effect+"\n\n<color=#B8CFDA><i>"+tech.History+"</i></color>",new Vector2(.395f,.115f),new Vector2(.965f,.33f),22);
        description.name="Technology description";description.fontSizeMin=18;
        int choice=selectedTechnology;bool submitted=false;
        string caption=unlocked?"Active":CouncilActions.Int("iSecrets")<tech.Cost?"Requires "+tech.Cost+" secrets":"Research · "+tech.Cost+" secrets";
        var research=CampaignHUD.MakeTurnButton(desk.transform,caption,new Vector2(.66f,.025f),new Vector2(.975f,.095f),()=>{
            if(submitted)return;submitted=true;
            bool ok=TechnologySystem.Unlock(choice,out var report);Finish(ok,report);
            if(!ok)submitted=false;
        });
        research.name="Research selected technology";research.interactable=!unlocked&&CouncilActions.Int("iSecrets")>=tech.Cost;ImperialTheme.Button(research);
        TechnologyResearchAvailability.Attach(research,choice);
        var summary=MedicalText(desk.transform,MonasticResearch.ResearchMonks+" monks researching\n+"+MonasticResearch.DailyProgress+"/day · "+MonasticResearch.Progress+"/"+MonasticResearch.Rules.ProgressPerSecret+" next secret",new Vector2(.025f,.105f),new Vector2(.365f,.18f),20);
        summary.name="Technology monastery summary";
        var monastery=CampaignHUD.MakeButton(desk.transform,"Monastic research",new Vector2(.025f,.025f),new Vector2(.325f,.095f),()=>{
            MonasticResearchView();ControlSingleton.Instance.ReturnAction=Technologies;
        });ImperialTheme.Button(monastery);monastery.name="Technology monasteries";
        var back=CampaignHUD.MakeButton(desk.transform,"Return",new Vector2(.35f,.025f),new Vector2(.635f,.095f),()=>ControlSingleton.Instance.GoBack());
        ImperialTheme.Button(back);back.name="Technology return";
        TechnologyPanelRail.Attach((RectTransform)desk.transform);
        // Retain a visible error surface for a balance change between opening and confirming.
        notice=MedicalText(screen.transform,"",new Vector2(.025f,.025f),new Vector2(.425f,.12f),23);
    }
}

// Discoveries can change affordability while the desk remains open.
public sealed class TechnologyResearchAvailability:MonoBehaviour
{
    Button button;int index,lastSecrets=int.MinValue;bool lastUnlocked;
    public static void Attach(Button button,int index){var view=button.gameObject.AddComponent<TechnologyResearchAvailability>();view.button=button;view.index=index;view.Update();}
    void Update(){if(!VariableSingleton.IsInitialized)return;var tech=TechnologySystem.All[index];int secrets=CouncilActions.Int("iSecrets");bool unlocked=VariableSingleton.GetBoolVariable(tech.Key);
        if(secrets==lastSecrets&&unlocked==lastUnlocked)return;lastSecrets=secrets;lastUnlocked=unlocked;
        button.interactable=!unlocked&&secrets>=tech.Cost;
        button.GetComponentInChildren<TMP_Text>().text=unlocked?"Active":secrets<tech.Cost?"Requires "+tech.Cost+" secrets":"Research · "+tech.Cost+" secrets";
    }
}
