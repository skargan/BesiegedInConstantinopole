using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public static class PoliticalSystem
{
    public static readonly string[] Names={"Imperial Loyalists","Orthodox Hardliners","Western Unionists","Ottoman Pragmatists"};
    public static readonly Color[] Colours={new Color(.62f,.43f,.17f),new Color(.39f,.28f,.52f),new Color(.19f,.48f,.63f),new Color(.54f,.22f,.19f)};
    public static readonly string[] Beliefs={"Preserve the Emperor's authority and the unity of his government.","Oppose surrender and Church Union; distrust foreign mercenaries and new methods.","Support Church Union, Western alliances and closer ties with the Latin powers.","Seek terms with the Sultan and protect their position under a settlement."};
    public static readonly string[] Orders={"Rally the garrison","Inspire the people","Expedite Western aid","Call upon their creditors"};
    public static readonly string[] Effects={"Army morale +5 points","Public morale +10 points","Western expeditions arrive up to 3 days sooner","Treasury +3,000 gold"};
    public static readonly string[] Passives={"New troop training −10% time (rounded up to whole days)","Daily provisions consumption −5%","Western aid acceptance +5 percentage points","Market purchase totals −5%"};
    public static bool Passive(int bloc)=>!Banned(bloc)&&Share(bloc)>20&&DailyLifeEvents.I("iPolBlocked"+bloc)<=Day;
    public static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    public static int Owed(int bloc)=>DailyLifeEvents.I("iPolOwed"+bloc);
    public static string FavourUnavailable(int bloc)
    {
        if(bloc<0||bloc>3)return "Unknown faction";
        if(Banned(bloc))return "Faction banished";
        if(Share(bloc)<=20)return "Requires more than 20% influence";
        int until=DailyLifeEvents.I("iPolBlocked"+bloc);
        if(until>Day)return "Cooperation withheld for "+(until-Day)+" days";
        int last=DailyLifeEvents.I("iPolLast"+bloc);
        if(last>0&&Day-last<7)return "Available in "+(7-Day+last)+" days";
        if(bloc==2&&!Enumerable.Range(0,CouncilActions.Allies.Length).Any(i=>Western(i)&&CouncilActions.Int("iAidState"+i)==1&&CouncilActions.Int("iAidDays"+i)>1))return "Awaiting a Western expedition";
        return null;
    }
    public static string AdvancePolitics()
    {
        if(DailyLifeEvents.I("iPolDriftTurn")==VariableSingleton.Turn+1)return "";
        VariableSingleton.SetInt("iPolDriftTurn",VariableSingleton.Turn+1);
        PoliticalDemands.Schedule();
        var eligible=Enumerable.Range(0,4).Where(i=>!Banned(i)&&Share(i)<100).ToArray();
        if(eligible.Length==0)return "";
        int bloc=eligible[UnityEngine.Random.Range(0,eligible.Length)],before=Share(bloc);
        Gain(bloc,UnityEngine.Random.Range(1,11));
        return "Political drift: "+Names[bloc]+" gain "+(Share(bloc)-before)+" influence points (now "+Share(bloc)+"%). Influence is redistributed from the other factions.\n\n";
    }
    public static int Share(int i)=>VariableSingleton.GetIntVariable("iBloc"+i);
    public static bool Banned(int i)=>VariableSingleton.GetIntVariable("iBlocBan"+i)==1;
    public static string Allegiance(int character)
    {int a=VariableSingleton.GetIntVariable("iAllegiance"+character);return a==0?"Unaffiliated":Names[Mathf.Clamp(a-1,0,3)];}
    [YarnCommand("SetAllegiance")]
    public static void SetAllegiance(int character,int bloc)
    {
        if(character<0 || character>7 || bloc<0 || bloc>4 || (bloc>0 && Banned(bloc-1))) return;
        VariableSingleton.SetInt("iAllegiance"+character,bloc);
    }
    public static void Gain(int bloc,int amount)
    {
        if(Banned(bloc)) return;
        amount=Mathf.Min(amount,100-Share(bloc));
        for(int n=0;n<amount;n++)
        {
            int donor=-1;
            for(int i=0;i<4;i++) if(i!=bloc && Share(i)>0 && (donor<0 || Share(i)>Share(donor))) donor=i;
            if(donor<0) break;
            VariableSingleton.SetInt("iBloc"+donor,Share(donor)-1);
            VariableSingleton.SetInt("iBloc"+bloc,Share(bloc)+1);
        }
    }
    public static bool Western(int i)=>i==1 || i==2 || i==3 || i==5 || i==6 || i==7 || i==8 || i==9 || i==12 || i==13 || i==14 || i==15;
    public static bool CanAid(int bloc)=>FavourUnavailable(bloc)==null;
    public static bool Aid(int bloc,out string report)
    {
        report=FavourUnavailable(bloc);
        if(bloc<0 || bloc>3 || !CanAid(bloc)) return false;
        var before=VariableSingleton.Instance.CaptureState();
        if(bloc==0) VariableSingleton.ChangePercentFloat("fTroopRep",.05f);
        if(bloc==1) VariableSingleton.ChangePercentFloat("fPeopleRep",.1f);
        if(bloc==2) for(int i=0;i<CouncilActions.Allies.Length;i++) if(Western(i)&&CouncilActions.Int("iAidState"+i)==1) VariableSingleton.SetInt("iAidDays"+i,Mathf.Max(1,CouncilActions.Int("iAidDays"+i)-3));
        if(bloc==3) VariableSingleton.ChangeInt("iMoney",3000);
        Gain(bloc,5);
        int serial=DailyLifeEvents.I("iPolIssued"+bloc)+1;
        VariableSingleton.SetInt("iPolIssued"+bloc,serial);
        VariableSingleton.SetInt("iPolDebtDay"+bloc+"_"+serial,Day);
        VariableSingleton.SetInt("iPolOwed"+bloc,Owed(bloc)+1);
        VariableSingleton.SetInt("iPolLast"+bloc,Day);
        report=Orders[bloc]+".\n"+Effects[bloc]+"\n\n"+EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState())+"\n"+Names[bloc]+" influence: "+Share(bloc)+"%.\nFavours owed: "+Owed(bloc)+". This favour cannot be called in before "+VariableSingleton.GetDate().AddDays(30).ToString("d MMM yyyy")+".";
        return true;
    }
    public static string BanishPreview(int bloc)
    {
        string people=string.Join(", ",Enumerable.Range(0,8).Where(i=>CouncilActions.Int("iAllegiance"+i)==bloc+1 && VariableSingleton.GetBoolVariable(CourtRoster.Keys[i])).Select(i=>CourtRoster.Names[i]));
        return "Banish "+Names[bloc]+"?\n\nTheir influence passes to the Loyalists. All five civic relations fall by 15 points.\nCharacters expelled: "+(people.Length==0?"None currently in service":people)+".\n\nThis uses the major action and your single faction banishment for this campaign.";
    }
    public static bool Banish(int bloc,out string report)
    {
        report="Banishment is unavailable.";
        if(bloc<1 || bloc>3 || Banned(bloc)||CouncilActions.Int("iBanishmentUsed")!=0)return false;
        int power=Share(bloc);VariableSingleton.SetInt("iBloc"+bloc,0);VariableSingleton.SetInt("iBloc0",Share(0)+power);
        if(PoliticalDemands.Active==bloc)VariableSingleton.SetInt("iPolDemand",0);
        VariableSingleton.SetInt("iBlocBan"+bloc,1);VariableSingleton.SetInt("iBanishmentUsed",1);
        foreach(string key in CouncilActions.FactionKeys) VariableSingleton.ChangePercentFloat(key,-.15f);
        for(int i=0;i<8;i++) if(CouncilActions.Int("iAllegiance"+i)==bloc+1)
        {VariableSingleton.ChangeBool(CourtRoster.Keys[i],false);VariableSingleton.SetInt("iCourtState"+i,6);if(CouncilActions.Int("iCommander")==i)VariableSingleton.SetInt("iCommander",-1);}
        report=Names[bloc]+" have been banished.\nCivic relations -15 points each.\nLoyalist influence: "+Share(0)+"%.";
        return true;
    }
    static Sprite circle;
    public static void DrawPie(Transform parent)
    {
        if(circle==null){var t=new Texture2D(256,256);for(int y=0;y<256;y++)for(int x=0;x<256;x++)t.SetPixel(x,y,new Vector2(x-127.5f,y-127.5f).sqrMagnitude<127*127?Color.white:Color.clear);t.Apply();circle=Sprite.Create(t,new Rect(0,0,256,256),new Vector2(.5f,.5f));}
        float angle=0;
        for(int i=0;i<4;i++){var obj=CampaignHUD.Panel(parent,"Influence slice",Vector2.zero,Vector2.one);var im=obj.GetComponent<Image>();im.sprite=circle;im.type=Image.Type.Filled;im.fillMethod=Image.FillMethod.Radial360;im.fillOrigin=2;im.fillClockwise=true;im.fillAmount=Share(i)/100f;im.color=Colours[i];obj.transform.localEulerAngles=new Vector3(0,0,-angle);angle+=Share(i)*3.6f;im.raycastTarget=false;}
    }
}
