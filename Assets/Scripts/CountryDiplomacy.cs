using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// One append-only country index is shared by aid, resident envoys and regional intervention.
// All mutable fields use the campaign's saved integer store; goodwill is in hundredths of a point.
public static class CountryDiplomacy
{
    public enum AttitudeLevel { Hostile, Wary, Neutral, Friendly, Allied }
    public enum EnvoyAssignment { None, Outbound, Resident, Returning }
    public enum WarCommitment { Untried, Refused, Mobilising, Intervening }
    [Serializable] public sealed class Definition
    {
        public string Id, Name, Description, InterventionType, StoryPrerequisite, BalanceNote;
        public int StartingGoodwill, StartingAttitude, EnvoyDays, UnionFull, UnionSymbolic, UnionRejected;
        public bool Catholic, AidBySea, WarEligible;
        public int AidDays, Infantry, Archers, Cavalry, LegacyTransportShips, Gold, Provisions;
        public int MobilisationDays, DelayDays, ShipmentBudget;
        public string[] TargetProvinces;
    }
    [Serializable] sealed class Catalogue { public int Version; public Definition[] Powers; }
    static Definition[] definitions;
    public static Definition[] Definitions {
        get {
            if(definitions==null) {
                var asset=Resources.Load<TextAsset>("Content/DiplomaticPowers");
                if(asset==null)throw new InvalidOperationException("Missing Content/DiplomaticPowers.json");
                var data=JsonUtility.FromJson<Catalogue>(asset.text);
                if(data==null||data.Version!=1||data.Powers==null||data.Powers.Length<31)throw new InvalidOperationException("Invalid diplomatic powers catalogue");
                definitions=data.Powers;
                string[] stable={"hungary","venice","genoa","papacy","albania","hospitallers","naples","holy_roman_empire","france","athens","georgia","trebizond","cyprus","poland_lithuania","teutonic_order","england","karaman","aq_qoyunlu","wallachia","moldavia","serbia","bosnia","crimea","mamluks","morea","ragusa","ionian_tocco","florence","milan","siena","theodoro"};
                for(int i=0;i<stable.Length;i++)if(definitions[i].Id!=stable[i]||definitions[i].EnvoyDays<1)throw new InvalidOperationException("Diplomatic save index changed: "+i);
            }
            return definitions;
        }
    }
    const string Prefix="iDiplomacy";
    static int I(string key,int fallback=0)=>DailyLifeEvents.I(Prefix+key,fallback);
    static void Put(string key,int value)=>VariableSingleton.SetInt(Prefix+key,value);
    static string K(string field,int i)=>field+"_"+i;
    public static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    static bool Valid(int i)=>i>=0&&i<Definitions.Length;
    static DateTime Date(int day)=>new DateTime((long)Math.Max(0,day)*TimeSpan.TicksPerDay);
    static int Policy=>DailyLifeEvents.I("iReligion")==1?1:DailyLifeEvents.I("iReligion")==2?2:0;
    static int ReadAttitude(int i)=>Mathf.Clamp(I(K("Attitude",i)),0,4);
    static int ReadEnvoy(int i)=>I(K("Envoy",i));
    static int ReadWar(int i)=>I(K("War",i));

    public static int Index(string id)
    {
        if(string.IsNullOrEmpty(id))return -1;
        switch(id.ToLowerInvariant()) {
            case "poland":case "lithuania":case "grand_duchy_lithuania":id="poland_lithuania";break;
            case "aragon":case "aragon_crown":case "crown_of_aragon":case "sardinia":case "sicily":id="naples";break;
            case "dubrovnik":id="ragusa";break;
            case "ionian":case "ionian_lordships":case "cephalonia":case "zakynthos":case "leucas":id="ionian_tocco";break;
            case "duchy_of_milan":id="milan";break;
            case "mangup":case "gothia":id="theodoro";break;
            case "moldova":id="moldavia";break;
            case "crimean_khanate":id="crimea";break;
            case "mamluk_sultanate":case "mamluk":id="mamluks";break;
            case "turkmen_confederations":id="aq_qoyunlu";break;
            case "gazaria":case "genoa_crimea":case "genoese_crimea":id="genoa";break;
        }
        return Array.FindIndex(Definitions,d=>string.Equals(d.Id,id,StringComparison.OrdinalIgnoreCase));
    }

    public static CouncilActions.Ally[] CreateAllies()=>Definitions.Select(d=>new CouncilActions.Ally(d.Name,d.Description,d.StartingGoodwill,d.AidDays,d.Infantry,d.Archers,d.Cavalry,d.LegacyTransportShips,d.Gold,d.Provisions)).ToArray();

    // Call before loading dictionaries. This also clears arbitrary once-only event tokens.
    public static void ResetForRestore()
    {
        if(VariableSingleton.Instance!=null)
            foreach(var value in VariableSingleton.Instance.CaptureState().ints)
                if(value.key.StartsWith(Prefix,StringComparison.Ordinal))VariableSingleton.SetInt(value.key,0);
        Put("Ready",0);VariableSingleton.SetInt("iEnvoys",0);
    }
    public static void BeginCampaign(){ResetForRestore();Ensure();}
    public static void AfterRestore(){Ensure();}
    public static void Ensure()
    {
        if(I("Ready")==0) {
            Put("Ready",1);Put("LastDay",Day);Put("Policy",Policy);
            // Legacy campaigns receive their initial three envoys without changing previous aid.
            VariableSingleton.SetInt("iEnvoys",Math.Max(3,DailyLifeEvents.I("iEnvoys")));
        }
        for(int i=0;i<Definitions.Length;i++)if(I(K("Ready",i))==0) {
            var d=Definitions[i];Put(K("Ready",i),1);Put(K("Goodwill",i),Mathf.Clamp(d.StartingGoodwill,0,95)*100);
            Put(K("Attitude",i),i==3?(Policy==1?3:Policy==2?2:d.StartingAttitude):d.StartingAttitude);
        }
        SynchronisePolicy();
    }
    public static void EnsureAtDate(DateTime date){Ensure();Advance(date);}
    // Religious effects are live modifiers. Attitude changes happen only at the policy transition.
    public static void SynchronisePolicy()
    {
        if(I("Ready")==0)return;
        int old=I("Policy"),now=Policy;if(old==now)return;
        if(old!=0&&now==0) {
            Put(K("Attitude",3),Math.Max(0,ReadAttitude(3)-1));Put("RepudiationUntil",Day+30);
        } else Put(K("Attitude",3),now==1?3:now==2?2:1);
        Put("Policy",now);
    }

    public static float Goodwill(int i){Ensure();return Valid(i)?I(K("Goodwill",i))/100f:0;}
    public static AttitudeLevel Attitude(int i){Ensure();return (AttitudeLevel)(Valid(i)?ReadAttitude(i):0);}
    public static float DailyGrowth(int i)=>new[]{.10f,.25f,.50f,1f,1.5f}[(int)Attitude(i)];
    public static float ChangeGoodwill(int i,float points)
    {
        Ensure();if(!Valid(i))return 0;int before=I(K("Goodwill",i));
        Put(K("Goodwill",i),Mathf.Clamp(before+Mathf.RoundToInt(points*100),0,9500));return (I(K("Goodwill",i))-before)/100f;
    }
    public static void SetAttitude(int i,AttitudeLevel attitude){Ensure();if(Valid(i))Put(K("Attitude",i),Mathf.Clamp((int)attitude,0,4));}
    public static bool ApplyEvent(string countryId,string eventKey,float goodwill,int attitudeDelta=0)
    {
        int i=Index(countryId);Ensure();if(i<0||string.IsNullOrEmpty(eventKey))return false;
        string key="Event_"+i+"_"+eventKey;if(I(key)>0)return false;
        Put(key,1);ChangeGoodwill(i,goodwill);
        int before=ReadAttitude(i);Put(K("Attitude",i),attitudeDelta>0?Math.Max(before,Math.Min(3,before+attitudeDelta)):Math.Max(0,before+attitudeDelta));return true;
    }
    // Named temporary effects are replaced in place; reopening an event cannot stack its modifier.
    public static bool SetTemporaryModifier(string countryId,string eventKey,float points,int calendarDays)
    {
        Ensure();int i=Index(countryId);if(i<0||string.IsNullOrEmpty(eventKey)||calendarDays<1)return false;
        string token="Modifier_"+i+"_"+eventKey;
        int slot=I(token,-1);
        // Zero is the dictionary reset sentinel, so saved slots are one-based.
        if(slot<1){slot=I(K("ModifierCount",i))+1;Put(K("ModifierCount",i),slot);Put(token,slot);}
        Put("ModifierValue_"+i+"_"+slot,Mathf.RoundToInt(points*100));Put("ModifierUntil_"+i+"_"+slot,Day+calendarDays);return true;
    }
    public static float PolicyModifier(int i){Ensure();var d=Definitions[i];return Policy==1?d.UnionFull:Policy==2?d.UnionSymbolic:d.UnionRejected;}
    public static float EventModifier(int i)
    {
        Ensure();float sum=Definitions[i].Catholic&&I("RepudiationUntil")>Day?-10:0;
        for(int s=1;s<=I(K("ModifierCount",i));s++)if(I("ModifierUntil_"+i+"_"+s)>Day)sum+=I("ModifierValue_"+i+"_"+s)/100f;
        return sum;
    }
    public static float FocusModifier=>DailyLifeEvents.I("iFocus")==4?10:0;
    public static float AdviserModifier=>CourtRoster.Bonus(4,10,20);
    public static float FinalChance(int i)=>!Valid(i)?0:Mathf.Round(Mathf.Clamp(Goodwill(i)+PolicyModifier(i)+FocusModifier+AdviserModifier+EventModifier(i),0,95)*100)/100f;
    static string Signed(float value)=>value.ToString("+0.##;-0.##;0");
    public static string ChanceBreakdown(int i)=>"Goodwill "+Goodwill(i).ToString("0.##")+" · Church policy "+Signed(PolicyModifier(i))+" · Focus "+Signed(FocusModifier)+" · Adviser "+Signed(AdviserModifier)+" · Events "+Signed(EventModifier(i))+" = "+FinalChance(i).ToString("0.##")+"% (maximum 95%).";

    public static int AvailableEnvoys{get{Ensure();return Math.Max(0,DailyLifeEvents.I("iEnvoys"));}}
    public static int TotalEnvoys{get{Ensure();return AvailableEnvoys+Enumerable.Range(0,Definitions.Length).Count(i=>ReadEnvoy(i)>0);}}
    public static EnvoyAssignment EnvoyState(int i){Ensure();return (EnvoyAssignment)(Valid(i)?ReadEnvoy(i):0);}
    public static int EnvoyDeadline(int i){Ensure();return I(K("EnvoyDue",i));}
    public static DateTime EnvoyDue(int i)=>Date(EnvoyDeadline(i));
    public static string EnvoyStatus(int i)
    {
        switch(EnvoyState(i)) {
            case EnvoyAssignment.Outbound:return "Travelling · arrives "+EnvoyDue(i).ToString("d MMM yyyy");
            case EnvoyAssignment.Resident:return "Resident · +"+DailyGrowth(i).ToString("0.##")+" goodwill per day";
            case EnvoyAssignment.Returning:return "Returning · home "+EnvoyDue(i).ToString("d MMM yyyy");
            default:return "No envoy assigned";
        }
    }
    public static string DispatchReason(int i){Ensure();return !Valid(i)?"Unknown court":ReadEnvoy(i)>0?"An envoy is already assigned to this court":AvailableEnvoys<1?"No available envoy":null;}
    public static bool Dispatch(int i,out string report)
    {
        report=DispatchReason(i);if(report!=null)return false;
        VariableSingleton.ChangeInt("iEnvoys",-1);Put(K("Envoy",i),1);Put(K("EnvoyDue",i),Day+Definitions[i].EnvoyDays);
        report="An envoy departs for "+Definitions[i].Name+". Arrival: "+EnvoyDue(i).ToString("d MMM yyyy")+". Diplomatic work begins after arrival.";return true;
    }
    public static string RecallReason(int i){Ensure();return !Valid(i)?"Unknown court":ReadEnvoy(i)==2||ReadEnvoy(i)==1?null:"No outward or resident envoy to recall";}
    public static bool Recall(int i,out string report)
    {
        report=RecallReason(i);if(report!=null)return false;
        Put(K("Envoy",i),3);Put(K("EnvoyDue",i),Day+Definitions[i].EnvoyDays);
        report="Your envoy leaves "+Definitions[i].Name+" and returns on "+EnvoyDue(i).ToString("d MMM yyyy")+". Existing aid and agreements remain; unfinished appeals pause until an envoy is resident again.";return true;
    }
    public static string GiftReason(int i){Ensure();return !Valid(i)?"Unknown court":ReadEnvoy(i)!=2?"Requires a resident envoy":DailyLifeEvents.I("iMoney")<3000?"Requires 3,000 gold":null;}
    public static bool Gift(int i,out string report)
    {
        report=GiftReason(i);if(report!=null)return false;
        VariableSingleton.ChangeInt("iMoney",-3000);float gain=ChangeGoodwill(i,10);int previous=ReadAttitude(i);
        if(previous<3&&UnityEngine.Random.Range(0,100)<50)Put(K("Attitude",i),previous+1);
        report="Gifts to "+Definitions[i].Name+": 3,000 gold. Goodwill +"+gain.ToString("0.##")+"; now "+Goodwill(i).ToString("0.##")+". "+(ReadAttitude(i)>previous?"Attitude improves to "+Attitude(i)+".":"Attitude remains "+Attitude(i)+".");return true;
    }
    static string RenewalField(bool war,string field)=>(war?"War":"Aid")+field;
    public static int RenewalDays(int i,bool war){Ensure();return I(K(RenewalField(war,"RenewDays"),i));}
    public static bool RenewalReady(int i,bool war){Ensure();return I(K(RenewalField(war,"RenewReady"),i))>0;}
    public static string RenewReason(int i,bool war)
    {
        Ensure();if(!Valid(i))return "Unknown court";
        if(Definitions[i].Id=="ionian_tocco")return "The Ionian Isles negotiate shelter and exile, not relief appeals";
        if(ReadEnvoy(i)!=2)return "Requires a resident envoy";
        if(RenewalDays(i,war)>0)return "This appeal is already being renewed";
        if(RenewalDays(i,!war)>0)return "Your envoy must finish the other renewal project first";
        if(RenewalReady(i,war))return "A renewed request is already available";
        if(war) {if(!Definitions[i].WarEligible||ReadWar(i)!=1)return "A war appeal can be renewed after a refusal";}
        else if(DailyLifeEvents.I("iAidState"+i)<2)return DailyLifeEvents.I("iAidState"+i)==1?"Wait until the committed aid arrives or is lost":"The initial aid request is still available";
        return DailyLifeEvents.I("iMoney")<1000?"Requires 1,000 gold":null;
    }
    public static bool Renew(int i,bool war,out string report)
    {
        report=RenewReason(i,war);if(report!=null)return false;
        VariableSingleton.ChangeInt("iMoney",-1000);Put(K(RenewalField(war,"RenewDays"),i),10);
        report="Your envoy renews the "+(war?"war negotiation":"aid appeal")+" to "+Definitions[i].Name+". Cost: 1,000 gold; ten resident days of work unlock one further request. Time abroad is counted only while the envoy is resident.";return true;
    }
    public static string AidRequestReason(int i)
    {
        Ensure();if(!Valid(i))return "Unknown court";
        if(Definitions[i].Id=="hospitallers"&&MercenaryContracts.RhodianReserved)return "The Rhodian detachment is committed through the mercenary brokers";
        if(Definitions[i].Id=="ionian_tocco")return "The Ionian Isles negotiate shelter and exile, not ordinary aid";
        int state=DailyLifeEvents.I("iAidState"+i);
        if(state==1)return "Aid is already travelling";
        if(state!=0&&!RenewalReady(i,false))return "This appeal has been answered. A resident envoy can renew it.";
        if(DailyLifeEvents.I("iMoney")<1000)return "Requires 1,000 gold";
        if(Definitions[i].AidBySea&&DailyLifeEvents.I("iShips")<1)return "A sea appeal requires one available ship; it is not consumed";
        return null;
    }
    public static void ConsumeAidPermission(int i){Put(K("AidRenewReady",i),0);}
    public static WarCommitment WarState(int i){Ensure();return (WarCommitment)(Valid(i)?ReadWar(i):0);}
    public static int MobilisationDeadline(int i){Ensure();return I(K("WarDue",i));}
    public static DateTime MobilisationDue(int i)=>Date(MobilisationDeadline(i));
    public static bool IsIntervening(int i)=>WarState(i)==WarCommitment.Intervening&&ProvincialWar.I("Peace"+i)==0;
    public static IEnumerable<int> ActiveInterventions=>Enumerable.Range(0,Definitions.Length).Where(IsIntervening);
    public static string WarStatus(int i)=>ProvincialWar.I("Peace"+i)>0?"Separate peace":ProvincialWar.I("Pause"+i)>ProvincialWar.I("LastDay")?"Campaign paused":WarState(i)==WarCommitment.Mobilising?"Mobilising · ready "+MobilisationDue(i).ToString("d MMM yyyy"):WarState(i)==WarCommitment.Intervening?"Intervening":WarState(i)==WarCommitment.Refused?"War appeal declined":"No war commitment";
    public static string WarReason(int i)
    {
        Ensure();if(!Valid(i))return "Unknown court";
        var d=Definitions[i];if(!d.WarEligible)return d.Id=="morea"?"Morea follows its dynastic cooperation decisions":"This court offers aid rather than a regional war commitment";
        if(ReadWar(i)>=2)return "A war commitment already exists";
        if(ReadWar(i)==1&&!RenewalReady(i,true))return "Renew the refused war appeal first";
        if(ReadEnvoy(i)!=2)return "Requires a resident envoy";
        if(ReadAttitude(i)<3)return "Requires a Friendly or Allied court";
        if(FinalChance(i)<75)return "Requires at least 75% final acceptance chance";
        if(!string.IsNullOrEmpty(d.StoryPrerequisite)&&DailyLifeEvents.I(d.StoryPrerequisite)==0)return "A prior agreement is required";
        return null;
    }
    public static bool NegotiateWar(int i,out string report)
    {
        report=WarReason(i);if(report!=null)return false;
        float chance=FinalChance(i);Put(K("WarRenewReady",i),0);
        bool success=UnityEngine.Random.Range(0f,100f)<chance;Put(K("War",i),success?2:1);
        if(success) {Put(K("Attitude",i),4);Put(K("WarDue",i),Day+Definitions[i].MobilisationDays);}
        report=success?Definitions[i].Name+" agrees to intervene. The court is now Allied; mobilisation completes on "+MobilisationDue(i).ToString("d MMM yyyy")+". "+WarInterventions.Terms(i):Definitions[i].Name+" refuses war. A resident envoy can renew this war appeal after ten further days of paid diplomatic work.";
        report+=DiplomaticReplies.FormatWar(Definitions[i].Id,Definitions[i].Name,success,Policy);return true;
    }

    // Calendar days are processed once. Arrival day grants no completed resident work day.
    public static string Advance(DateTime date)
    {
        Ensure();int target=TrainingPool.Day(date),last=I("LastDay");if(target<=last)return "";
        var report=new StringBuilder();
        for(int day=last+1;day<=target;day++)for(int i=0;i<Definitions.Length;i++) {
            int state=ReadEnvoy(i);
            if(state==2) {
                int gain=new[]{10,25,50,100,150}[ReadAttitude(i)];Put(K("Goodwill",i),Math.Min(9500,I(K("Goodwill",i))+gain));
                foreach(bool war in new[]{false,true}) {
                    string key=K(RenewalField(war,"RenewDays"),i);int left=I(key);
                    if(left>0) {Put(key,left-1);if(left==1){Put(K(RenewalField(war,"RenewReady"),i),1);report.AppendLine(Definitions[i].Name+": renewed "+(war?"war":"aid")+" appeal ready. One further request is available.");}}
                }
            }
            if((state==1||state==3)&&I(K("EnvoyDue",i))<=day) {
                Put(K("Envoy",i),state==1?2:0);Put(K("EnvoyDue",i),0);
                if(state==3)VariableSingleton.ChangeInt("iEnvoys",1);
                report.AppendLine(Date(day).ToString("d MMM")+": "+(state==1?"your envoy reaches "+Definitions[i].Name+" and takes up residence.":"your envoy returns from "+Definitions[i].Name+" and is available for reassignment."));
            }
            if(ReadWar(i)==2&&I(K("WarDue",i))<=day) {
                Put(K("War",i),3);report.AppendLine(Date(day).ToString("d MMM")+": "+Definitions[i].Name+" finishes mobilisation and begins "+Definitions[i].InterventionType.ToLowerInvariant()+". Ottoman deliveries will report any actual disruption.");
            }
        }
        Put("LastDay",target);return report.Length==0?"":report.ToString()+"\n";
    }
}
