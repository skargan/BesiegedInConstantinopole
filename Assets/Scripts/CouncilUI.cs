using System;



using UnityEngine;



using UnityEngine.UI;



using TMPro;







// Adds working council screens without replacing the authored management/turn components.



public partial class CouncilUI : MonoBehaviour



{



    public const string BuildLabel="Version 36.13 · Courts of the Mediterranean";



    private GameObject screen;



    private ManagerUI manager;



    private Transform body;



    private TMP_Text notice;



    private bool paySecret;



    private TMP_Text briefing;







    public static CouncilUI For(ManagerUI owner)



    {



        var ui=owner.GetComponent<CouncilUI>();



        if(ui==null) ui=owner.gameObject.AddComponent<CouncilUI>();



        ui.manager=owner;



        return ui;



    }



    public void BuildManagementMenu()



    {



        // Preserve the original phase objects and callbacks; only replace the management navigation.



        foreach(var button in manager.GetComponentsInChildren<Button>(true)) button.gameObject.SetActive(false);



        foreach(var label in manager.GetComponentsInChildren<TMP_Text>(true)) label.gameObject.SetActive(false);



        Sprite source=null;



        foreach(var candidate in manager.transform.root.GetComponentsInChildren<Image>(true))



            if(candidate.sprite!=null && candidate.sprite.name=="Management") {source=candidate.sprite;break;}



        var backdrop=CampaignHUD.Panel(manager.transform,"Council backdrop",Vector2.zero,Vector2.one);



        ImperialTheme.Panel(backdrop);



        if(source!=null)



        {



            var portrait=CampaignHUD.Panel(manager.transform,"Emperor portrait",new Vector2(.29f,.025f),new Vector2(.71f,.84f));



            var picture=portrait.GetComponent<Image>();



            var region=source.rect;region.width*=.595f;



            picture.sprite=Sprite.Create(source.texture,region,new Vector2(.5f,.5f));



            picture.color=Color.white;picture.raycastTarget=false;



        }



        var rect=manager.GetComponent<RectTransform>();



        rect.anchorMin=new Vector2(0,0);rect.anchorMax=new Vector2(1,.87f);rect.offsetMin=rect.offsetMax=Vector2.zero;



        var background=manager.GetComponent<Image>(); if(background!=null) background.color=new Color(0,0,0,0);



        var heading=CampaignHUD.Label(manager.transform,"THE IMPERIAL COUNCIL",new Vector2(.30f,.85f),new Vector2(.70f,.98f),32);heading.alignment=TextAlignmentOptions.Center;



        var briefingPanel=CampaignHUD.Panel(manager.transform,"Council briefing",new Vector2(.30f,.04f),new Vector2(.70f,.23f));



        briefingPanel.GetComponent<Image>().color=new Color(.07f,.09f,.105f,.96f);



        briefing=CampaignHUD.Label(briefingPanel.transform,"",new Vector2(.05f,.05f),new Vector2(.95f,.96f),22);
        CouncilQuoteView.Attach(briefing);



        string[] labels={"Siege overview","Military and recruitment","Fleet","Spies and intelligence","The Palace","Rest","Defence of the walls","Society and politics","Diplomacy and court","Technologies","Treasury and market"};



        UnityEngine.Events.UnityAction[] actions={Overview,manager.BringUpTroopsUI,Fleet,Spies,Palace,manager.ContinueToRest,manager.BringUpSiegeUI,Factions,Diplomacy,Technologies,Market};



        for(int i=0;i<labels.Length;i++)



        {



            int row=i<6?i:i-6;float x=i<6?.025f:.72f;float top=.82f-row*.128f;



            var button=CampaignHUD.MakeButton(manager.transform,labels[i],new Vector2(x,top-.105f),new Vector2(x+.255f,top),actions[i]);



            CouncilEmblems.Add(button.transform,i);

            var caption=button.GetComponentInChildren<TMP_Text>();caption.rectTransform.anchorMin=new Vector2(.23f,.03f);caption.rectTransform.anchorMax=new Vector2(.96f,.97f);

            if(i==5) {button.name="ButtonToContinue";TurnActionButton.Mark(button);}



        }



        CampaignHUD.MakeButton(manager.transform,"Development Options",new Vector2(.72f,.075f),new Vector2(.975f,.18f),()=>AssaultPractice.Show(manager));
    }



    private string CityAtGlanceText()
    {
        string weakest="";float support=2f;
        for(int i=0;i<4;i++){float value=VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[i]);if(value<support){support=value;weakest=CouncilActions.FactionNames[i];}}
        string aid="Relief expeditions: none";int soonest=int.MaxValue,committed=0;
        for(int i=0;i<CouncilActions.Allies.Length;i++)if(DailyLifeEvents.I("iAidState"+i)==1){
            committed++;int days=DailyLifeEvents.I("iAidDays"+i);
            if(days<soonest){soonest=days;aid="Next relief: "+CouncilActions.Allies[i].Name+" · "+days+" days";}
        }
        return "Food remaining: "+SupplyEconomy.DaysLeft+" days · Daily consumption: "+SupplyEconomy.DailyNeed+" provisions\nLowest support: "+weakest+" "+Mathf.RoundToInt(support*100)+"%\n"+aid+(committed>1?" · "+committed+" expeditions committed":"");
    }

    public void Hide() { if(screen!=null) screen.SetActive(false); }



    private void Open(string title,string introduction)



    {



        if(screen!=null) {screen.SetActive(false);Destroy(screen);}



        screen=CampaignHUD.Panel(manager.transform.parent,"Council screen",new Vector2(.015f,.025f),new Vector2(.985f,.865f));



        screen.transform.SetAsLastSibling();



        ImperialTheme.Panel(screen);
        string sceneArt=SceneBackdrop.ForCouncil(title);
        if(sceneArt!=null){SceneBackdrop.Apply(screen,sceneArt);var shade=CampaignHUD.Panel(screen.transform,"Readable heading",new Vector2(0,.80f),Vector2.one);shade.GetComponent<Image>().color=new Color(.015f,.035f,.05f,.90f);shade.GetComponent<Image>().raycastTarget=false;}




        CampaignHUD.Label(screen.transform,title,new Vector2(.035f,.90f),new Vector2(.965f,.98f),34);



        CampaignHUD.Explanation(screen.transform,introduction,new Vector2(.035f,.81f),new Vector2(.965f,.90f),21);



        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.035f,.02f),new Vector2(.75f,.09f),21);



        var back=CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.79f,.02f),new Vector2(.965f,.09f),()=>ControlSingleton.Instance.GoBack());



        var viewport=CampaignHUD.Panel(screen.transform,"Council scroll",new Vector2(.035f,.11f),new Vector2(.945f,.79f));



        if(sceneArt!=null){var artRect=viewport.GetComponent<RectTransform>();artRect.anchorMax=new Vector2(.945f,.61f);if(title=="Diplomacy and court"){artRect.anchorMin=new Vector2(.57f,.11f);artRect.anchorMax=new Vector2(.945f,.77f);}}
        viewport.AddComponent<RectMask2D>();



        viewport.GetComponent<Image>().color=ImperialTheme.Navy;



        var content=new GameObject("Council content",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);



        var rect=content.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);rect.offsetMin=rect.offsetMax=Vector2.zero;



        body=content.transform;



        var scroll=viewport.AddComponent<ScrollRect>();scroll.content=rect;scroll.viewport=viewport.GetComponent<RectTransform>();scroll.horizontal=false;scroll.scrollSensitivity=45;scroll.movementType=ScrollRect.MovementType.Clamped;



        CampaignHUD.AddScrollbar(scroll,screen.transform,new Vector2(.95f,.11f),new Vector2(.965f,.79f));



        ControlSingleton.Instance.ClearHistory();



        ControlSingleton.Instance.LogToEsc(manager.gameObject,screen);



        manager.gameObject.SetActive(false);



    }



    private void BodyHeight(float height) { ((RectTransform)body).sizeDelta=new Vector2(0,height); }



    private GameObject Row(int index,float height)



    {



        var row=CampaignHUD.Panel(body,"Council row",Vector2.zero,Vector2.one);



        var r=row.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);



        r.offsetMin=new Vector2(8,-(index+1)*height+6);r.offsetMax=new Vector2(-8,-index*height-6);



        row.GetComponent<Image>().color=ImperialTheme.Surface;ImperialTheme.Frame(row);



        return row;



    }



    private void Finish(bool success,string result)



    {



        if(!success) {notice.text=result;return;}



        Hide();ActionResults.Instance.Message=result;manager.CompleteAction();



    }



    // Legacy navigation bindings now open the five civic measures directly.
    public void Factions()=>CivicRelations();

    public void CivicRelations()



    {



        Open("Society and politics","Army morale, public morale, noble support, clergy support and Galata relations. Audiences cost 300 gold and improve their measure by 3 points.");



        ControlSingleton.Instance.ReturnAction=Hide;

        BodyHeight(5*120);



        for(int i=0;i<5;i++)



        {



            int choice=i;var row=Row(i,120);



            CampaignHUD.Label(row.transform,CouncilActions.FactionNames[i]+" — "+VariableSingleton.GetPercentFloat(CouncilActions.FactionKeys[i]),new Vector2(.02f,.66f),new Vector2(.66f,.96f),27);



            CampaignHUD.Explanation(row.transform,CouncilActions.FactionDescriptions[i],new Vector2(.02f,.08f),new Vector2(.65f,.64f),21);



            if(i==4) {

                CampaignHUD.MakeButton(row.transform,"Send a gift",new Vector2(.67f,.12f),new Vector2(.82f,.88f),()=>ReviewFaction(4));

                CampaignHUD.MakeButton(row.transform,"Galata relations",new Vector2(.83f,.12f),new Vector2(.99f,.88f),Galata);

                continue;

            }

            var button=CampaignHUD.MakeButton(row.transform,CouncilActions.FactionOrders[i]+"\n300 gold · +3 points",new Vector2(.68f,.12f),new Vector2(.98f,.88f),()=>ReviewFaction(choice));



            button.interactable=CouncilActions.Int("iMoney")>=300 && VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[i])<1;



        }



    }



    public void ReviewFaction(int index)

    {

        if(index<0 || index>=CouncilActions.FactionKeys.Length) return;

        string[] descriptions={

            "Visit the defenders on the walls and reward their courage.",

            "Distribute relief among the families enduring the siege.",

            "Receive the noble households and strengthen their ties to the throne.",

            "Sponsor the painters and gilders working for the Great Church.",

            "Send a gift to the podestà and cultivate goodwill across the Golden Horn."

        };

        string[] confirms={"Confirm visit","Confirm relief","Confirm reception","Confirm commission","Confirm gift"};

        Open(CouncilActions.FactionOrders[index],descriptions[index]);

        // Replace the council list with a focused review; the return route is the society screen.

        foreach(Transform child in screen.transform) child.gameObject.SetActive(false);

        ControlSingleton.Instance.ReturnAction=index==4?(Action)Galata:Factions;

        var title=CampaignHUD.Label(screen.transform,CouncilActions.FactionOrders[index],new Vector2(.06f,.83f),new Vector2(.94f,.96f),34);title.alignment=TextAlignmentOptions.Center;

        var intro=CampaignHUD.Explanation(screen.transform,descriptions[index],new Vector2(.07f,.72f),new Vector2(.93f,.83f),24);intro.alignment=TextAlignmentOptions.Center;

        float support=VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[index]);

        float gain=Mathf.Min(3,(1-support)*100);

        ReviewCard(.06f,.48f,"COST","GOLD","−300","Current treasury: "+CouncilActions.Int("iMoney").ToString("N0"),new Color(.89f,.67f,.59f));

        ReviewCard(.52f,.94f,"BENEFIT",CouncilActions.FactionNames[index].ToUpperInvariant(),"+"+gain.ToString("0.#")+(Mathf.Abs(gain-1)<.001f?" point":" points"),"Current support: "+(support*100).ToString("0.#")+"%",new Color(.72f,.80f,.67f));


        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.1f,.01f),new Vector2(.9f,.08f),21);notice.alignment=TextAlignmentOptions.Center;

        CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.15f,.10f),new Vector2(.46f,.21f),()=>{if(index==4)Galata();else Factions();});

        bool submitted=false;

        var confirm=CampaignHUD.MakeTurnButton(screen.transform,confirms[index],new Vector2(.51f,.10f),new Vector2(.85f,.21f),()=>{

            if(submitted) return;

            submitted=true;

            bool ok=CouncilActions.SupportFaction(index,out var report);

            if(!ok) {submitted=false;notice.text=report;return;}

            Finish(true,report);

        });

        confirm.name="Confirm society action";

        confirm.interactable=CouncilActions.Int("iMoney")>=300 && support<1;

        if(!confirm.interactable) notice.text=support>=1?"Support is already at its maximum.":"You need 300 gold.";

    }



    private void ReviewCard(float left,float right,string kind,string resource,string change,string current,Color colour)

    {

        var card=CampaignHUD.Panel(screen.transform,"Review "+kind,new Vector2(left,.38f),new Vector2(right,.70f));

        card.GetComponent<Image>().color=ImperialTheme.Surface;ImperialTheme.Frame(card);

        var border=card.AddComponent<Outline>();border.effectColor=new Color(.50f,.40f,.23f);border.effectDistance=new Vector2(1,-1);

        var category=CampaignHUD.Label(card.transform,kind,new Vector2(.04f,.79f),new Vector2(.96f,.97f),20);category.alignment=TextAlignmentOptions.Center;category.color=new Color(.87f,.75f,.55f);

        var name=CampaignHUD.Label(card.transform,resource,new Vector2(.04f,.58f),new Vector2(.96f,.79f),27);name.alignment=TextAlignmentOptions.Center;

        var amount=CampaignHUD.Label(card.transform,change,new Vector2(.04f,.27f),new Vector2(.96f,.60f),40);amount.alignment=TextAlignmentOptions.Center;amount.color=colour;

        var holding=CampaignHUD.Label(card.transform,current,new Vector2(.04f,.04f),new Vector2(.96f,.25f),24);holding.alignment=TextAlignmentOptions.Center;

    }



    public void TroopUpgrades(int selected=0)

    {

        Open("Training and equipment","Drills improve your soldiers over five days. Technologies add their own bonuses. Troops remain ready to fight throughout training.");

        ControlSingleton.Instance.ReturnAction=()=>{Hide();manager.BringUpTroopsUI();};

        BodyHeight(1200);

        string[] technologyNames={"Roman Tactics","Upgraded Crossbows","Cataphract Armour"};

        string[] technologyKeys={"RomanTraining","UpgradedCrossbows","PlateArmour"};

        for(int i=0;i<3;i++) {

            int unit=i;var row=Row(i,300);int rank=TroopTraining.Rank(i);

            CampaignHUD.Label(row.transform,TroopTraining.Units[i]+" · "+TroopTraining.Ranks[rank],new Vector2(.03f,.82f),new Vector2(.69f,.98f),28);

            for(int j=0;j<5;j++) {

                var pip=CampaignHUD.Panel(row.transform,"Training level "+j,new Vector2(.03f+j*.128f,.69f),new Vector2(.15f+j*.128f,.77f));

                pip.GetComponent<Image>().color=j<=rank?new Color(.65f,.49f,.22f):new Color(.12f,.10f,.11f);

                var label=CampaignHUD.Label(pip.transform,TroopTraining.Ranks[j],Vector2.zero,Vector2.one,15);label.alignment=TextAlignmentOptions.Center;

            }

            string tech=technologyNames[i]+": "+(TechnologySystem.Has(technologyKeys[i])?"active (+25%)":"not adopted");

            if(i==0) tech+=" · Grenades: "+(TechnologySystem.Has("Grenades")?"+0.25":"not adopted");

            if(i==1) tech+=" · Handcannons: "+(TechnologySystem.Has("Handcannons")?"+0.5":"not adopted");

            if(i==2) tech+="\nDismounted Champions: "+(TechnologySystem.Has("DismountedChampions")?"active":"not adopted");

            CampaignHUD.Explanation(row.transform,tech,new Vector2(.03f,.40f),new Vector2(.69f,.66f),20);

            CampaignHUD.Label(row.transform,TroopTraining.Breakdown(i),new Vector2(.03f,.07f),new Vector2(.69f,.39f),22);

            string reason=TroopTraining.Unavailable(i);

            string text=TroopTraining.Due(i)>0?"Drills complete in\n"+Mathf.Max(0,TroopTraining.Due(i)-TrainingPool.Day(VariableSingleton.GetDate()))+" days":reason??"Upgrade training\n5,000 gold · 5 days";

            var button=CampaignHUD.MakeButton(row.transform,text,new Vector2(.73f,.20f),new Vector2(.97f,.77f),()=>ReviewTraining(unit));button.interactable=reason==null;

        }

        var specialists=Row(3,300);

        CampaignHUD.Label(specialists.transform,"Specialist forces",new Vector2(.03f,.78f),new Vector2(.97f,.96f),28);

        CampaignHUD.Explanation(specialists.transform,"Varangian Drills: "+(TechnologySystem.Has("VarangianDrills")?"active · lethality +20%":"not adopted")+"\nGrapeshot: "+(TechnologySystem.Has("Grapeshot")?"active · cannon lethality ×3.5":"not adopted")+"\nArtillery Ships: "+(TechnologySystem.Has("ArtilleryShips")?"active · defensive fleet reports inflict 200 casualties":"not adopted · defensive fleet reports inflict 100 casualties"),new Vector2(.03f,.15f),new Vector2(.70f,.76f),23);

        CampaignHUD.MakeButton(specialists.transform,"Technologies",new Vector2(.74f,.25f),new Vector2(.97f,.68f),Technologies);

        Canvas.ForceUpdateCanvases();

        var upgradeScroll=body.GetComponentInParent<ScrollRect>();upgradeScroll.verticalNormalizedPosition=1-Mathf.Clamp01(selected/3f);

    }



    public void ReviewTraining(int unit)

    {

        Open("Upgrade training","");

        foreach(Transform child in screen.transform) child.gameObject.SetActive(false);

        ControlSingleton.Instance.ReturnAction=()=>TroopUpgrades(unit);

        var title=CampaignHUD.Label(screen.transform,"Upgrade "+TroopTraining.Units[unit].ToLower()+" training",new Vector2(.06f,.83f),new Vector2(.94f,.96f),34);title.alignment=TextAlignmentOptions.Center;

        var intro=CampaignHUD.Explanation(screen.transform,"Experienced officers drill your troops for five days. They remain available for battle.",new Vector2(.07f,.72f),new Vector2(.93f,.83f),24);intro.alignment=TextAlignmentOptions.Center;

        ReviewCard(.06f,.48f,"COST","GOLD","−5,000","Current treasury: "+CouncilActions.Int("iMoney").ToString("N0"),new Color(.89f,.67f,.59f));

        ReviewCard(.52f,.94f,"TRAINING",TroopTraining.Ranks[Mathf.Min(4,TroopTraining.Rank(unit)+1)].ToUpperInvariant(),"×"+TroopTraining.Base(unit).ToString("0.##")+" → ×"+TroopTraining.Next(unit).ToString("0.##"),"Ready "+VariableSingleton.GetDate().AddDays(5).ToString("d MMM yyyy"),new Color(.72f,.80f,.67f));


        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.1f,.01f),new Vector2(.9f,.08f),21);

        CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.15f,.10f),new Vector2(.46f,.21f),()=>TroopUpgrades(unit));

        bool submitted=false;

        var button=CampaignHUD.MakeTurnButton(screen.transform,"Confirm training",new Vector2(.51f,.10f),new Vector2(.85f,.21f),()=>{if(submitted)return;bool ok=TroopTraining.Order(unit,out var report);if(ok)submitted=true;Finish(ok,report);});button.interactable=TroopTraining.Unavailable(unit)==null;

    }



    public void Training()
    {
        Open("Recruitment and recovery","");
        CampaignHUD.MakeButton(screen.transform,"Infirmary · "+MedicalRecovery.Total+" wounded",new Vector2(.62f,.87f),new Vector2(.96f,.95f),Infirmary);
        ControlSingleton.Instance.ReturnAction=()=>{Hide();manager.BringUpTroopsUI();};
        var orders=new System.Collections.Generic.List<TrainingPool.Order>(TrainingPool.Orders);orders.Sort((a,b)=>a.readyDay.CompareTo(b.readyDay));
        var lines=new System.Collections.Generic.List<string>();
        foreach(var o in orders)lines.Add(TrainingPool.OrderName(o)+" · "+o.quantity.ToString("N0")+"\nReady "+TrainingPool.ReadyDate(o).ToString("d MMM")+" · "+TrainingPool.Remaining(o)+" days"+(o.engineersReserved>0?" · "+o.engineersReserved+" engineers committed":""));
        for(int i=0;i<3;i++)if(TroopTraining.Due(i)>0){var ready=new DateTime((long)TroopTraining.Due(i)*TimeSpan.TicksPerDay);lines.Add(TroopTraining.Units[i]+" · advanced drills\nReady "+ready.ToString("d MMM"));}
        float height=Mathf.Max(620,90+lines.Count*100);BodyHeight(height+92);
        var queue=SpyPanel("Recruitment queue",0,height-8);var qr=queue.GetComponent<RectTransform>();qr.anchorMax=new Vector2(.51f,1);
        var stock=SpyPanel("Manpower and losses",0,612);var sr=stock.GetComponent<RectTransform>();sr.anchorMin=new Vector2(.51f,1);
        var heading=CampaignHUD.Label(queue.transform,"RECRUITMENT & CONSTRUCTION",Vector2.zero,Vector2.one,26);heading.rectTransform.anchorMin=new Vector2(.04f,1);heading.rectTransform.anchorMax=new Vector2(.96f,1);heading.rectTransform.offsetMin=new Vector2(0,-64);heading.rectTransform.offsetMax=new Vector2(0,-10);
        if(lines.Count==0)lines.Add("No orders in progress.");
        for(int i=0;i<lines.Count;i++){var label=CampaignHUD.Label(queue.transform,lines[i],Vector2.zero,Vector2.one,24);label.rectTransform.anchorMin=new Vector2(.04f,1);label.rectTransform.anchorMax=new Vector2(.96f,1);label.rectTransform.offsetMin=new Vector2(0,-164-i*100);label.rectTransform.offsetMax=new Vector2(0,-74-i*100);}
        CampaignHUD.Label(stock.transform,"AVAILABLE MANPOWER",new Vector2(.05f,.87f),new Vector2(.95f,.97f),26);
        CampaignHUD.Label(stock.transform,RecruitmentLogistics.AvailableManpower.ToString("N0"),new Vector2(.05f,.73f),new Vector2(.95f,.88f),42);
        CampaignHUD.Label(stock.transform,"Recruitment pool: "+RecruitmentLogistics.ManpowerPool.ToString("N0")+"\nServing: "+RecruitmentLogistics.Serving.ToString("N0")+" · In recruitment: "+RecruitmentLogistics.Queued.ToString("N0")+"\nRecovering: "+MedicalRecovery.RegularWounded.ToString("N0")+" regular soldiers\nSpecialists and crews committed: "+SpecialistPersonnel.LocalManpowerCommitted.ToString("N0"),new Vector2(.05f,.53f),new Vector2(.95f,.73f),21);
        CampaignHUD.Label(stock.transform,"LOSSES · INCLUDES WOUNDED",new Vector2(.05f,.43f),new Vector2(.95f,.53f),26);
        string[] keys={"iInfantry","iArchers","iCavalry","iVarangians"};string[] names={"Infantry","Archers","Cavalry","Varangian Guard"};
        for(int i=0;i<4;i++){float y=.40f-i*.09f;CampaignHUD.Label(stock.transform,names[i],new Vector2(.05f,y-.08f),new Vector2(.68f,y),24);var value=CampaignHUD.Label(stock.transform,RecruitmentLogistics.LossText(keys[i]),new Vector2(.70f,y-.08f),new Vector2(.95f,y),24);value.alignment=TextAlignmentOptions.Right;}
        var specialistLink=SpyPanel("Specialist recruitment",height,80);
        CampaignHUD.MakeButton(specialistLink.transform,"Specialists · engineers, monks, agents and envoys",new Vector2(.03f,.12f),new Vector2(.97f,.88f),Specialists);
    }

    private GameObject SpyPanel(string name,float top,float height)

    {

        var panel=CampaignHUD.Panel(body,name,Vector2.zero,Vector2.one);

        var rect=panel.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);

        rect.offsetMin=new Vector2(8,-top-height);rect.offsetMax=new Vector2(-8,-top);

        panel.GetComponent<Image>().color=ImperialTheme.Surface;ImperialTheme.Frame(panel);

        return panel;

    }

    public void Spies() { IntelligenceMenu(false); }

    public void ReviewDisruption()

    {

        bool useSecret=paySecret;

        Open("Disrupt Ottoman preparations",EspionageSystem.Descriptions[5]);

        foreach(Transform child in screen.transform) child.gameObject.SetActive(false);

        ControlSingleton.Instance.ReturnAction=Spies;

        var title=CampaignHUD.Label(screen.transform,"Disrupt Ottoman preparations",new Vector2(.06f,.83f),new Vector2(.94f,.96f),34);title.alignment=TextAlignmentOptions.Center;

        var intro=CampaignHUD.Explanation(screen.transform,EspionageSystem.Descriptions[5],new Vector2(.07f,.72f),new Vector2(.93f,.83f),24);intro.alignment=TextAlignmentOptions.Center;

        ReviewCard(.06f,.48f,"COST",useSecret?"SECRETS":"GOLD",useSecret?"−1":"−1,000","Available: "+CouncilActions.Int(useSecret?"iSecrets":"iMoney").ToString("N0"),new Color(.89f,.67f,.59f));

        ReviewCard(.52f,.94f,"BENEFIT","OPERATIONS DELAY","+1 siege day","Current delay: "+CouncilActions.Int("iEnemyDelay")+" days",new Color(.72f,.80f,.67f));

        var terms=CampaignHUD.Explanation(screen.transform,"Enemy camp alert +20 points · Agent loss risk "+EspionageSystem.LossChance(5).ToString("0.#")+"%\nDelays accumulate and begin taking effect during the siege.",new Vector2(.06f,.23f),new Vector2(.94f,.37f),22);terms.alignment=TextAlignmentOptions.Center;

        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.06f,.01f),new Vector2(.94f,.08f),21);

        CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.15f,.10f),new Vector2(.46f,.21f),Spies);

        bool submitted=false;

        var confirm=CampaignHUD.MakeTurnButton(screen.transform,"Confirm disruption",new Vector2(.51f,.10f),new Vector2(.85f,.21f),()=>{

            if(submitted)return;

            submitted=true;

            if(!CouncilActions.SpyMission(5,useSecret,out var report)){submitted=false;notice.text=report;return;}

            Finish(true,report);

        });

        confirm.name="Confirm spy disruption";

        confirm.interactable=CouncilActions.Int("iSpies")>0 && CouncilActions.Int(useSecret?"iSecrets":"iMoney")>=(useSecret?1:1000);

    }



    public void Fleet()

    {

        Open("The Imperial Fleet", "Command the sea: compare fleets, issue orders and inspect the harbour's defences.");

        int ships=CouncilActions.Int("iShips"),order=Mathf.Clamp(CouncilActions.Int("iFleet"),0,5);

        bool known=CouncilActions.Int("iSpies")>=2;

        string[] orders={"Raid Ottoman supplies","Raid coastal shipping","Bring grain from Crimea","Defend the Golden Horn","Bring volunteers","Awaiting new orders"};

        string[] effects={"Successful report: remove 3 days of Ottoman food stores.","Successful report: gain 750 gold.","Successful report: gain 150 provisions.","Successful report: inflict "+RecurringEvents.FleetDefensiveDamage+" enemy casualties.","Successful report: gain 50 infantry and 25 archers.","The previous fleet was lost. Issue fresh orders to the surviving or newly built ships."};

        var building=new System.Collections.Generic.List<string>();int queued=0;

        foreach(var o in TrainingPool.Orders) if(o.unit=="iShips") {if(o.kind!=TrainingPool.ShipRepair)queued+=o.quantity;building.Add(o.quantity+" "+(o.kind==TrainingPool.ShipRepair?"warships · Repairing":"warships · Building")+" · ready "+TrainingPool.ReadyDate(o).ToString("d MMM yyyy")+" ("+TrainingPool.Remaining(o)+" days)");}

        float yardHeight=Mathf.Max(240,150+building.Count*40);

        BodyHeight(1050+yardHeight);

        var strength=SpyPanel("Naval balance",0,170);

        FleetText(strength.transform,"IMPERIAL WARSHIPS",.02f,.66f,.32f,.94f,22);

        FleetText(strength.transform,ships.ToString(),.02f,.30f,.32f,.67f,38);

        FleetText(strength.transform,queued+" building · "+SpecialistPersonnel.RepairingShips+" repairing",.02f,.06f,.32f,.30f,20);

        FleetText(strength.transform,"CONTROL OF THE SEA",.34f,.66f,.66f,.94f,22);

        FleetText(strength.transform,NavalState.SeaControl,.34f,.30f,.66f,.67f,29);

        FleetText(strength.transform,"Uncontested when our ships outnumber theirs",.34f,.04f,.66f,.30f,18);

        FleetText(strength.transform,"OTTOMAN VESSELS",.68f,.66f,.98f,.94f,22);

        FleetText(strength.transform,known?NavalState.EnemyShips.ToString():"Unknown",.68f,.30f,.98f,.67f,38);

        FleetText(strength.transform,known?"+1/day · "+NavalState.ReplacementSource:"Requires 2 agents in the enemy camp",.68f,.04f,.98f,.30f,19);

        var controls=SpyPanel("Fleet command",184,90);

        CampaignHUD.MakeButton(controls.transform,"Change mission",new Vector2(.02f,.15f),new Vector2(.33f,.85f),FleetOrders);

        var battle=CampaignHUD.MakeButton(controls.transform,"Review naval battle",new Vector2(.35f,.15f),new Vector2(.66f,.85f),ReviewNavalBattle);

        battle.interactable=ships>0&&NavalState.EnemyShips>0;

        CampaignHUD.MakeButton(controls.transform,"Build warships",new Vector2(.68f,.15f),new Vector2(.98f,.85f),()=>{Hide();manager.BringUpTroopsUI();});

        var mission=SpyPanel("Current mission",288,180);

        FleetText(mission.transform,"CURRENT ORDERS · "+orders[order],.03f,.69f,.97f,.95f,27);

        FleetText(mission.transform,ships==0?"No ships available. New warships take 30 days to build.":effects[order],.03f,.37f,.97f,.68f,24);

        FleetText(mission.transform,"Routine report in "+FleetOperations.DaysRemaining+" active siege days. Reports follow a 14–21-day cycle; changing mission starts a new cycle.",.03f,.05f,.97f,.35f,20);

        var harbour=SpyPanel("Harbour and command",482,250);

        FleetText(harbour.transform,"HARBOUR DEFENCE",.03f,.79f,.97f,.96f,26);

        float bonus=ships>0&&order==3?CourtRoster.Bonus(1,.05f,.15f)*100:0;

        FleetText(harbour.transform,"Great Chain: "+(VariableSingleton.GetBoolVariable("bGoldenHornChain")?"intact":"removed"),.03f,.59f,.48f,.78f,24);

        FleetText(harbour.transform,"Artillery Ships: "+(TechnologySystem.Has("ArtilleryShips")?"active":"not researched"),.52f,.59f,.97f,.78f,24);

        FleetText(harbour.transform,"Alvise Diedo: "+CourtRoster.Status(1),.03f,.39f,.97f,.58f,22);

        FleetText(harbour.transform,"Current archer support: +"+bonus.ToString("0")+"% · Diedo assists wall archers while the fleet defends the Golden Horn.",.03f,.19f,.97f,.38f,21);

        FleetText(harbour.transform,"Harbour reports: 4 casualties per ship; Artillery Ships doubles this effect.",.03f,.03f,.97f,.18f,20);

        var report=SpyPanel("Latest naval engagement",746,160);

        FleetText(report.transform,"LATEST NAVAL BATTLE",.03f,.70f,.97f,.95f,25);

        FleetText(report.transform,CouncilActions.Int("iNavalBattles")>0?"Our ships lost: "+CouncilActions.Int("iNavalOurLoss")+"     Ottoman ships lost: "+CouncilActions.Int("iNavalEnemyLoss"):"No naval battle has been fought.",.03f,.36f,.97f,.68f,23);



        var yard=SpyPanel("The shipyards",920,yardHeight);

        FleetText(yard.transform,"SHIPYARDS · "+queued+" building · "+SpecialistPersonnel.RepairingShips+" repairing",.03f,.79f,.97f,.97f,25);

        FleetText(yard.transform,building.Count==0?"No orders in progress. Build ships through Military and recruitment.":string.Join("\n",building),.03f,.28f,.97f,.77f,22);
        CampaignHUD.MakeButton(yard.transform,"Repair damaged ships · "+SpecialistPersonnel.AvailableDamagedShips+" available",new Vector2(.03f,.04f),new Vector2(.97f,.25f),RepairDamagedShips);

        var preparation=SpyPanel("Fleet preparation",934+yardHeight,100);

        FleetText(preparation.transform,CouncilActions.Int("iFleetPrepared")>0?"Prepared expedition: +50% to the next successful report. Changing mission removes this bonus.":"Preparation bonus: none.",.03f,.12f,.97f,.88f,22);

    }



    private TMP_Text FleetText(Transform parent,string text,float x0,float y0,float x1,float y1,int size)

    {

        var label=CampaignHUD.Label(parent,text,new Vector2(x0,y0),new Vector2(x1,y1),size);

        label.alignment=TextAlignmentOptions.Center;label.enableAutoSizing=true;

        label.fontSizeMin=16;label.fontSizeMax=size;label.overflowMode=TextOverflowModes.Ellipsis;

        return label;

    }



    public void Diplomacy()

    {

        Open("Diplomacy and court","Receive your advisers, send envoys abroad or exchange messages with the Sultan.");

        var links=Row(0,340);

        var incoming=new System.Collections.Generic.List<string>();

        for(int i=0;i<CouncilActions.Allies.Length;i++) if(CouncilActions.Int("iAidState"+i)==1)

            incoming.Add(CouncilActions.Allies[i].Name+": "+CouncilActions.Int("iAidDays"+i)+" days");

        float summaryHeight=System.Math.Max(130,70+incoming.Count*35);

        var summary=Row(0,summaryHeight);var sr=summary.GetComponent<RectTransform>();sr.anchoredPosition+=new Vector2(0,-340);

        BodyHeight(340+summaryHeight);

        CampaignHUD.Label(summary.transform,incoming.Count==0?"No foreign expeditions are currently committed.":"Committed reinforcements\n"+string.Join("\n",incoming),new Vector2(.03f,.06f),new Vector2(.97f,.94f),25);

        CampaignHUD.MakeButton(links.transform,"Diplomatic relations · "+CouncilActions.Allies.Length+" powers",new Vector2(.04f,.77f),new Vector2(.96f,.97f),DiplomaticRelations);

        CampaignHUD.MakeButton(links.transform,"Negotiate with the Sultan",new Vector2(.04f,.53f),new Vector2(.96f,.73f),Sultan);

        CampaignHUD.MakeButton(links.transform,"Court and characters",new Vector2(.04f,.29f),new Vector2(.48f,.49f),Court);

        CampaignHUD.MakeButton(links.transform,"Galata relations",new Vector2(.52f,.29f),new Vector2(.96f,.49f),Galata);
        CampaignHUD.MakeButton(links.transform,"War Beyond the Walls · theatre map",new Vector2(.04f,.05f),new Vector2(.96f,.25f),WarTheatreUI.Show);

    }



    public void DiplomaticRelations() => UnifiedDiplomaticRelations();



    public void DiplomaticRelationsFor(string group)
    {
        int index=CouncilActions.AllyIndexForRealm(group);
        if(index<0) { DiplomaticRelations(); return; }
        DiplomaticPower(index);
    }

    public void DiplomaticPower(int index) => UnifiedDiplomaticPower(index);

    public void Chronicle(int index)

    {

        var profile=DiplomaticChronicles.All[index];

        Open("About "+CouncilActions.Allies[index].Name,profile.Religion+" · The realm and its stake in the siege");

        ControlSingleton.Instance.ReturnAction=DiplomaticRelations;

        BodyHeight(1050);

        var article=Row(0,1050);

        var prose=CampaignHUD.Label(article.transform,DiplomaticChronicles.Article(index),new Vector2(.055f,.02f),new Vector2(.945f,.98f),28);

        Canvas.ForceUpdateCanvases();

        float height=Mathf.Max(400,prose.GetPreferredValues(prose.text,prose.rectTransform.rect.width,0).y+70);

        BodyHeight(height);

        var articleRect=article.GetComponent<RectTransform>();articleRect.offsetMin=new Vector2(8,-height+6);

    }



    public void AssaultPracticeMenu(){
        Open("Development Options","Practice leaves your campaign unchanged. Choose a system to test.");ControlSingleton.Instance.ReturnAction=AssaultPractice.Exit;BodyHeight(650);
        string[] names={"Ladder assault","Ram and siege tower","Janissary breach","Command a cannon","Mouse-sword duel","24 assault incidents","Albania regional war"};
        for(int i=0;i<7;i++){int n=i;float x=i%2*.5f,y=1-i/2*.245f;var tile=CampaignHUD.Panel(body,names[i],new Vector2(x+.012f,y-.225f),new Vector2(x+.488f,y-.015f));ImperialTheme.Panel(tile,false);CampaignHUD.Label(tile.transform,names[i],new Vector2(.04f,.55f),new Vector2(.96f,.94f),25);CampaignHUD.MakeButton(tile.transform,i==5?"Choose an incident":"Launch practice",new Vector2(.04f,.07f),new Vector2(.96f,.48f),()=>{if(n==6)AlbaniaWarTrialUI.Show();else if(n==4)AssaultPractice.Duel();else if(n==5)AssaultIncidentPractice();else {AssaultPractice.ForcedIncident=-1;AssaultPractice.Launch(n);}});}
    }
    public void AssaultIncidentPractice(){
        Open("Assault incident catalogue","Choose one of the 24 situations to force it into a practice assault. Campaign encounters still check actual equipment, troops and circumstances.");ControlSingleton.Instance.ReturnAction=AssaultPracticeMenu;BodyHeight(24*130);
        for(int i=0;i<AssaultIncidents.All.Length;i++){int n=i;var e=AssaultIncidents.All[i];var row=Row(i,130);CampaignHUD.Label(row.transform,(i+1)+" · "+e.Title+"\n"+e.Condition,new Vector2(.03f,.1f),new Vector2(.70f,.9f),24);CampaignHUD.MakeButton(row.transform,"Test incident",new Vector2(.75f,.15f),new Vector2(.97f,.85f),()=>{AssaultPractice.ForcedIncident=n;AssaultPractice.Launch(n==23?2:1);});}
    }

    public void SultanGiftMenu() {
        Open("Gifts for Mehmed","Choose a gift to review. Nothing is spent until you confirm. A return gift is possible.");
        ControlSingleton.Instance.ReturnAction=Sultan;
        BodyHeight(8*180);
        for(int i=0;i<SultanGifts.Gifts.Length;i++) {
            int choice=i;var g=SultanGifts.Gifts[i];var row=Row(i,180);
            CampaignHUD.Label(row.transform,g.Name,new Vector2(.03f,.69f),new Vector2(.72f,.95f),26);
            CampaignHUD.Explanation(row.transform,g.Description+"\n"+g.Cost+" · Sultan's fury −"+g.Fury+" points",new Vector2(.03f,.08f),new Vector2(.72f,.68f),22);
            CampaignHUD.MakeButton(row.transform,"Review gift",new Vector2(.76f,.23f),new Vector2(.97f,.77f),()=>ReviewSultanGift(choice));
        }
    }
    private void ReviewSultanGift(int index) {
        var g=SultanGifts.Gifts[index];Open("Gift to Mehmed · "+g.Name,"Review the offering before sending your envoy.");
        ControlSingleton.Instance.ReturnAction=SultanGiftMenu;BodyHeight(520);
        CampaignHUD.Explanation(body,g.Description+"\n\nCost: "+g.Cost+"\nSultan's fury −"+g.Fury+" points (minimum zero).\nA return gift is possible, not guaranteed.",new Vector2(.05f,.35f),new Vector2(.95f,.95f),27);
        string unavailable=SultanGifts.GiftUnavailable(index);
        var confirm=CampaignHUD.MakeTurnButton(body,unavailable??"Confirm — send gift",new Vector2(.05f,.08f),new Vector2(.62f,.25f),()=>{bool ok=SultanGifts.Send(index,out var report);Finish(ok,report);});confirm.interactable=unavailable==null;
        CampaignHUD.MakeButton(body,"Choose another gift",new Vector2(.66f,.08f),new Vector2(.95f,.25f),SultanGiftMenu);
    }

    public void Sultan()=>SultanAudience();

    public void ReviewTerms()

    {

        Open("The Sultan's terms",SurrenderTerms.Valid?"The sealed offer remains open today. Your council awaits your decision.":"This offer has expired. Send an envoy to request fresh terms.");

        ControlSingleton.Instance.ReturnAction=Sultan;BodyHeight(7*185+220);

        for(int i=0;i<7;i++) {

            var row=Row(i,185);

            CampaignHUD.Label(row.transform,SurrenderTerms.Groups[i],new Vector2(.025f,.15f),new Vector2(.24f,.87f),26);

            CampaignHUD.Label(row.transform,SurrenderTerms.Heading(i),new Vector2(.27f,.70f),new Vector2(.97f,.97f),25);

            CampaignHUD.Explanation(row.transform,SurrenderTerms.Detail(i),new Vector2(.27f,.08f),new Vector2(.97f,.68f),22);

        }

        var footer=Row(7,185);

        CampaignHUD.Label(footer.transform,"Assassination risk: "+SurrenderTerms.AssassinationRisk+"%\nInhabitants "+(SurrenderTerms.Severe(0)?10:0)+"% + military "+(SurrenderTerms.Severe(1)?50:0)+"% + household "+(SurrenderTerms.Severe(3)?20:0)+"%",new Vector2(.03f,.12f),new Vector2(.65f,.9f),23);

        var accept=CampaignHUD.MakeButton(footer.transform,"Accept terms…",new Vector2(.70f,.2f),new Vector2(.97f,.8f),ConfirmTerms);accept.interactable=SurrenderTerms.Valid;

        notice.text="At offer: fury "+CouncilActions.Int("iTermsFury")+"% · camp alert "+CouncilActions.Int("iTermsAlert")+"% · active wall "+CouncilActions.Int("iTermsWalls")+"%";

    }

    public void ConfirmTerms()

    {

        Open("Confirm surrender","Your acceptance ends this campaign. The terms shown in the sealed offer will determine the City's fate.");

        ControlSingleton.Instance.ReturnAction=ReviewTerms;BodyHeight(600);

        var text=CampaignHUD.Label(body,"Your fate: "+SurrenderTerms.Heading(2)+"\n\nInhabitants: "+SurrenderTerms.Heading(0)+"\nMilitary: "+SurrenderTerms.Heading(1)+"\nHousehold: "+SurrenderTerms.Heading(3)+"\nNobility: "+SurrenderTerms.Heading(4)+"\n\nAssassination risk: "+SurrenderTerms.AssassinationRisk+"%",new Vector2(.06f,.25f),new Vector2(.94f,.96f),28);text.alignment=TextAlignmentOptions.Center;

        bool submitted=false;

        var confirm=CampaignHUD.MakeButton(body,"Confirm surrender",new Vector2(.55f,.05f),new Vector2(.94f,.20f),()=>{if(submitted)return;if(SurrenderTerms.Accept(out var report)){submitted=true;Hide();CampaignHUD.Instance.CheckEnding();}else notice.text=report;});confirm.interactable=SurrenderTerms.Valid;

        CampaignHUD.MakeButton(body,"Return to terms",new Vector2(.06f,.05f),new Vector2(.45f,.20f),ReviewTerms);

    }



    // Retired faction screen: preserve the callback for older scene bindings.
    public void Politics()=>CivicRelations();

    public void Overview()
    {
        Open("Siege overview",VariableSingleton.HasSiegeStarted()?"Siege day "+VariableSingleton.SiegeDay+" · "+VariableSingleton.GetDate().ToString("d MMMM yyyy"):"Preparing the City · "+VariableSingleton.GetDate().ToString("d MMMM yyyy"));
        float offset=CityStatistics()-204;
        var defence=SpyPanel("Defences",offset+204,260);
        CampaignHUD.Label(defence.transform,"DEFENCES",new Vector2(.025f,.81f),new Vector2(.97f,.98f),28);
        var meters=CampaignHUD.Panel(defence.transform,"Wall and moat integrity",new Vector2(.015f,.02f),new Vector2(.985f,.80f));meters.GetComponent<Image>().color=Color.clear;
        string wall=VariableSingleton.Instance.EnduringWall.GetCodeName();
        wall=wall=="OuterWall"?"Theodosian Wall":wall=="MiddleWall"?"Constantinian Wall":wall=="InnerWall"?"Severan Wall":wall;
        OverviewMeter(meters.transform,"ACTIVE DEFENCE · "+wall,VariableSingleton.Instance.EnduringWall.GetHealth(),.03f,.48f);
        OverviewMeter(meters.transform,"MOAT",VariableSingleton.GetFloatVariable("fMoat"),.52f,.97f);
        var command=SpyPanel("Siege commander",offset+478,100);
        int commander=CouncilActions.Int("iCommander");
        var commandName=CampaignHUD.Label(command.transform,"SIEGE COMMANDER · "+CourtRoster.CommanderName(commander),new Vector2(.025f,.53f),new Vector2(.72f,.96f),26);
        var commandBonus=CampaignHUD.Label(command.transform,CourtRoster.CommanderBenefit(commander),new Vector2(.025f,.06f),new Vector2(.72f,.52f),23);
        foreach(var label in new[]{commandName,commandBonus}){label.enableAutoSizing=true;label.fontSizeMin=17;label.fontSizeMax=label.fontSize;}
        CampaignHUD.MakeButton(command.transform,"Change commander",new Vector2(.75f,.18f),new Vector2(.975f,.82f),CommanderSelection);
        FocusCard(offset+592);
        var links=SpyPanel("Defence navigation",offset+826,90);
        CampaignHUD.MakeButton(links.transform,"Spies and intelligence",new Vector2(.03f,.12f),new Vector2(.48f,.88f),Spies);
        CampaignHUD.MakeButton(links.transform,"Defence of the walls",new Vector2(.52f,.12f),new Vector2(.97f,.88f),()=>{Hide();manager.BringUpSiegeUI();});
        var camp=SpyPanel("Enemy camp",offset+944,200);
        CampaignHUD.Label(camp.transform,"ENEMY CAMP",new Vector2(.025f,.76f),new Vector2(.97f,.97f),28);
        int delay=CouncilActions.Int("iEnemyDelay");
        CampaignHUD.Label(camp.transform,"Operations delay: "+delay+(delay==1?" day":" days")+" · Agents in camp: "+CouncilActions.Int("iSpies")+" · Secrets: "+CouncilActions.Int("iSecrets")+" · Alert: "+EspionageSystem.Alert+"%",new Vector2(.025f,.43f),new Vector2(.97f,.73f),24);
        CampaignHUD.Label(camp.transform,"Siege ramp: "+VariableSingleton.GetPercentFloat("fSiegeRamp")+" · Siege tower: "+(VariableSingleton.GetBoolVariable("bSiegeTower")?"ready":"none")+" · Battering ram: "+(VariableSingleton.GetBoolVariable("bBatteringRam")?"ready":"none"),new Vector2(.025f,.08f),new Vector2(.97f,.40f),24);
        float top=offset+1158;
        for(int row=0;row<5;row++){
            float height=row>=3?240:120;
            for(int col=0;col<2;col++){
                int n=row*2+col;bool known=CouncilActions.Int("iSpies")>n;
                var item=SpyPanel("Enemy report "+n,top,height);var rect=item.GetComponent<RectTransform>();rect.anchorMin=new Vector2(col*.5f,1);rect.anchorMax=new Vector2(col*.5f+.5f,1);
                var label=CampaignHUD.Label(item.transform,EspionageSystem.IntelligenceLabels[n],new Vector2(.04f,.71f),new Vector2(.96f,.96f),24);
                var value=CampaignHUD.Label(item.transform,known?EspionageSystem.IntelligenceValue(n):"Requires "+(n+1)+" agents",new Vector2(.04f,.06f),new Vector2(.96f,.69f),23);
                value.enableAutoSizing=true;value.fontSizeMin=19;value.fontSizeMax=23;
                if(!known){label.color=value.color=new Color(.55f,.60f,.63f);value.fontStyle|=FontStyles.Italic;}
                if(n==0){label.text="Enemy soldiers · View types";EnemyTroopsLink(item,Overview);ButtonAudio.Attach(item.GetComponent<Button>());label.margin=Vector4.zero;}
            }
            top+=height+14;
        }
        var briefingLink=SpyPanel("Intelligence briefing",top,90);
        CampaignHUD.MakeButton(briefingLink.transform,"Read the latest enemy action",new Vector2(.03f,.12f),new Vector2(.97f,.88f),IntelligenceBriefing);
        BodyHeight(top+104);
        foreach(var label in body.GetComponentsInChildren<TMP_Text>()) {
            if(label.enableAutoSizing)continue;
            label.enableAutoSizing=true;label.fontSizeMax=label.fontSize;label.fontSizeMin=Mathf.Min(18,label.fontSize);
        }
    }

    private void OverviewMeter(Transform parent,string name,float value,float left,float right)
    {
        var title=CampaignHUD.Label(parent,name,new Vector2(left,.70f),new Vector2(right,.96f),25);
        var amount=CampaignHUD.Label(parent,Mathf.RoundToInt(value*100)+"% integrity",new Vector2(left,.38f),new Vector2(right,.68f),30);
        var track=CampaignHUD.Panel(parent,name+" meter",new Vector2(left,.15f),new Vector2(right,.29f));ImperialTheme.Panel(track,false);
        var fill=CampaignHUD.Panel(track.transform,"Integrity",new Vector2(0,.12f),new Vector2(Mathf.Clamp01(value),.88f));
        var image=fill.GetComponent<Image>();image.sprite=null;image.color=name=="MOAT"?new Color(.23f,.66f,.67f):new Color(.83f,.77f,.63f);image.raycastTarget=false;
        track.transform.Find("Imperial goldwork").SetAsLastSibling();
        title.alignment=amount.alignment=TextAlignmentOptions.Center;
    }

    public void Technologies()



    {



        Open("Technologies and workshops","Recover knowledge in the monasteries, gather secrets through agents, or learn through events. Research strengthens the city's defence.");



        notice.text="Available secrets: "+CouncilActions.Int("iSecrets");



        BodyHeight(170+TechnologySystem.All.Length*240);
        MonasticTechnologySummary();



        for(int i=0;i<TechnologySystem.All.Length;i++)



        {



            int choice=i;var technology=TechnologySystem.All[i];var row=Row(i,240);
            row.GetComponent<RectTransform>().anchoredPosition+=new Vector2(0,-170);



            TechnologyArtwork.Attach(row.transform,technology.Key);
            bool active=VariableSingleton.GetBoolVariable(technology.Key);



            CampaignHUD.Label(row.transform,technology.Name+(active?" — active":""),new Vector2(.255f,.72f),new Vector2(.72f,.98f),27);



            var technologyDescription=CampaignHUD.Explanation(row.transform,technology.Effect+"\n<color=#B8CFDA><i>"+technology.History+"</i></color>",new Vector2(.255f,.05f),new Vector2(.72f,.70f),21);



            technologyDescription.enableAutoSizing=true; technologyDescription.fontSizeMin=16; technologyDescription.fontSizeMax=21;
            var button=CampaignHUD.MakeTurnButton(row.transform,active?"Active":technology.Cost+" secrets\nResearch",new Vector2(.76f,.18f),new Vector2(.98f,.82f),()=>{bool ok=TechnologySystem.Unlock(choice,out var report);Finish(ok,report);});



            button.interactable=!active && VariableSingleton.GetIntVariable("iSecrets")>=technology.Cost;



            if(!active && CouncilActions.Int("iSecrets")<technology.Cost) button.GetComponentInChildren<TMP_Text>().text="Requires "+technology.Cost+" secrets";



            if(active) { var colours=button.colors;colours.disabledColor=new Color(1f,.78f,.32f,1);button.colors=colours;row.GetComponent<Image>().color=new Color(.22f,.17f,.075f,1);button.GetComponentInChildren<TMP_Text>().color=new Color(1f,.9f,.57f); }




        }



    }



    public void Court()

    {

        Open("Court and characters","Serving advisers and officers. Their specialist benefits apply while in service; appointment strengthens the commander's benefit. Portraits are artistic interpretations.");

        ControlSingleton.Instance.ReturnAction=Diplomacy;

        bool shahServing=SultanGifts.Known && (DailyLifeEvents.I("iShahStatus")==4 || DailyLifeEvents.I("iShahStatus")==7);

        int count=0;for(int i=0;i<8;i++)if(CourtRoster.InService(i))count++;

        BodyHeight(Mathf.Max(210,(count+(shahServing?1:0)+(CharacterPreparations.InService?1:0))*230));

        int position=0;

        for(int i=0;i<10;i++) {

            if(!CourtRoster.InService(i))continue;

            var row=Row(position++,230);IllustratedIdentity.Portrait(row.transform,i);

            CampaignHUD.Label(row.transform,CourtRoster.Names[i],new Vector2(.20f,.74f),new Vector2(.98f,.98f),29);

            CampaignHUD.Label(row.transform,CourtRoster.Status(i)+" · Loyalty "+CouncilActions.Int("iLoyalty"+i)+" / 100",new Vector2(.20f,.53f),new Vector2(.98f,.75f),22);

            CampaignHUD.Explanation(row.transform,CourtRoster.Roles[i]+"\n"+CourtRoster.Notes[i],new Vector2(.20f,.09f),new Vector2(.60f,.52f),22);

            CampaignHUD.Label(row.transform,"<color=#E8C36F>"+CourtRoster.Abilities[i]+"</color>",new Vector2(.64f,.09f),new Vector2(.97f,.51f),23);

        }

        if(shahServing) {var row=Row(position++,230);IllustratedIdentity.Portrait(row.transform,201);CampaignHUD.Label(row.transform,"Shaherezade — "+SultanGifts.Status+"\n\n"+SultanGifts.Note,new Vector2(.20f,.08f),new Vector2(.97f,.94f),25);}

        if(CharacterPreparations.InService) {

            int c=CharacterPreparations.Candidate;var row=Row(position++,230);IllustratedIdentity.Portrait(row.transform,100+c);

            CampaignHUD.Label(row.transform,CharacterPreparations.Names[c]+" · "+CharacterPreparations.Status+" · Loyalty "+CharacterPreparations.Loyalty+" / 100",new Vector2(.20f,.72f),new Vector2(.97f,.97f),26);

            CampaignHUD.Explanation(row.transform,CharacterPreparations.Roles[c]+"\n"+CharacterPreparations.Profiles[c],new Vector2(.20f,.13f),new Vector2(.56f,.68f),23);

            CampaignHUD.Label(row.transform,"<color=#E8C36F>"+CharacterPreparations.Abilities[c]+"</color>",new Vector2(.60f,.40f),new Vector2(.97f,.70f),23);

            var appoint=CampaignHUD.MakeTurnButton(row.transform,CharacterPreparations.IsCommander?"Siege commander":"Appoint commander",new Vector2(.60f,.08f),new Vector2(.97f,.36f),()=>{bool ok=CharacterPreparations.Appoint(out var report);Finish(ok,report);});appoint.interactable=!CharacterPreparations.IsCommander;

        }

        if(position==0)CampaignHUD.Label(body,"No advisers are currently in your service.",new Vector2(.04f,.2f),new Vector2(.96f,.9f),28);

        if(CharacterPreparations.Invited&&!CharacterPreparations.InService)notice.text="Expected arrival: "+CharacterPreparations.Name()+" · "+CharacterPreparations.Status;

    }



    public void WallSpecialActions()

    {

        Open("Wall defences · Special actions","Direct your artillery against the Ottoman batteries.");

        ControlSingleton.Instance.ReturnAction=()=>{Hide();manager.BringUpSiegeUI();};

        BodyHeight(360);

        for(int i=0;i<2;i++) {

            bool grand=i==1;var row=Row(i,180);

            CampaignHUD.Label(row.transform,grand?"Fire the Grand Cannon":"Counter-battery fire",new Vector2(.03f,.65f),new Vector2(.69f,.96f),28);

            CampaignHUD.Explanation(row.transform,grand?"Turn the great gun against the enemy batteries.":"Your gunners concentrate their fire on the Ottoman artillery.",new Vector2(.03f,.38f),new Vector2(.69f,.64f),21);

            CampaignHUD.Label(row.transform,"3 ammunition · Destroy 1–5 enemy cannons",new Vector2(.03f,.08f),new Vector2(.69f,.37f),22);

            string reason=TechnologySystem.ArtilleryUnavailable(grand);

            var fire=CampaignHUD.MakeTurnButton(row.transform,reason??"Fire",new Vector2(.72f,.15f),new Vector2(.97f,.85f),()=>{string report;bool ok=grand?TechnologySystem.FireGrandCannon(out report):TechnologySystem.CounterBattery(out report);Finish(ok,report);});

            fire.interactable=reason==null;

        }

    }



    // The palace map now opens the actual defensive orders.
    public void PalaceActions()
    {
        Hide();
        manager.BringUpSiegeUI();
    }
}
