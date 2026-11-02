using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    void StyleSpecialistButtons(){foreach(var button in screen.GetComponentsInChildren<Button>(true))ImperialTheme.Button(button);}
    int PendingSpecialists(string unit)=>TrainingPool.Orders.Where(o=>o!=null&&o.unit==unit&&o.kind!=TrainingPool.Recuperation).Sum(o=>o.quantity);
    TMP_Text SpecialistText(Transform parent,string text,Vector2 from,Vector2 to,int size=23)
    {
        var label=CampaignHUD.Label(parent,text,from,to,size);label.enableAutoSizing=true;label.fontSizeMin=18;label.fontSizeMax=size;return label;
    }
    GameObject SpecialistCard(string title,int column,int row)
    {
        var card=SpyPanel(title,row*304,290);var r=card.GetComponent<RectTransform>();
        r.anchorMin=new Vector2(column*.5f,1);r.anchorMax=new Vector2((column+1)*.5f,1);
        SpecialistText(card.transform,title,new Vector2(.04f,.83f),new Vector2(.96f,.98f),28);return card;
    }
    public void SpecialistRoster()
    {
        SpecialistPersonnel.EnsureInitialized();CountryDiplomacy.Ensure();
        Open("Specialists","Assign skilled people with care. An engineer cannot build, teach and repair at the same time.");
        ControlSingleton.Instance.ReturnAction=()=>{Hide();manager.BringUpTroopsUI();};BodyHeight(682);
        var engineers=SpecialistCard("ENGINEERS · "+SpecialistPersonnel.AvailableEngineers+" available",0,0);
        SpecialistText(engineers.transform,
            "Construction "+SpecialistPersonnel.ConstructionEngineers+" · Ship repairs "+SpecialistPersonnel.RepairingEngineers+
            "\nInstructors "+SpecialistPersonnel.Instructors+" · Recuperating "+SpecialistPersonnel.RecuperatingEngineers+
            "\nWounded "+SpecialistPersonnel.WoundedEngineers+" · Permanent crews "+SpecialistPersonnel.OperationalEngineers+
            "\nTrainees "+PendingSpecialists("iEngineers")+" · 250 gold, 10 days each",new Vector2(.04f,.29f),new Vector2(.96f,.81f),23);
        CampaignHUD.MakeButton(engineers.transform,"Train engineers",new Vector2(.04f,.05f),new Vector2(.96f,.25f),()=>SpecialistRecruitment("iEngineers"));
        var monks=SpecialistCard("MONKS · "+MedicalRecovery.Monks+" in service",1,0);
        SpecialistText(monks.transform,MonasticResearch.InfirmaryMonks+" tending the wounded · "+Math.Max(0,MedicalRecovery.Monks-MonasticResearch.InfirmaryMonks)+" researching"+
            "\n"+MedicalRecovery.Total+" patients · "+MedicalRecovery.FreePlaces+" free care places"+
            "\n"+MedicalRecovery.QueuedMonks+" recruits expected · five arrive in five days"+
            "\nFive monks cost 1,000 gold.",new Vector2(.04f,.29f),new Vector2(.96f,.81f),23);
        CampaignHUD.MakeButton(monks.transform,"Infirmary & monasteries",new Vector2(.04f,.05f),new Vector2(.59f,.25f),Infirmary);
        var recruitMonks=CampaignHUD.MakeButton(monks.transform,"Call five monks",new Vector2(.61f,.05f),new Vector2(.96f,.25f),()=>{
            if(MedicalRecovery.RecruitMonks(out var text)){SpecialistRoster();CampaignHUD.Instance.ShowReport("Monks summoned",text,()=>CampaignHUD.Instance.DismissReport());}else notice.text=text;
        });recruitMonks.interactable=CouncilActions.Int("iMoney")>=1000;
        var agents=SpecialistCard("AGENTS · "+CouncilActions.Int("iSpies")+" in the enemy camp",0,1);
        SpecialistText(agents.transform,ProvinceCampaign.Assigned+" assigned to provincial networks"+
            "\nRecruit a new camp contact for 1,000 gold."+
            "\nThis is an intelligence operation: it uses your major action and carries the current camp risk.",new Vector2(.04f,.29f),new Vector2(.96f,.81f),23);
        CampaignHUD.MakeButton(agents.transform,"Recruit camp agent",new Vector2(.04f,.05f),new Vector2(.58f,.25f),ReviewSpecialistAgent);
        CampaignHUD.MakeButton(agents.transform,"Intelligence",new Vector2(.61f,.05f),new Vector2(.96f,.25f),Spies);
        var envoys=SpecialistCard("ENVOYS · "+CountryDiplomacy.AvailableEnvoys+" available / "+CountryDiplomacy.TotalEnvoys+" total",1,1);
        SpecialistText(envoys.transform,(CountryDiplomacy.TotalEnvoys-CountryDiplomacy.AvailableEnvoys)+" travelling or serving abroad"+
            "\n"+PendingSpecialists("iEnvoys")+" new envoys in training"+
            "\n750 gold · one manpower · ten days each"+
            "\nResident envoys open deeper negotiations.",new Vector2(.04f,.29f),new Vector2(.96f,.81f),23);
        CampaignHUD.MakeButton(envoys.transform,"Train envoys",new Vector2(.04f,.05f),new Vector2(.58f,.25f),()=>SpecialistRecruitment("iEnvoys"));
        CampaignHUD.MakeButton(envoys.transform,"Diplomacy",new Vector2(.61f,.05f),new Vector2(.96f,.25f),DiplomaticRelations);
        var footer=SpyPanel("Recruitment commitments",612,64);
        SpecialistText(footer.transform,RecruitmentLogistics.AvailableManpower.ToString("N0")+" manpower available · Construction and specialist orders reserve their people immediately.",new Vector2(.02f,.1f),new Vector2(.69f,.9f),20);
        CampaignHUD.MakeButton(footer.transform,"Orders & return dates",new Vector2(.72f,.12f),new Vector2(.98f,.88f),Training);
        StyleSpecialistButtons();
    }
    void ReviewSpecialistAgent()
    {
        Open("Recruit a camp agent","A new contact among the Ottoman host can see what your soldiers cannot.");ControlSingleton.Instance.ReturnAction=Specialists;BodyHeight(340);
        var row=SpyPanel("Recruitment terms",0,320);
        SpecialistText(row.transform,"1,000 gold · one major action\nGain one camp agent immediately; the operation can still cost an agent."+
            "\nEnemy camp alert +"+EspionageSystem.AlertGain[0]+" · Agent loss risk "+EspionageSystem.LossChance(0).ToString("0.#")+"%",new Vector2(.04f,.38f),new Vector2(.96f,.94f),26);
        var confirm=CampaignHUD.MakeTurnButton(row.transform,"Confirm recruitment",new Vector2(.30f,.08f),new Vector2(.70f,.30f),()=>{bool ok=CouncilActions.SpyMission(0,false,out var report);Finish(ok,report);});confirm.interactable=CouncilActions.Int("iMoney")>=SpecialistPersonnel.AgentPrice;StyleSpecialistButtons();
    }
    public void SpecialistRecruitment(string unit)
    {
        bool engineer=unit=="iEngineers";if(!engineer&&unit!="iEnvoys")return;
        int price=TrainingPool.UnitPrice(unit,0),maximum=TrainingPool.MaximumOrder(unit,price);
        Open(engineer?"Train engineers":"Train envoys",engineer?"Organise skilled craftsmen and develop their apprentices.":"Prepare a new generation of messengers and resident diplomats.");
        ControlSingleton.Instance.ReturnAction=Specialists;BodyHeight(535);
        var detail=SpyPanel("Recruitment terms",0,200);
        string terms=engineer?"Each trainee: 250 gold, one manpower and an engineer instructor for ten days. The instructor returns with the graduate.":"Each envoy: 750 gold and one manpower. Ready in ten days; no instructor required.";
        if(engineer&&SpecialistPersonnel.Available==0)terms+="\nNo engineers are available. One trainee may learn without an instructor"+(SpecialistPersonnel.EmergencyTrainingActive?"; that exceptional place is already occupied.":".");
        SpecialistText(detail.transform,terms+"\nAvailable manpower: "+RecruitmentLogistics.AvailableManpower.ToString("N0")+" · Available engineers: "+SpecialistPersonnel.Available,new Vector2(.04f,.08f),new Vector2(.96f,.94f),24);
        var order=SpyPanel("Select recruits",214,300);TMP_Text amount=SpecialistText(order.transform,"",new Vector2(.04f,.74f),new Vector2(.96f,.96f),28);
        var preview=SpecialistText(order.transform,"",new Vector2(.04f,.27f),new Vector2(.96f,.55f),23);
        var confirm=CampaignHUD.MakeButton(order.transform,"Confirm training",new Vector2(.29f,.05f),new Vector2(.71f,.23f),()=>{});
        int count=maximum>0?1:0;
        Action refresh=()=>{amount.text=count+" "+TrainingPool.Name(unit).ToLowerInvariant()+" · maximum "+maximum;preview.text=count>0?(count*price).ToString("N0")+" gold · "+count+" manpower"+(engineer?" · "+(SpecialistPersonnel.Available==0?0:count)+" instructors":"")+"\nReady "+VariableSingleton.GetDate().AddDays(TrainingPool.Duration(unit)).ToString("d MMM yyyy"):"No recruits can be commissioned with the current resources.";confirm.interactable=count>0;};
        SpecialistQuantitySlider(order.transform,maximum,count,n=>{count=n;refresh();});
        bool submitted=false;confirm.onClick.AddListener(()=>{
            if(submitted)return;if(!TrainingPool.CanPlace(unit,count,price,out var reason)){notice.text=reason;return;}
            if(!TrainingPool.Place(unit,count,price)){notice.text="The order could not be placed. Review available personnel and resources.";return;}submitted=true;
            string text=count+" "+TrainingPool.Name(unit).ToLowerInvariant()+" commissioned. Ready "+VariableSingleton.GetDate().AddDays(TrainingPool.Duration(unit)).ToString("d MMM yyyy")+".\n"+preview.text+"\nYou may continue issuing orders.";
            SpecialistRoster();CampaignHUD.Instance.ShowReport("Training commissioned",text,()=>CampaignHUD.Instance.DismissReport());
        });refresh();StyleSpecialistButtons();
    }
    void SpecialistQuantitySlider(Transform parent,int maximum,int initial,Action<int> changed)
    {
        var area=CampaignHUD.Panel(parent,"Quantity selection",new Vector2(.16f,.58f),new Vector2(.84f,.71f));area.GetComponent<Image>().color=Color.clear;
        var track=CampaignHUD.Panel(area.transform,"Track",new Vector2(.015f,.39f),new Vector2(.985f,.61f));track.GetComponent<Image>().color=ImperialTheme.Navy;track.GetComponent<Image>().raycastTarget=false;
        var fill=CampaignHUD.Panel(track.transform,"Fill",Vector2.zero,Vector2.one);fill.GetComponent<Image>().color=ImperialTheme.Gold;fill.GetComponent<Image>().raycastTarget=false;
        var handle=CampaignHUD.Panel(area.transform,"Handle",Vector2.zero,Vector2.one);var hr=handle.GetComponent<RectTransform>();hr.anchorMin=hr.anchorMax=new Vector2(0,.5f);hr.sizeDelta=new Vector2(18,18);handle.GetComponent<Image>().color=ImperialTheme.Gold;
        var slider=area.AddComponent<Slider>();slider.wholeNumbers=true;slider.minValue=0;slider.maxValue=Math.Max(1,maximum);slider.fillRect=fill.GetComponent<RectTransform>();slider.handleRect=hr;slider.targetGraphic=handle.GetComponent<Image>();slider.SetValueWithoutNotify(initial);slider.interactable=maximum>0;
        slider.onValueChanged.AddListener(v=>changed(Math.Min(maximum,Mathf.RoundToInt(v))));
        var minus=CampaignHUD.MakeButton(parent,"−",new Vector2(.04f,.57f),new Vector2(.13f,.72f),()=>slider.value=Math.Max(0,slider.value-1));minus.interactable=maximum>0;
        var plus=CampaignHUD.MakeButton(parent,"+",new Vector2(.87f,.57f),new Vector2(.96f,.72f),()=>slider.value=Math.Min(maximum,slider.value+1));plus.interactable=maximum>0;
    }
    public void RepairDamagedShips()
    {
        Open("Repair damaged ships","Refit surviving hulls and replace lost crew. Their surviving crews stay with their ships.");ControlSingleton.Instance.ReturnAction=Fleet;BodyHeight(535);
        int maximum=TrainingPool.MaximumShipRepairs,count=maximum>0?1:0;
        var terms=SpyPanel("Ship repair terms",0,200);
        SpecialistText(terms.transform,SpecialistPersonnel.AvailableDamagedShips+" damaged hulls available · "+SpecialistPersonnel.RepairingShips+" already assigned\nEach ship: 250 gold, 5 materials, 10 replacement crew and 2 engineers for 5 days. Both engineers return when the ship is ready.",new Vector2(.04f,.08f),new Vector2(.96f,.94f),25);
        var order=SpyPanel("Select repairs",214,300);var amount=SpecialistText(order.transform,"",new Vector2(.04f,.74f),new Vector2(.96f,.96f),28);var preview=SpecialistText(order.transform,"",new Vector2(.04f,.27f),new Vector2(.96f,.55f),23);
        var confirm=CampaignHUD.MakeButton(order.transform,"Confirm repairs",new Vector2(.29f,.05f),new Vector2(.71f,.23f),()=>{});
        Action refresh=()=>{amount.text=count+" ships · maximum "+maximum;preview.text=count>0?(count*250).ToString("N0")+" gold · "+count*5+" materials · "+count*10+" replacement crew · "+count*2+" engineers\nReady "+VariableSingleton.GetDate().AddDays(5).ToString("d MMM yyyy"):"No repairs can be commissioned with the current hulls, personnel and resources.";confirm.interactable=count>0;};
        SpecialistQuantitySlider(order.transform,maximum,count,n=>{count=n;refresh();});bool submitted=false;
        confirm.onClick.AddListener(()=>{if(submitted)return;if(!TrainingPool.PlaceShipRepairs(count,out var text)){notice.text=text;return;}submitted=true;Fleet();CampaignHUD.Instance.ShowReport("Ship repairs commissioned",text+"\nYou may continue issuing orders.",()=>CampaignHUD.Instance.DismissReport());});refresh();StyleSpecialistButtons();
    }
}
