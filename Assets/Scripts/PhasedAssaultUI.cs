using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhasedAssaultUI : MonoBehaviour
{
    public const float VolleyDuration=7f,CoverDuration=3f,SortieDuration=8f;
    public static bool CanAnimate {get {bool focus=Application.isFocused;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(TestFocus&&!string.IsNullOrEmpty(VariableSingleton.TestSavePath))focus=true;
#endif
        return focus&&(CampaignHUD.Instance==null||!CampaignHUD.Instance.HasModal)&&(Current==null||!Current.paused);}}
    public static bool Enabled=true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool TestFocus;
#endif
    public static bool ActiveMode { get=>PlayerPrefs.GetInt("Besieged.ActiveAssault",0)==1; set {PlayerPrefs.SetInt("Besieged.ActiveAssault",value?1:0);PlayerPrefs.Save();} }
    public static PhasedAssaultUI Current;
    public PhasedAssault Battle;public int CurrentIncident=-1;
    GameObject panel,choices,arena; TMP_Text title,description,ledger,timer,phaseText;RectTransform marker;
    float elapsed,deadline; RectTransform formation,hourglass; int lastEnemyDead,lastOurDead;Action timeout; bool active,paused,waiting;string activity="";int sortieStrength;
    BattleCrowdView crowd;TMP_Text cue;RectTransform aim;Vector2 aimPoint=new Vector2(.5f,.45f);bool loaded,drawn;float drawAge,feedbackUntil;int firedWindow=-1;Button loadButton,drawButton,fireButton;bool contactsRunning,approachIncident,contactIncident,cannonOffered;
    public static void Open(PhasedAssault battle,Transform parent) {
        if(Current!=null)return;
        var go=new GameObject("Phased assault controller");var ui=go.AddComponent<PhasedAssaultUI>();Current=ui;ui.Battle=battle;ui.active=ActiveMode;ui.Build(parent);ui.Briefing();
    }
    void Build(Transform parent) {
        ControlSingleton.Instance.ClearHistory();
        panel=CampaignHUD.Panel(parent,"The assault",new Vector2(.015f,.015f),new Vector2(.985f,.865f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
        title=CampaignHUD.Label(panel.transform,"",new Vector2(.035f,.91f),new Vector2(.74f,.98f),32);
        phaseText=CampaignHUD.Explanation(panel.transform,"",new Vector2(.035f,.85f),new Vector2(.96f,.91f),19);
        arena=CampaignHUD.Panel(panel.transform,"Battlefield",new Vector2(.035f,.45f),new Vector2(.64f,.83f));
        var art=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/assault-battlefield");if(art==null)art=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/walls-war");
        SceneBackdrop.Apply(arena,Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/assault-battlefield")!=null?"assault-battlefield":"walls-war");
        crowd=arena.AddComponent<BattleCrowdView>();crowd.Initialise(Battle);
        cue=CampaignHUD.Label(arena.transform,"",new Vector2(.04f,.84f),new Vector2(.96f,.99f),21);cue.alignment=TextAlignmentOptions.Center;cue.fontStyle=FontStyles.Bold;cue.raycastTarget=false;
        aim=Picture(arena.transform,"Arrow landing area",AssaultImpact.Glow,new Vector2(.4f,.35f),new Vector2(.6f,.5f));aim.GetComponent<RawImage>().color=new Color(.95f,.78f,.30f,.35f);aim.gameObject.SetActive(false);
        var read=CampaignHUD.Panel(panel.transform,"Situation",new Vector2(.035f,.24f),new Vector2(.64f,.43f));read.GetComponent<Image>().color=ImperialTheme.Navy;
        description=CampaignHUD.Explanation(read.transform,"",new Vector2(.025f,.02f),new Vector2(.975f,.98f),23);description.enableAutoSizing=true;description.fontSizeMin=18;
        var log=CampaignHUD.Panel(panel.transform,"Assault ledger",new Vector2(.66f,.24f),new Vector2(.965f,.83f));log.GetComponent<Image>().color=ImperialTheme.Navy;
        ledger=CampaignHUD.Explanation(log.transform,"",new Vector2(.04f,.04f),new Vector2(.96f,.96f),22);ledger.enableAutoSizing=true;ledger.fontSizeMin=17;
        choices=CampaignHUD.Panel(panel.transform,"Orders",new Vector2(.035f,.025f),new Vector2(.965f,.22f));choices.GetComponent<Image>().color=Color.clear;
        timer=CampaignHUD.Label(panel.transform,"",new Vector2(.75f,.92f),new Vector2(.96f,.98f),23);
        var hour=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/assault-hourglass");if(hour!=null){hourglass=Picture(panel.transform,"Hourglass",hour,new Vector2(.72f,.92f),new Vector2(.75f,.98f));}
    }
    static RectTransform Picture(Transform parent,string name,Texture2D texture,Vector2 min,Vector2 max) {
        var go=new GameObject(name,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
        var image=go.GetComponent<RawImage>();image.texture=texture;image.raycastTarget=false;return r;
    }
    void Screen(string heading,string text,int phase) {
        waiting=false;timeout=null;activity="";timer.text="";if(cue!=null)cue.text="";if(aim!=null)aim.gameObject.SetActive(false);
        foreach(Transform child in choices.transform)Destroy(child.gameObject);
        if(marker!=null)Destroy(marker.parent.gameObject);
        title.text=heading;description.text=text;
        string[] names={"Command","Approach","Engines","At the walls","Breach","Aftermath"};
        phaseText.text="";for(int i=0;i<names.Length;i++)phaseText.text+=(i==phase?"<color=#E8C36F>"+names[i]+"</color>":names[i])+(i<5?"   →   ":"");
        RefreshLedger();
    }
    void Order(string label,int slot,int count,Action action,bool enabled=true) {
        float w=1f/count;var button=CampaignHUD.MakeButton(choices.transform,label,new Vector2(slot*w+.008f,.07f),new Vector2((slot+1)*w-.008f,.93f),()=>{if(paused||CampaignHUD.Instance.HasModal)return;action();});button.interactable=enabled;
        var text=button.GetComponentInChildren<TMP_Text>();text.enableAutoSizing=true;text.fontSizeMin=16;text.fontSizeMax=23;
    }
    void RefreshLedger() {
        if(Battle.Dead>lastEnemyDead || Battle.OurDead>lastOurDead)Impact(Battle.OurDead>lastOurDead);
        lastEnemyDead=Battle.Dead;lastOurDead=Battle.OurDead;if(crowd!=null)crowd.FloatLosses();
        ledger.text="<b>THIS ASSAULT</b>\nCommitted attackers: "+Battle.Attackers.ToString("N0")+"\nEnemy survivors: "+Battle.EnemyLeft.ToString("N0")+"\nEnemy dead: "+Battle.Dead.ToString("N0")+"\nOur dead: "+Battle.OurDead+"\n\nArchers ×"+Battle.VolleyMultiplier.ToString("0.00")+"\nReserve guards: "+Battle.Guards+"\nBreach pressure: "+(Battle.Finished?0:Battle.GuardsNeeded)+"\nEmperor health: "+Battle.Health+"%\n\n"+(Battle.Journal.Count>0?Battle.Journal[Battle.Journal.Count-1]:"No casualties yet. Orders below affect only this assault.");
    }
    public void Briefing() {
        Screen("The Ottoman assault","Choose where you command. Frontline leadership adds 8% to melee output but increases exposure and can bring a personal emergency. Safe command has no personal combat risk. Timed play is optional."+(!Battle.Practice?"\n"+DefencePosture.Summary:"")+(Battle.Frontline?" Your palace order to join this assault is recorded; confirm it below or choose shelter.":""),0);
        if(!Battle.Practice)Order("Review land / sea dispositions",3,4,()=>DefencePosture.Show(FindObjectOfType<ManagerUI>(true),()=>{AI.Instance.RefreshDisposition(Battle);Briefing();}));
        Order(active?"Minigames: ON\nSwitch off":"Minigames: OFF\nSwitch on",0,Battle.Practice?3:4,()=>{active=!active;ActiveMode=active;Briefing();});
        Order("Command from shelter",1,Battle.Practice?3:4,()=>{Battle.Frontline=false;PrepareApproach();});
        Order(Battle.Frontline?"Confirm palace order\nJoin the parapet":"Join the parapet\nPersonal risk",2,Battle.Practice?3:4,()=>{Battle.Frontline=true;PrepareApproach();});
    }
    void BeginTimer(string kind,float seconds,Action onTimeout) {
        activity=kind;elapsed=0;deadline=seconds;timeout=onTimeout;waiting=true;Battle.TimedMoments++;
        var track=CampaignHUD.Panel(arena.transform,"Timing track",new Vector2(.08f,.10f),new Vector2(.92f,.21f));track.GetComponent<Image>().color=ImperialTheme.Navy;
        var zone=CampaignHUD.Panel(track.transform,"Effective range",new Vector2(kind=="cover"?.45f:.30f,0),new Vector2(kind=="cover"?.80f:.70f,1));zone.GetComponent<Image>().color=new Color(.22f,.40f,.36f);
        var centre=CampaignHUD.Panel(track.transform,"Ideal range",new Vector2(kind=="cover"?.57f:.36f,0),new Vector2(kind=="cover"?.68f:.64f,1));centre.GetComponent<Image>().color=new Color(.8f,.64f,.3f);
        marker=CampaignHUD.Panel(track.transform,"Enemy advance",new Vector2(0,0),new Vector2(.015f,1)).GetComponent<RectTransform>();marker.GetComponent<Image>().color=Color.white;
    }
    void PrepareApproach(){if(!approachIncident){approachIncident=true;Incident(0,Cover);}else Cover();}
    void Incident(int phase,Action next){
        int id=Battle.Practice&&AssaultPractice.ForcedIncident>=0&&AssaultIncidents.All[AssaultPractice.ForcedIncident].Phase==phase?AssaultPractice.ForcedIncident:AssaultIncidents.Select(phase,Battle);
        if(id<0){next();return;}var e=AssaultIncidents.All[id];
        Screen(e.Title,e.Text,phase==0?1:3);CurrentIncident=id;Battle.Journal.Add("Incident: "+e.Title);
        for(int i=0;i<e.Choices.Length;i++){int choiceIndex=i;var c=e.Choices[i];string why=AssaultIncidents.Unavailable(c,Battle);Order(c.Name+"\n"+(why??(string.IsNullOrEmpty(c.Terms)?"Ordinary response":c.Terms)),i,e.Choices.Length,()=>{if(CurrentIncident==id&&AssaultIncidents.Choose(id,choiceIndex,Battle)){CurrentIncident=-1;RefreshLedger();next();}},why==null);}
    }
    void Cover() {
        if(!active || CouncilActions.Int("iTurkCannons")==0){Battle.Shelter(.65f);Volley();return;}
        Screen("Watch the Ottoman gun crews","The fuse is lit. Order cover as the gun crews pull back. Green/gold means safe timing; waiting through the blast risks archers. Read first, then start the three-second challenge.",1);
        Order("Start cover drill · 3 seconds",0,1,()=>{
            OrderLive("Take cover [Space]",()=>{Battle.Shelter(Mathf.Clamp01(elapsed/CoverDuration));Volley();});
            BeginTimer("cover",CoverDuration,()=>{Battle.Shelter(1f);Volley();});
        });
    }
    public void Volley() {
        if(!cannonOffered&&Battle.Cannons>0&&Battle.Ammo>0&&(active||Battle.ArrowScreens)){cannonOffered=true;Cannon();return;}
        Screen("Archers to the parapet",active?"Three volleys over seven seconds. A: load. W: draw. D: fire. Move the mouse over the battlefield to place your landing area. Loose when the fresh rank enters effective range. Instructions are untimed; start when ready.":"Minigames are off: the captains manage cover and range at ordinary strength. No timing bonus, penalty or cover casualties apply.",1);
        if(Battle.Archers==0){AfterVolley();return;}
        if(active)Order("Start volleys · 7 seconds",0,1,StartVolleys);
        else Order("Automatic volleys · ×1.00",0,1,()=>{Battle.Shelter(.65f);while(Battle.Shots<3)Battle.Volley(0);Arrows();AfterVolley();});
    }
    void OrderLive(string label,Action action){foreach(Transform t in choices.transform)Destroy(t.gameObject);Order(label,0,1,action);}
    void StartVolleys(){
        description.text="A: load → W: draw → D: loose. Aim at the advancing rank. Gold marks the best timing. Three volleys; seven seconds.";feedbackUntil=0;
        foreach(Transform t in choices.transform)Destroy(t.gameObject);loaded=drawn=false;firedWindow=-1;drawAge=0;aim.gameObject.SetActive(true);
        loadButton=LiveButton("A · Load",0,LoadBow);drawButton=LiveButton("W · Draw",1,DrawBow);fireButton=LiveButton("D · Fire",2,Fire);
        BeginTimer("volley",VolleyDuration,()=>{while(Battle.Shots<3)Battle.Volley(-.25f);AfterVolley();});
    }
    Button LiveButton(string text,int n,Action action){Order(text,n,3,action);return choices.transform.GetChild(choices.transform.childCount-1).GetComponent<Button>();}
    void LoadBow(){if(activity!="volley"||!waiting)return;int window=Mathf.Min(2,(int)(elapsed/(VolleyDuration/3)));if(firedWindow==window||loaded)return;loaded=true;drawn=false;drawAge=0;}
    void DrawBow(){if(activity=="volley"&&waiting&&loaded&&!drawn){drawn=true;drawAge=0;}}

    void Fire() {
        if(activity!="volley"||!waiting||!loaded||!drawn||drawAge<.16f)return;
        float slice=VolleyDuration/3;int window=Mathf.Min(2,(int)(elapsed/slice));if(firedWindow==window)return;
        while(Battle.Shots<window)Battle.Volley(-.25f);
        float progress=(elapsed-window*slice)/slice;Vector2 target=new Vector2(Mathf.Lerp(.83f,.20f,progress),Mathf.Lerp(.64f,.16f,progress));
        float aimPenalty=Mathf.Clamp(Vector2.Distance(aimPoint,target)-.14f,0,.5f)*.5f;
        float score=Mathf.Clamp(PhasedAssault.VolleyScore(progress)-aimPenalty,-.25f,.25f);Battle.Volley(score);firedWindow=window;loaded=drawn=false;feedbackUntil=elapsed+.45f;
        Arrows();RefreshLedger();cue.text="Volley "+(window+1)+"/3 · "+score.ToString("+0%;-0%;0%")+" · reloading";
    }
    void AfterVolley() {
        waiting=false;crowd.Advance(.78f);Battle.Approach(VariableSingleton.GetFloatVariable("fMoat"),TechnologySystem.Has("GreekFire")?Battle.Attackers*.05f:0);
        if(Battle.EnemyLeft<=0){FinishBattle(true);return;}
        if(Battle.Tower||Battle.Ram)SortieMenu();else Contact();
    }
    void Cannon(){
        if(!active){Screen("Choose the gunners' target","Timber mantlets protect the advancing ranks from arrows. Fire into the ranks for ordinary cannon damage, or shatter the screens: half cannon damage now, full arrow effect afterwards. This choice is untimed.",1);Order("Fire into the ranks",0,2,()=>{Battle.FireGun(0);Volley();});Order("Shatter the arrow shields",1,2,()=>{Battle.FireAtScreens();Volley();});return;}
        Screen("Command the wall gun","Move the mouse over the ground to aim. Fire once into the approaching ranks: D or the fire button. Your shot uses this assault's gun allocation; it cannot create unlimited extra damage. This aiming encounter is untimed.",1);
        Order("Take the gun · aim and fire",0,Battle.ArrowScreens?3:2,()=>{foreach(Transform t in choices.transform)Destroy(t.gameObject);Order("Fire the cannon [D]",0,1,CannonShot);activity="cannon";waiting=true;elapsed=0;aim.gameObject.SetActive(true);cue.text="AIM INTO THE DENSEST RANKS · D: FIRE";crowd.Advance(.5f,true);});
        Order("Let the gunners fire normally",1,Battle.ArrowScreens?3:2,Volley);
        if(Battle.ArrowScreens)Order("Shatter the arrow shields\nHalf gun damage; restore arrows",2,3,()=>{Battle.FireAtScreens();RefreshLedger();Volley();});
    }
    void CannonShot(){
        if(activity!="cannon"||!waiting)return;waiting=false;float distance=Vector2.Distance(aimPoint,new Vector2(.515f,.40f));Battle.FireGun(Mathf.Clamp(.25f-distance,-.25f,.25f));crowd.Blast(aimPoint);RefreshLedger();cue.text="SHOT AWAY · "+Battle.Dead+" total enemy losses";StartCoroutine(AfterCannon());
    }
    System.Collections.IEnumerator AfterCannon(){float t=0;while(t<.65f){if(CanAnimate)t+=Time.unscaledDeltaTime;yield return null;}Volley();}
    void SortieMenu() {
        Screen("A target outside the gate","A siege engine has advanced beyond its escort. A brief sortie risks fewer men; a heavy sortie has a better chance of destroying it. Casualties rise sharply with time outside. Success reduces breach pressure by 12.",2);
        Order("Keep the gates shut",0,3,()=>{Battle.Sortie(0,0);Contact();});
        Order("Send up to 100 raiders",1,3,()=>StartSortie(1),Battle.Infantry>0);
        Order("Send up to 300 men",2,3,()=>StartSortie(2),Battle.Infantry>0);
    }
    void StartSortie(int strength) {
        sortieStrength=strength;Screen("Recall the sortie","Enemy reinforcements gather beyond the smoke. A longer stay improves the chance of burning the engine but increases losses. If time expires, the party withdraws.",2);
        if(active&&Battle.TimedMoments<2){Order("Begin sortie · 8 seconds",0,1,()=>{OrderLive("Recall now",()=>{Battle.Sortie(sortieStrength,Mathf.Clamp01(elapsed/SortieDuration));Contact();});BeginTimer("sortie",SortieDuration,()=>{Battle.Sortie(sortieStrength,1);Contact();});});}
        else{Order("Quick strike\nLow exposure",0,3,()=>{Battle.Sortie(strength,.2f);Contact();});Order("Press the attack\nModerate exposure",1,3,()=>{Battle.Sortie(strength,.55f);Contact();});Order("Stay until it burns\nHigh exposure",2,3,()=>{Battle.Sortie(strength,.9f);Contact();});}
    }
    public void Contact() {
        if(Battle.Round>=3||Battle.EnemyLeft<=0){Reserve();return;}
        if(!contactIncident){contactIncident=true;Incident(1,Contact);return;}
        if(!contactsRunning){contactsRunning=true;StartCoroutine(ContactWaves());}
    }
    System.Collections.IEnumerator ContactWaves(){
        while(Battle.Round<3&&Battle.EnemyLeft>0){
            Screen("Contact · wave "+(Battle.Round+1)+" of 3","Ladders strike stone. The enemy meets your companies on the wall. Losses appear over the fighting ranks; the right-hand ledger retains the totals.",3);
            Order("Resolve remaining contact",0,1,()=>{while(Battle.Round<3&&Battle.EnemyLeft>0)Battle.Clash();});
            float t=0;while(t<1.1f){if(CanAnimate)t+=Time.unscaledDeltaTime;yield return null;}
            Battle.Clash();RefreshLedger();crowd.Advance(.8f+Battle.Round*.06f);
        }
        contactsRunning=false;Reserve();
    }
    void Reserve() {
        if(Battle.EnemyLeft<=0||Battle.GuardsNeeded==0){Danger();return;}
        string clue=Battle.SignalFront==0?"The drums gather near the gate, while scattered ladders still threaten the curtain.":"Dense shield formations gather below the curtain, while the gate is loudly threatened.";
        Screen("Two threatened positions",clue+" These signs may conceal a feint. Divide the same reserve between the two fronts. Correct concentration lowers pressure; leaving the main effort thin increases it.",4);
        Order("Gate 75% · Curtain 25%",0,3,()=>{Battle.AssignReserve(75);Danger();});
        Order("Divide evenly · 50 / 50",1,3,()=>{Battle.AssignReserve(50);Danger();});
        Order("Gate 25% · Curtain 75%",2,3,()=>{Battle.AssignReserve(25);Danger();});
    }
    void Danger() {
        if(Battle.Frontline && Battle.GuardsNeeded>0){Battle.Journal.Add("Incident: "+AssaultIncidents.All[23].Title);Screen("The Emperor at the breach","The fighting has reached your position. Your guard can escort you away with a non-fatal wound. Leading a countercharge reduces breach pressure by 8, but costs 15–40 health and can kill you. No timer applies.",4);Order("Accept the escort\nLose 3–10 health; survive",0,active?3:2,()=>{Battle.Emergency(false);Breach();});Order("Lead the countercharge\n15–40 health; possible death",1,active?3:2,()=>{Battle.Emergency(true);Breach();});if(active)Order("Hold the passage personally\nMouse-sword duel; nonfatal exit",2,3,()=>{panel.SetActive(false);AssaultDuelUI.Open(panel.transform.parent,(won,wounds)=>{panel.SetActive(true);Battle.CompleteDuel(won,wounds);Breach();});});}
        else Breach();
    }
    void Breach() {
        if(Battle.Health<=0||Battle.GuardsNeeded==0){FinishBattle(true);return;}
        Screen("Hold the breach","The line has been penetrated. Commit "+Battle.GuardsNeeded+" Varangians to hold it, at the cost of those guards, or fall back to the next defensive line. Surviving attackers remain in the Ottoman army.",4);
        Order("Commit "+Battle.GuardsNeeded+" guards",0,2,()=>FinishBattle(true),Battle.Guards>=Battle.GuardsNeeded);
        Order("Abandon this line",1,2,()=>FinishBattle(false));
    }
    public void FinishBattle(bool guards) {
        if(Battle.Health<=0)guards=false;
        Battle.Resolve(guards);if(!Battle.Finished)return;
        AI.Instance.CommitPhased(Battle);
        Screen("Assault aftermath",Battle.Result+"\nEnemy losses: "+Battle.Dead+". Our losses: "+Battle.OurDead+". Read the ledger or return to the siege report; the next siege day awaits your orders.",5);
        Order("Read phase ledger",0,2,()=>CampaignHUD.Instance.ShowReport("Assault ledger",string.Join("\n\n",Battle.Journal),()=>CampaignHUD.Instance.DismissReport()));
        Order(Battle.Practice?"Return to assault practice":"Continue to siege report",1,2,()=>{Destroy(panel);Current=null;if(Battle.Practice)AssaultPractice.Return();else FindObjectOfType<AssaultUI>(true).AssaultAftermathUI();Destroy(gameObject);});
    }
    void Impact(bool ourLoss) {
        for(int i=0;i<12;i++) {
            var go=new GameObject("Impact smoke",typeof(RectTransform),typeof(RawImage));go.transform.SetParent(arena.transform,false);
            var r=go.GetComponent<RectTransform>();float x=PresentationRandom.Range(.25f,.8f),y=ourLoss?.13f:PresentationRandom.Range(.3f,.6f);
            r.anchorMin=r.anchorMax=new Vector2(x,y);r.sizeDelta=new Vector2(30,30);var image=go.GetComponent<RawImage>();image.texture=AssaultImpact.Glow;image.raycastTarget=false;image.color=ourLoss?new Color(.8f,.32f,.12f,.6f):new Color(.8f,.69f,.4f,.6f);go.AddComponent<AssaultImpact>();
        }
    }
    void Arrows() {
        var rect=arena.GetComponent<RectTransform>();
        for(int i=0;i<16;i++){var go=new GameObject("Arrow",typeof(RectTransform),typeof(Image));go.transform.SetParent(arena.transform,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=Vector2.zero;r.sizeDelta=new Vector2(11,2);var img=go.GetComponent<Image>();img.color=new Color(.95f,.85f,.6f);img.raycastTarget=false;
            Vector2 origin=new Vector2(.07f+i*.006f,.09f),end=aimPoint+new Vector2((i%4-1.5f)*.013f,(i/4-1.5f)*.012f);go.AddComponent<AssaultArrow>().Initialise(Vector2.Scale(origin,rect.rect.size),Vector2.Scale(end,rect.rect.size));}
    }
    void Update() {
        bool focused=Application.isFocused;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(!string.IsNullOrEmpty(VariableSingleton.TestSavePath)&&TestFocus)focused=true;
#endif
        paused=!focused||(CampaignHUD.Instance!=null&&CampaignHUD.Instance.HasModal);
        if(hourglass!=null)hourglass.localRotation=Quaternion.Euler(0,0,waiting&&!paused?Mathf.Sin(elapsed*2)*12:0);
        if(!waiting||paused)return;
        elapsed+=Time.unscaledDeltaTime;
        if(activity!="cannon"&&elapsed>=deadline){waiting=false;var expired=timeout;timeout=null;expired?.Invoke();return;}
        if(activity=="cannon"){
            Vector2 point;var frame=arena.GetComponent<RectTransform>();if(RectTransformUtility.ScreenPointToLocalPointInRectangle(frame,Input.mousePosition,null,out point)&&frame.rect.Contains(point))aimPoint=new Vector2((point.x-frame.rect.xMin)/frame.rect.width,(point.y-frame.rect.yMin)/frame.rect.height);
            aim.anchorMin=aimPoint-new Vector2(.065f,.035f);aim.anchorMax=aimPoint+new Vector2(.065f,.035f);timer.text="Aim · D to fire";if(Input.GetKeyDown(KeyCode.D))CannonShot();return;
        }
        float slice=VolleyDuration/3;int window=Mathf.Min(2,(int)(elapsed/slice));
        float pos=activity=="volley"?(elapsed-window*slice)/slice:elapsed/deadline;
        if(crowd!=null)crowd.Advance(activity=="volley"?pos:.2f+pos*.2f,activity=="volley");
        if(marker!=null){marker.anchorMin=new Vector2(Mathf.Clamp01(pos)*.985f,0);marker.anchorMax=new Vector2(Mathf.Clamp01(pos)*.985f+.015f,1);}
        timer.text=(activity=="volley"?"Volley "+(window+1)+"/3 · ":"")+Mathf.Max(0,deadline-elapsed).ToString("0.0")+"s";
        if(activity=="cover"){
            cue.text=pos<.45f?"FUSE LIT · WAIT":pos<=.8f?"GUN CREW WITHDRAWS · COVER NOW":"GUN FLASH · TAKE COVER";
            if(Input.GetKeyDown(KeyCode.Space)){Battle.Shelter(pos);Volley();return;}
        }
        if(activity=="volley"){
            if(drawn)drawAge+=Time.unscaledDeltaTime;
            // Settle an expired volley before accepting controls for the new rank.
            if(window>Battle.Shots){while(Battle.Shots<window)Battle.Volley(-.25f);Arrows();loaded=drawn=false;RefreshLedger();}
            Vector2 local;var rect=arena.GetComponent<RectTransform>();
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,Input.mousePosition,null,out local)&&rect.rect.Contains(local))aimPoint=new Vector2((local.x-rect.rect.xMin)/rect.rect.width,(local.y-rect.rect.yMin)/rect.rect.height);
            aim.anchorMin=aimPoint-new Vector2(.09f,.045f);aim.anchorMax=aimPoint+new Vector2(.09f,.045f);
            if(Input.GetKeyDown(KeyCode.A))LoadBow();if(Input.GetKeyDown(KeyCode.W))DrawBow();if(Input.GetKeyDown(KeyCode.D))Fire();
            if(firedWindow!=window&&elapsed>=feedbackUntil){
                if(!loaded)cue.text="VOLLEY "+(window+1)+"/3 · A: LOAD";
                else if(!drawn)cue.text="W: DRAW · enemy approaching";
                else cue.text=pos<.3f?"DRAWN · WAIT":pos<=.7f?"EFFECTIVE RANGE · D: LOOSE":"TOO CLOSE · D: RELEASE";
            }
            loadButton.interactable=firedWindow!=window;drawButton.interactable=loaded&&!drawn;fireButton.interactable=drawn&&drawAge>=.16f&&firedWindow!=window;
        }
        if(elapsed>=deadline){waiting=false;var call=timeout;timeout=null;call?.Invoke();}
    }
    void OnDestroy(){if(panel!=null)Destroy(panel);if(Current==this)Current=null;}
}
public class AssaultArrow : MonoBehaviour {
    float age;RectTransform rect;Vector2 start,end;
    public void Initialise(Vector2 from,Vector2 to){rect=GetComponent<RectTransform>();start=from;end=to;rect.anchoredPosition=start;}
    void Update(){if(!PhasedAssaultUI.CanAnimate||rect==null)return;age+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(age/.65f);Vector2 point=Vector2.Lerp(start,end,t)+new Vector2(0,Mathf.Sin(t*Mathf.PI)*45);Vector2 d=point-rect.anchoredPosition;rect.anchoredPosition=point;rect.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);if(t>=1)Destroy(gameObject);}
}

public class AssaultImpact : MonoBehaviour {
    static Texture2D glow;float age;Vector2 velocity;RawImage image;RectTransform rect;Color colour;
    public static Texture2D Glow {get {if(glow!=null)return glow;glow=new Texture2D(32,32,TextureFormat.RGBA32,false);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float alpha=Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/16);glow.SetPixel(x,y,new Color(1,1,1,alpha*alpha));}glow.Apply();return glow;}}
    void Awake(){image=GetComponent<RawImage>();rect=GetComponent<RectTransform>();colour=image.color;velocity=new Vector2(PresentationRandom.Range(-25,25),PresentationRandom.Range(15,55));}
    void Update(){if(!PhasedAssaultUI.CanAnimate)return;age+=Time.unscaledDeltaTime;rect.anchoredPosition+=velocity*Time.unscaledDeltaTime;rect.localScale=Vector3.one*(1+age*2);image.color=new Color(colour.r,colour.g,colour.b,Mathf.Max(0,colour.a*(1-age)));if(age>=1)Destroy(gameObject);}
}
