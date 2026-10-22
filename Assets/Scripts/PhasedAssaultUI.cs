using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhasedAssaultUI : MonoBehaviour
{
    public static bool Enabled=true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool TestFocus;
#endif
    public static bool ActiveMode { get=>PlayerPrefs.GetInt("Besieged.ActiveAssault",0)==1; set {PlayerPrefs.SetInt("Besieged.ActiveAssault",value?1:0);PlayerPrefs.Save();} }
    public static PhasedAssaultUI Current;
    public PhasedAssault Battle;
    GameObject panel,choices,arena; TMP_Text title,description,ledger,timer,phaseText;RectTransform marker;
    float elapsed,deadline; RectTransform formation,hourglass; int lastEnemyDead,lastOurDead;Action timeout; bool active,paused,waiting;string activity="";int sortieStrength;
    readonly List<GameObject> particles=new List<GameObject>();
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
        var troopArt=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/assault-formation");
        if(troopArt!=null){formation=Picture(arena.transform,"Enemy formation",troopArt,new Vector2(.38f,.3f),new Vector2(.62f,.7f));}
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
        waiting=false;timeout=null;activity="";timer.text="";
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
        lastEnemyDead=Battle.Dead;lastOurDead=Battle.OurDead;
        ledger.text="<b>THIS ASSAULT</b>\nCommitted attackers: "+Battle.Attackers.ToString("N0")+"\nEnemy survivors: "+Battle.EnemyLeft.ToString("N0")+"\nEnemy dead: "+Battle.Dead.ToString("N0")+"\nOur dead: "+Battle.OurDead+"\n\nArchers ×"+Battle.VolleyMultiplier.ToString("0.00")+"\nReserve guards: "+Battle.Guards+"\nBreach pressure: "+(Battle.Finished?0:Battle.GuardsNeeded)+"\nEmperor health: "+Battle.Health+"%\n\n"+(Battle.Journal.Count>0?Battle.Journal[Battle.Journal.Count-1]:"No casualties yet. Orders below affect only this assault.");
    }
    public void Briefing() {
        Screen("The Ottoman assault","Choose where you command. Frontline leadership adds 8% to melee output but increases exposure and can bring a personal emergency. Safe command has no personal combat risk. Timed play is optional."+(Battle.Frontline?" Your palace order to join this assault is recorded; confirm it below or choose shelter.":""),0);
        Order(active?"Mode: Active timing\nChange to assisted":"Mode: Assisted\nChange to active timing",0,3,()=>{active=!active;ActiveMode=active;Briefing();});
        Order("Command from shelter",1,3,()=>{Battle.Frontline=false;Cover();});
        Order(Battle.Frontline?"Confirm palace order\nJoin the parapet":"Join the parapet\nPersonal risk",2,3,()=>{Battle.Frontline=true;Cover();});
    }
    void BeginTimer(string kind,float seconds,Action onTimeout) {
        activity=kind;elapsed=0;deadline=seconds;timeout=onTimeout;waiting=true;Battle.TimedMoments++;
        var track=CampaignHUD.Panel(arena.transform,"Timing track",new Vector2(.08f,.10f),new Vector2(.92f,.21f));track.GetComponent<Image>().color=ImperialTheme.Navy;
        var zone=CampaignHUD.Panel(track.transform,"Effective range",new Vector2(.30f,0),new Vector2(.70f,1));zone.GetComponent<Image>().color=new Color(.22f,.40f,.36f);
        var centre=CampaignHUD.Panel(track.transform,"Ideal range",new Vector2(.45f,0),new Vector2(.55f,1));centre.GetComponent<Image>().color=new Color(.8f,.64f,.3f);
        marker=CampaignHUD.Panel(track.transform,"Enemy advance",new Vector2(0,0),new Vector2(.015f,1)).GetComponent<RectTransform>();marker.GetComponent<Image>().color=Color.white;
    }
    void Cover() {
        if(CouncilActions.Int("iTurkCannons")==0){Volley();return;}
        Screen("Gun crews at the parapet","Ottoman crews run back from their guns. Pull the archers into cover near the gold band. Early cover costs 10% archer output; late cover grants +10% firing output but can kill 1.5% of your archers. Deadline: take cover automatically.",1);
        if(active) {Order("Take cover",0,1,()=>{Battle.Shelter(Mathf.Clamp01(elapsed/30));Volley();});BeginTimer("cover",30,()=>{Battle.Shelter(.2f);Volley();});}
        else {Order("Let captains manage cover\nOrdinary output; no casualties",0,2,()=>{Battle.Shelter(.65f);Volley();});Order("Keep firing\n+10% output; lose 1.5% archers",1,2,()=>{Battle.Shelter(1);Volley();});}
    }
    public void Volley() {
        Screen("Loose on my command","Three volleys. Fire as the approaching marker reaches the gold centre. The average timing changes archer lethality by at most ±25%; other troops are unaffected. Missed volleys score −25%.",1);
        if(Battle.Archers==0){AfterVolley();return;}
        if(active){Order("Loose arrows!",0,1,Fire);BeginTimer("volley",30,()=>{while(Battle.Shots<3)Battle.Volley(-.25f);AfterVolley();});}
        else Order("Let the captains judge the range\nOrdinary lethality ×1.00",0,1,()=>{while(Battle.Shots<3)Battle.Volley(0);Arrows();AfterVolley();});
    }
    void Fire() {
        if(Battle.Shots>=3||!waiting)return;
        float progress=(elapsed-Battle.Shots*10)/10f;Battle.Volley(PhasedAssault.VolleyScore(Mathf.Clamp01(progress)));Arrows();
        elapsed=Battle.Shots*10;RefreshLedger();if(Battle.Shots>=3)AfterVolley();
    }
    void AfterVolley() {
        waiting=false;Battle.Approach(VariableSingleton.GetFloatVariable("fMoat"),TechnologySystem.Has("GreekFire")?Battle.Attackers*.05f:0);
        if(Battle.EnemyLeft<=0){FinishBattle(true);return;}
        if(Battle.Tower||Battle.Ram)SortieMenu();else Contact();
    }
    void SortieMenu() {
        Screen("A target outside the gate","A siege engine has advanced beyond its escort. A brief sortie risks fewer men; a heavy sortie has a better chance of destroying it. Casualties rise sharply with time outside. Success reduces breach pressure by 12.",2);
        Order("Keep the gates shut",0,3,()=>{Battle.Sortie(0,0);Contact();});
        Order("Send up to 100 raiders",1,3,()=>StartSortie(1),Battle.Infantry>0);
        Order("Send up to 300 men",2,3,()=>StartSortie(2),Battle.Infantry>0);
    }
    void StartSortie(int strength) {
        sortieStrength=strength;Screen("Recall the sortie","Enemy reinforcements gather beyond the smoke. A longer stay improves the chance of burning the engine but increases losses. If time expires, the party withdraws.",2);
        if(active&&Battle.TimedMoments<2){Order("Recall now",0,1,()=>{Battle.Sortie(sortieStrength,Mathf.Clamp01(elapsed/30));Contact();});BeginTimer("sortie",30,()=>{Battle.Sortie(sortieStrength,1);Contact();});}
        else{Order("Quick strike\nLow exposure",0,3,()=>{Battle.Sortie(strength,.2f);Contact();});Order("Press the attack\nModerate exposure",1,3,()=>{Battle.Sortie(strength,.55f);Contact();});Order("Stay until it burns\nHigh exposure",2,3,()=>{Battle.Sortie(strength,.9f);Contact();});}
    }
    public void Contact() {
        if(Battle.Round>=3||Battle.EnemyLeft<=0){Reserve();return;}
        Screen("Contact · wave "+(Battle.Round+1)+" of 3",Battle.Round==0?"Ladders strike stone. Your infantry engages the survivors of the approach. The captains commit fresh ranks as the enemy reaches the ladders.":Battle.Round==1?"The first rank pulls back. Fresh men cross the bodies beneath the wall; your depleted companies must meet them.":"The last committed wave advances. The outcome here decides whether the enemy reaches a breach.",3);
        Order("Resolve this wave",0,1,()=>{Battle.Clash();Contact();});
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
        if(Battle.Frontline && Battle.GuardsNeeded>0){Screen("The Emperor at the breach","The fighting has reached your position. Your guard can escort you away with a non-fatal wound. Leading a countercharge reduces breach pressure by 8, but costs 15–40 health and can kill you. No timer applies.",4);Order("Accept the escort\nLose 3–10 health; survive",0,2,()=>{Battle.Emergency(false);Breach();});Order("Lead the countercharge\n15–40 health; possible death",1,2,()=>{Battle.Emergency(true);Breach();});}
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
        Order("Continue to siege report",1,2,()=>{Destroy(panel);Current=null;FindObjectOfType<AssaultUI>(true).AssaultAftermathUI();Destroy(gameObject);});
    }
    void Impact(bool ourLoss) {
        for(int i=0;i<12;i++) {
            var go=new GameObject("Impact smoke",typeof(RectTransform),typeof(RawImage));go.transform.SetParent(arena.transform,false);
            var r=go.GetComponent<RectTransform>();float x=UnityEngine.Random.Range(.25f,.8f),y=ourLoss?.13f:UnityEngine.Random.Range(.3f,.6f);
            r.anchorMin=r.anchorMax=new Vector2(x,y);r.sizeDelta=new Vector2(30,30);var image=go.GetComponent<RawImage>();image.texture=AssaultImpact.Glow;image.raycastTarget=false;image.color=ourLoss?new Color(.8f,.32f,.12f,.6f):new Color(.8f,.69f,.4f,.6f);go.AddComponent<AssaultImpact>();
        }
    }
    void Arrows() {
        for(int i=0;i<16;i++) {var arrow=CampaignHUD.Panel(arena.transform,"Arrow trail",new Vector2(.1f+i*.035f,.12f),new Vector2(.13f+i*.035f,.125f));arrow.GetComponent<Image>().color=new Color(.9f,.81f,.55f);arrow.GetComponent<Image>().raycastTarget=false;arrow.AddComponent<AssaultArrow>();}
    }
    void Update() {
        if(formation!=null){float advance=activity=="volley"?Mathf.Clamp01((elapsed-Battle.Shots*10)/10f):Battle.Round/3f;formation.anchoredPosition=new Vector2(0,-advance*45);formation.localScale=Vector3.one*(.6f+advance*.4f);formation.gameObject.SetActive(Battle.EnemyLeft>0);}
        bool focused=Application.isFocused;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(!string.IsNullOrEmpty(VariableSingleton.TestSavePath)&&TestFocus)focused=true;
#endif
        paused=!focused||(CampaignHUD.Instance!=null&&CampaignHUD.Instance.HasModal);
        if(hourglass!=null)hourglass.localRotation=Quaternion.Euler(0,0,waiting&&!paused?Mathf.Sin(elapsed*.7f)*12:0);
        if(!waiting||paused)return;
        elapsed+=Time.unscaledDeltaTime;
        float pos=activity=="volley"?(elapsed-Battle.Shots*10)/10f:elapsed/deadline;
        if(marker!=null){marker.anchorMin=new Vector2(Mathf.Clamp01(pos)*.985f,0);marker.anchorMax=new Vector2(Mathf.Clamp01(pos)*.985f+.015f,1);}
        timer.text="Time: "+Mathf.CeilToInt(Mathf.Max(0,deadline-elapsed))+"s";
        if(activity=="volley"&&elapsed>=(Battle.Shots+1)*10&&Battle.Shots<3){Battle.Volley(-.25f);RefreshLedger();}
        if(elapsed>=deadline){waiting=false;var call=timeout;timeout=null;call?.Invoke();}
    }
    void OnDestroy(){if(panel!=null)Destroy(panel);if(Current==this)Current=null;}
}
public class AssaultArrow : MonoBehaviour {
    float age;RectTransform rect;
    void Awake(){rect=GetComponent<RectTransform>();}
    void Update(){if(!Application.isFocused||(CampaignHUD.Instance!=null&&CampaignHUD.Instance.HasModal))return;age+=Time.unscaledDeltaTime;rect.anchoredPosition+=new Vector2(240,180)*Time.unscaledDeltaTime;rect.localRotation=Quaternion.Euler(0,0,35-age*35);if(age>1.2f)Destroy(gameObject);}
}

public class AssaultImpact : MonoBehaviour {
    static Texture2D glow;float age;Vector2 velocity;RawImage image;RectTransform rect;Color colour;
    public static Texture2D Glow {get {if(glow!=null)return glow;glow=new Texture2D(32,32,TextureFormat.RGBA32,false);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float alpha=Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/16);glow.SetPixel(x,y,new Color(1,1,1,alpha*alpha));}glow.Apply();return glow;}}
    void Awake(){image=GetComponent<RawImage>();rect=GetComponent<RectTransform>();colour=image.color;velocity=new Vector2(UnityEngine.Random.Range(-25,25),UnityEngine.Random.Range(15,55));}
    void Update(){if(!Application.isFocused||(CampaignHUD.Instance!=null&&CampaignHUD.Instance.HasModal))return;age+=Time.unscaledDeltaTime;rect.anchoredPosition+=velocity*Time.unscaledDeltaTime;rect.localScale=Vector3.one*(1+age*2);image.color=new Color(colour.r,colour.g,colour.b,Mathf.Max(0,colour.a*(1-age)));if(age>=1)Destroy(gameObject);}
}
