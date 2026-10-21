using System;

using UnityEngine;

using UnityEngine.UI;

using TMPro;



// Adds working council screens without replacing the authored management/turn components.

public partial class CouncilUI : MonoBehaviour

{

    public const string BuildLabel="Version 19.1 · Midnight Mosaic · 21 October 2026";

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

        var briefingPanel=CampaignHUD.Panel(manager.transform,"Council briefing",new Vector2(.30f,.04f),new Vector2(.70f,.41f));

        briefingPanel.GetComponent<Image>().color=new Color(.07f,.09f,.105f,.96f);

        briefing=CampaignHUD.Label(briefingPanel.transform,"",new Vector2(.05f,.05f),new Vector2(.95f,.96f),22);

        string[] labels={"Siege overview","Military and recruitment","Fleet","Spies and intelligence","The Palace","Rest · major action\nThen hear petitions","Defence of the walls","Society and politics","Diplomacy and court","Technologies","Treasury and market"};

        UnityEngine.Events.UnityAction[] actions={Overview,manager.BringUpTroopsUI,Fleet,Spies,Palace,manager.ContinueToRest,manager.BringUpSiegeUI,Factions,Diplomacy,Technologies,Market};

        for(int i=0;i<labels.Length;i++)

        {

            int row=i<6?i:i-6;float x=i<6?.025f:.72f;float top=.82f-row*.128f;

            var button=CampaignHUD.MakeButton(manager.transform,labels[i],new Vector2(x,top-.105f),new Vector2(x+.255f,top),actions[i]);

            int[] emblems={0,1,-1,4,3,-1,0,3,4,4,2};
            if(emblems[i]>=0) {
                ImperialTheme.Icon(button.transform,emblems[i],new Vector2(.025f,.1f),new Vector2(.22f,.9f));
                var caption=button.GetComponentInChildren<TMP_Text>();caption.rectTransform.anchorMin=new Vector2(.23f,.03f);caption.rectTransform.anchorMax=new Vector2(.96f,.97f);
            }
            if(i==5) button.name="ButtonToContinue";

        }

    }

    private void Update()

    {

        if(briefing==null || !VariableSingleton.IsInitialized) return;

        string weakest="";float support=2f;

        for(int i=0;i<4;i++) {float value=VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[i]);if(value<support){support=value;weakest=CouncilActions.FactionNames[i];}}

        string aid="No relief expedition committed.";
        int soonest=int.MaxValue,committed=0;
        for(int i=0;i<CouncilActions.Allies.Length;i++) if(CouncilActions.Int("iAidState"+i)==1) {
            committed++;int days=CouncilActions.Int("iAidDays"+i);
            if(days<soonest){soonest=days;aid="Next aid: "+CouncilActions.Allies[i].Name+" · "+days+" days";}
        }
        briefing.fontSize=20;
        briefing.text="<b>THE CITY AT A GLANCE</b>\nLowest support: "+weakest+" "+Mathf.RoundToInt(support*100)+"%\n"+aid+(committed>0?"\nCommitted expeditions: "+committed+" · Details in Diplomacy":"")+"\nFood remaining: "+SupplyEconomy.DaysLeft+" days · Enemy camp alert: "+EspionageSystem.Alert+"%\n\n<size=85%><i>‘The best way of avenging thyself is not to become like [the wrong-doer].’</i>\nMarcus Aurelius · Meditations VI.6</size>";

    }

    public void Hide() { if(screen!=null) screen.SetActive(false); }

    private void Open(string title,string introduction)

    {

        if(screen!=null) {screen.SetActive(false);Destroy(screen);}

        screen=CampaignHUD.Panel(manager.transform.parent,"Council screen",new Vector2(.015f,.025f),new Vector2(.985f,.865f));

        screen.transform.SetAsLastSibling();

        ImperialTheme.Panel(screen);

        CampaignHUD.Label(screen.transform,title,new Vector2(.035f,.90f),new Vector2(.965f,.98f),34);

        CampaignHUD.Explanation(screen.transform,introduction,new Vector2(.035f,.81f),new Vector2(.965f,.90f),21);

        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.035f,.02f),new Vector2(.75f,.09f),21);

        var back=CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.79f,.02f),new Vector2(.965f,.09f),()=>ControlSingleton.Instance.GoBack());

        var viewport=CampaignHUD.Panel(screen.transform,"Council scroll",new Vector2(.035f,.11f),new Vector2(.945f,.79f));

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

    public void Factions()
    {
        Open("Society and politics", "Political influence measures each faction's share of power, not popular approval. Civic relations are shown separately.");
        BodyHeight(590);
        var tabs=SpyPanel("Society navigation",0,70);
        CampaignHUD.MakeButton(tabs.transform,"Political dealings",new Vector2(.02f,.15f),new Vector2(.49f,.85f),Politics);
        CampaignHUD.MakeButton(tabs.transform,"Civic relations and audiences",new Vector2(.51f,.15f),new Vector2(.98f,.85f),CivicRelations);
        for(int i=0;i<4;i++)
        {
            int col=i%2, row=i/2, share=PoliticalSystem.Share(i);
            bool banned=PoliticalSystem.Banned(i);
            var card=SpyPanel(PoliticalSystem.Names[i],84+row*250,238);
            var rect=card.GetComponent<RectTransform>();
            rect.anchorMin=new Vector2(col*.5f,1);rect.anchorMax=new Vector2(col*.5f+.49f,1);
            var title=CampaignHUD.Label(card.transform,PoliticalSystem.Names[i],new Vector2(.04f,.80f),new Vector2(.96f,.97f),25);
            title.alignment=TextAlignmentOptions.Center;
            var value=CampaignHUD.Label(card.transform,banned?"BANISHED":share+"% influence",new Vector2(.04f,.64f),new Vector2(.96f,.80f),24);
            value.alignment=TextAlignmentOptions.Center;
            var track=CampaignHUD.Panel(card.transform,"Influence",new Vector2(.06f,.57f),new Vector2(.94f,.60f));
            track.GetComponent<Image>().color=new Color(.12f,.16f,.19f);
            var fill=CampaignHUD.Panel(track.transform,"Current share",Vector2.zero,new Vector2(Mathf.Clamp01(share/100f),1));
            fill.GetComponent<Image>().color=PoliticalSystem.Colours[i];
            CampaignHUD.Explanation(card.transform,PoliticalSystem.Beliefs[i],new Vector2(.05f,.29f),new Vector2(.95f,.55f),20);
            var offer=CampaignHUD.Label(card.transform,PoliticalSystem.Effects[i],new Vector2(.05f,.15f),new Vector2(.95f,.29f),20);
            offer.alignment=TextAlignmentOptions.Center;
            string status=banned?"This faction can no longer offer aid.":share<20?"Needs 20% influence to offer aid.":!PoliticalSystem.CanAid(i)?"Needs a Western expedition already on its way.":"A favour is available · uses your major action";
            var availability=CampaignHUD.Label(card.transform,status,new Vector2(.05f,.02f),new Vector2(.95f,.14f),18);
            availability.alignment=TextAlignmentOptions.Center;
        }
    }

    public void CivicRelations()

    {

        Open("Civic relations and audiences","Five overlapping interests. Each audience costs 300 gold, improves its measure by 3 points (up to 100%), and uses your major action.");

        ControlSingleton.Instance.ReturnAction=Factions;
        BodyHeight(5*142);

        for(int i=0;i<5;i++)

        {

            int choice=i;var row=Row(i,142);

            CampaignHUD.Label(row.transform,CouncilActions.FactionNames[i]+" — "+VariableSingleton.GetPercentFloat(CouncilActions.FactionKeys[i]),new Vector2(.02f,.66f),new Vector2(.66f,.96f),27);

            CampaignHUD.Explanation(row.transform,CouncilActions.FactionDescriptions[i],new Vector2(.02f,.08f),new Vector2(.65f,.64f),21);

            if(i==4) {
                CampaignHUD.MakeButton(row.transform,"Send a gift",new Vector2(.67f,.12f),new Vector2(.82f,.88f),()=>ReviewFaction(4));
                CampaignHUD.MakeButton(row.transform,"Galatan diplomacy",new Vector2(.83f,.12f),new Vector2(.99f,.88f),Galata);
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
        var commitment=CampaignHUD.Explanation(screen.transform,"Uses your major action",new Vector2(.1f,.24f),new Vector2(.9f,.34f),24);commitment.alignment=TextAlignmentOptions.Center;
        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.1f,.01f),new Vector2(.9f,.08f),21);notice.alignment=TextAlignmentOptions.Center;
        CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.15f,.10f),new Vector2(.46f,.21f),()=>{if(index==4)Galata();else Factions();});
        bool submitted=false;
        var confirm=CampaignHUD.MakeButton(screen.transform,confirms[index],new Vector2(.51f,.10f),new Vector2(.85f,.21f),()=>{
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
        var action=CampaignHUD.Explanation(screen.transform,"Uses your major action",new Vector2(.1f,.24f),new Vector2(.9f,.34f),24);action.alignment=TextAlignmentOptions.Center;
        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.1f,.01f),new Vector2(.9f,.08f),21);
        CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.15f,.10f),new Vector2(.46f,.21f),()=>TroopUpgrades(unit));
        bool submitted=false;
        var button=CampaignHUD.MakeButton(screen.transform,"Confirm training",new Vector2(.51f,.10f),new Vector2(.85f,.21f),()=>{if(submitted)return;bool ok=TroopTraining.Order(unit,out var report);if(ok)submitted=true;Finish(ok,report);});button.interactable=TroopTraining.Unavailable(unit)==null;
    }

    public void Training()
    {
        Open("Ongoing training","Recruits, ships and cannon prepare alongside your existing troops' drills. Several orders can proceed together; payment is made when you place them.");
        ControlSingleton.Instance.ReturnAction=()=>{Hide();manager.BringUpTroopsUI();};
        var orders=new System.Collections.Generic.List<TrainingPool.Order>(TrainingPool.Orders);orders.Sort((a,b)=>a.readyDay.CompareTo(b.readyDay));
        int pending=0;for(int i=0;i<3;i++) if(TroopTraining.Due(i)>0)pending++;
        BodyHeight(Mathf.Max(180,(orders.Count+pending)*140));
        if(orders.Count+pending==0) CampaignHUD.Explanation(body,"No training or construction is underway. Train recruits or upgrade your soldiers through Military and recruitment.",new Vector2(.03f,.2f),new Vector2(.97f,.9f),26);
        for(int i=0;i<orders.Count;i++) {
            var o=orders[i];var row=Row(i,140);
            CampaignHUD.Label(row.transform,TrainingPool.Name(o.unit)+" · "+o.quantity.ToString("N0"),new Vector2(.03f,.52f),new Vector2(.63f,.92f),28);
            CampaignHUD.Explanation(row.transform,o.unit=="iShips" || o.unit=="iCannons"?"Under construction":"New recruits in training",new Vector2(.03f,.1f),new Vector2(.63f,.48f),23);
            CampaignHUD.Label(row.transform,"Ready "+TrainingPool.ReadyDate(o).ToString("d MMM yyyy")+"\n"+TrainingPool.Remaining(o)+" days remaining",new Vector2(.65f,.1f),new Vector2(.98f,.92f),25);
        }
        int index=orders.Count;
        for(int i=0;i<3;i++) if(TroopTraining.Due(i)>0) {
            var row=Row(index++,140);var ready=new DateTime((long)TroopTraining.Due(i)*TimeSpan.TicksPerDay);
            CampaignHUD.Label(row.transform,TroopTraining.Units[i]+" · advanced drills",new Vector2(.03f,.52f),new Vector2(.63f,.92f),28);
            CampaignHUD.Explanation(row.transform,"Troops remain available for battle",new Vector2(.03f,.1f),new Vector2(.63f,.48f),23);
            CampaignHUD.Label(row.transform,"Ready "+ready.ToString("d MMM yyyy")+"\n"+Mathf.Max(0,TroopTraining.Due(i)-TrainingPool.Day(VariableSingleton.GetDate()))+" days remaining",new Vector2(.65f,.1f),new Vector2(.98f,.92f),25);
        }
    }

    private GameObject SpyPanel(string name,float top,float height)
    {
        var panel=CampaignHUD.Panel(body,name,Vector2.zero,Vector2.one);
        var rect=panel.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);
        rect.offsetMin=new Vector2(8,-top-height);rect.offsetMax=new Vector2(-8,-top);
        panel.GetComponent<Image>().color=ImperialTheme.Surface;ImperialTheme.Frame(panel);
        return panel;
    }
    public void Spies()
    {
        Open("Spies and intelligence","Recruit contacts, uncover the enemy's plans and choose your operations. Each operation uses your major action.");
        BodyHeight(578+CouncilActions.SpyOrders.Length*238+12);
        int spies=CouncilActions.Int("iSpies");
        var network=SpyPanel("Intelligence network",0,180);
        CampaignHUD.Label(network.transform,"INTELLIGENCE NETWORK  ·  "+spies+" agents",new Vector2(.025f,.70f),new Vector2(.62f,.96f),26);
        CampaignHUD.Explanation(network.transform,"Each agent reveals another level of intelligence.",new Vector2(.025f,.49f),new Vector2(.62f,.71f),21);
        for(int i=0;i<8;i++) {
            float x=.025f+i*.073f;bool unlocked=spies>i;
            var segment=CampaignHUD.Panel(network.transform,"Intelligence level "+(i+1),new Vector2(x,.15f),new Vector2(x+.065f,.43f));
            segment.GetComponent<Image>().color=unlocked?new Color(.58f,.43f,.22f):new Color(.12f,.10f,.12f);
            var number=CampaignHUD.Label(segment.transform,(i+1).ToString(),Vector2.zero,Vector2.one,23);number.alignment=TextAlignmentOptions.Center;
            if(!unlocked)number.color=new Color(.57f,.54f,.52f);
        }
        CampaignHUD.Label(network.transform,"ENEMY CAMP ALERT  ·  "+EspionageSystem.Alert+"%",new Vector2(.66f,.68f),new Vector2(.97f,.96f),26);
        var meter=CampaignHUD.Panel(network.transform,"Enemy camp alert meter",new Vector2(.66f,.47f),new Vector2(.97f,.59f));meter.GetComponent<Image>().color=new Color(.10f,.07f,.09f);
        var fill=CampaignHUD.Panel(meter.transform,"Alert fill",Vector2.zero,new Vector2(EspionageSystem.Alert/100f,1));fill.GetComponent<Image>().color=new Color(.69f,.29f,.22f);
        CampaignHUD.Explanation(network.transform,"Falls by 1–10 points each turn.\nHigher alert puts agents at greater risk.",new Vector2(.66f,.06f),new Vector2(.97f,.40f),20);
        var intel=SpyPanel("Intelligence reports",192,280);
        for(int i=0;i<8;i++) {
            float left=.01f+(i%4)*.247f,top=.97f-(i/4)*.49f;bool known=spies>i;
            var cell=CampaignHUD.Panel(intel.transform,"Intelligence report "+(i+1),new Vector2(left,top-.45f),new Vector2(left+.237f,top));
            cell.GetComponent<Image>().color=known?ImperialTheme.Surface:ImperialTheme.Navy;
            var title=CampaignHUD.Label(cell.transform,(i+1)+"  "+EspionageSystem.IntelligenceLabels[i],new Vector2(.04f,.49f),new Vector2(.96f,.95f),22);
            var value=CampaignHUD.Label(cell.transform,known?EspionageSystem.IntelligenceValue(i):"Requires "+(i+1)+" agents",new Vector2(.04f,.03f),new Vector2(.96f,.48f),21);
            if(!known) {title.color=new Color(.57f,.54f,.52f);value.color=title.color;value.fontStyle|=FontStyles.Italic;}
        }
        var payment=SpyPanel("Operation payment",484,80);
        CampaignHUD.Label(payment.transform,"OPERATIONS  ·  "+CouncilActions.Int("iSecrets")+" secrets available",new Vector2(.02f,.12f),new Vector2(.60f,.9f),25);
        var gold=CampaignHUD.MakeButton(payment.transform,"Gold",new Vector2(.65f,.12f),new Vector2(.80f,.88f),()=>{paySecret=false;Spies();});
        var secrets=CampaignHUD.MakeButton(payment.transform,"Secrets",new Vector2(.82f,.12f),new Vector2(.98f,.88f),()=>{paySecret=true;Spies();});
        (paySecret?secrets:gold).GetComponent<Image>().color=new Color(.50f,.34f,.12f);
        for(int i=0;i<CouncilActions.SpyOrders.Length;i++) {
            int mission=i;var row=SpyPanel("Spy operation "+i,578+i*238,226);
            CampaignHUD.Label(row.transform,CouncilActions.SpyOrders[i],new Vector2(.025f,.78f),new Vector2(.70f,.98f),27);
            CampaignHUD.Explanation(row.transform,EspionageSystem.Descriptions[i],new Vector2(.025f,.32f),new Vector2(.69f,.76f),22);
            CampaignHUD.Label(row.transform,CouncilActions.SpyEffects[i],new Vector2(.025f,.04f),new Vector2(.69f,.29f),21);
            var risk=CampaignHUD.Label(row.transform,"Agent loss risk: "+EspionageSystem.LossChance(i).ToString("0.#")+"%\nEnemy camp alert: "+(i==3?"unchanged":"+"+EspionageSystem.AlertGain[i]+" points"),new Vector2(.71f,.49f),new Vector2(.98f,.94f),21);risk.alignment=TextAlignmentOptions.Center;
            var button=CampaignHUD.MakeButton(row.transform,paySecret?"Use 1 secret":"Pay 1,000 gold",new Vector2(.72f,.08f),new Vector2(.98f,.39f),()=>{if(mission==5){ReviewDisruption();return;}bool ok=CouncilActions.SpyMission(mission,paySecret,out var result);Finish(ok,result);});
            string unavailable=i!=0 && spies<1?"Recruit an agent first":
                (paySecret?CouncilActions.Int("iSecrets")<1:CouncilActions.Int("iMoney")<1000)?(paySecret?"Requires 1 secret":"Requires 1,000 gold"):
                i==1 && CouncilActions.Int("iTurkCannons")==0 || i==2 && CouncilActions.Int("iFood")==0 || i==3 && CouncilActions.Int("iTurkSpies")==0?"No targets remain":null;
            if(unavailable!=null) {button.interactable=false;button.GetComponentInChildren<TMP_Text>().text=unavailable;}
        }
    }

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
        var terms=CampaignHUD.Explanation(screen.transform,"Uses your major action · Enemy camp alert +20 points · Agent loss risk "+EspionageSystem.LossChance(5).ToString("0.#")+"%\nDelays accumulate and begin taking effect during the siege.",new Vector2(.06f,.23f),new Vector2(.94f,.37f),22);terms.alignment=TextAlignmentOptions.Center;
        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.06f,.01f),new Vector2(.94f,.08f),21);
        CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.15f,.10f),new Vector2(.46f,.21f),Spies);
        bool submitted=false;
        var confirm=CampaignHUD.MakeButton(screen.transform,"Confirm disruption",new Vector2(.51f,.10f),new Vector2(.85f,.21f),()=>{
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
        Open("The Imperial Fleet","Inspect your ships, their orders and the benefits they bring to the City's defence.");
        int ships=CouncilActions.Int("iShips"),order=Mathf.Clamp(CouncilActions.Int("iFleet"),0,5);
        string[] orders={"Raid Ottoman supplies","Raid coastal shipping","Bring grain from Crimea","Defend the Golden Horn","Bring volunteers","Fleet lost"};
        string[] effects={"A successful fleet report removes 3 days of Ottoman food stores.","A successful fleet report brings 750 gold.","A successful fleet report brings 150 provisions.","A successful fleet report destroys "+RecurringEvents.FleetDefensiveDamage+" enemy casualties (4 per ship; doubled by Artillery Ships).","A successful fleet report brings 50 infantry and 25 archers.","The fleet cannot carry out its former orders."};
        var building=new System.Collections.Generic.List<string>();int queued=0;
        foreach(var o in TrainingPool.Orders) if(o.unit=="iShips") {queued+=o.quantity;building.Add(o.quantity+" warships · ready "+TrainingPool.ReadyDate(o).ToString("d MMM yyyy")+" ("+TrainingPool.Remaining(o)+" days)");}
        BodyHeight(1120+building.Count*35);
        var summary=Row(0,180);
        CampaignHUD.Label(summary.transform,"Ships in service: "+ships+"     Under construction: "+queued,new Vector2(.03f,.65f),new Vector2(.97f,.97f),29);
        CampaignHUD.Label(summary.transform,"Current orders: "+orders[order],new Vector2(.03f,.37f),new Vector2(.97f,.65f),25);
        CampaignHUD.Explanation(summary.transform,ships==0?"No ships are available. New vessels take 30 days to build.":order==5?"The previous fleet was lost. Your captains must receive fresh orders before reports can bring rewards.":effects[order],new Vector2(.03f,.06f),new Vector2(.97f,.36f),23);
        var command=Row(1,180);
        CampaignHUD.Label(command.transform,"Command and equipment",new Vector2(.03f,.74f),new Vector2(.97f,.96f),27);
        float bonus=ships>0 && order==3?CourtRoster.Bonus(1,.05f,.15f)*100:0;
        CampaignHUD.Explanation(command.transform,"Alvise Diedo: "+CourtRoster.Status(1)+". Current archer support: +"+bonus.ToString("0")+"%.\nArtillery Ships: "+(TechnologySystem.Has("ArtilleryShips")?"active · defensive reports inflict twice the casualties.":"not adopted · available through Technologies.")+"\nDiedo supports the archers during wall assaults when ships are defending the Golden Horn.\nHarbour chain: "+(VariableSingleton.GetBoolVariable("bGoldenHornChain")?"intact":"removed")+" · Ottoman fleet: "+(CouncilActions.Int("iSpies")>=2?NavalState.EnemyShips+" vessels":"strength unknown")+" · "+NavalState.SeaControl,new Vector2(.03f,.08f),new Vector2(.97f,.72f),22);
        var missions=Row(2,220);missions.GetComponent<RectTransform>().anchoredPosition+=new Vector2(0,80);
        CampaignHUD.Label(missions.transform,"Orders and reports",new Vector2(.03f,.78f),new Vector2(.97f,.97f),27);
        CampaignHUD.Explanation(missions.transform,"Preparation grants +50% to the next successful report. Use Change fleet mission below for later orders; this costs a major action and clears the preparation bonus. Fleet-report events deliver the rewards below.\n\nSupply raids: −3 enemy food days · Coastal raids: +750 gold\nCrimean grain: +150 provisions · Harbour defence: −"+RecurringEvents.FleetDefensiveDamage+" enemy irregulars\nVolunteers: +50 infantry and +25 archers",new Vector2(.03f,.07f),new Vector2(.97f,.77f),23);
        var yard=Row(0,180+building.Count*35);yard.GetComponent<RectTransform>().anchoredPosition+=new Vector2(0,-590);
        CampaignHUD.Label(yard.transform,"The shipyards",new Vector2(.03f,.72f),new Vector2(.97f,.97f),27);
        CampaignHUD.Explanation(yard.transform,building.Count==0?"No warships are under construction. Build ships through Military and recruitment.":string.Join("\n",building),new Vector2(.03f,.1f),new Vector2(.97f,.70f),23);
        var orderRow=Row(0,150);orderRow.GetComponent<RectTransform>().anchoredPosition+=new Vector2(0,-950-building.Count*35);
        CampaignHUD.MakeButton(orderRow.transform,"Change fleet mission · major action",new Vector2(.03f,.2f),new Vector2(.97f,.8f),FleetOrders);
        var links=Row(0,150);links.GetComponent<RectTransform>().anchoredPosition+=new Vector2(0,-790-building.Count*35);
        CampaignHUD.MakeButton(links.transform,"Build warships",new Vector2(.03f,.2f),new Vector2(.47f,.8f),()=>{Hide();manager.BringUpTroopsUI();});
        CampaignHUD.MakeButton(links.transform,"Ongoing construction",new Vector2(.53f,.2f),new Vector2(.97f,.8f),Training);
    }

    public void Diplomacy()
    {
        Open("Diplomacy and court","Receive your advisers, send envoys abroad or exchange messages with the Sultan.");
        var links=Row(0,255);
        var incoming=new System.Collections.Generic.List<string>();
        for(int i=0;i<CouncilActions.Allies.Length;i++) if(CouncilActions.Int("iAidState"+i)==1)
            incoming.Add(CouncilActions.Allies[i].Name+": "+CouncilActions.Int("iAidDays"+i)+" days");
        float summaryHeight=System.Math.Max(130,70+incoming.Count*35);
        var summary=Row(0,summaryHeight);var sr=summary.GetComponent<RectTransform>();sr.anchoredPosition+=new Vector2(0,-255);
        BodyHeight(255+summaryHeight);
        CampaignHUD.Label(summary.transform,incoming.Count==0?"No foreign expeditions are currently committed.":"Committed reinforcements\n"+string.Join("\n",incoming),new Vector2(.03f,.06f),new Vector2(.97f,.94f),25);
        CampaignHUD.MakeButton(links.transform,"Diplomatic relations · 16 powers",new Vector2(.04f,.69f),new Vector2(.96f,.96f),DiplomaticRelations);
        CampaignHUD.MakeButton(links.transform,"Negotiate with the Sultan",new Vector2(.04f,.37f),new Vector2(.96f,.64f),Sultan);
        CampaignHUD.MakeButton(links.transform,"Court and characters",new Vector2(.04f,.05f),new Vector2(.48f,.32f),Court);
        CampaignHUD.MakeButton(links.transform,"Galatan diplomacy",new Vector2(.52f,.05f),new Vector2(.96f,.32f),Galata);
    }

    public void DiplomaticRelations()
    {
        Open("Diplomatic relations","One appeal per ally per campaign. Appeal: 1,000 gold and an available ship. Expedite: 1,000 gold, up to 5 days sooner. Both use the major action. Scroll for all allies.");

        ControlSingleton.Instance.ReturnAction=Diplomacy;
        BodyHeight(CouncilActions.Allies.Length*180);

        for(int i=0;i<CouncilActions.Allies.Length;i++)

        {

            int allyIndex=i;var ally=CouncilActions.Allies[i];int state=CouncilActions.Int("iAidState"+i);

            var row=Row(i,180);

            string status=state==0?$"{CouncilActions.AidChance(i)}% acceptance · {ally.Days} days travel":state==1?$"Aid due in {CouncilActions.Int("iAidDays"+i)} days":state==2?"Aid delivered":state==4?"Relief fleet lost at sea":"Appeal declined";

            CampaignHUD.Label(row.transform,ally.Name+" — "+status,new Vector2(.02f,.70f),new Vector2(.74f,.97f),25);

            CampaignHUD.Explanation(row.transform,ally.Description+"\nPromised package: "+ally.Cargo,new Vector2(.02f,.07f),new Vector2(.57f,.68f),20);

            var profile=DiplomaticChronicles.All[i];
            var faith=CampaignHUD.MakeButton(row.transform,profile.Religion+"\nAbout",new Vector2(.59f,.12f),new Vector2(.74f,.66f),()=>Chronicle(allyIndex));
            faith.GetComponentInChildren<TMP_Text>().fontSize=17;
            faith.GetComponentInChildren<TMP_Text>().rectTransform.anchorMax=new Vector2(1,.44f);
            IllustratedIdentity.Faith(faith.transform,profile.Religion,profile.Orthodox,new Vector2(.22f,.42f),new Vector2(.78f,.98f));

            var button=CampaignHUD.MakeButton(row.transform,state==1?"Expedite aid\n1,000 gold":state==0?"Request aid\n1,000 gold":"Resolved",new Vector2(.76f,.18f),new Vector2(.98f,.82f),()=>{string result;bool ok=state==1?CouncilActions.HurryAid(allyIndex,out result):CouncilActions.RequestAid(allyIndex,out result);Finish(ok,result);});

            button.interactable=(state==0 || state==1) && CouncilActions.Int("iMoney")>=1000 && (state==1 || CouncilActions.Int("iShips")>0) && (state!=1 || CouncilActions.Int("iAidDays"+i)>1);

        }

    }

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

    public void Sultan()
    {
        Open("Messages to the Sultan","Envoys carry your words between the palace and Mehmed's court. Each conversation topic is available once per day.");
        ControlSingleton.Instance.ReturnAction=Diplomacy;BodyHeight((SurrenderTerms.HasOffer?8:7)*160);
        if(SurrenderTerms.HasOffer) {var offer=Row(7,160);CampaignHUD.MakeButton(offer.transform,SurrenderTerms.Valid?"Review surrender terms":"Read expired offer",new Vector2(.08f,.2f),new Vector2(.92f,.8f),ReviewTerms);}
        for(int position=0;position<SultanNegotiations.Titles.Length;position++)
        {
            int i=new[]{0,1,3,4,2,5,6}[position];int choice=i;var row=Row(position,160);
            CampaignHUD.Label(row.transform,SultanNegotiations.Titles[i],new Vector2(.03f,.55f),new Vector2(.7f,.95f),27);
            CampaignHUD.Explanation(row.transform,SultanNegotiations.Effects[i],new Vector2(.03f,.08f),new Vector2(.7f,.53f),22);
            var button=CampaignHUD.MakeButton(row.transform,SultanNegotiations.Available(i)?"Send message":SultanNegotiations.Unavailable(i),new Vector2(.75f,.18f),new Vector2(.97f,.82f),()=>{
                bool ok=SultanNegotiations.Act(choice,out var report);
                if(!ok){notice.text=report;return;}
                if(choice==0) {ReviewTerms();return;}
                if(SultanNegotiations.IsConversation(choice)) CampaignHUD.Instance.ShowReport(SultanNegotiations.Titles[choice],report,()=>{CampaignHUD.Instance.DismissReport();Sultan();});
                else Finish(true,report);
            });button.interactable=SultanNegotiations.Available(i);
        }
    }

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

    public void Politics()
    {
        Open("Political dealings","A bloc needs 20% influence to act. Calling in a favour uses the major action and transfers up to 5 influence points to that bloc.");
        ControlSingleton.Instance.ReturnAction=Factions;
        BodyHeight(1080);
        var chart=Row(0,240);
        var pie=CampaignHUD.Panel(chart.transform,"Influence chart",new Vector2(.025f,.08f),new Vector2(.19f,.92f));
        pie.GetComponent<Image>().color=Color.clear;
        var pr=pie.GetComponent<RectTransform>();pr.anchorMin=pr.anchorMax=new Vector2(.10f,.5f);pr.sizeDelta=new Vector2(190,190);pr.anchoredPosition=Vector2.zero;
        PoliticalSystem.DrawPie(pie.transform);
        string legend="";
        for(int i=0;i<4;i++) legend+="<color=#"+ColorUtility.ToHtmlStringRGB(Color.Lerp(PoliticalSystem.Colours[i],Color.white,.45f))+">"+PoliticalSystem.Names[i]+"</color>  "+PoliticalSystem.Share(i)+"%\n";
        CampaignHUD.Label(chart.transform,legend,new Vector2(.25f,.08f),new Vector2(.96f,.93f),26);
        for(int i=0;i<4;i++)
        {
            int bloc=i;var row=Row(i+1,205);
            var rect=row.GetComponent<RectTransform>();rect.anchoredPosition+=new Vector2(0,-35);
            CampaignHUD.Label(row.transform,PoliticalSystem.Names[i]+" - "+PoliticalSystem.Share(i)+"%"+(PoliticalSystem.Banned(i)?" | Banished":""),new Vector2(.02f,.76f),new Vector2(.66f,.98f),26);
            CampaignHUD.Explanation(row.transform,PoliticalSystem.Beliefs[i]+"\n\n"+PoliticalSystem.Effects[i]+" | Influence +5",new Vector2(.02f,.05f),new Vector2(.66f,.74f),21);
            var order=CampaignHUD.MakeButton(row.transform,PoliticalSystem.Banned(i)?"Faction banished":PoliticalSystem.Share(i)<20?"Requires 20% influence":i==2&&!PoliticalSystem.CanAid(i)?"Awaiting a Western expedition":PoliticalSystem.Orders[i],new Vector2(.69f,.43f),new Vector2(.98f,.94f),()=>{bool ok=PoliticalSystem.Aid(bloc,out var report);Finish(ok,report);});
            order.interactable=PoliticalSystem.CanAid(i);
            if(i>0){var ban=CampaignHUD.MakeButton(row.transform,"Banish faction...",new Vector2(.69f,.06f),new Vector2(.98f,.36f),()=>{
                CampaignHUD.Instance.ShowReport("Banishment",PoliticalSystem.BanishPreview(bloc),()=>{CampaignHUD.Instance.DismissReport();bool ok=PoliticalSystem.Banish(bloc,out var report);Finish(ok,report);});
                var modal=GameObject.Find("Event outcome");
                foreach(var button in modal.GetComponentsInChildren<Button>()) button.GetComponentInChildren<TMP_Text>().text="Confirm banishment";
                CampaignHUD.MakeButton(modal.transform,"Cancel",new Vector2(.69f,.07f),new Vector2(.88f,.16f),()=>CampaignHUD.Instance.DismissReport());
            });ban.interactable=!PoliticalSystem.Banned(i)&&CouncilActions.Int("iBanishmentUsed")==0;}
        }
    }


    public void Overview()
    {
        Open("Siege overview",VariableSingleton.HasSiegeStarted()?"Siege day "+VariableSingleton.SiegeDay+" · "+VariableSingleton.GetDate().ToString("d MMMM yyyy"):"Preparing the City · "+VariableSingleton.GetDate().ToString("d MMMM yyyy"));
        BodyHeight(1440);
        var defence=SpyPanel("Defence summary",0,230);
        string wall=VariableSingleton.Instance.EnduringWall.GetCodeName();
        wall=wall=="OuterWall"?"Theodosian Wall":wall=="MiddleWall"?"Constantinian Wall":wall=="InnerWall"?"Severan Wall":wall;
        OverviewMeter(defence.transform,"ACTIVE DEFENCE · "+wall,VariableSingleton.Instance.EnduringWall.GetHealth(),.03f,.48f);
        OverviewMeter(defence.transform,"MOAT",VariableSingleton.GetFloatVariable("fMoat"),.52f,.97f);
        var respite=SpyPanel("Operations delay",244,170);int delay=CouncilActions.Int("iEnemyDelay");
        CampaignHUD.Label(respite.transform,"OPERATIONS DELAY  ·  "+delay+(delay==1?" day":" days"),new Vector2(.025f,.54f),new Vector2(.97f,.94f),28);
        CampaignHUD.Explanation(respite.transform,delay>0?"The next "+delay+" Ottoman operations are held back. Each delay buys a siege day free of attacks and siege works; other daily needs continue. Food shortages can postpone operations before this reserve is used.":"No delay is in reserve. Agents can disrupt Ottoman preparations to buy the defenders time.",new Vector2(.025f,.05f),new Vector2(.97f,.52f),22);
        var network=SpyPanel("Network summary",428,110);
        CampaignHUD.Label(network.transform,"AGENTS  "+CouncilActions.Int("iSpies")+"     SECRETS  "+CouncilActions.Int("iSecrets")+"     ENEMY CAMP ALERT  "+EspionageSystem.Alert+"%",new Vector2(.025f,.17f),new Vector2(.97f,.86f),27);
        for(int n=0;n<8;n++) {
            int col=n%2,row=n/2;bool known=CouncilActions.Int("iSpies")>n;
            var item=SpyPanel("Enemy report "+n,552+row*120,108);var rect=item.GetComponent<RectTransform>();rect.anchorMin=new Vector2(col*.5f,1);rect.anchorMax=new Vector2(col*.5f+.49f,1);
            var label=CampaignHUD.Label(item.transform,EspionageSystem.IntelligenceLabels[n],new Vector2(.04f,.51f),new Vector2(.96f,.94f),24);
            var value=CampaignHUD.Label(item.transform,known?EspionageSystem.IntelligenceValue(n):"Requires "+(n+1)+" agents",new Vector2(.04f,.06f),new Vector2(.96f,.49f),23);
            if(!known){label.color=value.color=new Color(.55f,.60f,.63f);value.fontStyle|=FontStyles.Italic;}
        }
        var situation=SpyPanel("Command and siege works",1048,260);
        int commander=CouncilActions.Int("iCommander");string command=CourtRoster.CommanderName(commander);
        CampaignHUD.Label(situation.transform,"SIEGE COMMANDER · "+command,new Vector2(.025f,.78f),new Vector2(.97f,.98f),26);
        CampaignHUD.Explanation(situation.transform,"Enemy ramp: "+VariableSingleton.GetPercentFloat("fSiegeRamp")+" · Threatens the walls when complete.\nSiege tower: "+(VariableSingleton.GetBoolVariable("bSiegeTower")?"ready":"none")+" · Battering ram: "+(VariableSingleton.GetBoolVariable("bBatteringRam")?"ready":"none")+"\n"+NavalState.SeaControl+" · Our ships: "+CouncilActions.Int("iShips")+" · Ottoman vessels: "+(CouncilActions.Int("iSpies")>=2?NavalState.EnemyShips.ToString():"requires 2 agents")+"\nAmmunition: "+CouncilActions.Int("iAmmunition")+" · Provisions: "+CouncilActions.Int("iProvisions")+" · Daily consumption: "+SupplyEconomy.DailyNeed,new Vector2(.025f,.05f),new Vector2(.97f,.76f),23);
        var links=SpyPanel("Overview navigation",1320,95);
        CampaignHUD.MakeButton(links.transform,"Spies and intelligence",new Vector2(.03f,.12f),new Vector2(.48f,.88f),Spies);
        CampaignHUD.MakeButton(links.transform,"Defence of the walls",new Vector2(.52f,.12f),new Vector2(.97f,.88f),()=>{Hide();manager.BringUpSiegeUI();});
    }
    private void OverviewMeter(Transform parent,string name,float value,float left,float right)
    {
        var title=CampaignHUD.Label(parent,name,new Vector2(left,.70f),new Vector2(right,.96f),25);
        var amount=CampaignHUD.Label(parent,Mathf.RoundToInt(value*100)+"% integrity",new Vector2(left,.38f),new Vector2(right,.68f),30);
        var track=CampaignHUD.Panel(parent,name+" meter",new Vector2(left,.15f),new Vector2(right,.29f));ImperialTheme.Panel(track,false);
        var fill=CampaignHUD.Panel(track.transform,"Integrity",Vector2.zero,new Vector2(Mathf.Clamp01(value),1));ImperialTheme.Panel(fill);fill.GetComponent<Image>().color=value<.25f?new Color(.65f,.27f,.20f):new Color(.35f,.70f,.78f);
        title.alignment=amount.alignment=TextAlignmentOptions.Center;
    }

    public void Technologies()

    {

        Open("Technologies and workshops","Secrets represent expertise and contacts. Research uses your major action. Obtain secrets through events or Spies > Gather secrets. Scroll to see every research option.");

        notice.text="Available secrets: "+CouncilActions.Int("iSecrets");

        BodyHeight(TechnologySystem.All.Length*180);

        for(int i=0;i<TechnologySystem.All.Length;i++)

        {

            int choice=i;var technology=TechnologySystem.All[i];var row=Row(i,180);

            bool active=VariableSingleton.GetBoolVariable(technology.Key);

            CampaignHUD.Label(row.transform,technology.Name+(active?" — active":""),new Vector2(.02f,.72f),new Vector2(.72f,.98f),27);

            CampaignHUD.Explanation(row.transform,technology.Effect+"\n<color=#B8CFDA><i>"+technology.History+"</i></color>",new Vector2(.02f,.05f),new Vector2(.72f,.70f),21);

            var button=CampaignHUD.MakeButton(row.transform,active?"Active":technology.Cost+" secrets\nResearch",new Vector2(.76f,.18f),new Vector2(.98f,.82f),()=>{bool ok=TechnologySystem.Unlock(choice,out var report);Finish(ok,report);});

            button.interactable=!active && VariableSingleton.GetIntVariable("iSecrets")>=technology.Cost;

            if(!active && CouncilActions.Int("iSecrets")<technology.Cost) button.GetComponentInChildren<TMP_Text>().text="Requires "+technology.Cost+" secrets";

            if(active) { var colours=button.colors;colours.disabledColor=new Color(1f,.78f,.32f,1);button.colors=colours;row.GetComponent<Image>().color=new Color(.22f,.17f,.075f,1);button.GetComponentInChildren<TMP_Text>().color=new Color(1f,.9f,.57f); }
            if(i==2 && !active) {button.interactable=false;button.GetComponentInChildren<TMP_Text>().text="Not yet available";}

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
        for(int i=0;i<8;i++) {
            if(!CourtRoster.InService(i))continue;
            var row=Row(position++,230);IllustratedIdentity.Portrait(row.transform,i);
            CampaignHUD.Label(row.transform,CourtRoster.Names[i],new Vector2(.20f,.74f),new Vector2(.98f,.98f),29);
            CampaignHUD.Label(row.transform,CourtRoster.Status(i)+" · Loyalty "+CouncilActions.Int("iLoyalty"+i)+" / 100 · "+PoliticalSystem.Allegiance(i),new Vector2(.20f,.53f),new Vector2(.98f,.75f),22);
            CampaignHUD.Explanation(row.transform,CourtRoster.Roles[i]+"\n"+CourtRoster.Notes[i],new Vector2(.20f,.09f),new Vector2(.60f,.52f),22);
            CampaignHUD.Label(row.transform,"<color=#E8C36F>"+CourtRoster.Abilities[i]+"</color>",new Vector2(.64f,.09f),new Vector2(.97f,.51f),23);
        }
        if(shahServing) {var row=Row(position++,230);CampaignHUD.Label(row.transform,"Shaherezade — "+SultanGifts.Status+"\n\n"+SultanGifts.Note,new Vector2(.03f,.08f),new Vector2(.97f,.94f),25);}
        if(CharacterPreparations.InService) {
            int c=CharacterPreparations.Candidate;var row=Row(position++,230);
            CampaignHUD.Label(row.transform,CharacterPreparations.Names[c]+" · "+CharacterPreparations.Status+" · Loyalty "+CharacterPreparations.Loyalty+" / 100",new Vector2(.03f,.72f),new Vector2(.97f,.97f),26);
            CampaignHUD.Explanation(row.transform,CharacterPreparations.Roles[c]+"\n"+CharacterPreparations.Profiles[c],new Vector2(.03f,.13f),new Vector2(.56f,.68f),23);
            CampaignHUD.Label(row.transform,"<color=#E8C36F>"+CharacterPreparations.Abilities[c]+"</color>",new Vector2(.60f,.40f),new Vector2(.97f,.70f),23);
            var appoint=CampaignHUD.MakeButton(row.transform,CharacterPreparations.IsCommander?"Siege commander":"Appoint commander · major action",new Vector2(.60f,.08f),new Vector2(.97f,.36f),()=>{bool ok=CharacterPreparations.Appoint(out var report);Finish(ok,report);});appoint.interactable=!CharacterPreparations.IsCommander;
        }
        if(position==0)CampaignHUD.Label(body,"No advisers are currently in your service.",new Vector2(.04f,.2f),new Vector2(.96f,.9f),28);
        if(CharacterPreparations.Invited&&!CharacterPreparations.InService)notice.text="Expected arrival: "+CharacterPreparations.Name()+" · "+CharacterPreparations.Status;
    }

    private int[] purchaseAmounts={1000,100,20};
    public void Market()
    {
        Open("Treasury and market","Purchases use gold and remain minor actions. Prices for provisions rise when the siege begins.");
        BodyHeight(780);
        var account=Row(0,120);
        CampaignHUD.Label(account.transform,"Treasury: "+CouncilActions.Int("iMoney").ToString("N0")+" gold   ·   Food: "+SupplyEconomy.DaysLeft+" full days\nDaily consumption during the siege: "+SupplyEconomy.DailyNeed+" provisions",new Vector2(.02f,.08f),new Vector2(.98f,.92f),25);
        string[] resources={"iProvisions","iMaterials","iAmmunition"},names={"Provisions","Materials","Ammunition"};int[] steps={100,10,1};
        for(int i=0;i<3;i++) {
            int index=i;string resource=resources[i];var row=Row(i+1,150);
            CampaignHUD.Label(row.transform,names[i]+": "+CouncilActions.Int(resource).ToString("N0")+"\n"+SupplyEconomy.Price(resource)+" gold each",new Vector2(.02f,.1f),new Vector2(.35f,.9f),24);
            CampaignHUD.MakeButton(row.transform,"−",new Vector2(.39f,.22f),new Vector2(.46f,.78f),()=>{purchaseAmounts[index]=Mathf.Max(steps[index],purchaseAmounts[index]-steps[index]);Market();});
            var quantity=CampaignHUD.Label(row.transform,purchaseAmounts[i].ToString("N0"),new Vector2(.47f,.2f),new Vector2(.59f,.8f),27);quantity.alignment=TextAlignmentOptions.Center;
            CampaignHUD.MakeButton(row.transform,"+",new Vector2(.60f,.22f),new Vector2(.67f,.78f),()=>{purchaseAmounts[index]=Mathf.Min(100000,purchaseAmounts[index]+steps[index]);Market();});
            long cost=(long)purchaseAmounts[i]*SupplyEconomy.Price(resource);
            var buy=CampaignHUD.MakeButton(row.transform,"Buy\n"+cost.ToString("N0")+" gold",new Vector2(.71f,.17f),new Vector2(.97f,.83f),()=>{SupplyEconomy.Buy(resource,purchaseAmounts[index],out var report);Market();notice.text=report;});buy.interactable=cost<=CouncilActions.Int("iMoney");
        }
        var tax=Row(4,150);CampaignHUD.Label(tax.transform,"Emergency levy: +"+(SupplyEconomy.DailyNeed*100).ToString("N0")+" gold\nPublic morale −15 points · Major action",new Vector2(.02f,.1f),new Vector2(.65f,.9f),24);
        CampaignHUD.MakeButton(tax.transform,"Collect emergency tax",new Vector2(.70f,.17f),new Vector2(.97f,.83f),()=>{SupplyEconomy.EmergencyTax(out var report);Finish(true,report);});
    }

    public void WallSpecialActions()
    {
        Open("Wall defences · Special actions","Direct your artillery against the Ottoman batteries. Firing uses your major action.");
        ControlSingleton.Instance.ReturnAction=()=>{Hide();manager.BringUpSiegeUI();};
        BodyHeight(360);
        for(int i=0;i<2;i++) {
            bool grand=i==1;var row=Row(i,180);
            CampaignHUD.Label(row.transform,grand?"Fire the Grand Cannon":"Counter-battery fire",new Vector2(.03f,.65f),new Vector2(.69f,.96f),28);
            CampaignHUD.Explanation(row.transform,grand?"Turn the great gun against the enemy batteries.":"Your gunners concentrate their fire on the Ottoman artillery.",new Vector2(.03f,.38f),new Vector2(.69f,.64f),21);
            CampaignHUD.Label(row.transform,"3 ammunition · Destroy 1–5 enemy cannons",new Vector2(.03f,.08f),new Vector2(.69f,.37f),22);
            string reason=TechnologySystem.ArtilleryUnavailable(grand);
            var fire=CampaignHUD.MakeButton(row.transform,reason??"Fire · major action",new Vector2(.72f,.15f),new Vector2(.97f,.85f),()=>{string report;bool ok=grand?TechnologySystem.FireGrandCannon(out report):TechnologySystem.CounterBattery(out report);Finish(ok,report);});
            fire.interactable=reason==null;
        }
    }

    public void Palace()

    {

        Open("The Palace","Review the Emperor's position. Recruitment remains a minor action; a council mission, audience, repair or rest spends the major action.");

        BodyHeight(600);

        CampaignHUD.Label(body,"Health: "+VariableSingleton.GetPercentFloat("fHealth")+"\nSiege Focus: "+FocusIcon.Names[Mathf.Clamp(CouncilActions.Int("iFocus"),0,4)]+"\nPopulation: "+VariableSingleton.PopulationInPeople()+"\nMerchant favour: "+DailyLifeEvents.I("iMerchantFavor",50)+" / 100\nFencing: "+EmperorCombat.Skill+" / 100"+"\n\n<i>Rest restores health and ends management. Your council will then hear the next event.</i>",new Vector2(.03f,.61f),new Vector2(.97f,.98f),23);



        CampaignHUD.MakeButton(body,"Practise fencing · major action · +5 skill / -5 health",new Vector2(.08f,.27f),new Vector2(.92f,.43f),()=>{bool ok=EmperorCombat.Train(out var report);Finish(ok,report);});
        CampaignHUD.MakeButton(body,"Lead next wall assault / cancel · costs 5 health when fighting",new Vector2(.08f,.46f),new Vector2(.92f,.59f),()=>{notice.text=EmperorCombat.ToggleWall();});


        CampaignHUD.MakeButton(body,"Rest — use the major action",new Vector2(.08f,.03f),new Vector2(.92f,.16f),()=>{Hide();manager.ContinueToRest();});

    }

}

