using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Reuses the existing map symbols, court flags, fonts and gold/mosaic controls.
public sealed class ProvincialConflictUI : MonoBehaviour
{
    static GameObject root;static string selected;static int armySlot;static Action continuation;
    static ProvincialWar.Province P=>ProvincialWar.Find(selected);
    static Transform overviewSurface;
    static Transform Surface=>overviewSurface!=null?overviewSurface:root.transform;
    public static bool Open=>root!=null;
    public static bool Back(){if(!Open)return false;if(!P.Pending)Exit();return true;}
    public static void Close(){ProvinceHistoryUI.Close();overviewSurface=null;if(root!=null){root.SetActive(false);Destroy(root);root=null;}}
    public static bool Intercept(Action next)
    {
        var pending=ProvincialWar.Ready?ProvincialWar.Pending:null;if(pending==null)return false;
        continuation=next;Show(pending.Id);return true;
    }
    public static void Show(string id,int slot=0)
    {
        ProvinceCampaign.Ensure();selected=id;armySlot=slot;WarTheatreUI.Close();Draw();
    }
    static TMP_Text Text(string s,float x,float y,float w,float h,int font=22)
    {
        var t=CampaignHUD.Label(Surface,s,new Vector2(x,y),new Vector2(x+w,y+h),font);
        if(overviewSurface!=null){t.enableAutoSizing=true;t.fontSizeMax=Math.Min(font,22);t.fontSizeMin=16;}
        return t;
    }
    static void Button(string s,float x,float y,float w,float h,Action action,bool enabled=true)
    {
        var b=CampaignHUD.MakeButton(Surface,s,new Vector2(x,y),new Vector2(x+w,y+h),()=>action());b.interactable=enabled;
    }
    static void Commit(Action action){action();SaveGames.Checkpoint(SaveGames.Phase,null,"Provincial command",ActionResults.Instance==null?"":ActionResults.Instance.GetActionText());Draw();}
    static void Exit()
    {
        Close();if(continuation!=null){var next=continuation;continuation=null;if(!Intercept(next))next();}else WarTheatreUI.Show();
    }
    static void Draw()
    {
        Close();root=new GameObject("Provincial command",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=245;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        root.GetComponent<Image>().color=ImperialTheme.Navy;ImperialTheme.Panel(root);
        if(P.Get("BattleStage")>0){Battle();return;}
        Text(P.Name+" · War Beyond the Walls",.035f,.91f,.76f,.07f,32);
        if(P.Get("Settlement")>0){Settlement();return;}
        if(P.Get("Continuation")>0){Continuation();return;}
        if(P.Get("Assistance")>0){Assistance();return;}
        Overview();
        var history=CampaignHUD.MakeButton(root.transform,"History",new Vector2(.835f,.915f),new Vector2(.975f,.98f),()=>ProvinceHistoryUI.Show(selected));history.name="Province history button";
    }
    static void Overview()
    {
        var p=P;var c=p.Config;
        SceneBackdrop.Apply(root,"Provinces/"+p.Id);
        var panel=new GameObject("Province inspection",typeof(RectTransform),typeof(Image));
        panel.transform.SetParent(root.transform,false);
        var rect=panel.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.33f,.02f);rect.anchorMax=new Vector2(.985f,.895f);rect.offsetMin=rect.offsetMax=Vector2.zero;
        ImperialTheme.Panel(panel,false);panel.GetComponent<Image>().color=new Color(.018f,.043f,.063f,.94f);
        var heading=CampaignHUD.Panel(root.transform,"Province heading",new Vector2(.02f,.905f),new Vector2(.985f,.99f));heading.GetComponent<Image>().color=new Color(.018f,.043f,.063f,.9f);
        heading.transform.SetSiblingIndex(1);
        overviewSurface=panel.transform;
        Text(ProvinceCampaign.Status(p.Id),.035f,.85f,.93f,.05f,21);
        Text("Authority  "+p.Authority+" / "+p.Ceiling+"\nResistance  "+p.Resistance+"\nWealth  "+p.Wealth+"%",.04f,.66f,.29f,.17f,25);
        Text("Ottoman garrison\n"+(p.Known?p.Garrison.ToString("N0")+" / "+c.Garrison.ToString("N0"):"Exact strength unknown")+"\nNon-Ottoman territory  "+(p.TotalControl/100f).ToString("0.##")+"%",.35f,.66f,.29f,.17f,24);
        string economy="Economic importance: "+string.Join(", ",new[]{c.FoodDays>0?"food":null,c.Recruits>0?"recruits":null,c.Ships>0?"shipbuilding":null,c.Cannons>0?"artillery":null,c.Gold>0?"revenue":null}.Where(x=>x!=null));
        Text(economy+"\n"+(p.Known||!p.Hostile?
            "Food reserve: "+SharedProvisions.Format(p.FoodReserve)+" provisions\n"+
            "Fortnightly capacity: "+SharedProvisions.Format(c.FortnightlyProvisions*p.Wealth/100d)+" provisions\n"+
            "Base output: "+c.Recruits+" troops · "+c.Gold+" gold · "+c.Cannons+" cannons · "+c.Ships+" ships":
            "Send a resident agent for exact enemy strengths and local supply figures."),.66f,.64f,.30f,.19f,18);
        Text("Provincial armies",.04f,.59f,.92f,.05f,25);
        var forces=p.Present.ToArray();if(forces.Length==0)Text("No rebel or allied army is present.",.04f,.49f,.92f,.07f);
        else {
            int current=Array.FindIndex(forces,a=>a.Slot==armySlot);if(current<0)current=0;armySlot=forces[current].Slot;var a=forces[current];
            Button("‹",.04f,.49f,.045f,.07f,()=>{armySlot=forces[(current+forces.Length-1)%forces.Length].Slot;Draw();});
            Button("›",.91f,.49f,.045f,.07f,()=>{armySlot=forces[(current+1)%forces.Length].Slot;Draw();});
            Text(a.Name+"  ·  "+a.Troops.ToString("N0")+" troops  ·  "+(a.Control/100f).ToString("0.##")+"% territory  ·  "+(current+1)+" / "+forces.Length,.11f,.49f,.77f,.07f,23);
            Text("Order: "+a.Stance+(a.Protected?" · Hidden Muster: "+Mathf.Max(0,p.Get("MusterUntil")-ProvincialWar.I("LastDay"))+" days remaining":"")+(a.Rebel?" · Recruit from Resistance":" · Committed replacements: "+ProvincialWar.I("Reserve"+a.Country)),.04f,.43f,.92f,.05f,20);
            string[] titles={"Gather Strength","Seek Battle","Capture Territory","Skirmish","Pillage","Hold position"};
            for(int i=0;i<6;i++){var order=(ProvincialWar.Order)i;string reason=ProvincialWar.OrderReason(a,order);Button(titles[i],.04f+(i%3)*.31f,.33f-(i/3)*.09f,.29f,.075f,()=>Commit(()=>ProvincialWar.GiveOrder(a,order,out _)),reason==null);}
            Text(a.Troops<ProvincialWar.Rules.MinimumArmy?"Below 1,000 troops: gather strength or defend; offensive orders are unavailable.":a.Protected?"Any offensive order ends Hidden Muster immediately.":"Orders persist. Territory claims strengthen an ally's claim at settlement.",.04f,.18f,.92f,.05f,18);
        }
        Button("Launch rebellion · 1,000 troops",.04f,.09f,.32f,.07f,()=>Commit(()=>ProvincialWar.Launch(p.Id,out _)),ProvincialWar.LaunchReason(p)==null);
        Button("Agent and network",.38f,.09f,.29f,.07f,()=>{Close();FindObjectOfType<CouncilUI>(true).ProvincialOperationsFor(p.Id);});
        Button("Return to map",.69f,.09f,.27f,.07f,Exit);
        Text(ProvincialWar.LaunchReason(p)??"Requires 30 Resistance. The founding army and Hidden Muster are available once per province.",.04f,.025f,.92f,.045f,17);
    }
    static void Continuation()
    {
        var p=P;string power=CountryDiplomacy.Definitions[p.Owner-ProvincialWar.AllyOwnerOffset].Name;
        Text("Our ally's next campaign",.04f,.80f,.92f,.08f,32);
        Text(power+" now holds "+p.Name+". Its captains send their sovereign's instructions: they may consolidate their gains, make a separate peace, or continue against a neighbouring Ottoman province.\n\nTheir decision has already been made; it will not change if this letter is opened again.",.07f,.40f,.86f,.32f,26);
        Button("Read their reply",.30f,.19f,.40f,.10f,()=>{string report="";Commit(()=>report=ProvincialWar.ContinueAlly(p));Close();CampaignHUD.Instance.ShowReport("Our ally's intentions",report,()=>{CampaignHUD.Instance.DismissReport();Exit();});});
    }
    static void Assistance()
    {
        var p=P;var r=ProvincialWar.Rules;Text("The price of restoration",.04f,.80f,.92f,.08f,32);
        Text("The magistrates of "+p.Name+" send accounts stained by rain and hurried hands. Their workshops stand idle; the men who accepted the imperial seal now ask whether Constantinople can help them make it mean something.\n\nSend "+r.AssistanceCost+" gold to restore "+r.AssistanceWealth+" Wealth, or refuse. Refusal costs "+r.AssistanceWealth+" Wealth and carries a "+r.AssistanceChaosChance+"% risk of administrative collapse.",.07f,.39f,.86f,.35f,25);
        for(int i=0;i<2;i++){bool pay=i==0;Button(pay?"Send assistance":"They must find their own means",.06f+i*.47f,.19f,.43f,.11f,()=>{string report="";Commit(()=>report=ProvincialWar.Assistance(p,pay));Close();CampaignHUD.Instance.ShowReport("The provincial magistrates",report,()=>{CampaignHUD.Instance.DismissReport();Exit();});},!pay||CouncilActions.Int("iMoney")>=r.AssistanceCost);}
    }
    static void Battle()
    {
        var p=P;int stage=p.Get("BattleStage");
        SceneBackdrop.Apply(root,"ProvincialBattles/"+ProvincialBattleArt.Key(p));
        var panel=new GameObject("Battle dispatch",typeof(RectTransform),typeof(Image));
        panel.transform.SetParent(Surface,false);
        var rect=panel.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.555f,.025f);rect.anchorMax=new Vector2(.985f,.975f);rect.offsetMin=rect.offsetMax=Vector2.zero;
        panel.GetComponent<Image>().color=new Color(.018f,.043f,.063f,.95f);
        Text(p.Name,.575f,.905f,.39f,.045f,29);
        string title=stage<=2?"I · Manoeuvre":stage<=4?"II · Clash":stage==5?"III · Aftermath":"Battle concluded";
        Text(title,.575f,.845f,.39f,.045f,24);
        bool choices=stage==1||stage==3||stage==5;
        var report=Text(ProvincialBattle.Report(p),.575f,choices?.365f:.245f,.39f,choices?.455f:.575f,23);
        report.name="Battle report";report.enableAutoSizing=true;report.fontSizeMin=18;report.fontSizeMax=23;
        report.alignment=TextAlignmentOptions.TopLeft;
        if(stage==1){for(int i=0;i<3;i++){int choice=i;string[] descriptions={"Caution · Protect the withdrawal route","Speed · Divide the enemy's strength","Coordination · Bring allies into battle"};Button(descriptions[i],.575f,.275f-i*.085f,.39f,.07f,()=>Commit(()=>ProvincialBattle.Maneuver(p,choice)),i!=2||ProvincialBattle.CanCoordinate(p));}}
        else if(stage==3){for(int i=0;i<3;i++){int choice=i;Button(ProvincialBattle.Tactics[i],.575f,.275f-i*.085f,.39f,.07f,()=>Commit(()=>ProvincialBattle.Clash(p,choice)));}}
        else if(stage==5){string[] options=p.Get("BResult")>0?new[]{"Pursue","Consolidate"}:p.Get("BResult")<0?new[]{"Withdraw","Cover the Retreat","Fight On"}:new[]{"Disengage","Renew the Attack"};for(int i=0;i<options.Length;i++){int choice=i;Button(options[i],.575f,.275f-i*.085f,.39f,.07f,()=>Commit(()=>ProvincialBattle.Aftermath(p,choice)));}}
        else Button(stage==6?"Acknowledge result":"Continue",.575f,.12f,.39f,.08f,()=>{Commit(()=>ProvincialBattle.Continue(p));if(!p.Pending)Exit();});
        Text("War Beyond the Walls · The encounter resolves today",.575f,.045f,.39f,.035f,16);
    }
    static void Settlement()
    {
        var p=P;var r=ProvincialWar.Rules;var ally=ProvincialWar.QualifyingAlly(p);int restore=p.Resistance>=r.StrongResistance?r.StrongRestoreChance:r.RestoreChance;
        Text("Who Shall Govern?",.04f,.81f,.92f,.08f,32);
        Text("The officials can no longer enforce their writ. Local leaders, armed companies and the men who fought here now ask whose seal shall stand above the gates.\n\nChoose a settlement for "+p.Name+". Failure leaves the province in chaos for "+r.ChaosDays+" days; no exports leave during that time.",.06f,.55f,.88f,.24f,25);
        string[] titles={"Proclaim Imperial Restoration · "+r.RestoreCost+" gold · "+restore+"% success",ally==null?"Entrust to our ally · no qualifying claimant":"Entrust to "+ally.Name+" · "+r.EndorseChance+"% success","Leave settlement to local leaders · "+r.LocalChance+"% imperial allegiance"};
        for(int i=0;i<3;i++){int choice=i;Button(titles[i],.10f,.41f-i*.12f,.80f,.10f,()=>{string report="";bool ok=false;Commit(()=>ok=ProvincialWar.Settle(p,choice,out report));if(ok){Close();CampaignHUD.Instance.ShowReport("The provincial settlement",report,()=>{CampaignHUD.Instance.DismissReport();Exit();});}},i==0?CouncilActions.Int("iMoney")>=r.RestoreCost:i!=1||ally!=null);}
        Text("An ally must hold more than 10% and lead every other ally. Ties do not select a ruler automatically. Gold is paid even if restoration fails.",.10f,.055f,.80f,.08f,18);
    }
}

// Gate only management time: historical petitions and assaults retain their existing sequence.
public sealed class ProvincialConflictGate : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Attach(){var go=new GameObject("Provincial pending-event gate");go.AddComponent<ProvincialConflictGate>();}
    void LateUpdate()
    {
        if(!VariableSingleton.IsInitialized||!ProvincialWar.Ready||ProvincialConflictUI.Open||ProvincialWar.Pending==null)return;
        var manager=FindObjectOfType<ManagerUI>();if(manager==null||!manager.isActiveAndEnabled)return;
        if(CampaignHUD.Instance==null||CampaignHUD.Instance.HasModal||CampaignHUD.Instance.HasEnded||GameObject.Find("War Beyond the Walls map")!=null)return;
        ProvincialConflictUI.Show(ProvincialWar.Pending.Id);
    }
}
